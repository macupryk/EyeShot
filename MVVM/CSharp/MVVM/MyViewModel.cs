using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Graphics;
using WpfApplication1.Models;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace WpfApplication1
{
    public class MyViewModel : INotifyPropertyChanged
    {
        #region Common properties

        private System.Windows.Media.FontFamily _fontFamily = System.Windows.SystemFonts.CaptionFontFamily;

        public System.Windows.Media.FontFamily FontFamily
        {
            get { return _fontFamily; }
            set
            {
                _fontFamily = value; 
                NotifyPropertyChanged("FontFamily");
            }
        }


        private bool _autoRefresh = true;
        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            set
            {
                _autoRefresh = value;
                NotifyPropertyChanged("AutoRefresh");
            }
        }
        
        public colorThemeType ColorThemeType
        {
            get { return MyBackgroundSettings.ColorTheme; }
            set
            {
                MyBackgroundSettings.ColorTheme = value;
                NotifyPropertyChanged("ColorThemeType");
            }
        }

        private Color _selectionColor = Colors.Yellow;
        public Color SelectionColor
        {
            get { return _selectionColor; }
            set
            {
                _selectionColor = value;
                NotifyPropertyChanged("SelectionColor");
            }
        }

        private displayType _displayType = displayType.Rendered;
        public displayType DisplayType
        {
            get { return _displayType; }
            set
            {
                _displayType = value;
                NotifyPropertyChanged("DisplayType");
            }
        }

        private Visibility _lightingVisibility;

        public Visibility LightingVisibility
        {
            get { return _lightingVisibility; }
            set
            {
                _lightingVisibility = value; 
                NotifyPropertyChanged("LightingVisibility");
            }
        }
        
        private bool _lighting;
        public bool Lighting
        {
            get { return _lighting; }
            set
            {
                _lighting = value;
                MyOriginSymbol.Lighting = value;
                MyCoordinateSystemIcon.Lighting = value;
                MyGrids[0].Lighting = value;

                if (value)
                    LightingVisibility = Visibility.Visible;
                else
                    LightingVisibility = Visibility.Collapsed;

                NotifyPropertyChanged("Lighting");
            }
        }        

        private actionType _actionType = actionType.SelectByPick;
        public actionType ActionType
        {
            get { return _actionType; }
            set
            {
                _actionType = value;
                NotifyPropertyChanged("ActionType");
            }
        }

        #endregion

        #region Legend
        private bool _legendVisible = true;

        public bool LegendVisible
        {
            get { return _legendVisible; }
            set
            {
                _legendVisible = value;
                NotifyPropertyChanged("LegendVisible");
            }
        }

        private string _legendTitle = "Data binding";

        public string LegendTitle
        {
            get { return _legendTitle; }
            set
            {
                _legendTitle = value;
                NotifyPropertyChanged("LegendTitle");
            }
        }

        private string _legendSubtitle = "This sample shows different ways for the data binding.";

        public string LegendSubtitle
        {
            get { return _legendSubtitle; }
            set
            {
                _legendSubtitle = value;
                NotifyPropertyChanged("LegendSubtitle");
            }
        }

        private string _legendItemSize;

        public string LegendItemSize
        {
            get { return _legendItemSize; }
            set
            {
                _legendItemSize = value;
                NotifyPropertyChanged("LegendItemSize");
            }
        }
        
        #endregion

        #region Background

        private BackgroundSettings _myBackgroundSettings;

        public BackgroundSettings MyBackgroundSettings
        {
            get { return _myBackgroundSettings; }
            set { _myBackgroundSettings = value; }
        }        

        public backgroundStyleType BackgroundStyle
        {
            get { return _myBackgroundSettings.StyleMode; }
            set
            {
                _myBackgroundSettings.StyleMode = value;
                NotifyPropertyChanged("BackgroundStyle");

                switch (value)
                {
                    case backgroundStyleType.None:
                        TopColorVisibility = Visibility.Collapsed;
                        BottomColorVisibility = Visibility.Collapsed;
                        IntermediateColorVisibility = Visibility.Collapsed;
                        ImagesVisibility = Visibility.Collapsed;
                        break;
                    case backgroundStyleType.Solid:
                        TopColorVisibility = Visibility.Visible;
                        BottomColorVisibility = Visibility.Collapsed;
                        IntermediateColorVisibility = Visibility.Collapsed;
                        ImagesVisibility = Visibility.Collapsed;
                        break;
                    case backgroundStyleType.LinearGradient:
                        TopColorVisibility = Visibility.Visible;
                        BottomColorVisibility = Visibility.Visible;
                        IntermediateColorVisibility = Visibility.Collapsed;
                        ImagesVisibility = Visibility.Collapsed;
                        break;
                    case backgroundStyleType.CubicGradient:
                        TopColorVisibility = Visibility.Visible;
                        BottomColorVisibility = Visibility.Visible;
                        IntermediateColorVisibility = Visibility.Visible;
                        ImagesVisibility = Visibility.Collapsed;
                        break;
                    case backgroundStyleType.Image:
                        TopColorVisibility = Visibility.Collapsed;
                        BottomColorVisibility = Visibility.Collapsed;
                        IntermediateColorVisibility = Visibility.Collapsed;
                        ImagesVisibility = Visibility.Visible;
                        break;
                }
            }
        }

        #region Background Colors

        public Color TopColor
        {
            get { return Helper.ConvertColor(_myBackgroundSettings.TopColor); }
            set
            {
                _myBackgroundSettings.TopColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("TopColor");
            }
        }

        private Visibility _topColorVisibility;

        public Visibility TopColorVisibility
        {
            get { return _topColorVisibility; }
            set
            {
                _topColorVisibility = value;
                NotifyPropertyChanged("TopColorVisibility");
            }
        }

        public Color BottomColor
        {
            get { return Helper.ConvertColor(_myBackgroundSettings.BottomColor); }
            set
            {
                _myBackgroundSettings.BottomColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("BottomColor");
            }
        }

        private Visibility _bottomColorVisibility;

        public Visibility BottomColorVisibility
        {
            get { return _bottomColorVisibility; }
            set
            {
                _bottomColorVisibility = value;
                NotifyPropertyChanged("BottomColorVisibility");
            }
        }

        public Color IntermediateColor
        {
            get { return Helper.ConvertColor(_myBackgroundSettings.IntermediateColor); }
            set
            {
                _myBackgroundSettings.IntermediateColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("IntermediateColor");
            }
        }

        private Visibility _intermediateColorVisibility;

        public Visibility IntermediateColorVisibility
        {
            get { return _intermediateColorVisibility; }
            set
            {
                _intermediateColorVisibility = value;
                NotifyPropertyChanged("IntermediateColorVisibility");
            }
        }

        public double IntermediateColorPosition
        {
            get { return _myBackgroundSettings.IntermediateColorPosition * 100; }
            set
            {
                _myBackgroundSettings.IntermediateColorPosition = value/100;
                NotifyPropertyChanged("IntermediateColorPosition");
            }
        }

        private Visibility _imagesVisibility;

        public Visibility ImagesVisibility
        {
            get { return _imagesVisibility; }
            set
            {
                _imagesVisibility = value;
                NotifyPropertyChanged("ImagesVisibility");
            }
        }

        #endregion

        public System.Windows.Media.ImageSource Image1 { get; set; }
        public System.Windows.Media.ImageSource Image2 { get; set; }
        public System.Windows.Media.ImageSource Image3 { get; set; }

        #endregion

        #region Toolbar
        private ObservableCollection<ToolBar> _myToolBars;

        public ObservableCollection<ToolBar> MyToolBars
        {
            get { return _myToolBars; }
            set
            {
                _myToolBars = value;
                NotifyPropertyChanged("MyToolBars");
            }
        }        

        private ToolBar MyToolBar { get { return MyToolBars[0]; } }

        public ToolBar.positionType TbPositionType
        {
            get { return MyToolBar.Position; }
            set
            {
                MyToolBar.Position = value;
                NotifyPropertyChanged("TbPositionType");
            }
        }

        public bool TbVisible
        {
            get { return MyToolBar.Visible; }
            set
            {
                MyToolBar.Visible = value;
                NotifyPropertyChanged("TbVisible");
            }
        }

        #region Command
        private int _buttonsCount;        

        public ICommand AddToolbarButtonCommand { get; internal set; }

        private bool CanExecuteAddToolbarButton(object parameter)
        {
            return MyToolBar.Buttons.Count == _buttonsCount;
        }

        private void CreateAddToolbarButtonCommand()
        {
            AddToolbarButtonCommand = new RelayCommand(AddToolbarButtonExecute, CanExecuteAddToolbarButton);
        }

        public void AddToolbarButtonExecute(object parameter)
        {
            BitmapImage gearsBmp = new BitmapImage(Helper.GetUriFromResource("gears.png"));
            MyToolBar.Buttons.Add(new devDept.Eyeshot.ToolBarButton(gearsBmp, "MyCustomButton", "My custom button", devDept.Eyeshot.ToolBarButton.styleType.PushButton, true));
            CustomButtonVisibility = Visibility.Visible;
        }

        public ICommand RemoveToolbarButtonCommand { get; internal set; }

        private bool CanExecuteRemoveToolbarButton(object parameter)
        {
            return MyToolBar.Buttons.Count > _buttonsCount;
        }

        private void CreateRemoveToolbarButtonCommand()
        {
            RemoveToolbarButtonCommand = new RelayCommand(RemoveToolbarButtonExecute, CanExecuteRemoveToolbarButton);
        }

        public void RemoveToolbarButtonExecute(object parameter)
        {            
            MyToolBar.Buttons.RemoveAt(_buttonsCount);
            CustomButtonVisibility = Visibility.Collapsed;
            CustomButtonVisible = true;
        }
        #endregion        

        public bool HomeButtonVisible
        {
            get { return MyToolBar.Buttons[0].Visible; }
            set
            {
                MyToolBar.Buttons[0].Visible = value;
                NotifyPropertyChanged("HomeButtonVisible");
            }
        }

        public bool MagnifyingGlassButtonVisible
        {
            get { return MyToolBar.Buttons[1].Visible; }
            set
            {
                MyToolBar.Buttons[1].Visible = value;
                NotifyPropertyChanged("MagnifyingGlassButtonVisible");
            }
        }

        public bool ZoomWindowButtonVisible
        {
            get { return MyToolBar.Buttons[2].Visible; }
            set
            {
                MyToolBar.Buttons[2].Visible = value;
                NotifyPropertyChanged("ZoomWindowButtonVisible");
            }
        }

        public bool ZoomButtonVisible
        {
            get { return MyToolBar.Buttons[3].Visible; }
            set
            {
                MyToolBar.Buttons[3].Visible = value;
                NotifyPropertyChanged("ZoomButtonVisible");
            }
        }

        public bool PanButtonVisible
        {
            get { return MyToolBar.Buttons[4].Visible; }
            set
            {
                MyToolBar.Buttons[4].Visible = value;
                NotifyPropertyChanged("PanButtonVisible");
            }
        }

        public bool RotateButtonVisible
        {
            get { return MyToolBar.Buttons[5].Visible; }
            set
            {
                MyToolBar.Buttons[5].Visible = value;
                NotifyPropertyChanged("RotateButtonVisible");
            }
        }

        public bool ZoomFitButtonVisible
        {
            get { return MyToolBar.Buttons[6].Visible; }
            set
            {
                MyToolBar.Buttons[6].Visible = value; 
                NotifyPropertyChanged("ZoomFitButtonVisible");
            }
        }

        public bool CustomButtonVisible
        {
            get { return MyToolBar.Buttons.Count == _buttonsCount ? true : MyToolBar.Buttons[7].Visible; }
            set
            {
                if (MyToolBar.Buttons.Count > _buttonsCount)
                    MyToolBar.Buttons[7].Visible = value;
                NotifyPropertyChanged("CustomButtonVisible");
            }
        }

        private Visibility _customButtonVisibility = Visibility.Collapsed;

        public Visibility CustomButtonVisibility
        {
            get { return _customButtonVisibility; }
            set
            {
                _customButtonVisibility = value;
                NotifyPropertyChanged("CustomButtonVisibility");
            }
        }
        
        #endregion

        #region Grid

        private ObservableCollection<Grid> _myGrids;

        public ObservableCollection<Grid> MyGrids
        {
            get { return _myGrids; }
            set
            {
                _myGrids = value;
                NotifyPropertyChanged("MyGrids");
            }
        }

        public bool GridAlwaysBehind
        {
            get { return MyGrids[0].AlwaysBehind; }
            set
            {
                MyGrids[0].AlwaysBehind = value;
                NotifyPropertyChanged("GridAlwaysBehind");

            }
        }

        public double GridStep
        {
            get { return MyGrids[0].Step; }
            set
            {
                MyGrids[0].Step = value;
                NotifyPropertyChanged("GridStep");
            }
        }

        public bool GridAutoSize
        {
            get { return MyGrids[0].AutoSize; }
            set
            {
                MyGrids[0].AutoSize = value;
                NotifyPropertyChanged("GridAutoSize");
            }
        }

        public bool GridAutoStep
        {
            get { return MyGrids[0].AutoStep; }
            set
            {
                MyGrids[0].AutoStep = value;
                NotifyPropertyChanged("GridAutoStep");

                if (value)
                    GridNumberOfLinesVisibility = Visibility.Visible;
                else
                    GridNumberOfLinesVisibility = Visibility.Collapsed;
            }
        }

        private Visibility _gridNumberOfLinesVisibility = Visibility.Collapsed;

        public Visibility GridNumberOfLinesVisibility
        {
            get { return _gridNumberOfLinesVisibility; }
            set
            {
                _gridNumberOfLinesVisibility = value;
                NotifyPropertyChanged("GridNumberOfLinesVisibility");
            }
        }


        public int GridMinNumberOfLines
        {
            get { return MyGrids[0].MinNumberOfLines; }
            set
            {
                MyGrids[0].MinNumberOfLines = value;
                NotifyPropertyChanged("GridMinNumberOfLines");
            }
        }

        public int GridMaxNumberOfLines
        {
            get { return MyGrids[0].MaxNumberOfLines; }
            set
            {
                MyGrids[0].MaxNumberOfLines = value;
                NotifyPropertyChanged("GridMaxNumberOfLines");
            }
        }

        public bool GridVisible
        {
            get { return MyGrids[0].Visible; }
            set
            {
                MyGrids[0].Visible = value;
                NotifyPropertyChanged("GridVisible");
            }
        }

        public Color GridLineColor
        {
            get { return Helper.ConvertColor(MyGrids[0].LineColor); }
            set
            {
                MyGrids[0].LineColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("GridLineColor");
            }
        }

        public Color GridColorAxisX
        {
            get { return Helper.ConvertColor(MyGrids[0].ColorAxisX); }
            set
            {
                MyGrids[0].ColorAxisX = Helper.ConvertColor(value);
                NotifyPropertyChanged("GridColorAxisX");
            }
        }

        public Color GridColorAxisY
        {
            get { return Helper.ConvertColor(MyGrids[0].ColorAxisY); }
            set
            {
                MyGrids[0].ColorAxisY = Helper.ConvertColor(value);
                NotifyPropertyChanged("GridColorAxisY");
            }
        }

        public Color GridFillColor
        {
            get { return Helper.ConvertColor(MyGrids[0].FillColor); }
            set
            {
                MyGrids[0].FillColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("GridFillColor");
            }
        }

        public int GridMajorLinesEvery
        {
            get { return MyGrids[0].MajorLinesEvery; }
            set
            {
                MyGrids[0].MajorLinesEvery = value;
                NotifyPropertyChanged("GridMajorLinesEvery");

            }
        }

        public Color GridMajorLineColor
        {
            get { return Helper.ConvertColor(MyGrids[0].MajorLineColor); }
            set
            {
                MyGrids[0].MajorLineColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("GridMajorLineColor");

            }
        }

        #endregion

        #region Coordinate System Icon

        private CoordinateSystemIcon _myCoordinateSystemIcon;

        public CoordinateSystemIcon MyCoordinateSystemIcon
        {
            get { return _myCoordinateSystemIcon; }
            set
            {
                _myCoordinateSystemIcon = value;
                NotifyPropertyChanged("MyCoordinateSystemIcon");
            }
        }

        public bool CsiVisible
        {
            get { return MyCoordinateSystemIcon.Visible; }
            set
            {
                MyCoordinateSystemIcon.Visible = value;
                NotifyPropertyChanged("CsiVisible");
            }
        }        

        public coordinateSystemPositionType CsiPositionType
        {
            get { return MyCoordinateSystemIcon.Position; }
            set
            {
                MyCoordinateSystemIcon.Position = value;
                NotifyPropertyChanged("CsiPositionType");
            }
        }

        public Color CsiLabelColor
        {
            get { return Helper.ConvertColor(MyCoordinateSystemIcon.LabelColor); }
            set
            {
                MyCoordinateSystemIcon.LabelColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("CsiLabelColor");
            }
        }

        private Visibility _csiArrowVisibility;

        public Visibility CsiArrowVisibility
        {
            get { return _csiArrowVisibility; }
            set
            {
                _csiArrowVisibility = value;
                NotifyPropertyChanged("CsiArrowVisibility");
            }
        }


        public Color CsiArrowColorX
        {
            get { return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorX); }
            set
            {
                MyCoordinateSystemIcon.ArrowColorX = Helper.ConvertColor(value);
                NotifyPropertyChanged("CsiArrowColorX");
            }
        }

        public Color CsiArrowColorY
        {
            get { return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorY); }
            set
            {
                MyCoordinateSystemIcon.ArrowColorY = Helper.ConvertColor(value);
                NotifyPropertyChanged("CsiArrowColorY");
            }
        }

        public Color CsiArrowColorZ
        {
            get { return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorZ); }
            set
            {
                MyCoordinateSystemIcon.ArrowColorZ = Helper.ConvertColor(value);
                NotifyPropertyChanged("CsiArrowColorZ");
            }
        }

        public string CsiLabelX
        {
            get { return MyCoordinateSystemIcon.LabelAxisX; }
            set
            {
                MyCoordinateSystemIcon.LabelAxisX = value;
                NotifyPropertyChanged("CsiLabelX");
            }
        }

        public string CsiLabelY
        {
            get { return MyCoordinateSystemIcon.LabelAxisY; }
            set
            {
                MyCoordinateSystemIcon.LabelAxisY = value;
                NotifyPropertyChanged("CsiLabelY");
            }
        }

        public string CsiLabelZ
        {
            get { return MyCoordinateSystemIcon.LabelAxisZ; }
            set
            {
                MyCoordinateSystemIcon.LabelAxisZ = value;
                NotifyPropertyChanged("CsiLabelZ");
            }
        }

        #endregion

        #region Origin Symbol

        private OriginSymbol _myOriginSymbol;

        public OriginSymbol MyOriginSymbol
        {
            get { return _myOriginSymbol; }
            set
            {
                _myOriginSymbol = value;
                NotifyPropertyChanged("MyOriginSymbol");
            }
        }

        public bool OsVisible
        {
            get { return MyOriginSymbol.Visible; }
            set
            {
                MyOriginSymbol.Visible = value;
                NotifyPropertyChanged("OsVisible");
            }
        }        

        public originSymbolStyleType OsStyleType
        {
            get { return MyOriginSymbol.StyleMode; }
            set
            {
                MyOriginSymbol.StyleMode = value;
                if (value == originSymbolStyleType.CoordinateSystem)
                    OsArrowVisibility = Visibility.Visible;
                else
                    OsArrowVisibility = Visibility.Collapsed;
                NotifyPropertyChanged("OsStyleType");
            }
        }

        public Color OsLabelColor
        {
            get { return Helper.ConvertColor(MyOriginSymbol.LabelColor); }
            set
            {
                MyOriginSymbol.LabelColor = Helper.ConvertColor(value);
                NotifyPropertyChanged("OsLabelColor");
            }
        }

        private Visibility _osArrowVisibility;

        public Visibility OsArrowVisibility
        {
            get { return _osArrowVisibility; }
            set
            {
                _osArrowVisibility = value;
                NotifyPropertyChanged("OsArrowVisibility");
            }
        }


        public Color OsArrowColorX
        {
            get { return Helper.ConvertColor(MyOriginSymbol.ArrowColorX); }
            set
            {
                MyOriginSymbol.ArrowColorX = Helper.ConvertColor(value);
                NotifyPropertyChanged("OsArrowColorX");
            }
        }

        public Color OsArrowColorY
        {
            get { return Helper.ConvertColor(MyOriginSymbol.ArrowColorY); }
            set
            {
                MyOriginSymbol.ArrowColorY = Helper.ConvertColor(value);
                NotifyPropertyChanged("OsArrowColorY");
            }
        }

        public Color OsArrowColorZ
        {
            get { return Helper.ConvertColor(MyOriginSymbol.ArrowColorZ); }
            set
            {
                MyOriginSymbol.ArrowColorZ = Helper.ConvertColor(value);
                NotifyPropertyChanged("OsArrowColorZ");
            }
        }

        public string OsLabelX
        {
            get { return MyOriginSymbol.LabelAxisX; }
            set
            {
                MyOriginSymbol.LabelAxisX = value;
                NotifyPropertyChanged("OsLabelX");
            }
        }

        public string OsLabelY
        {
            get { return MyOriginSymbol.LabelAxisY; }
            set
            {
                MyOriginSymbol.LabelAxisY = value;
                NotifyPropertyChanged("OsLabelY");
            }
        }

        public string OsLabelZ
        {
            get { return MyOriginSymbol.LabelAxisZ; }
            set
            {
                MyOriginSymbol.LabelAxisZ = value;
                NotifyPropertyChanged("OsLabelZ");
            }
        }

        #endregion

        #region ViewCubeIcon 

        private bool _viewCubeVisible = true;       
        public bool ViewCubeVisible
        {
            get { return _viewCubeVisible; }
            set
            {
                _viewCubeVisible = value;
                NotifyPropertyChanged("ViewCubeVisible");
            }
        }

        private bool _viewCubeShowRing = true;
        public bool ViewCubeShowRing
        {
            get { return _viewCubeShowRing; }
            set
            {
                _viewCubeShowRing = value;
                NotifyPropertyChanged("ViewCubeShowRing");
            }
        }

        private Color _viewCubeColor = Color.FromArgb(240, 77, 77, 77);
        public Color ViewCubeColor
        {
            get { return _viewCubeColor; }
            set
            {
                _viewCubeColor = value;
                NotifyPropertyChanged("ViewCubeColor");
            }
        }

        private string _frontLabel = "FRONT";
        public string FrontLabel
        {
            get { return _frontLabel; }
            set
            {
                _frontLabel = value;
                NotifyPropertyChanged("FrontLabel");
            }
        }

        private string _backLabel = "BACK";
        public string BackLabel
        {
            get { return _backLabel; }
            set
            {
                _backLabel = value;
                NotifyPropertyChanged("BackLabel");
            }
        }

        private string _rightLabel = "RIGHT";
        public string RightLabel
        {
            get { return _rightLabel; }
            set
            {
                _rightLabel = value;
                NotifyPropertyChanged("RightLabel");
            }
        }

        private string _leftLabel = "LEFT";
        public string LeftLabel
        {
            get { return _leftLabel; }
            set
            {
                _leftLabel = value;
                NotifyPropertyChanged("LeftLabel");
            }
        }

        private string _topLabel = "TOP";

        public string TopLabel
        {
            get { return _topLabel; }
            set
            {
                _topLabel = value;
                NotifyPropertyChanged("TopLabel");
            }
        }

        private string _bottomLabel = "BOTTOM";
        public string BottomLabel
        {
            get { return _bottomLabel; }
            set
            {
                _bottomLabel = value;
                NotifyPropertyChanged("BottomLabel");
            }
        }

        private Visibility _vcLabelVisibility;

        public Visibility VcLabelVisibility
        {
            get { return _vcLabelVisibility; }
            set
            {
                _vcLabelVisibility = value;
                NotifyPropertyChanged("VcLabelVisibility");
            }
        }

        public System.Windows.Media.ImageSource VcResetImages { get; set; }
        public System.Windows.Media.ImageSource VcImage1 { get; set; }
        public System.Windows.Media.ImageSource VcImage2 { get; set; }

        private System.Windows.Media.ImageSource[] _vcFaceImages = new ImageSource[6];
        public System.Windows.Media.ImageSource[] VcFaceImages
        {
            get { return _vcFaceImages; }
            set
            {
                if (value == null)
                {
                    _vcFaceImages[0] =
                        _vcFaceImages[1] =
                            _vcFaceImages[2] =
                                _vcFaceImages[3] =
                                    _vcFaceImages[4] =
                                        _vcFaceImages[5] = null;

                    VcLabelVisibility = Visibility.Visible;
                }
                else
                {
                    _vcFaceImages = value;

                    VcLabelVisibility = Visibility.Collapsed;
                }

                NotifyPropertyChanged("VcFaceImages");
            }
        }

        #endregion

        #region EntityList
        private MyEntityList _entityList;

        public MyEntityList EntityList
        {
            get { return _entityList; }
            set
            {
                _entityList = value;
                NotifyPropertyChanged("ViewportEntities");
            }
        }            
        #endregion

        private const string Pictures = "../../../../../../dataset/Assets/Pictures/";
        public MyViewModel()
        {           
            // Initializes the Coordinate System Icon for the data binding.
            MyCoordinateSystemIcon = new CoordinateSystemIcon();            

            // Initializes the Origin Symbol for the data binding.
            MyOriginSymbol = OriginSymbol.GetDefaultOriginSymbol();
            OsStyleType = originSymbolStyleType.Ball;

            // Initializes the Grids collection for the data binding.
            Grid grid = new Grid() {Step = 10, MajorLinesEvery = 4};
            MyGrids = new ObservableCollection<Grid> { grid };

            // Initializes the ToolBar for the data binding.
            MyToolBars = new ObservableCollection<ToolBar>(new List<ToolBar>() {ToolBar.GetDefaultToolBar()});
            MyToolBar.Position = ToolBar.positionType.HorizontalTopCenter;
            // Uses toolbar buttons count info to enable/disable the Add/Remove buttons
            _buttonsCount = MyToolBar.Buttons.Count;
            // Creates the command for the the Add/Remove buttons
            CreateAddToolbarButtonCommand();
            CreateRemoveToolbarButtonCommand();

            // Initializes the BackgroundSettings for the the data binding.
            MyBackgroundSettings = new BackgroundSettings(backgroundStyleType.Solid, Helper.ConvertColor("#FF434752"), System.Drawing.Color.White, Helper.ConvertColor("#FFEDEDED"), .75, null);

            // Sets the ViewModel's BackgroundStyle: in this way the "Background" comboboxes will be updated too.
            BackgroundStyle = MyBackgroundSettings.StyleMode;

            
            // Initializes the Background images.
            MyBackgroundSettings.Image = Image1 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background1.jpg"));
            Image2 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background2.jpg"));
            Image3 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background3.jpg"));

            // Initializes the Images for the ViewCube buttons.
            VcResetImages = ViewCubeIcon.GetDefaultViewCubeIcon().FrontImage;            
            VcImage1 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Front.jpg"));
            VcImage2 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Front.jpg"));

            // Initializes the EntitiList collection for the the data binding.
            _entityList = new MyEntityList();            
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));                
            }
        }
        #endregion              
    }
}
