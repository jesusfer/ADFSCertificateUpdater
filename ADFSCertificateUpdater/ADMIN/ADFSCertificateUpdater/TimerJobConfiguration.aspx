<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TimerJobConfiguration.aspx.cs" Inherits="ADFSCertificateUpdater.Admin.ADFSCertificateUpdater.TimerJobConfiguration" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <style>
        /* Set the size of the right part with all input controls */
        .ms-inputformcontrols {
            width: 650px;
        }

        /* Set the display of the title of each section */
        .ms-standardheader {
            color: #0072c6;
            font-weight: bold;
            font-size: 1.15em;
        }

        /* Only used in td elements in grid view that displays LDAP connections */
        .ms-vb2 {
            vertical-align: middle;
        }

        .ldapcp-success {
            color: green;
            font-weight: bold;
        }

        .ldapcp-HideCol {
            display: none;
        }

        #divNewLdapConnection label {
            display: inline-block;
            line-height: 1.8;
            width: 100px;
        }

        #divNewLdapConnection fieldset {
            border: 0;
            margin: 0;
            padding: 0;
        }

            #divNewLdapConnection fieldset ol {
                margin: 0;
                padding: 0;
            }

            #divNewLdapConnection fieldset li {
                list-style: none;
                padding: 5px;
                margin: 0;
            }

        #divNewLdapConnection em {
            font-weight: bold;
            font-style: normal;
            color: #f00;
        }
    </style>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    ADFS Certificate Updater configuration
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
    ADFS Certificate Updater
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table border="0" cellspacing="0" cellpadding="0" width="100%">
        <tr>
			<td></td>
            <td style="text-align:right">
                <input type="button" value="Refresh the page" onclick="javascript:window.location = window.location"/>
            </td>
        </tr>
        <wssuc:inputformsection title="Choose trusted providers" runat="server">
            <template_description>
				<wssawc:EncodedLiteral runat="server" text="Choose what trusted providers will the timer job update" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
			</template_description>
            <template_inputformcontrols>
				<tr><td>
				<wssawc:SPGridView runat="server" ID="GridProviders" AutoGenerateColumns="false" OnRowCommand="GridProviders_RowCommand">
					<Columns>
						<asp:BoundField HeaderText="Name" DataField="Name"/>
						<asp:CheckBoxField HeaderText="Is selected?" DataField="IsSelected"/>
						<asp:BoundField HeaderText="Last log" DataField="LastLog" HtmlEncode="false"/>
						<asp:ButtonField HeaderText="Enable/disable" ButtonType="Button" CommandName="Switch" Text="Switch"/>
					</Columns>
				</wssawc:SPGridView>
				</td></tr>
			</template_inputformcontrols>
        </wssuc:inputformsection>
    </table>
</asp:Content>


