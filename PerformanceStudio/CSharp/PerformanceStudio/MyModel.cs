using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Geometry;
using devDept.Graphics;

namespace PerformanceStudio
{
    class MyModel : devDept.Eyeshot.Model
    {
        private Point3D _current;
        private System.Drawing.Point _mouseLocation;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e));
            _current = ScreenToWorld(RenderContextUtility.ConvertPoint(GetMousePosition(e)));
            // paint the viewport surface
            PaintBackBuffer();
            // consolidates the drawing
            SwapBuffers();
            base.OnMouseMove(e);
        }

        protected override void DrawOverlay(Model.DrawSceneParams myParams)
        {
            // text drawing
            if (_current != null)
            {
                DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10, "Point Coord: " + _current, new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.Black, System.Drawing.ContentAlignment.BottomLeft);
            }
            else
            {
                DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10, "Depth for Transparency", new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.Black, System.Drawing.ContentAlignment.BottomLeft);
            }
            base.DrawOverlay(myParams);
        }
    }
}
