using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Drawing.Point;

namespace WpfApplication1
{
    public class MyModel : Model
    {
        bool displayHelp;

        private void RenderText()
        {
            List<string> helpStrings = new List<string>();

            string formatString = "{0:0.00}";

            if (displayHelp)
            {
                helpStrings.Add("First person camera behavior");
                helpStrings.Add( "  Press W and S to move forwards and backwards");
                helpStrings.Add( "  Press A and D to strafe left and right");
                helpStrings.Add( "  Press E and Q to move up and down");
                helpStrings.Add( "  Move mouse to free look");
                helpStrings.Add("");
                helpStrings.Add( "Flight camera behavior");
                helpStrings.Add( "  Press W and S to move forwards and backwards");
                helpStrings.Add( "  Press A and D to yaw left and right");
                helpStrings.Add( "  Press E and Q to move up and down");
                helpStrings.Add( "  Move mouse to pitch and roll");
                helpStrings.Add("");
                helpStrings.Add( "Press M to enable/disable mouse smoothing");
                helpStrings.Add( "Press + and - to change camera rotation speed");
                helpStrings.Add( "Press , and . to change mouse sensitivity");
                helpStrings.Add( "Press SPACE to toggle between flight and first person behavior");
                helpStrings.Add( "Press ESC to exit");
                helpStrings.Add("");
                helpStrings.Add( "Press H to hide help");
            }
            else
            {
                helpStrings.Add("Camera");
                helpStrings.Add("  Speed:" + string.Format(formatString,Viewports[0].Navigation.RotationSpeed));

                helpStrings.Add( "  Behavior: " + Viewports[0].Navigation.Mode);
                helpStrings.Add("");
                helpStrings.Add( "Press H to display help");
            }

            Font myFont = UtilityEx.GetFont(FontFamily, FontStyle, FontWeight, FontSize);

            int posY = (int)Size.Height - 2 * myFont.Height;            
            
            for (int i = 0; i < helpStrings.Count; i++)
            {
                DrawText(10, posY, helpStrings[i], myFont, Color.White,ContentAlignment.BottomLeft);
                posY -= (int)1.5*myFont.Height;
            }
        }

        protected override void DrawOverlay(DrawSceneParams myParams)
        {
            base.DrawOverlay(myParams);
            RenderText();
        }      

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            
            switch (e.Key)
            {
                case Key.Space:
                    if (Viewports[0].Navigation.Mode == devDept.Eyeshot.Camera.navigationType.Walk)

                        Viewports[0].Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Fly;

                    else
                        Viewports[0].Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Walk;
                    break;

                case Key.H:
                    displayHelp = !displayHelp;
                    break;

                case Key.Add:
                    Viewports[0].Navigation.RotationSpeed += 0.2;
                    if (Viewports[0].Navigation.RotationSpeed >= 10)
                        Viewports[0].Navigation.RotationSpeed = 10;
                    break;

                case Key.Subtract:
                    Viewports[0].Navigation.RotationSpeed -= 0.2f;
                    if (Viewports[0].Navigation.RotationSpeed <= 0)
                        Viewports[0].Navigation.RotationSpeed = 0.01;

                    break;
            }

            Invalidate();
        }     
    }

}