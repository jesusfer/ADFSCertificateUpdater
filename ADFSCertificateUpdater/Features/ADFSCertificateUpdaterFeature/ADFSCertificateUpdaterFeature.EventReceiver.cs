using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace ADFSCertificateUpdater.Features.ADFSCertificateUpdaterFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("c982aac9-ccde-4323-ad70-1966bffc956c")]
    public class ADFSCertificateUpdaterFeatureEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            RegisterLogger();

            // Make sure the job isn't there
            foreach (SPJobDefinition job in SPFarm.Local.TimerService.JobDefinitions)
            {
                if (job.Name == ADFSCertificateUpdaterJob.JobName)
                {
                    job.Delete();
                }
            }

            // Add the configuration object. Getting it for the first time forces the creation.
            var config = ADFSCertificateUpdaterConfiguration.GetFromConfigDB();

            // Install the job
            ADFSCertificateUpdaterJob newJob = new ADFSCertificateUpdaterJob();
            // Run daily at 0015
            var schedule = new SPDailySchedule();
            schedule.BeginHour = 0;
            schedule.EndHour = 0;
            schedule.BeginMinute = 15;
            schedule.EndMinute = 15;
            newJob.Schedule = schedule;

            newJob.Update();
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // Delete the job
            foreach (SPJobDefinition job in SPFarm.Local.TimerService.JobDefinitions)
            {
                if (job.Name == ADFSCertificateUpdaterJob.JobName)
                {
                    job.Delete();
                }
            }
            // Remove the configuration object
            var config = ADFSCertificateUpdaterConfiguration.GetFromConfigDB();
            config.Delete();

            UnregisterLogger();
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
        #region Logger methods

        private static void RegisterLogger()
        {
            Logger.WriteTraceStandalone(TraceSeverity.Verbose, "Registering Logger");
            // Add logging

            Logger service = Logger.Local;
            if (service == null)
            {
                service = new Logger();
                service.Update();

                if (service.Status != SPObjectStatus.Online)
                {
                    service.Provision();
                }
            }

            Logger.WriteTraceStandalone(TraceSeverity.Verbose, "Done registering Logger");
        }

        private static void UnregisterLogger()
        {
            Logger.WriteTraceStandalone(TraceSeverity.Verbose, "Unregistering Logger");
            // Add logging

            Logger service = Logger.Local;
            if (service != null)
            {
                service.Unprovision();
                service.Delete();
            }

            Logger.WriteTraceStandalone(TraceSeverity.Verbose, "Done unregistering Logger");
        }

        #endregion
    }
}
