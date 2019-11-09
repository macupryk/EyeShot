using System;
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
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
    // This code file shows how to blink an entity (over the other entities) with a defined offset time by using tempEntities.
    // It shows also how to display and keep update a TreeView of the Entities list on the scene.
    public partial class MainWindow
    {
        bool showEntity = false;
        System.Threading.Timer blinkTimer = null;
        Model.SelectedItem selectedItem = null;
        Mesh blinkEntity = null;
        
        public void StartBlink(int intervalMs)
        {
            if (selectedItem == null || blinkEntity != null)
                return;

            // gets a unique mesh to blink from the selected entity
            blinkEntity = GetUniqueEntity((Entity)selectedItem.Item);
            
            // stores blink temp entity reference to keep them synchronized during translation
            if (selectedItem.Item is BlockReference)
                ((Entity)selectedItem.Item).EntityData = blinkEntity;

            // if selected item is not a root element, find the root
            if (selectedItem.HasParents())
            {
                // transforms temp entity to the real position of the original leaf entity
                Transformation t = new Identity();
                foreach (BlockReference parent in selectedItem.Parents)
                {
                    t = parent.Transformation * t;
                }
                
                blinkEntity.TransformBy(t);

                // stores blink temp entity reference into root element
                selectedItem.Parents.Last().EntityData = blinkEntity;
            }

            blinkEntity.Color = Color.FromArgb(100, Color.Yellow);
            
            // hides edges for blink entity
            blinkEntity.Edges = null;
            blinkEntity.EdgeStyle = Mesh.edgeStyleType.None;

            // computes the needed data to draw temp entity
            if(blinkEntity.RegenMode == regenType.RegenAndCompile)
                blinkEntity.Regen(0.1);

            // starts the blink action
            blinkTimer = new System.Threading.Timer(Blink, blinkEntity, 0, intervalMs);
        }
        public void StopBlink()
        {
            if (blinkTimer == null)
                return;
            
            blinkTimer.Dispose();

            model1.TempEntities.Remove(blinkEntity);

            showEntity = false;
            blinkEntity = null;
        }
        private void Blink(object sender)
        {
            // Draws the temp entity on the scene alternately at each timer tick (defined by the interval time in ms)
            showEntity = !showEntity;

            if (showEntity)
                model1.TempEntities.Add((Entity)sender);
            else
                model1.TempEntities.Remove((Entity)sender);

            Dispatcher.Invoke(() =>
            {
                //refresh the screen
                model1.Invalidate();            
            }); 
        }
        private void blinkToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (blinkToggle.IsChecked.HasValue && blinkToggle.IsChecked.Value)
            {
                blinkToggle.Content = "Disable";

                if (treeView1.SelectedItem == null)

                    ((TreeNode)treeView1.Items[0]).IsSelected = true;

                //starts a new blink action of 500ms
                StartBlink(500);
            }
            else
            {
                blinkToggle.Content = "Enable";

                // stops current blink action
                StopBlink();
            }

            //refresh the screen
            model1.Invalidate();
        }

        #region TreeView methods
        public static void PopulateTree(TreeView tv, IList<Entity> entList, BlockKeyedCollection blocks, TreeNode parentNode = null)
        {
            ItemCollection nodes;
            if (parentNode == null)
            {
                nodes = tv.Items;
            }
            else
            {
                nodes = parentNode.Items;
            }
            
            for (int i = 0; i < entList.Count; i++)
            {
                Entity ent = entList[i];
                if (ent is BlockReference)
                {
                    Block child;
                    string blockName = ((BlockReference)ent).BlockName;

                    if (blocks.TryGetValue(blockName, out child))
                    {
                        TreeNode parentTn = new TreeNode(parentNode, GetNodeName(blockName, i));
                        parentTn.Tag = ent;

                        nodes.Add(parentTn);
                        PopulateTree(tv, child.Entities, blocks, parentTn);
                    }
                }
                else
                {
                    string type = ent.GetType().ToString().Split('.').LastOrDefault();
                    var node = new TreeNode(parentNode, GetNodeName(type, i));
                    node.Tag = ent;
                    nodes.Add(node);
                }
            }
        }
        private static string GetNodeName(string name, int index)
        {
            return String.Format("{0} ({1})", name, index);
        }

        private void treeView1_SelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // creates a selected entity instance from the TreeView selection
            selectedItem = SynchTreeSelection(treeView1, model1);

            if (blinkToggle.IsChecked.HasValue && blinkToggle.IsChecked.Value)
            {
                // stops current blink if active
                StopBlink();

                //start a new blink action with the new selected entity
                StartBlink(500);
            }

            //refresh the screen
            model1.Invalidate();
        }
        
        public Model.SelectedItem SynchTreeSelection(TreeView tv, Model vl)
        {
            // Fill a stack of entities and blockreferences starting from the node tags
            Stack<BlockReference> parents = new Stack<BlockReference>();

            TreeNode node = (TreeNode)tv.SelectedItem;
            
            Entity entity = node.Tag as Entity;

            node = node.ParentNode;

            while (node != null)
            {
                var ent = node.Tag as Entity;
                if (ent != null)

                    parents.Push((BlockReference)ent);

                node = node.ParentNode;
            }
            
            // The top most parent is the root Blockreference: must reverse the order, creating a new Stack
            Stack<BlockReference> stack = new Stack<BlockReference>(parents);

            // return the selected entity instance
            return new Model.SelectedItem(stack, entity);
        }

        #endregion
    }

    /// <summary>
    /// In the XAML markup, I have specified a HierarchicalDataTemplate for the ItemTemplate of the TreeView.
    /// This class represent the ViewModel for TreeView's Items.
    /// </summary>
    public class TreeNode : FrameworkElement
    {
        public TreeNode(TreeNode parent)
        {
            Items = new TreeView().Items;
            ParentNode = parent;
        }

        public TreeNode(TreeNode parent, string text) : this(parent)
        {
            Text = text;
        }

        public string Text { get; set; }

        public TreeNode ParentNode { get; set; }

        public ItemCollection Items { get; set; }

        public int GetLevel()
        {
            if (ParentNode != null)
            {
                return ParentNode.GetLevel() + 1;
            }
            return 0;
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(TreeNode), new PropertyMetadata(default(bool)));

        public TreeNode GetChildNode(string name)
        {
            foreach (TreeNode node in Items)
            {
                if (node.Text.Equals(name))
                    return node;
            }

            return null;
        }

        public bool ContainsChildNode(string name)
        {
            foreach (TreeNode node in Items)
            {
                if (node.Text.Equals(name))
                    return true;
            }

            return false;
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(TreeNode), new PropertyMetadata(default(bool)));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public override string ToString()
        {
            return Text;
        }

        public void Remove()
        {
            while (Items.Count > 0)
            {
                ((TreeNode)Items[0]).Remove();
            }

            if (ParentNode != null)
                ParentNode.Items.Remove(this);
        }
    }
}