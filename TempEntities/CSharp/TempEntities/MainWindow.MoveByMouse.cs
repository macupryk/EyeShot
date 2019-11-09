using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Point = System.Drawing.Point;

namespace WpfApplication1
{
    
    // This code file shows how to move entity in the scene with the mouse.
    // It displays also some temp arrow entities showing the moving plane direction.
    public partial class MainWindow
    {
        int entityIndex = -1;
        bool move;
        Plane xyzPlane;
        Point3D moveFrom;
        Point3D centerOfArrows;
        Mesh[] tempArrows;

        private void CreateArrowsDirections()
        {
            // removes previous arrows if present
            if(tempArrows != null)
            {
                model1.TempEntities.Remove(tempArrows[0]);
                model1.TempEntities.Remove(tempArrows[1]);
                model1.TempEntities.Remove(tempArrows[2]);
                model1.TempEntities.Remove(tempArrows[3]);
            }

            //creates 4 temporary arrows on the current moving plane to display when the mouse is over an entity
            tempArrows = new Mesh[4];

            devDept.Eyeshot.Entities.Region arrowShape = new devDept.Eyeshot.Entities.Region(new LinearPath(xyzPlane, new Point2D[]
            {
                new Point2D(0,-2),
                new Point2D(4,-2),
                new Point2D(4,-4),
                new Point2D(10,0),
                new Point2D(4,4),
                new Point2D(4,2),
                new Point2D(0,2),
                new Point2D(0,-2),
            }),xyzPlane);

            //right arrow
            tempArrows[0] = arrowShape.ExtrudeAsMesh(2, 0.1, Mesh.natureType.Plain);
            tempArrows[0].Regen(0.1);
            tempArrows[0].Color = Color.FromArgb(100, Color.Red);

            //top arrow
            tempArrows[1] = (Mesh)tempArrows[0].Clone();
            tempArrows[1].Rotate(Math.PI / 2, xyzPlane.AxisZ);
            tempArrows[1].Regen(0.1);

            //left arrow
            tempArrows[2] = (Mesh)tempArrows[0].Clone();
            tempArrows[2].Rotate(Math.PI, xyzPlane.AxisZ);
            tempArrows[2].Regen(0.1);

            //bottom arrow
            tempArrows[3] = (Mesh)tempArrows[0].Clone();
            tempArrows[3].Rotate(-Math.PI / 2, xyzPlane.AxisZ);
            tempArrows[3].Regen(0.1);
            
            Vector3D diagonalV = new Vector3D(tempArrows[0].BoxMin, tempArrows[0].BoxMax);
            double offset = Math.Max(Vector3D.Dot(diagonalV, xyzPlane.AxisX), Vector3D.Dot(diagonalV, xyzPlane.AxisY));
            Vector3D translateX = xyzPlane.AxisX *  offset/ 2;
            Vector3D translateY = xyzPlane.AxisY *  offset/ 2;

            tempArrows[0].Translate(translateX);
            tempArrows[1].Translate(translateY);
            tempArrows[2].Translate(-1 * translateX);
            tempArrows[3].Translate(-1 * translateY);
            
            centerOfArrows = Point3D.Origin;
        }
        private void TranslateAndShowArrows(Point mouseLocation)
        {
            // gets the entity index under mouse cursor
            entityIndex = model1.GetEntityUnderMouseCursor(mouseLocation);

            if (entityIndex < 0)
            {
                // removes previous temporary arrows if present
                if (tempArrows != null)
                {
                    model1.TempEntities.Remove(tempArrows[0]);
                    model1.TempEntities.Remove(tempArrows[1]);
                    model1.TempEntities.Remove(tempArrows[2]);
                    model1.TempEntities.Remove(tempArrows[3]);
                }

                //refresh the screen
                model1.Invalidate();
                return;
            }

            // gets the center of the entity bounding box
            Entity ent = model1.Entities[entityIndex];
            Point3D center = (ent.BoxMax + ent.BoxMin) / 2;
            
            // gets translation from arrows center position to entity center position
            Vector3D trans = new Vector3D(centerOfArrows, center);

            // translates arrows
            tempArrows[0].Translate(trans);
            tempArrows[2].Translate(trans);
            tempArrows[1].Translate(trans);
            tempArrows[3].Translate(trans);

            // updates center position
            centerOfArrows = center;

            // if not already added, adds them to TempEntities list
            if (model1.TempEntities.Count < 4)
            {
                model1.TempEntities.Add(tempArrows[0]);
                model1.TempEntities.Add(tempArrows[1]);
                model1.TempEntities.Add(tempArrows[2]);
                model1.TempEntities.Add(tempArrows[3]);
                
                // updates camera Near and Far planes to avoid clipping temp entity on the scene during translation
                model1.TempEntities.UpdateBoundingBox();
            }

            //refresh the screen
            model1.Invalidate();
        }       

        private void planeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // changes moving plane 
            switch (planeCombo.SelectedIndex)
            {
                case 0:
                   xyzPlane = Plane.XY;
                    break;
                case 1:
                    xyzPlane = Plane.ZX;
                    break;
                case 2:
                    xyzPlane = Plane.YZ;
                    break;
                default:
                    xyzPlane = Plane.XY;
                    break;
            }

            // creates arrows lying on the chosen plane
            CreateArrowsDirections();
        }

        private void model1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));

            if (!move ||e.LeftButton != MouseButtonState.Pressed || model1.ActionMode != actionType.None || model1.GetToolBar().Contains(mousePos))
                return;

            // gets the entity index
            entityIndex = model1.GetEntityUnderMouseCursor(mousePos);

            if (entityIndex < 0)

                return;

            // gets 3D start point
            model1.ScreenToPlane(mousePos, xyzPlane, out moveFrom);
        }

        private void model1_MouseMove(object sender, MouseEventArgs e)
        {
            // if moving action is enabled, then draws temporary arrows when the mouse is hover an entity
            if(move && e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                TranslateAndShowArrows(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)));
            }

            if (!move || e.LeftButton != MouseButtonState.Pressed || model1.ActionMode != actionType.None || model1.GetToolBar().Contains(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))))
                return;

            if (moveFrom == null)
                return;

            // if we found an entity and the left mouse button is down
            if (entityIndex != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                // removes temp arrows during translation, if present
                model1.TempEntities.Remove(tempArrows[0]);
                model1.TempEntities.Remove(tempArrows[1]);
                model1.TempEntities.Remove(tempArrows[2]);
                model1.TempEntities.Remove(tempArrows[3]);

                // gets the entity reference
                Entity entity = model1.Entities[entityIndex] as Entity;

                // current 3D point
                Point3D moveTo;

                model1.ScreenToPlane(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), xyzPlane, out moveTo);

                Vector3D delta = Vector3D.Subtract(moveTo, moveFrom);
                
                // sets start as current
                moveFrom = moveTo;

                // applies the translation
                entity.Translate(delta);
                
                // regens entities that need it
                model1.Entities.Regen();

                // refresh the screen
                model1.Invalidate();

                // sets start as current
                moveFrom = moveTo;
                
                //updates blinked entity if present
                if (entity.EntityData != null)
                {
                    ((Entity)entity.EntityData).Translate(delta);
                    ((Entity)entity.EntityData).Regen(0.01);
                }

            }
        }
        private void model1_MouseUp(object sender, MouseEventArgs e)
        {
            entityIndex = -1;
        }

        private void moveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (moveToggle.IsChecked.HasValue && moveToggle.IsChecked.Value)
            {
                moveToggle.Content = "Disable";

                move = true;
            }
            else
            {
                moveToggle.Content = "Enable";

                move = false;
            }
        }
    }
}