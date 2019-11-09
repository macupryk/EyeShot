using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        LinearPath[] inners;
        Entity[] shapes;

        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                    
            comboBoxAnimation.SelectedIndex = 0;
                        
            model1.ZoomFit();

            model1.Invalidate();

            base.OnContentRendered(e);
        }

        private void buttonHexagon_Click(object sender, RoutedEventArgs e)
        {
            // if there is an active animation don't do anything
            if (model1.AnimationFrameNumber > 0)
                return;

            // animate one shape at a time            
            if (!animating)
            {
                MoveShapes(0);
                buttonHexagon.IsEnabled = false;
            }
        }

        private void buttonTriangle_Click(object sender, RoutedEventArgs e)
        {
            // if there is an active animation don't do anything
            if (model1.AnimationFrameNumber > 0)
                return;

            // animate one shape at a time
            if (!animating)
            {
                MoveShapes(1);
                buttonTriangle.IsEnabled = false;
            }
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void comboBoxAnimation_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {

            // Clear the viewport and build the shapes
            model1.Entities.Clear();
            model1.Materials.Clear();
            model1.Blocks.Clear();

            BuildShapesAndTransformations();

            // Enable buttons
            buttonTriangle.IsEnabled = true;
            buttonHexagon.IsEnabled = true;

            if (animating)
            {
                animating = false;
                myTimer.Stop();
            }

            model1.Invalidate();
        }

        private void BuildShapesAndTransformations()
        {
            inners = new LinearPath[2];

            startOrientation = new Quaternion[2];
            finalOrientation = new Quaternion[2];

            // Hexagon
            CompositeCurve c = CompositeCurve.CreateHexagon(5);
            c.Regen(0);
            inners[0] = new LinearPath(c.Vertices);
            inners[0].Reverse();

            startOrientation[0] = new Quaternion(Vector3D.AxisZ, 180 / 2.0);
            Transformation transf = new Translation(7, 0, 0) * new Rotation(Math.PI / 2, Vector3D.AxisZ);
            inners[0].TransformBy(transf);

            // Triangle
            inners[1] = new LinearPath(new Point3D[]
                                             {
                                                 new Point3D(0, 0, 0),
                                                 new Point3D(7, 0, 0),
                                                 new Point3D(3.5, 7, 0),
                                                 new Point3D(0, 0, 0)
                                             });

            inners[1].Reverse();
            transf = new Translation(23, 0, 0) * new Rotation(Math.PI / 3, Vector3D.AxisZ);
            startOrientation[1] = new Quaternion(Vector3D.AxisZ, 180 / 3.0);
            inners[1].TransformBy(transf);

            // Extrude the 2 inner profiles to build 2 shapes
            shapes = new Entity[2];

            devDept.Eyeshot.Entities.Region firstInnerReg = new devDept.Eyeshot.Entities.Region(inners[0], Plane.XY, false);

            shapes[0] = firstInnerReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain);
            shapes[0].ColorMethod = colorMethodType.byEntity;
            shapes[0].Color = System.Drawing.Color.Green;

            devDept.Eyeshot.Entities.Region secondInnerReg = new devDept.Eyeshot.Entities.Region(inners[1], Plane.XY, false);

            shapes[1] = secondInnerReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain);
            shapes[1].ColorMethod = colorMethodType.byEntity;
            shapes[1].Color = System.Drawing.Color.Gainsboro;


            // Save the original shapes for the animation
            originalShapes = new Entity[] { (Entity)shapes[0].Clone(), (Mesh)shapes[1].Clone() };

            LinearPath outer = new LinearPath(new Point3D[] {new Point3D(0, -10, 0),
                                                    new Point3D(30, -10, 0),
                                                    new Point3D(30, 10, 0),
                                                    new Point3D(0, 10, 0),
                                                    new Point3D(0, -10, 0)});

            devDept.Eyeshot.Entities.Region plate = new devDept.Eyeshot.Entities.Region(new ICurve[] { outer, inners[0], inners[1] }, Plane.XY, false);

            // Build a mesh with 2 holes
            Mesh m = plate.ExtrudeAsMesh(3, 0.1, Mesh.natureType.Plain);

            // Transform the mesh and the the 2 inner profiles, to position them in the exact place of the holes
            transf = new Translation(0, 3, 10) * new Rotation(Math.PI / 2, Vector3D.AxisX);
            m.TransformBy(transf);
            model1.Entities.Add(m, System.Drawing.Color.Brown);

            inners[0].TransformBy(transf);
            inners[1].TransformBy(transf);

            // Rotation quaternion of the 2 inners
            Quaternion q = new Quaternion(Vector3D.AxisX, 90);
            finalOrientation[0] = q * startOrientation[0];
            finalOrientation[1] = q * startOrientation[1];

            // Define a Transformation for the 2 shapes, and store the rotation Quaternion
            transf = new Translation(20, -25, 0) * new Rotation(Math.PI / 9, Vector3D.AxisZ);
            startOrientation[0] = new Quaternion(Vector3D.AxisZ, 180 / 9.0) * startOrientation[0];

            shapesTransform = new Transformation[2];
            shapesTransform[0] = transf;

            transf = new Translation(-10, -44, 0) * new Rotation(Math.PI / 5, Vector3D.AxisZ);
            shapesTransform[1] = transf;
            startOrientation[1] = new Quaternion(Vector3D.AxisZ, 180 / 5.0) * startOrientation[1];

            if (comboBoxAnimation.SelectedIndex == 1)
            {
                // Block Reference Animation

                // Add the Blocks to the viewport and create the BlockReferences for the animation
                AddBlockDefinition(new Block("B1"), 0);
                AddBlockDefinition(new Block("B2"), 1);
            }
            else
            {
                // Transform the shapes
                shapes[0].TransformBy(shapesTransform[0]);
                shapes[1].TransformBy(shapesTransform[1]);
            }

            // Add the entities to the viewport
            model1.Entities.Add(shapes[0]);
            model1.Entities.Add(shapes[1]);
        }

        private void MoveShapes(int index)
        {
            switch (comboBoxAnimation.SelectedIndex)
            {
                case 0:
                    Transformation(index);
                    break;

                case 1:
                    Animation(index);
                    break;

                case 2:
                    Direct(index);
                    break;
            }
        }

        #region Transformation

        private Entity[] originalShapes;
        private Transformation[] shapesTransform;
        private Quaternion[] startOrientation, finalOrientation;
        Vector3D stepTranslation;
        Vector3D rotationAxis;
        double stepAngle;
        private int shapeIndex;

        // Animation steps and time
        int animationSteps = 40;
        int animationtime = 1000;
        private DispatcherTimer myTimer;

        bool animating;

        private void Transformation(int index)
        {
            Vector3D axis;
            double angle;

            // Compute the quaternion necessary to rotate from the start orientation to the final orientation
            startOrientation[index].ToAxisAngle(out axis, out angle);
            axis.Negate();
            Quaternion inverseQuat = new Quaternion(axis, angle);
            Quaternion q = finalOrientation[index] * inverseQuat;

            q.ToAxisAngle(out rotationAxis, out stepAngle);

            // Angle of rotation for each animation frame
            stepAngle /= animationSteps;

            // Index of the shape to animate
            shapeIndex = index;

            // Compute the translation factor for each frame of animation, from the initial position to the final position
            Vector3D translationVect = Vector3D.Subtract(inners[index].Vertices[0], shapes[index].Vertices[0]);
            stepTranslation = translationVect / animationSteps;

            animationFrame = 0;
            animating = true;

            // Start a timer for the animation
            myTimer = new DispatcherTimer();            
            myTimer.Interval = new TimeSpan(animationtime / animationSteps);
            myTimer.Tick += TransformShape;
            myTimer.Start();
        }

        private int animationFrame = 0;

        private void TransformShape(object sender, EventArgs e)
        {
            animationFrame++;

            // remove the old shape from the viewport
            model1.Entities.Remove(shapes[shapeIndex]);

            // work on the original cloned shape
            Mesh m = (Mesh)originalShapes[shapeIndex].Clone();

            // translate the shape to the origin
            Point3D firstPt = (Point3D)m.Vertices[0].Clone();
            Transformation t = new Translation(-firstPt.X, -firstPt.Y, -firstPt.Z);

            // rotate it by an angle proportional to the frame number
            t = new Rotation(Utility.DegToRad(animationFrame * stepAngle), rotationAxis) * t;

            // translate it back to the original position
            t = new Translation(firstPt.X, firstPt.Y, firstPt.Z) * t;

            // apply the transformations to the shape
            t = shapesTransform[shapeIndex] * t;

            // translate it towards the final position by an amount proportional to the frame number
            t = new Translation(stepTranslation * animationFrame) * t;

            m.TransformBy(t);

            shapes[shapeIndex] = m;

            // add the new shape to the viewport
            model1.Entities.Add(m);
            model1.Invalidate();

            if (animationFrame == animationSteps)
            {
                myTimer.Stop();
                animating = false;
            }

        }
        #endregion

        #region Animation

        private void Animation(int index)
        {

            model1.StartAnimation(1, animationSteps);
            AnimatedBlockRef br = (AnimatedBlockRef)shapes[index];
            br.animate = true;
            br.deltaT = 1.0 / animationSteps;

        }

        private void AddBlockDefinition(Block block, int index)
        {
            block.Entities.Add(shapes[index]);
            model1.Blocks.Add(block);

            // Create a blockReference
            AnimatedBlockRef br = new AnimatedBlockRef(shapesTransform[index], block.Name);

            // Store the orientation quaternions
            br.startOrientation = startOrientation[index];
            br.finalOrientation = finalOrientation[index];

            // Store the translation vector between initial and final positions
            br.translationVect = Vector3D.Subtract(inners[index].Vertices[0], br.Transformation * block.Entities[0].Vertices[0]);

            // Store the first point (for correct rotation around origin)
            br.firstPt = (Point3D)block.Entities[0].Vertices[0].Clone();

            // Initialize the data for animation
            br.Init();

            // Put the BlockReference in the shapes array
            shapes[index] = br;
        }
        #endregion

        #region Direct
        private void Direct(int index)
        {

            // Transform the shape for the initial position to the final position in one shot
            Plane initialPlane = new Plane(shapes[index].Vertices[0], shapes[index].Vertices[1], shapes[index].Vertices[2]);
            Plane finalPlane = new Plane(inners[index].Vertices[0], inners[index].Vertices[1], inners[index].Vertices[2]);

            Transformation t = new Transformation();
            t.Rotation(initialPlane, finalPlane);

            shapes[index].TransformBy(t);

            model1.Entities.Regen();
            model1.Invalidate();

        }
        #endregion
  
    }
}