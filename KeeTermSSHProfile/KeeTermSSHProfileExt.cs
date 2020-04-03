using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeeTermSSHProfile
{
    public sealed class KeeTermSSHProfileExt: Plugin
    {
        private IPluginHost host = null;
        private GlobalSettings options;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            this.host = host;
            this.options = GlobalSettings.Load(host);
               

            host.MainWindow.FileSaved += OnFileSaved;
            GlobalWindowManager.WindowAdded += OnWindowAdded;

            return true;
        }

        public override void Terminate() {
            host.MainWindow.FileSaved -= OnFileSaved;
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            var pwEntryForm = e.Form as PwEntryForm;
            if (pwEntryForm != null)
            {
                var entryOptions = new UI.EntryOptions(this);
                pwEntryForm.Shown += (s, e2) =>
                {
                    AddTab(pwEntryForm, entryOptions);
                };
                var foundControls = pwEntryForm.Controls.Find("m_btnOK", true);
                var okButton = foundControls[0] as Button;
                okButton.GotFocus += (s, e2) =>
                {
                    entryOptions.CurrentSettings.Save(pwEntryForm.EntryBinaries);
                };
            }

            var optionsForm = e.Form as OptionsForm;
            if (optionsForm != null)
            {
                optionsForm.Shown += (s, e2) =>
                {
                    AddTab(optionsForm, new UI.GlobalOptions(this));                    
                };                
            }
        }

        private void OnFileSaved(object sender, FileSavedEventArgs e) {
            MessageService.ShowInfo("KeeTermSSHProfileExt has been notified that the user tried to save to the following file:",
                e.Database.IOConnectionInfo.Path, "Result: " +
                (e.Success ? "success." : "failed."));
        }

        // Utilities
        public static void AddTab(Form form, UserControl aPanel)
        {            
            var foundControls = form.Controls.Find("m_tabMain", true);
            if (foundControls.Length != 1)
            {
                return;
            }

            var tabControl = foundControls[0] as TabControl;
            if (tabControl == null)
            {
                return;
            }

            /*
            if (tabControl.ImageList == null)
            {
                tabControl.ImageList = new ImageList();
                tabControl.ImageList.ImageSize = new Size(DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
            }
            var imageIndex = tabControl.ImageList.Images.Add(Resources.KeeAgentIcon_png, Color.Transparent);
            */
            var newTab = new TabPage("Még nem tudom");
            //newTab.ImageIndex = imageIndex;
            //newTab.ImageKey = cTabImageKey;
            newTab.UseVisualStyleBackColor = true;
            newTab.Controls.Add(aPanel);
            aPanel.Dock = DockStyle.Fill;
            tabControl.Controls.Add(newTab);            
        }
    }
}
