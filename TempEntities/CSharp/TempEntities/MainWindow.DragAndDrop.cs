using System;
using System.Windows;
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
using System.Windows.Controls;

namespace WpfApplication1
{
    // This code file shows how to perform a DragAndDrop operation by using TempEntities
    public partial class MainWindow
    {
        private string _selBlockName;
        private bool isDragging;
        private Point3D dragFrom;
        private Entity tempEntity;
        private BlockReference currentRef;
        
        private void listView1_SelectedChanged(object sender, SelectionChangedEventArgs  e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                // saves the selected material to be apply
                _selBlockName = ((ImageItem)this.listView1.SelectedItems[0]).Name;

                // start a dragdrop anction to listView1
                try
                {    
                    DragDrop.DoDragDrop((ListView)sender, _selBlockName, DragDropEffects.Copy);
                }
                catch { }

                // clear selection
                this.listView1.SelectedItems.Clear();
            }
        }
        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (_selBlockName != null)
            {
                // shows copy cursor inside the listView
                e.Effects = DragDropEffects.Copy;

                if (!isDragging)
                {
                    isDragging = true;

                    // start a drag-drop action to viewport
                    DragDrop.DoDragDrop(model1, _selBlockName, DragDropEffects.Copy);
                }
            }
        }
        
        private void ListView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // reset dragging operation if it ends inside ListView
            isDragging = false;
        }

        private void viewport_dragEnter(object sender, DragEventArgs e)
        {
            if (isDragging && tempEntity == null && _selBlockName != null)
            {
                // shows copy cursor inside the viewport
                e.Effects = DragDropEffects.Copy;

                // creates the tempEntity from the block data
                currentRef = new BlockReference(_selBlockName);
                Entity temp = GetUniqueEntity(currentRef);

                // if is checked shows only the axis-aligned bounding box as temp entity
                if (bboxCheckBox.IsChecked.Value)
                {
                    Size3D s = temp.BoxSize;
                    Point3D bm = temp.BoxMin;
                    temp = Mesh.CreateBox(s.X, s.Y, s.Z);
                    temp.Translate(bm.X, bm.Y, bm.Z);
                    temp.Regen(0.1);
                }
                // adds the temp entity to the viewport
                model1.TempEntities.Add(temp, Color.FromArgb(100, model1.Blocks[_selBlockName].Entities[0].Color));
                tempEntity = temp;

                // saves the start point position of the temp entity
                dragFrom = Plane.XY.PointAt(Plane.XY.Project((temp.BoxMax + temp.BoxMin) / 2));
                
                // refresh the screen
                model1.Invalidate();
            }
            else
                //shows default cursor
                e.Effects = DragDropEffects.None;

        }
        private void viewport_dragOver(object sender, DragEventArgs e)
        {
            // gets current mouse position
            System.Drawing.Point mouseLocation = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));
           

            if (model1.ActionMode != actionType.None || model1.GetToolBar().Contains(mouseLocation))

                return;

            if (isDragging && tempEntity != null)
            {
                // current 3D point
                Point3D dragTo;
                
                model1.ScreenToPlane(mouseLocation, Plane.XY, out dragTo);
                
                Vector3D delta = Vector3D.Subtract(dragTo, dragFrom);

                // applies the translation to the temp entity
                tempEntity.Translate(delta);
                tempEntity.Regen(0.1);

                // saves translations applied
                if (tempEntity.EntityData == null)
                {
                    tempEntity.EntityData = delta;
                }
                else
                {
                    tempEntity.EntityData = ((Vector3D) tempEntity.EntityData) + delta;
                }

                // updates camera Near and Far planes to avoid clipping temp entity on the scene during translation
                model1.TempEntities.UpdateBoundingBox();

                // refresh the screen
                model1.Invalidate();

                // sets start as current
                dragFrom = dragTo;
            }

        }
        private void viewport_dragDrop(object sender, DragEventArgs e)
        {
            if (isDragging)
            {
                //shows default cursor
                e.Effects = DragDropEffects.None;

                if (_selBlockName != null)
                {
                    // gets current mouse position
                    System.Drawing.Point mouseLocation = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));

                    // current 3D point
                    Point3D dragTo;

                    model1.ScreenToPlane(mouseLocation, Plane.XY, out dragTo);
                    
                    Vector3D delta = (Vector3D)tempEntity.EntityData;

                    // translates entity to the temp entity current position
                    currentRef.Transformation = new Translation(delta);

                    // adds the entity to the viewport
                    model1.Entities.Add(currentRef);

                    // refresh the entities treeView
                    PopulateTree(treeView1, new List<Entity>(){currentRef}, model1.Blocks);
                }
                
                FinishDraggingOperation();
            }
        }


        private void viewport_dragLeave(object sender, EventArgs e)
        {
            if (isDragging)
            {
                FinishDraggingOperation();
            }
        }

        private void FinishDraggingOperation()
        {
            // removes current dragging tempEntity from the viewport
            model1.TempEntities.Remove(tempEntity);

            // reset dragging values
            _selBlockName = null;
            currentRef = null;
            tempEntity = null;
            isDragging = false;

            // refresh the screen
            model1.Invalidate();
        }
    }
}