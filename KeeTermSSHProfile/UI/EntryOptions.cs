using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Util;
using KeePassLib;

namespace KeeTermSSHProfile.UI
{
    public partial class EntryOptions : UserControl
    {
        private KeeTermSSHProfileExt Ext;
        public EntrySettings InitialSettings { get; private set; }
        public EntrySettings CurrentSettings { get; private set; }

        private ListViewItem selectedEntry;

        public EntryOptions(KeeTermSSHProfileExt ext)
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            Ext = ext;        
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var pwEntryForm = ParentForm as PwEntryForm;
            if (pwEntryForm != null)
            {
                InitialSettings = EntrySettings.Load(pwEntryForm.EntryRef);

                if (InitialSettings == null)
                {
                    CurrentSettings = new EntrySettings();
                }
                else
                {
                    CurrentSettings = InitialSettings.Clone();
                }
                
                UpdateControls(CurrentSettings);
            }

            btnAdd.Enabled = true;
            btnEdit.Enabled = false;
            btnRemove.Enabled = false;
            btnLaunch.Enabled = false;

            sshEntries.Items.Add(new ListViewItem(new string[] { "Trigon Worker", "zozzz@192.168.0.1:4565" }));
            sshEntries.Items.Add(new ListViewItem(new string[] { "Trigon Manager", "zozzz@192.168.0.2:4565" }));
        }

        private void UpdateControls(EntrySettings settings)
        {

            // testValueInp.Text = settings.test;
        }

        private void testValueInp_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void sshEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lv = sender as ListView;
            if (lv != null)
            {
                if (lv.SelectedItems.Count > 0)
                {
                    selectedEntry = lv.SelectedItems[0];
                    btnEdit.Enabled = true;
                    btnRemove.Enabled = true;
                    btnLaunch.Enabled = true;
                    return;
                }
            }

            btnEdit.Enabled = false;
            btnRemove.Enabled = false;
        }
    }
}
