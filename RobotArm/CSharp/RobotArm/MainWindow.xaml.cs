using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using System.IO;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Color = System.Drawing.Color;
using devDept.Geometry;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;

namespace RobotArm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BlockReference r1, r2, r3, r4, r5, r6;        
        private static CollisionDetection cd = null;
        private CollisionDetection.collisionCheckType _checkMethod = CollisionDetection.collisionCheckType.OB;
        private bool firstOnly = false;
        private double degreeAngle = 9;
        private string selectedPart = "A1";
        private double previousValue = 0;

        private const string FileName =
#if NURBS
            "../../../../../../dataset/Assets/RobotArm.eye";
#else
            "../../../../../../dataset/Assets/RobotArm_PRO.eye";
#endif

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

        }

        protected override void OnContentRendered(EventArgs e)
        {
            // Builds robot arm model and adds it to the scene
            AddRobotArm();
            // Builds a generic model to test the collision detection and adds it to the scene
            AddCollisionModel();

            // Hides grid and origin symbol
            model1.GetOriginSymbol().Visible = false;
            model1.GetGrid().Visible = false;

            // Fits the model in the viewport
            model1.ZoomFit();

            // Refreshes the model control
            model1.Invalidate();

            base.OnContentRendered(e);
        }

        public void EnableButtons()
        {
            collisionButton.IsEnabled = true;
            collisionMethodCombo.IsEnabled = true;
            rotate1.IsEnabled = true;
            rotate2.IsEnabled = true;
            rotate3.IsEnabled = true;
            rotate4.IsEnabled = true;
            rotate5.IsEnabled = true;
            rotate6.IsEnabled = true;
            movePosButton.IsEnabled = true;
            moveNegButton.IsEnabled = true;
            rotateSlider.IsEnabled = true;
            firstCheckBox.IsEnabled = true;
        }

        public void DisableButtons()
        {
            collisionButton.IsEnabled = false;
            collisionMethodCombo.IsEnabled = false;
            rotate1.IsEnabled = false;
            rotate2.IsEnabled = false;
            rotate3.IsEnabled = false;
            rotate4.IsEnabled = false;
            rotate5.IsEnabled = false;
            rotate6.IsEnabled = false;
            firstCheckBox.IsEnabled = false;
        }

        private void AddCollisionModel()
        {
            double baseFoot = 50;
            double heightFoot = 300;
            double widthTable = 550;
            double depthTable = 325;
            double thickTable = 50;
            double radius = 10;
            double heightElem = 50;

#if NURBS
            // Foot table block
            Brep s = Brep.CreateBox(baseFoot, baseFoot, heightFoot);
            s.ColorMethod = colorMethodType.byParent;
            Block foot = new Block("foot");
            foot.Entities.Add(s);
            model1.Blocks.Add(foot);

            // Plane table block
            Brep s1 = Brep.CreateBox(widthTable, depthTable, thickTable);
            s1.ColorMethod = colorMethodType.byParent;
            Block plate = new Block("plate");
            plate.Entities.Add(s1);
            model1.Blocks.Add(plate);

            // Single elem block
            Brep s2 = Brep.CreateCylinder(radius, heightElem);
            s2.Translate(radius, radius, 0);
            s2.ColorMethod = colorMethodType.byParent;
            Block elem = new Block("elem");
            elem.Entities.Add(s2);
            model1.Blocks.Add(elem);

#else
            // Foot table block
            Mesh s = Mesh.CreateBox(baseFoot, baseFoot, heightFoot);
            s.ColorMethod = colorMethodType.byParent;
            Block foot = new Block("foot");
            foot.Entities.Add(s);
            model1.Blocks.Add(foot);

            // Plane table block
            Mesh s1 = Mesh.CreateBox(widthTable, depthTable, thickTable);
            s1.ColorMethod = colorMethodType.byParent;
            Block plate = new Block("plate");
            plate.Entities.Add(s1);
            model1.Blocks.Add(plate);

            // Single elem block
            Mesh s2 = Mesh.CreateCylinder(radius, heightElem, 20);
            s2.Translate(radius, radius, 0);
            s2.ColorMethod = colorMethodType.byParent;
            Block elem = new Block("elem");
            elem.Entities.Add(s2);
            model1.Blocks.Add(elem);
#endif
            // Table block
            Block table = new Block("table");
            table.Entities.Add(new BlockReference(0, 0, 0, "foot", 0) { ColorMethod = colorMethodType.byParent });
            table.Entities.Add(new BlockReference(widthTable - baseFoot, 0, 0, "foot", 0) { ColorMethod = colorMethodType.byParent });
            table.Entities.Add(new BlockReference(widthTable - baseFoot, depthTable - baseFoot, 0, "foot", 0) { ColorMethod = colorMethodType.byParent });
            table.Entities.Add(new BlockReference(0, depthTable - baseFoot, 0, "foot", 0) { ColorMethod = colorMethodType.byParent });
            table.Entities.Add(new BlockReference(0, 0, heightFoot, "plate", 0) { ColorMethod = colorMethodType.byParent });
            model1.Blocks.Add(table);

            // Elem's grid block
            Block grid = new Block("grid");
            double offset = 100;
            double offsetX = 25;
            double offsetY = 25;
            while (offsetX < widthTable - 5)
            {
                grid.Entities.Add(new BlockReference(offsetX, offsetY, 0, "elem", 0) { ColorMethod = colorMethodType.byParent });
                while (offsetY < depthTable - 5)
                {
                    grid.Entities.Add(new BlockReference(offsetX, offsetY, 0, "elem", 0) { ColorMethod = colorMethodType.byParent });
                    offsetY += radius * 2 + offset;
                }
                offsetY = 25;
                offsetX += radius * 2 + offset;
            }
            model1.Blocks.Add(grid);

            // Final container block
            Block container = new Block("container");

            BlockReference item = new BlockReference(0, 0, 0, "table", 0);
            item.ColorMethod = colorMethodType.byEntity;
            item.Color = Color.Gray;
            container.Entities.Add(item);

            BlockReference item2 = new BlockReference(0, 0, heightFoot + thickTable, "grid", 0);
            item2.ColorMethod = colorMethodType.byEntity;
            item2.Color = Color.Azure;
            container.Entities.Add(item2);

            // Adds container to the blocks collection
            model1.Blocks.Add(container);

            // Adds container reference to the scene
            model1.Entities.Add(new BlockReference(250, -125, 0, "container", 0), "default", Color.WhiteSmoke);
        }

        private void AddRobotArm()
        {
            Entity[] robotEntities = new Entity[7];
            Point3D AP1;
            Point3D AP2;
            Point3D AP3;
            Point3D AP4;
            Point3D AP5;
            Point3D AP6;

            AP1 = new Point3D(0, 0, 0);
            AP2 = new Point3D(25, 0, 400);
            AP3 = new Point3D(25, 0, 855);
            AP4 = new Point3D(25, 0, 890);
            AP5 = new Point3D(445, 0, 890);
            AP6 = new Point3D(525, 0, 890);

            model1.OpenFile(FileName);
            for (int i = 0; i < model1.Entities.Count; i++)
            {
                robotEntities[i] = model1.Entities[i];
            }
            model1.Entities.Clear();

            // Creates a dictionary to identify the robot arm part index from its name

            Dictionary<string, int> robotParts = new Dictionary<string, int>();

            string[] robotPartNames = new string[7];

            robotPartNames[0] = "E0";
            robotPartNames[1] = "E1";
            robotPartNames[2] = "E2";
            robotPartNames[3] = "E3";
            robotPartNames[4] = "E4";
            robotPartNames[5] = "E5";
            robotPartNames[6] = "E6";

            for (int i = 0; i < robotPartNames.Length; i++)
            {
                robotParts.Add(robotPartNames[i], i);
            }

            // Creates a BlockReference for each group of entities that represents a body part
            // and uses the EntityData property to store the rotation center.

            r6 = BuildBlockReference("P6", new Entity[] { robotEntities[robotParts["E6"]] });
            r6.EntityData = AP6;

            r5 = BuildBlockReference("P56", new Entity[] { robotEntities[robotParts["E5"]], r6 });
            r5.EntityData = AP5;

            r4 = BuildBlockReference("P456", new Entity[] { robotEntities[robotParts["E4"]], r5 });
            r4.EntityData = AP4;

            r3 = BuildBlockReference("P3456", new Entity[] { robotEntities[robotParts["E3"]], r4 });
            r3.EntityData = AP3;

            r2 = BuildBlockReference("P23456", new Entity[] { robotEntities[robotParts["E2"]], r3 });
            r2.EntityData = AP2;

            r1 = BuildBlockReference("P123456", new Entity[] { robotEntities[robotParts["E1"]], r2 });
            r1.EntityData = AP1;

            BlockReference brRobot = BuildBlockReference("Robot", new Entity[] { robotEntities[robotParts["E0"]], r1 });

            model1.Layers[0].Color = Color.SandyBrown;
            model1.Entities.Add(brRobot);
        }        

        private BlockReference BuildBlockReference(string newName, IList<Entity> entities)
        {
            // Creates a new BlockReference from the given list of entities
            Block bl = new Block(newName);
            bl.Entities.AddRange(entities);

            model1.Blocks.Add(bl);

            BlockReference br = new BlockReference(new Identity(), newName);
            br.ColorMethod = colorMethodType.byEntity;
            return br;
        }
        private void Model1OnWorkCompleted(object sender, WorkCompletedEventArgs workCompletedEventArgs)
        {
            // Clears previous selection
            model1.Entities.ClearSelection();

            if (cd.Result != null && cd.Result.Count > 0)
            {
                for (var i = 0; i < cd.Result.Count; i++)
                {
                    Tuple<CollisionDetection.CollisionResultItem, CollisionDetection.CollisionResultItem> tuple =
                        cd.Result[i];
                    // Selects the intersecting entities
                    tuple.Item1.Entity.SetSelection(true, tuple.Item1.Parents);
                    tuple.Item2.Entity.SetSelection(true, tuple.Item2.Parents);
                }
                this.intersectLabel.Content = "True";

                TimeSpan fromMillisecond = new TimeSpan(0, 0, 0, 0, (int)cd.ExecutionTime);
                timeLabel.Content = string.Format("{0:D2}m:{1:D2}s:{2:D3}ms", fromMillisecond.Minutes, fromMillisecond.Seconds,
                    fromMillisecond.Milliseconds);

                numLabel.Content = cd.Result.Count.ToString();
            }
            else
            {
                this.intersectLabel.Content = "False";

                TimeSpan fromMillisecond = new TimeSpan(0, 0, 0, 0, (int)cd.ExecutionTime);
                timeLabel.Content = string.Format("{0:D2}m:{1:D2}s:{2:D3}ms", fromMillisecond.Minutes, fromMillisecond.Seconds,
                    fromMillisecond.Milliseconds);

                numLabel.Content = "0";
            }

            // Refreshes the model control
            model1.Invalidate();
            EnableButtons();
        }

        private void SelectPartToRotate(object sender, EventArgs e)
        {
            selectedPart = ((RadioButton)sender).Content.ToString();
            previousValue = 0;
            rotateSlider.Value = 0;
        }

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            double value = ((Slider)sender).Value - previousValue;
            previousValue = ((Slider)sender).Value;
            RotateAxis(selectedPart, degreeAngle*value);

            // Refreshes the model control            
            model1.Invalidate();
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // If collision detection is enable, starts a collision check
            if (cd != null)
            {
                model1.StartWork(cd);
                DisableButtons();
            }            
        }

        private void RotateAxis(string Axis, double Degree)
        {
            double angleInRadians = Utility.DegToRad(Degree);
            switch (Axis)
            {
                case "A1":
                    r1.Rotate(angleInRadians, Vector3D.AxisZ, (Point3D)r1.EntityData);
                    break;

                case "A2":
                    r2.Rotate(angleInRadians, Vector3D.AxisY, (Point3D)r2.EntityData);
                    break;

                case "A3":
                    r3.Rotate(angleInRadians, Vector3D.AxisY, (Point3D)r3.EntityData);
                    break;

                case "A4":
                    r4.Rotate(angleInRadians, Vector3D.AxisX, (Point3D)r4.EntityData);
                    break;

                case "A5":
                    r5.Rotate(angleInRadians, Vector3D.AxisY, (Point3D)r5.EntityData);
                    break;

                case "A6":
                    r6.Rotate(angleInRadians, Vector3D.AxisX, (Point3D)r6.EntityData);
                    break;

                default:
                    break;
            }

            // Regenerates entities
            model1.Entities.Regen();
        }

        private void CollisionMethodCombo_OnSelectionChanged(object sender, EventArgs e)
        {
            // Changes the collision accuracy method
            switch (collisionMethodCombo.SelectedIndex)
            {
                case 0: //OBB
                    _checkMethod = CollisionDetection.collisionCheckType.OB;
                    break;
                case 1: //OBB with Octree
                    _checkMethod = CollisionDetection.collisionCheckType.OBWithSubdivisionTree;
                    break;
                case 2: //Octree
                    _checkMethod = CollisionDetection.collisionCheckType.SubdivisionTree;
                    break;
                case 3: //Geometric intersection
                    _checkMethod = CollisionDetection.collisionCheckType.Accurate;
                    break;
                default: //OBB
                    _checkMethod = CollisionDetection.collisionCheckType.OB;
                    break;
            }
            if (cd != null)
            {
                // Updates collision detection accuracy method and start a collision check
                cd.CheckMethod = _checkMethod;
                model1.StartWork(cd);
                DisableButtons();
            }
        }

        private void collisionButton_Click(object sender, EventArgs e)
        {
            if (collisionButton.IsChecked.GetValueOrDefault(false))
            {
                collisionButton.Content = "Disable Collision Detection";

                if(collisionMethodCombo.SelectedIndex == -1)
                    collisionMethodCombo.SelectedIndex = 0;

                // Sets a new istance of CollisionDetection
                cd = new CollisionDetection(model1.Entities, model1.Blocks, firstOnly, _checkMethod, maxTrianglesNumForOctreeNode: 5);

                // Starts the first collision check
                model1.StartWork(cd);
                DisableButtons();
            }
            else
            {
                collisionButton.Content = "Enable Collision Detection";

                // Removes all the objects defined for collision detection before to delete this instance
                cd.ClearCache();
                cd = null;
                model1.Entities.ClearSelection();
                model1.Invalidate();
            }
        }
        
        private void firstCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (cd != null)
            {
                // If geometryIntesection is true, then set the CollisionDetection.FirstOnly flag to false to be sure to find the first really colliding tuple of entities
                cd.FirstOnly = firstCheckBox.IsChecked.GetValueOrDefault(false);
                model1.StartWork(cd);
                DisableButtons();
            }
        }
        
        private void angleText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Changes the degree angle used to rotate the robot arm components
                degreeAngle = double.Parse(angleText.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Insert a valid degree angle.");
                degreeAngle = 9;
            }
        }

        private void movePosButton_Click(object sender, EventArgs e)
        {
            RotateAxis(selectedPart, degreeAngle);

            // If collision detection is enable starts a collision check
            if (cd != null)
            {
                model1.StartWork(cd);
                DisableButtons();
            }

            rotateSlider.Value++;

            // Refreshes the model control
            model1.Invalidate();
        }

        private void moveNegButton_Click(object sender, EventArgs e)
        {
            RotateAxis(selectedPart, -degreeAngle);

            // If collision detection is enable starts a collision check
            if (cd != null)
            {
                model1.StartWork(cd);
                DisableButtons();
            }

            rotateSlider.Value--;

            // Refreshes the model control
            model1.Invalidate();
        }
    }
}
