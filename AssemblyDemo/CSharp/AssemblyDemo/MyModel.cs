using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using devDept.CustomControls;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;

namespace WpfApplication1
{
    class MyModel : Model
    {
        public bool duplicatedBlockDetected;
        private string WarningMessage = "Multiple references to the same block at root level. Limitation on nested instance settings.";
        //was the last key pressed the right mouse button?
        private bool lastDownWasRight = false;
        
        public MyModel()
        {
            Entities = new MyEntityList();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // we avoid mouse up actions for the right mouse button click
            // becuse we need that button just to for the ContextMenu
            if (e.RightButton != MouseButtonState.Released || !lastDownWasRight)
                base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // we avoid mouse down actions for the right mouse button click
            // becuse we need that button just to for the ContextMenu
            lastDownWasRight = true;
            if (e.RightButton != MouseButtonState.Pressed)
            {
                lastDownWasRight = false;
                base.OnMouseDown(e);
            }
        }

        protected override void DrawOverlay(DrawSceneParams data)
        {
            // display warning message
            if (duplicatedBlockDetected && !Entities.IsOpenCurrentBlockReference)
            {
                DrawText(Size.Width - 5, 5, WarningMessage,
                    new Font(System.Drawing.FontFamily.GenericSerif, 1, System.Drawing.FontStyle.Regular,
                        GraphicsUnit.Pixel), Color.Red, Color.FromArgb(127, Color.White), ContentAlignment.BottomRight);
            }
        }
    }

    class MyEntityList : EntityList
    {
        // The tree of the current EntityList assembly
        internal AssemblyTreeView assemblyTree;
        
        public override void Paste()
        {
            base.Paste();
            CheckDuplicatedBlockReferences();

            // if there is an AssemblyBrowser Tree associated, then update the tree
            if(assemblyTree != null)
                assemblyTree.PopulateTree(this);
        }

        public override void AddRange(IEnumerable<Entity> collection)
        {
            base.AddRange(collection);
            CheckDuplicatedBlockReferences();
        }
        
        public override void Add(Entity entity)
        {
            base.Add(entity);
            CheckDuplicatedBlockReferences();
        }

        public override bool Remove(Entity entity)
        {
            bool remove = base.Remove(entity);
            CheckDuplicatedBlockReferences();
            return remove;
        }

        public override void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            CheckDuplicatedBlockReferences();
        }

        /// <summary>
        /// Checks if there are multiple references to the same block in the current EntityList to display an error message on the Model.
        /// </summary>
        private void CheckDuplicatedBlockReferences()
        {
            if (CurrentBlockReference != null)
                return;

            HashSet<string> blocksNames = new HashSet<string>();
            ((MyModel)environment).duplicatedBlockDetected = false;
            foreach (Entity entity in this)
            {
                if (entity is BlockReference)
                {
                    BlockReference br = (BlockReference) entity;
                    if (blocksNames.Contains(br.BlockName))
                    {
                        ((MyModel)environment).duplicatedBlockDetected = true;
                        break;
                    }
                    blocksNames.Add(br.BlockName);
                }
            }
        }
    }
}
