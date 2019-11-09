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
        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                    
            CreateHybridEntity(model1);

            model1.BoundingBox.Visible = true;
            
            model1.ZoomFit();

            model1.Invalidate();
         
            base.OnContentRendered(e);
        }

        public static void CreateHybridEntity(Model vp)
        {
            double width = 0.5;
            Point3D[] profilePoints = new Point3D[]
                                    {
                                        new Point3D(-5, 0, 0), new Point3D(5, 0, 0), new Point3D(5, 0.5, 0),  
                                        new Point3D(width, width, 0), new Point3D(width, 10 - width, 0), new Point3D(5, 10 - width, 0), 
                                        new Point3D(5, 10, 0), new Point3D(-5, 10, 0), new Point3D(-5, 10 - width, 0), 
                                        new Point3D(-width, 10 - width, 0), new Point3D(-width, width, 0), new Point3D(-5, width, 0), new Point3D(-5, 0, 0)
                                    };

            LinearPath profileLp = new LinearPath(profilePoints);
            Region profile = new Region(profileLp);

            double length1 = 80;
            
            MyHybridMesh m = profile.ExtrudeAsMesh<MyHybridMesh>(new Vector3D(0, 0, length1), 0.1, Mesh.natureType.Plain);
            m.Rotate(Math.PI / 2, Vector3D.AxisZ);
            m.Translate(5, 0, 0);
            m.wireVertices = BuildWire(length1).ToArray();            
            vp.Entities.Add(m, System.Drawing.Color.Red);

            MyHybridMesh m2 = (MyHybridMesh)m.Clone();
            m2.Rotate(Math.PI, Vector3D.AxisZ);
            m2.Translate(60, 0, 0);            
            vp.Entities.Add(m2, System.Drawing.Color.Red);

            double length2 = 60;
            MyHybridMesh m3 = profile.ExtrudeAsMesh<MyHybridMesh>(new Vector3D(0, 0, length2), 0.1, Mesh.natureType.Plain);
            m3.Rotate(Math.PI / 2, Vector3D.AxisZ);
            m3.Translate(5, 0, 0);
            m3.wireVertices = BuildWire(length2).ToArray();
            m3.Rotate(Math.PI / 2, Vector3D.AxisY);
            m3.Translate(0, 0, length1);            
            vp.Entities.Add(m3, System.Drawing.Color.Green);

        }

        private static List<Point3D> BuildWire(double length)
        {
            List<Point3D> wires = new List<Point3D>();

            Point3D p1 = new Point3D(-20, 0, 0);
            Point3D p2 = Point3D.Origin;

            wires.Add(new Point3D(0, 0, 0));
            wires.Add(new Point3D(0, 0, length));

            wires.Add(p1);
            wires.Add(p2);

            int numArrows = 10;
            double step = length / numArrows;

            Point3D ptArrow1 = new Point3D(-4, 0, -2);
            Point3D ptArrow2 = new Point3D(-4, 0, 2);

            Vector3D dir = Vector3D.AxisZ;

            for (int i = 0; i < numArrows + 1; i++)
            {
                Point3D offset = dir * step * i;
                Point3D newPos = p1 + offset;
                Point3D newPos2 = p2 + offset;

                wires.Add(newPos);
                wires.Add((Point3D)newPos2.Clone());
                wires.Add((Point3D)newPos2.Clone());
                wires.Add(newPos2 + ptArrow1);
                wires.Add((Point3D)newPos2.Clone());
                wires.Add(newPos2 + ptArrow2);
            }

            return wires;
        }

        private bool wire;

        private void ChangeNatureButton_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeNature();
        }

        private void ChangeNature()
        {
            for (int i = 0; i < model1.Entities.Count; i++)
            {
                Entity ent = model1.Entities[i];
                if (ent is MyHybridMesh)
                {
                    MyHybridMesh mcm = (MyHybridMesh)ent;
                    mcm.ChangeNature(wire ? entityNatureType.Polygon : entityNatureType.Wire);
                    ent.UpdateBoundingBox(new TraversalParams(null, model1)); // to update the values inside the entity
                }
            }

            model1.Entities.Regen();

            model1.Entities.UpdateBoundingBox();
            wire = !wire;
            model1.Invalidate();
        }

        private bool resetActionMode = true;
        private void SelectVisibleByPickButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectVisibleByPickButton.IsChecked.Value)
            {
                model1.ActionMode = actionType.SelectVisibleByPick;
                resetActionMode = false;

                //selectVisibleByPickButton.IsChecked = false;
                selectVisibleByBoxButton.IsChecked = false;
                selectByPickButton.IsChecked = false;
                selectByBoxButton.IsChecked = false;
                selectByBoxEnclButton.IsChecked = false;
                resetActionMode = true;
            }
            else if (resetActionMode)
            {
                model1.ActionMode = actionType.None;
            }
        }

        private void selectVisibleByBoxButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectVisibleByBoxButton.IsChecked.Value)
            {
                model1.ActionMode = actionType.SelectVisibleByBox;
                resetActionMode = false;

                selectVisibleByPickButton.IsChecked = false;
                // selectVisibleByBoxButton.IsChecked = false;
                selectByPickButton.IsChecked = false;
                selectByBoxButton.IsChecked = false;
                selectByBoxEnclButton.IsChecked = false;

                resetActionMode = true;
            }
            else if (resetActionMode)
            {
                model1.ActionMode = actionType.None;
            }
        }

        private void selectByPickButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectByPickButton.IsChecked.Value)
            {
                model1.ActionMode = actionType.SelectByPick;
                resetActionMode = false;

                selectVisibleByPickButton.IsChecked = false;
                selectVisibleByBoxButton.IsChecked = false;
                // selectByPickButton.IsChecked = false;
                selectByBoxButton.IsChecked = false;
                selectByBoxEnclButton.IsChecked = false;

                resetActionMode = true;
            }
            else if (resetActionMode)
            {
                model1.ActionMode = actionType.None;
            }
        }

        private void selectByBoxButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectByBoxButton.IsChecked.Value)
            {
                model1.ActionMode = actionType.SelectByBox;
                resetActionMode = false;

                selectVisibleByPickButton.IsChecked = false;
                selectVisibleByBoxButton.IsChecked = false;
                selectByPickButton.IsChecked = false;
                // selectByBoxButton.IsChecked = false;
                selectByBoxEnclButton.IsChecked = false;

                resetActionMode = true;
            }
            else if (resetActionMode)
            {
                model1.ActionMode = actionType.None;
            }
        }

        private void selectByBoxEnclButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectByBoxEnclButton.IsChecked.Value)
            {
                model1.ActionMode = actionType.SelectByBoxEnclosed;
                resetActionMode = false;

                selectVisibleByPickButton.IsChecked = false;
                selectVisibleByBoxButton.IsChecked = false;
                selectByPickButton.IsChecked = false;
                selectByBoxButton.IsChecked = false;
                // selectByBoxEnclButton.IsChecked = false;

                resetActionMode = true;
            }
            else if (resetActionMode)
            {
                model1.ActionMode = actionType.None;
            }
        }

        private void clearSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Entities.ClearSelection();
            model1.Invalidate();
        }

        private void invertSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Entities.InvertSelection();
            model1.Invalidate();
        }

        public class MyHybridMesh : Mesh
        {
            public Point3D[] wireVertices;

            private EntityGraphicsData wireGraphicsData = new EntityGraphicsData();

            public MyHybridMesh()
                : base()
            {
            }

            public MyHybridMesh(MyHybridMesh another)
                : base(another)
            {
                wireVertices = new Point3D[another.wireVertices.Length];
                for (int i = 0; i < wireVertices.Length; i++)
                {
                    wireVertices[i] = (Point3D)another.wireVertices[i].Clone();
                }
            }

            public void ChangeNature(entityNatureType nature)
            {
                entityNature = nature;
            }

            public override void Compile(CompileParams data)
            {
                data.RenderContext.Compile(wireGraphicsData, (context, @params) =>
                {
                    context.DrawLines(wireVertices);
                }, null);

                base.Compile(data);
            }

            public override void Regen(RegenParams data)
            {
                entityNatureType currNature = entityNature;

                entityNature = entityNatureType.Polygon; // so the regen of the mesh is done correctly

                base.Regen(data);

                entityNature = currNature;
            }

            public override void Dispose()
            {
                wireGraphicsData.Dispose();
                base.Dispose();
            }

            protected override void DrawForShadow(RenderParams renderParams)
            {
                if (entityNature != entityNatureType.Wire)
                    base.DrawForShadow(renderParams);
            }
            
            protected override void Draw(DrawParams data)
            {
                if (entityNature == entityNatureType.Wire)
                    data.RenderContext.Draw(wireGraphicsData);
                else
                {
                    base.Draw(data);
                }
            }


            protected override void Render(RenderParams data)
            {
                if (entityNature == entityNatureType.Wire)
                    data.RenderContext.Draw(wireGraphicsData);
                else
                    base.Render(data);
            }

            protected override void DrawForSelection(GfxDrawForSelectionParams data)
            {
                if (entityNature == entityNatureType.Wire)
                    data.RenderContext.Draw(wireGraphicsData);
                else
                    base.DrawForSelection(data);
            }

            protected override void DrawEdges(DrawParams data)
            {
                if (entityNature != entityNatureType.Wire)
                    base.DrawEdges(data);
            }

            protected override void DrawIsocurves(DrawParams data)
            {
                if (entityNature != entityNatureType.Wire)
                    base.DrawIsocurves(data);
            }            

            protected override void DrawHiddenLines(DrawParams data)
            {
                if (entityNature == entityNatureType.Wire)
                    Draw(data);
                else
                    base.DrawHiddenLines(data);
            }

            protected override void DrawNormals(DrawParams data)
            {
                if (entityNature != entityNatureType.Wire)
                    base.DrawNormals(data);
            }            

            protected override void DrawSilhouettes(DrawSilhouettesParams drawSilhouettesParams)
            {
                if (entityNature != entityNatureType.Wire)
                    base.DrawSilhouettes(drawSilhouettesParams);
            }


            protected override void DrawWireframe(DrawParams drawParams)
            {
                if (entityNature == entityNatureType.Wire)
                    Draw(drawParams);
                else

                    base.DrawWireframe(drawParams);
            }

            protected override void DrawSelected(DrawParams drawParams)
            {
                if (entityNature == entityNatureType.Wire)
                    drawParams.RenderContext.Draw(wireGraphicsData);
                else
                    base.DrawSelected(drawParams);
            }

            protected override void DrawVertices(DrawParams drawParams)
            {
                if (entityNature == entityNatureType.Wire)
                {
                    drawParams.RenderContext.DrawPoints(wireVertices);                    
                }
                else
                    base.DrawVertices(drawParams);
            }

            protected override bool InsideOrCrossingFrustum(FrustumParams data)
            {
                if (entityNature == entityNatureType.Wire)
                {
                    var transform = data.Transformation;
                    if (transform == null)
                        transform = new Identity();

                    for (int i = 0; i < wireVertices.Length; i += 2)
                    {
                        if (Utility.IsSegmentInsideOrCrossing(data.Frustum, new Segment3D(transform * wireVertices[i], transform * wireVertices[i + 1])))
                            return true;
                    }

                    return false;
                }

                return base.InsideOrCrossingFrustum(data);
            }

            protected override bool ThroughTriangle(FrustumParams data)
            {
                if (entityNature == entityNatureType.Wire)
                    return false;
                //else
                return base.ThroughTriangle(data);
            }

            public override void TransformBy(Transformation xform)
            {
                if (wireVertices != null)
                    foreach (Point3D s in wireVertices)

                        s.TransformBy(xform);

                base.TransformBy(xform);
            }

            public override object Clone()
            {
                return new MyHybridMesh(this);
            }

            protected override bool AllVerticesInFrustum(FrustumParams data)
            {
                if (entityNature == entityNatureType.Wire)

                    return UtilityEx.AllVerticesInFrustum(data, wireVertices, wireVertices.Length);

                return base.AllVerticesInFrustum(data);
            }


            protected override bool ComputeBoundingBox(TraversalParams data, out Point3D boxMin, out Point3D boxMax)
            {
                if (entityNature == entityNatureType.Wire)

                    UtilityEx.ComputeBoundingBox(data.Transformation, wireVertices, out boxMin, out boxMax);

                else

                    base.ComputeBoundingBox(data, out boxMin, out boxMax);

                return true;
            }
        }
    }
}