using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.ApplicationPages;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Generic;
using Microsoft.SharePoint.Administration.Claims;

namespace ADFSCertificateUpdater.Admin.ADFSCertificateUpdater
{
    public partial class TimerJobConfiguration : GlobalAdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PopulateGridProviders();
        }

        private void PopulateGridProviders()
        {
            var config = ADFSCertificateUpdaterConfiguration.GetFromConfigDB();
            var fullProviderList = SPSecurityTokenServiceManager.Local.TrustedLoginProviders;
            var pc = new PropertyCollectionBinder();
            foreach (SPTrustedLoginProvider tlp in fullProviderList)
            {
                var ctp = config.SelectedProviders.Find(x => x.ProviderName == tlp.Name);
                var isSelected = ctp != null;
                var logs = isSelected ? ctp.LastLog : new List<String>();
                pc.AddRow(tlp.Name, isSelected, logs);
            }
            pc.BindGrid(GridProviders);
        }

        protected void BtnOK_Click(Object sender, EventArgs e)
        { }

        protected void GridProviders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Switch")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow selectedRow = GridProviders.Rows[index];
                string name = selectedRow.Cells[0].Text;
                var config = ADFSCertificateUpdaterConfiguration.GetFromConfigDB();
                var selectedProvider = config.SelectedProviders.Find(x => x.ProviderName == name);
                if (selectedProvider != null)
                {
                    config.SelectedProviders.Remove(selectedProvider);
                }
                else
                {
                    config.SelectedProviders.Add(new ChosenTrustedProvider() { ProviderName = name });
                }
                config.Update();
                PopulateGridProviders();
            }
        }
    }

    public class PropertyCollectionBinder
    {
        protected DataTable PropertyCollection = new DataTable();
        public PropertyCollectionBinder()
        {
            PropertyCollection.Columns.Add("Name", typeof(string));
            PropertyCollection.Columns.Add("IsSelected", typeof(bool));
            PropertyCollection.Columns.Add("LastLog", typeof(string));
        }

        public void AddRow(string name, bool isSelected, List<string> lastLog)
        {
            DataRow newRow = PropertyCollection.Rows.Add();
            newRow["Name"] = name;
            newRow["IsSelected"] = isSelected;
            newRow["LastLog"] = String.Join("<br/>", lastLog);
        }

        public void BindGrid(SPGridView grid)
        {
            grid.DataSource = PropertyCollection.DefaultView;
            grid.DataBind();
        }
    }
}
