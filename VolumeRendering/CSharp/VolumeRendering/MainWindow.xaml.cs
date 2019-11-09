using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Linq;
using devDept.Eyeshot;
using devDept.Eyeshot.Dicom;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using MouseButton = System.Windows.Input.MouseButton;
using Point = System.Windows.Point;
using devDept.Eyeshot.Triangulation;
using devDept.Eyeshot.Translators;
using Environment = devDept.Eyeshot.Environment;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MyVolumeRendering _volumeRendering;
        private List<string> _files;
        private DicomElement _currentSeries;
        private IodElement _currentElement;
        private DicomTree _dicomTree;
        private Dictionary<string, string> _isoValueDictionary;
        private List<HounsfieldColorTable> _hounsfieldColors;
        private string _scansDir;
        private Interval _picturesInterval;
        private Interval _loadedInderval;
        private int _windowWidth, _windowCenter;
        private bool _playingSlices;
        private bool _viewportIsWorking; 

        ObservableCollection<ListViewModelItem> Layers { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.     

            // Event handlers  
            model1.KeyDown += model1_KeyDown;
            model1.WorkCancelled += model1_WorkCancelled;
            model1.WorkCompleted += model1_WorkCompleted;
            model1.SelectionChanged += model1_SelectionChanged; 
            model1.MeasureCompleted += model1_MeasureCompleted;

            model1.AnimateCamera = true;
        }        

        protected override void OnContentRendered(EventArgs e)
        {
            model1.Units = linearUnitsType.Millimeters;
            model1.Camera.ProjectionMode = projectionType.Orthographic;            

            model1.DisplayMode = displayType.Rendered;
            model1.Rendered.ShowEdges = false;
            model1.ClippingPlane1.Capping = false;

            CheckScansFolder();

            SetPath(_scansDir);

            FillHounsfieldColors();
            UpdateLayerListView();
            FillComboIsoValue();

            if (_currentElement != null)
            {
                if (trackBarFirstSlice.Maximum >= 360)
                {
                    // Initializes for PHENIX sample scan.
                    trackBarFirstSlice.Value = 130;
                    trackBarLastSlice.Value = 260;
                }
            }
            else
            {
                SetEnable(false);
            }

            model1.Invalidate();

            base.OnContentRendered(e);         
        }

        #region Model event handlers
        private void model1_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.Key == Key.Delete)
            {
                if (model1.Layers.Count > 1)
                {
                    List<string> emptyLayers = new List<string>();
                    for (int i = 1; i < model1.Layers.Count; i++)
                    {
                        string name = model1.Layers[i].Name;
                        if (model1.Entities.Count(x => x.LayerName == name) == 0 && !emptyLayers.Contains(name))
                            emptyLayers.Add(name);
                    }
                    foreach (var emptyLayer in emptyLayers)
                    {
                        model1.Layers.Remove(emptyLayer);
                    }
                }
                UpdateLayerListView();
            }
        }

        private void model1_MeasureCompleted(object sender, EventArgs eventArgs)
        {
            rdBtnNone.IsChecked = true;
        }

        private void model1_SelectionChanged(object sender, Model.SelectionChangedEventArgs selectionChangedEventArgs)
        {
            btnSplitMeshes.IsEnabled = btnSmoothMeshes.IsEnabled = model1.Entities.Count(x => x.Selected) > 0;
        }

        private void model1_WorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            if (e.WorkUnit is WriteSTL)
                ShowExportedMessage(StlFile);

            _viewportIsWorking = false;
            SetEnable();
        }

        private void model1_WorkCancelled(object sender, EventArgs e)
        {
            _viewportIsWorking = false;
            SetEnable();
        }
        #endregion

        #region Helper
        private void Init()
        {
            if (String.IsNullOrEmpty(_path)
                || !Directory.Exists(_path))
                return;

            _currentElement = null;
            _currentSeries = null;
            ResetImage();


            // Gets all files sorted by name, excluding some extensions
            string[] excludeExts = { "zip", "rar", "7z", "txt", "xml" };
            _files = FilterFiles(_path, excludeExts).ToList();

            // Initializes trackbars values
            _picturesInterval = new Interval(0, _files.Count - 1);
            trackBarFirstSlice.Minimum = trackBarLastSlice.Minimum = (int)_picturesInterval.Min;
            trackBarFirstSlice.Maximum = trackBarLastSlice.Maximum = (int)_picturesInterval.Max;
            trackBarCurrentImage.Minimum = trackBarFirstSlice.Value;
            trackBarCurrentImage.Maximum = trackBarLastSlice.Value;

            lblTrkBarFirstSliceEnd.Content = lblTrkBarLastSliceEnd.Content = _picturesInterval.Max.ToString();

            Cursor = Cursors.Wait;
            _dicomTree = new DicomTree(_files.ToArray());
            // Sets default cursor
            Cursor = null;

            if (String.IsNullOrEmpty(_dicomTree.Log))
                txtErrors.Text = String.Empty;
            else
                txtErrors.Text = _dicomTree.Log;

            FillTreeView(_dicomTree);
            FillComboSeries(_dicomTree);

            LoadFile((int)trackBarCurrentImage.Value);
        }

        // Gets all files for a given path, excluding some extensions
        private IEnumerable<string> FilterFiles(string path, params string[] exts)
        {
            return
                Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(file => exts.All(x => !file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(f => f);
        }

        // Enables or disables controls to prevent unsuitable end-user usages.                
        private void SetEnable(bool enable = true)
        {
            if (_viewportIsWorking || _playingSlices)
            {
                grpVolumeRendering.IsEnabled = false;                
                btnExport.IsEnabled = false;
                cmbCurrentSeries.IsEnabled = false;
                txtPath.IsEnabled = false;
                btnPath.IsEnabled = false;
            }
            else
            {
                grpVolumeRendering.IsEnabled = enable;                
                btnExport.IsEnabled = enable;
                cmbCurrentSeries.IsEnabled = enable;

                if (_currentElement == null)
                {
                    txtPath.IsEnabled = true;
                    btnPath.IsEnabled = true;
                    SelectionRectangle = Rectangle.Empty;
                }
                else
                {
                    txtPath.IsEnabled = enable;
                    btnPath.IsEnabled = enable;
                }
            }

            if (_playingSlices)
            {
                grpWindowLevel.IsEnabled = false;
                grpIsoLevel.IsEnabled = false;
                btnSelectArea.IsEnabled = false;
            }
            else
            {
                grpWindowLevel.IsEnabled = enable;
                grpIsoLevel.IsEnabled = enable;
                btnSelectArea.IsEnabled = enable;
            }

            if (_currentElement != null && !_currentElement.IsSupportedDicomFile())
            {
                btnPlaySlices.IsEnabled = false;
                btnStopSlices.IsEnabled = false;
                btnSelectArea.IsEnabled = false;
            }
            else
            {
                btnPlaySlices.IsEnabled = enable;
                btnStopSlices.IsEnabled = enable;
            }

            grpActions.IsEnabled = enable;
            
            tabPageSlices.IsEnabled = enable;
            tabPageDicomTree.IsEnabled = enable;
            tabPageDetails.IsEnabled = enable;
            tabPageErrors.IsEnabled = true;

        }
        
        private System.Windows.Media.Color ConvertColor(System.Drawing.Color drawingColor)
        {
            return Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        private System.Drawing.Color ConvertColor(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        #endregion 

        #region Scans
        private void CheckScansFolder()
        {
            string prjPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            if (String.IsNullOrEmpty(prjPath))
                throw new EyeshotException("Unable to get the local path!");

            _scansDir = System.IO.Path.Combine(prjPath, "Scans");
            if (!Directory.Exists(_scansDir))
                Directory.CreateDirectory(_scansDir);

        }
       
        private void txtDownloadScans_LinkClicked(object sender, RequestNavigateEventArgs e)
        {            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        #endregion

        #region Series
        private void FillComboSeries(DicomTree tree)
        {            
            List<DicomElement> sources = new List<DicomElement>();

            foreach (var elem in tree.Tree)
            {
                List<DicomElement> series = GetDicomSeries(elem);
                if (series != null)
                    sources.AddRange(series);
            }

            cmbCurrentSeries.ItemsSource = sources;

            if (cmbCurrentSeries.Items.Count > 0 && cmbCurrentSeries.SelectedItem != cmbCurrentSeries.Items[0])
                cmbCurrentSeries.SelectedItem = cmbCurrentSeries.Items[0];
        }

        private void SelectSeries(DicomElement dicomElement)
        {
            if (dicomElement == null)
                return;

            _currentSeries = dicomElement;
            _volumeRendering = null;

            trackBarCurrentImage.Value = trackBarCurrentImage.Minimum = trackBarFirstSlice.Value = 0;
            trackBarLastSlice.Value = trackBarFirstSlice.Maximum = trackBarLastSlice.Maximum = trackBarCurrentImage.Maximum = _currentSeries.Elements.Count - 1;

            lblCurrentImageFirst.Content = "0";
            lblCurrentImageLast.Content = (_currentSeries.Elements.Count - 1).ToString();


            lblTrkBarFirstSliceStart.Content = lblTrkBarLastSliceStart.Content = "0";
            lblTrkBarFirstSliceEnd.Content = lblTrkBarLastSliceEnd.Content = lblLastSliceValue.Content = trackBarFirstSlice.Maximum.ToString();

            lblFirstSliceValue.Content = trackBarCurrentImage.Value.ToString();

            lblWindowCenterValue.Content = String.Empty;
            lblWindowWidthValue.Content = String.Empty;

            ResetImage();
            LoadFile((int)trackBarCurrentImage.Value);
        }

        private List<DicomElement> GetDicomSeries(DicomElement elem)
        {
            if (elem.DicomNode == DicomElement.dicomNodeType.Instance)
                return null;

            List<DicomElement> result = new List<DicomElement>();
            DicomElement study = null;
            while (study == null)
            {
                if (elem.Elements.Count == 0)
                    break;

                if (elem.DicomNode == DicomElement.dicomNodeType.Study)
                {
                    study = elem;
                }
                else
                {
                    elem = elem.Elements[0];
                }

            }

            if (study != null)
                result = study.Elements;

            return result;
        }

        private void cmbCurrentSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectSeries((DicomElement)cmbCurrentSeries.SelectedItem);
        }
        #endregion        

        #region Hounsfield
        private void FillHounsfieldColors()
        {
            _hounsfieldColors = new List<HounsfieldColorTable>();

            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Air", FromValue = -1000, ToValue = -1000, Color = System.Drawing.Color.White });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Skin", FromValue = -999, ToValue = -501, Color = System.Drawing.Color.FromArgb(229, 182, 162) });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Lung", FromValue = -500, ToValue = -500, Color = System.Drawing.Color.BlueViolet });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Fat", FromValue = -100, ToValue = -50, Color = System.Drawing.Color.AntiqueWhite });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Water", FromValue = 0, ToValue = 0, Color = System.Drawing.Color.Aqua });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Cerebrospinal fluid", FromValue = 15, ToValue = 15, Color = System.Drawing.Color.FromArgb(235, 199, 147) });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Kidney", FromValue = 30, ToValue = 30, Color = System.Drawing.Color.FromArgb(160, 45, 60) });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Blood", FromValue = 31, ToValue = 45, Color = System.Drawing.Color.Crimson });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Muscle", FromValue = 10, ToValue = 40, Color = System.Drawing.Color.OrangeRed });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Grey matter", FromValue = 37, ToValue = 45, Color = System.Drawing.Color.DarkGray });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "White matter", FromValue = 20, ToValue = 30, Color = System.Drawing.Color.LightGray });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Liver", FromValue = 10, ToValue = 40, Color = System.Drawing.Color.RosyBrown });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Soft Tissue, Contrast", FromValue = 100, ToValue = 300, Color = System.Drawing.Color.HotPink });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Tooth", FromValue = 1600, ToValue = 1900, Color = System.Drawing.Color.FromArgb(241, 232, 223) });
            _hounsfieldColors.Add(new HounsfieldColorTable { Description = "Bone", FromValue = 500, ToValue = 3000, Color = System.Drawing.Color.FromArgb(232, 221, 199) });
        }
        
        private System.Drawing.Color GetColorByIsoLevel(int isoLevel)
        {
            var hc = _hounsfieldColors.Where(x => x.FromValue <= isoLevel && x.ToValue >= isoLevel).FirstOrDefault();
            if (hc != null)
                return hc.Color;
            
            return System.Drawing.Color.Beige;
        }
        #endregion

        #region Iso Level
        private void FillComboIsoValue()
        {
            _isoValueDictionary = new Dictionary<string, string>()
            {                                                
                {"Blood", "35"},   
                {"Bone", "500"},   
                {"Fat", "-85"},                
                {"Lung", "-500"},                
                {"Muscle", "25"},                
                {"Skin", "-800"},    
                {"Tooth", "1800"},
                {"",""}
            };
            
            cmbIsoLevel.ItemsSource = _isoValueDictionary;
            cmbIsoLevel.DisplayMemberPath = "Key";
            cmbIsoLevel.SelectedIndex = 1;
        }

        private void GetIsoValue(Point location)
        {        
            if (_drawingSelection
                || _currentSeries == null
                || trackBarCurrentImage.Value >= _currentSeries.Elements.Count)
            {
                return;
            }

            int y = (int)location.Y;
            int row = Math.Max(0, (y - 1));
            int column = (int)location.X;

            IodElement el = (IodElement)_currentSeries.Elements[(int)trackBarCurrentImage.Value];
            txtIsoLevel.Text = el.GetHounsfieldPixelValue(row, column).ToString();
        }

        private bool _selectingIsoValue;
        private void cmbIsoLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIsoLevel.SelectedIndex > -1 && cmbIsoLevel.SelectedIndex != (_isoValueDictionary.Count - 1))
            {
                _selectingIsoValue = true;
                txtIsoLevel.Text = ((KeyValuePair<string, string>)cmbIsoLevel.SelectedItem).Value;
                int isoLevel;
                int.TryParse(txtIsoLevel.Text, out isoLevel);                
                layerColorPicker.SelectedColor = ConvertColor(GetColorByIsoLevel(isoLevel));
                txtLayerName.Text = ((KeyValuePair<string, string>)cmbIsoLevel.SelectedItem).Key;
                _selectingIsoValue = false;
            }
        }

        private void txtIsoLevel_TextChanged(object sender, EventArgs e)
        {
            if (_selectingIsoValue) return;

            if (_isoValueDictionary.ContainsValue(txtIsoLevel.Text))
            {
                cmbIsoLevel.SelectedItem = _isoValueDictionary.FirstOrDefault(x => x.Value == txtIsoLevel.Text);
            }
            else
            {
                if (!String.IsNullOrEmpty(cmbIsoLevel.Text))
                    cmbIsoLevel.Text = "";

                txtLayerName.Text = "Iso-" + txtIsoLevel.Text;
            }
        }
        #endregion        

        #region Slices
        private int GetElementIndex(IodElement element)
        {
            return _currentSeries.Elements.Cast<IodElement>().ToList().FindIndex(x => x.SliceInfo.InstanceNumber == element.SliceInfo.InstanceNumber);
        }

        private void LoadFile(int i)
        {
            string fileName;
            _wLDeltaPoint = new Point();
            _wLChangeValWidth = 0.5;
            _wLChangeValCentre = 20.0;
            _rightMouseDown = false;

            try
            {
                if (_currentSeries == null || _currentSeries.Elements.Count <= i)
                {
                    SetEnable(false);
                    UpdateImage();
                    return;
                }

                SetCurrentElement((IodElement)_currentSeries.Elements[i]);
                if (String.IsNullOrEmpty(lblWindowCenterValue.Content.ToString()))
                    SetWindowCenter(_currentElement.GetWindowCenter());
                if (String.IsNullOrEmpty(lblWindowWidthValue.Content.ToString()))
                    SetWindowWidth(_currentElement.GetWindowWidth());

                fileName = System.IO.Path.GetFileName(_currentElement.Tag.FileName) + "    (" + i + " / " +
                           (_currentSeries.Elements.Count - 1) + ")";

                lblFilenameValue.Content = fileName;

                SetEnable(true);

                UpdateImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int originalSourceHeight;
        private int originalSourceWidth;

        private void UpdateImage()
        {
            if (_currentElement == null)
            {
                ResetImage();

                txtDownloadScans.Visibility = Visibility.Visible;
                canvasPictureBox1.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtDownloadScans.Visibility = Visibility.Collapsed;
                canvasPictureBox1.Visibility = Visibility.Visible;

                DicomVersion dicomVersion = _currentElement.GetDicomVersion();
                string imageErrorMsg = String.Format(
                        "Unable to load the image. Transfer Syntax: {0} Dicom Version: {1}",
                        _currentElement.GetTransferSyntax(), dicomVersion);

                try
                {
                    if (!_currentElement.IsSupportedDicomFile())
                    {
                        ResetImage();
                        txtErrors.AppendText(imageErrorMsg + System.Environment.NewLine);
                        tabControlBottom.SelectedIndex = 3;
                        SelectionRectangle = Rectangle.Empty;
                    }
                    else
                    {
                        if (SelectionRectangle.IsEmpty && imagePictureBox1.Source != null)
                        {
                            int x = 60;
                            int y = 65;                            
                            int width = 400 + x > imagePictureBox1.Source.Width ? (int)imagePictureBox1.Source.Width - x : 400;
                            int height = 270 + y > imagePictureBox1.Source.Height ? (int)imagePictureBox1.Source.Height - y : 270;
                            
                            SelectionRectangle = new Rectangle(x, y, width, height);
                            //SelectionRectangle = new Rectangle(0, 0, 512, 512);//full    
                        }

                        if (tabControlBottom.SelectedIndex == 3)
                            tabControlBottom.SelectedIndex = 0;

                        Bitmap bmp = _currentElement.GetBitmap(_windowCenter, _windowWidth);
                        if (bmp == null) return;

                        originalSourceWidth = bmp.Width;
                        originalSourceHeight = bmp.Height;

                        bmp = new Bitmap(bmp, new System.Drawing.Size(512,512));

                        imagePictureBox1.Source = RenderContextUtility.ConvertImage((Bitmap)bmp.Clone());                        
                    }
                }
                catch
                {
                    ResetImage();
                    txtErrors.AppendText(imageErrorMsg + System.Environment.NewLine);

                    // Shows Errors tab page.
                    tabControlBottom.SelectedIndex = 3;
                    SelectionRectangle = Rectangle.Empty;
                }


            }
        }
        
        private void SetCurrentElement(IodElement element)
        {
            _currentElement = element;
            FillSlicesDetailsTree(_currentElement);
        }

        private void SetWindowCenter(int value)
        {
            _windowCenter = value;
            lblWindowCenterValue.Content = _windowCenter.ToString();
        }

        private void SetWindowWidth(int value)
        {
            _windowWidth = value;
            lblWindowWidthValue.Content = _windowWidth.ToString();
        }

        private void btnAddSlice_Click(object sender, EventArgs e)
        {
            AddSlices((IodElement)_currentSeries.Elements[(int)trackBarCurrentImage.Value]);
        }

        private void AddSlices(IodElement element = null)
        {
            EntityList imgList = new EntityList();

            _picturesInterval = new Interval(trackBarFirstSlice.Value, trackBarLastSlice.Value);

            List<DicomElement> dicomElements;

            if (element != null)
                dicomElements = new List<DicomElement> { element };
            else
                dicomElements = _currentSeries.Elements.GetRange((int)_picturesInterval.Min,
                (int)(_picturesInterval.Max - _picturesInterval.Min + 1));

            LinearPath rectangle = null;

            foreach (var el in dicomElements)
            {
                IodElement iodElement = (IodElement)el;

                float spaceInX, spaceInY;
                iodElement.GetPixelSpacing(out spaceInY, out spaceInX);

                Bitmap bitmap = iodElement.GetBitmap(_windowCenter, _windowWidth);

                Picture pic = new Picture(Plane.XY, iodElement.SliceInfo.Columns, iodElement.SliceInfo.Rows, bitmap);
                // Picture will be not involved in lighting
                pic.Lighted = false;
                pic.Scale(spaceInX, spaceInY, 0);

                Point3D basePoint = new devDept.Geometry.Point3D(iodElement.SliceInfo.ImageUpperLeftX,
                    iodElement.SliceInfo.ImageUpperLeftY - (iodElement.GetRows() * spaceInY), iodElement.SliceInfo.ImageUpperLeftZ);

                pic.Translate(basePoint.X, basePoint.Y, basePoint.Z);

                imgList.Add(pic);

                if (rectangle == null)
                {
                    pic.Regen(.001);
                    rectangle = new LinearPath(pic.Vertices);
                }
            }

            bool zoomFit = model1.Entities.Count == 0;
            model1.Entities.AddRange(imgList);
            model1.Invalidate();
            if (zoomFit)
                model1.ZoomFit();
        }
        #endregion
     
        #region Dicom Tree
        private void FillTreeView(DicomTree tree)
        {
            treeDicom.Items.Clear();

            foreach (DicomElement element in tree.Tree)
            {                
                TreeNode node = new TreeNode(element.Header);
                node.Tag = element;
                treeDicom.Items.Add(node);

                AddChildren(element.Elements, node);
            }
        }

        private void FillSlicesDetailsTree(IodElement element)
        {
            treeSliceDetails.Items.Clear();
            
            TreeNode node = new TreeNode(_currentElement.Header);

            foreach (XElement xe in element.Tag.XDocument.Descendants("DataSet").First().Elements("DataElement"))
                AddDICOMAttributeToString(node, xe);

            treeSliceDetails.Items.Add(node);
            node.IsExpanded = true;
        }

        // Helper method to add one DICOM attribute to the DICOM Tag Tree.
        private void AddDICOMAttributeToString(TreeNode parent, XElement theXElement)
        {
            string aTag = theXElement.Attribute("Tag").Value;
            string aTagName = theXElement.Attribute("TagName").Value;
            string aTagData = theXElement.Attribute("Data").Value;

            // Enrich the Transfer Syntax attribute (0002,0010) with human-readable string from dictionary
            if (aTag.Equals("(0002,0010)"))
                aTagData = string.Format("{0} ({1})", aTagData, TransferSyntaxDictionary.GetTransferSyntaxName(aTagData));

            // Enrich the SOP Class UID attribute (0008,0016) with human-readable string from dictionary
            if (aTag.Equals("(0008,0016)"))
                aTagData = string.Format("{0} ({1})", aTagData, SopClassDictionary.GetSopClassName(aTagData));

            string s = string.Format("{0} {1}", aTag, aTagName);

            // Do some cut-off in order to align the TagData
            if (s.Length > 50)
                s = s.Remove(50);
            else
                s = s.PadRight(50);

            s = string.Format("{0} {1}", s, aTagData);
            
            TreeNode node = new TreeNode(s);
            parent.Items.Add(node);

            // In case the DICOM attributes has childrens (= Sequence), call the helper method recursively.
            if (theXElement.HasElements)
                foreach (XElement xe in theXElement.Elements("DataElement"))
                    AddDICOMAttributeToString(node, xe);
        }

        private static void AddChildren(List<DicomElement> elements, TreeNode parentNode)
        {
            if (elements == null)
                return;

            foreach (DicomElement el in elements)
            {
                TreeNode childNode = new TreeNode(el.Header);
                childNode.Tag = el;
                parentNode.Items.Add(childNode);

                AddChildren(el.Elements, childNode);
            }
        }

        private void CollapseAll(TreeView tv)
        {
            foreach (TreeNode i in tv.Items)
            {
                i.IsExpanded = false;
            }
        }

        private void SearchNodeInTree(TreeView tv, IodElement element)
        {
            tv.IsHitTestVisible = true;  

            CollapseAll(tv);

            ItemCollection tnc = tv.Items;

            foreach (TreeNode tn in tnc)
            {
                if (tn.Items.Count > 0)
                {
                    tn.IsExpanded = true;
                    SearchNodeInNodes(tn, element);
                }

                if (ReferenceEquals(element, tn.Tag))
                {
                    tn.IsSelected = true;
                    return;
                }
            }            
        }

        private void SearchNodeInNodes(TreeNode parentTn, IodElement element)
        {
            ObservableCollection<TreeNode> tnc = parentTn.Items;

            foreach (TreeNode tn in tnc)
            {
                if (tn.Items.Count > 0)
                {
                    tn.IsExpanded = true;
                    SearchNodeInNodes(tn, element);
                }

                if (ReferenceEquals(element, tn.Tag))
                {
                    tn.IsSelected = true;
                    return;
                }
            }            
        }

        private void treeDicom_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode selectedNode = (TreeNode) e.NewValue;                            
            if (selectedNode == null)
                return;

            if (selectedNode.Tag is IodElement)
            {
                IodElement element = (IodElement)selectedNode.Tag;
                if (element != null)
                {
                    if (!element.Parent.Equals(_currentSeries))
                    {
                        SelectSeries(element.Parent);
                    }

                    if (!element.Equals(_currentElement))
                        LoadFile(GetElementIndex(element));
                }
            }
            else if (selectedNode.Tag is DicomElement)
            {
                DicomElement element = (DicomElement)selectedNode.Tag;
                if (element.DicomNode == DicomElement.dicomNodeType.Series)
                {
                    if (!element.Equals(_currentSeries))
                    {
                        SelectSeries(element);
                    }
                }
            }

            int idx = GetElementIndex(_currentElement);

            if (idx < trackBarCurrentImage.Minimum)
                trackBarCurrentImage.Minimum = idx;

            if (idx > trackBarCurrentImage.Maximum)
                trackBarCurrentImage.Maximum = idx;

            trackBarCurrentImage.Value = idx;
        }
        #endregion                

        #region Volume Rendering

        private void btnAddVolume_Click(object sender, System.EventArgs e)
        {            
            if (SelectionRectangle.IsEmpty)
                MessageBox.Show("Select the rectangle in the picture box to compute the mesh.", "Choose Area");

            _picturesInterval = new Interval(trackBarFirstSlice.Value, trackBarLastSlice.Value);

            int isoLevel;
            int.TryParse(txtIsoLevel.Text, out isoLevel);

            model1.Focus();

            DoMarchingCubes(isoLevel);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            model1.Clear();
            UpdateLayerListView();
            model1.Invalidate();
        }

        private void DoMarchingCubes(int isoLevel)
        {
            if (_loadedInderval.Min != _picturesInterval.Min || _loadedInderval.Max != _picturesInterval.Max)
            {
                _loadedInderval = _picturesInterval;

                txtIsoLevel.Text = isoLevel.ToString();
            }

            GC.Collect();


            // Initialize marching cube algorithm            
            if (_currentSeries == null)
                return;

            List<DicomElement> dicomElements = _currentSeries.Elements.GetRange((int)_picturesInterval.Min,
                (int)(_picturesInterval.Max - _picturesInterval.Min + 1));


            double widthRatio = originalSourceWidth * 1.0 / imagePictureBox1.ActualWidth,
                    heightRatio =originalSourceHeight * 1.0 / imagePictureBox1.ActualHeight;

            int x = (int)(SelectionRectangle.Location.X * widthRatio);
            int y = (int)(SelectionRectangle.Location.Y * heightRatio);
            int width = (int)(SelectionRectangle.Width * widthRatio);
            int height = (int)(SelectionRectangle.Height * heightRatio);

            //if (imagePictureBox1.Height < imagePictureBox1.Source.Height)
            //{
            //    // Recomputes the selected rectangle considering the scale factor.
            //    double zoomFactor = (double)imagePictureBox1.Source.Height / imagePictureBox1.Height;
            //    double offsetX = SelectionRectangle.X - (double)(imagePictureBox1.Source.Height - imagePictureBox1.Height) / 2;

            //    if (offsetX < 0)
            //    {
            //        width = Convert.ToInt32(width - offsetX);
            //        x = 0;
            //    }
            //    else
            //    {
            //        x = (int)(zoomFactor * offsetX);
            //    }

            //    y = (int)(zoomFactor * SelectionRectangle.Y);
            //    if (y > imagePictureBox1.Source.Width)
            //        y = Convert.ToInt32(imagePictureBox1.Source.Width);

            //    width = Convert.ToInt32(width * zoomFactor);
            //    if (width > imagePictureBox1.Source.Width)
            //        width = Convert.ToInt32(imagePictureBox1.Source.Width);

            //    height = Convert.ToInt32(height * zoomFactor);
            //    if (height > imagePictureBox1.Source.Height)
            //        height = Convert.ToInt32(imagePictureBox1.Source.Height);

            //}

            _volumeRendering = new MyVolumeRendering(dicomElements, new Point3D(x, y, 0), width, height, (int)_picturesInterval.Length + 1); //+1 because the elements array starts from 0
            _volumeRendering.LightWeight = chkPreview.IsChecked.Value;


            _volumeRendering.IsoLevel = isoLevel;
            
            Layer layer = model1.Layers[0];
            string layerName = txtLayerName.Text;
            if (!model1.Layers.Contains(layerName) && !String.IsNullOrEmpty(layerName))
            {                
                layer = new Layer(layerName, ConvertColor(layerColorPicker.SelectedColor.Value));
                model1.Layers.Add(layer);
                UpdateLayerListView();
            }
            else
                layer = model1.Layers[layerName];

            _volumeRendering.Layer = layer;

            _viewportIsWorking = true;
            SetEnable();
            model1.StartWork(_volumeRendering);
        }
        #endregion
  
        #region PictureBox
        private bool _drawingSelection;

        // For Window Level 
        Point _wLDeltaPoint;
        int _wLDeltaX;
        int _wLDeltaY;
        double _wLChangeValWidth;
        double _wLChangeValCentre;
        bool _rightMouseDown;
     
        private void btnSelectArea_Click(object sender, System.EventArgs e)
        {
            _drawingSelection = true;
            StartDrawingSelection();
        }

        private Rectangle _selectionRectangle;
        private Rectangle SelectionRectangle
        {
            get { return _selectionRectangle; }
            set
            {
                _selectionRectangle = value;
                if (_selectionRectangle.IsEmpty)
                    dragSelectionBorder.Visibility = Visibility.Hidden;
                else
                {
                    dragSelectionBorder.Visibility = Visibility.Visible;
                    DrawSelectionRect(_selectionRectangle);
                }
            }
        }

        private Point _firstPt;
        private Point _secondPt;

        private bool _dragging = false;

        //private bool _drawingSelection;
        public void StartDrawingSelection()
        {
            _drawingSelection = true;
            canvasPictureBox1.Cursor = Cursors.Cross;
        }

        private void StopDrawingSelection()
        {
            _drawingSelection = false;
            // Sets default cursor
            canvasPictureBox1.Cursor = null;            
        }

        private Point GetPositionForSelection(MouseEventArgs e)
        {
            return e.GetPosition(canvasPictureBox1);
        }

        private void pictureGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point location = GetPositionForSelection(e);

            if (location.X > imagePictureBox1.ActualWidth || location.Y > imagePictureBox1.ActualHeight)
                return;

            if (_drawingSelection)
            {
                if (_dragging == false)
                {
                    _firstPt = location;
                    _dragging = true;
                }
            }
            else if (imagePictureBox1.Source != null)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    _wLDeltaPoint.X = location.X;
                    _wLDeltaPoint.Y = location.Y;
                    _rightMouseDown = true;
                    canvasPictureBox1.Cursor = Cursors.Hand;
                }
                else
                {
                    GetIsoValue(location);
                }
            }

            base.OnMouseDown(e);
        }        

        /// <summary>
        /// Event raised when the user releases the left mouse-button.
        /// </summary>
        private void pictureGrid_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragging)
            {
                _secondPt = GetPositionForSelection(e);
                _dragging = false;
                StopDrawingSelection();
            }
            else if (_rightMouseDown)
            {
                _rightMouseDown = false;

                // Sets default cursor
                canvasPictureBox1.Cursor = null;
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Event raised when the user moves the mouse button.
        /// </summary>
        private void pictureGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point location = GetPositionForSelection(e);
            if (_drawingSelection)
            {
                if (location.X < imagePictureBox1.Source.Width && location.Y < imagePictureBox1.Source.Height)
                    canvasPictureBox1.Cursor = Cursors.Cross;
                else
                    canvasPictureBox1.Cursor = null;

                if (_dragging)
                {
                    _secondPt = location;
                    if (_secondPt.X > imagePictureBox1.Source.Width)
                        _secondPt.X = imagePictureBox1.Source.Width - 2;
                    if (_secondPt.Y > imagePictureBox1.Source.Height)
                        _secondPt.Y = imagePictureBox1.Source.Height - 2;
                    if (_secondPt.X < 0)
                        _secondPt.X = 0;
                    if (_secondPt.Y < 0)
                        _secondPt.Y = 0;

                    UpdateDragSelectionRect(_firstPt, _secondPt);
                }
            }            
            else if (_rightMouseDown)
            {
                DetermineMouseSensitivity();

                _wLDeltaX = (int)((_wLDeltaPoint.X - location.X) * _wLChangeValWidth);
                _wLDeltaY = (int)((_wLDeltaPoint.Y - location.Y) * _wLChangeValCentre);

                SetWindowCenter(_windowCenter - _wLDeltaY);
                SetWindowWidth(_windowWidth - _wLDeltaX);

                _wLDeltaPoint.X = location.X;
                _wLDeltaPoint.Y = location.Y;

                UpdateImage();
            }

            base.OnMouseMove(e);
        }        

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x = _firstPt.X;
            double y = _firstPt.Y;
            double width = _secondPt.X - x;
            double height = _secondPt.Y - y;

            if (_secondPt.X < _firstPt.X)
            {
                x = _secondPt.X;
                width = _firstPt.X - x;
            }

            if (_secondPt.Y < _firstPt.Y)
            {
                y = _secondPt.Y;
                height = _firstPt.Y - y;
            }

            _selectionRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);

            DrawSelectionRect(_selectionRectangle);


        }

        private void DrawSelectionRect(Rectangle rec)
        {
            // Updates the coordinates of the rectangle used for drag selection.            
            Canvas.SetLeft(dragSelectionBorder, rec.X);
            Canvas.SetTop(dragSelectionBorder, rec.Y);
            dragSelectionBorder.Width = rec.Width;
            dragSelectionBorder.Height = rec.Height;            
        }

        private void ResetImage()
        {
            imagePictureBox1.Source = null;
            SelectionRectangle = Rectangle.Empty;
        }

        private void btnReset_Click(object sender, System.EventArgs e)
        {
            SetWindowCenter(_currentElement.GetWindowCenter());
            SetWindowWidth(_currentElement.GetWindowWidth());
            UpdateImage();
        }

        // Modifies the 'sensitivity' of the mouse based on the current window width        
        private void DetermineMouseSensitivity()
        {
            if (_windowWidth < 10)
                _wLChangeValWidth = 0.1;
            else if (_windowWidth >= 20000)
                _wLChangeValWidth = 40;
            else
                _wLChangeValWidth = 0.1 + (_windowWidth / 300.0);

            _wLChangeValCentre = _wLChangeValWidth;
        }                  
        #endregion        

        #region TrackBars
        private void trackBarFirstSlice_ValueChanged(object sender, System.EventArgs e)
        {
            lblFirstSliceValue.Content = lblCurrentImageFirst.Content = trackBarFirstSlice.Value.ToString();

            if (trackBarFirstSlice.Value > trackBarLastSlice.Value)
            {
                trackBarCurrentImage.Maximum = trackBarFirstSlice.Value;
                trackBarCurrentImage.Value = trackBarLastSlice.Value = trackBarFirstSlice.Value;
                lblLastSliceValue.Content = lblCurrentImageLast.Content = trackBarLastSlice.Value.ToString();
            }

            if (trackBarCurrentImage.Value < trackBarFirstSlice.Value)

                trackBarCurrentImage.Value = trackBarFirstSlice.Value;


            trackBarCurrentImage.Minimum = trackBarFirstSlice.Value;
        }

        private void trackBarLastSlice_ValueChanged(object sender, EventArgs e)
        {
            lblLastSliceValue.Content = lblCurrentImageLast.Content = trackBarLastSlice.Value.ToString();

            if (trackBarLastSlice.Value < trackBarFirstSlice.Value)
            {
                trackBarCurrentImage.Minimum = trackBarLastSlice.Value;
                trackBarCurrentImage.Value = trackBarFirstSlice.Value = trackBarLastSlice.Value;
                lblFirstSliceValue.Content = lblCurrentImageFirst.Content = trackBarFirstSlice.Value.ToString();
            }

            if (trackBarCurrentImage.Value > trackBarLastSlice.Value)

                trackBarCurrentImage.Value = trackBarLastSlice.Value;

            trackBarCurrentImage.Maximum = trackBarLastSlice.Value;
        }

        private void trackBarCurrentImage_ValueChanged(object sender, EventArgs e)
        {
            LoadFile((int)trackBarCurrentImage.Value);

            lblCurrentImageValue.Content = trackBarCurrentImage.Value.ToString();
        }

        private DispatcherTimer _slicesTimer;
        private void btnPlaySlices_Click(object sender, RoutedEventArgs e)
        {
            if (_playingSlices)
            {
                btnPlaySlices.IsChecked = true;
                return;
            }
            pictureGrid.IsEnabled = false;
            _playingSlices = true;
            SetEnable();
            btnSelectArea.IsEnabled = false;
            _slicesTimer = new DispatcherTimer();
            _slicesTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _slicesTimer.Tick += _slicesTimer_Tick;
            _slicesTimer.IsEnabled = true;
        }

        private void _slicesTimer_Tick(object sender, EventArgs e)
        {
            if (trackBarCurrentImage.Value == trackBarCurrentImage.Maximum)
                trackBarCurrentImage.Value = trackBarCurrentImage.Minimum;
            trackBarCurrentImage.Value++;
        }

        private void btnStopSlices_Click(object sender, RoutedEventArgs e)
        {
            if (_slicesTimer == null) return;

            _slicesTimer.IsEnabled = false;
            _slicesTimer = null;
            _playingSlices = false;
            btnSelectArea.IsEnabled = true;
            btnPlaySlices.IsChecked = false;
            SetEnable();
            pictureGrid.IsEnabled = true;
        }

        private void tabControl1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!model1.IsLoaded || _currentElement == null) return;

            if (!((TabItem)tabControlBottom.Items[tabControlBottom.SelectedIndex]).IsEnabled)
                return;

            switch (tabControlBottom.SelectedIndex)
            {
                case 0:// Slices
                    int idx = GetElementIndex(_currentElement);
                    if (trackBarFirstSlice.Value > idx)
                        trackBarFirstSlice.Value = idx;

                    if (trackBarLastSlice.Value < idx)
                        trackBarLastSlice.Value = idx;

                    trackBarCurrentImage.Value = idx;
                    break;
                case 1:// Dicom Tree
                    SearchNodeInTree(treeDicom, _currentElement);
                    treeDicom.Focus();
                    break;
                case 2:// Slice Details
                    FillSlicesDetailsTree(_currentElement);
                    break;
            }
        }
        #endregion

        #region Path
        private string _path;

        private void SetPath(string path)
        {
            if (path.Equals(_path, StringComparison.InvariantCultureIgnoreCase))
                return;

            _path = path;
            txtPath.Text = _path;

            Init();
        }

        private void btnPath_OnClick(object sender, RoutedEventArgs e)
        {            
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SetPath(dialog.SelectedPath);
            }
        }

        private void txtPath_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SetPath(txtPath.Text);
        }
        #endregion        

        #region Layers
        void UpdateLayerListView()
        {
            model1.Layers[0].Name = "Slices";
            model1.Layers[0].Color = System.Drawing.Color.WhiteSmoke;
            layerListView.Items.Clear();
            Layers = new ObservableCollection<ListViewModelItem>();

            for (int i = 0; i < model1.Layers.Count; i++)
            {
                Layer la = model1.Layers[i];
                ListViewModelItem lvi = new ListViewModelItem(la);
                Layers.Add(lvi);

                layerListView.Items.Add(lvi);

            }            
        }

        private void layerListView_ItemChecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = (ListViewModelItem)cb.DataContext;
            
            if (item.IsChecked)
                model1.Layers.TurnOn(item.LayerName);

            else
                model1.Layers.TurnOff(item.LayerName);

            // updates bounding box, shadow and transparency
            model1.Entities.UpdateBoundingBox();

            model1.Invalidate();
        }
        #endregion        

        #region Actions

        private void rdBtnNone_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (model1 == null)
                return;

            if (rdBtnNone.IsChecked.Value)
            {
                model1.ActionMode = actionType.None;
                model1.Focus();
            }
        }

        private void rdBtnSelect_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (rdBtnSelect.IsChecked.Value)
            {                
                model1.ActionMode = actionType.SelectVisibleByPick;
                model1.Focus();
            }            
        }

        private void btnSplitMeshes_Click(object sender, RoutedEventArgs e)
        {            
            Cursor = Cursors.Wait;

            List<Mesh> meshToRomove = new List<Mesh>();
            Dictionary<string, List<Mesh>> meshToAdd = new Dictionary<string, List<Mesh>>();
            foreach (var ent in model1.Entities.Where(x => x.Selected == true))
            {
                if (ent is Mesh)
                {                                   
                    // For each selected mesh, cleans the triangles from the duplicated vertices and creates a new mesh
                    Mesh m = (Mesh)ent;                    
                    List<IndexTriangle> cleanedTriangles;
                    List<Point3D> uniqueVertices;
                    Utility.CleanTriangles(m.Triangles, m.Vertices, out cleanedTriangles, out uniqueVertices);
                    Mesh newMesh = new Mesh(uniqueVertices, cleanedTriangles);

                    // Divides the new mesh into separate objects meshes.
                    var meshes = newMesh.SplitDisjoint();

                    if (meshes.Length < 2)
                        continue;

                    // Creates list of new meshes that will be add to the original layer
                    string layerName = m.LayerName;
                    List<Mesh> newMeshes = new List<Mesh>();
                    if (meshToAdd.ContainsKey(layerName))
                        newMeshes = meshToAdd[layerName];
                    else
                        meshToAdd.Add(layerName, newMeshes);

                    foreach (Mesh mesh in meshes)
                    {
                        // Sets the NormalAveragingMode equal to the original mesh
                        mesh.NormalAveragingMode = m.NormalAveragingMode;
                        newMeshes.Add(mesh);
                    }

                    meshToAdd[layerName] = newMeshes;

                    meshToRomove.Add(m);
                }
            }

            foreach (var pair in meshToAdd)
                model1.Entities.AddRange(pair.Value, pair.Key);

            foreach (var mesh in meshToRomove)
                model1.Entities.Remove(mesh);
            
            model1.Invalidate();            

            Cursor = null;            
        }

        private void btnSmoothMeshes_Click(object sender, RoutedEventArgs e)
        {        
            Cursor = Cursors.Wait;
         
            foreach (var ent in model1.Entities.Where(x => x.Selected == true))
            {
                Mesh m = (Mesh)ent;

                // For each selected mesh generated with the "Preview" option, generates a smoothed mesh.
                if (m != null && m.LightWeight == true)
                {
                    // Converts the Triangles into SmoothTriangles
                    List<IndexTriangle> cleanedTrianlges;
                    List<Point3D> uniqueVerices;
                    Utility.CleanTriangles(m.Triangles, m.Vertices, out cleanedTrianlges, out uniqueVerices);                                        
                    SmoothTriangle[] smoothTriangles = new SmoothTriangle[cleanedTrianlges.Count];
                    for (int i = 0; i < cleanedTrianlges.Count; i++)
                    {
                        IndexTriangle triangle = cleanedTrianlges[i];
                        smoothTriangles[i] = new SmoothTriangle(triangle.V1, triangle.V2, triangle.V3);
                    }

                    // Sets the LightWeight property to false
                    m.LightWeight = false;
                    // Assigns the new arrays
                    m.Triangles = smoothTriangles;
                    m.Vertices = uniqueVerices.ToArray();
                    // Updates the normals to get a smoothed mesh.
                    m.UpdateNormals();
                }
            }
            
            model1.Entities.Regen();
            model1.Invalidate();

            Cursor = null;            
        }

        private void btnInvertSelection_Click(object sender, RoutedEventArgs e)
        {
            model1.Entities.InvertSelection();

            model1.Invalidate();

            model1.Focus();
        }

        private void rdBtnMeasure_CheckedChanged(object sender, RoutedEventArgs e)
        {
            model1.Measure(rdBtnMeasure.IsChecked.Value);
        }

        private bool _editingClippingPlane;
        private void rdBtnClip_CheckedChanged(object sender, RoutedEventArgs e)
        {           
            if (rdBtnClip.IsChecked.Value)
            {
                if (!_editingClippingPlane)
                {
                    btnAddSection.IsEnabled = true;

                    _editingClippingPlane = true;

                    model1.ActionMode = actionType.None;

                    // sets the Z coordinate of the origin of the clippingPlane
                    model1.ClippingPlane1.Plane.Origin.Z = ((model1.Entities.BoxMin +
                                                                      model1.Entities.BoxMax) / 2).Z;
                    // enables a clippingPlane                           
                    model1.ClippingPlane1.Edit(null);
                }

                model1.Focus();
            }
            else
            {
                // disables the clippingPlane and its change
                model1.ClippingPlane1.Cancel();

                _editingClippingPlane = false;

                btnAddSection.IsEnabled = false;
            }

            model1.Invalidate();
        }

        private void btnAddSection_Click(object sender, RoutedEventArgs e)
        {
            if (!rdBtnClip.IsChecked.Value) return;

            for (int i = 0; i < model1.Entities.Count; i++)
            {
                Entity entity = model1.Entities[i];
                Layer layer = model1.Layers[entity.LayerName];

                if (layer.Visible && entity is Mesh)
                {
                    Mesh m = (Mesh)entity;

                    string layerName = String.Format("{0}Section", layer.Name);                    
                    if (!model1.Layers.Contains(layerName))
                    {
                        Layer newLayer = new Layer(layerName, layer.Color);
                        model1.Layers.Add(newLayer);
                        UpdateLayerListView();                        
                    }

                    ICurve[] curves = m.Section(model1.ClippingPlane1.Plane, 0);

                    foreach (Entity curve in curves)
                    {
                        model1.Entities.Add(curve, layerName);
                    }
                }
            }

            model1.Invalidate();
            model1.Focus();
        }
        #endregion        

        #region Export to STL
        private const string StlFile = "Dicom.stl";

        private void btnExport_OnClick(object sender, RoutedEventArgs e)
        {
            _viewportIsWorking = true;
            SetEnable(false);
            WriteSTL ws = new WriteSTL(new WriteParams(model1), StlFile);
            model1.StartWork(ws);
        }
        private void ShowExportedMessage(string filename)
        {
            string fullPath = String.Format(@"{0}\{1}", System.Environment.CurrentDirectory, filename);
            MessageBox.Show(String.Format("File saved in {0}", fullPath));
            model1.Focus();
        }
        #endregion

        #region Save to XML
        private const string DicomTreeXmlFile = "DicomTree.xml";        
        private void treeDicom_SaveToXml(object sender, RoutedEventArgs e)
        {
            var doc = new XDocument();

            foreach (var element in _dicomTree.Tree)
            {
                XElement el = XmlAddDicomElements(element);
                doc.Add(el);
            }

            using (var xml = new XmlTextWriter(DicomTreeXmlFile, Encoding.ASCII) { Formatting = Formatting.Indented })
            {
                doc.Save(xml);
            }

            ShowExportedMessage(DicomTreeXmlFile);
        }

        private XElement XmlAddDicomElements(DicomElement dicomElement)
        {
            XElement el = new XElement("DicomElement");
            el.Add(new XAttribute("Name", dicomElement.ToString()));

            if (dicomElement.Elements != null)
                foreach (DicomElement element in dicomElement.Elements)
                    el.Add(XmlAddDicomElements(element));

            return el;
        }

        private const string SliceXmlFile = "Slice.xml";

        private void treeSliceDetails_SaveToXml(object sender, RoutedEventArgs e)
        {
            var doc = new XDocument(_currentElement.Tag.XDocument);

            foreach (XElement element in doc.Descendants("DataSet").First().Elements("DataElement"))
                XmlRemoveAttributes(element, new List<string>() { "Tag", "TagName", "Data" });

            using (var xml = new XmlTextWriter(SliceXmlFile, Encoding.ASCII) { Formatting = Formatting.Indented })
            {
                doc.Save(xml);
            }

            ShowExportedMessage(SliceXmlFile);
        }

        private void XmlRemoveAttributes(XElement theXElement, List<string> skippedAttributes)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            foreach (string att in skippedAttributes)
                attributes.Add(att, theXElement.Attribute(att).Value);

            theXElement.RemoveAttributes();

            foreach (var pair in attributes)
                theXElement.Add(new XAttribute(pair.Key, pair.Value));

            if (theXElement.HasElements)
                foreach (XElement xe in theXElement.Elements("DataElement"))
                    XmlRemoveAttributes(xe, skippedAttributes);
        }
        #endregion                
    }

    class HounsfieldColorTable
    {
        public String Description { get; set; }
        public int FromValue { get; set; }
        public int ToValue { get; set; }
        public System.Drawing.Color Color { get; set; }
    }

    class MyVolumeRendering : VolumeRendering
    {
        public Layer Layer { get; set; }

        public MyVolumeRendering(IList<DicomElement> elements, Point3D gridOrigin, int nCellsInX, int nCellsInY, int nCellsInZ, ScalarField3D func = null)
            : base(elements, gridOrigin, nCellsInX, nCellsInY, nCellsInZ, func)
        {
        }


        protected override void WorkCompleted(Environment model)
        {
            base.WorkCompleted(model);

            if (Result != null)
            {
                Result.NormalAveragingMode = Mesh.normalAveragingType.Averaged;
                Result.ColorMethod = colorMethodType.byLayer;
                model.Entities.Add(Result, Layer.Name);
            }

            model.ZoomFit();
        }
    }    


    /// <summary>    
    /// This class represent the Model for Layers List.
    /// </summary>    
    class ListViewModelItem
    {
        public ListViewModelItem(Layer layer)
        {
            Layer = layer;
            IsChecked = layer.Visible;
            ForeColor = RenderContextUtility.ConvertColor(Layer.Color);
        }

        public Layer Layer { get; set; }

        public string LayerName { get { return Layer.Name; } }

        public string ColorName { get { return Layer.Color.Name; } }

        public Brush ForeColor { get; set; }

        public bool IsChecked { get; set; }
    }

    /// <summary>
    /// In the XAML markup, I have specified a HierarchicalDataTemplate for the ItemTemplate of the TreeView.
    /// This class represent the ViewModel for TreeView's Items.
    /// </summary>
    class TreeNode : FrameworkElement
    {
        public TreeNode()
        {
            Items = new ObservableCollection<TreeNode>();
        }

        public TreeNode(string text)
            : this()
        {
            Text = text;
        }

        public string Text { get; set; }        

        public ObservableCollection<TreeNode> Items { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof (bool), typeof (TreeNode), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof (bool), typeof (TreeNode), new PropertyMetadata(default(bool)));

        public bool IsExpanded
        {
            get { return (bool) GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }        
    }
}