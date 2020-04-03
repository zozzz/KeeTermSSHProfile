using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeeTermSSHProfile.UI
{
    public partial class GlobalOptions : UserControl
    {
        private KeeTermSSHProfileExt ext;

        public GlobalOptions(KeeTermSSHProfileExt ext)
        {
            InitializeComponent();
            this.ext = ext;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
