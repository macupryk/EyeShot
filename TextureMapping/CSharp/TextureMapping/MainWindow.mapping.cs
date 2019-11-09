using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.IO;
using System.Collections.ObjectModel;

namespace WpfApplication1
{
    partial class MainWindow
    {
        private const string Textures = "../../../../../../dataset/Assets/Textures/";
        readonly string dirName = "myPictures";

        protected void CreateTexture()
        {
            // setting scene for saving
            model1.GetBackground().StyleMode = backgroundStyleType.Solid;
            backgroundStyleType oldStyle = model1.GetBackground().StyleMode;
            model1.Backface.ColorMethod = backfaceColorMethodType.SingleColor;
            System.Windows.Media.Brush oldColor = model1.GetBackground().TopColor;
            model1.GetBackground().TopColor = new SolidColorBrush(Colors.White);
            model1.Shaded.ShadowMode = shadowType.None;
            model1.Viewports[0].GetToolBar().Visible = false;
            model1.GetCoordinateSystemIcon().Visible = false;
            model1.Rendered.PlanarReflections = false;
            model1.GetOriginSymbol().Visible = false;
            model1.GetViewCubeIcon().Visible = false;

            // sets rendered only display mode
            model1.DisplayMode = displayType.Rendered;

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // defines a new material using a texture
            MaterialKeyedCollection list = new MaterialKeyedCollection();

            Material mat = new Material("Cherry", new Bitmap(Textures + "Cherry.jpg"));
            list.Add(mat);
            mat = new Material("Bricks", new Bitmap(Textures + "Bricks.jpg"));
            list.Add(mat);
            mat = new Material("Maple", new Bitmap(Textures + "Maple.jpg"));
            list.Add(mat);
            mat = new Material("Floor", new Bitmap(Textures + "floor_color_map.jpg"));
            list.Add(mat);
            mat = new Material("Wenge", new Bitmap(Textures + "Wenge.jpg"));
            list.Add(mat);
            mat = new Material("Marble", new Bitmap(Textures + "marble.jpg"));
            list.Add(mat);
            mat = Material.Chrome;
            mat.Environment = 0.7f;
            mat.Name = "Chrome";
            list.Add(mat);
            mat = Material.Emerald;
            mat.Name = "Glass";
            list.Add(mat);
            mat = Material.Gold;
            mat.Environment = 0.05f;
            mat.Name = "Gold";
            list.Add(mat);
            mat = new Material("Strips", new Bitmap(Textures + "strips.png"));
            mat.Diffuse = System.Drawing.Color.FromArgb(254, System.Drawing.Color.White);
            list.Add(mat);

            // creates the directory to save material elements
            if (!System.IO.Directory.Exists(dirName))
                System.IO.Directory.CreateDirectory(dirName);
            else
            {   // deletes all previous files
                foreach (string filePath in System.IO.Directory.GetFiles(dirName))
                    System.IO.File.Delete(filePath);
            }

            // saves material elements
            foreach (Material m in list)
            {
                CreateMaterialSphere(m);
            }

            // fills ListView with previous saved images
            this.listView1.ItemsSource = Fill_listView;

            // restores scene
            model1.GetBackground().StyleMode = oldStyle;
            model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor;
            model1.GetBackground().TopColor = oldColor;
            model1.Viewports[0].GetToolBar().Visible = true;
            model1.GetCoordinateSystemIcon().Visible = true;
            model1.Rendered.PlanarReflections = true;
            model1.GetOriginSymbol().Visible = true;
            model1.GetViewCubeIcon().Visible = true;
        }

        public void CreateMaterialSphere(Material mat)
        {

            // adds the material to the viewport's master material collection
            model1.Materials.Add(mat);

            // Creates a new RichMesh sphere with earth radius, slices and stacks
            Mesh rm = Mesh.CreateSphere(6000, 36, 18, Mesh.natureType.RichSmooth);

            // assigns the material to all triangles and maps the material texture spherically
            rm.ApplyMaterial(mat.Name, textureMappingType.Spherical, 1, 1);

            // deletes previous entities
            model1.Entities.Clear();

            // adds the mesh to the viewport
            model1.Entities.Add(rm);

            // fits the model in the viewport
            model1.ZoomFit();

            // save image
            Bitmap materialSphere = model1.RenderToBitmap(1);
            materialSphere.Save(dirName + "\\" + mat.Name + ".bmp");
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
    }

    public class ImageItem
    {
        public string Name { get; set; }

        public ImageSource Image { get; set; }
    }
}
