using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using devDept.Eyeshot.Labels;
using devDept.Eyeshot;
using devDept.Graphics;
using System.Drawing;
using System.Collections;
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using System.Diagnostics;
using MouseButton = System.Windows.Input.MouseButton;
using Point = System.Drawing.Point;

namespace WpfApplication1
{

    public class MyModel : devDept.Eyeshot.Model
    {
        public MyModel() : base()
        {
            CameraMoveEnd += MyModel_CameraMoveEnd;
        }
        
        private bool _measuring;      

        private Point3D _measureEndPoint;
        private Point3D _currentPoint;
        
        Plane _plane = Plane.XY;

        bool _firstClick = false;
        
    
        private readonly List<Point3D> _points = new List<Point3D>();

        public delegate void MeasureCompletedEventHandler(object sender, EventArgs e);
        public event MeasureCompletedEventHandler MeasureCompleted;

        private void SetPlane()
        {            
            _plane = Camera.NearPlane;
        }   

        void MyModel_CameraMoveEnd(object sender, Model.CameraMoveEventArgs e)
        {
            if (_measuring)
                SetPlane();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Point location = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            if (GetToolBar().Contains(location))
            {
                base.OnMouseUp(e);

                return;
            }
            
            if (_measuring)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (_firstClick == false)
                    {
                        _points.Clear();
                        _firstClick = true;
                    }

                    if (FindClosestPoint(location) == -1)
                        StopMeasuring(false);
                    else
                    {
                        _points.Add(_measureEndPoint);

                        if (_points.Count > 1)
                        {
                            _line = new Line(_points[0], _points[1])
                            {
                                LineWeightMethod = colorMethodType.byEntity,
                                LineWeight = 1
                            };

                            string text = String.Format("{0} mm", Math.Round(_line.Length(),2));
                            var to = new TextOnly((_line.StartPoint + _line.EndPoint)/2, text, new Font("Tahoma", 8.25f), Color.Black, ContentAlignment.BottomLeft);
                            Entities.Add(_line, Color.Black);
                            Labels.Add(to);

                            Invalidate();

                            StopMeasuring(false);
                        }
                    }
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    ResetMeasuring();
                }

            }           
         
            base.OnMouseUp(e);
        }        

        private void ResetMeasuring()
        {
            _firstClick = false;
            _points.Clear();
            _measureEndPoint = null;
            _currentPoint = null;
        }

        protected Cursor DefaultCursor
        {
            get
            {
                if (_measuring)
                    return Cursors.Cross;

                return GetDefaultCursor();
            }
        }
        
        public void Measure(bool start)
        {
            if (start)
            {
                ActionMode = actionType.None;                                
                _measuring = true;
                SetDefaultCursor(DefaultCursor);
                Cursor = DefaultCursor;
                SetPlane();
                Focus();
            }
            else if (_measuring)
                StopMeasuring(true);
        }

        private void StopMeasuring(bool fromCheckedChanged)
        {
            ResetMeasuring();
            _measuring = false;
            SetDefaultCursor(DefaultCursor);
            Cursor = DefaultCursor;
            if (MeasureCompleted != null && !fromCheckedChanged)
                MeasureCompleted(this, new EventArgs());
        }
        
        private Line _line;
        System.Drawing.Point _mouseLocation;

        protected override void OnMouseMove(MouseEventArgs e)        
        {
            // save the current mouse position
            _mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            // if start is valid and actionMode is None and it's not in the toolbar area

            if (_currentPoint == null || ActionMode != actionType.None || GetToolBar().Contains(_mouseLocation))
            {

                base.OnMouseMove(e);

                return;
            }

            base.OnMouseMove(e);

            FindClosestPoint(_mouseLocation);

            // paint the viewport surface
            PaintBackBuffer();

            // consolidates the drawing
            SwapBuffers();

        }
                      
        private int FindClosestPoint(System.Drawing.Point point)
        {                        
            int result = -1;            
            int entityIndex = GetEntityUnderMouseCursor(point);            

            if (entityIndex != -1)
            {
                Entity ent = Entities[entityIndex];
                if (ent is ICurve)
                {
                    result = FindClosestVertex(point, 8, out _measureEndPoint);
                }
            }
            else
            {
                _measureEndPoint = null;
            } 
            return result;
        }

        protected override void DrawOverlay(DrawSceneParams myParams)
        {            
            if (!_measuring)
            {
                base.DrawOverlay(myParams);

                return;
            }

            ScreenToPlane(_mouseLocation, _plane, out _currentPoint);                        

            // size line 
            renderContext.SetLineSize(1);

            // draw inverted
            renderContext.EnableXOR(true);

            renderContext.SetState(depthStencilStateType.DepthTestOff);

            if (ActionMode == actionType.None && !GetToolBar().Contains(_mouseLocation))
            {
                if (_points.Count > 0)
                {
                    List<Point3D> pts2 = new List<Point3D>();

                    // Draw elastic line
                    pts2.Add(WorldToScreen(_points[0]));
                    pts2.Add(WorldToScreen(_currentPoint));

                    renderContext.DrawLines(pts2.ToArray());                    
                }

                // disables draw inverted
                renderContext.EnableXOR(false);

                if (_measureEndPoint != null)
                {
                    // text drawing
                    DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10,
                        "Current point: "
                        + _measureEndPoint.X.ToString("f2") + ", "
                        + _measureEndPoint.Y.ToString("f2") + ", "
                        + _measureEndPoint.Z.ToString("f2"), new Font("Tahoma", 8.25f), Color.Black, ContentAlignment.BottomLeft);
                }
                
            }
            else
            {
                // disables draw inverted
                renderContext.EnableXOR(false);
            }

            base.DrawOverlay(myParams);
        }        
    }

}