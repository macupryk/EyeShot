using System;
using System.Collections.Generic;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Graphics;
using System.Drawing;
using devDept.Geometry;

namespace WpfApplication1
{
    public class MyModel : Model
    {

        List<Point> stroke = new List<Point>();

        bool leftButtonDown = false;

        public MyModel()
            : base()
        {
        }                

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) || ActionMode != actionType.None)
                return;

            stroke.Clear();
            Invalidate();

            if (e.LeftButton == MouseButtonState.Pressed)            
            {
                leftButtonDown = true;
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (leftButtonDown)
            {
                stroke.Add(RenderContextUtility.ConvertPoint(GetMousePosition(e)));

                // Repaints the scene and draws the strokes in the DrawOverlay
                PaintBackBuffer();
                SwapBuffers();
                //Invalidate();
            }

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.LeftButton == MouseButtonState.Released)            
            {
                leftButtonDown = false;
            }

        }

        protected override void DrawOverlay(DrawSceneParams myParams)
        {
            DrawLines();

            base.DrawOverlay(myParams);
        }

        private void DrawLines()
        {
            // Sets the shader for the thick lines
            renderContext.SetShader(shaderType.NoLightsThickLines);

            // Sets the line size
            renderContext.SetLineSize(4);

            // Sets the pen color
            renderContext.SetColorWireframe(Color.Red);

            for (int i = 0; i < stroke.Count - 1; i++)

                DrawLine(i);

            renderContext.SetLineSize(1);
        }

        private void DrawLine(int i)
        {
            Point current = stroke[i];
            Point next = stroke[i + 1];
            renderContext.DrawLine(new Point2D(current.X, Size.Height - current.Y), new Point2D(next.X, Size.Height - next.Y));
        }      
    }

}