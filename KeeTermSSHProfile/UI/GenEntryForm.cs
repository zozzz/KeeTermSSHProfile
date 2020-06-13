using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace KeeTermSSHProfile.UI
{
    public class GenEntryForm : Form
    {
        public const int GAP = 10;
        public const int INSET = 10;

        public FlowLayoutPanel body { get; private set; }
        public Label buttonsSep { get; private set; }
        public FlowLayoutPanel buttons { get; private set; }
        public List<GenEntryFormGroup> groups { get; private set; }
        public GenEntry entry { get; private set; }

        public event EventHandler<GenEntryForm> Saved;

        public static GenEntryFormGroup CreateGroup(string title)
        {
            return new GenEntryFormGroup(title);
        }

        public static TextBox CreateTextBox(GenEntry entry, string field)
        {
            var ctrl = new TextBox();
            var property = entry.GetType().GetProperty(field);
            var initial = property != null ? property.GetValue(entry) : entry.Get(field, "");

            ctrl.Text = initial != null ? initial.ToString() : "";
            ctrl.TextChanged += (a, e) => {
                if (property != null)
                {
                    property.SetValue(entry, ctrl.Text);
                }
                else
                {
                    entry.Set(field, ctrl.Text);
                }
            };

            return ctrl;
        }

        public GenEntryForm(GenEntry entry)
        {
            this.entry = entry;
            groups = new List<GenEntryFormGroup>();

            InitFormLayout();
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 400;
            Height = 600;

            AddGroup(CreateCommonGroup());

            var generators = Generator.Available()
                .OrderBy(generator => generator.Title);
            foreach (var generator in generators)
            {
                var groupControl = generator.CreateOptionGroup(entry);
                if (groupControl != null)
                {
                    AddGroup(groupControl);
                }
            }
        }

        public void AddGroup(GenEntryFormGroup group)
        {
            if (!groups.Contains(group))
            {
                groups.Add(group);
                body.Controls.Add(group.group);
                UpdateGroups();
            }
        }

        public void RemoveGroup(GenEntryFormGroup group)
        {
            groups.Remove(group);
            body.Controls.Remove(group.group);
            UpdateGroups();
        }

        private GenEntryFormGroup CreateCommonGroup()
        {
            var res = CreateGroup("Common");

            res.Controls.Add(new GenEntryFormRow("Title", new Control[] { CreateTextBox(entry, "title") }));            

            return res;
        }

        protected void InitFormLayout()
        {
            body = new FlowLayoutPanel();
            body.FlowDirection = FlowDirection.TopDown;
            body.WrapContents = false;
            body.AutoScroll = true;            

            buttons = new FlowLayoutPanel();
            buttons.FlowDirection = FlowDirection.RightToLeft;            

            buttonsSep = new Label();
            buttonsSep.Text = "";
            buttonsSep.BorderStyle = BorderStyle.Fixed3D;
            buttonsSep.AutoSize = false;
            buttonsSep.Height = 2;

            var btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Click += BtnOk_Click;

            var btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;

            buttons.Height = btnOk.Height + btnOk.Margin.Top + btnOk.Margin.Bottom;

            buttons.Controls.Add(btnOk);
            buttons.Controls.Add(btnCancel);

            Controls.Add(body);
            Controls.Add(buttonsSep);
            Controls.Add(buttons);
            ResumeLayout(false);
            UpdateElements();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Debug.Print("BtnOk_Click", Saved);
            Saved?.Invoke(this, this);
            Hide();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateElements();
            UpdateGroups();
        }

        private void UpdateElements()
        {
            var formWidth = this.ClientRectangle.Width;
            var formHeight = this.ClientRectangle.Height;

            body.Width = formWidth;
            body.Padding = new Padding(INSET, 0, INSET, 0);
            buttons.Width = formWidth - INSET * 2;

            var buttonsSize = Helper.MeasureFlowLayout(buttons);
            buttons.Location = new Point(INSET, formHeight - buttonsSize.Height - INSET);

            body.Height = formHeight - buttonsSize.Height - GAP - INSET * 2;
            body.Location = new Point(0, INSET);

            buttonsSep.Location = new Point(0, buttons.Location.Y - GAP / 2);
            buttonsSep.Width = formWidth;
        }

        private void UpdateGroups()
        {
            var formWidth = this.ClientRectangle.Width;
            var top = 0;
            foreach (var group in groups)
            {
                group.Width = formWidth - INSET * 3;
                group.UpdateElements();
                group.group.Location = new Point(INSET, top);
                top += group.Height;
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var group in groups)
            {
                group.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class GenEntryFormGroup
    {
        public GroupBox group { get; private set; }
        public FlowLayoutPanel body { get; private set; }

        public Control.ControlCollection Controls { get { return body.Controls; } }

        public int Width {
            set {
                group.Width = value;
                body.Width = value - GenEntryForm.GAP * 2;
            }
            get {
                return group.Width;
            }
        }

        public int Height {
            set {
                group.Height = value;
                body.Height = value - GenEntryForm.GAP * 3;
            }
            get {
                return group.Height;
            }
        }

        public GenEntryFormGroup(string title)
        {
            group = new GroupBox();
            group.Text = title;
            body = new FlowLayoutPanel();
            body.FlowDirection = FlowDirection.TopDown;
            body.WrapContents = false;
            //body.BackColor = Color.MediumVioletRed;
            body.Height = 0;

            group.Controls.Add(body);
            body.Location = new Point(GenEntryForm.GAP, GenEntryForm.GAP * 2);
        }

        public void UpdateElements()
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.Width = body.Width;
            }

            var bodySize = Helper.MeasureFlowLayout(body);
            Height = bodySize.Height + GenEntryForm.GAP * 3;
        }

        public void Dispose()
        {
            group.Dispose();
            body.Dispose();
        }
    }

    public class GenEntryFormRow : UserControl
    {
        public const int LABEL_WIDTH = 80;

        public Label label { get; private set; }
        public override string Text {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public FlowLayoutPanel body { get; private set; }

        public GenEntryFormRow(string label, Control[] controls) : base()
        {
            this.Margin = new Padding(0, 0, 0, GenEntryForm.GAP);

            this.label = new Label();
            this.label.Text = label;
            this.label.Width = LABEL_WIDTH;

            this.body = new FlowLayoutPanel();
            this.body.FlowDirection = FlowDirection.TopDown;
            this.body.WrapContents = false;
            this.body.AutoScroll = false;
            this.body.Height = 0;

            foreach (var ctrl in controls)
            {
                this.body.Controls.Add(ctrl);
            }

            this.Controls.Add(this.label);
            this.Controls.Add(this.body);
            this.Location = new Point(0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            body.Width = Width - label.Width - GenEntryForm.GAP;

            if (body.Controls.Count > 0)
            {
                var last = body.Controls[body.Controls.Count - 1];
                foreach (Control child in body.Controls)
                {
                    child.Margin = new Padding(0, 0, 0, child == last ? 0 : GenEntryForm.GAP);
                    child.Width = body.Width;
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            body.Height = 0;
            var bodySize = Helper.MeasureFlowLayout(body);

            body.Height = bodySize.Height;
            Height = Math.Max(label.Height, body.Height);
            label.Location = new Point(0, 5);
            body.Location = new Point(label.Width + GenEntryForm.GAP, 0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    /*
    public class ManagedControl : Control {
        public GenEntry entry;
        public string field;
        public Control control;

        public ManagedControl(GenEntry entry, string field, Control control)
        {
        }
    }*/

}
