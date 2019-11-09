Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Navigation
Imports System.Windows.Threading
Imports System.Xml.Linq
Imports System.Text
Imports System.Xml
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Dicom
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports Brush = System.Windows.Media.Brush
Imports Color = System.Windows.Media.Color
Imports Cursors = System.Windows.Input.Cursors
Imports MouseButton = System.Windows.Input.MouseButton
Imports Point = System.Windows.Point
Imports devDept.Eyeshot.Triangulation
Imports devDept.Eyeshot.Translators


''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Private _volumeRendering As MyVolumeRendering
    Private _files As List(Of String)
    Private _currentSeries As DicomElement
    Private _currentElement As IodElement
    Private _dicomTree As DicomTree
    Private _isoValueDictionary As Dictionary(Of String, String)
    Private _hounsfieldColors As List(Of HounsfieldColorTable)
    Private _scansDir As String
    Private _picturesInterval As Interval
    Private _loadedInderval As Interval
    Private _windowWidth As Integer, _windowCenter As Integer
    Private _playingSlices As Boolean
    Private _viewportIsWorking As Boolean

    Private Property Layers() As ObservableCollection(Of ListViewModelItem)
        Get
            Return m_Layers
        End Get
        Set(value As ObservableCollection(Of ListViewModelItem))
            m_Layers = value
        End Set
    End Property
    Private m_Layers As ObservableCollection(Of ListViewModelItem)

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.     
        ' Event handlers  
        AddHandler model1.KeyDown, AddressOf model1_KeyDown
        AddHandler model1.WorkCancelled, AddressOf model1_WorkCancelled
        AddHandler model1.WorkCompleted, AddressOf model1_WorkCompleted
        AddHandler model1.SelectionChanged, AddressOf model1_SelectionChanged
        AddHandler model1.MeasureCompleted, AddressOf model1_MeasureCompleted

        model1.AnimateCamera = True
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        model1.Units = linearUnitsType.Millimeters
        model1.Camera.ProjectionMode = projectionType.Orthographic

        model1.DisplayMode = displayType.Rendered
        model1.Rendered.ShowEdges = False
        model1.ClippingPlane1.Capping = False

        CheckScansFolder()

        SetPath(_scansDir)

        FillHounsfieldColors()
        UpdateLayerListView()
        FillComboIsoValue()

        If _currentElement IsNot Nothing Then
            If trackBarFirstSlice.Maximum >= 360 Then
                ' Initializes for PHENIX sample scan.
                trackBarFirstSlice.Value = 130
                trackBarLastSlice.Value = 260
            End If
        Else
            SetEnable(False)
        End If

        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

#Region "Model event handlers"
    Private Sub model1_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Delete Then
            If model1.Layers.Count > 1 Then
                Dim emptyLayers As New List(Of String)()
	            For i As Integer = 1 To model1.Layers.Count - 1
                    Dim index As Integer = i
		            Dim name As String = model1.Layers(i).Name                    
		            If model1.Entities.Where(Function(x) x.LayerName = name).Count = 0 AndAlso Not emptyLayers.Contains(name) Then
			            emptyLayers.Add(name)
		            End If
	            Next
	            For Each emptyLayer As String In emptyLayers
		            model1.Layers.Remove(emptyLayer)
	            Next
            End If
            UpdateLayerListView()
        End If
    End Sub

    Private Sub model1_MeasureCompleted(sender As Object, eventArgs As EventArgs)
        rdBtnNone.IsChecked = True
    End Sub

    Private Sub model1_SelectionChanged(sender As Object, selectionChangedEventArgs As Model.SelectionChangedEventArgs)
        Dim value As Boolean = model1.Entities.Where(Function(x) x.Selected).Count > 0
        btnSplitMeshes.IsEnabled = value
        btnSmoothMeshes.IsEnabled = value
    End Sub

    Private Sub model1_WorkCompleted(sender As Object, e As WorkCompletedEventArgs)
        If TypeOf e.WorkUnit Is WriteSTL Then
            ShowExportedMessage(StlFile)
        End If
        _viewportIsWorking = False
        SetEnable()
    End Sub

    Private Sub model1_WorkCancelled(sender As Object, e As EventArgs)
        _viewportIsWorking = False
        SetEnable()
    End Sub
#End Region

#Region "Helper"
    Private Sub Init()
        If [String].IsNullOrEmpty(_path) OrElse Not Directory.Exists(_path) Then
            Return
        End If

        _currentElement = Nothing
        _currentSeries = Nothing
        ResetImage()

        ' Gets all files sorted by name, excluding some extensions            
        Dim excludeExts As String() = New String() {"zip", "rar", "7z", "txt", "xml"}
        _files = FilterFiles(_path, excludeExts).ToList()

        ' Initializes trackbars values
        _picturesInterval = New Interval(0, _files.Count - 1)
        trackBarFirstSlice.Minimum = InlineAssignHelper(trackBarLastSlice.Minimum, CInt(_picturesInterval.Min))
        trackBarFirstSlice.Maximum = InlineAssignHelper(trackBarLastSlice.Maximum, CInt(_picturesInterval.Max))
        trackBarCurrentImage.Minimum = trackBarFirstSlice.Value
        trackBarCurrentImage.Maximum = trackBarLastSlice.Value

        lblTrkBarFirstSliceEnd.Content = InlineAssignHelper(lblTrkBarLastSliceEnd.Content, _picturesInterval.Max.ToString())

        Cursor = Cursors.Wait
        _dicomTree = New DicomTree(_files.ToArray())
        ' Sets default cursor
        Cursor = Nothing

        If [String].IsNullOrEmpty(_dicomTree.Log) Then
            txtErrors.Text = [String].Empty
        Else
            txtErrors.Text = _dicomTree.Log
        End If

        FillTreeView(_dicomTree)
        FillComboSeries(_dicomTree)

        LoadFile(CInt(trackBarCurrentImage.Value))
    End Sub

    ' Gets all files for a given path, excluding some extensions
    Private Function FilterFiles(path As String, ParamArray exts As String()) As IEnumerable(Of String)
	    Return Directory. _
                EnumerateFiles(path, "*.*", SearchOption.AllDirectories) _
                .Where(Function(file) exts.All(Function(x) Not file.EndsWith(x, StringComparison.OrdinalIgnoreCase))) _
                .OrderBy(Function(f) f)
    End Function

    ' Enables or disables controls to prevent unsuitable end-user usages.                
    Private Sub SetEnable(Optional enable As Boolean = True)
        If _viewportIsWorking OrElse _playingSlices Then
            grpVolumeRendering.IsEnabled = False
            btnExport.IsEnabled = False
            cmbCurrentSeries.IsEnabled = False
            txtPath.IsEnabled = False
            btnPath.IsEnabled = False
        Else
            grpVolumeRendering.IsEnabled = enable
            btnExport.IsEnabled = enable
            cmbCurrentSeries.IsEnabled = enable

            If _currentElement Is Nothing Then
                txtPath.IsEnabled = True
                btnPath.IsEnabled = True
                SelectionRectangle = Rectangle.Empty
            Else
                txtPath.IsEnabled = enable
                btnPath.IsEnabled = enable
            End If
        End If

        If _playingSlices Then
            grpWindowLevel.IsEnabled = False
            grpIsoLevel.IsEnabled = False
            btnSelectArea.IsEnabled = False
        Else
            grpWindowLevel.IsEnabled = enable
            grpIsoLevel.IsEnabled = enable
            btnSelectArea.IsEnabled = enable
        End If

        If _currentElement IsNot Nothing AndAlso Not _currentElement.IsSupportedDicomFile() Then
            btnPlaySlices.IsEnabled = False
            btnStopSlices.IsEnabled = False
            btnSelectArea.IsEnabled = False
        Else
            btnPlaySlices.IsEnabled = enable
            btnStopSlices.IsEnabled = enable
        End If

        grpActions.IsEnabled = enable

        tabPageSlices.IsEnabled = enable
        tabPageDicomTree.IsEnabled = enable
        tabPageDetails.IsEnabled = enable
        tabPageErrors.IsEnabled = True

    End Sub

    Private Function ConvertColor(drawingColor As System.Drawing.Color) As System.Windows.Media.Color
        Return Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B)
    End Function

    Private Function ConvertColor(mediaColor As System.Windows.Media.Color) As System.Drawing.Color
        Return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B)
    End Function

#End Region

#Region "Scans"
    Private Sub CheckScansFolder()
        Dim prjPath As String = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))
        If [String].IsNullOrEmpty(prjPath) Then
            Throw New EyeshotException("Unable to get the local path!")
        End If

        _scansDir = System.IO.Path.Combine(prjPath, "Scans")
        If Not Directory.Exists(_scansDir) Then
            Directory.CreateDirectory(_scansDir)
        End If

    End Sub

    Private Sub txtDownloadScans_LinkClicked(sender As Object, e As RequestNavigateEventArgs)
        System.Diagnostics.Process.Start(New System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri))
        e.Handled = True
    End Sub
#End Region

#Region "Series"
    Private Sub FillComboSeries(tree As DicomTree)
        Dim sources As New List(Of DicomElement)()

        For Each elem As DicomElement In tree.Tree
            Dim series As List(Of DicomElement) = GetDicomSeries(elem)
            If series IsNot Nothing Then
                sources.AddRange(series)
            End If
        Next

        cmbCurrentSeries.ItemsSource = sources

        If cmbCurrentSeries.Items.Count > 0 AndAlso (cmbCurrentSeries.SelectedItem Is Nothing OrElse cmbCurrentSeries.SelectedItem <> cmbCurrentSeries.Items(0)) Then
            cmbCurrentSeries.SelectedItem = cmbCurrentSeries.Items(0)
        End If
    End Sub

    Private Sub SelectSeries(dicomElement As DicomElement)
        If dicomElement Is Nothing Then
            Return
        End If

        _currentSeries = dicomElement
        _volumeRendering = Nothing

        trackBarCurrentImage.Value = InlineAssignHelper(trackBarCurrentImage.Minimum, InlineAssignHelper(trackBarFirstSlice.Value, 0))
        trackBarLastSlice.Value = InlineAssignHelper(trackBarFirstSlice.Maximum, InlineAssignHelper(trackBarLastSlice.Maximum, InlineAssignHelper(trackBarCurrentImage.Maximum, _currentSeries.Elements.Count - 1)))

        lblCurrentImageFirst.Content = "0"
        lblCurrentImageLast.Content = (_currentSeries.Elements.Count - 1).ToString()


        lblTrkBarFirstSliceStart.Content = InlineAssignHelper(lblTrkBarLastSliceStart.Content, "0")
        lblTrkBarFirstSliceEnd.Content = InlineAssignHelper(lblTrkBarLastSliceEnd.Content, InlineAssignHelper(lblLastSliceValue.Content, trackBarFirstSlice.Maximum.ToString()))

        lblFirstSliceValue.Content = trackBarCurrentImage.Value.ToString()

        lblWindowCenterValue.Content = [String].Empty
        lblWindowWidthValue.Content = [String].Empty

        ResetImage()
        LoadFile(CInt(trackBarCurrentImage.Value))
    End Sub

    Private Function GetDicomSeries(elem As DicomElement) As List(Of DicomElement)
        If elem.DicomNode = DicomElement.dicomNodeType.Instance Then
            Return Nothing
        End If

        Dim result As New List(Of DicomElement)()
        Dim study As DicomElement = Nothing
        While study Is Nothing
            If elem.Elements.Count = 0 Then
                Exit While
            End If

            If elem.DicomNode = DicomElement.dicomNodeType.Study Then
                study = elem
            Else
                elem = elem.Elements(0)

            End If
        End While

        If study IsNot Nothing Then
            result = study.Elements
        End If

        Return result
    End Function

    Private Sub cmbCurrentSeries_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        SelectSeries(DirectCast(cmbCurrentSeries.SelectedItem, DicomElement))
    End Sub
#End Region

#Region "Hounsfield"
    Private Sub FillHounsfieldColors()
        _hounsfieldColors = New List(Of HounsfieldColorTable)()

        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Air", _
             .FromValue = -1000, _
             .ToValue = -1000, _
             .Color = System.Drawing.Color.White _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Skin", _
             .FromValue = -999, _
             .ToValue = -501, _
             .Color = System.Drawing.Color.FromArgb(255, 213, 185) _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Lung", _
             .FromValue = -500, _
             .ToValue = -500, _
             .Color = System.Drawing.Color.FromArgb(160, 45, 60) _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Fat", _
             .FromValue = -100, _
             .ToValue = -50, _
             .Color = System.Drawing.Color.AntiqueWhite _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Water", _
             .FromValue = 0, _
             .ToValue = 0, _
             .Color = System.Drawing.Color.Aqua _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Cerebrospinal fluid", _
             .FromValue = 15, _
             .ToValue = 15, _
             .Color = System.Drawing.Color.FromArgb(235, 199, 147) _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Kidney", _
             .FromValue = 30, _
             .ToValue = 30, _
             .Color = System.Drawing.Color.FromArgb(160, 45, 60) _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Blood", _
             .FromValue = 31, _
             .ToValue = 45, _
             .Color = System.Drawing.Color.Crimson _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Muscle", _
             .FromValue = 10, _
             .ToValue = 40, _
             .Color = System.Drawing.Color.OrangeRed _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Grey matter", _
             .FromValue = 37, _
             .ToValue = 45, _
             .Color = System.Drawing.Color.DarkGray _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "White matter", _
             .FromValue = 20, _
             .ToValue = 30, _
             .Color = System.Drawing.Color.LightGray _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Liver", _
             .FromValue = 10, _
             .ToValue = 40, _
             .Color = System.Drawing.Color.RosyBrown _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Soft Tissue, Contrast", _
             .FromValue = 100, _
             .ToValue = 300, _
             .Color = System.Drawing.Color.HotPink _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Tooth", _
             .FromValue = 1600, _
             .ToValue = 1900, _
             .Color = System.Drawing.Color.FromArgb(241, 232, 223) _
        })
        _hounsfieldColors.Add(New HounsfieldColorTable() With { _
             .Description = "Bone", _
             .FromValue = 500, _
             .ToValue = 3000, _
             .Color = System.Drawing.Color.FromArgb(232, 221, 199) _
        })
    End Sub

    Private Function GetColorByIsoLevel(isoLevel As Integer) As System.Drawing.Color
        Dim hc = _hounsfieldColors.Where(Function(x) x.FromValue <= isoLevel AndAlso x.ToValue >= isoLevel).FirstOrDefault()
        If hc IsNot Nothing Then
            Return hc.Color
        End If

        Return System.Drawing.Color.Beige
    End Function
#End Region

#Region "Iso Level"
    Private Sub FillComboIsoValue()
        _isoValueDictionary = New Dictionary(Of String, String)() From { _
            {"Blood", "35"}, _
            {"Bone", "500"}, _
            {"Fat", "-85"}, _
            {"Lung", "-500"}, _
            {"Muscle", "25"}, _
            {"Skin", "-800"}, _
            {"Tooth", "1800"}, _
            {"", ""} _
        }

        cmbIsoLevel.ItemsSource = _isoValueDictionary
        cmbIsoLevel.DisplayMemberPath = "Key"
        cmbIsoLevel.SelectedIndex = 1
    End Sub

    Private Sub GetIsoValue([location] As Point)            
        If _drawingSelection OrElse _currentSeries Is Nothing OrElse trackBarCurrentImage.Value >= _currentSeries.Elements.Count Then
            Return
        End If

        Dim y As Integer = location.Y
        Dim row As Integer = Math.Max(0, (y - 1))
        Dim column As Integer = location.X

        Dim el As IodElement = DirectCast(_currentSeries.Elements(CInt(trackBarCurrentImage.Value)), IodElement)
        txtIsoLevel.Text = el.GetHounsfieldPixelValue(row, column).ToString()
    End Sub

    Private _selectingIsoValue As Boolean
    Private Sub cmbIsoLevel_SelectedIndexChanged(sender As Object, e As EventArgs)
        If cmbIsoLevel.SelectedIndex > -1 AndAlso cmbIsoLevel.SelectedIndex <> (_isoValueDictionary.Count - 1) Then
            _selectingIsoValue = True
            txtIsoLevel.Text = DirectCast(cmbIsoLevel.SelectedItem, KeyValuePair(Of String, String)).Value
            Dim isoLevel As Integer
            Integer.TryParse(txtIsoLevel.Text, isoLevel)
            layerColorPicker.SelectedColor = ConvertColor(GetColorByIsoLevel(isoLevel))
            txtLayerName.Text = DirectCast(cmbIsoLevel.SelectedItem, KeyValuePair(Of String, String)).Key
            _selectingIsoValue = False
        End If
    End Sub

    Private Sub txtIsoLevel_TextChanged(sender As Object, e As EventArgs)
        If _selectingIsoValue Then
            Return
        End If

        If _isoValueDictionary.ContainsValue(txtIsoLevel.Text) Then
            cmbIsoLevel.SelectedItem = _isoValueDictionary.FirstOrDefault(Function(x) x.Value = txtIsoLevel.Text)
        Else
            If Not [String].IsNullOrEmpty(cmbIsoLevel.Text) Then
                cmbIsoLevel.Text = ""
            End If

            txtLayerName.Text = "Iso-" + txtIsoLevel.Text
        End If
    End Sub
#End Region

#Region "Slices"
    Private Function GetElementIndex(element As IodElement) As Integer
        Return _currentSeries.Elements.Cast(Of IodElement)().ToList().FindIndex(Function(x) x.SliceInfo.InstanceNumber = element.SliceInfo.InstanceNumber)
    End Function

    Private Sub LoadFile(i As Integer)
        Dim fileName As String
        _wLDeltaPoint = New Point()
        _wLChangeValWidth = 0.5
        _wLChangeValCentre = 20.0
        _rightMouseDown = False

        Try
            If _currentSeries Is Nothing OrElse _currentSeries.Elements.Count <= i Then
                SetEnable(False)
                UpdateImage()
                Return
            End If

            SetCurrentElement(DirectCast(_currentSeries.Elements(i), IodElement))
            If [String].IsNullOrEmpty(lblWindowCenterValue.Content.ToString()) Then
                SetWindowCenter(_currentElement.GetWindowCenter())
            End If
            If [String].IsNullOrEmpty(lblWindowWidthValue.Content.ToString()) Then
                SetWindowWidth(_currentElement.GetWindowWidth())
            End If

            fileName = System.IO.Path.GetFileName(_currentElement.Tag.FileName) + "    (" + i.ToString() + " / " + (_currentSeries.Elements.Count - 1).ToString() + ")"

            lblFilenameValue.Content = fileName

            SetEnable(True)

            UpdateImage()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private originalSourceHeight As Integer
    Private originalSourceWidth As Integer
    Private Sub UpdateImage()
        If _currentElement Is Nothing Then
            ResetImage()

            txtDownloadScans.Visibility = Visibility.Visible
            canvasPictureBox1.Visibility = Visibility.Collapsed
        Else
            txtDownloadScans.Visibility = Visibility.Collapsed
            canvasPictureBox1.Visibility = Visibility.Visible

            Dim dicomVersion As DicomVersion = _currentElement.GetDicomVersion()
            Dim imageErrorMsg As String = [String].Format("Unable to load the image. Transfer Syntax: {0} Dicom Version: {1}", _currentElement.GetTransferSyntax(), dicomVersion)

            Try
                If Not _currentElement.IsSupportedDicomFile() Then
                    ResetImage()
                    txtErrors.AppendText(imageErrorMsg + System.Environment.NewLine)
                    tabControlBottom.SelectedIndex = 3
                    SelectionRectangle = Rectangle.Empty
                Else
                    If SelectionRectangle.IsEmpty AndAlso imagePictureBox1.Source IsNot Nothing Then
	                    Dim x As Integer = 60
	                    Dim y As Integer = 65	                    
	                    Dim width As Integer = If(400 + x > imagePictureBox1.Source.Width, CInt(imagePictureBox1.Source.Width) - x, 400)
	                    Dim height As Integer = If(270 + y > imagePictureBox1.Source.Height, CInt(imagePictureBox1.Source.Height) - y, 270)

		                'SelectionRectangle = New Rectangle(0, 0, 512, 512) //full    
	                    SelectionRectangle = New Rectangle(x, y, width, height)
                    End If

                    If tabControlBottom.SelectedIndex = 3 Then
                        tabControlBottom.SelectedIndex = 0
                    End If

                    Dim bmp As Bitmap = _currentElement.GetBitmap(_windowCenter, _windowWidth)
                    If bmp Is Nothing Then
                        Return
                    End If

                    originalSourceWidth = bmp.Width
                    originalSourceHeight = bmp.Height

                    bmp = New Bitmap(bmp, New System.Drawing.Size(512, 512))

                    imagePictureBox1.Source = RenderContextUtility.ConvertImage(DirectCast(bmp.Clone(), Bitmap))
                End If
            Catch
                ResetImage()
                txtErrors.AppendText(imageErrorMsg + System.Environment.NewLine)

                ' Shows Errors tab page.
                tabControlBottom.SelectedIndex = 3
                SelectionRectangle = Rectangle.Empty


            End Try
        End If
    End Sub

    Private Sub SetCurrentElement(element As IodElement)
        _currentElement = element
        FillSlicesDetailsTree(_currentElement)
    End Sub

    Private Sub SetWindowCenter(value As Integer)
        _windowCenter = value
        lblWindowCenterValue.Content = _windowCenter.ToString()
    End Sub

    Private Sub SetWindowWidth(value As Integer)
        _windowWidth = value
        lblWindowWidthValue.Content = _windowWidth.ToString()
    End Sub

    Private Sub btnAddSlice_Click(sender As Object, e As EventArgs)
        AddSlices(DirectCast(_currentSeries.Elements(CInt(trackBarCurrentImage.Value)), IodElement))
    End Sub

    Private Sub AddSlices(Optional element As IodElement = Nothing)
        Dim imgList As New EntityList()

        _picturesInterval = New Interval(trackBarFirstSlice.Value, trackBarLastSlice.Value)

        Dim dicomElements As List(Of DicomElement)

        If element IsNot Nothing Then
            dicomElements = New List(Of DicomElement)() From { _
                element _
            }
        Else
            dicomElements = _currentSeries.Elements.GetRange(CInt(_picturesInterval.Min), CInt(_picturesInterval.Max - _picturesInterval.Min + 1))
        End If

        Dim rectangle As LinearPath = Nothing

        For Each el As IodElement In dicomElements
            Dim iodElement As IodElement = DirectCast(el, IodElement)

            Dim spaceInX As Single, spaceInY As Single
            iodElement.GetPixelSpacing(spaceInY, spaceInX)

            Dim bitmap As Bitmap = iodElement.GetBitmap(_windowCenter, _windowWidth)

            Dim pic As New Picture(Plane.XY, iodElement.SliceInfo.Columns, iodElement.SliceInfo.Rows, bitmap)
            ' Picture will be not involved in lighting
            pic.Lighted = False
            pic.Scale(spaceInX, spaceInY, 0)

            Dim basePoint As Point3D = New devDept.Geometry.Point3D(iodElement.SliceInfo.ImageUpperLeftX, iodElement.SliceInfo.ImageUpperLeftY - (iodElement.GetRows() * spaceInY), iodElement.SliceInfo.ImageUpperLeftZ)

            pic.Translate(basePoint.X, basePoint.Y, basePoint.Z)

            imgList.Add(pic)

            If rectangle Is Nothing Then
                pic.Regen(0.001)
                rectangle = New LinearPath(pic.Vertices)
            End If
        Next

        Dim zoomFit As Boolean = model1.Entities.Count = 0
        model1.Entities.AddRange(imgList)
        model1.Invalidate()
        If zoomFit Then
            model1.ZoomFit()
        End If
    End Sub
#End Region

#Region "Dicom Tree"
    Private Sub FillTreeView(tree As DicomTree)
        treeDicom.Items.Clear()

        For Each element As DicomElement In tree.Tree
            Dim node As New TreeNode(element.Header)
            node.Tag = element
            treeDicom.Items.Add(node)

            AddChildren(element.Elements, node)
        Next
    End Sub

    Private Sub FillSlicesDetailsTree(element As IodElement)
        treeSliceDetails.Items.Clear()

        Dim node As New TreeNode(_currentElement.Header)

        For Each xe As XElement In element.Tag.XDocument.Descendants("DataSet").First().Elements("DataElement")
            AddDICOMAttributeToString(node, xe)
        Next

        treeSliceDetails.Items.Add(node)
        node.IsExpanded = True
    End Sub

    ' Helper method to add one DICOM attribute to the DICOM Tag Tree.
    Private Sub AddDICOMAttributeToString(parent As TreeNode, theXElement As XElement)
        Dim aTag As String = theXElement.Attribute("Tag").Value
        Dim aTagName As String = theXElement.Attribute("TagName").Value
        Dim aTagData As String = theXElement.Attribute("Data").Value

        ' Enrich the Transfer Syntax attribute (0002,0010) with human-readable string from dictionary
        If aTag.Equals("(0002,0010)") Then
            aTagData = String.Format("{0} ({1})", aTagData, TransferSyntaxDictionary.GetTransferSyntaxName(aTagData))
        End If

        ' Enrich the SOP Class UID attribute (0008,0016) with human-readable string from dictionary
        If aTag.Equals("(0008,0016)") Then
            aTagData = String.Format("{0} ({1})", aTagData, SopClassDictionary.GetSopClassName(aTagData))
        End If

        Dim s As String = String.Format("{0} {1}", aTag, aTagName)

        ' Do some cut-off in order to align the TagData
        If s.Length > 50 Then
            s = s.Remove(50)
        Else
            s = s.PadRight(50)
        End If

        s = String.Format("{0} {1}", s, aTagData)

        Dim node As New TreeNode(s)
        parent.Items.Add(node)

        ' In case the DICOM attributes has childrens (= Sequence), call the helper method recursively.
        If theXElement.HasElements Then
            For Each xe As XElement In theXElement.Elements("DataElement")
                AddDICOMAttributeToString(node, xe)
            Next
        End If
    End Sub

    Private Shared Sub AddChildren(elements As List(Of DicomElement), parentNode As TreeNode)
        If elements Is Nothing Then
            Return
        End If

        For Each el As DicomElement In elements
            Dim childNode As New TreeNode(el.Header)
            childNode.Tag = el
            parentNode.Items.Add(childNode)

            AddChildren(el.Elements, childNode)
        Next
    End Sub

    Private Sub CollapseAll(tv As TreeView)
        For Each i As TreeNode In tv.Items
            i.IsExpanded = False
        Next
    End Sub

    Private Sub SearchNodeInTree(tv As TreeView, element As IodElement)
        tv.IsHitTestVisible = True

        CollapseAll(tv)

        Dim tnc As ItemCollection = tv.Items

        For Each tn As TreeNode In tnc
            If tn.Items.Count > 0 Then
                tn.IsExpanded = True
                SearchNodeInNodes(tv, tn, element)
            End If

            If ReferenceEquals(element, tn.Tag) Then
                tn.IsSelected = True
                Return
            End If
        Next
    End Sub

    Private Sub SearchNodeInNodes(tv As TreeView, parentTn As TreeNode, element As IodElement)
        Dim tnc As ObservableCollection(Of TreeNode) = parentTn.Items

        For Each tn As TreeNode In tnc
            If tn.Items.Count > 0 Then
                tn.IsExpanded = True
                SearchNodeInNodes(tv, tn, element)
            End If

            If ReferenceEquals(element, tn.Tag) Then
                tn.IsSelected = True
                Return
            End If
        Next
    End Sub

    Private Sub treeDicom_OnSelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        Dim selectedNode As TreeNode = DirectCast(e.NewValue, TreeNode)
        If selectedNode Is Nothing Then
            Return
        End If

        If TypeOf selectedNode.Tag Is IodElement Then
            Dim element As IodElement = DirectCast(selectedNode.Tag, IodElement)
            If element IsNot Nothing Then
                If Not element.Parent.Equals(_currentSeries) Then
                    SelectSeries(element.Parent)
                End If

                If Not element.Equals(_currentElement) Then
                    LoadFile(GetElementIndex(element))
                End If
            End If
        ElseIf TypeOf selectedNode.Tag Is DicomElement Then
            Dim element As DicomElement = DirectCast(selectedNode.Tag, DicomElement)
            If element.DicomNode = DicomElement.dicomNodeType.Series Then
                If Not element.Equals(_currentSeries) Then
                    SelectSeries(element)
                End If
            End If
        End If

        Dim idx As Integer = GetElementIndex(_currentElement)

        If idx < trackBarCurrentImage.Minimum Then
	        trackBarCurrentImage.Minimum = idx
        End If

        If idx > trackBarCurrentImage.Maximum Then
	        trackBarCurrentImage.Maximum = idx
        End If

        trackBarCurrentImage.Value = idx
    End Sub
#End Region

#Region "Volume Rendering"

    Private Sub btnAddVolume_Click(sender As Object, e As System.EventArgs)
        If SelectionRectangle.IsEmpty Then
            MessageBox.Show("Select the rectangle in the picture box to compute the mesh.", "Choose Area")
        End If

        _picturesInterval = New Interval(trackBarFirstSlice.Value, trackBarLastSlice.Value)

        Dim isoLevel As Integer
        Integer.TryParse(txtIsoLevel.Text, isoLevel)

        model1.Focus()

        DoMarchingCubes(isoLevel)
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs)
        model1.Clear()
        UpdateLayerListView()
        model1.Invalidate()
    End Sub

    Private Sub DoMarchingCubes(isoLevel As Integer)
        If _loadedInderval.Min <> _picturesInterval.Min OrElse _loadedInderval.Max <> _picturesInterval.Max Then
            _loadedInderval = _picturesInterval

            txtIsoLevel.Text = isoLevel.ToString()
        End If

        GC.Collect()


        ' Initialize marching cube algorithm            
        If _currentSeries Is Nothing Then
            Return
        End If

        Dim dicomElements As List(Of DicomElement) = _currentSeries.Elements.GetRange(CInt(_picturesInterval.Min), CInt(_picturesInterval.Max - _picturesInterval.Min + 1))


        Dim widthRatio As Double = originalSourceWidth * 1.0 / imagePictureBox1.ActualWidth,
            heightRatio As Double = originalSourceHeight * 1.0 / imagePictureBox1.ActualHeight

        Dim x As Integer = SelectionRectangle.Location.X * widthRatio
        Dim y As Integer = SelectionRectangle.Location.Y * heightRatio
        Dim width As Integer = SelectionRectangle.Width * widthRatio
        Dim height As Integer = SelectionRectangle.Height * heightRatio

        _volumeRendering = New MyVolumeRendering(dicomElements, New Point3D(x, y, 0), width, height, CInt(_picturesInterval.Length) +1) '+1 because the elements array starts from 0
        _volumeRendering.LightWeight = chkPreview.IsChecked.Value

        _volumeRendering.IsoLevel = isoLevel
        
        Dim layer As Layer = model1.Layers(0)
        Dim layerName As String = txtLayerName.Text
        If Not model1.Layers.Contains(layerName) AndAlso Not [String].IsNullOrEmpty(layerName) Then
            layer = New Layer(layerName, ConvertColor(layerColorPicker.SelectedColor))
            model1.Layers.Add(layer)
            UpdateLayerListView()
        Else
            layer = model1.Layers(layerName)
        End If

        _volumeRendering.Layer = layer

        _viewportIsWorking = True
        SetEnable()
        model1.StartWork(_volumeRendering)
    End Sub
#End Region

#Region "PictureBox"
    Private _drawingSelection As Boolean

    ' For Window Level 
    Private _wLDeltaPoint As Point
    Private _wLDeltaX As Integer
    Private _wLDeltaY As Integer
    Private _wLChangeValWidth As Double
    Private _wLChangeValCentre As Double
    Private _rightMouseDown As Boolean

    Private Sub btnSelectArea_Click(sender As Object, e As System.EventArgs)
        _drawingSelection = True
        StartDrawingSelection()
    End Sub

    Private _selectionRectangle As Rectangle
    Private Property SelectionRectangle() As Rectangle
        Get
            Return _selectionRectangle
        End Get
        Set(value As Rectangle)
            _selectionRectangle = value
            If _selectionRectangle.IsEmpty Then
                dragSelectionBorder.Visibility = Visibility.Hidden
            Else
                dragSelectionBorder.Visibility = Visibility.Visible
                DrawSelectionRect(_selectionRectangle)
            End If
        End Set
    End Property

    Private _firstPt As Point
    Private _secondPt As Point

    Private _dragging As Boolean = False

    'private bool _drawingSelection;
    Public Sub StartDrawingSelection()
        _drawingSelection = True
        canvasPictureBox1.Cursor = Cursors.Cross
    End Sub

    Private Sub StopDrawingSelection()
        _drawingSelection = False
        ' Sets default cursor
        canvasPictureBox1.Cursor = Nothing
    End Sub

    Private Function GetPositionForSelection(e As MouseEventArgs) As Point
        Return e.GetPosition(canvasPictureBox1)
    End Function

    Private Sub pictureGrid_OnMouseDown(sender As Object, e As MouseButtonEventArgs)
        Dim location As Point = GetPositionForSelection(e)

        If location.X > imagePictureBox1.ActualWidth OrElse location.Y > imagePictureBox1.ActualHeight Then
	        Return
        End If

        If _drawingSelection Then
            If _dragging = False Then
                _firstPt = location
                _dragging = True
            End If
        ElseIf imagePictureBox1.Source IsNot Nothing Then
            If e.ChangedButton = MouseButton.Right Then
                _wLDeltaPoint.X = location.X
                _wLDeltaPoint.Y = location.Y
                _rightMouseDown = True
                canvasPictureBox1.Cursor = Cursors.Hand
            Else
                GetIsoValue(location)
            End If
        End If

        MyBase.OnMouseDown(e)
    End Sub

    ''' <summary>
    ''' Event raised when the user releases the left mouse-button.
    ''' </summary>
    Private Sub pictureGrid_OnMouseUp(sender As Object, e As MouseButtonEventArgs)
        If _dragging Then
            _secondPt = GetPositionForSelection(e)
            _dragging = False
            StopDrawingSelection()
        ElseIf _rightMouseDown Then
            _rightMouseDown = False

            ' Sets default cursor
            canvasPictureBox1.Cursor = Nothing
        End If

        MyBase.OnMouseUp(e)
    End Sub

    ''' <summary>
    ''' Event raised when the user moves the mouse button.
    ''' </summary>
    Private Sub pictureGrid_OnMouseMove(sender As Object, e As MouseEventArgs)
        Dim location As Point = GetPositionForSelection(e)
        If _drawingSelection Then
	        If location.X < imagePictureBox1.Source.Width AndAlso location.Y < imagePictureBox1.Source.Height Then
		        canvasPictureBox1.Cursor = Cursors.Cross
	        Else
		        canvasPictureBox1.Cursor = Nothing
	        End If

	        If _dragging Then
		        _secondPt = location
		        If _secondPt.X > imagePictureBox1.Source.Width Then
			        _secondPt.X = imagePictureBox1.Source.Width - 2
		        End If
		        If _secondPt.Y > imagePictureBox1.Source.Height Then
			        _secondPt.Y = imagePictureBox1.Source.Height - 2
		        End If
		        If _secondPt.X < 0 Then
			        _secondPt.X = 0
		        End If
		        If _secondPt.Y < 0 Then
			        _secondPt.Y = 0
		        End If

		        UpdateDragSelectionRect(_firstPt, _secondPt)
	        End If
        ElseIf _rightMouseDown Then
            DetermineMouseSensitivity()

            _wLDeltaX = CInt((_wLDeltaPoint.X - location.X) * _wLChangeValWidth)
            _wLDeltaY = CInt((_wLDeltaPoint.Y - location.Y) * _wLChangeValCentre)

            SetWindowCenter(_windowCenter - _wLDeltaY)
            SetWindowWidth(_windowWidth - _wLDeltaX)

            _wLDeltaPoint.X = location.X
            _wLDeltaPoint.Y = location.Y

            UpdateImage()
        End If

        MyBase.OnMouseMove(e)
    End Sub

    ''' <summary>
    ''' Update the position and size of the rectangle used for drag selection.
    ''' </summary>
    Private Sub UpdateDragSelectionRect(pt1 As Point, pt2 As Point)
        Dim x As Double = _firstPt.X
        Dim y As Double = _firstPt.Y
        Dim width As Double = _secondPt.X - x
        Dim height As Double = _secondPt.Y - y

        If _secondPt.X < _firstPt.X Then
            x = _secondPt.X
            width = _firstPt.X - x
        End If

        If _secondPt.Y < _firstPt.Y Then
            y = _secondPt.Y
            height = _firstPt.Y - y
        End If

        _selectionRectangle = New Rectangle(CInt(x), CInt(y), CInt(width), CInt(height))

        DrawSelectionRect(_selectionRectangle)


    End Sub

    Private Sub DrawSelectionRect(rec As Rectangle)
        ' Updates the coordinates of the rectangle used for drag selection.            
        Canvas.SetLeft(dragSelectionBorder, rec.X)
        Canvas.SetTop(dragSelectionBorder, rec.Y)
        dragSelectionBorder.Width = rec.Width
        dragSelectionBorder.Height = rec.Height
    End Sub

    Private Sub ResetImage()
	    imagePictureBox1.Source = Nothing
	    SelectionRectangle = Rectangle.Empty
    End Sub

    Private Sub btnReset_Click(sender As Object, e As System.EventArgs)
        SetWindowCenter(_currentElement.GetWindowCenter())
        SetWindowWidth(_currentElement.GetWindowWidth())
        UpdateImage()
    End Sub

    ' Modifies the 'sensitivity' of the mouse based on the current window width        
    Private Sub DetermineMouseSensitivity()
        If _windowWidth < 10 Then
            _wLChangeValWidth = 0.1
        ElseIf _windowWidth >= 20000 Then
            _wLChangeValWidth = 40
        Else
            _wLChangeValWidth = 0.1 + (_windowWidth / 300.0)
        End If

        _wLChangeValCentre = _wLChangeValWidth
    End Sub
#End Region

#Region "TrackBars"
    Private Sub trackBarFirstSlice_ValueChanged(sender As Object, e As System.EventArgs)
        lblFirstSliceValue.Content = InlineAssignHelper(lblCurrentImageFirst.Content, trackBarFirstSlice.Value.ToString())

        If trackBarFirstSlice.Value > trackBarLastSlice.Value Then
            trackBarCurrentImage.Maximum = trackBarFirstSlice.Value
            trackBarCurrentImage.Value = InlineAssignHelper(trackBarLastSlice.Value, trackBarFirstSlice.Value)
            lblLastSliceValue.Content = InlineAssignHelper(lblCurrentImageLast.Content, trackBarLastSlice.Value.ToString())
        End If

        If trackBarCurrentImage.Value < trackBarFirstSlice.Value Then

            trackBarCurrentImage.Value = trackBarFirstSlice.Value
        End If


        trackBarCurrentImage.Minimum = trackBarFirstSlice.Value
    End Sub

    Private Sub trackBarLastSlice_ValueChanged(sender As Object, e As EventArgs)
        lblLastSliceValue.Content = InlineAssignHelper(lblCurrentImageLast.Content, trackBarLastSlice.Value.ToString())

        If trackBarLastSlice.Value < trackBarFirstSlice.Value Then
            trackBarCurrentImage.Minimum = trackBarLastSlice.Value
            trackBarCurrentImage.Value = InlineAssignHelper(trackBarFirstSlice.Value, trackBarLastSlice.Value)
            lblFirstSliceValue.Content = InlineAssignHelper(lblCurrentImageFirst.Content, trackBarFirstSlice.Value.ToString())
        End If

        If trackBarCurrentImage.Value > trackBarLastSlice.Value Then

            trackBarCurrentImage.Value = trackBarLastSlice.Value
        End If

        trackBarCurrentImage.Maximum = trackBarLastSlice.Value
    End Sub

    Private Sub trackBarCurrentImage_ValueChanged(sender As Object, e As EventArgs)
        LoadFile(CInt(trackBarCurrentImage.Value))

        lblCurrentImageValue.Content = trackBarCurrentImage.Value.ToString()
    End Sub

    Private _slicesTimer As DispatcherTimer
    Private Sub btnPlaySlices_Click(sender As Object, e As RoutedEventArgs)
        If _playingSlices Then
            btnPlaySlices.IsChecked = True
            Return
        End If
        pictureGrid.IsEnabled = False
        _playingSlices = True
        SetEnable()
        btnSelectArea.IsEnabled = False
        _slicesTimer = New DispatcherTimer()
        _slicesTimer.Interval = New TimeSpan(0, 0, 0, 0, 50)
        AddHandler _slicesTimer.Tick, AddressOf _slicesTimer_Tick
        _slicesTimer.IsEnabled = True
    End Sub

    Private Sub _slicesTimer_Tick(sender As Object, e As EventArgs)
        If trackBarCurrentImage.Value = trackBarCurrentImage.Maximum Then
            trackBarCurrentImage.Value = trackBarCurrentImage.Minimum
        End If
        trackBarCurrentImage.Value += 1
    End Sub

    Private Sub btnStopSlices_Click(sender As Object, e As RoutedEventArgs)
        If _slicesTimer Is Nothing Then
            Return
        End If

        _slicesTimer.IsEnabled = False
        _slicesTimer = Nothing
        _playingSlices = False
        btnSelectArea.IsEnabled = True
        btnPlaySlices.IsChecked = False
        SetEnable()
        pictureGrid.IsEnabled = True
    End Sub

    Private Sub tabControl1_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not model1.IsLoaded OrElse _currentElement Is Nothing Then
            Return
        End If

        If Not DirectCast(tabControlBottom.Items(tabControlBottom.SelectedIndex), TabItem).IsEnabled Then
            Return
        End If

        Select Case tabControlBottom.SelectedIndex
            Case 0
                ' Slices
                Dim idx As Integer = GetElementIndex(_currentElement)
                If trackBarFirstSlice.Value > idx Then
                    trackBarFirstSlice.Value = idx
                End If

                If trackBarLastSlice.Value < idx Then
                    trackBarLastSlice.Value = idx
                End If

                trackBarCurrentImage.Value = idx
                Exit Select
            Case 1
                ' Dicom Tree
                SearchNodeInTree(treeDicom, _currentElement)
                treeDicom.Focus()
                Exit Select
            Case 2
                ' Slice Details
                FillSlicesDetailsTree(_currentElement)
                Exit Select
        End Select
    End Sub
#End Region

#Region "Path"
    Private _path As String

    Private Sub SetPath(path As String)
        If path.Equals(_path, StringComparison.InvariantCultureIgnoreCase) Then
            Return
        End If

        _path = path
        txtPath.Text = _path

        Init()
    End Sub

    Private Sub btnPath_OnClick(sender As Object, e As RoutedEventArgs)
        Dim dialog = New System.Windows.Forms.FolderBrowserDialog()

        Dim result As System.Windows.Forms.DialogResult = dialog.ShowDialog()
        If result = System.Windows.Forms.DialogResult.OK Then
            SetPath(dialog.SelectedPath)
        End If
    End Sub

    Private Sub txtPath_OnLostFocus(sender As Object, e As RoutedEventArgs)
        SetPath(txtPath.Text)
    End Sub
#End Region

#Region "Layers"
    Private Sub UpdateLayerListView()
        model1.Layers(0).Name = "Slices"
        model1.Layers(0).Color = System.Drawing.Color.WhiteSmoke
        layerListView.Items.Clear()
        Layers = New ObservableCollection(Of ListViewModelItem)()

        For i As Integer = 0 To model1.Layers.Count - 1
            Dim la As Layer = model1.Layers(i)
            Dim lvi As New ListViewModelItem(la)
            Layers.Add(lvi)


            layerListView.Items.Add(lvi)
        Next
    End Sub

    Private Sub layerListView_ItemChecked(sender As Object, e As RoutedEventArgs)
        Dim cb = TryCast(sender, CheckBox)
        Dim item = DirectCast(cb.DataContext, ListViewModelItem)        

        If item.IsChecked Then

            model1.Layers.TurnOn(item.LayerName)
        Else


            model1.Layers.TurnOff(item.LayerName)
        End If

        ' updates bounding box, shadow and transparency
        model1.Entities.UpdateBoundingBox()

        model1.Invalidate()
    End Sub
#End Region

#Region "Actions"

    Private Sub rdBtnNone_CheckedChanged(sender As Object, e As RoutedEventArgs)
        If model1 Is Nothing Then
            Return
        End If

        If rdBtnNone.IsChecked.Value Then
            model1.ActionMode = actionType.None
            model1.Focus()
        End If
    End Sub

    Private Sub rdBtnSelect_CheckedChanged(sender As Object, e As RoutedEventArgs)
        If rdBtnSelect.IsChecked.Value Then
            model1.ActionMode = actionType.SelectVisibleByPick
            model1.Focus()
        End If
    End Sub

    Private Sub btnSplitMeshes_Click(sender As Object, e As RoutedEventArgs)
        Cursor = Cursors.Wait

        Dim meshToRomove As New List(Of Mesh)()
        Dim meshToAdd As New Dictionary(Of String, List(Of Mesh))()
        For Each ent As Entity In model1.Entities.Where(Function(x) x.Selected = True)
            If TypeOf ent Is Mesh Then
                ' For each selected mesh, cleans the triangles from the duplicated vertices and creates a new mesh
                Dim m As Mesh = DirectCast(ent, Mesh)
                Dim cleanedTriangles As List(Of IndexTriangle) = Nothing
                Dim uniqueVertices As List(Of Point3D) = Nothing
                Utility.CleanTriangles(m.Triangles, m.Vertices, cleanedTriangles, uniqueVertices)
                Dim newMesh As New Mesh(uniqueVertices, cleanedTriangles)

                ' Divides the new mesh into separate objects meshes.
                Dim meshes = newMesh.SplitDisjoint()

                If meshes.Length < 2 Then
                    Continue For
                End If

                ' Creates list of new meshes that will be add to the original layer
                Dim layerName As String = m.LayerName
                Dim newMeshes As New List(Of Mesh)()
                If meshToAdd.ContainsKey(layerName) Then
                    newMeshes = meshToAdd(layerName)
                Else
                    meshToAdd.Add(layerName, newMeshes)
                End If

                For Each mesh As Mesh In meshes
                    ' Sets the NormalAveragingMode equal to the original mesh
                    mesh.NormalAveragingMode = m.NormalAveragingMode
                    newMeshes.Add(mesh)
                Next

                meshToAdd(layerName) = newMeshes

                meshToRomove.Add(m)
            End If
        Next

        For Each pair As KeyValuePair(Of String, List(Of Mesh)) In meshToAdd
            model1.Entities.AddRange(pair.Value, pair.Key)
        Next

        For Each mesh As Mesh In meshToRomove
            model1.Entities.Remove(mesh)
        Next

        model1.Invalidate()

        Cursor = Nothing
    End Sub

    Private Sub btnSmoothMeshes_Click(sender As Object, e As RoutedEventArgs)
        Cursor = Cursors.Wait

	    For Each ent As Entity In model1.Entities.Where(Function(x) x.Selected = True)
		    Dim m As Mesh = DirectCast(ent, Mesh)

		    ' For each selected mesh generated with the "Preview" option, generates a smoothed mesh.
		    If m IsNot Nothing AndAlso m.LightWeight = True Then
			    ' Converts the Triangles into SmoothTriangles
                Dim cleanedTrianlges As List(Of IndexTriangle) = Nothing
                Dim uniqueVerices As List(Of Point3D) = Nothing
			    Utility.CleanTriangles(m.Triangles, m.Vertices, cleanedTrianlges, uniqueVerices)
			    Dim smoothTriangles As SmoothTriangle() = New SmoothTriangle(cleanedTrianlges.Count - 1) {}
			    For i As Integer = 0 To cleanedTrianlges.Count - 1
				    Dim triangle As IndexTriangle = cleanedTrianlges(i)
				    smoothTriangles(i) = New SmoothTriangle(triangle.V1, triangle.V2, triangle.V3)
			    Next

			    ' Sets the LightWeight property to false
			    m.LightWeight = False
			    ' Assigns the new arrays
			    m.Triangles = smoothTriangles
			    m.Vertices = uniqueVerices.ToArray()
			    ' Updates the normals to get a smoothed mesh.
			    m.UpdateNormals()
		    End If
	    Next

	    model1.Entities.Regen()
	    model1.Invalidate()

	    Cursor = Nothing
    End Sub

    Private Sub btnInvertSelection_Click(sender As Object, e As RoutedEventArgs)
        model1.Entities.InvertSelection()

        model1.Invalidate()

        model1.Focus()
    End Sub

    Private Sub rdBtnMeasure_CheckedChanged(sender As Object, e As RoutedEventArgs)
        model1.Measure(rdBtnMeasure.IsChecked.Value)
    End Sub

    Private _editingClippingPlane As Boolean
    Private Sub rdBtnClip_CheckedChanged(sender As Object, e As RoutedEventArgs)
        If rdBtnClip.IsChecked.Value Then
            If Not _editingClippingPlane Then
                btnAddSection.IsEnabled = True

                _editingClippingPlane = True

                model1.ActionMode = actionType.None

                ' sets the Z coordinate of the origin of the clippingPlane
                model1.ClippingPlane1.Plane.Origin.Z = ((model1.Entities.BoxMin + model1.Entities.BoxMax) / 2).Z
                
                ' enables a clippingPlane                           
                model1.ClippingPlane1.Edit(Nothing)
            End If

            model1.Focus()
        Else
            ' disables the clippingPlane and its change
            model1.ClippingPlane1.Cancel()

            _editingClippingPlane = False

            btnAddSection.IsEnabled = False
        End If

        model1.Invalidate()
    End Sub

    Private Sub btnAddSection_Click(sender As Object, e As RoutedEventArgs)
        If Not rdBtnClip.IsChecked.Value Then
            Return
        End If

        For i As Integer = 0 To model1.Entities.Count - 1
            Dim entity As Entity = model1.Entities(i)
            Dim layer As Layer = model1.Layers(entity.LayerName)

            If layer.Visible And TypeOf entity Is Mesh Then
                Dim m As Mesh = DirectCast(entity, Mesh)

                Dim layerName As String = [String].Format("{0}Section", layer.Name)
               
                If Not model1.Layers.Contains(layerName) Then
                    Dim newLayer As New Layer(layerName, layer.Color)
                    model1.Layers.Add(newLayer)
                    UpdateLayerListView()                    
                End If

                Dim curves As ICurve() = m.Section(model1.ClippingPlane1.Plane, 0)

                For Each curve As Entity In curves
                    model1.Entities.Add(curve, layerName)
                Next
            End If
        Next

        model1.Invalidate()
        model1.Focus()
    End Sub
#End Region

#Region "Export to STL"
    Private Const StlFile As String = "Dicom.stl"

    Private Sub btnExport_OnClick(sender As Object, e As RoutedEventArgs)
        _viewportIsWorking = True
        SetEnable(False)
        Dim ws As New WriteSTL(New WriteParams(model1), StlFile)
        model1.StartWork(ws)
    End Sub
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
    Private Sub ShowExportedMessage(filename As String)
	    Dim fullPath As String = [String].Format("{0}\{1}", System.Environment.CurrentDirectory, filename)
	    MessageBox.Show([String].Format("File saved in {0}", fullPath))
	    model1.Focus()
    End Sub
#End Region

#Region "Save to XML"
    Private Const DicomTreeXmlFile As String = "DicomTree.xml"    
    Private Sub treeDicom_SaveToXml(sender As Object, eventArgs As RoutedEventArgs)
	    Dim doc = New XDocument()

	    For Each element As DicomElement In _dicomTree.Tree
		    Dim el As XElement = XmlAddDicomElements(element)
		    doc.Add(el)
	    Next

	    Using xml As New XmlTextWriter(DicomTreeXmlFile, Encoding.ASCII)
	        xml.Formatting = Formatting.Indented
	        doc.Save(xml)
        End Using

	    ShowExportedMessage(DicomTreeXmlFile)
    End Sub

    Private Function XmlAddDicomElements(dicomElement As DicomElement) As XElement
	    Dim el As New XElement("DicomElement")
	    el.Add(New XAttribute("Name", dicomElement.ToString()))

	    If dicomElement.Elements IsNot Nothing Then
		    For Each element As DicomElement In dicomElement.Elements
			    el.Add(XmlAddDicomElements(element))
		    Next
	    End If

	    Return el
    End Function

    Private Const SliceXmlFile As String = "Slice.xml"   

    Private Sub treeSliceDetails_SaveToXml(sender As Object, eventArgs As RoutedEventArgs)
	    Dim doc As XDocument
        doc = New XDocument(_currentElement.Tag.XDocument)
        
        For Each element As XElement In doc.Descendants("DataSet").First().Elements("DataElement")
            Dim attributesList As New List(Of String)
            attributesList.Add("Tag")
            attributesList.Add("TagName")
            attributesList.Add("Data")
		    XmlRemoveAttributes(element, attributesList)
	    Next

	    Using xml As New XmlTextWriter(SliceXmlFile, Encoding.ASCII)
	        xml.Formatting = Formatting.Indented
	        doc.Save(xml)
        End Using

	    ShowExportedMessage(SliceXmlFile)
    End Sub

    Private Sub XmlRemoveAttributes(theXElement As XElement, skippedAttributes As List(Of String))
	    Dim attributes As New Dictionary(Of String, String)()
	    For Each att As String In skippedAttributes
		    attributes.Add(att, theXElement.Attribute(att).Value)
	    Next

	    theXElement.RemoveAttributes()

	    For Each pair As KeyValuePair(Of String, String) In attributes
		    theXElement.Add(New XAttribute(pair.Key, pair.Value))
	    Next

	    If theXElement.HasElements Then
		    For Each xe As XElement In theXElement.Elements("DataElement")
			    XmlRemoveAttributes(xe, skippedAttributes)
		    Next
	    End If
    End Sub
#End Region
End Class

Class HounsfieldColorTable
    Public Property Description() As [String]
        Get
            Return m_Description
        End Get
        Set(value As [String])
            m_Description = value
        End Set
    End Property
    Private m_Description As [String]
    Public Property FromValue() As Integer
        Get
            Return m_FromValue
        End Get
        Set(value As Integer)
            m_FromValue = value
        End Set
    End Property
    Private m_FromValue As Integer
    Public Property ToValue() As Integer
        Get
            Return m_ToValue
        End Get
        Set(value As Integer)
            m_ToValue = value
        End Set
    End Property
    Private m_ToValue As Integer
    Public Property Color() As System.Drawing.Color
        Get
            Return m_Color
        End Get
        Set(value As System.Drawing.Color)
            m_Color = value
        End Set
    End Property
    Private m_Color As System.Drawing.Color
End Class

Class MyVolumeRendering
    Inherits VolumeRendering
    Public Property Layer() As Layer
        Get
            Return m_Layer
        End Get
        Set(value As Layer)
            m_Layer = value
        End Set
    End Property
    Private m_Layer As Layer

    Public Sub New(elements As IList(Of DicomElement), gridOrigin As Point3D, nCellsInX As Integer, nCellsInY As Integer, nCellsInZ As Integer, Optional func As ScalarField3D = Nothing)
        MyBase.New(elements, gridOrigin, nCellsInX, nCellsInY, nCellsInZ, func)
    End Sub


    Protected Overrides Sub WorkCompleted(model As Environment)
        MyBase.WorkCompleted(model)

        If Result IsNot Nothing Then
            Result.NormalAveragingMode = Mesh.normalAveragingType.Averaged
            Result.ColorMethod = colorMethodType.byLayer
            model.Entities.Add(Result, Layer.Name)
        End If

        model.ZoomFit()
    End Sub
End Class


''' <summary>    
''' This class represent the Model for Layers List.
''' </summary>    
Class ListViewModelItem
    Public Sub New(layer__1 As Layer)
        Layer = layer__1
        IsChecked = layer__1.Visible
        ForeColor = RenderContextUtility.ConvertColor(Layer.Color)
    End Sub

    Public Property Layer() As Layer
        Get
            Return m_Layer
        End Get
        Set(value As Layer)
            m_Layer = value
        End Set
    End Property
    Private m_Layer As Layer

    Public ReadOnly Property LayerName() As String
        Get
            Return Layer.Name
        End Get
    End Property

    Public ReadOnly Property ColorName() As String
        Get
            Return Layer.Color.Name
        End Get
    End Property

    Public Property ForeColor() As Brush
        Get
            Return m_ForeColor
        End Get
        Set(value As Brush)
            m_ForeColor = value
        End Set
    End Property
    Private m_ForeColor As Brush

    Public Property IsChecked() As Boolean
        Get
            Return m_IsChecked
        End Get
        Set(value As Boolean)
            m_IsChecked = value
        End Set
    End Property
    Private m_IsChecked As Boolean
End Class

''' <summary>
''' In the XAML markup, I have specified a HierarchicalDataTemplate for the ItemTemplate of the TreeView.
''' This class represent the ViewModel for TreeView's Items.
''' </summary>
Class TreeNode
    Inherits FrameworkElement
    Public Sub New()
        Items = New ObservableCollection(Of TreeNode)()
    End Sub

    Public Sub New(text__1 As String)
        Me.New()
        Text = text__1
    End Sub

    Public Property Text() As String
        Get
            Return m_Text
        End Get
        Set(value As String)
            m_Text = value
        End Set
    End Property
    Private m_Text As String

    Public Property Items() As ObservableCollection(Of TreeNode)
        Get
            Return m_Items
        End Get
        Set(value As ObservableCollection(Of TreeNode))
            m_Items = value
        End Set
    End Property
    Private m_Items As ObservableCollection(Of TreeNode)

    Public Overrides Function ToString() As String
        Return Text
    End Function

    Public Shared ReadOnly IsSelectedProperty As DependencyProperty = DependencyProperty.Register("IsSelected", GetType(Boolean), GetType(TreeNode), New PropertyMetadata(False))

    Public Property IsSelected() As Boolean
        Get
            Return CBool(GetValue(IsSelectedProperty))
        End Get
        Set(value As Boolean)
            SetValue(IsSelectedProperty, value)
        End Set
    End Property

    Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register("IsExpanded", GetType(Boolean), GetType(TreeNode), New PropertyMetadata(False))

    Public Property IsExpanded() As Boolean
        Get
            Return CBool(GetValue(IsExpandedProperty))
        End Get
        Set(value As Boolean)
            SetValue(IsExpandedProperty, value)
        End Set
    End Property
End Class
