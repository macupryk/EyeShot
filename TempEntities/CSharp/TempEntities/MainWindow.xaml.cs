using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        readonly string dirName = "myPictures";

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
#if NURBS
            this.addItemCombo.SelectionChanged += this.addItemCombo_SelectedIndexChanged;
            this.addItemToggle.Click += this.addItemToggle_CheckedChanged;
            this.model1.PreviewMouseDown += this.model1_MouseDownItem;
#else
            groupBox1.IsEnabled = false;
#endif
        }
        

        protected override void OnContentRendered(EventArgs e)
        {
            // creates the element for the ListView
            CreateElements();

            // clear entities on the scene
            model1.Entities.Clear();

            model1.Entities.Add(new BlockReference("Box"));

            model1.Entities.Add(new BlockReference(-20, 50, 0, "Cylinder", 0));

            model1.Entities.Add(new BlockReference(60, -15, 0, "Slot", 0));

            model1.Entities.Add(new BlockReference(10, 50, 0, "Triangle", 0));

            model1.Entities.Add(new BlockReference(-30, -30, 0, "Weels", 0));

            // fills the TreeView with the entities in the scene
            PopulateTree( treeView1  , model1.Entities, model1.Blocks);

            // creates the arrow to display during moving action
            CreateArrowsDirections();

            // Fits the model in the scene
            model1.ZoomFit();

            // refresh the screen
            model1.Invalidate();

            base.OnContentRendered(e);
        }
        private Mesh GetUniqueEntity(Entity ent)
        {
            // creates a unique mesh from the entity in input
            Mesh uniqueEntity = null;

            // if the entity is a BlockReference, then merges all the entities in its block     
#if NURBS
            if (ent is BlockReference)
            {
                Entity[] ents = ((BlockReference)ent).Explode(model1.Blocks, keepTessellation: true);
                uniqueEntity = ((Brep)ents[0]).ConvertToMesh(weldNow: false);

                for (int i = 1; i < ents.Length; i++)
                {
                    (uniqueEntity).MergeWith(((Brep)ents[i]).ConvertToMesh(weldNow: false), false);
                }
            }
            else
            {
                uniqueEntity = (Mesh)((Brep)ent).ConvertToMesh().Clone();
            }
#else
            if (ent is BlockReference)
            {
                Entity[] ents = ((BlockReference)ent).Explode(model1.Blocks, keepTessellation: true);
                uniqueEntity = (Mesh)ents[0];

                for (int i = 1; i < ents.Length; i++)
                {
                    (uniqueEntity).MergeWith((Mesh)ents[i], false);
                }
            }
            else
            {
                uniqueEntity = (Mesh)ent.Clone();
            }
#endif
            // regens data (if needed) before to add it into TempEntities list
            if (uniqueEntity.RegenMode == regenType.RegenAndCompile)
                uniqueEntity.Regen(0.01);

            return uniqueEntity;
        }

        #region Fills ListView
        protected void CreateElements()
        {
            // setting scene for saving
            System.Windows.Media.Brush oldColor = model1.GetBackground().TopColor;
            model1.Backface.ColorMethod = backfaceColorMethodType.SingleColor;
            model1.GetBackground().TopColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            model1.Viewports[0].GetToolBar().Visible = false;
            model1.GetCoordinateSystemIcon().Visible = false;
            model1.GetOriginSymbol().Visible = false;
            model1.GetViewCubeIcon().Visible = false;
            model1.GetGrid().Visible = false;
            model1.Flat.EdgeThickness = 10;
            model1.Flat.SilhouetteThickness = 10;
            model1.DisplayMode = displayType.Flat;
            model1.Flat.ColorMethod = flatColorMethodType.EntityMaterial;

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // creates the directory to save material elements
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            else
            {
                // deletes all previous files
                foreach (string filePath in Directory.GetFiles(dirName))
                    File.Delete(filePath);
            }

            Entity[] list = new Entity[4];

            // initialiazes the plane
          Plane p = new Plane();

            // sets the colors and material of objects
            Material m = new Material("wood", new Bitmap("../../../../../../dataset/Assets/Textures/Maple.jpg"));
            model1.Materials.Add(m);

            Color[] colors = new Color []
            {
                Color.Gray,
                Color.FromArgb(255, 0xF9, 0x88, 0x66),
                Color.FromArgb(255, 0xFF, 0x42, 0x0E),
                Color.FromArgb(255, 0x80, 0xBD, 0x9E),
                Color.FromArgb(255, 0x89, 0xDA, 0x59)
            };

            // a set of objects
#if NURBS

            // slot
            devDept.Eyeshot.Entities.Region slot = devDept.Eyeshot.Entities.Region.CreateRoundedRectangle(60,20,5, true);
            devDept.Eyeshot.Entities.Region circle = devDept.Eyeshot.Entities.Region.CreateCircle(3.6);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(-20, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(40, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            Brep slotMesh = slot.ExtrudeAsBrep(Vector3D.AxisZ * 5);
            slotMesh.Rotate(Math.PI / 2, Vector3D.AxisZ);
            slotMesh.Color = colors[0];
            slotMesh.MaterialName = "wood";
            slotMesh.ColorMethod = colorMethodType.byEntity;

            // triangle
            LinearPath trianglePath = new LinearPath(Point3D.Origin, new Point3D(36, 0, 0), new Point3D(18, 0, 25), Point3D.Origin);
            devDept.Eyeshot.Entities.Region triangleRegion2 = new devDept.Eyeshot.Entities.Region(trianglePath);
            Brep triangleMesh = triangleRegion2.ExtrudeAsBrep(Vector3D.AxisY * 5);
            triangleMesh.Color = colors[1];
            triangleMesh.ColorMethod = colorMethodType.byEntity;
            triangleMesh.Rotate(Utility.DegToRad(90), Vector3D.AxisZ);
            triangleMesh.Translate(52, -3, 0);

            // weels
            Brep weelAxisMesh = Brep.CreateCylinder(3, 65);
            weelAxisMesh.MaterialName = "wood";
            weelAxisMesh.Rotate(Math.PI / 2, Vector3D.AxisY);
            weelAxisMesh.Color = colors[2];
            weelAxisMesh.ColorMethod = colorMethodType.byEntity;

            devDept.Eyeshot.Entities.Region outer = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 12);
            devDept.Eyeshot.Entities.Region inner = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 3);
            devDept.Eyeshot.Entities.Region weel = devDept.Eyeshot.Entities.Region.Difference(outer, inner)[0];

            Brep weelRMesh = weel.ExtrudeAsBrep(10);
            weelRMesh.Translate(55, 0, 0);
            weelRMesh.Color = colors[2];
            weelRMesh.ColorMethod = colorMethodType.byEntity;

            Brep weelLMesh = weel.ExtrudeAsBrep(-10);
            weelLMesh.Translate(10, 0, 0);
            weelLMesh.Color = colors[2];
            weelLMesh.ColorMethod = colorMethodType.byEntity;

            // cylinder
            Brep cylMesh = Brep.CreateCylinder(3.5, 40);
            cylMesh.Color = colors[3];
            cylMesh.ColorMethod = colorMethodType.byEntity;

            //box
            Brep baseMesh = Brep.CreateBox(40, 40, 5); 
            baseMesh.Color = colors[4];
            baseMesh.ColorMethod = colorMethodType.byEntity;

#else

            // slot
            devDept.Eyeshot.Entities.Region slot = devDept.Eyeshot.Entities.Region.CreateRoundedRectangle(60, 20, 5, true);
            devDept.Eyeshot.Entities.Region circle = devDept.Eyeshot.Entities.Region.CreateCircle(3.6);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(-20, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(40, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            Mesh slotMesh = slot.ExtrudeAsMesh(Vector3D.AxisZ * 5, 0.01, Mesh.natureType.RichSmooth);
            slotMesh.Rotate(Math.PI / 2, Vector3D.AxisZ);
            slotMesh.Color = colors[0];
            slotMesh.ApplyMaterial("wood", textureMappingType.Cubic, 2, 2);
            slotMesh.ColorMethod = colorMethodType.byEntity;

            // triangle
            LinearPath trianglePath = new LinearPath(Point3D.Origin, new Point3D(36, 0, 0), new Point3D(18, 0, 25), Point3D.Origin);
            devDept.Eyeshot.Entities.Region triangleRegion2 = new devDept.Eyeshot.Entities.Region(trianglePath);
            Mesh triangleMesh = triangleRegion2.ExtrudeAsMesh(Vector3D.AxisY * 5, 0.01, Mesh.natureType.RichSmooth);
            triangleMesh.Color = colors[1];
            triangleMesh.ColorMethod = colorMethodType.byEntity;
            triangleMesh.Rotate(Utility.DegToRad(90), Vector3D.AxisZ);
            triangleMesh.Translate(52, -3, 0);

            // weels
            Mesh weelAxisMesh = Mesh.CreateCylinder(3, 65, 50, Mesh.natureType.RichSmooth);
            weelAxisMesh.ApplyMaterial("wood", textureMappingType.Cylindrical, 2, 2);
            weelAxisMesh.Rotate(Math.PI / 2, Vector3D.AxisY);
            weelAxisMesh.Color = colors[2];
            weelAxisMesh.ColorMethod = colorMethodType.byEntity;

            devDept.Eyeshot.Entities.Region outer = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 12);
            devDept.Eyeshot.Entities.Region inner = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 3);
            devDept.Eyeshot.Entities.Region weel = devDept.Eyeshot.Entities.Region.Difference(outer, inner)[0];

            Mesh weelRMesh = weel.ExtrudeAsMesh(10, 0.01, Mesh.natureType.RichSmooth);
            weelRMesh.Translate(55, 0, 0);
            weelRMesh.Color = colors[2];
            weelRMesh.ColorMethod = colorMethodType.byEntity;

            Mesh weelLMesh = weel.ExtrudeAsMesh(-10, 0.01, Mesh.natureType.RichSmooth);
            weelLMesh.Translate(10, 0, 0);
            weelLMesh.Color = colors[2];
            weelLMesh.ColorMethod = colorMethodType.byEntity;

            // cylinder
            Mesh cylMesh = Mesh.CreateCylinder(3.5, 40, 50);
            cylMesh.Color = colors[3];
            cylMesh.ColorMethod = colorMethodType.byEntity;

            //box
            Mesh baseMesh = Mesh.CreateBox(40, 40, 5);
            baseMesh.Color = colors[4];
            baseMesh.ColorMethod = colorMethodType.byEntity;

#endif

            // blocks containing the geometry
            Block baseBlock = new Block("Box");
            baseBlock.Entities.Add(baseMesh);

            Block redTriangleBlock = new Block("Slot");
            redTriangleBlock.Entities.Add(slotMesh);

            Block yellowTriangleBlock = new Block("Triangle");
            yellowTriangleBlock.Entities.Add(triangleMesh);

            Block greenBlock = new Block("Cylinder");
            greenBlock.Entities.Add(cylMesh);

            Block weelsBlock = new Block("weels");
            weelsBlock.Entities.Add(weelAxisMesh);
            weelsBlock.Entities.Add(weelRMesh);
            weelsBlock.Entities.Add(weelLMesh);

            model1.Blocks.Add(baseBlock);
            model1.Blocks.Add(redTriangleBlock);
            model1.Blocks.Add(yellowTriangleBlock);
            model1.Blocks.Add(greenBlock);
            model1.Blocks.Add(weelsBlock);

            // saves entities elements
            foreach (Block b in model1.Blocks)
            {
                // deletes previous entities
                model1.Entities.Clear();

                // adds the entity to the viewport
                BlockReference reference = new BlockReference(b.Name);
                model1.Entities.Add(reference);

                // fits the model in the viewport
                model1.ZoomFit();

                // save image
                Bitmap materialSphere = model1.RenderToBitmap(1);
                materialSphere.Save(dirName + "\\" + b.Name + ".bmp");
            }
            
            // fills ListView with saved images
            this.listView1.ItemsSource = Fill_listView;

            // restores scene
            model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor;
            model1.GetBackground().TopColor = oldColor;
            model1.Viewports[0].GetToolBar().Visible = true;
            model1.GetCoordinateSystemIcon().Visible = true;
            model1.GetOriginSymbol().Visible = true;
            model1.GetViewCubeIcon().Visible = true;
            model1.GetGrid().Visible = true;
            model1.Flat.EdgeThickness = 1;
            model1.Flat.SilhouetteThickness = 2;
            model1.DisplayMode = displayType.Rendered;
        }

        public System.Collections.ObjectModel.ObservableCollection<ImageItem> Fill_listView
        {
            get
            {
                var results = new ObservableCollection<ImageItem>();
                DirectoryInfo dir = new DirectoryInfo(@dirName);
                foreach (FileInfo file in dir.GetFiles())
                {
                    string name = file.Name.Split('.')[0];
                    BitmapImage bitmap = new BitmapImage(new Uri(file.FullName));
                    results.Add(new ImageItem(){ Name = name, Image = bitmap});
                }
                return results;
            }
        }
        #endregion
    }

    public class ImageItem
    {
        public string Name { get; set; }

        public ImageSource Image { get; set; }
    }
}