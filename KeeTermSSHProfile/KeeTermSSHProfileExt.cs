using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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

        private void OnWindowAdded(object s, GwmWindowEventArgs e)
        {
            var pwEntryForm = e.Form as PwEntryForm;            
            if (pwEntryForm != null)
            {
                var entryOptions = new UI.EntryOptions(this);
                pwEntryForm.Shown += (s2, e2) =>
                {
                    var pageOverride = new UI.EntryPageOverride(pwEntryForm);

                    var foundControls = pwEntryForm.Controls.Find("m_btnOK", true);
                    var okButton = foundControls[0] as Button;
                    okButton.GotFocus += (s3, e3) =>
                    {
                        Debug.Print("okButton.GotFocus");
                        pageOverride.Save();                        
                    };

                    // UpdateKeeEntryPanel(pwEntryForm);
                    /*
                    var btn = new System.Windows.Forms.Button();
                    btn.Location = new System.Drawing.Point(383, 67);
                    btn.Name = "btnRemove";
                    btn.Size = new System.Drawing.Size(75, 23);
                    btn.TabIndex = 3;
                    btn.Text = "Remove";
                    btn.UseVisualStyleBackColor = true;
                    // pwEntryForm.Controls.Add(btn);

                    var foundControlsX = pwEntryForm.Controls.Find("m_tabMain", true);
                    if (foundControlsX.Length != 1)
                    {
                        return;
                    }

                    var tabControl = foundControlsX[0] as TabControl;
                    if (tabControl == null)
                    {
                        return;
                    }

                    tabControl.Controls[0].Controls.Add(btn);

                    // AddTab(pwEntryForm, entryOptions);
                    */
                };                
            }

            var optionsForm = e.Form as OptionsForm;
            if (optionsForm != null)
            {
                optionsForm.Shown += (s2, e2) =>
                {
                    AddTab(optionsForm, new UI.GlobalOptions(this));                    
                };                
            }
        }

        private void OnFileSaved(object sender, FileSavedEventArgs e) {
            Generator.UpdateAll(e.Database);            
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

        private void UpdateKeeEntryPanel(Form form)
        {
            var entryTab = FindControl<TabPage>(form, "m_tabEntry");
            if (entryTab == null)
            {
                return;
            }
            Debug.Print("entryTab found");

            var notes = FindControl<KeePass.UI.CustomRichTextBoxEx>(entryTab, "m_rtNotes");
            if (notes == null)
            {
                return;
            }

            Debug.Print("Notes found");
            notes.Size = new System.Drawing.Size(374, 80);

            /*
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

            var basicTab = tabControl.Controls[0];

            var richEdit = basicTab.Controls.Find("m_rtNotes", true);
            */

        }

        private T FindControl<T>(Control ctrl, string name) where T: Control
        {
            var found = ctrl.Controls.Find(name, true);
            Debug.Print("Find: " + name + " len: " + found.Length.ToString());
            if (found.Length != 1)
            {
                return null;
            }

            return found[0] as T;
        }
    }
}
