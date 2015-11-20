using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ADFSCertificateUpdater
{
    [Guid("8FBF7C1D-682F-4C67-91F7-2776ACAB44D0")]
    class ADFSCertificateUpdaterConfiguration : SPPersistedObject
    {
        #region Fields
        
        static readonly string ConfigDBObjectName = "ADFSCertificateUpdaterConfiguration";
        static readonly Guid ConfigDBObjectID = new Guid("8FBF7C1D-682F-4C67-91F7-2776ACAB44D0");

        [Persisted]
        private List<ChosenTrustedProvider> m_ChosenTrustedProviders;

        [Persisted]
        private bool m_DryRun = false;

        #endregion
        
        #region Properties

        /// <summary>
        /// Trusted providers that have been selected for update.
        /// </summary>
        public List<ChosenTrustedProvider> SelectedProviders
        {
            get
            {
                if (null == m_ChosenTrustedProviders)
                    m_ChosenTrustedProviders = new List<ChosenTrustedProvider>();
                return m_ChosenTrustedProviders;
            }
            set
            {
                m_ChosenTrustedProviders = value;
            }
        }

        /// <summary>
        /// Should we save the changes or not?
        /// </summary>

        public bool DryRun
        {
            get
            {
                return m_DryRun;
            }
            set
            {
                m_DryRun = value;
            }
        }

        #endregion

        #region Constructor

        public ADFSCertificateUpdaterConfiguration()
        { }

        public ADFSCertificateUpdaterConfiguration(string name, SPPersistedObject parent)
            : base(name, parent)
        { }

        #endregion

        #region Methods

        /// <summary>
        /// This method creates the Persisted Object to store the configuration of the timer job.
        /// </summary>
        /// <returns></returns>
        internal static void CreatePersistedObject()
        {
            try
            {
                ADFSCertificateUpdaterConfiguration defaultConfig = new ADFSCertificateUpdaterConfiguration(ConfigDBObjectName, SPFarm.Local);
                defaultConfig.Id = ConfigDBObjectID;
                defaultConfig.SelectedProviders.Add(new ChosenTrustedProvider {
                    ProviderName = "ADFS UPN"
                });
                defaultConfig.Update();
                Logger.Verbose(Logger.CategoryConfiguration, "Created the configuration object");
            }
            catch (Exception exc)
            {
                Logger.Verbose(Logger.CategoryConfiguration, "Exception creating configuration object: " + exc.Message);
            }
        }

        /// <summary>
        /// Get the current object from the Configuration DB. Create one with the default settings if it cannot be found.
        /// </summary>
        /// <returns></returns>
        public static ADFSCertificateUpdaterConfiguration GetFromConfigDB()
        {
            var parent = SPFarm.Local;
            var persistedObject = (ADFSCertificateUpdaterConfiguration)parent.GetObject(ConfigDBObjectName, SPFarm.Local.Id, typeof(ADFSCertificateUpdaterConfiguration));
            if (null == persistedObject)
            {
                CreatePersistedObject();
                persistedObject = GetFromConfigDB();
            }
            return persistedObject;
        }

        #endregion
    }

    public class ChosenTrustedProvider : SPAutoSerializingObject
    {
        [Persisted]
        public String ProviderName = String.Empty;

        [Persisted]
        public List<String> LastLog = new List<string>();
		
		[Persisted]
		public Boolean Success = false;
    }
}
