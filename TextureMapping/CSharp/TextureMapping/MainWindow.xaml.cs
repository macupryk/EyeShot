using System;
using System.Windows;
using System.Windows.Controls;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public bool Dragging;
        public textureMappingType SelMapping = textureMappingType.Spherical;
        string _selMaterial = "";
        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            
            textBoxU.Text = string.Format("{0:f1}", 1);
            textBoxV.Text = string.Format("{0:f1}", 1);

            model1.InitializeScene += new EventHandler(model1_InitializeScene);
        }

        private void model1_InitializeScene(object sender, EventArgs e)
        {
            // Creates material sphere images and fills the listview
            CreateTexture();

            // Deletes all entities in the scene
            model1.Entities.Clear();

            // Adds entities to display
            Primitives();           

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();         
        }

        void viewport_dragEnter(object sender, DragEventArgs e)
        {
            if (Dragging)
                // shows copy cursor inside the viewport
                e.Effects = DragDropEffects.Copy;
        }

        void viewport_dragDrop(object sender, DragEventArgs e)
        {
            if (Dragging)
            {
                e.Effects = DragDropEffects.None;
                if (_selMaterial != "")
                {
                    // gets target entity
                    int selectedIndex = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint((model1.GetMousePosition(e))));
                    if (selectedIndex != -1)
                    {
                        Entity selEntity = model1.Entities[selectedIndex];
                        if (selEntity != null)
                        {
                            // gets scale u,v value 
                            double u, v;
                            Double.TryParse(this.textBoxU.Text, out u);
                            Double.TryParse(this.textBoxV.Text, out v);

                            // assigns the material to all triangles and maps the material texture with specific mapping
                            ((Mesh)selEntity).ApplyMaterial(_selMaterial, SelMapping, u, v);
                            model1.Entities.Regen();
                        }
                    }
                }
                Dragging = false;
                model1.Invalidate();
            }
        }

        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (_selMaterial != "")
            {
                // shows copy cursor inside the listView
                e.Effects = DragDropEffects.Copy;
                if (!Dragging)
                {
                    Dragging = true;
                    // start a dragdrop action to viewport
                    DragDrop.DoDragDrop(model1, _selMaterial, DragDropEffects.Copy);
                }
            }
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                // saves the selected material to be apply
                _selMaterial = ((ImageItem)this.listView1.SelectedItems[0]).Name;

                // start a dragdrop anction to listView1
                 try
                {    
                     DragDrop.DoDragDrop((ListView)sender, _selMaterial, DragDropEffects.Copy);
                }
                catch { }

                // clear selection
                this.listView1.SelectedItems.Clear();
            }
        }

        private void comboBoxMapping_SelectionChanged(object sender, EventArgs e)
        {
            string mapping = ((ComboBoxItem) comboBoxMapping.SelectedItem).Name.ToString();
            switch (mapping)
            {
                case "Spherical":
                    SelMapping = textureMappingType.Spherical;
                    break;
                case "Cubic":
                    SelMapping = textureMappingType.Cubic;
                    break;
                case "Cylindrical":
                    SelMapping = textureMappingType.Cylindrical;
                    break;
                case "Plate":
                    SelMapping = textureMappingType.Plate;
                    break;
                default:
                    SelMapping = textureMappingType.Spherical;
                    break;
            }
        }

        public void Primitives()
        {
            model1.GetGrid().Step = 25;
            model1.GetGrid().Min.X = -25;
            model1.GetGrid().Min.Y = -25;

            model1.GetGrid().Max.X = 125;
            model1.GetGrid().Max.Y = 175;

            double deltaOffset = 50;
            double offsetX = 0;
            double offsetY = 0;

            System.Drawing.Color color = System.Drawing.Color.SlateGray;

            // First Row

            // Box
            Mesh mesh = Mesh.CreateBox(40, 40, 30);
            mesh.Translate(-20, -20, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // Cone
            mesh = Mesh.CreateCone(20, 10, 30, 30, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // second cone
            mesh = Mesh.CreateCone(20, 0, 30, 30, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // Second Row
            offsetX = 0;
            offsetY += deltaOffset;

            // Cylinder
            mesh = Mesh.CreateCylinder(15, 25, 20, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // prism
            mesh = Mesh.CreateCone(20, 10, 30, 3, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // Sphere
            mesh = Mesh.CreateSphere(20, 3, 3, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);

            // Third Row
            offsetX = 0;
            offsetY += deltaOffset;

            // sphere
            mesh = Mesh.CreateSphere(20, 8, 6, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // second sphere
            mesh = Mesh.CreateSphere(20, 50, 50, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // torus
            mesh = Mesh.CreateTorus(18, 5, 15, 17, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // Fourth Row
            offsetX = 0;
            offsetY += deltaOffset;

            // spring
            mesh = Mesh.CreateSpring(10, 2, 16, 24, 10, 6, true, true, Mesh.natureType.Smooth);
            mesh.EdgeStyle = Mesh.edgeStyleType.None;
            mesh.Translate(offsetX, offsetY, 0);
            model1.Entities.Add(mesh, color);
            offsetX += deltaOffset;

            // Sweep
            double z = 30;
            double radius = 15;

            devDept.Eyeshot.Entities.Line l1 = new devDept.Eyeshot.Entities.Line(0, 0, 0, 0, 0, z);
            Arc a1 = new Arc(new Point3D(radius, 0, z), new Point3D(0, 0, z), new Point3D(radius, 0, z + radius));
            devDept.Eyeshot.Entities.Line l2 = new devDept.Eyeshot.Entities.Line(radius, 0, z + radius, 30, 0, z + radius);

            CompositeCurve composite = new CompositeCurve(l1, a1, l2);
            LinearPath lpOuter = new LinearPath(10, 16);
            LinearPath lpInner = new LinearPath(5, 11);
            lpInner.Translate(2.5, 2.5, 0);
            lpInner.Reverse();

            Region profile = new Region(lpOuter, lpInner);
            mesh = profile.SweepAsMesh(composite, 0.1);
            mesh.Translate(offsetX - 10, offsetY - 8, 0);
            model1.Entities.Add(mesh, color);

            // Hexagon with hole region revolved
            offsetX += deltaOffset;

            LinearPath lp = new LinearPath(7);
            for (int i = 0; i <= 360; i += 60)
                lp.Vertices[i / 60] = new Point3D(10 * Math.Cos(Utility.DegToRad(i)), 10 * Math.Sin(Utility.DegToRad(i)), 0);

            Circle circle = new Circle(new Point3D(0, 0, 0), 7);
            circle.Reverse();
            
            Region profile2 = new Region(lp, circle);
            mesh = profile2.RevolveAsMesh(0, Utility.DegToRad(10), Vector3D.AxisX, new Point3D(0, -60, 0), Utility.NumberOfSegments(60, Math.PI / 6, 0.1), 0.1, Mesh.natureType.Smooth); mesh.FlipNormal();
            mesh.Scale(2, 2, 2);

            mesh.Translate(offsetX, offsetY, 0);
            mesh.EdgeStyle = Mesh.edgeStyleType.Sharp;
            mesh.SmoothingAngle = Utility.DegToRad(59);
            model1.Entities.Add(mesh, color);

        }

    }
}