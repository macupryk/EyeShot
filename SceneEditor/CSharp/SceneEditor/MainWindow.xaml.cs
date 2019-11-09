using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Drawing;
using WindowsApplication1;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using devDept.Graphics;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DrawLight[] Lights;
        private DrawCamera Camera;
        public MainWindow()
        {
            InitializeComponent();
            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            // model2.Unlock("");

            model1.WorkCompleted += model1_WorkCompleted;
            model1.CameraMoveEnd += model1_CameraMoveEnd;

            // sets origin symbol color and coordinate system color
            model2.GetOriginSymbol().LabelColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255,255,255));
            model2.GetCoordinateSystemIcon().LabelColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
        }

        protected override void OnContentRendered(EventArgs e)
        {
            ReadFile rf = new ReadFile("../../../../../../dataset/Assets/Motherboard_ASRock_A330ION.eye");

            ////// model1 settings (View)///////////

            // hides grids
            model1.GetGrid().Visible = false;

            // hides origin symbol
            model1.GetOriginSymbol().Visible = false;

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // loads the entities on the scene
            model1.StartWork(rf);

            // shows color of each light
            colorPanel1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light1.Color.R, 
                                                                                      model1.Light1.Color.G, 
                                                                                      model1.Light1.Color.B));
            colorPanel2.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light2.Color.R,
                                                                                      model1.Light2.Color.G,
                                                                                      model1.Light2.Color.B));
            colorPanel3.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light3.Color.R,
                                                                                      model1.Light3.Color.G,
                                                                                      model1.Light3.Color.B));
            colorPanel4.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light4.Color.R,
                                                                                      model1.Light4.Color.G,
                                                                                      model1.Light4.Color.B));
            colorPanel5.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light5.Color.R,
                                                                                      model1.Light5.Color.G,
                                                                                      model1.Light5.Color.B));
            colorPanel6.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light6.Color.R,
                                                                                      model1.Light6.Color.G,
                                                                                      model1.Light6.Color.B));
            colorPanel7.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light7.Color.R,
                                                                                      model1.Light7.Color.G,
                                                                                      model1.Light7.Color.B));
            colorPanel8.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light8.Color.R,
                                                                                      model1.Light8.Color.G,
                                                                                      model1.Light8.Color.B));
            AmbientLightPanel.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.AmbientLight.R,
                                                                                      model1.AmbientLight.G,
                                                                                      model1.AmbientLight.B));
            // shows the default lights settings

            this.activeLight1.IsChecked = true;                                       // light active = true
            this.lightType1.SelectedIndex = 3;                                      // light Type = DirectionalStationary
            Vector3D direction = model1.Light1.Direction;
            this.lightDX1.Text = direction.X.ToString(CultureInfo.CurrentCulture);  // X direction of the light
            this.lightDY1.Text = direction.Y.ToString(CultureInfo.CurrentCulture);  // Y direction of the light
            this.lightDZ1.Text = direction.Z.ToString(CultureInfo.CurrentCulture);  // Z direction of the light
            this.yieldShadowRadio1.IsChecked = model1.Light1.YieldShadow;    // shadow projection of the light

            this.activeLight2.IsChecked = true;
            this.lightType2.SelectedIndex = 3;
            direction = model1.Light2.Direction;
            this.lightDX2.Text = direction.X.ToString(CultureInfo.CurrentCulture);
            this.lightDY2.Text = direction.Y.ToString(CultureInfo.CurrentCulture);
            this.lightDZ2.Text = direction.Z.ToString(CultureInfo.CurrentCulture);
            this.yieldShadowRadio2.IsChecked = model1.Light2.YieldShadow;

            this.activeLight3.IsChecked = true;
            this.lightType3.SelectedIndex = 3;
            direction = model1.Light3.Direction;
            this.lightDX3.Text = direction.X.ToString(CultureInfo.CurrentCulture);
            this.lightDY3.Text = direction.Y.ToString(CultureInfo.CurrentCulture);
            this.lightDZ3.Text = direction.Z.ToString(CultureInfo.CurrentCulture);
            this.yieldShadowRadio3.IsChecked = model1.Light3.YieldShadow;

            this.lightType4.SelectedIndex = 0;
            this.lightType5.SelectedIndex = 0;
            this.lightType6.SelectedIndex = 0;
            this.lightType7.SelectedIndex = 0;
            this.lightType8.SelectedIndex = 0;

            ////// model2 settings (Scene Editor)//////////
            
            model2.GetGrid().Visible = false;
            model2.SetView(viewType.Trimetric);

            // disables planar reflection
            model2.Rendered.PlanarReflections = false;

            // hides silhouettes drawing
            model2.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;

            // hides shadows drawing
            model2.Rendered.ShadowMode = shadowType.None;

            // sets the light1 to y direction of camera orientation
            model2.Light1.Color = System.Drawing.Color.LightGray;
            model2.Light1.Direction = new Vector3D(0, 1, 0);
            model2.Light1.Stationary = true;

            // turns off Light2 and Light3
            model2.Light2.Active = false;
            model2.Light3.Active = false;

            // adds 2 custom layers
            model2.Layers.Add(new Layer("Camera"));
            model2.Layers.Add(new Layer("Lights"));   

            // fits the model in the viewport 
            model2.ZoomFit();

            //refresh the model control
            model2.Invalidate();

            base.OnContentRendered(e);
        }

        private void model1_WorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            ReadFileAsync rfa = (ReadFileAsync)e.WorkUnit;

            // adds MotherBoard Entities and Materials in view scene
            rfa.AddToScene(model1);

            model1.ZoomFit();
            
            // updates the bounding box size values of the viewport
            model1.Entities.UpdateBoundingBox();

            // copies all items inside model1's master collections to model2
            model1.CopyTo(model2);

            // sets the center and the radius of external content sphere for drawing directional Lights in model2
            Point3D max = model1.Entities.BoxMax;
            Point3D min = model1.Entities.BoxMin;
            Point3D center = new Point3D(min.X + (max.X - min.X)/2, min.Y + (max.Y - min.Y)/2, min.Z + (max.Z - min.Z)/2);
            double radius = Math.Sqrt(Math.Pow(max.X - min.X, 2) + Math.Pow(max.Y - min.Y, 2) + Math.Pow(max.Z - min.Z, 2));

            // creates Light editors
            Lights = new DrawLight[8];
            for (int i = 0; i < 8; i++)

                Lights[i] = new DrawLight(model1, i + 1, center, radius, "Lights");

            UpdateLights();

            model1_CameraMoveEnd(null, null);
            model2.ZoomFit();
        }

        private void model1_CameraMoveEnd(object sender, Model.CameraMoveEventArgs e)
        {
            // removes previous camera drawing
            if(Camera != null)
                Camera.DeletePrevious(model2);

            // draws new camera and new view model of model1 in model2
            Camera = new DrawCamera(model1.Viewports[0], model1.Size.Height, "Camera");
            Camera.Draw(model2);

            for (int i = 0; i < 8; i++)
            
                Lights[i].MoveIfStationary(model2);

            model2.Entities.Regen();
            model2.Invalidate();
        }

        private void Settings_InputsChanged(object sender, EventArgs e)
        {
            UpdateLights();
        }

        private void EnableControls(bool? active, ComboBox type, TextBox x, TextBox y, TextBox z, TextBox dx, TextBox dy, TextBox dz, TextBox spotExp, TextBox linearAt, Slider spotAngle, Button colorButton, RadioButton yieldShadow)
        {
            if (active != null && (bool) active)
            {
                type.IsEnabled = true;
                colorButton.IsEnabled = true;

                switch (type.SelectedIndex)
                {
                    case 0: //point light settings
                        x.IsEnabled = true;
                        y.IsEnabled = true;
                        z.IsEnabled = true;
                        dx.IsEnabled = false;
                        dy.IsEnabled = false;
                        dz.IsEnabled = false;
                        spotExp.IsEnabled = false;
                        linearAt.IsEnabled = false;
                        spotAngle.IsEnabled = false;
                        yieldShadow.IsEnabled = false;
                        yieldShadow.IsChecked = false;
                        break;
                    case 1: //spot light settings
                        x.IsEnabled = true;
                        y.IsEnabled = true;
                        z.IsEnabled = true;
                        dx.IsEnabled = true;
                        dy.IsEnabled = true;
                        dz.IsEnabled = true;
                        spotExp.IsEnabled = true;
                        linearAt.IsEnabled = true;
                        spotAngle.IsEnabled = true;
                        yieldShadow.IsEnabled = true;
                        break;
                    case 2: //directional light settings
                        x.IsEnabled = false;
                        y.IsEnabled = false;
                        z.IsEnabled = false;
                        dx.IsEnabled = true;
                        dy.IsEnabled = true;
                        dz.IsEnabled = true;
                        spotExp.IsEnabled = false;
                        linearAt.IsEnabled = false;
                        spotAngle.IsEnabled = false;
                        yieldShadow.IsEnabled = true;
                        break;
                    case 3: //directional stationary light settings
                        x.IsEnabled = false;
                        y.IsEnabled = false;
                        z.IsEnabled = false;
                        dx.IsEnabled = true;
                        dy.IsEnabled = true;
                        dz.IsEnabled = true;
                        spotExp.IsEnabled = false;
                        linearAt.IsEnabled = false;
                        spotAngle.IsEnabled = false;
                        yieldShadow.IsEnabled = true;
                        break;
                }
            }
            else 
            {
                // Light turn off
                type.IsEnabled = false;
                x.IsEnabled = false;
                y.IsEnabled = false;
                z.IsEnabled = false;
                dx.IsEnabled = false;
                dy.IsEnabled = false;
                dz.IsEnabled = false;
                spotExp.IsEnabled = false;
                linearAt.IsEnabled = false;
                spotAngle.IsEnabled = false;
                colorButton.IsEnabled = false;
                yieldShadow.IsEnabled = false;
                yieldShadow.IsChecked = false;
            }
        }

        private void ChangeSettings(int indexLight, bool? active, ComboBox type, TextBox xt, TextBox yt, TextBox zt, TextBox dxt, TextBox dyt, TextBox dzt, 
            TextBox spotExp, TextBox linearAt, Slider spotAngle, Button colorButton, RadioButton yieldShadow)
        {
            // enables/disables light settings by the type of Light
            EnableControls(active, type, xt, yt, zt, dxt, dyt, dzt, spotExp, linearAt, spotAngle, colorButton, yieldShadow);

            double exp, linear, x, y, z, dx, dy, dz;

            Double.TryParse(spotExp.Text, out exp);
            Double.TryParse(linearAt.Text, out linear);

            // position values
            Double.TryParse(xt.Text, out x);
            Double.TryParse(yt.Text, out y);
            Double.TryParse(zt.Text, out z);

            // direction values
            Double.TryParse(dxt.Text, out dx);
            Double.TryParse(dyt.Text, out dy);
            Double.TryParse(dzt.Text, out dz);

            // sets the Light values
            Lights[indexLight - 1].SetLight(type.SelectedIndex, active, x, y, z, dx, dy, dz, exp, linear, spotAngle.Value, yieldShadow.IsChecked);

            model1.Invalidate();
            model2.Invalidate();
        }

        private void UpdateLights()
        {
            if (Lights == null) return;

            ChangeSettings(1, activeLight1.IsChecked, lightType1, lightX1, lightY1, lightZ1, lightDX1, lightDY1, lightDZ1, lightExponent1,
                lightLinearA1, lightAngle1, colorButton_1, yieldShadowRadio1);
            ChangeSettings(2, activeLight2.IsChecked, lightType2, lightX2, lightY2, lightZ2, lightDX2, lightDY2, lightDZ2, lightExponent2,
                lightLinearA2, lightAngle2, colorButton_2, yieldShadowRadio2);
            ChangeSettings(3, activeLight3.IsChecked, lightType3, lightX3, lightY3, lightZ3, lightDX3, lightDY3, lightDZ3, lightExponent3,
                lightLinearA3, lightAngle3, colorButton_3, yieldShadowRadio3);
            ChangeSettings(4, activeLight4.IsChecked, lightType4, lightX4, lightY4, lightZ4, lightDX4, lightDY4, lightDZ4, lightExponent4,
                lightLinearA4, lightAngle4, colorButton_4, yieldShadowRadio4);
            ChangeSettings(5, activeLight5.IsChecked, lightType5, lightX5, lightY5, lightZ5, lightDX5, lightDY5, lightDZ5, lightExponent5,
                lightLinearA5, lightAngle5, colorButton_5, yieldShadowRadio5);
            ChangeSettings(6, activeLight6.IsChecked, lightType6, lightX6, lightY6, lightZ6, lightDX6, lightDY6, lightDZ6, lightExponent6,
                lightLinearA6, lightAngle6, colorButton_6, yieldShadowRadio6);
            ChangeSettings(7, activeLight7.IsChecked, lightType7, lightX7, lightY7, lightZ7, lightDX7, lightDY7, lightDZ7, lightExponent7,
                lightLinearA7, lightAngle7, colorButton_7, yieldShadowRadio7);
            ChangeSettings(8, activeLight8.IsChecked, lightType8, lightX8, lightY8, lightZ8, lightDX8, lightDY8, lightDZ8, lightExponent8,
                lightLinearA8, lightAngle8, colorButton_8, yieldShadowRadio8);
         
            DrawLights();
        }

        private void DrawLights()
        {
            for (int i = 0; i < 8; i++)
            {
                Lights[i].DeletePrevious(model2);
                Lights[i].Draw(model2);
            }
            model2.Invalidate();
        }

        private void YieldShadowButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (Lights == null) return;

            int indexLight = 0;

            // sets the yieldShadow to only one Light (not supported in Wpf yet)
            if (yieldShadowRadio1.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio1.IsChecked;
            if (yieldShadowRadio2.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio2.IsChecked;
            if (yieldShadowRadio3.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio3.IsChecked;
            if (yieldShadowRadio4.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio4.IsChecked;
            if (yieldShadowRadio5.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio5.IsChecked;
            if (yieldShadowRadio6.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio6.IsChecked;
            if (yieldShadowRadio7.IsChecked != null)
                Lights[indexLight++].Light.YieldShadow = (bool)yieldShadowRadio7.IsChecked;
            if (yieldShadowRadio8.IsChecked != null)
                Lights[indexLight].Light.YieldShadow = (bool)yieldShadowRadio8.IsChecked;

            model1.Invalidate();
        }

        private void colorButtons_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            int indexLight; 

            // gets index Light from button Name
            int.TryParse(((Button)sender).Name.Split('_')[1], out indexLight);
            colorDialog.Color = Lights[indexLight-1].Light.Color;

            // gets and sets color of Light
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               Lights[indexLight-1].Light.Color = colorDialog.Color;
                switch (indexLight)
                {
                    case 1:
                        colorPanel1.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 2:
                        colorPanel2.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 3:
                        colorPanel3.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 4:
                        colorPanel4.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 5:
                        colorPanel5.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 6:
                        colorPanel6.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 7:
                        colorPanel7.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                    case 8:
                        colorPanel8.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B));
                        break;
                }
            } 
            model1.Invalidate();
        }

        private void ambientLightButton_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            // gets and sets AmbientLight color
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                model1.AmbientLight = colorDialog.Color;
                AmbientLightPanel.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R,
                                                                                                colorDialog.Color.G,
                                                                                                colorDialog.Color.B)); 
            }
            model1.Invalidate();
        }
    }
}
