// Copyright (c) 2012, Outercurve Foundation.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must  retain  the  above copyright notice, this
//   list of conditions and the following disclaimer.
//
// - Redistributions in binary form  must  reproduce the  above  copyright  notice,
//   this list of conditions  and  the  following  disclaimer in  the documentation
//   and/or other materials provided with the distribution.
//
// - Neither  the  name  of  the  Outercurve Foundation  nor   the   names  of  its
//   contributors may be used to endorse or  promote  products  derived  from  this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,  BUT  NOT  LIMITED TO, THE IMPLIED
// WARRANTIES  OF  MERCHANTABILITY   AND  FITNESS  FOR  A  PARTICULAR  PURPOSE  ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL,  SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT  OF  SUBSTITUTE  GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)  HOWEVER  CAUSED AND ON
// ANY  THEORY  OF  LIABILITY,  WHETHER  IN  CONTRACT,  STRICT  LIABILITY,  OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE)  ARISING  IN  ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using WebsitePanel.Providers.HostedSolution;
using WebsitePanel.EnterpriseServer;
using WebsitePanel.Providers.OS;

namespace WebsitePanel.Portal.ExchangeServer
{
    public partial class EnterpriseStorageFolderGeneralSettings : WebsitePanelModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!ES.Services.EnterpriseStorage.CheckUsersDomainExists(PanelRequest.ItemID))
                {
                    Response.Redirect(EditUrl("SpaceID", PanelSecurity.PackageId.ToString(), "enterprisestorage_folders",
                        "ItemID=" + PanelRequest.ItemID));
                }

                BindSettings();
            }
        }

        private void BindSettings()
        {
            try
            {
                // get settings

                Organization org = ES.Services.Organizations.GetOrganization(PanelRequest.ItemID);

                SystemFile folder = ES.Services.EnterpriseStorage.GetEnterpriseFolder(
                    PanelRequest.ItemID, PanelRequest.FolderID);

                litFolderName.Text = string.Format("{0}", folder.Name);

                // bind form
                txtFolderName.Text = folder.Name;
                lblFolderUrl.Text = folder.Url;

                var esPermissions = ES.Services.EnterpriseStorage.GetEnterpriseFolderPermissions(PanelRequest.ItemID,folder.Name);

                chkDirectoryBrowsing.Checked = ES.Services.EnterpriseStorage.GetDirectoryBrowseEnabled(PanelRequest.ItemID, folder.Url);

                permissions.SetPermissions(esPermissions);

            }
            catch (Exception ex)
            {
                messageBox.ShowErrorMessage("ENETERPRISE_STORAGE_GET_FOLDER_SETTINGS", ex);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                litFolderName.Text = txtFolderName.Text;

                SystemFile folder = new SystemFile { Name = PanelRequest.FolderID, Url = lblFolderUrl.Text };

                if (!ES.Services.EnterpriseStorage.CheckEnterpriseStorageInitialization(PanelSecurity.PackageId, PanelRequest.ItemID))
                {
                    ES.Services.EnterpriseStorage.CreateEnterpriseStorage(PanelSecurity.PackageId, PanelRequest.ItemID);
                }

                //File is renaming
                if (PanelRequest.FolderID != txtFolderName.Text)
                {
                    //check if filename is correct
                    foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
                    {
                        if (txtFolderName.Text.Contains(invalidChar.ToString()))
                        {
                            messageBox.ShowErrorMessage("FILES_RENAME_FILE");

                            return;
                        }
                    }

                    folder = ES.Services.EnterpriseStorage.RenameEnterpriseFolder(PanelRequest.ItemID, PanelRequest.FolderID, txtFolderName.Text);
                }

                ES.Services.EnterpriseStorage.SetEnterpriseFolderSettings(PanelRequest.ItemID, folder, permissions.GetPemissions(), chkDirectoryBrowsing.Checked, 100);


                Response.Redirect(EditUrl("SpaceID", PanelSecurity.PackageId.ToString(), "enterprisestorage_folders",
                        "ItemID=" + PanelRequest.ItemID));
            }
            catch (Exception ex)
            {
                messageBox.ShowErrorMessage("ENTERPRISE_STORAGE_UPDATE_FOLDER_SETTINGS", ex);
            }
        }
    }
}