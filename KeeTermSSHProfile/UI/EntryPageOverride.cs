using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

using KeePass.Forms;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Utility;

namespace KeeTermSSHProfile.UI
{
    class EntryPageOverride
    {
        private PwEntryForm form;
        private TabPage entryPage;
        private Label notesLabel;
        private CustomRichTextBoxEx notesCtrl;
        private Point notesLocation;
        private int notesHeight;
        private Point notesLabelLocation;

        private Label sshLabel;
        private FlowLayoutPanel sshButtons;
        private ContextMenuStrip sshMenuStrip;

        private EntrySettings initialSettings;
        private EntrySettings currentSettings;
        private int activeAutoGenButton;

        public EntryPageOverride(PwEntryForm form)
        {
            this.form = form;
            this.entryPage = FindControl<TabPage>(form, "m_tabEntry");
            if (this.entryPage != null)
            {
                this.notesLabel = FindControl<Label>(this.entryPage, "m_lblNotes");
                this.notesCtrl = FindControl<CustomRichTextBoxEx>(this.entryPage, "m_rtNotes");
                if (this.notesLabel != null && this.notesCtrl != null)
                {
                    sshMenuStrip = CreateSSHMenuStrip();

                    notesLocation = notesCtrl.Location;
                    notesHeight = notesCtrl.Height;
                    notesLabelLocation = notesLabel.Location;
                    Update();
                }
            }
        }

        public void Save()
        {
            // TODO: ha üres, akkor törölni az egészet
            currentSettings.Save(form.EntryBinaries);
        }

        private void Update()
        {
            initialSettings = EntrySettings.Load(form.EntryRef);
            if (initialSettings == null)
            {
                initialSettings = new EntrySettings();
            }
            currentSettings = initialSettings.Clone();

            Update(currentSettings);
        }

        private void Update(EntrySettings settings)
        {
            var container = notesCtrl.Parent;
            
            if (sshButtons != null)
            {
                container.Controls.Remove(sshButtons);
                sshButtons.Dispose();
                sshButtons = null;
            }

            if (sshLabel == null)
            {
                sshLabel = new Label();
                sshLabel.Location = notesLabelLocation;
                sshLabel.Width = 70;
                sshLabel.Text = "AutoGen:";
                container.Controls.Add(sshLabel);
            }

            sshButtons = CreateButtons(settings);
            sshButtons.Location = new Point(notesLocation.X - 5, notesLocation.Y - 5);

            var lastBtn = sshButtons.Controls[sshButtons.Controls.Count - 1] as Button;
            var buttonsHeight = lastBtn.Location.Y + lastBtn.Height;
            sshButtons.Height = buttonsHeight;

            notesLabel.Location = new Point(notesLabelLocation.X, notesLabelLocation.Y + buttonsHeight);            
            notesCtrl.Location = new Point(notesLocation.X, notesLocation.Y + buttonsHeight);            
            notesCtrl.Height = notesHeight - buttonsHeight;

            container.Controls.Add(sshButtons);
            container.ResumeLayout(false);
            container.PerformLayout();
        }

        private FlowLayoutPanel CreateButtons(EntrySettings settings)
        {
            var flow = new FlowLayoutPanel();
            flow.Width = this.notesCtrl.Width + 10;
            flow.Height = 50;
            flow.AutoSize = false;
            flow.FlowDirection = FlowDirection.LeftToRight;
            flow.WrapContents = true;
            flow.Margin = new Padding(0);

            int index = 0;
            foreach (var entry in settings.entries)
            {
                flow.Controls.Add(CreateSSHButton(index, entry.title));
                index += 1;
            }            

            var btnAdd = CreateNormalButton("Add");
            btnAdd.Click += OnAddButtonClick;
            flow.Controls.Add(btnAdd);
            flow.ResumeLayout(false);
            flow.PerformLayout();

            return flow;
        }

        private Button CreateSSHButton(int index, string text)
        {
            var btn = new Button();
            using (var g = form.CreateGraphics())
            {
                var size = g.MeasureString(text, btn.Font);
                btn.Margin = new Padding(4);
                btn.Width = (int)size.Width + 12;
                // btn.Padding = new Padding(3);
                btn.Text = text;
                btn.MouseUp += (s, e) => {
                    activeAutoGenButton = index;
                    OnEntryButtonClick(s, e);
                };
            }
            return btn;
        }

        private Button CreateNormalButton(string text)
        {
            var btn = new Button();
            using (var g = form.CreateGraphics())
            {
                var size = g.MeasureString(text, btn.Font);
                btn.Margin = new Padding(4);
                btn.Width = (int)size.Width + 12;
                // btn.Padding = new Padding(3);
                btn.Text = text;
            }
            return btn;
        }

        private ContextMenuStrip CreateSSHMenuStrip()
        {
            var res = new ContextMenuStrip();

            var itemEdit = res.Items.Add("Edit");
            itemEdit.Click += (s, e) => {
                ShowEntryForm(activeAutoGenButton);
            };

            var itemLaunch = res.Items.Add("Launch");
            var itemRemove = res.Items.Add("Remove");

            return res;
        }

        private void OnEntryButtonClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (sshMenuStrip.Visible)
            {
                sshMenuStrip.Visible = false;
                sshMenuStrip.Hide();
            }
            else
            {
                var button = sender as Button;
                sshMenuStrip.Visible = true;
                sshMenuStrip.Show(button, new Point(0, button.Height));
            }            
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            ShowEntryForm();
        }

        private void ShowEntryForm()
        {
            ShowEntryForm(currentSettings.entries.Count);
        }

        private void ShowEntryForm(int index)
        {
            GenEntry entry;
            if (index < currentSettings.entries.Count)
            {
                entry = currentSettings.entries[index].Clone();
            }
            else
            {
                entry = new GenEntry();
                entry.guid = Guid.NewGuid().ToString();
            }

            using (var form = new GenEntryForm(entry))
            {                
                form.Saved += (s, e) => {
                    if (index < currentSettings.entries.Count)
                    {
                        currentSettings.entries[index] = entry;
                    }
                    else
                    {
                        currentSettings.entries.Add(entry);
                    }
                    Update(currentSettings);
                };

                form.ShowDialog(this.form);
            }
        }

        private static T FindControl<T>(Control ctrl, string name) where T : Control
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
