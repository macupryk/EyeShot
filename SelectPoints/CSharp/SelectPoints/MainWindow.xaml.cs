using System;
using System.Collections.Generic;
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
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Graphics;

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

            model1.SelectionChanged += Model_OnSelectionChanged;                
        }

        private void Model_OnSelectionChanged(object sender, EventArgs e)
        {
            MyFastPointCloud mfp;

            if (GetSelectedPointCloud(out mfp))
            {
                model1.Entities.Regen();
                model1.Invalidate();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            model1.GetGrid().Visible = false;

            // generates point cloud points
            Entity ent = FunctionPlot();

            // adds it to the vieport
            model1.Entities.Add(ent);

            // Sets trimetric view
            model1.SetView(viewType.Trimetric);

            // Fits the model in the viewport
            model1.ZoomFit();

            // Refresh the viewport
            model1.Invalidate();

            // Sets the SelectByPolygon action mode
            model1.ActionMode = actionType.SelectByPolygon;            

            base.OnContentRendered(e);
        }        

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            MyFastPointCloud mfp;

            if (GetSelectedPointCloud(out mfp))
            {
                mfp.DeletePoints();

                model1.Entities.Regen();

                model1.Invalidate();
            }
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            MyFastPointCloud mfp;

            if (GetSelectedPointCloud(out mfp))
            {
                mfp.Undo();

                model1.Entities.Regen();

                model1.Invalidate();
            }
        }

        private void unselectButton_Click(object sender, RoutedEventArgs e)
        {
            MyFastPointCloud mfp;

            if (GetSelectedPointCloud(out mfp))
            {
                mfp.Unselect();

                model1.Entities.Regen();

                model1.Invalidate();
            }
        }

        private bool GetSelectedPointCloud(out MyFastPointCloud mfp)
        {
            foreach (Entity ent in model1.Entities)
            {
                if (ent is MyFastPointCloud)
                {
                    mfp = (MyFastPointCloud)ent;

                    if (mfp.CustomSelected)

                        return true;
                }
            }

            mfp = null;

            return false;
        }


        /// <summary>
        /// Draws a point cloud of 80x80 vertices
        /// </summary>
        public FastPointCloud FunctionPlot()
        {

            int rows = 80;
            int cols = 80;
            float scale = 4f;

            PointCloud surface = new PointCloud(rows * cols, 3, PointCloud.natureType.Multicolor);

            for (int j = 0; j < rows; j++)

                for (int i = 0; i < cols; i++)
                {

                    float x = i / 5f;
                    float y = j / 5f;

                    float f = 0;

                    float den = (float)Math.Sqrt(x * x + y * y);

                    if (den != 0)

                        f = scale * (float)Math.Sin(Math.Sqrt(x * x + y * y)) / den;

                    surface.Vertices[i + j * cols] = new PointRGB(x, y, f, MyFastPointCloud.BaseColor, MyFastPointCloud.BaseColor, MyFastPointCloud.BaseColor);

                }

            MyFastPointCloud surfaceFast = new MyFastPointCloud(surface.ConvertToFastPointCloud(), model1);

            surfaceFast.LineWeightMethod = colorMethodType.byEntity;
            surfaceFast.LineWeight = 2;

            return surfaceFast;

        }

        class MyFastPointCloud : FastPointCloud
        {
            public Model model;
            private int selectedCount = 0;

            private bool _customSelected;

            internal static byte BaseColor = 150;

            // use a custom flag for selection, otherwise the entity will be drawn with selection color
            public bool CustomSelected
            {
                get { return _customSelected; }
                set { _customSelected = value; }
            }

            /// <summary>
            /// Point list of the last delete action
            /// </summary>
            List<Point3D> lastDeleteContents = new List<Point3D>();

            public MyFastPointCloud(FastPointCloud another, Model control)
                : base(another)
            {
                model = control;
            }

            protected override bool IsCrossingScreenPolygon(ScreenPolygonParams data)
            {
                int newSelectedCount = 0;

                for (int j = 0; j < PointArray.Length; j += 3)
                {
                    Point2D onScreen = model.WorldToScreen(PointArray[j], PointArray[j + 1], PointArray[j + 2]);

                    if (ColorArray[j] != 255 &&
                        onScreen.X > data.Min.X && onScreen.X < data.Max.X &&
                        onScreen.Y > data.Min.Y && onScreen.Y < data.Max.Y &&
                        Utility.PointInPolygon(onScreen, data.ScreenPolygon))
                    {
                        // sets point color to red
                        ColorArray[j] = 255;

                        newSelectedCount++;
                    }

                }

                selectedCount += newSelectedCount;

                if (newSelectedCount != 0)
                {
                    RegenMode = regenType.CompileOnly;

                    CustomSelected = true;
                }
                else

                    if (selectedCount == 0)

                        CustomSelected = false;

                return false;
            }

            /// <summary>
            /// Deletes selected points
            /// </summary>
            public void DeletePoints()
            {
                int count = 0;
                bool firstTime = true;

                // fills the vertices array only with black points
                for (int j = 0; j < PointArray.Length; j += 3)
                {
                    if (ColorArray[j] == BaseColor)
                    {
                        PointArray[count] = PointArray[j];
                        PointArray[count + 1] = PointArray[j + 1];
                        PointArray[count + 2] = PointArray[j + 2];
                        ColorArray[count] = ColorArray[j];
                        ColorArray[count + 1] = ColorArray[j + 1];
                        ColorArray[count + 2] = ColorArray[j + 2];
                        count += 3;
                    }

                    else
                    {

                        if (firstTime)
                        {
                            lastDeleteContents.Clear();
                            firstTime = false;
                        }
                        lastDeleteContents.Add(new Point3D(PointArray[j], PointArray[j + 1], PointArray[j + 2]));

                    }
                }

                ResizeVertices(PointArray.Length - (selectedCount * 3));

                RegenMode = regenType.RegenAndCompile;

                selectedCount = 0;
            }

            /// <summary>
            /// Resizes the entity vertices array
            /// </summary>
            void ResizeVertices(int newSize)
            {

                float[] v = PointArray;

                Array.Resize<float>(ref v, newSize);

                byte[] b = ColorArray;

                Array.Resize<byte>(ref b, newSize);

                PointArray = v;

                ColorArray = b;
            }

            /// <summary>
            /// Undo deletion.
            /// </summary>
            public void Undo()
            {
                int prevLen = PointArray.Length;

                ResizeVertices(PointArray.Length + (lastDeleteContents.Count * 3));

                // adds back the points to the point cloud
                for (int i = 0, j = 0; i < lastDeleteContents.Count * 3; i += 3, j++)
                {
                    PointArray[prevLen + i] = (float)lastDeleteContents[j].X;
                    PointArray[prevLen + i + 1] = (float)lastDeleteContents[j].Y;
                    PointArray[prevLen + i + 2] = (float)lastDeleteContents[j].Z;
                    ColorArray[prevLen + i] = 255;
                    ColorArray[prevLen + i + 1] = BaseColor;
                    ColorArray[prevLen + i + 2] = BaseColor;
                }

                selectedCount += lastDeleteContents.Count;

                lastDeleteContents.Clear();

                RegenMode = regenType.RegenAndCompile;
            }

            /// <summary>
            /// Unselect points
            /// </summary>
            public void Unselect()
            {
                // set the color of each point to black
                for (int i = 0; i < PointArray.Length; i += 3)

                    ColorArray[i] = BaseColor;

                selectedCount = 0;

                RegenMode = regenType.CompileOnly;
            }

        }        
        
    }
}