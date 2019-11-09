using System;
using System.Collections.Generic;
using System.Drawing;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;

// Values for hitting the targets
// Target 1 --> Launch angle 34, Direction angle: 48, Fire power: 10
// Target 2 --> Launch angle: 30, Direction angle: 9, Fire power: 13
// Target 3 --> Launch angle: 23, Direction angle: -42, Fire power: 12

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        int _numEntityInScene = 10;

        Target _target1 = new Target();
        Target _target2 = new Target();
        Target _target3 = new Target();

        int[] _initialSlidersValue = new int[3];

        private double _lastLaunchAngleSlider_Value, _lastDirectionAngleSlider_Value;
        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.       

            model1.GetOriginSymbol().Visible = false;

            model1.DisplayMode = displayType.Rendered;

            // display mode settings
            model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor;
            model1.Rendered.EdgeThickness = 1;
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            model1.Rendered.PlanarReflections = false;
            // shadows are not currently supported in animations
            model1.Rendered.ShadowMode = devDept.Graphics.shadowType.None;

            // grid settings
            model1.GetGrid().Visible = false;

            model1.Camera.FocalLength = 20;

            // bounding box override
            model1.BoundingBox.OverrideSceneExtents = true;
            model1.BoundingBox.Min = new Point3D(-200, -200, -100);
            model1.BoundingBox.Max = new Point3D(+200, +200, +100);

            // sets shadows and lights
            model1.Rendered.ShadowMode = shadowType.Realistic;
            model1.Rendered.RealisticShadowQuality = realisticShadowQualityType.High;
            
            model1.Light2.Active = false;
            model1.Light3.Active = false;
            model1.Light4.Active = false;
            model1.Light5.Active = false;
            model1.Light6.Active = false;
            model1.Light7.Active = false;
            model1.Light8.Active = false;

            model1.Light1.Type = lightType.Directional;
            model1.Light1.Stationary = false;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            Block rocketBlock = CreateRocketBlock();
            Block forestBlock = CreateForestTreeBlock();

            model1.Blocks.Add(rocketBlock);
            model1.Blocks.Add(forestBlock);

            // creates rocket block reference
            BlockReference rocketBlockReference = new BlockReference(0, 0, 0, "Rocket", 1, 1, 1, 0);
            model1.Entities.Add(rocketBlockReference);

            // creates 3 targets
            model1.Entities.Add(_target1.CreateTarget(60, 70), Color.Red);
            model1.Entities.Add(_target2.CreateTarget(140, 20), Color.Red);
            model1.Entities.Add(_target3.CreateTarget(80, -70), Color.Red);

            // creates 3 hit regions that cover the targets when the rocket hit them
            model1.Entities.Add(_target1.CreateHitRegion(60, 70), Color.Red);
            model1.Entities.Add(_target2.CreateHitRegion(140, 20), Color.Red);
            model1.Entities.Add(_target3.CreateHitRegion(80, -70), Color.Red);

            // sets visibility of hit regions, initially false. 
            model1.Entities[4].Visible = false;
            model1.Entities[5].Visible = false;
            model1.Entities[6].Visible = false;

            // creates the forest block
            BlockReference forestBlockReference = new BlockReference(0, 0, 0, "Forest", 1, 1, 1, 0);
            model1.Entities.Add(forestBlockReference);

            // create the ground
            CreateGround();

            // initializes WPF graphic elements 
            resetButton.IsEnabled = false;

            _initialSlidersValue[0] = (int)launchAngleSlider.Value;
            _initialSlidersValue[1] = (int)directionAngleSlider.Value;
            _initialSlidersValue[2] = (int)firePowerSlider.Value;

            launchAngleNumLabel.Content = launchAngleSlider.Value.ToString();
            directionAngleNumLabel.Content = directionAngleSlider.Value.ToString();
            firePowerNumLabel.Content = firePowerSlider.Value.ToString();

            // sets view
            model1.SetView(viewType.Trimetric, true, false);

            model1.ZoomFit();
            model1.Invalidate();

            base.OnContentRendered(e);
        }

        private Block CreateTreeBlock()
        {
            Block block = new Block("Tree");

            Mesh trunk = Mesh.CreateCylinder(3, 8, 30);
            trunk.Translate(0, 0, -3);
            trunk.Color = Color.Brown;
            trunk.ColorMethod = colorMethodType.byEntity;

            IList<Point3D> points = new List<Point3D>();
            points.Add(new Point3D(0, 0, 5));
            points.Add(new Point3D(0, 0, 41));
            points.Add(new Point3D(-8, 0, 29));
            points.Add(new Point3D(-3, 0, 29));
            points.Add(new Point3D(-13, 0, 17));
            points.Add(new Point3D(-3, 0, 17));
            points.Add(new Point3D(-18, 0, 5));
            points.Add(new Point3D(0, 0, 5));

            Mesh leaves = Mesh.CreatePlanar(points, Mesh.natureType.Smooth); //meshNatureType.Smooth);
            leaves.Color = Color.Green;
            leaves.ColorMethod = colorMethodType.byEntity;

            Mesh leaves1 = (Mesh)leaves.Clone();
            leaves1.Rotate((2 * Math.PI) / 3, Vector3D.AxisZ);
            leaves1.Color = Color.Green;

            Mesh leaves2 = (Mesh)leaves.Clone();
            leaves2.Rotate((4 * Math.PI) / 3, Vector3D.AxisZ);
            leaves2.Color = Color.Green;

            block.Entities.Add(trunk);
            block.Entities.Add(leaves);
            block.Entities.Add(leaves1);
            block.Entities.Add(leaves2);

            return block;
        }

        private Block CreateForestTreeBlock()
        {
            Block block = new Block("Forest");

            Block treeBlock = CreateTreeBlock();

            model1.Blocks.Add(treeBlock);

            BlockReference treeBlockReference = new BlockReference(-75, 50, 0, "Tree", 0.5, 0.5, 0.5, 0);
            block.Entities.Add(treeBlockReference);

            BlockReference treeBlockReference1 = new BlockReference(-70, 45, 0, "Tree", 1, 1, 1, 0);
            block.Entities.Add(treeBlockReference1);

            BlockReference treeBlockReference2 = new BlockReference(-68, 60, 0, "Tree", 0.7, 0.7, 0.7, 0);
            block.Entities.Add(treeBlockReference2);

            BlockReference treeBlockReference3 = new BlockReference(150, -50, 0, "Tree", 0.5, 0.5, 0.5, 0);
            block.Entities.Add(treeBlockReference3);

            BlockReference treeBlockReference4 = new BlockReference(160, -45, 0, "Tree", 0.7, 0.7, 0.7, 0);
            block.Entities.Add(treeBlockReference4);

            BlockReference treeBlockReference5 = new BlockReference(155, -70, 0, "Tree", 0.5, 0.5, 0.5, 0);
            block.Entities.Add(treeBlockReference5);

            BlockReference treeBlockReference6 = new BlockReference(-70, -55, 0, "Tree", 1, 1, 1, 0);
            block.Entities.Add(treeBlockReference6);

            BlockReference treeBlockReference7 = new BlockReference(-75, -80, 0, "Tree", 0.7, 0.7, 0.7, 0);
            block.Entities.Add(treeBlockReference7);

            BlockReference treeBlockReference8 = new BlockReference(110, 85, 0, "Tree", 1, 1, 1, 0);
            block.Entities.Add(treeBlockReference8);

            BlockReference treeBlockReference9 = new BlockReference(120, 70, 0, "Tree", 0.7, 0.7, 0.7, 0);
            block.Entities.Add(treeBlockReference9);

            BlockReference treeBlockReference10 = new BlockReference(180, 75, 0, "Tree", 1, 1, 1, 0);
            block.Entities.Add(treeBlockReference10);

            BlockReference treeBlockReference11 = new BlockReference(150, 65, 0, "Tree", 1, 1, 1, 0);
            block.Entities.Add(treeBlockReference11);

            return block;
        }

        public Block CreateRocketBlock()
        {
            Block rocketBlock = new Block("Rocket");

            // missile bottom
            LinearPath lpMissileBottom = new LinearPath(
                new Point3D(0, 0, 1), new Point3D(1.5, 0, 1), new Point3D(1.5, 0, 1.5),
                new Point3D(0, 0, 1.5), new Point3D(0, 0, 1));

            Mesh missileBottom = lpMissileBottom.RevolveAsMesh(0, 2 * Math.PI, new Vector3D(0, 0, 1), new Point3D(0, 0, 1), 20, 0.1, Mesh.natureType.Smooth);
            missileBottom.ColorMethod = colorMethodType.byEntity;
            missileBottom.Color = Color.Red;
            missileBottom.Weld();
            rocketBlock.Entities.Add(missileBottom);

            // missile body
            LinearPath lpMissileBody = new LinearPath(
                new Point3D(0, 0, 1.5), new Point3D(1.5, 0, 1.5),
                new Point3D(1.95, 0, 3.5), new Point3D(2.15, 0, 4.25),
                new Point3D(2.25, 0, 5.75), new Point3D(2.25, 0, 7.25),
                new Point3D(2.15, 0, 8.75), new Point3D(1.95, 0, 10.25),
                new Point3D(1.75, 0, 11), new Point3D(1.15, 0, 13),
                new Point3D(0, 0, 13), new Point3D(0, 0, 0)
                );

            Mesh missileBody = lpMissileBody.RevolveAsMesh(0, 2 * Math.PI, new Vector3D(0, 0, 1), new Point3D(0, 0, 1.5), 20, 0.1, Mesh.natureType.Smooth);
            missileBody.ColorMethod = colorMethodType.byEntity;
            missileBody.Color = Color.White;
            missileBody.Weld();
            rocketBlock.Entities.Add(missileBody);

            // missile edge
            LinearPath lpMissileEdge = new LinearPath(
               new Point3D(0, 0, 13), new Point3D(1.15, 0, 13),
               new Point3D(0.85, 0, 13.5), new Point3D(0.65, 0, 13.75),
               new Point3D(0.45, 0, 13.85), new Point3D(0.25, 0, 13.92),
               new Point3D(0.05, 0, 13.992),
               new Point3D(0, 0, 14), new Point3D(0, 0, 14)
               );

            Mesh missileEdge = lpMissileEdge.RevolveAsMesh(0, 2 * Math.PI, new Vector3D(0, 0, 1), new Point3D(0, 0, 11), 50, 0.1, Mesh.natureType.Smooth);
            missileEdge.ColorMethod = colorMethodType.byEntity;
            missileEdge.Color = Color.Red;
            missileEdge.Weld();
            rocketBlock.Entities.Add(missileEdge);

            // missile wings
            LinearPath lpMissileWing = new LinearPath(
                new Point3D(2.15, 0, 4.25), new Point3D(4, 0, 2),
                new Point3D(4, 0, 0), new Point3D(1.5, 0, 1.5),
                new Point3D(1.95, 0, 3.5), new Point3D(2.15, 0, 4.25)
                );

            devDept.Eyeshot.Entities.Region regionWing = new devDept.Eyeshot.Entities.Region(lpMissileWing, Plane.XZ);
            Mesh missileWing1 = regionWing.ExtrudeAsMesh(0.15, 0.1, Mesh.natureType.Plain);
            missileWing1.ColorMethod = colorMethodType.byEntity;
            missileWing1.Color = Color.Red;
            missileWing1.Weld();
            rocketBlock.Entities.Add(missileWing1);

            Mesh missileWing2 = (Mesh)missileWing1.Clone();
            missileWing2.Rotate(Math.PI / 2, Vector3D.AxisZ);
            rocketBlock.Entities.Add(missileWing2);

            Mesh missileWing3 = (Mesh)missileWing2.Clone();
            missileWing3.Rotate(Math.PI / 2, Vector3D.AxisZ);
            rocketBlock.Entities.Add(missileWing3);

            Mesh missileWing4 = (Mesh)missileWing3.Clone();
            missileWing4.Rotate(Math.PI / 2, Vector3D.AxisZ);
            rocketBlock.Entities.Add(missileWing4);

            return rocketBlock;
        }

        private void CreateGround()
        {
            const int rows = 5;
            const int cols = 5;

            PointRGB[] vertices = new PointRGB[rows * cols];

            double surfaceOffset = -3;

            Random random = new Random();
            double maxHeightSurface = 3;
            double minHeightSurface = -3;

            int indexArray = 0;
            for (int j = 0; j < rows; j++)
                for (int i = 0; i < cols; i++)
                {
                    // values 87.5 and 50 for dividing in 4 parts the grid(350x200)
                    double x = i * 87.5 - 150;
                    double y = j * 50 - 100;

                    double z = random.NextDouble() * (maxHeightSurface - minHeightSurface) + minHeightSurface;

                    // sets saddlebrown color 
                    int red = 139;
                    int green = 69;
                    int blue = 19;

                    if (x == -62.5 && y == 0 || x == 25 && y == 0)
                        z = -surfaceOffset;

                    if ((i % 2 == 0) && (j % 2 == 0))
                    {
                        // sets greenforest color
                        red = 34;
                        green = 139;
                        blue = 34;
                    }

                    vertices[indexArray++] = new PointRGB(x, y, z, (byte)red, (byte)green, (byte)blue);
                }

            IndexTriangle[] triangles = new IndexTriangle[((rows - 1) * (cols - 1) * 2)];

            indexArray = 0;
            for (int j = 0; j < (rows - 1); j++)
                for (int i = 0; i < (cols - 1); i++)
                {
                    triangles[indexArray++] = (new IndexTriangle(i + j * cols, i + j * cols + 1, i + (j + 1) * cols + 1));
                    triangles[indexArray++] = (new IndexTriangle(i + j * cols, i + (j + 1) * cols + 1, i + (j + 1) * cols));
                }

            Mesh surface = new Mesh();
            surface.NormalAveragingMode = Mesh.normalAveragingType.Averaged;

            surface.Vertices = vertices;
            surface.Triangles = triangles;

            // sets surface lower than the grid
            surface.Translate(0, 0, surfaceOffset);

            model1.Entities.Add(surface);

            // fits the model in the model1
            model1.ZoomFit();
        }

        // utility functions
        private void ChangeStateLaunchButtons(bool status)
        {
            resetButton.IsEnabled = !status;
            fireButton.IsEnabled = status;
            firePowerSlider.IsEnabled = status;
            directionAngleSlider.IsEnabled = status;
            launchAngleSlider.IsEnabled = status;
        }

        private void ResetSlidersLabelsValue()
        {
            // resets Sliders values
            launchAngleSlider.Value = _initialSlidersValue[0];
            directionAngleSlider.Value = _initialSlidersValue[1];
            firePowerSlider.Value = _initialSlidersValue[2];

            // resets number labels values
            launchAngleNumLabel.Content = launchAngleSlider.Value.ToString();
            directionAngleNumLabel.Content = directionAngleSlider.Value.ToString();
            firePowerNumLabel.Content = firePowerSlider.Value.ToString();
        }

        // resets the scene after resetButton has been clicked
        private void ResetScene()
        {
            Entity rocket = model1.Entities[0];

            // resets rocket direction angle at initial value
            rocket.Rotate(-_lastDirectionAngleSlider_Value, Vector3D.AxisZ);

            // resets rocket launch angle at initial value
            rocket.Rotate(-_lastLaunchAngleSlider_Value, new Vector3D(-Math.Sin(Utility.DegToRad(directionAngleSlider.Value)), Math.Cos(Utility.DegToRad(directionAngleSlider.Value)), 0));

            // makes the rocket visible again in the scene
            rocket.Visible = true;
            _lastDirectionAngleSlider_Value = 0;
            _lastLaunchAngleSlider_Value = 0;

            // makes the targets invisible again in the scene
            model1.Entities[4].Visible = false;
            model1.Entities[5].Visible = false;
            model1.Entities[6].Visible = false;
        }

        // slider that controls the angle for launching the rocket
        private void LaunchAngleSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (model1 == null) return;

            Entity rocket = model1.Entities[0];
            rocket.Rotate(Utility.DegToRad(90 - launchAngleSlider.Value) - _lastLaunchAngleSlider_Value, new Vector3D(-Math.Sin(Utility.DegToRad(directionAngleSlider.Value)), Math.Cos(Utility.DegToRad(directionAngleSlider.Value)), 0));
            model1.Entities.Regen();
            model1.Invalidate();
            _lastLaunchAngleSlider_Value = Utility.DegToRad(90 - launchAngleSlider.Value);
            launchAngleNumLabel.Content = launchAngleSlider.Value.ToString();
        }

        // slider that controls the direction for launching the rocket
        private void DirectionAngleSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (model1 == null) return;
            Entity rocket = model1.Entities[0];
            rocket.Rotate(Utility.DegToRad(directionAngleSlider.Value) - _lastDirectionAngleSlider_Value, Vector3D.AxisZ);
            model1.Entities.Regen();
            model1.Invalidate();
            _lastDirectionAngleSlider_Value = Utility.DegToRad(directionAngleSlider.Value);
            directionAngleNumLabel.Content = directionAngleSlider.Value.ToString();
        }

        // slider that controls the fire power for launching the rocket
        private void FirePowerSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (model1 == null) return;
            firePowerNumLabel.Content = firePowerSlider.Value.ToString();
        }

        // this function fires the rocket
        private void FireButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
                        
            if (model1 == null) return;

            BlockReference rocketBlockReference = (BlockReference)model1.Entities[0];
            rocketBlockReference.Visible = false;

            Parabolating trajectory = new Parabolating("Rocket", model1, Utility.DegToRad(launchAngleSlider.Value), Utility.DegToRad(directionAngleSlider.Value), firePowerSlider.Value);

            trajectory.vp.targets.Add(_target1);
            trajectory.vp.targets.Add(_target2);
            trajectory.vp.targets.Add(_target3);

            model1.Entities.Add(trajectory);

            if (model1.Entities.Count > _numEntityInScene)
                model1.Entities.RemoveAt(_numEntityInScene - 1);

            model1.StartAnimation(50);

            model1.Entities.Regen();
            model1.Invalidate();

            ChangeStateLaunchButtons(false);
        }

        // this function deletes the rocket already thrown and resets the rocket's position
        private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (model1.Entities.Count > _numEntityInScene - 1)
                model1.Entities.RemoveAt(_numEntityInScene - 1);

            ResetSlidersLabelsValue();
            ChangeStateLaunchButtons(true);

            ResetScene();

            model1.Entities.Regen();
            model1.Invalidate();
        }
    }

    #region Animation helper classes

    class Parabolating : BlockReference
    {
        public Point3D tip;

        double alpha;
        public double xPos, yPos, zPos;
        double theta, phi, v;
        double a = -0.5;
        double time;
        public MySingleModel vp;
        public int timeHitsGround;

        public Parabolating(string blockName, MySingleModel vp, double theta, double phi, double v)
            : base(0, 0, 0, blockName, 1, 1, 1, 0)
        {
            this.vp = vp;
            this.theta = theta;
            this.phi = phi;
            this.v = v;
            timeHitsGround = TimeRocketHitsGround(a, v, theta, 14); // rocket's height = 14 --> look CreateRocketBlock()
        }

        // this function calculates when the tip hits the ground. precision is limited by the fact frame number is an integer and not a double.
        private int TimeRocketHitsGround(double a, double v, double theta, double c)
        {
            double b = v * Math.Sin(theta);
            double delta = b * b - 4 * a * c;
            double result1 = (-b + Math.Sqrt(delta)) / (2 * a);
            double result2 = (-b - Math.Sqrt(delta)) / (2 * a);
            if (result1 >= 0)
                return (int)result1;
            else
                return (int)result2;
        }

        protected override void Animate(int frameNumber)
        {
            time = frameNumber;

            xPos = v * Math.Cos(theta) * Math.Cos(phi) * time;
            yPos = v * Math.Cos(theta) * Math.Sin(phi) * time;
            zPos = v * Math.Sin(theta) * time + a * time * time;

            vp.xPos = xPos;
            vp.yPos = yPos;
            vp.zPos = zPos;

            double vXY = v * Math.Cos(theta);
            double vZ = v * Math.Sin(theta) + a * time * 2;

            alpha = -Utility.RadToDeg(Math.Atan(vZ / vXY));

            tip = new Point3D(0, 0, 14); // rocket tip's location

            Translation t1 = new Translation(xPos, yPos, zPos);
            devDept.Geometry.Rotation t2 = new devDept.Geometry.Rotation(Utility.DegToRad(alpha + 90), new Vector3D(-Math.Sin(phi), Math.Cos(phi), 0));

            tip.TransformBy(t1 * t2);
        }

        public override void MoveTo(DrawParams data)
        {
            base.MoveTo(data);

            data.RenderContext.TranslateMatrixModelView(xPos, yPos, zPos);
            data.RenderContext.RotateMatrixModelView(alpha + 90, -Math.Sin(phi), Math.Cos(phi), 0);
        }

        // makes rocket always visible. Actual rocket position may go out of frustum.
        public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
        {
            return true;
        }
    }

    #endregion

    public class MySingleModel : Model
    {

        public double xPos, yPos, zPos;
        public List<Target> targets = new List<Target>();
        int time;

        protected override void OnAnimationTimerTick(object stateInfo)
        {
            time++;
            base.OnAnimationTimerTick(stateInfo);

            BlockReference rocketLaunched = (BlockReference)Entities[9];

            Parabolating parabolating = (Parabolating)rocketLaunched;

            if (parabolating != null && (parabolating.timeHitsGround - 2) == time)
            {
                StopAnimation();
                time = 0;

                rocketLaunched.Visible = true;
                
                Target[] targetsArray = targets.ToArray();

                // checks if the rocket hits one of the targets
                for (int i = 0; i < 3; i++)
                {
                    if (xPos > (targetsArray[i].xTarget - 7) && xPos < (targetsArray[i].xTarget + 5) && yPos > (targetsArray[i].yTarget - 7) && yPos < (targetsArray[i].yTarget + 5))
                    {
                        Entities[i + 4].Visible = true;
                        Dispatcher.Invoke(() => { Invalidate();});
                        break;
                    }
                }
            }
        }
    }


    public class Target
    {
        public double xTarget, yTarget;
        private LinearPath lpTarget;
        public Target()
        {

        }

        public Entity CreateTarget(double xTarget, double yTarget)
        {
            this.xTarget = xTarget;
            this.yTarget = yTarget;

            lpTarget = new LinearPath(new Point3D(3, 3, 0), new Point3D(3, 8, 0), new Point3D(-3, 8, 0), new Point3D(-3, 3, 0),
                    new Point3D(-8, 3, 0), new Point3D(-8, -3, 0), new Point3D(-3, -3, 0), new Point3D(-3, -8, 0), new Point3D(3, -8, 0),
                    new Point3D(3, -3, 0), new Point3D(8, -3, 0), new Point3D(8, 3, 0), new Point3D(3, 3, 0));

            lpTarget.Rotate(-Math.PI / 4, Vector3D.AxisZ);
            lpTarget.Translate(xTarget, yTarget, 0);

            return lpTarget;
        }

        public Entity CreateHitRegion(double xTarget, double yTarget)
        {
            this.xTarget = xTarget;
            this.yTarget = yTarget;

            devDept.Eyeshot.Entities.Region hitRegion = new devDept.Eyeshot.Entities.Region(lpTarget, Plane.XY);
            return hitRegion;
        }
    }
}