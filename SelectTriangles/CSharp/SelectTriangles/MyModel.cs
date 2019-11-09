using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Graphics;

namespace WpfApplication1
{
    public class MyModel : Model
    {
        private bool processSubItems = false;      // enabled triangles selection   
        internal bool firstOnlyInternal = false;    // needed for selection by pick
        internal bool processVisibleOnly = false;   // used for visible by pick to find all the triangles selected during GetCrossingEntities
        public override void ProcessSelection(Rectangle selectionBox, bool firstOnly, bool invert, SelectionChangedEventArgs eventArgs, bool selectableOnly = true)
        {
            // Selects the entities first
            foreach (var entity in Entities)
            {
                if (entity is ISelect)
                    ((ISelect)entity).DrawSubItemsForSelection = false;
            }

            base.ProcessSelection(selectionBox, firstOnly, invert, eventArgs, selectableOnly);

            // Now selects the triangles for the selected entities
            processSubItems = true;
            SuspendSetColorForSelection = true;

            // Performs the triangles selection one entity at a time
            foreach (var entity in Entities)
            {
                if (entity is ISelect && entity.Selected)
                {
                    ((ISelect)entity).DrawSubItemsForSelection = true;

                    UpdateVisibleSelection();

                    base.ProcessSelection(selectionBox, firstOnly, invert, eventArgs, selectableOnly);

                    UpdateVisibleSelection();
                    ((ISelect)entity).DrawSubItemsForSelection = false;

                    if (firstOnly)
                        break;
                }
            }

            SuspendSetColorForSelection = false;
            processSubItems = false;
        }

        protected override int[] GetCrossingEntities(Rectangle selectionBox, bool firstOnly, bool selectableOnly = true)
        {
            if (!processSubItems)

                return base.GetCrossingEntities(selectionBox, firstOnly, selectableOnly);

            // Reads the visible triangles from the back buffer and selects them
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is ISelect && Entities[i].Selected)
                {
                    ISelect entity = Entities[i] as ISelect;

                    // Selects the triangles
                    base.GetCrossingEntities(selectionBox, firstOnly, selectableOnly);
                    
                    if (!processVisibleOnly)
                        // Removes the selection flag, otherwise the entity will be drawn all selected
                       ((Entity)entity).Selected = false;

                    if (firstOnly)
                        break;
                }
            }

            return new int[0];
        }

        public override void ProcessSelectionVisibleOnly(Rectangle selectionBox, bool firstOnly, bool invert, SelectionChangedEventArgs eventArgs,
            bool selectableOnly = true, bool temporarySelection = false)
        {
            // Selects the entities first
            foreach (var entity in Entities)
            {
                if (entity is ISelect)
                    ((ISelect) entity).DrawSubItemsForSelection = false;
            }

            base.ProcessSelectionVisibleOnly(selectionBox, firstOnly, invert, eventArgs, selectableOnly, temporarySelection);

            // Now selects the triangles for the selected entities
            processSubItems = true;
            SuspendSetColorForSelection = true;
            processVisibleOnly = true;

            // Performs the triangles selection one entity at a time
            foreach (var entity in Entities)
            {
                if (entity is ISelect && entity.Selected)
                {
                    ((ISelect) entity).DrawSubItemsForSelection = true;

                    UpdateVisibleSelection();

                    // gets only the triangles in the selection box
                    GetCrossingEntities(selectionBox, false);

                    base.ProcessSelectionVisibleOnly(selectionBox, firstOnly, invert, eventArgs, selectableOnly, temporarySelection);

                    UpdateVisibleSelection();
                    ((ISelect)entity).DrawSubItemsForSelection = false;
                }
            }

            SuspendSetColorForSelection = false;
            processSubItems = false;
            processVisibleOnly = false;
        }

        protected override int[] GetVisibleEntitiesFromBackBuffer(Viewport viewport, byte[] rgbValues, int stride, int bpp, Rectangle selectionBox,
            bool firstOnly)
        {
            if (!processSubItems)

                return base.GetVisibleEntitiesFromBackBuffer(viewport, rgbValues, stride, bpp, selectionBox, firstOnly);

            // Reads the visible triangles from the back buffer and selects them
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is ISelect && Entities[i].Selected)
                {
                    ISelect entity = Entities[i] as ISelect;

                    // Gets the indices of the triangles to select
                    int[] indices = base.GetVisibleEntitiesFromBackBuffer(viewport, rgbValues, stride, bpp, selectionBox, firstOnly);

                    // Selects the triangles
                    entity.SelectSubItems(indices);

                    // Removes the selection flag, otherwise the entity will be drawn all selected
                    ((Entity)entity).Selected = false; 

                    break;
                }
            }

            return new int[0];
        }
    }
}
