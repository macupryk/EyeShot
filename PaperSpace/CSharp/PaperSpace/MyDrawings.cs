using System;
using System.Drawing;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using MouseButton = System.Windows.Input.MouseButton;

namespace WpfApplication1
{
    public partial class MyDrawings : devDept.Eyeshot.Drawings
    {
        // current selection/position
        private Point3D _current;

        private bool _cursorOutside = true;

        // current mouse position
        private System.Drawing.Point _mouseLocation;

        // it always draws on XY plane
        private readonly Plane _plane = Plane.XY;

        // array of selected vertices (snapped points) 
        private Point3D[] _points = new Point3D[3];
        private int _numPoints;

        // current drawing plane and extension points required while dimensioning
        private Plane _drawingPlane;
        private Point3D _extPt1;
        private Point3D _extPt2;

        public static Color DrawingColor = Color.Black;

        /// <summary>
        /// Disables dimensioning and restore the default action mode (SelectVisibleByPick)
        /// </summary>
        public void DisableDimensioning()
        {
            _points = new Point3D[3];
            _numPoints = 0;
            DrawingLinearDim = false;
            ActionMode = actionType.SelectVisibleByPick;
        }

        /// <summary>
        /// Enable dimensioning and set action mode to none
        /// </summary>
        public void EnableDimensioning()
        {
            DrawingLinearDim = true;
            ActionMode = actionType.None;
        }

        private double GetUnitsConversionFactor()
        {
            return Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, GetActiveSheet().Units);
        }

        protected override void DrawOverlay(DrawSceneParams data)
        {
            if (DrawingLinearDim && ActionMode == actionType.None)  // checks if ZP is disabled (rotation not enabled in Drawings)
            {
                ScreenToPlane(_mouseLocation, _plane, out _current);

                if (_snappedPoint != null)
                {
                    DisplaySnappedVertex();
                }

                // set render context for interactive drawing
                renderContext.SetLineSize(1);

                renderContext.EnableXOR(true);

                renderContext.SetState(depthStencilStateType.DepthTestOff);

                // if cursor is outside from Drawings it does not need to draw anything on overlay
                if (!_cursorOutside)
                {
                    DrawPositionMark(_current);

                    if (_numPoints < 2)
                    {
                        DrawSelectionMark(_mouseLocation);

                        renderContext.EnableXOR(false);
                        string text = "Select the first point";
                        if (_numPoints == 1)
                            text = "Select the second point";

                        DrawText(_mouseLocation.X, (int) Size.Height - _mouseLocation.Y + 10,
                            text, new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
                    }
                    else
                    {
                        DrawInteractiveLinearDim();
                    }
                }
            }

            base.DrawOverlay(data);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _cursorOutside = false;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _cursorOutside = true;
            base.OnMouseLeave(e);

            Invalidate();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var mousePosition = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            if (DrawingLinearDim && ActionMode == actionType.None)
            {
                if (GetToolBar().Contains(mousePosition))
                {
                    base.OnMouseDown(e);

                    return;
                }

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (_numPoints < 2)
                    {
                        if (_snappedPoint != null)
                        {
                            _points[_numPoints++] = _snappedPoint; // adds the snapped point to the list of points
                        }

                        if (_numPoints == 1)
                        {
                            int index = GetEntityUnderMouseCursor(_mouseLocation);
                            if (index != -1)
                            {
                                var view = Entities[index] as devDept.Eyeshot.Entities.View;
                                if (view != null)
                                    _viewScale = view.Scale;
                                else
                                    _viewScale = 1;
                            }
                        }
                    }
                    else
                    {
                        // the following lines need to add LinearDim to Drawings
                        ScreenToPlane(mousePosition, _plane, out _current);
                        double unitsConversionFactor = GetUnitsConversionFactor();
                        var linearDim = new LinearDim(_drawingPlane, _points[0] / unitsConversionFactor, _points[1] / unitsConversionFactor, _current / unitsConversionFactor, DimTextHeight);
                        linearDim.Scale(unitsConversionFactor);
                        linearDim.LayerName = WiresLayerName;
                        linearDim.LinearScale = 1 / _viewScale;
                        Entities.Add(linearDim);
                        Invalidate();

                        DisableDimensioning();
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed) // restarts dimensioning
                {
                    _points = new Point3D[3];
                    _numPoints = 0;
                    _viewScale = 1;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DrawingLinearDim && ActionMode == actionType.None)
            {
                // saves the current mouse position
                _mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e));

                _snappedPoint = null;
                int index = GetEntityUnderMouseCursor(_mouseLocation);
                if (index != -1 && !(Entities[index] is RasterView))
                    FindClosestVertex(_mouseLocation, 50, out _snappedPoint); // returns the closest snapped point to the cursor

                // paints the viewport surface
                PaintBackBuffer();

                // consolidates the drawing
                SwapBuffers();
            }

            base.OnMouseMove(e);
        }
    }
}
