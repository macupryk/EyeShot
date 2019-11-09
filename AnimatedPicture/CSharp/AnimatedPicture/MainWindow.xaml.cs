using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using System.Windows.Threading;

namespace AnimatedPicture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer _timer;
        private int _imageIndex = 0;
        private Picture _pct;
        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            // Hides the edges
            model1.Rendered.ShowEdges = false;            

            // Adds the pattern for the lines
            model1.LineTypes.Add("DashDot", new float[] { 5f, -1f, 1f, -1f });            
        }
        
        protected override void OnContentRendered(EventArgs e)
        {
            // Adds the picture
            _pct = new Picture(Plane.XY, 100, 100, new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic1.png"));
            _pct.Lighted = false;
            model1.Entities.Add(_pct);

            // Adds the custom circle
            MyCircle c1 = new MyCircle(68, 11, 0, 46);            
            model1.Entities.Add(c1, System.Drawing.Color.Red);

            // Adds the custom lines            
            MyLine myLn1 = new MyLine(c1.Center.X - c1.Radius * 1.1, c1.Center.Y, c1.Center.X + c1.Radius * 1.1, c1.Center.Y);            
            myLn1.LineTypeMethod = colorMethodType.byEntity;
            myLn1.LineTypeName = "DashDot";
            model1.Entities.Add(myLn1, System.Drawing.Color.Green);
            
            MyLine myLn2 = new MyLine(c1.Center.X, c1.Center.Y - c1.Radius * 1.1, c1.Center.X, c1.Center.Y + c1.Radius * 1.1);            
            myLn2.LineTypeMethod = colorMethodType.byEntity;
            myLn2.LineTypeName = "DashDot";
            model1.Entities.Add(myLn2, System.Drawing.Color.Green);

            // Sets top view and fits the model in the viewport
            model1.SetView(viewType.Top, true, false);            

            // Refreshes the model control
            model1.Invalidate();

            // Starts the timer to update the picture
            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Tick += Timer_Tick1;
            _timer.Interval = TimeSpan.FromMilliseconds(120);
            _timer.Start();

            base.OnContentRendered(e);
        }        

        private void Timer_Tick1(object sender, EventArgs e)
        {
            switch (_imageIndex)
            {
                case 0:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic1.png");
                    break;
                case 1:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic2.png");
                    break;
                case 2:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic3.png");
                    break;
                case 3:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic4.png");
                    break;
                case 4:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic5.png");
                    break;
                case 5:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic6.png");
                    break;
                case 6:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic7.png");
                    break;
                case 7:
                    _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic8.png");
                    _imageIndex = -1;
                    break;
            }

            _imageIndex++;

            // Compiles the picture in the main thread to avoid an access violation exception.
            Dispatcher.BeginInvoke(new Action(() => RefreshPicture()), null);
        }

        private void RefreshPicture()
        {
            // Compiles the picture and refreshes the Model.
            _pct.RegenMode = regenType.CompileOnly;
            _pct.Compile(new CompileParams(model1));
            model1.Invalidate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stops the timer
            _timer.Stop();
        }


    }

    #region Custom classes
    internal class MyCircle : Circle
    {
        public MyCircle(double x, double y, double z, double radius) : base(x, y, z, radius) { }

        protected override void Draw(DrawParams data)
        {
            data.RenderContext.EndDrawBufferedLines();

            data.RenderContext.PushDepthStencilState();
            data.RenderContext.SetState(depthStencilStateType.DepthTestAlways);

            base.Draw(data);

            data.RenderContext.EndDrawBufferedLines();
            data.RenderContext.PopDepthStencilState();
        }
    }
    internal class MyLine : devDept.Eyeshot.Entities.Line
    {        
        public MyLine(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2) { }

        protected override void Draw(DrawParams data)
        {
            data.RenderContext.EndDrawBufferedLines();

            data.RenderContext.PushDepthStencilState();
            data.RenderContext.SetState(depthStencilStateType.DepthTestAlways);

            base.Draw(data);

            data.RenderContext.EndDrawBufferedLines();
            data.RenderContext.PopDepthStencilState();
        }
    }
    #endregion
}
