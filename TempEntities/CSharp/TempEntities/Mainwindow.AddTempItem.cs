using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace WpfApplication1
{
    public partial class MainWindow
    {
    #if NURBS
        private enum itemType
        {
            Vertex,
            Edge,
            Face,
            None
        }
        itemType itemMode = itemType.None;

        List<Entity> tempItems = new List<Entity>();

        private void addItemToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (addItemToggle.IsChecked.HasValue && addItemToggle.IsChecked.Value)
            {
                addItemToggle.Content = "Disable";

                // sets the item mode
                itemMode = (itemType)addItemCombo.SelectedIndex;
                
                switch (itemMode)
                {
                    case itemType.Vertex:

                        // sets selection filter mode in order to get only the vertices under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Vertex;
                        break;

                    case itemType.Edge:

                        // sets selection filter mode in order to get only the edges under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Edge;
                        break;

                    case itemType.Face:

                        // sets selection filter mode in order to get only the faces under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Face;
                        break;
                }

                // gets the leafs Brep entities under mouse cursor
                model1.AssemblySelectionMode = devDept.Eyeshot.Environment.assemblySelectionType.Leaf;
                
                // disables moving action and button
                move = false;
                moveToggle.IsEnabled = false;
            }
            else
            {
                addItemToggle.Content = "Enable";

                // disables the add item action
                itemMode = itemType.None;

                // clears current temporary items
                foreach (Entity item in tempItems)
                {
                    model1.TempEntities.Remove(item);

                }
                tempItems.Clear();

                // restores moving action and button
                moveToggle.IsEnabled = true;
                move = moveToggle.IsChecked.HasValue && moveToggle.IsChecked.Value;
            }
            // refresh the screen
            model1.Invalidate();
        }
        private void addItemCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (addItemToggle.IsChecked.HasValue && addItemToggle.IsChecked.Value)
            {
                // enables the add item action with the selected item mode
                itemMode = (itemType)addItemCombo.SelectedIndex;

                switch (itemMode)
                {
                    case itemType.Vertex:

                        // sets selection filter mode in order to get only the vertices under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Vertex;
                        break;

                    case itemType.Edge:

                        // sets selection filter mode in order to get only the edges under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Edge;
                        break;

                    case itemType.Face:

                        // sets selection filter mode in order to get only the faces under mouse cursor
                        model1.SelectionFilterMode = selectionFilterType.Face;
                        break;
                }
            }
            else
                // disables the add item action
                itemMode = itemType.None;
        }

        private void AddEntityItem(System.Windows.Point mousePosition)
        {
            if (itemMode == itemType.None)
                return;

            // the tranformation of the parent BlockReference
            Transformation trans = new Identity();

            // the item under mouse cursor to be added into TempEntities list
            Entity tempItem = null;

            // gets the vertex under mouse cursor
            devDept.Eyeshot.Environment.SelectedSubItem selItem =(devDept.Eyeshot.Environment.SelectedSubItem) model1.GetItemUnderMouseCursor(RenderContextUtility.ConvertPoint(mousePosition));

            if (selItem == null)
                return;

            //the Brep entity under mouse cursor
            Brep brep = (Brep) selItem.Item;

            // gets transformation of the parent BlockReference (there is only one level of hierarchy)
            trans = selItem.Parents.First().Transformation;

            switch (itemMode)
            {
                case itemType.Vertex:
                    // creates a Point as temp entity that represent the vertex item
                    tempItem = new devDept.Eyeshot.Entities.Point(brep.Vertices[selItem.Index], 15);
                    tempItem.Color = Color.FromArgb(150, Color.Blue);
                    break;

                case itemType.Edge:
                    // creates an ICurve as temp entity that represent the edge item
                    tempItem = (Entity)((Entity)brep.Edges[selItem.Index].Curve).Clone();
                    tempItem.LineWeight = 10;
                    tempItem.Color = Color.FromArgb(150, Color.Purple);
                    break;

                case itemType.Face:
                    // creates a Mesh as temp entity that represent the face item
                    tempItem = brep.Faces[selItem.Index].ConvertToMesh(skipEdges: true);
                    tempItem.Color = Color.FromArgb(150, Color.DeepSkyBlue);
                    break;
            }
            // transform the temp entity onto the represented item 
            tempItem.TransformBy(trans);

            // regens it before to add into TempEntity list
            if (tempItem is ICurve)
                tempItem.Regen(0.01);

            // adds it to the TempEntities list
            model1.TempEntities.Add(tempItem);

            //stores it into tempItems list
            tempItems.Add(tempItem);
        }

        private void model1_MouseDownItem(object sender, MouseButtonEventArgs e)
        {
            // add item(vertex, Edge or Face) under mouse cursor as Temporary Entity on the screen
            if(e.LeftButton == MouseButtonState.Pressed && model1.ActionMode == actionType.None && !model1.GetToolBar().Contains(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))))
                AddEntityItem(model1.GetMousePosition(e));
        }
    #endif
    }
}