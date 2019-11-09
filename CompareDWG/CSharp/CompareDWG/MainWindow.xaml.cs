using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using System.Collections;
using System.Linq;
using System.Threading;
using devDept.Eyeshot.Translators;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApplication1
{
    public partial class MainWindow
    {
        private static readonly Color NOT_MODIFIED_COLOR = Color.FromArgb(44, 44, 44);

        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            //model2.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            model1.Rotate.Enabled = false;
            model2.Rotate.Enabled = false;

            model1.GetViewCubeIcon().Visible = false;
            model2.GetViewCubeIcon().Visible = false;

            model1.Camera.ProjectionMode = projectionType.Orthographic;
            model2.Camera.ProjectionMode = projectionType.Orthographic;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            //Sets the view as Top
            model1.SetView(viewType.Top); 
            model2.SetView(viewType.Top);

            // Fits the model in the viewport
            model1.ZoomFit(); 
            model2.ZoomFit(); 
            
            model1.Invalidate(); 
            model2.Invalidate(); 

            base.OnContentRendered(e);
        }

        private void beforeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorCompareAndMark(model1, beforePathLabel, model2);
        }
        private void afterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorCompareAndMark(model2, afterPathLabel, model1);
        }

        private void ColorCompareAndMark(Model modelForFile, Label pathLabel, Model modelToColor)
        {
            OpenFile(modelForFile, pathLabel);

            ColorEntities(modelToColor.Entities);

            //The action therefore are same for both models, so we do not need parameters anymore.
            if (model1.Entities.Count > 0 &&
                model2.Entities.Count > 0)
            {
                CompareAndMark(model1.Entities, model2.Entities);

                model1.ZoomFit();
                model2.ZoomFit();

                model1.Invalidate();
                model2.Invalidate();
            }
        }
        public bool AreEqual(Entity ent1, Entity ent2)
        {
            if (ent1 is CompositeCurve)
            {
                CompositeCurve cc1 = (CompositeCurve)ent1;
                CompositeCurve cc2 = (CompositeCurve)ent2;

                if (cc1.CurveList.Count == cc2.CurveList.Count)
                {
                    int equalCurvesInListCount = 0;

                    foreach (Entity entC in cc1.CurveList)
                    {
                        foreach (Entity entC2 in cc2.CurveList)
                        {
                            if (entC.GetType() == entC2.GetType())
                            {
                                if (CompareIfEqual(entC, entC2))
                                {
                                    equalCurvesInListCount++;
                                    break;
                                }
                            }
                        }
                    }

                    if (cc1.CurveList.Count == equalCurvesInListCount)
                    {
                        return true;
                    }
                }
            }

            else if (ent1 is LinearPath)
            {
                LinearPath lp1 = (LinearPath)ent1;
                LinearPath lp2 = (LinearPath)ent2;

                if (lp1.Vertices.Length == lp2.Vertices.Length)
                {
                    for (int i = 0; i < lp1.Vertices.Length; i++)
                    {
                        if (!(lp1.Vertices[i] == lp2.Vertices[i]))
                            return false;
                    }
                    return true;
                }
            }

            else if (ent1 is PlanarEntity)
            {
                PlanarEntity pe1 = (PlanarEntity)ent1;
                PlanarEntity pe2 = (PlanarEntity)ent2;
                if (
                    pe1.Plane.AxisZ == pe2.Plane.AxisZ &&
                    pe1.Plane.AxisX == pe2.Plane.AxisX
                    )
                {
                    if (ent1 is Arc)
                    {
                        Arc arc1 = (Arc)ent1;
                        Arc arc2 = (Arc)ent2;

                        if (
                            arc1.Center == arc2.Center &&
                            arc1.Radius == arc2.Radius &&
                            arc1.Domain.Min == arc2.Domain.Min &&
                            arc1.Domain.Max == arc2.Domain.Max
                            )
                        {
                            return true;
                        }
                    }
                    else if (ent1 is Circle)
                    {
                        Circle c1 = (Circle)ent1;
                        Circle c2 = (Circle)ent2;

                        if (
                            c1.Center == c2.Center &&
                            c1.Radius == c2.Radius
                            )
                        {
                            return true;
                        }
                    }
                    else if (ent1 is EllipticalArc)
                    {
                        EllipticalArc e1 = (EllipticalArc)ent1;
                        EllipticalArc e2 = (EllipticalArc)ent2;

                        if (
                            e1.Center == e2.Center &&
                            e1.RadiusX == e2.RadiusX &&
                            e1.RadiusY == e2.RadiusY &&
                            e1.Domain.Low == e2.Domain.Low &&
                            e1.Domain.High == e2.Domain.High
                        )
                        {
                            return true;
                        }
                    }
                    else if (ent1 is Ellipse)
                    {
                        Ellipse e1 = (Ellipse)ent1;
                        Ellipse e2 = (Ellipse)ent2;

                        if (
                            e1.Center == e2.Center &&
                            e1.RadiusX == e2.RadiusX &&
                            e1.RadiusY == e2.RadiusY
                        )
                        {
                            return true;
                        }
                    }

                    else if (ent1 is Text)
                    {
                        if (ent1 is Dimension)
                        {
                            Dimension dim1 = (Dimension)ent1;
                            Dimension dim2 = (Dimension)ent2;

                            if (
                                dim1.InsertionPoint == dim2.InsertionPoint &&
                                dim1.DimLinePosition == dim2.DimLinePosition
                                )
                            {

                                if (ent1 is AngularDim)
                                {
                                    AngularDim ad1 = (AngularDim)ent1;
                                    AngularDim ad2 = (AngularDim)ent2;

                                    if (
                                        ad1.ExtLine1 == ad2.ExtLine1 &&
                                        ad1.ExtLine2 == ad2.ExtLine2 &&
                                        ad1.StartAngle == ad2.StartAngle &&
                                        ad1.EndAngle == ad2.EndAngle &&
                                        ad1.Radius == ad2.Radius
                                        )
                                    {
                                        return true;
                                    }
                                }
                                else if (ent1 is LinearDim)
                                {
                                    LinearDim ld1 = (LinearDim)ent1;
                                    LinearDim ld2 = (LinearDim)ent2;

                                    if (
                                        ld1.ExtLine1 == ld2.ExtLine1 &&
                                        ld1.ExtLine2 == ld2.ExtLine2
                                        )
                                    {
                                        return true;
                                    }
                                }
                                else if (ent1 is DiametricDim)
                                {
                                    DiametricDim dd1 = (DiametricDim)ent1;
                                    DiametricDim dd2 = (DiametricDim)ent2;

                                    if (
                                        dd1.Distance == dd2.Distance &&
                                        dd1.Radius == dd2.Radius &&
                                        dd1.CenterMarkSize == dd2.CenterMarkSize
                                    )
                                    {
                                        return true;
                                    }
                                }
                                else if (ent1 is RadialDim)
                                {
                                    RadialDim rd1 = (RadialDim)ent1;
                                    RadialDim rd2 = (RadialDim)ent2;

                                    if (
                                        rd1.Radius == rd2.Radius &&
                                        rd1.CenterMarkSize == rd2.CenterMarkSize
                                    )
                                    {
                                        return true;
                                    }
                                }
                                else if (ent1 is OrdinateDim)
                                {
                                    OrdinateDim od1 = (OrdinateDim)ent1;
                                    OrdinateDim od2 = (OrdinateDim)ent2;

                                    if (
                                        od1.DefiningPoint == od2.DefiningPoint &&
                                        od1.Origin == od2.Origin &&
                                        od1.LeaderEndPoint == od2.LeaderEndPoint
                                    )
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    Console.Write("Type " + ent1.GetType() + " not implemented.");
                                    return true;
                                }
                            }
                        }

                        else if (ent1 is devDept.Eyeshot.Entities.Attribute)
                        {
                            devDept.Eyeshot.Entities.Attribute att1 = (devDept.Eyeshot.Entities.Attribute)ent1;
                            devDept.Eyeshot.Entities.Attribute att2 = (devDept.Eyeshot.Entities.Attribute)ent2;

                            if (
                                att1.Value == att2.Value &&
                                att1.InsertionPoint == att2.InsertionPoint
                                )
                            {
                                return true;
                            }
                        }

                        else
                        {
                            Text tx1 = (Text)ent1;
                            Text tx2 = (Text)ent2;

                            if (
                                tx1.InsertionPoint == tx2.InsertionPoint &&
                                tx1.TextString == tx2.TextString &&
                                tx1.StyleName == tx2.StyleName &&
                                tx1.WidthFactor == tx2.WidthFactor &&
                                tx1.Height == tx2.Height
                                )
                            {
                                return true;
                            }
                        }
                    }

                    else
                    {
                        Console.Write("Type " + ent1.GetType() + " not implemented.");
                        return true;
                    }
                }
            }

            else if (ent1 is Line)
            {
                Line line1 = (Line)ent1;
                Line line2 = (Line)ent2;

                if (
                    line1.StartPoint == line2.StartPoint &&
                    line1.EndPoint == line2.EndPoint
                )
                {
                    return true;
                }
            }

            else if (ent1 is devDept.Eyeshot.Entities.Point)
            {
                devDept.Eyeshot.Entities.Point point1 = (devDept.Eyeshot.Entities.Point)ent1;
                devDept.Eyeshot.Entities.Point point2 = (devDept.Eyeshot.Entities.Point)ent2;

                if (
                    point1.Position == point2.Position
                )
                {
                    return true;
                }
            }

#if NURBS
            else if (ent1 is Curve)
            {
                Curve cu1 = (Curve)ent1;
                Curve cu2 = (Curve)ent2;

                if (
                    cu1.ControlPoints.Length == cu2.ControlPoints.Length &&
                    cu1.KnotVector.Length == cu2.KnotVector.Length &&
                    cu1.Degree == cu2.Degree
                    )
                {
                    for (int k = 0; k < cu1.ControlPoints.Length; k++)
                    {
                        if (cu1.ControlPoints[k] != cu2.ControlPoints[k])
                        {
                            return false;
                        }
                    }

                    for (int k = 0; k < cu1.KnotVector.Length; k++)
                    {
                        if (cu1.KnotVector[k] != cu2.KnotVector[k])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
#endif

            else
            {
                Console.Write("Type " + ent1.GetType() + " not implemented.");
                return true;
            }
            return false;
        }


        public bool AreEqualAttributes(Entity ent1, Entity ent2)
        {
            return 
                ent1.LayerName == ent2.LayerName &&
                ent1.GroupIndex == ent2.GroupIndex &&
                ent1.ColorMethod == ent2.ColorMethod &&
                ent1.Color == ent2.Color &&
                ent1.LineWeightMethod == ent2.LineWeightMethod &&
                ent1.LineWeight == ent2.LineWeight &&
                ent1.LineTypeMethod == ent2.LineTypeMethod &&
                ent1.LineTypeName == ent2.LineTypeName &&
                ent1.LineTypeScale == ent2.LineTypeScale &&
                ent1.MaterialName == ent2.MaterialName;
        }
        public void ColorEntities(EntityList list)
        {
            foreach (Entity ent in list)
            {
                ent.Color = NOT_MODIFIED_COLOR;
                ent.ColorMethod = colorMethodType.byEntity;
            }
        }
        
        public void CompareAndMark(IList<Entity> entList1, IList<Entity> entList2)
        {
            bool[] equalEntitiesInV2 = new bool[entList2.Count];

            for (int i = 0; i < entList1.Count(); i++)
            {
                Entity entVp1 = entList1[i];
                bool foundEqual = false;

                for (int j = 0; j < entList2.Count(); j++)
                {
                    Entity entVp2 = entList2[j];

                    if (!equalEntitiesInV2[j] &&
                        entVp1.GetType() == entVp2.GetType() &&
                        CompareIfEqual(entVp1, entVp2))
                    {
                        equalEntitiesInV2[j] = true;
                        foundEqual = true;
                        break;
                    }
                }
                if (!foundEqual)
                {
                    entList1[i].Color = Color.Yellow;
                    entList1[i].ColorMethod = colorMethodType.byEntity;
                }
            }

            for (int j = 0; j < entList2.Count; j++)
            {
                if (!equalEntitiesInV2[j])
                {
                    entList2[j].Color = Color.Yellow;
                    entList2[j].ColorMethod = colorMethodType.byEntity;
                }
            }
        }
        public bool CompareIfEqual(Entity entVp1, Entity entVp2)
        {
            bool areEqualAttributes = AreEqualAttributes(entVp1, entVp2);
            bool areEqual = AreEqual(entVp1, entVp2);

            return areEqualAttributes && areEqual;
        }
        public void OpenFile(Model model, Label labelPath)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Filter = "DWG File|*.dwg",
                Multiselect = false,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog1.ShowDialog().Value)
            {
                beforeButton.IsEnabled = afterButton.IsEnabled = false;
                labelPath.Content = "Loading . . .";

                //Force WPF refresh
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));

                var rfa = new ReadAutodesk(openFileDialog1.FileName);
                rfa.DoWork();

                model.Clear();

                rfa.AddToScene(model, NOT_MODIFIED_COLOR);

                Entity[] toAdd = model.Entities.Explode();

                model.Entities.AddRange(toAdd, NOT_MODIFIED_COLOR);

                beforeButton.IsEnabled = afterButton.IsEnabled = true;

                labelPath.Content = openFileDialog1.FileName;

                model.SetView(viewType.Top); 

                model.ZoomFit();
                model.Invalidate();
            }
        }

        #region Camera Sync
        private void camera1_MoveEnd(object sender, EventArgs e)
        {
            SyncCamera(model1, model2);
        }
        private void camera2_MoveEnd(object sender, EventArgs e)
        {
            SyncCamera(model2, model1);
        }
        private void SyncCamera(Model modelMovedCamera, Model modelCameraToMove)
        {
            Camera savedCamera;
            modelMovedCamera.SaveView(out savedCamera);

            // restores the camera to the other model
            modelCameraToMove.RestoreView(savedCamera);
            modelCameraToMove.AdjustNearAndFarPlanes();
            modelCameraToMove.Invalidate();
        }
        #endregion
    }
}