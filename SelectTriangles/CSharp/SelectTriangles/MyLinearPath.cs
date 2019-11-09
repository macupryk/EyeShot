using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using OpenGL;

namespace WpfApplication1
{
    public class MyLinearPath : LinearPath, ISelect
    {
        private MyModel vp;
        public IndexLine[] Lines;
        public MyLinearPath(MyModel vp, LinearPath other)
            : base(other)
        {
            this.vp = vp;
        }

        public MyLinearPath(MyLinearPath another) : base(another)
        {
            vp = another.vp;

            Lines = new IndexLine[another.Lines.Length];
            Array.Copy(another.Lines, Lines, Lines.Length);
        }

        public override object Clone()
        {
            return new MyLinearPath(this);
        }
        
        public List<int> selectedSubItems = new List<int>();

        // Gets or sets the list of selected lines 
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
            Draw(data);
        }
        
        public bool DrawSubItemsForSelection { get; set; }

        protected override void DrawForSelection(GfxDrawForSelectionParams data)
        {
            if (DrawSubItemsForSelection)
            {
                var prev = vp.SuspendSetColorForSelection;
                vp.SuspendSetColorForSelection = false;

                // Draws the lines with the color-coding needed for visibility computation 
                for (int index = 0; index < SelectedSubItems.Count; index++)
                {
                    Point3D p1 = Vertices[Lines[SelectedSubItems[index]].V1];
                    Point3D p2 = Vertices[Lines[SelectedSubItems[index]].V2];


                    vp.SetColorDrawForSelection(SelectedSubItems[index]);

                    IndexLine tri = Lines[SelectedSubItems[index]];
                    data.RenderContext.DrawLine(p1, p2);
                }

                vp.SuspendSetColorForSelection = prev;

                // reset the color to avoid issues with the entities drawn after this one
                data.RenderContext.SetColorWireframe(System.Drawing.Color.White); 
            }

            else
            {
                data.RenderContext.DrawIndexLines(Lines, Vertices); 

            }
        }
        
        private void DrawSelectedSubItems(DrawParams data)
        {
            // Draws the selected lines over the other lines 

            if (SelectedSubItems.Count == 0)
                return;
            
            bool popState = false;
            int alpha = 255;


            if (data.RenderContext.CurrentWireColor.A == 255)
            {
                data.RenderContext.PushDepthStencilState();
                data.RenderContext.SetState(depthStencilStateType.DepthTestEqual);
                popState = true;
            }
            else if (data.RenderContext.LightingEnabled())
            {
                data.RenderContext.PushBlendState();
                data.RenderContext.SetState(blendStateType.Blend);
                alpha = data.RenderContext.CurrentMaterial.Diffuse.A;
            }
            else
            {
                data.RenderContext.PushBlendState();
                data.RenderContext.SetState(blendStateType.Blend);
                alpha = data.RenderContext.CurrentWireColor.A;
            }

            if (vp.UseShaders)
            {
                data.RenderContext.PushShader();

                data.RenderContext.SetShader(shaderType.NoLightsThickLines);
            }
            
            data.RenderContext.SetLineSize(LineWeight);

            var prevCol = data.RenderContext.CurrentWireColor;
            data.RenderContext.SetColorWireframe(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow));
            
            
            // draws lines
            List<IndexLine> seLines = new List<IndexLine>(SelectedSubItems.Count);
            for (int i = 0; i < SelectedSubItems.Count; i++)
            {
                seLines.Add(Lines[SelectedSubItems[i]]);
            }
            data.RenderContext.DrawIndexLines(seLines, Vertices);

            // restores previous settings
            if (vp.UseShaders)
            {
                data.RenderContext.PopShader();
            }
            data.RenderContext.SetColorWireframe(prevCol);     
            data.RenderContext.SetLineSize(1);        

            if (popState)
                data.RenderContext.PopDepthStencilState();
            else
                data.RenderContext.PopBlendState();
        }   
        
        protected override void Draw(DrawParams data)
        {
            data.RenderContext.SetLineSize(LineWeight);

            if (Color.A != 255)
            {
                // draws only non-selected transparent lines to avoid blended color
                List<IndexLine> linesToDraw = Lines.ToList();
                SelectedSubItems.Sort();
                for (int i = SelectedSubItems.Count - 1; i >= 0; i--)
                {
                    linesToDraw.RemoveAt(SelectedSubItems[i]);
                }

                data.RenderContext.DrawIndexLines(linesToDraw, Vertices);
            }
            else
                data.RenderContext.DrawIndexLines(Lines, Vertices);
            data.RenderContext.SetLineSize(1);
            
            DrawSelectedSubItems(data);
        }
        
        protected override void DrawSelected(DrawParams data)
        {
            Draw(data);
        }
        protected override void DrawHiddenLines(DrawParams data)
        {
            Draw(data);
        }

        protected override void DrawFlat(DrawParams data)
        {
            Draw(data);
        }

        protected override void Render(RenderParams data)
        {
            Draw(data);
        }

        protected override void DrawWireframe(DrawParams data)
        {
            Draw(data);
        }

        protected override bool IsCrossing(FrustumParams data)
        {
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            bool res = InsideOrCrossingFrustum(data);
            
            return res;
        }

        protected override bool InsideOrCrossingFrustum(FrustumParams data)
        {
            // Computes the lines that are inside or crossing the selection planes

            bool insideOrCrossing = false;

            for (int i = 0; i < Lines.Length; i++)
            {
                if (Utility.IsSegmentInsideOrCrossing(data.Frustum, new Segment3D(Vertices[Lines[i].V1], Vertices[Lines[i].V2])))
                {
                    SelectedSubItems.Add(i);

                    insideOrCrossing = true;

                    //if selection filter is ByPick/VisibleByPick selects only the first line
                    if (vp.firstOnlyInternal && !vp.processVisibleOnly)
                        return true;
                }
            }

            return insideOrCrossing;
        }


        protected override bool IsCrossingScreenPolygon(ScreenPolygonParams data)
        {
            
            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            bool res = base.IsCrossingScreenPolygon(data);

            return res;
        }
        
        protected override bool InsideOrCrossingScreenPolygon(ScreenPolygonParams data)
        {
            // Computes the lines that are inside or crossing the screen polygon
            for (int i = 0; i < Lines.Length; i++)
            {
                Segment2D seg;
                
                IndexLine line = Lines[i];
                Point3D pt1 = Vertices[line.V1];
                Point3D pt2 = Vertices[line.V2];

                Point3D screenP1 = vp.Camera.WorldToScreen(pt1, data.ViewFrame);
                Point3D screenP2 = vp.Camera.WorldToScreen(pt2, data.ViewFrame);

                if (screenP1.Z > 1 || screenP2.Z > 1)
                    return false;  // for perspective

                seg = new Segment2D(screenP1, screenP2);

                if (UtilityEx.PointInPolygon(screenP1, data.ScreenPolygon) ||
                    UtilityEx.PointInPolygon(screenP2, data.ScreenPolygon))

                {
                    SelectedSubItems.Add(i);
                    continue;
                }

                for (int j = 0; j < data.ScreenSegments.Count; j++)
                {
                    Point2D i0;
                    if (Segment2D.Intersection(data.ScreenSegments[j], seg, out i0))

                    {
                        SelectedSubItems.Add(i);
                        break;
                    }
                }
            }

            return false;
        }

        protected override bool AllVerticesInFrustum(FrustumParams data)
        {
            // Computes the lines that are completely enclosed to the selection rectangle

            if (vp.processVisibleOnly && !Selected)
                return false;

            SelectedSubItems = new List<int>();

            for (int i = 0; i < Lines.Length; i++)
            {
                IndexLine line = Lines[i];

                if (Camera.IsInFrustum(Vertices[line.V1], data.Frustum) && Camera.IsInFrustum(Vertices[line.V2], data.Frustum))
                {
                    SelectedSubItems.Add(i);
                }
            }

            return false;
        }

        protected override bool AllVerticesInScreenPolygon(ScreenPolygonParams data)
        {
            // Computes the lines that are completely enclosed to the screen polygon

            if (vp.processVisibleOnly && !Selected)
                return false;
            
            SelectedSubItems = new List<int>();

            for (int i = 0; i < Lines.Length; i++)
            {
                IndexLine line = Lines[i];

                if (UtilityEx.AllVerticesInScreenPolygon(data, new List<Point3D>(){Vertices[line.V1], Vertices[line.V2]}, 2))
                {
                    SelectedSubItems.Add(i);
                }
            }

            return false;
        }

        public void SelectSubItems(int[] indices)
        {
            // sets as selected all the lines in the indices array
            SelectedSubItems = new List<int>(indices);
        }
    }
}
