using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using devDept.Eyeshot;
using devDept.Graphics;
using System.ComponentModel;

namespace WpfApplication1
{
    [ToolboxItem(true)/*, Designer(typeof(MyModelControlDesigner))*/]
    public class MyModel : Model
    {
        public MyModel()
        {
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            InitializeViewportsByNumber(2);

            base.OnHandleCreated(e);
        }        

        public override void UpdateViewportsSizeAndLocation()
        {
            int height1 = (int)(2 * Size.Height / 3.0 - ViewportsGap / 2.0);
            int height2 = (int)(1 * Size.Height / 3.0 - ViewportsGap / 2.0);

            Viewports[0].Size = new System.Drawing.Size((int)Size.Width, height1);
            Viewports[1].Size = new System.Drawing.Size((int)Size.Width, height2);

            Viewports[1].Location = new System.Drawing.Point(0, height1 + ViewportsGap);
        }

        private void InitializeViewportsByNumber(int numberOfViewports)
        {
            if (Viewports.Count > numberOfViewports)
            {
                while (Viewports.Count > numberOfViewports)
                    Viewports.RemoveAt(Viewports.Count - 1);
            }
            else
            {
                while (Viewports.Count < numberOfViewports)
                    Viewports.Add(new Viewport());
            }
        }

    }
}