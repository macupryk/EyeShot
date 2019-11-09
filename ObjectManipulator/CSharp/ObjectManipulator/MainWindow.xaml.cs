using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using Block = devDept.Eyeshot.Block;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Assets = "../../../../../../dataset/Assets/";

        ToolTip toolTip1 = new ToolTip();
        private string toolTipText;

        private string textDragTranslateOnAxis = "Drag arrow to translate\n";
        private string textDragTranslateOnView = "Drag sphere to translate\n";
        private string textDragScale = "Drag box to scale\n";
        private string textDragScaleUniform = "Drag sphere to scale\n";
        private string textDragRotate = "Drag arc to rotate";
        private string textDragRotateOnView = "Drag sphere to rotate";
        private string textSelectEntity = "Click to select\nDouble click to set current BlockReference\nRight click to edit";
        private string textApplyOrCancel = "Double click to apply transformation\nRight click to cancel editing";
        private string textResetCurrent = "Click to deselect\nDouble click to reset current BlockReference";

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            // Sets default values
            styleEnumButton.Set(model1.ObjectManipulator.StyleMode);
            ballActionEnumButton.Set(model1.ObjectManipulator.BallActionMode);

            // Add an EventHandler to the ObjectManipulator.MouseOver event to show a ToolTip when the mouse is over a part of the ObjectManipulator.
            model1.ObjectManipulator.MouseOver += OnObjectManipulatorMouseOver;
            model1.ObjectManipulator.MouseDrag += HideToolTip;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            // Get the body parts
            String[] fileNames = new String[]
                                      {
                                          Assets + "figure_Object001.eye",
                                          Assets + "figure_Object002.eye",
                                          Assets + "figure_Object003.eye",
                                          Assets + "figure_Object004.eye",
                                          Assets + "figure_Object005.eye",
                                          Assets + "figure_Object006.eye",
                                          Assets + "figure_Object007.eye",
                                          Assets + "figure_Object008.eye",
                                          Assets + "figure_Object009.eye",
                                          Assets + "figure_Object010.eye",
                                          Assets + "figure_Object011.eye",
                                          Assets + "figure_Object012.eye",
                                          Assets + "figure_Object013.eye",
                                          Assets + "figure_Object014.eye",
                                          Assets + "figure_Object015.eye",
                                          Assets + "figure_Object016.eye",
                                          Assets + "figure_Object017.eye",
                                          Assets + "figure_Object018.eye",
                                          Assets + "figure_Object019.eye",
                                          Assets + "figure_Object020.eye",
                                          Assets + "figure_Object021.eye",
                                          Assets + "figure_Object022.eye",
                                          Assets + "figure_Object023.eye",
                                          Assets + "figure_Object024.eye"
                                      };

            string[] partNames = {
                "LeftFoot",
                "RightFoot",
                "LeftAnkle",
                "RightAnkle",
                "LeftLowerLeg",
                "RightLowerLeg",
                "LeftKnee",
                "RightKnee",
                "LeftUpperLeg",
                "RightUpperLeg",
                "Torso",
                "LeftShoulder",
                "LeftUpperArm",
                "LeftElbow",
                "LeftLowerArm",
                "LeftWrist",
                "LeftHand",
                "RightShoulder",
                "RightUpperArm",
                "RightElbow",
                "RightLowerArm",
                "RightWrist",
                "RightHand",
                "Head"
            };

            // Create a BlockReference for each body part
            Entity[] entities = new Entity[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)

                entities[i] = CreateBlockReference(partNames[i], fileNames[i]);

            // Creates a dictionary to identify the body part index from its name
            Dictionary<string, int> parts = new Dictionary<string, int>();
            
            for (int i = 0; i < partNames.Length; i++)

                parts.Add(partNames[i], i);

            // Creates BlockReferences for the various groups of entities that form the body parts
            // and set in the EntityData proerty of each one the point to be used as the ObjectManipulator origin

            BlockReference brTemp, brTemp2;

            SetRotationPoint(entities[parts["LeftWrist"]], (BlockReference)entities[parts["LeftHand"]]);
            SetRotationPoint(entities[parts["RightWrist"]], (BlockReference)entities[parts["RightHand"]]);
            SetRotationPoint(entities[parts["LeftAnkle"]], (BlockReference)entities[parts["LeftFoot"]]);
            SetRotationPoint(entities[parts["RightAnkle"]], (BlockReference)entities[parts["RightFoot"]]);

            //Left leg                
            brTemp = BuildBlockReference("BrLeftLowerLeg", new Entity[] {entities[parts["LeftFoot"]], 
                                                                         entities[parts["LeftAnkle"]], 
                                                                         entities[parts["LeftLowerLeg"]]});

            SetRotationPoint(entities[parts["LeftKnee"]], brTemp);

            brTemp2 = BuildBlockReference("BrLeftUpperLeg", new Entity[]
                                                 {
                                                     entities[parts["LeftUpperLeg"]],
                                                     entities[parts["LeftKnee"]]
                                                 });

            BlockReference br1 = BuildBlockReference("BrLeftLeg", new Entity[]
                                                 {
                                                     brTemp,
                                                     brTemp2
                                                 });
            br1.EntityData = new Point3D(3.8, 44, 1.6);

            // Right leg
            brTemp = BuildBlockReference("BrRightLowerLeg", new Entity[] {entities[parts["RightFoot"]], 
                                                                          entities[parts["RightAnkle"]], 
                                                                          entities[parts["RightLowerLeg"]]});

            SetRotationPoint(entities[parts["RightKnee"]], brTemp);

            brTemp2 = BuildBlockReference("BrRightUpperLeg", new Entity[]{
                                                            entities[parts["RightUpperLeg"]],
                                                            entities[parts["RightKnee"]]
                                                        });

            BlockReference br2 = BuildBlockReference("BrRightLeg", new Entity[]{
                                                            brTemp,
                                                            brTemp2
                                                        });

            br2.EntityData = new Point3D(-3.8, 44, 1.6);

            // Left arm
            brTemp = BuildBlockReference("BrLeftLowerArm", new Entity[] {entities[parts["LeftHand"]], 
                                                                         entities[parts["LeftWrist"]], 
                                                                         entities[parts["LeftLowerArm"]]});

            SetRotationPoint(entities[parts["LeftElbow"]], brTemp);

            BlockReference br3 = BuildBlockReference("BrLefArm", new Entity[]{
                                                            brTemp,
                                                            entities[parts["LeftElbow"]],
                                                            entities[parts["LeftUpperArm"]],
                                                            entities[parts["LeftShoulder"]]
                                                        });

            SetRotationPoint(entities[parts["LeftShoulder"]], br3);


            // Right arm
            brTemp = BuildBlockReference("BrRightLowerArm", new Entity[] {entities[parts["RightHand"]], 
                                                                entities[parts["RightWrist"]], 
                                                                entities[parts["RightLowerArm"]]});

            SetRotationPoint(entities[parts["RightElbow"]], brTemp);

            BlockReference br4 = BuildBlockReference("BrRightArm", new Entity[]
                                                                       {
                                                                           brTemp,
                                                                           entities[parts["RightElbow"]],
                                                                           entities[parts["RightUpperArm"]],
                                                                           entities[parts["RightShoulder"]]
                                                                       });

            SetRotationPoint(entities[parts["RightShoulder"]], br4);

            // Creates the final BlockReference containing the whole model
            BlockReference brBody = BuildBlockReference("BrMan", new Entity[]
                                                                     {
                                                                         br1, br2, br3, br4,
                                                                         entities[parts["Torso"]],
                                                                         entities[parts["Head"]]
                                                                     }
                );

            entities[parts["Head"]].EntityData = new Point3D(0, 63, .38); // set the rotation point

            brBody.Rotate(Math.PI / 2, Vector3D.AxisX);

            model1.Entities.Add(brBody, System.Drawing.Color.Pink); 
            
            Mesh baseBox = Mesh.CreateBox(20, 20, 10);
            baseBox.Translate(50, 50, 0);
            model1.Materials.Add(new Material("wood", new Bitmap(Assets + "Textures/Wenge.jpg")));
            baseBox.ApplyMaterial("wood", textureMappingType.Cubic, 1, 1);
            model1.Entities.Add(baseBox);   
            
            model1.ZoomFit();            

            model1.ActionMode = actionType.SelectVisibleByPick;
            model1.GetViewCubeIcon().Visible = false;            
            model1.Rendered.ShadowMode = shadowType.None;
            model1.Rendered.ShowEdges = false;

            // Hide the original part when editing it
            model1.ObjectManipulator.ShowOriginalWhileEditing = false;            
            model1.Invalidate();
         
            base.OnContentRendered(e);
        }

        private BlockReference CreateBlockReference(string partName, String fileName)
        {
            devDept.Eyeshot.Translators.ReadFile readFile = new devDept.Eyeshot.Translators.ReadFile(fileName);
            readFile.DoWork();

            Entity entity = readFile.Entities[0];

            ((Mesh)entity).NormalAveragingMode = Mesh.normalAveragingType.Averaged;
            entity.ColorMethod = colorMethodType.byParent;

            Block bl = new Block(partName, Point3D.Origin);

            bl.Entities.Add(entity);
            model1.Blocks.Add(bl);

            BlockReference br = new BlockReference(new Identity(), partName);
            br.ColorMethod = colorMethodType.byParent;
            return br;
        }

        private void SetRotationPoint(Entity entity, BlockReference brTemp)
        {
            // Saves the rotation point in the Entity data of the BlockReference

            BlockReference br = (BlockReference)entity;

            Point3D boxMin, boxMax;
            Utility.ComputeBoundingBox(null, model1.Blocks[br.BlockName].Entities[0].Vertices, out boxMin, out boxMax);

            brTemp.EntityData = (boxMin + boxMax) / 2;
        }

        private BlockReference BuildBlockReference(string newName, IList<Entity> entities)
        {
            // Creates a new BlockReference from the given list of entities
            Block bl = new Block(newName);
            bl.Entities.AddRange(entities);

            model1.Blocks.Add(bl);

            BlockReference br = new BlockReference(new Identity(), newName);
            br.ColorMethod = colorMethodType.byParent;
            return br;
        }
        
        private bool Editing = false;

        private void model1_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            if (model1.GetMouseClicks(e) == 1 && e.RightButton == MouseButtonState.Pressed)
                model1_Click();

            if (model1.GetMouseClicks(e) == 2 && e.LeftButton == MouseButtonState.Pressed)
                model1_DoubleClick();

        }
        
        private void model1_Click()
        {
            if (Editing)
            {
                // Cancels the ObjectManipulator editing
                model1.ObjectManipulator.Cancel();
                Editing = false;
            }
            else
            {
                // Starts the edit the selected parts with the ObjectManipulator
                int countSelected;
                Entity selectedEnt = GetSelectedEntity(out countSelected);

                if (countSelected == 1)
                {
                    Editing = true;

                    Transformation initialTransformation = null;
                    bool center = true;


                    // If there is only one selected entity, position and orient the manipulator using the rotation point saved in its
                    // EntityData property and its transformation

                    Point3D rotationPoint = null;

                    if (selectedEnt.EntityData is Point3D)
                    {
                        center = false;
                        rotationPoint = (Point3D)selectedEnt.EntityData;
                    }

                    if (rotationPoint != null)

                        initialTransformation = new Translation(rotationPoint.X, rotationPoint.Y,
                            rotationPoint.Z);
                    else

                        initialTransformation = new Identity();
                    
                    // Enables the ObjectManipulator to start editing the selected objects
                    model1.ObjectManipulator.Enable(initialTransformation, center);
                }
            }

            model1.Invalidate();
        }

        private Entity GetSelectedEntity(out int countSelected)
        {
            countSelected = 0;
            Entity selectedEnt = null;

            foreach (Entity ent in model1.Entities)
            {
                if (ent.Selected)
                {
                    countSelected++;
                    selectedEnt = ent;
                }
            }
            return selectedEnt;
        }
        
        private void model1_DoubleClick()
        {
            if (Editing)
            {
                // Applies the transformation from the ObjectManipulator
                model1.ObjectManipulator.Apply();
                model1.Entities.Regen();
                Editing = false;
            }
            else
                // Sets the selected BlockReference as current
                model1.Entities.SetSelectionAsCurrent();

            model1.Invalidate();
        }
        
        private void model1_MouseMove(object sender, MouseEventArgs e)
        {
            if (model1.ActionMode != actionType.Rotate &&
                model1.ActionMode != actionType.Zoom &&
                model1.ActionMode != actionType.Pan)

                DrawTooltip(e);
        }

        private void Model1_OnMouseEnter(object sender, MouseEventArgs e)
        {
            model1.Focus();
        }

        private void model1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (model1.ActionMode != actionType.Rotate &&
                model1.ActionMode != actionType.Zoom &&
                model1.ActionMode != actionType.Pan)
            {
                // force a new display of the tooltip
                toolTipText = string.Empty;
                DrawTooltip(e);
            }
        }

        private void DrawTooltip(MouseEventArgs e)
        {
            if (model1.ObjectManipulator.Visible)
                return;
            
            int entId = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)));

            string newString = string.Empty;

            if (entId >= 0)

                newString = textSelectEntity;

            else

                newString = textResetCurrent;

            SetToolTipText(newString);
        }

        private void SetToolTipText(string newString)
        {
            if (String.Compare(newString, toolTipText) != 0)
            {
                toolTip1.IsOpen = false;
                toolTip1.Content = newString;
                model1.ToolTip = toolTip1;                
                toolTipText = newString;
                toolTip1.IsOpen = true;
            }
        }

        private void OnObjectManipulatorMouseOver(object sender, ObjectManipulator.ObjectManipulatorEventArgs args)
        {
            // force a new display of the tooltip
            string newString = string.Empty;

            switch (args.ActionMode)
            {
                case ObjectManipulator.actionType.Rotate:
                    newString = textDragRotate;
                    break;

                case ObjectManipulator.actionType.RotateOnView:
                    newString = textDragRotateOnView;
                    break;

                case ObjectManipulator.actionType.TranslateOnAxis:
                    newString = textDragTranslateOnAxis;
                    break;

                case ObjectManipulator.actionType.TranslateOnView:
                    newString = textDragTranslateOnView;
                    break;

                case ObjectManipulator.actionType.Scale:
                    newString = textDragScale;
                    break;

                case ObjectManipulator.actionType.UniformScale:
                    newString = textDragScaleUniform;
                    break;

                case ObjectManipulator.actionType.None:
                    newString = textApplyOrCancel;
                    break;

            }

            SetToolTipText(newString);
        }
        
        private void HideToolTip(object sender, EventArgs e)
        {
            toolTip1.IsOpen = false;
        }

        private void ComponentButton_Checked(object sender, RoutedEventArgs e)
        {
            // hides/shows components 
            if (translatingAxis.IsChecked != null)
            {
                model1.ObjectManipulator.TranslateX.Visible = translatingAxis.IsChecked.Value;
                model1.ObjectManipulator.TranslateY.Visible = translatingAxis.IsChecked.Value;
                model1.ObjectManipulator.TranslateZ.Visible = translatingAxis.IsChecked.Value;
            }

            if (rotationButton.IsChecked != null)
            {
                model1.ObjectManipulator.RotateX.Visible = rotationButton.IsChecked.Value;
                model1.ObjectManipulator.RotateY.Visible = rotationButton.IsChecked.Value;
                model1.ObjectManipulator.RotateZ.Visible = rotationButton.IsChecked.Value;
            }

            if (scalingButton.IsChecked != null)
            {
                model1.ObjectManipulator.ScaleX.Visible = scalingButton.IsChecked.Value;
                model1.ObjectManipulator.ScaleY.Visible = scalingButton.IsChecked.Value;
                model1.ObjectManipulator.ScaleZ.Visible = scalingButton.IsChecked.Value;
            }

            model1.CompileUserInterfaceElements();
            model1.Invalidate();
        }
        
        private void sizeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (model1 != null)
            {
                model1.ObjectManipulator.Size = (int) sizeBar.Value;
                model1.CompileUserInterfaceElements();
                model1.Invalidate();
            }
        }

        private void showDraggedOnlyCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ObjectManipulator.ShowDraggedItemOnly = showDraggedOnlyCheckBox.IsChecked != null && showDraggedOnlyCheckBox.IsChecked.Value;    
        }

        private void stepSettings_changed(object sender, EventArgs e)
        {
            if (model1 == null) return;

            double step;
            if (translationCheckBox.IsChecked != null && translationCheckBox.IsChecked.Value)
            {
                translationTextBox.IsEnabled = true;
                if (double.TryParse(translationTextBox.Text, out step))
                {
                    model1.ObjectManipulator.TranslationStep = step;
                }
            }
            else
            {
                translationTextBox.IsEnabled = false;
                model1.ObjectManipulator.TranslationStep = 0;
            }

            if (rotationCheckBox.IsChecked != null && rotationCheckBox.IsChecked.Value)
            {
                rotationTextBox.IsEnabled = true;
                if (double.TryParse(rotationTextBox.Text, out step))
                {
                    model1.ObjectManipulator.RotationStep = Utility.DegToRad(step);
                }
            }
            else
            {
                rotationTextBox.IsEnabled = false;
                model1.ObjectManipulator.RotationStep = 0;
            }

            if (scalingCheckBox.IsChecked != null && scalingCheckBox.IsChecked.Value)
            {
                scalingTextBox.IsEnabled = true;
                if (double.TryParse(scalingTextBox.Text, out step))
                {
                    model1.ObjectManipulator.ScalingStep = step;
                }
            }
            else
            {
                scalingTextBox.IsEnabled = false;
                model1.ObjectManipulator.ScalingStep = 0;
            }
        }

        private void styleEnumButton_Click(object sender, EventArgs e)
        {
            model1.ObjectManipulator.StyleMode = (ObjectManipulator.styleType)styleEnumButton.Value;
            model1.CompileUserInterfaceElements();
            model1.Invalidate();
        }

        private void ballActionEnumButton_Click(object sender, EventArgs e)
        {
            model1.ObjectManipulator.BallActionMode = (ObjectManipulator.ballActionType)ballActionEnumButton.Value;
            model1.Invalidate();
        }
    }
}