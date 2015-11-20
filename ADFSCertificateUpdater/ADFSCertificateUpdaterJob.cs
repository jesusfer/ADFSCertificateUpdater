using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ADFSCertificateUpdater
{
    public class ADFSCertificateUpdaterJob : SPJobDefinition
    {
        #region Fields

        internal static String JobName = "ADFSCertificateUpdaterJob";
        internal static String JobDisplayName = "ADFS Certificate Updater";
        internal static String JobDescription = "This timer job will download the metadata from the ADFS service and check if the primary signing certificate has changed";

        #endregion

        #region Properties

        public override string Description
        {
            get
            {
                return JobDescription;
            }
        }

        public override string DisplayName
        {
            get
            {
                return JobDisplayName;
            }
        }

        public override string TypeName
        {
            get
            {
                return JobName;
            }
        }

        #endregion

        #region Constructor

        //public ADFSCertificateUpdaterJob() : base() { }

        public ADFSCertificateUpdaterJob()
            : base(JobName, SPFarm.Local.TimerService, null, SPJobLockType.Job)
        {
            this.Title = JobDisplayName;
        }

        #endregion

        #region Methods

        public override void Execute(Guid targetInstanceId)
        {
            var config = ADFSCertificateUpdaterConfiguration.GetFromConfigDB();
            var providerNames = config.SelectedProviders.Select(x => x.ProviderName);
            var providerList = SPSecurityTokenServiceManager.Local.TrustedLoginProviders.Where(x => providerNames.Contains(x.Name));
            Logger.Verbose(
                Logger.CategoryGeneral,
                String.Format("Found {0} of {1} trusted providers", providerList.Count(), config.SelectedProviders.Count()));
            foreach (ChosenTrustedProvider ctp in config.SelectedProviders)
            {
                ProcessTrustedProvider(ctp, providerList.Where(x => x.Name == ctp.ProviderName).First());
                config.Update();
            }
        }

        private void ProcessTrustedProvider(ChosenTrustedProvider ctp, SPTrustedLoginProvider provider)
        {
            Logger.Medium(Logger.CategoryGeneral, "Begin processing trusted provider " + provider.Name);
            ctp.LastLog = new List<string>();
            ctp.Success = false;

            // Get current ADFS certificate
            var adfsCertificate = GetCurrentADFSCertificate(provider);
            if (adfsCertificate == null)
            {
                Logger.Unexpected(Logger.CategoryFederationMetadata, "Didn't get any token signing certificate. Skipping this provider");
                ctp.LastLog.Add("Didn't get any token signing certificate from federation metadata.");
                return;
            }
            // Check if the certificate has changed
            if (adfsCertificate.Thumbprint.Equals(provider.SigningCertificate.Thumbprint))
            {
                Logger.Medium(Logger.CategoryProviderCertificate, "The signing certificate has not changed. Skipping this provider");
                ctp.LastLog.Add("The signing certificate has not changed.");
                return;
            }
            Logger.Medium(Logger.CategoryGeneral, "The primary certificate has changed");
            ctp.LastLog.Add("New primary certificate found: " + adfsCertificate.Subject);
            // Add the certificate of the trusted store
            bool added = AddCertificateToTrust(adfsCertificate);
            // Change the certficate in the provider
            if (added)
            {
                ctp.LastLog.Add("Added the certificate as a trusted authority (or it was already).");
                bool updated = UpdateProviderCertificate(provider, adfsCertificate);
                if (updated)
                {
                    ctp.LastLog.Add("Updated the signing certificate in the provider.");
                }
            }
            ctp.Success = true;
            Logger.Medium(Logger.CategoryGeneral, "Finished processing trusted provider " + provider.Name);
        }

        /// <summary>
        /// Downloads the metadata from the federation service and returns the current primary token signing certificate.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private X509Certificate2 GetCurrentADFSCertificate(SPTrustedLoginProvider provider)
        {
            // The metadata is a XML document we get from the federation metadata endpoint
            var metadataUrl = String.Format("https://{0}/federationmetadata/2007-06/federationmetadata.xml", provider.ProviderUri.Host);
            var request = WebRequest.Create(metadataUrl);
            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }
            catch (Exception exc)
            {
                Logger.Unexpected(Logger.CategoryFederationMetadata, "Exception downloading metadata: " + exc.Message);
                return null;
            }

            // Once we have downloaded the XML find the primary token signing cert
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);
            var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            nsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsManager.AddNamespace("fed", "http://docs.oasis-open.org/wsfed/federation/200706");
            nsManager.AddNamespace("x", "urn:oasis:names:tc:SAML:2.0:metadata");
            nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            var certs = xmlDocument.SelectNodes("/x:EntityDescriptor/x:RoleDescriptor[@xsi:type = 'fed:SecurityTokenServiceType']/x:KeyDescriptor[@use = 'signing']", nsManager);
            Logger.Verbose(Logger.CategoryFederationMetadata, String.Format("Found {0} signing certs", certs.Count));

            var primaryBase64 = xmlDocument.SelectSingleNode("/x:EntityDescriptor/ds:Signature/ds:KeyInfo/ds:X509Data/ds:X509Certificate", nsManager).InnerText;
            var primaryCert = new X509Certificate2(Encoding.UTF8.GetBytes(primaryBase64));

            Logger.Medium(Logger.CategoryFederationMetadata, String.Format("Primary signing certificate: {0}", primaryCert.Subject));
            return primaryCert;
        }

        /// <summary>
        /// Adds the new certificate as a new trusted root authority in SharePoint.
        /// This method is unsupported by Microsoft Support.
        /// </summary>
        /// <param name="newCertificate"></param>
        /// <returns></returns>
        private bool AddCertificateToTrust(X509Certificate2 newCertificate)
        {
            // Check if the certificate is already in the store. If not, add it.
            // This bit is not supported by MS Support.
            try
            {
                var managerType = Type.GetType("Microsoft.SharePoint.Administration.SPTrustedRootAuthorityManager, " + typeof(SPFarm).Assembly.ToString());
                var localProperty = managerType.GetProperty("LocalOrThrow", BindingFlags.Static | BindingFlags.NonPublic);
                var localManager = localProperty.GetValue(null);

                var authorityType = Type.GetType("Microsoft.SharePoint.Administration.SPTrustedRootAuthority, " + typeof(SPFarm).Assembly.ToString());
                var authorityCtor = authorityType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();
                var newAuthority = (SPPersistedObject)authorityCtor.Invoke(new object[] { newCertificate.Subject, localManager, newCertificate });
                newAuthority.Update();
                Logger.Medium(Logger.CategoryRootAuthority, "Created new root authority");
            }
            catch (SPDuplicateObjectException dup)
            {
                // There is already a root authority with the same name as this certificate's subject
                Logger.Verbose(Logger.CategoryRootAuthority, "Duplicated certificate: " + dup.Message);
                return true;
            }
            catch (Exception exc)
            {
                Logger.Unexpected(Logger.CategoryRootAuthority, "Error adding a new root authority: " + exc.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Change the token signing certificate in the trusted identity provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="adfsCertificate"></param>
        /// <returns></returns>
        private bool UpdateProviderCertificate(SPTrustedLoginProvider provider, X509Certificate2 adfsCertificate)
        {
            try
            {
                provider.SigningCertificate = adfsCertificate;
                provider.Update();
                Logger.Medium(
                    Logger.CategoryProviderCertificate,
                    String.Format("Configured the token signing certificate to {0}", adfsCertificate.Subject));
                return true;
            }
            catch (Exception exc)
            {
                Logger.Unexpected(Logger.CategoryProviderCertificate, "Error changing the signing certificate: " + exc.Message);
            }
            return false;
        }

        #endregion
    }
}
