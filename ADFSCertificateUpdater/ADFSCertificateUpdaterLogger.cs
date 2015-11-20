using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace ADFSCertificateUpdater
{
    class Logger : SPDiagnosticsServiceBase
    {
        #region Fields

        private static string ADFSCertificateUpdaterLoggerAreaName = "ADFSCertificateUpdaterLogger";
        private static string ADFSCertificateUpdaterLoggerServiceName = "ADFS Certificate Updater";

        private static Logger f_Local;

        public enum LoggingCategories
        {
            General,
            Configuration,
            FederationMetadata,
            FarmTrustCertificate,
            TrustedProviderCertificate
        }

        public static string CategoryGeneral = Enum.GetName(typeof(LoggingCategories), LoggingCategories.General);
        public static string CategoryConfiguration = Enum.GetName(typeof(LoggingCategories), LoggingCategories.Configuration);
        public static string CategoryFederationMetadata = Enum.GetName(typeof(LoggingCategories), LoggingCategories.FederationMetadata);
        public static string CategoryRootAuthority = Enum.GetName(typeof(LoggingCategories), LoggingCategories.FarmTrustCertificate);
        public static string CategoryProviderCertificate = Enum.GetName(typeof(LoggingCategories), LoggingCategories.TrustedProviderCertificate);

        #endregion

        #region Properties

        public static Logger Local
        {
            get
            {
                if (null == f_Local)
                {
                    f_Local = SPFarm.Local.Services.GetValue<Logger>(ADFSCertificateUpdaterLoggerServiceName);
                }
                return f_Local;
            }
        }

        #endregion

        #region Constructors

        public Logger()
            : base(ADFSCertificateUpdaterLoggerServiceName, SPFarm.Local) { }

        public Logger(string serviceName, SPFarm local)
            : base(serviceName, local) { }

        #endregion

        #region Methods

        /// <summary>
        /// Provide areas
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            // First build categories for the area
            List<SPDiagnosticsCategory> diagnosticCategories = new List<SPDiagnosticsCategory>();
            foreach (string categoryName in Enum.GetNames(typeof(LoggingCategories)))
            {
                diagnosticCategories.Add(new SPDiagnosticsCategory(categoryName, null, TraceSeverity.Medium, EventSeverity.Information, 0, 0, false, true));
            }

            // Then build the area
            List<SPDiagnosticsArea> diagnosticAreas = new List<SPDiagnosticsArea>
            {
                new SPDiagnosticsArea(ADFSCertificateUpdaterLoggerAreaName, diagnosticCategories)
            };

            return diagnosticAreas;
        }

        public static void Unexpected(string category, string message)
        {
            WriteTrace(category, message, TraceSeverity.Unexpected);
        }

        public static void Verbose(string category, string message)
        {
            WriteTrace(category, message, TraceSeverity.Verbose);
        }

        public static void Medium(string category, string message)
        {
            WriteTrace(category, message, TraceSeverity.Medium);
        }

        public static void WriteTrace(string categoryName, string message, TraceSeverity severity)
        {
            try
            {
                var category = Logger.Local.Areas[ADFSCertificateUpdaterLoggerAreaName].Categories[categoryName];
                Logger.Local.WriteTrace(1, category, severity, message);
            }
            catch (Exception exc)
            {
                WriteTraceStandalone(TraceSeverity.Unexpected, "There was a problem writing to the trace log with our own logger: " + exc.Message);
                WriteTraceStandalone(severity, "Original message: " + message);
            }
        }

        public static void WriteTraceStandalone(TraceSeverity severity, string message)
        {
            var cat = new SPDiagnosticsCategory("Standalone logging", TraceSeverity.Verbose, EventSeverity.None);
            var cats = new List<SPDiagnosticsCategory>();
            cats.Add(cat);
            var area = new SPDiagnosticsArea("ADFSCertificateUpdaterLogger", cats);

            SPDiagnosticsService.Local.WriteTrace(2, cat, severity, message);
        }

        #endregion
    }
}
