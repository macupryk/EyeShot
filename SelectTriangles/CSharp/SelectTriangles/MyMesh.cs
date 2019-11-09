using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace WpfApplication1
{
    public class MyMesh : Mesh, ISelect
    {
        private MyModel vp;
        
        public MyMesh(MyModel vp, Mesh other)
            : base(other)
        {
            this.vp = vp;
        }
        
        public List<int> selectedSubItems = new List<int>();
        
        // Gets or sets the list of selected triangles 
        public List<int> SelectedSubItems
        {
            get { return selectedSubItems; }
            set
            {
                selectedSubItems = value;
                RegenMode = regenType.CompileOnly;
            }
        }
      
        protected override void DrawForShadow(RenderParams data)
        {
            DrawSelected(data);
        }
        
        public bool DrawSubItemsForSelection { get; set; }
        
        protected override void DrawForSelection(GfxDrawForSelectionParams data)
        {
            if (DrawSubItemsForSelection)
            {
                var prev = vp.SuspendSetColorForSelection;
                vp.SuspendSetColorForSelection = false;

                // Draws the triangles with the color-coding needed for visibility computation 
                for (int index = 0; index < selectedSubItems.Count; index++)
                {
                    //draws only the triangles with normals directions towards the Camera
                    int i = selectedSubItems[index];

                    Point3D p1 = Vertices[Triangles[i].V1];

                    Point3D p2 = Vertices[Triangles[i].V2];
                    Point3D p3 = Vertices[Triangles[i].V3];

                    double[] u = new[] { p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z };
                    double[] v = new[] { p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z };

                    // cross product
                    Vector3D Normal = new Vector3D(u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0]);
                    Normal.Normalize();

                    if (Vector3D.Dot(vp.Camera.NearPlane.AxisZ, Normal) <= 0)
                        continue;


                    vp.SetColorDrawForSelection(i);

                    IndexTriangle tri = Triangles[i];
                    data.RenderContext.DrawTriangles(new Point3D[]
                    {
                        Vertices[tri.V1],
                        Vertices[tri.V2],
                        Vertices[tri.V3],
                    },
                        Vector3D.AxisZ);
                }

                vp.SuspendSetColorForSelection = prev;

                // reset the color to avoid issues with the entities drawn after this one
                data.RenderContext.SetColorWireframe(System.Drawing.Color.White);
            }

            else
                base.DrawForSelection(data);
        }

        protected override void DrawIsocurves(DrawParams data)
        {
            data.RenderContext.Draw(drawData);
        }

        protected override void DrawSelected(DrawParams drawParams)
        {
            base.Draw(drawParams);
        }

        private void DrawSelectedSubItems(DrawParams data)
        {
            // Draws the selected triangles over the other triangles 
            if (SelectedSubItems.Count == 0)
                return;

            bool popState = false;
            int alpha = 255;
            if (data.RenderContext.LightingEnabled())
            {
                if (data.RenderContext.CurrentMaterial.Diffuse.A == 255)
                {
                    data.RenderContext.PushDepthStencilState();
                    data.RenderContext.SetState(depthStencilStateType.DepthTestEqual);
                    popState = true;
                }
                else
                    alpha = data.RenderContext.CurrentMaterial.Diffuse.A;
            }
            else
            {
                if (data.RenderContext.CurrentWireColor.A == 255)
                {
                    data.RenderContext.PushDepthStencilState();
                    data.RenderContext.SetState(depthStencilStateType.DepthTestEqual);
                    popState = true;
                }
                else
                    alpha = data.RenderContext.CurrentWireColor.A;
            }

            var prevCol = data.RenderContext.CurrentWireColor;
            var prevMatFront = data.RenderContext.CurrentMaterial.Diffuse;
            var prevMatBack = data.RenderContext.CurrentBackMaterial.Diffuse;
            
            if (data.RenderContext.LightingEnabled())

                data.RenderContext.SetMaterialFrontAndBackDiffuse(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow), true);
            
            else
                data.RenderContext.SetColorWireframe(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow));

            // to properly support multicolor mesh during triangles selection
            if (vp.UseShaders)
            {
                data.RenderContext.PushShader();

                switch (vp.DisplayMode)
                {
                    case displayType.Flat:
                    case displayType.HiddenLines:
                    case displayType.Wireframe:
                        data.RenderContext.SetShader(shaderType.NoLights);
                        break;
                    default:
                        data.RenderContext.SetShader(shaderType.Standard);
                        break;
                }
            }

            data.RenderContext.Draw(drawSelectedData);

            if (vp.UseShaders)
            {
                data.RenderContext.PopShader();
            }

            data.RenderContext.SetColorWireframe(prevCol);              
            data.RenderContext.SetMaterialFrontDiffuse(prevMatFront);
            data.RenderContext.SetMaterialBackDiffuse(prevMatBack);

            if (popState)
                data.RenderContext.PopDepthStencilState();
        }   
        
        public void CompileSelected(RenderContextBase renderContext)
        {
            VBOParams vboP = new VBOParams();
            Point3D[] pts;
            Vector3D[] selNormals;
            GetSelectedData(out pts, out selNormals);

            vboP.vertices = ConvertToFloatArray(pts);
            vboP.normals = ConvertToFloatArray(selNormals);
            vboP.primitiveMode = primitiveType.TriangleList;

            renderContext.CompileVBO(drawSelectedData, CompileSelectedSubItems, vboP);

            renderContext.Compile(drawSelectedEdgesData, CompileSelectedEdges, renderContext);


            needsCompileSelected = false;
        }

        private void CompileSelectedSubItems(RenderContextBase renderContext, object myParams)
        {
            // Compiles the selected triangles
            Point3D[] pts;
            Vector3D[] selNormals;
            GetSelectedData(out pts, out selNormals);

            renderContext.DrawTriangles(pts, selNormals);
        }

        EntityGraphicsData drawSelectedEdgesData = new EntityGraphicsData();

        private void CompileSelectedEdges(RenderContextBase renderContext, object myParams)
        {
            var pts = new Point3D[selectedSubItems.Count * 6];

            for (int i = 0, count = 0; i < SelectedSubItems.Count; i++)
            {
                var tri1 = Triangles[SelectedSubItems[i]];
                pts[count++] = Vertices[tri1.V1];
                pts[count++] = Vertices[tri1.V2];
                pts[count++] = Vertices[tri1.V2];
                pts[count++] = Vertices[tri1.V3];
                pts[count++] = Vertices[tri1.V3];
                pts[count++] = Vertices[tri1.V1];                
            }

            ((RenderContextBase)myParams).DrawLines(pts);
        }

        private void GetSelectedData(out Point3D[] pts, out Vector3D[] selNormals)
        {
            pts = new Point3D[SelectedSubItems.Count*3];
            selNormals = new Vector3D[pts.Length];

            int count = 0;

            if (Triangles[0] is ITriangleSupportsNormals)
            {
                for (int i = 0; i < SelectedSubItems.Count; i++, count += 3)
                {
                    var tri1 = Triangles[SelectedSubItems[i]];

                    pts[count] = Vertices[tri1.V1];
                    pts[count + 1] = Vertices[tri1.V2];
                    pts[count + 2] = Vertices[tri1.V3];

                    ITriangleSupportsNormals tri = (ITriangleSupportsNormals) tri1;
                    selNormals[count] = Normals[tri.N1];
                    selNormals[count + 1] = Normals[tri.N2];
                    selNormals[count + 2] = Normals[tri.N3];
                }
            }
            else
            {
                for (int i = 0; i < SelectedSubItems.Count; i++, count += 3)
                {
                    var tri1 = Triangles[SelectedSubItems[i]];

                    pts[count] = Vertices[tri1.V1];
                    pts[count + 1] = Vertices[tri1.V2];
                    pts[count + 2] = Vertices[tri1.V3];

                    selNormals[count] = Normals[SelectedSubItems[i]];
                    selNormals[count + 1] = Normals[SelectedSubItems[i]];
                    selNormals[count + 2] = Normals[SelectedSubItems[i]];
                }
            }
        }

        float[] ConvertToFloatArray(Point3D[] pts)
        {
            float[] a = new float[pts.Length * 3];

            int count = 0;
            for (int i = 0; i < pts.Length; i++)
            {
                a[count++] = (float)pts[i].X;
                a[count++] = (float)pts[i].Y;
                a[count++] = (float)pts[i].Z;
            }
            return a;
        }

        protected override void Draw(DrawParams data)
        {
            base.Draw(data);

            DrawSelectedSubItems(data);
        }

        protected override void DrawHiddenLines(DrawParams data)
        {
            base.DrawHiddenLines(data);

            DrawSelectedSubItems(data);
        }

        protected override void DrawFlat(DrawParams data)
        {
            base.DrawFlat(data);

            DrawSelectedSubItems(data);
        }

        protected override void Render(RenderParams data)
        {
            Draw(data);
        }

        protected override void DrawWireframe(DrawParams data)
        {
            if (Edges != null && Edges.Length > 0)
            data.RenderContext.Draw(drawEdgesData);

            if (SelectedSubItems.Count > 0)
            {
                data.RenderContext.PushDepthStencilState();
                data.RenderContext.SetState(depthStencilStateType.DepthTestLessEqual);
                data.RenderContext.SetColorWireframe(System.Drawing.Color.Yellow);

                data.RenderContext.Draw(drawSelectedEdgesData);

                data.RenderContext.PopDepthStencilState();
            }
        }

        protected override bool IsCrossing(FrustumParams data)
        {
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();
            
            bool res = InsideOrCrossingFrustum(data);

            if (data.DisplayMode != displayType.Wireframe && ThroughTriangle(data))
                res = true;
                        
            UpdateCompileSelection();

            return res;
        }

        protected override bool InsideOrCrossingFrustum(FrustumParams data)
        {
            // Computes the triangles that are inside or crossing the selection planes

            bool insideOrCrossing = false;

            for (int i = 0; i < Triangles.Length; i++)
            {
                var verts = GetTriangleVertices(Triangles[i]);
                if (Utility.InsideOrCrossingFrustum(verts[0], verts[1], verts[2], data.Frustum))
                {
                    SelectedSubItems.Add(i);
                    
                    insideOrCrossing = true;

                    //if selection filter is ByPick/VisibleByPick selects only the first triangle
                    if (vp.firstOnlyInternal && !vp.processVisibleOnly)
                        return true;
                }
            }

            return insideOrCrossing;
        }

        protected override bool ThroughTriangle(FrustumParams data)
        {
            SelectedSubItems.Sort();

            //if selection filter is ByPick/VisibleByPick selects only the first triangle
            if (vp.firstOnlyInternal && !vp.processVisibleOnly && SelectedSubItems.Count > 0)
                return false;

            bool through = false;
            
            for (int i = 0; i < Triangles.Length; i++)
            {
                if (SelectedSubItems.BinarySearch(i) >= 0)
                    continue;

                if (ThroughTriangle(data, GetTriangleVertices(Triangles[i])))
                {
                    SelectedSubItems.Add(i);
                    
                    through = true;

                    if (vp.firstOnlyInternal && !vp.processVisibleOnly)
                        return true;
                }
            }
            
            return through;
        }

        // Gets the list of the vertices of the triangle
        Point3D[] GetTriangleVertices(IndexTriangle tri)
        {
            return new Point3D[] {Vertices[tri.V1], Vertices[tri.V2], Vertices[tri.V3]};
        }

        bool ThroughTriangle(FrustumParams data, Point3D[] vertices)
        {
            Transformation transform = data.Transformation;

            if (transform == null)
            {
                if (FrustumEdgesTriangleIntersection(data.SelectionEdges, vertices[0], vertices[1], vertices[2]))

                    return true;
            }
            else
            {
                if (FrustumEdgesTriangleIntersection(data.SelectionEdges, transform* vertices[0], transform* vertices[1], transform* vertices[2]))

                    return true;
            }

            return false;
        }

        protected override bool IsCrossingScreenPolygon(ScreenPolygonParams data)
        {
            // Computes the triangles that are crossing the screen polygon
            
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            bool res = base.IsCrossingScreenPolygon(data);

            UpdateCompileSelection();
            
            return res;
        }

        private void UpdateCompileSelection()
        {
            needsCompileSelected = SelectedSubItems.Count > 0;            
        }

        public bool needsCompileSelected;
        
        protected override bool InsideOrCrossingScreenPolygon(ScreenPolygonParams data)
        {
            // Computes the triangles that are inside or crossing the screen polygon

            for (int i = 0; i < Triangles.Length; i++)
            {
                var verts = GetTriangleVertices(Triangles[i]);
                    
                if (UtilityEx.InsideOrCrossingScreenPolygon(verts[0], verts[1], verts[2], data))
                {
                    SelectedSubItems.Add(i);
                }
            }

            return false;
        }

        protected override bool ThroughTriangleScreenPolygon(ScreenPolygonParams data)
        {
            SelectedSubItems.Sort();
            
            for (int i = 0; i < Triangles.Length; i ++)
            {
                if (SelectedSubItems.BinarySearch(i) >= 0)
                    continue;

                var verts = GetTriangleVertices(Triangles[i]);
                if (ThroughTriangleScreenPolygon(verts[0], verts[1], verts[2], data))
                {
                    SelectedSubItems.Add(i);
                }
            }
            return false;
        }

        protected override bool AllVerticesInFrustum(FrustumParams data)
        {
            // Computes the triangles that are completely enclosed to the selection rectangle
            
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            for (int i = 0; i < Triangles.Length; i++)
            {
                IndexTriangle tri = Triangles[i];

                if (Camera.IsInFrustum(Vertices[tri.V1], data.Frustum) && Camera.IsInFrustum(Vertices[tri.V2], data.Frustum) &&
                    Camera.IsInFrustum(Vertices[tri.V3], data.Frustum))
                {
                    SelectedSubItems.Add(i);
                }
            }

            UpdateCompileSelection();
            return false;
        }

        protected override bool AllVerticesInScreenPolygon(ScreenPolygonParams data)
        {
            // Computes the triangles that are completely enclosed to the screen polygon
            
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            for (int i = 0; i < Triangles.Length; i++)
            {
                var verts = GetTriangleVertices(Triangles[i]);

                if (UtilityEx.AllVerticesInScreenPolygon(data, verts, 3))
                {
                    SelectedSubItems.Add(i);
                }
            }

            UpdateCompileSelection();           

            return false;
        }
        
        public void SelectSubItems(int[] indices)
        {
            // sets as selected all the triangles in the indices array
            SelectedSubItems = new List<int>(indices);
            UpdateCompileSelection();            
        }
    }
}
