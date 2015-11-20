## ADFSCertificateUpdater
A custom SharePoint 2013 timer job that will update the certificate in the trusted providers from the ADFS federation metadata.

#### What is this about

SharePoint does not include any automatic way to update the certificates in the trusted providers when they expire. This usually leads to problems when the certificate auto rollover happens in ADFS and then breaks the ADFS authentication in SharePoint.

This project aims to solve this problem by creating a custom timer job that will download the ADFS federation metadata and check if the primary token signing certificate has changed. If it has changed, then it will udpate SharePoint with the new certificate.

The project currently includes:
* A custom timer job definition.
* A custom admin page in Central Administration that allows an admin to select which providers to update.
* Custom ULS logging.
