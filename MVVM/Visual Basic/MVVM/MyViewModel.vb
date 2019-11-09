Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Graphics
Imports Brush = System.Windows.Media.Brush
Imports Color = System.Windows.Media.Color


Public Class MyViewModel
	Implements INotifyPropertyChanged
	#Region "Common properties"
    Private _fontFamily As System.Windows.Media.FontFamily = System.Windows.SystemFonts.CaptionFontFamily

    Public Property FontFamily() As System.Windows.Media.FontFamily
        Get
            Return _fontFamily
        End Get
        Set(value As System.Windows.Media.FontFamily)
            _fontFamily = value
            NotifyPropertyChanged("FontFamily")
        End Set
    End Property


	Private _autoRefresh As Boolean = True
	Public Property AutoRefresh() As Boolean
		Get
			Return _autoRefresh
		End Get
		Set
			_autoRefresh = value
			NotifyPropertyChanged("AutoRefresh")
		End Set
	End Property

    Public Property ColorThemeType() As colorThemeType
	    Get
		    Return MyBackgroundSettings.ColorTheme
	    End Get
	    Set
		    MyBackgroundSettings.ColorTheme = value
		    NotifyPropertyChanged("ColorThemeType")
	    End Set
    End Property

	Private _actionType As actionType = actionType.SelectByPick
	Public Property ActionType() As actionType
		Get
			Return _actionType
		End Get
		Set
			_actionType = value
			NotifyPropertyChanged("ActionType")
		End Set
	End Property

	Private _selectionColor As Color = Colors.Yellow
	Public Property SelectionColor() As Color
		Get
			Return _selectionColor
		End Get
		Set
			_selectionColor = value
			NotifyPropertyChanged("SelectionColor")
		End Set
	End Property

	Private _displayType As displayType = displayType.Rendered
	Public Property DisplayType() As displayType
		Get
			Return _displayType
		End Get
		Set
			_displayType = value
			NotifyPropertyChanged("DisplayType")
		End Set
	End Property

	Private _lightingVisibility As Visibility

	Public Property LightingVisibility() As Visibility
		Get
			Return _lightingVisibility
		End Get
		Set
			_lightingVisibility = value
			NotifyPropertyChanged("LightingVisibility")
		End Set
	End Property


    Private _lighting As Boolean
    Public Property Lighting() As Boolean
        Get
            Return _lighting
        End Get
        Set(value As Boolean)
            _lighting = value
            MyOriginSymbol.Lighting = value
            MyCoordinateSystemIcon.Lighting = value
            MyGrids(0).Lighting = value

            If value Then
                LightingVisibility = Visibility.Visible
            Else
                LightingVisibility = Visibility.Collapsed
            End If

            NotifyPropertyChanged("Lighting")
        End Set
    End Property

#End Region

    #Region "Legend"
    Private _legendVisible As Boolean = True

    Public Property LegendVisible() As Boolean
        Get
            Return _legendVisible
        End Get
        Set(value As Boolean)
            _legendVisible = value
            NotifyPropertyChanged("LegendVisible")
        End Set
    End Property

    Private _legendTitle As String = "Data binding"

    Public Property LegendTitle() As String
        Get
            Return _legendTitle
        End Get
        Set(value As String)
            _legendTitle = value
            NotifyPropertyChanged("LegendTitle")
        End Set
    End Property

    Private _legendSubtitle As String = "This sample shows different ways for the data binding."

    Public Property LegendSubtitle() As String
        Get
            Return _legendSubtitle
        End Get
        Set(value As String)
            _legendSubtitle = value
            NotifyPropertyChanged("LegendSubtitle")
        End Set
    End Property

    Private _legendItemSize As String

    Public Property LegendItemSize() As String
        Get
            Return _legendItemSize
        End Get
        Set(value As String)
            _legendItemSize = value
            NotifyPropertyChanged("LegendItemSize")
        End Set
    End Property

#End Region

	#Region "Background"

	Private _myBackgroundSettings As BackgroundSettings

	Public Property MyBackgroundSettings() As BackgroundSettings
		Get
			Return _myBackgroundSettings
		End Get
		Set
			_myBackgroundSettings = value
		End Set
	End Property

	Public Property BackgroundStyle() As backgroundStyleType
		Get
			Return _myBackgroundSettings.StyleMode
		End Get
		Set
			_myBackgroundSettings.StyleMode = value
			NotifyPropertyChanged("BackgroundStyle")

			Select Case value
				Case backgroundStyleType.None
					TopColorVisibility = Visibility.Collapsed
					BottomColorVisibility = Visibility.Collapsed
					IntermediateColorVisibility = Visibility.Collapsed
					ImagesVisibility = Visibility.Collapsed
					Exit Select
				Case backgroundStyleType.Solid
					TopColorVisibility = Visibility.Visible
					BottomColorVisibility = Visibility.Collapsed
					IntermediateColorVisibility = Visibility.Collapsed
					ImagesVisibility = Visibility.Collapsed
					Exit Select
				Case backgroundStyleType.LinearGradient
					TopColorVisibility = Visibility.Visible
					BottomColorVisibility = Visibility.Visible
					IntermediateColorVisibility = Visibility.Collapsed
					ImagesVisibility = Visibility.Collapsed
					Exit Select
				Case backgroundStyleType.CubicGradient
					TopColorVisibility = Visibility.Visible
					BottomColorVisibility = Visibility.Visible
					IntermediateColorVisibility = Visibility.Visible
					ImagesVisibility = Visibility.Collapsed
					Exit Select
				Case backgroundStyleType.Image
					TopColorVisibility = Visibility.Collapsed
					BottomColorVisibility = Visibility.Collapsed
					IntermediateColorVisibility = Visibility.Collapsed
					ImagesVisibility = Visibility.Visible
					Exit Select
			End Select
		End Set
	End Property

	#Region "Background Colors"

	Public Property TopColor() As Color
		Get
			Return Helper.ConvertColor(_myBackgroundSettings.TopColor)
		End Get
		Set
			_myBackgroundSettings.TopColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("TopColor")
		End Set
	End Property

	Private _topColorVisibility As Visibility

	Public Property TopColorVisibility() As Visibility
		Get
			Return _topColorVisibility
		End Get
		Set
			_topColorVisibility = value
			NotifyPropertyChanged("TopColorVisibility")
		End Set
	End Property

	Public Property BottomColor() As Color
		Get
			Return Helper.ConvertColor(_myBackgroundSettings.BottomColor)
		End Get
		Set
			_myBackgroundSettings.BottomColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("BottomColor")
		End Set
	End Property

	Private _bottomColorVisibility As Visibility

	Public Property BottomColorVisibility() As Visibility
		Get
			Return _bottomColorVisibility
		End Get
		Set
			_bottomColorVisibility = value
			NotifyPropertyChanged("BottomColorVisibility")
		End Set
	End Property

	Public Property IntermediateColor() As Color
		Get
			Return Helper.ConvertColor(_myBackgroundSettings.IntermediateColor)
		End Get
		Set
			_myBackgroundSettings.IntermediateColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("IntermediateColor")
		End Set
	End Property

	Private _intermediateColorVisibility As Visibility

	Public Property IntermediateColorVisibility() As Visibility
		Get
			Return _intermediateColorVisibility
		End Get
		Set
			_intermediateColorVisibility = value
			NotifyPropertyChanged("IntermediateColorVisibility")
		End Set
	End Property

	Public Property IntermediateColorPosition() As Double
		Get
			Return _myBackgroundSettings.IntermediateColorPosition * 100
		End Get
		Set
			_myBackgroundSettings.IntermediateColorPosition = value / 100
			NotifyPropertyChanged("IntermediateColorPosition")
		End Set
	End Property

	Private _imagesVisibility As Visibility

	Public Property ImagesVisibility() As Visibility
		Get
			Return _imagesVisibility
		End Get
		Set
			_imagesVisibility = value
			NotifyPropertyChanged("ImagesVisibility")
		End Set
	End Property

	#End Region

	Public Property Image1() As System.Windows.Media.ImageSource
		Get
			Return m_Image1
		End Get
		Set
			m_Image1 = Value
		End Set
	End Property
	Private m_Image1 As System.Windows.Media.ImageSource
	Public Property Image2() As System.Windows.Media.ImageSource
		Get
			Return m_Image2
		End Get
		Set
			m_Image2 = Value
		End Set
	End Property
	Private m_Image2 As System.Windows.Media.ImageSource
	Public Property Image3() As System.Windows.Media.ImageSource
		Get
			Return m_Image3
		End Get
		Set
			m_Image3 = Value
		End Set
	End Property
	Private m_Image3 As System.Windows.Media.ImageSource

	#End Region

	#Region "Toolbar"
	Private _myToolBars As ObservableCollection(Of ToolBar)

    Public Property MyToolBars() As ObservableCollection(Of ToolBar)
	    Get
		    Return _myToolBars
	    End Get
	    Set
		    _myToolBars = value
		    NotifyPropertyChanged("MyToolBars")
	    End Set
    End Property

    Private ReadOnly Property MyToolBar() As ToolBar
	    Get
		    Return MyToolBars(0)
	    End Get
    End Property

	Public Property TbPositionType() As ToolBar.positionType
		Get
			Return MyToolBar.Position
		End Get
		Set
			MyToolBar.Position = value
			NotifyPropertyChanged("TbPositionType")
		End Set
	End Property

	Public Property TbVisible() As Boolean
		Get
			Return MyToolBar.Visible
		End Get
		Set
			MyToolBar.Visible = value
			NotifyPropertyChanged("TbVisible")
		End Set
	End Property

	#Region "Command"
	Private _buttonsCount As Integer

	Public Property AddToolbarButtonCommand() As ICommand
		Get
			Return m_AddToolbarButtonCommand
		End Get
		Friend Set
			m_AddToolbarButtonCommand = Value
		End Set
	End Property
	Private m_AddToolbarButtonCommand As ICommand

	Private Function CanExecuteAddToolbarButton(parameter As Object) As Boolean
		Return MyToolBar.Buttons.Count = _buttonsCount
	End Function

	Private Sub CreateAddToolbarButtonCommand()
		AddToolbarButtonCommand = New RelayCommand(AddressOf AddToolbarButtonExecute, AddressOf CanExecuteAddToolbarButton)            
	End Sub

	Public Sub AddToolbarButtonExecute(parameter As Object)
		Dim gearsBmp As New BitmapImage(Helper.GetUriFromResource("gears.png"))
		MyToolBar.Buttons.Add(New devDept.Eyeshot.ToolBarButton(gearsBmp, "MyCustomButton", "My custom button", devDept.Eyeshot.ToolBarButton.styleType.PushButton, True))
		CustomButtonVisibility = Visibility.Visible
	End Sub

	Public Property RemoveToolbarButtonCommand() As ICommand
		Get
			Return m_RemoveToolbarButtonCommand
		End Get
		Friend Set
			m_RemoveToolbarButtonCommand = Value
		End Set
	End Property
	Private m_RemoveToolbarButtonCommand As ICommand

	Private Function CanExecuteRemoveToolbarButton(parameter As Object) As Boolean
		Return MyToolBar.Buttons.Count > _buttonsCount
	End Function

	Private Sub CreateRemoveToolbarButtonCommand()
		RemoveToolbarButtonCommand = New RelayCommand(AddressOf RemoveToolbarButtonExecute, AddressOf CanExecuteRemoveToolbarButton)
	End Sub

	Public Sub RemoveToolbarButtonExecute(parameter As Object)
		MyToolBar.Buttons.RemoveAt(_buttonsCount)
		CustomButtonVisibility = Visibility.Collapsed
		CustomButtonVisible = True
	End Sub
	#End Region

	Public Property HomeButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(0).Visible
		End Get
		Set
			MyToolBar.Buttons(0).Visible = value
			NotifyPropertyChanged("HomeButtonVisible")
		End Set
	End Property

	Public Property MagnifyingGlassButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(1).Visible
		End Get
		Set
			MyToolBar.Buttons(1).Visible = value
			NotifyPropertyChanged("MagnifyingGlassButtonVisible")
		End Set
	End Property

	Public Property ZoomWindowButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(2).Visible
		End Get
		Set
			MyToolBar.Buttons(2).Visible = value
			NotifyPropertyChanged("ZoomWindowButtonVisible")
		End Set
	End Property

	Public Property ZoomButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(3).Visible
		End Get
		Set
			MyToolBar.Buttons(3).Visible = value
			NotifyPropertyChanged("ZoomButtonVisible")
		End Set
	End Property

	Public Property PanButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(4).Visible
		End Get
		Set
			MyToolBar.Buttons(4).Visible = value
			NotifyPropertyChanged("PanButtonVisible")
		End Set
	End Property

	Public Property RotateButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(5).Visible
		End Get
		Set
			MyToolBar.Buttons(5).Visible = value
			NotifyPropertyChanged("RotateButtonVisible")
		End Set
	End Property

	Public Property ZoomFitButtonVisible() As Boolean
		Get
			Return MyToolBar.Buttons(6).Visible
		End Get
		Set
			MyToolBar.Buttons(6).Visible = value
			NotifyPropertyChanged("ZoomFitButtonVisible")
		End Set
	End Property

	Public Property CustomButtonVisible() As Boolean
		Get
			Return If(MyToolBar.Buttons.Count = _buttonsCount, True, MyToolBar.Buttons(7).Visible)
		End Get
		Set
			If MyToolBar.Buttons.Count > _buttonsCount Then
				MyToolBar.Buttons(7).Visible = value
			End If
			NotifyPropertyChanged("CustomButtonVisible")
		End Set
	End Property

	Private _customButtonVisibility As Visibility = Visibility.Collapsed

	Public Property CustomButtonVisibility() As Visibility
		Get
			Return _customButtonVisibility
		End Get
		Set
			_customButtonVisibility = value
			NotifyPropertyChanged("CustomButtonVisibility")
		End Set
	End Property

	#End Region

	#Region "Grid"

	Private _myGrids As ObservableCollection(Of Grid)

	Public Property MyGrids() As ObservableCollection(Of Grid)
		Get
			Return _myGrids
		End Get
		Set
			_myGrids = value
			NotifyPropertyChanged("MyGrids")
		End Set
	End Property

	Public Property GridAlwaysBehind() As Boolean
		Get
			Return MyGrids(0).AlwaysBehind
		End Get
		Set
			MyGrids(0).AlwaysBehind = value

			NotifyPropertyChanged("GridAlwaysBehind")
		End Set
	End Property

	Public Property GridStep() As Double
		Get
			Return MyGrids(0).[Step]
		End Get
		Set
			MyGrids(0).[Step] = value
			NotifyPropertyChanged("GridStep")
		End Set
	End Property

	Public Property GridAutoSize() As Boolean
		Get
			Return MyGrids(0).AutoSize
		End Get
		Set
			MyGrids(0).AutoSize = value
			NotifyPropertyChanged("GridAutoSize")
		End Set
	End Property

	Public Property GridAutoStep() As Boolean
		Get
			Return MyGrids(0).AutoStep
		End Get
		Set
			MyGrids(0).AutoStep = value
			NotifyPropertyChanged("GridAutoStep")

			If value Then
				GridNumberOfLinesVisibility = Visibility.Visible
			Else
				GridNumberOfLinesVisibility = Visibility.Collapsed
			End If
		End Set
	End Property

	Private _gridNumberOfLinesVisibility As Visibility = Visibility.Collapsed

	Public Property GridNumberOfLinesVisibility() As Visibility
		Get
			Return _gridNumberOfLinesVisibility
		End Get
		Set
			_gridNumberOfLinesVisibility = value
			NotifyPropertyChanged("GridNumberOfLinesVisibility")
		End Set
	End Property


	Public Property GridMinNumberOfLines() As Integer
		Get
			Return MyGrids(0).MinNumberOfLines
		End Get
		Set
			MyGrids(0).MinNumberOfLines = value
			NotifyPropertyChanged("GridMinNumberOfLines")
		End Set
	End Property

	Public Property GridMaxNumberOfLines() As Integer
		Get
			Return MyGrids(0).MaxNumberOfLines
		End Get
		Set
			MyGrids(0).MaxNumberOfLines = value
			NotifyPropertyChanged("GridMaxNumberOfLines")
		End Set
	End Property

	Public Property GridVisible() As Boolean
		Get
			Return MyGrids(0).Visible
		End Get
		Set
			MyGrids(0).Visible = value
			NotifyPropertyChanged("GridVisible")
		End Set
	End Property

	Public Property GridLineColor() As Color
		Get
			Return Helper.ConvertColor(MyGrids(0).LineColor)
		End Get
		Set
			MyGrids(0).LineColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("GridLineColor")
		End Set
	End Property

	Public Property GridColorAxisX() As Color
		Get
			Return Helper.ConvertColor(MyGrids(0).ColorAxisX)
		End Get
		Set
			MyGrids(0).ColorAxisX = Helper.ConvertColor(value)
			NotifyPropertyChanged("GridColorAxisX")
		End Set
	End Property

	Public Property GridColorAxisY() As Color
		Get
			Return Helper.ConvertColor(MyGrids(0).ColorAxisY)
		End Get
		Set
			MyGrids(0).ColorAxisY = Helper.ConvertColor(value)
			NotifyPropertyChanged("GridColorAxisY")
		End Set
	End Property

	Public Property GridFillColor() As Color
		Get
			Return Helper.ConvertColor(MyGrids(0).FillColor)
		End Get
		Set
			MyGrids(0).FillColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("GridFillColor")
		End Set
	End Property

	Public Property GridMajorLinesEvery() As Integer
		Get
			Return MyGrids(0).MajorLinesEvery
		End Get
		Set
			MyGrids(0).MajorLinesEvery = value

			NotifyPropertyChanged("GridMajorLinesEvery")
		End Set
	End Property

	Public Property GridMajorLineColor() As Color
		Get
			Return Helper.ConvertColor(MyGrids(0).MajorLineColor)
		End Get
		Set
			MyGrids(0).MajorLineColor = Helper.ConvertColor(value)

			NotifyPropertyChanged("GridMajorLineColor")
		End Set
	End Property

	#End Region

	#Region "Coordinate System Icon"

	Private _myCoordinateSystemIcon As CoordinateSystemIcon

	Public Property MyCoordinateSystemIcon() As CoordinateSystemIcon
		Get
			Return _myCoordinateSystemIcon
		End Get
		Set
			_myCoordinateSystemIcon = value
			NotifyPropertyChanged("MyCoordinateSystemIcon")
		End Set
	End Property

	Public Property CsiVisible() As Boolean
		Get
			Return MyCoordinateSystemIcon.Visible
		End Get
		Set
			MyCoordinateSystemIcon.Visible = value
			NotifyPropertyChanged("CsiVisible")
		End Set
	End Property

	Public Property CsiPositionType() As coordinateSystemPositionType
		Get
			Return MyCoordinateSystemIcon.Position
		End Get
		Set
			MyCoordinateSystemIcon.Position = value
			NotifyPropertyChanged("CsiPositionType")
		End Set
	End Property

	Public Property CsiLabelColor() As Color
		Get
			Return Helper.ConvertColor(MyCoordinateSystemIcon.LabelColor)
		End Get
		Set
			MyCoordinateSystemIcon.LabelColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("CsiLabelColor")
		End Set
	End Property

	Private _csiArrowVisibility As Visibility

	Public Property CsiArrowVisibility() As Visibility
		Get
			Return _csiArrowVisibility
		End Get
		Set
			_csiArrowVisibility = value
			NotifyPropertyChanged("CsiArrowVisibility")
		End Set
	End Property


	Public Property CsiArrowColorX() As Color
		Get
			Return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorX)
		End Get
		Set
			MyCoordinateSystemIcon.ArrowColorX = Helper.ConvertColor(value)
			NotifyPropertyChanged("CsiArrowColorX")
		End Set
	End Property

	Public Property CsiArrowColorY() As Color
		Get
			Return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorY)
		End Get
		Set
			MyCoordinateSystemIcon.ArrowColorY = Helper.ConvertColor(value)
			NotifyPropertyChanged("CsiArrowColorY")
		End Set
	End Property

	Public Property CsiArrowColorZ() As Color
		Get
			Return Helper.ConvertColor(MyCoordinateSystemIcon.ArrowColorZ)
		End Get
		Set
			MyCoordinateSystemIcon.ArrowColorZ = Helper.ConvertColor(value)
			NotifyPropertyChanged("CsiArrowColorZ")
		End Set
	End Property

	Public Property CsiLabelX() As String
		Get
			Return MyCoordinateSystemIcon.LabelAxisX
		End Get
		Set
			MyCoordinateSystemIcon.LabelAxisX = value
			NotifyPropertyChanged("CsiLabelX")
		End Set
	End Property

	Public Property CsiLabelY() As String
		Get
			Return MyCoordinateSystemIcon.LabelAxisY
		End Get
		Set
			MyCoordinateSystemIcon.LabelAxisY = value
			NotifyPropertyChanged("CsiLabelY")
		End Set
	End Property

	Public Property CsiLabelZ() As String
		Get
			Return MyCoordinateSystemIcon.LabelAxisZ
		End Get
		Set
			MyCoordinateSystemIcon.LabelAxisZ = value
			NotifyPropertyChanged("CsiLabelZ")
		End Set
	End Property

	#End Region

	#Region "Origin Symbol"

	Private _myOriginSymbol As OriginSymbol

	Public Property MyOriginSymbol() As OriginSymbol
		Get
			Return _myOriginSymbol
		End Get
		Set
			_myOriginSymbol = value
			NotifyPropertyChanged("MyOriginSymbol")
		End Set
	End Property

	Public Property OsVisible() As Boolean
		Get
			Return MyOriginSymbol.Visible
		End Get
		Set
			MyOriginSymbol.Visible = value
			NotifyPropertyChanged("OsVisible")
		End Set
	End Property

	Public Property OsStyleType() As originSymbolStyleType
		Get
			Return MyOriginSymbol.StyleMode
		End Get
		Set
			MyOriginSymbol.StyleMode = value
			If value = originSymbolStyleType.CoordinateSystem Then
				OsArrowVisibility = Visibility.Visible
			Else
				OsArrowVisibility = Visibility.Collapsed
			End If
			NotifyPropertyChanged("OsStyleType")
		End Set
	End Property

	Public Property OsLabelColor() As Color
		Get
			Return Helper.ConvertColor(MyOriginSymbol.LabelColor)
		End Get
		Set
			MyOriginSymbol.LabelColor = Helper.ConvertColor(value)
			NotifyPropertyChanged("OsLabelColor")
		End Set
	End Property

	Private _osArrowVisibility As Visibility

	Public Property OsArrowVisibility() As Visibility
		Get
			Return _osArrowVisibility
		End Get
		Set
			_osArrowVisibility = value
			NotifyPropertyChanged("OsArrowVisibility")
		End Set
	End Property


	Public Property OsArrowColorX() As Color
		Get
			Return Helper.ConvertColor(MyOriginSymbol.ArrowColorX)
		End Get
		Set
			MyOriginSymbol.ArrowColorX = Helper.ConvertColor(value)
			NotifyPropertyChanged("OsArrowColorX")
		End Set
	End Property

	Public Property OsArrowColorY() As Color
		Get
			Return Helper.ConvertColor(MyOriginSymbol.ArrowColorY)
		End Get
		Set
			MyOriginSymbol.ArrowColorY = Helper.ConvertColor(value)
			NotifyPropertyChanged("OsArrowColorY")
		End Set
	End Property

	Public Property OsArrowColorZ() As Color
		Get
			Return Helper.ConvertColor(MyOriginSymbol.ArrowColorZ)
		End Get
		Set
			MyOriginSymbol.ArrowColorZ = Helper.ConvertColor(value)
			NotifyPropertyChanged("OsArrowColorZ")
		End Set
	End Property

	Public Property OsLabelX() As String
		Get
			Return MyOriginSymbol.LabelAxisX
		End Get
		Set
			MyOriginSymbol.LabelAxisX = value
			NotifyPropertyChanged("OsLabelX")
		End Set
	End Property

	Public Property OsLabelY() As String
		Get
			Return MyOriginSymbol.LabelAxisY
		End Get
		Set
			MyOriginSymbol.LabelAxisY = value
			NotifyPropertyChanged("OsLabelY")
		End Set
	End Property

	Public Property OsLabelZ() As String
		Get
			Return MyOriginSymbol.LabelAxisZ
		End Get
		Set
			MyOriginSymbol.LabelAxisZ = value
			NotifyPropertyChanged("OsLabelZ")
		End Set
	End Property

	#End Region

	#Region "ViewCubeIcon"

	Private _viewCubeVisible As Boolean = True
	Public Property ViewCubeVisible() As Boolean
		Get
			Return _viewCubeVisible
		End Get
		Set
			_viewCubeVisible = value
			NotifyPropertyChanged("ViewCubeVisible")
		End Set
	End Property

	Private _viewCubeShowRing As Boolean = True
	Public Property ViewCubeShowRing() As Boolean
		Get
			Return _viewCubeShowRing
		End Get
		Set
			_viewCubeShowRing = value
			NotifyPropertyChanged("ViewCubeShowRing")
		End Set
	End Property

	Private _viewCubeColor As Color = Color.FromArgb(240, 77, 77, 77)
	Public Property ViewCubeColor() As Color
		Get
			Return _viewCubeColor
		End Get
		Set
			_viewCubeColor = value
			NotifyPropertyChanged("ViewCubeColor")
		End Set
	End Property

	Private _frontLabel As String = "FRONT"
	Public Property FrontLabel() As String
		Get
			Return _frontLabel
		End Get
		Set
			_frontLabel = value
			NotifyPropertyChanged("FrontLabel")
		End Set
	End Property

	Private _backLabel As String = "BACK"
	Public Property BackLabel() As String
		Get
			Return _backLabel
		End Get
		Set
			_backLabel = value
			NotifyPropertyChanged("BackLabel")
		End Set
	End Property

	Private _rightLabel As String = "RIGHT"
	Public Property RightLabel() As String
		Get
			Return _rightLabel
		End Get
		Set
			_rightLabel = value
			NotifyPropertyChanged("RightLabel")
		End Set
	End Property

	Private _leftLabel As String = "LEFT"
	Public Property LeftLabel() As String
		Get
			Return _leftLabel
		End Get
		Set
			_leftLabel = value
			NotifyPropertyChanged("LeftLabel")
		End Set
	End Property

	Private _topLabel As String = "TOP"

	Public Property TopLabel() As String
		Get
			Return _topLabel
		End Get
		Set
			_topLabel = value
			NotifyPropertyChanged("TopLabel")
		End Set
	End Property

	Private _bottomLabel As String = "BOTTOM"
	Public Property BottomLabel() As String
		Get
			Return _bottomLabel
		End Get
		Set
			_bottomLabel = value
			NotifyPropertyChanged("BottomLabel")
		End Set
	End Property

	Private _vcLabelVisibility As Visibility

	Public Property VcLabelVisibility() As Visibility
		Get
			Return _vcLabelVisibility
		End Get
		Set
			_vcLabelVisibility = value
			NotifyPropertyChanged("VcLabelVisibility")
		End Set
	End Property

	Public Property VcResetImages() As System.Windows.Media.ImageSource
		Get
			Return m_VcResetImages
		End Get
		Set
			m_VcResetImages = Value
		End Set
	End Property
	Private m_VcResetImages As System.Windows.Media.ImageSource
	Public Property VcImage1() As System.Windows.Media.ImageSource
		Get
			Return m_VcImage1
		End Get
		Set
			m_VcImage1 = Value
		End Set
	End Property
	Private m_VcImage1 As System.Windows.Media.ImageSource
	Public Property VcImage2() As System.Windows.Media.ImageSource
		Get
			Return m_VcImage2
		End Get
		Set
			m_VcImage2 = Value
		End Set
	End Property
	Private m_VcImage2 As System.Windows.Media.ImageSource

	Private _vcFaceImages As System.Windows.Media.ImageSource() = New ImageSource(5) {}
	Public Property VcFaceImages() As System.Windows.Media.ImageSource()
		Get
			Return _vcFaceImages
		End Get
		Set
			If value Is Nothing Then
				_vcFaceImages(0) = InlineAssignHelper(_vcFaceImages(1), InlineAssignHelper(_vcFaceImages(2), InlineAssignHelper(_vcFaceImages(3), InlineAssignHelper(_vcFaceImages(4), InlineAssignHelper(_vcFaceImages(5), Nothing)))))

				VcLabelVisibility = Visibility.Visible
			Else
				_vcFaceImages = value

				VcLabelVisibility = Visibility.Collapsed
			End If

			NotifyPropertyChanged("VcFaceImages")
		End Set
	End Property

	#End Region

	#Region "EntityList"
	Private _entityList As MyEntityList

	Public Property EntityList() As MyEntityList
		Get
			Return _entityList
		End Get
		Set
			_entityList = value
			NotifyPropertyChanged("ViewportEntities")
		End Set
	End Property	
	#End Region

    Private Const Pictures As String = "../../../../../../dataset/Assets/Pictures/"
    Public Sub New()
        ' Initializes the Coordinate System Icon for the data binding.
        MyCoordinateSystemIcon = New CoordinateSystemIcon()

        ' Initializes the Origin Symbol for the data binding.
        MyOriginSymbol = OriginSymbol.GetDefaultOriginSymbol()
        OsStyleType = originSymbolStyleType.Ball

        ' Initializes the Grids collection for the data binding.
        Dim grid = New Grid()
        grid.Step = 10
        grid.MajorLinesEvery = 4
        MyGrids = New ObservableCollection(Of Grid)() From {grid}

        ' Initializes the ToolBar for the data binding.
        MyToolBars = New ObservableCollection(Of ToolBar)(New List(Of ToolBar)() From { ToolBar.GetDefaultToolBar() })
        MyToolBar.Position = ToolBar.positionType.HorizontalTopCenter
        ' Uses toolbar buttons count info to enable/disable the Add/Remove buttons
        _buttonsCount = MyToolBar.Buttons.Count
        ' Creates the command for the the Add/Remove buttons
        CreateAddToolbarButtonCommand()
        CreateRemoveToolbarButtonCommand()

        ' Initializes the BackgroundSettings for the the data binding.
        MyBackgroundSettings = New BackgroundSettings(backgroundStyleType.Solid, Helper.ConvertColor("#FF434752"), System.Drawing.Color.White, Helper.ConvertColor("#FFEDEDED"), 0.75, Nothing)

        ' Sets the ViewModel's BackgroundStyle: in this way the "Background" comboboxes will be updated too.
        BackgroundStyle = MyBackgroundSettings.StyleMode

        ' Initializes the Background images.
        MyBackgroundSettings.Image = InlineAssignHelper(Image1, RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background1.jpg")))
        Image2 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background2.jpg"))
        Image3 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"background3.jpg"))

        ' Initializes the Images for the ViewCube buttons.
        VcResetImages = ViewCubeIcon.GetDefaultViewCubeIcon().FrontImage
        VcImage1 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Front.jpg"))
        VcImage2 = RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Front.jpg"))

        ' Initializes the EntitiList collection for the the data binding.
        _entityList = New MyEntityList()
    End Sub

	#Region "INotifyPropertyChanged"
	Public Event INotifyPropertyChanged_PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

	Private Sub NotifyPropertyChanged(info As [String])
		RaiseEvent INotifyPropertyChanged_PropertyChanged(Me, New PropertyChangedEventArgs(info))
	End Sub
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
	#End Region

		
End Class
