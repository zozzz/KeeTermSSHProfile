using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace KeeTermSSHProfile.UI
{
    public class Helper
    {
        public static Size MeasureFlowLayout(FlowLayoutPanel panel)
        {
            panel.ResumeLayout(false);
            panel.PerformLayout();

            var count = panel.Controls.Count;
            Control lastItem;

            if (count != 0)
            {
                switch (panel.FlowDirection)
                {
                    case FlowDirection.TopDown:
                    case FlowDirection.BottomUp:
                        lastItem = panel.Controls[count - 1];
                        return new Size(panel.Width, lastItem.Location.Y + lastItem.Height);
            

                    case FlowDirection.LeftToRight:
                    case FlowDirection.RightToLeft:
                        lastItem = panel.Controls[count - 1];
                        return new Size(lastItem.Location.X + lastItem.Width, panel.Height);
                }
            }

            return new Size(panel.Width, panel.Height);
        }
    }
}
