using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Collections.ObjectModel;
using Bitmap = System.Drawing.Bitmap;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {        
        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.            

            // removes some of the standard Eyeshot buttons
            List<ToolBarButton> buttons = new List<ToolBarButton>();            

            BitmapImage leftBmp = new BitmapImage(GetUriFromResource("previous.png"));                        
            buttons.Add(new devDept.Eyeshot.ToolBarButton(leftBmp, "PreviousViewButton", "Previous View", devDept.Eyeshot.ToolBarButton.styleType.PushButton, true));

            BitmapImage rightBmp = new BitmapImage(GetUriFromResource("next.png"));                        
            buttons.Add(new devDept.Eyeshot.ToolBarButton(rightBmp, "NextViewButton", "Next View", devDept.Eyeshot.ToolBarButton.styleType.PushButton, true));

            // Add a separator button            
            buttons.Add(new devDept.Eyeshot.ToolBarButton(null, "Separator", "", devDept.Eyeshot.ToolBarButton.styleType.Separator, true));

            buttons.Add(model1.GetToolBar().Buttons[0]);
            buttons.Add(model1.GetToolBar().Buttons[1]);

            // Add a separator button            
            buttons.Add(new devDept.Eyeshot.ToolBarButton(null, "Separator", "", devDept.Eyeshot.ToolBarButton.styleType.Separator, true));

            BitmapImage usersBmp = new BitmapImage(GetUriFromResource("users.png"));                        
            buttons.Add(new devDept.Eyeshot.ToolBarButton(usersBmp, "MyPushButton", "MyPushButton", devDept.Eyeshot.ToolBarButton.styleType.PushButton, true));

            BitmapImage gearsBmp = new BitmapImage(GetUriFromResource("gears.png"));                        
            buttons.Add(new devDept.Eyeshot.ToolBarButton(gearsBmp, "MyToggleButton", "MyToggleButton", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, true));

            model1.GetToolBar().Buttons = new ToolBarButtonList(model1.GetToolBar(), buttons);          
        }

        private Uri GetUriFromResource(string resourceFilename)
        {
            return new Uri(@"pack://application:,,,/Resources/" + resourceFilename);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            //////
            // adds a custom mesh to the CoordinateSystemIcon
            //////
            model1.GetCoordinateSystemIcon().Entities.Clear();

            // reads the mesh obj (eyeshot file) from the Assets directory
            devDept.Eyeshot.Translators.ReadFile rf = new devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/figure_Object011.eye");
            rf.DoWork();
            Mesh torso = (Mesh)rf.Entities[0];
            torso.Scale(0.5, 0.5, 0.5);
            torso.NormalAveragingMode = Mesh.normalAveragingType.Averaged;
            torso.Regen(0.1);

            // orients the mesh
            Point3D midPoint = (torso.BoxMin + torso.BoxMax) / 2;
            torso.Translate(-midPoint.X, -midPoint.Y, -midPoint.Z);
            torso.Rotate(Math.PI / 2, Vector3D.AxisX);

            // sets the color            
            torso.Color = System.Drawing.Color.Pink;

            // sets the model on the CoordinateSystemIcon entities and remove the labels
            CoordinateSystemIcon csi = model1.GetCoordinateSystemIcon();
            csi.Entities.Add(torso);
            csi.LabelAxisX = "";
            csi.LabelAxisY = "";
            csi.LabelAxisZ = "";
            csi.Lighting = true;

            model1.CompileUserInterfaceElements();
            // sets my event handler to the ToolBarButton.Click event

            // Previous view
            model1.GetToolBar().Buttons[0].Click += PreviousViewClickEventHandler;

            // Next view
            model1.GetToolBar().Buttons[1].Click += NextViewClickEventHandler;

            // MyPushButton
            model1.GetToolBar().Buttons[6].Click += MyPushButtonClickEventHandler;

            // MyToggleButton
            model1.GetToolBar().Buttons[7].Click += MyToggleButtonClickEventHandler;            
            
            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();

            //refresh the viewport
            model1.Invalidate();
         
            base.OnContentRendered(e);
        }

        public void PreviousViewClickEventHandler(object sender, EventArgs e)
        {
            model1.PreviousView();
        }

        public void NextViewClickEventHandler(object sender, EventArgs e)
        {
            model1.NextView();
        }

        public void MyPushButtonClickEventHandler(object sender, EventArgs e)
        {
            MessageBox.Show("You clicked the custom PushButton.");
        }

        public void MyToggleButtonClickEventHandler(object sender, EventArgs e)
        {
            if (((devDept.Eyeshot.ToolBarButton)sender).Pushed)
                MessageBox.Show("You pressed the custom ToggleButton.");
            else
                MessageBox.Show("You un-pressed the custom ToggleButton.");
        }        
    }
}