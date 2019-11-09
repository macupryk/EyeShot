using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        [Serializable]
        struct CustomData
        {

            public uint id;
            public float price;
            public string description;

            public CustomData(uint id, float price, string description)
            {
                this.id = id;
                this.price = price;
                this.description = description;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {            
            // hides grid
            model1.GetGrid().Visible = false;

            // Our red line
            Line myLine = new Line(0, 0, 0, 50, 10, 0);

            // The red line custom data
            myLine.EntityData = new CustomData(8321, 6.99f, "Steel wire");


            // Our blue triangle
            Triangle myTri = new Triangle(0, 10, 0, 40, 40, 0, 10, 70, 0);

            // The blue triangle custom data
            myTri.EntityData = new CustomData(9876, 18.99f, "Plastic panel");


            // We add both to the master entity array            
            model1.Entities.Add(myLine, System.Drawing.Color.Red);
            model1.Entities.Add(myTri, System.Drawing.Color.Blue);

            // sets trimetric view            
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport            
            model1.ZoomFit();

            //refresh the model control
            model1.Invalidate();  
         
            base.OnContentRendered(e);
        }     

        private void selectButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectButton.IsChecked != null && selectButton.IsChecked.Value)
                model1.ActionMode = actionType.SelectByPick;
            else
                model1.ActionMode = actionType.None;
        }

        private void clearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            model1.Entities.ClearSelection();
            model1.Invalidate();
        }

        private void model1_SelectionChanged(object sender, Model.SelectionChangedEventArgs e)
        {
            foreach (Entity ent in model1.Entities)
            {
                if (ent.Selected)

                    if (ent.EntityData is CustomData)
                    {

                        CustomData cd = (CustomData)ent.EntityData;

                        MessageBox.Show("ID = " + cd.id +
                                        System.Environment.NewLine +
                                        "Price = $" + cd.price.ToString() +
                                        System.Environment.NewLine +
                                        "Description = " + cd.description, "CustomData");
                    }
            }
        }        
    }
}