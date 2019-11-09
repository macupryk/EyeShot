using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using devDept.Eyeshot.Translators;

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

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {
            model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor;
            model1.Rendered.EdgeThickness = 1;

            model1.GetGrid().Min = new Point3D(-150, -100);
            model1.GetGrid().Max = new Point3D(+200, +100);
            model1.GetGrid().Step = 20;
            model1.GetGrid().AutoSize = false;


            Block b = new Block("CrankShaft");

            LinearPath lp = new LinearPath(6);

            lp.Vertices[0] = new Point3D(0, -50, 0);
            lp.Vertices[1] = new Point3D(0, -20, 0);
            lp.Vertices[2] = new Point3D(50, -20, 0);
            lp.Vertices[3] = new Point3D(50, +20, 0);
            lp.Vertices[4] = new Point3D(0, +20, 0);
            lp.Vertices[5] = new Point3D(0, +50, 0);

            lp.ColorMethod = colorMethodType.byEntity;            
            lp.Color = System.Drawing.Color.Blue;
            lp.LineWeightMethod = colorMethodType.byEntity;
            lp.LineWeight = 1.5f;

            b.Entities.Add(lp);

            model1.Blocks.Add(b);

            model1.Entities.Add(new Rotating("CrankShaft"));


            b = new Block("ConnectingRod");

            Plane XZ = new Plane(Point3D.Origin, Vector3D.AxisX, Vector3D.AxisZ);

            b.Entities.Add(new Circle(XZ, Point2D.Origin, 8));
            b.Entities.Add(new Circle(XZ, Point2D.Origin, 6));
            b.Entities.Add(new Circle(XZ, new Point2D(120, 0), 15));
            b.Entities.Add(new Circle(XZ, new Point2D(120, 0), 10));
            b.Entities.Add(new Line(XZ, new Point2D(6.928, 4), new Point2D(105.543, 4)));
            b.Entities.Add(new Line(XZ, new Point2D(6.928, -4), new Point2D(105.543, -4)));
            b.Entities.Add(new Line(XZ, new Point2D(-2, 0), new Point2D(2, 0)));
            b.Entities.Add(new Line(XZ, new Point2D(0, -2), new Point2D(0, 2)));
            b.Entities.Add(new Line(XZ, new Point2D(120 - 3, 0), new Point2D(120 + 3, 0)));
            b.Entities.Add(new Line(XZ, new Point2D(120, -3), new Point2D(120, 3)));

            foreach (Entity ent in b.Entities)
            {
                ent.ColorMethod = colorMethodType.byEntity;                
                ent.Color = System.Drawing.Color.Red;
            }

            model1.Blocks.Add(b);

            model1.Entities.Add(new Oscillating("ConnectingRod"));


            b = new Block("Axis");

            Line line = new Line(0, +30, 0, -30);

            line.ColorMethod = colorMethodType.byEntity;            
            line.Color = System.Drawing.Color.Black;

            b.Entities.Add(line);

            model1.Blocks.Add(b);

            model1.Entities.Add(new Translating("Axis"));


            b = new Block("Piston");

            ReadFile readFile = new ReadFile("../../../../../../dataset/Assets/Piston.eye");
            readFile.DoWork();

            Mesh m = (Mesh)readFile.Entities[0];

            m.EdgeStyle = Mesh.edgeStyleType.Sharp;
            m.ColorMethod = colorMethodType.byEntity;            
            m.Color = System.Drawing.Color.DarkGray;

            b.Entities.Add(m);

            model1.Blocks.Add(b);

            model1.Entities.Add(new Translating("Piston"));

            // Bounding box override
            model1.BoundingBox.Min = new Point3D(-110, -50, -70);
            model1.BoundingBox.Max = new Point3D(+170, +50, +70);
            model1.BoundingBox.OverrideSceneExtents = true;

            // Shadows are not currently supported in animations
            model1.Rendered.ShadowMode = shadowType.None;

            model1.StartAnimation(1);
            
            model1.ZoomFit();

            model1.Invalidate();
         
            base.OnContentRendered(e);
        }

        #region Animation helper classes

        class Translating : BlockReference
        {

            double alpha;
            double xPos;

            public Translating(string blockName)
                : base(0, 0, 0, blockName, 1, 1, 1, 0)
            {
            }

            protected override void Animate(int frameNumber)
            {

                alpha += 2;

                if (alpha > 359)

                    alpha = 0;

                // cranckshaft radius
                double r = 50;
                // connecting rod length
                double l = 120;

                double beta = Math.Asin(r * Math.Sin(Utility.DegToRad(alpha)) / l);

                xPos = r * Math.Cos(Utility.DegToRad(alpha)) - l * Math.Cos(beta);

            }

            private Transformation customTransform;

            public override void MoveTo(DrawParams data)
            {
                base.MoveTo(data);
                
                // 100 + xPos: the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
                customTransform = new Translation(100 + xPos, 0, 0);
                data.RenderContext.MultMatrixModelView(customTransform);
            }

            public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
            {
                // Call the base with the transformed "center", to avoid undesired clipping
                return base.IsInFrustum(data, customTransform * center, radius);
            }
        }

        class Oscillating : BlockReference
        {

            double alpha;
            double beta;
            double xPos;

            public Oscillating(string blockName)
                : base(0, 0, 0, blockName, 1, 1, 1, 0)
            {
            }

            protected override void Animate(int frameNumber)
            {

                alpha += 2f;

                if (alpha > 359)

                    alpha = 0;

                // cranckshaft radius
                double r = 50;
                // connecting rod length
                double l = 120;

                beta = Math.Asin(r * Math.Sin(Utility.DegToRad(alpha)) / l);

                xPos = r * Math.Cos(Utility.DegToRad(alpha)) - l * Math.Cos(beta);

            }

            private Transformation customTransform;

            public override void MoveTo(DrawParams data)
            {
                base.MoveTo(data);
                
                // 100 + xPos: the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
                customTransform = new Translation(100 + xPos, 0, 0) * new devDept.Geometry.Rotation(beta, new Vector3D(0, 1, 0));
                data.RenderContext.MultMatrixModelView(customTransform);
            }

            public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
            {
                // Call the base with the transformed "center", to avoid undesired clipping
                return base.IsInFrustum(data, customTransform * center, radius);
            }
        }

        class Rotating : BlockReference
        {

            double alpha;

            public Rotating(string blockName)
                : base(0, 0, 0, blockName, 1, 1, 1, 0)
            {
            }

            protected override void Animate(int frameNumber)
            {

                alpha += 2f;

                if (alpha > 359)

                    alpha = 0;
            }

            private Transformation customTransform;

            public override void MoveTo(DrawParams data)
            {
                base.MoveTo(data);

                // the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
                customTransform = new Translation(100, 0, 0) * new devDept.Geometry.Rotation(Utility.DegToRad(alpha), new Vector3D(0, 1, 0));
                data.RenderContext.MultMatrixModelView(customTransform);
            }

            public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
            {
                // Call the base with the transformed "center", to avoid undesired clipping
                return base.IsInFrustum(data, customTransform * center, radius);
            }
        }

        #endregion
    }
}