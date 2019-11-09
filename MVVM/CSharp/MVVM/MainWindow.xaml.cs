using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using WpfApplication1.Models;
using Brush = System.Windows.Media.Brush;
using Image = System.Windows.Controls.Image;
using ToolBar = devDept.Eyeshot.ToolBar;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Pictures = "../../../../../../dataset/Assets/Pictures/";

        private MyViewModel _myViewModel;
        private Random _rand = new Random(123);
        public MainWindow()
        {
            InitializeComponent();

             // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            _myViewModel = (MyViewModel)DataContext;          
            _myViewModel.Lighting = false;            

            foreach (var value in Enum.GetValues(typeof(colorThemeType)))            
                colorThemeTypes.Items.Add(value);

            foreach (var value in Enum.GetValues(typeof(displayType)))
                displayTypes.Items.Add(value);

            foreach (var value in Enum.GetValues(typeof(backgroundStyleType)))
                styles.Items.Add(value);            

            foreach (var value in Enum.GetValues(typeof(coordinateSystemPositionType)))
                csiPositionTypes.Items.Add(value);

            foreach (var value in Enum.GetValues(typeof(originSymbolStyleType)))
                osStyleTypes.Items.Add(value);

            foreach (var value in Enum.GetValues(typeof(ToolBar.positionType)))
                tbPositionTypes.Items.Add(value);

            actionTypes.Items.Add(actionType.None);
            actionTypes.Items.Add(actionType.SelectByPick);
            actionTypes.Items.Add(actionType.SelectByBox);
            actionTypes.Items.Add(actionType.SelectVisibleByPick);
            actionTypes.Items.Add(actionType.SelectVisibleByBox);                   
            
            colors.SelectedIndex = 1;
        }                

        private void BtnAddEntity_OnClick(object sender, RoutedEventArgs e)
        {
            var randomColor = System.Drawing.Color.FromArgb(255, (byte)_rand.Next(255), (byte)_rand.Next(255), (byte)_rand.Next(255));
            var translateX = _rand.Next(100) * -5;
            var translateY = _rand.Next(100) * -5;
            var translateZ = _rand.Next(100) * -5;
#if STANDARD
            var faces = GetBoxFaces();
            foreach (Entity entity in faces)
            {
                entity.Color = randomColor;
                entity.ColorMethod = colorMethodType.byEntity;
                entity.Translate(translateX, translateY, translateZ);
            }
            _myViewModel.EntityList.AddRange(faces);            
#else
            Mesh m = Mesh.CreateBox(50, 50, 50);
            m.Color = randomColor;
            m.ColorMethod = colorMethodType.byEntity;
            m.Translate(translateX, translateY, translateZ);

            _myViewModel.EntityList.Add(m);
#endif
            model1.ZoomFit();
        }

#if STANDARD
        private List<Entity> GetBoxFaces()
        {
            double boxWidth = 50;
            double boxDepth = 50;
            double boxHeight = 50;

            Quad boxBottom = new Quad(0, boxDepth, 0,
                                 boxWidth, boxDepth, 0,
                                 boxWidth, 0, 0,
                                 0, 0, 0);

            Quad boxTop = new Quad(0, boxDepth, boxHeight,
                                 boxWidth, boxDepth, boxHeight,
                                 boxWidth, 0, boxHeight,
                                 0, 0, boxHeight);

            Quad boxFront = new Quad(0, 0, 0,
                                  boxWidth, 0, 0,
                                  boxWidth, 0, boxHeight,
                                  0, 0, boxHeight);

            Quad boxRight = new Quad(boxWidth, 0, 0,
                                boxWidth, boxDepth, 0,
                                boxWidth, boxDepth, boxHeight,
                                boxWidth, 0, boxHeight);

            Quad boxRear = new Quad(boxWidth, boxDepth, 0,
                            0, boxDepth, 0,
                            0, boxDepth, boxHeight,
                            boxWidth, boxDepth, boxHeight);

            Quad boxLeft = new Quad(0, boxDepth, 0,
                    0, 0, 0,
                    0, 0, boxHeight,
                    0, boxDepth, boxHeight);

            return new List<Entity>() { boxBottom, boxTop, boxFront, boxRight, boxRear, boxLeft };            
        }
#endif

        private void BtnRemoveEntities_OnClick(object sender, RoutedEventArgs e)
        {                 
            _myViewModel.EntityList.RemoveRange(_myViewModel.EntityList.Where(x => x.Selected).ToList());
        }

        private void BtnImage_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton) sender;
            bool isChecked = b.IsChecked.Value;
            Image image = (Image) b.Content;

            _myViewModel.MyBackgroundSettings.Image = image.Source;

            btnImage1.IsChecked = false;
            btnImage2.IsChecked = false;
            btnImage3.IsChecked = false;

            b.IsChecked = isChecked;
        }

        private void Colors_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            switch (((ObservableCollection<Brush>)colors.SelectedItem).Count)
            {
                case 9:
                    {
                        _myViewModel.LegendItemSize = "10,30";
                        break;
                    }
                case 17:
                    {
                        _myViewModel.LegendItemSize = "10,25";
                        break;
                    }
                case 33:
                    {
                        _myViewModel.LegendItemSize = "10,15";
                        break;
                    }
            }
        }

#region ViewCube Images

        private void BtnVcImage_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleButton b = (ToggleButton)sender;
            bool isChecked = b.IsChecked.Value;                        

            btnVcResetImages.IsChecked = false;
            btnVcImage1.IsChecked = false;
            btnVcImage2.IsChecked = false;
            
            b.IsChecked = isChecked;
        }        
        
        private void BtnVcResetImages_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_myViewModel != null)
                _myViewModel.VcFaceImages = null;
        }

        private void BtnVcImage1_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_myViewModel != null)
                _myViewModel.VcFaceImages = new ImageSource[] {
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Front.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Back.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Top.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Bottom.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Left.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Right.jpg"))};
        }

        private void BtnVcImage2_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_myViewModel != null)
                _myViewModel.VcFaceImages = new ImageSource[] {
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Front.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Back.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Top.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Bottom.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Left.jpg")),
                    RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Right.jpg"))};
        }

#endregion
    }
}