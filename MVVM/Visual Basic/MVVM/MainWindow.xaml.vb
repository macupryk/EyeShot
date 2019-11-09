Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports WpfApplication1.WpfApplication1
Imports ToolBar = devDept.Eyeshot.ToolBar


''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Private Const Pictures As String = "../../../../../../dataset/Assets/Pictures/"

    Private _myViewModel As MyViewModel
    Private _rand As New Random(123)
    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        _myViewModel = DirectCast(DataContext, MyViewModel)
        _myViewModel.Lighting = False

        For Each value As colorThemeType In [Enum].GetValues(GetType(colorThemeType))
	        colorThemeTypes.Items.Add(value)
        Next

        For Each value As displayType In [Enum].GetValues(GetType(displayType))
	        displayTypes.Items.Add(value)
        Next

        For Each value As backgroundStyleType In [Enum].GetValues(GetType(backgroundStyleType))
	        styles.Items.Add(value)
        Next

        For Each value As coordinateSystemPositionType In [Enum].GetValues(GetType(coordinateSystemPositionType))
	        csiPositionTypes.Items.Add(value)
        Next

        For Each value As originSymbolStyleType In [Enum].GetValues(GetType(originSymbolStyleType))
	        osStyleTypes.Items.Add(value)
        Next

        For Each value As ToolBar.positionType In [Enum].GetValues(GetType(ToolBar.positionType))
	        tbPositionTypes.Items.Add(value)
        Next

        actionTypes.Items.Add(actionType.None)
        actionTypes.Items.Add(actionType.SelectByPick)
        actionTypes.Items.Add(actionType.SelectByBox)
        actionTypes.Items.Add(actionType.SelectVisibleByPick)
        actionTypes.Items.Add(actionType.SelectVisibleByBox)

        colors.SelectedIndex = 1
        
    End Sub

    Private Sub BtnAddEntity_OnClick(sender As Object, e As RoutedEventArgs)
        Dim randomColor = System.Drawing.Color.FromArgb(255, CByte(_rand.[Next](255)), CByte(_rand.[Next](255)), CByte(_rand.[Next](255)))
	    Dim translateX = _rand.[Next](100) * -5
	    Dim translateY = _rand.[Next](100) * -5
	    Dim translateZ = _rand.[Next](100) * -5
#If STANDARD Then
	    Dim faces = GetBoxFaces()
	    For Each entity As Entity In faces
		    entity.Color = randomColor
		    entity.ColorMethod = colorMethodType.byEntity
		    entity.Translate(translateX, translateY, translateZ)
	    Next
	    _myViewModel.EntityList.AddRange(faces)
#Else
	    Dim m As Mesh = Mesh.CreateBox(50, 50, 50)
	    m.Color = randomColor
	    m.ColorMethod = colorMethodType.byEntity
	    m.Translate(translateX, translateY, translateZ)

	    _myViewModel.EntityList.Add(m)
#End If
	    model1.ZoomFit()
    End Sub

#If STANDARD Then
    Private Function GetBoxFaces() As List(Of Entity)
	    Dim boxWidth As Double = 50
	    Dim boxDepth As Double = 50
	    Dim boxHeight As Double = 50

	    Dim boxBottom As New Quad(0, boxDepth, 0, boxWidth, boxDepth, 0, _
		    boxWidth, 0, 0, 0, 0, 0)

	    Dim boxTop As New Quad(0, boxDepth, boxHeight, boxWidth, boxDepth, boxHeight, _
		    boxWidth, 0, boxHeight, 0, 0, boxHeight)

	    Dim boxFront As New Quad(0, 0, 0, boxWidth, 0, 0, _
		    boxWidth, 0, boxHeight, 0, 0, boxHeight)

	    Dim boxRight As New Quad(boxWidth, 0, 0, boxWidth, boxDepth, 0, _
		    boxWidth, boxDepth, boxHeight, boxWidth, 0, boxHeight)

	    Dim boxRear As New Quad(boxWidth, boxDepth, 0, 0, boxDepth, 0, _
		    0, boxDepth, boxHeight, boxWidth, boxDepth, boxHeight)

	    Dim boxLeft As New Quad(0, boxDepth, 0, 0, 0, 0, _
		    0, 0, boxHeight, 0, boxDepth, boxHeight)

	    Return New List(Of Entity)() From { _
		    boxBottom, _
		    boxTop, _
		    boxFront, _
		    boxRight, _
		    boxRear, _
		    boxLeft _
	    }
    End Function
#End If

    Private Sub BtnRemoveEntities_OnClick(sender As Object, e As RoutedEventArgs)
        _myViewModel.EntityList.RemoveRange(_myViewModel.EntityList.Where(Function(x) x.Selected).ToList())
    End Sub

    Private Sub BtnImage_OnClick(sender As Object, e As RoutedEventArgs)
        Dim b As ToggleButton = DirectCast(sender, ToggleButton)
        Dim isChecked As Boolean = b.IsChecked.Value
        Dim image As Controls.Image = DirectCast(b.Content, Controls.Image)

        _myViewModel.MyBackgroundSettings.Image = image.Source

        btnImage1.IsChecked = False
        btnImage2.IsChecked = False
        btnImage3.IsChecked = False

        b.IsChecked = isChecked
    End Sub

    Private Sub Colors_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Select Case DirectCast(colors.SelectedItem, ObservableCollection(Of Media.Brush)).Count
            Case 9
                If True Then
                    _myViewModel.LegendItemSize = "10,30"
                    Exit Select
                End If
            Case 17
                If True Then
                    _myViewModel.LegendItemSize = "10,25"
                    Exit Select
                End If
            Case 33
                If True Then
                    _myViewModel.LegendItemSize = "10,15"
                    Exit Select
                End If
        End Select
    End Sub

#Region "ViewCube Images"

    Private Sub BtnVcImage_OnClick(sender As Object, e As RoutedEventArgs)
        Dim b As ToggleButton = DirectCast(sender, ToggleButton)
        Dim isChecked As Boolean = b.IsChecked.Value

        btnVcResetImages.IsChecked = False
        btnVcImage1.IsChecked = False
        btnVcImage2.IsChecked = False

        b.IsChecked = isChecked
    End Sub

    Private Sub BtnVcResetImages_OnChecked(sender As Object, e As RoutedEventArgs)
        If _myViewModel IsNot Nothing Then
            _myViewModel.VcFaceImages = Nothing
        End If
    End Sub

    Private Sub BtnVcImage1_OnChecked(sender As Object, e As RoutedEventArgs)
        If _myViewModel IsNot Nothing Then
            _myViewModel.VcFaceImages = New ImageSource() {RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Front.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Back.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Top.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Bottom.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Left.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Spongebob_Right.jpg"))}
        End If
    End Sub

    Private Sub BtnVcImage2_OnChecked(sender As Object, e As RoutedEventArgs)
        If _myViewModel IsNot Nothing Then
            _myViewModel.VcFaceImages = New ImageSource() {RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Front.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Back.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Top.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Bottom.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Left.jpg")),RenderContextUtility.ConvertImage(new Bitmap(Pictures+"Noel_Right.jpg"))}
        End If
    End Sub

#End Region

End Class
