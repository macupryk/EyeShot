Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot
Imports devDept.Graphics

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        AddHandler model1.SelectionChanged, AddressOf Model_OnSelectionChanged
    End Sub

    Private Sub Model_OnSelectionChanged(sender As Object, e As EventArgs)
        Dim mfp As MyFastPointCloud = Nothing

        If GetSelectedPointCloud(mfp) Then
            model1.Entities.Regen()
            model1.Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        model1.GetGrid().Visible = False

        ' generates point cloud points
        Dim ent As Entity = FunctionPlot()

        ' adds it to the vieport
        model1.Entities.Add(ent)

        ' Sets trimetric view
        model1.SetView(viewType.Trimetric)

        ' Fits the model in the viewport
        model1.ZoomFit()

        ' Refresh the viewport
        model1.Invalidate()

        ' Sets the SelectByPolygon action mode
        model1.ActionMode = actionType.SelectByPolygon

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs)
        Dim mfp As MyFastPointCloud = Nothing

        If GetSelectedPointCloud(mfp) Then
            mfp.DeletePoints()

            model1.Entities.Regen()

            model1.Invalidate()
        End If
    End Sub

    Private Sub undoButton_Click(sender As Object, e As RoutedEventArgs)
        Dim mfp As MyFastPointCloud = Nothing

        If GetSelectedPointCloud(mfp) Then
            mfp.Undo()

            model1.Entities.Regen()

            model1.Invalidate()
        End If
    End Sub

    Private Sub unselectButton_Click(sender As Object, e As RoutedEventArgs)
        Dim mfp As MyFastPointCloud = Nothing

        If GetSelectedPointCloud(mfp) Then
            mfp.Unselect()

            model1.Entities.Regen()

            model1.Invalidate()
        End If
    End Sub

    Private Function GetSelectedPointCloud(ByRef mfp As MyFastPointCloud) As Boolean
        For Each ent As Entity In model1.Entities
            If TypeOf ent Is MyFastPointCloud Then
                mfp = DirectCast(ent, MyFastPointCloud)

                If mfp.CustomSelected Then

                    Return True
                End If
            End If
        Next

        mfp = Nothing

        Return False
    End Function


    ''' <summary>
    ''' Draws a point cloud of 80x80 vertices
    ''' </summary>
    Public Function FunctionPlot() As FastPointCloud

        Dim rows As Integer = 80
        Dim cols As Integer = 80
        Dim scale As Single = 4.0F

        Dim surface As New PointCloud(rows * cols, 3, PointCloud.natureType.Multicolor)

        For j As Integer = 0 To rows - 1

            For i As Integer = 0 To cols - 1

                Dim x As Single = i / 5.0F
                Dim y As Single = j / 5.0F

                Dim f As Single = 0

                Dim den As Single = CSng(Math.Sqrt(x * x + y * y))

                If den <> 0 Then

                    f = scale * CSng(Math.Sin(Math.Sqrt(x * x + y * y))) / den
                End If


                surface.Vertices(i + j * cols) = New PointRGB(x, y, f, MyFastPointCloud.BaseColor, MyFastPointCloud.BaseColor, MyFastPointCloud.BaseColor)
            Next
        Next

        Dim surfaceFast As New MyFastPointCloud(surface.ConvertToFastPointCloud(), model1)

        surfaceFast.LineWeightMethod = colorMethodType.byEntity
        surfaceFast.LineWeight = 2

        Return surfaceFast

    End Function

    Private Class MyFastPointCloud
        Inherits FastPointCloud
        Public model As Model
        Private selectedCount As Integer = 0

        Private _customSelected As Boolean

        Friend Shared BaseColor As Byte = 150

        ' use a custom flag for selection, otherwise the entity will be drawn with selection color
        Public Property CustomSelected() As Boolean
            Get
                Return _customSelected
            End Get
            Set(value As Boolean)
                _customSelected = value
            End Set
        End Property

        ''' <summary>
        ''' Point list of the last delete action
        ''' </summary>
        Private lastDeleteContents As New List(Of Point3D)()

        Public Sub New(another As FastPointCloud, control As Model)
            MyBase.New(another)
            model = control
        End Sub

        Protected Overrides Function IsCrossingScreenPolygon(data As ScreenPolygonParams) As Boolean
            Dim newSelectedCount As Integer = 0

            For j As Integer = 0 To PointArray.Length - 1 Step 3
                Dim onScreen As Point2D = model.WorldToScreen(PointArray(j), PointArray(j + 1), PointArray(j + 2))

                If ColorArray(j) <> 255 AndAlso onScreen.X > data.Min.X AndAlso onScreen.X < data.Max.X AndAlso onScreen.Y > data.Min.Y AndAlso onScreen.Y < data.Max.Y AndAlso Utility.PointInPolygon(onScreen, data.ScreenPolygon) Then
                    ' sets point color to red
                    ColorArray(j) = 255

                    newSelectedCount += 1

                End If
            Next

            selectedCount += newSelectedCount

            If newSelectedCount <> 0 Then
                RegenMode = regenType.CompileOnly

                CustomSelected = True

            ElseIf selectedCount = 0 Then

                CustomSelected = False
            End If

            Return False
        End Function

        ''' <summary>
        ''' Deletes selected points
        ''' </summary>
        Public Sub DeletePoints()
            Dim count As Integer = 0
            Dim firstTime As Boolean = True

            ' fills the vertices array only with black points
            For j As Integer = 0 To PointArray.Length - 1 Step 3
                If ColorArray(j) = BaseColor Then
                    PointArray(count) = PointArray(j)
                    PointArray(count + 1) = PointArray(j + 1)
                    PointArray(count + 2) = PointArray(j + 2)
                    ColorArray(count) = ColorArray(j)
                    ColorArray(count + 1) = ColorArray(j + 1)
                    ColorArray(count + 2) = ColorArray(j + 2)
                    count += 3
                Else


                    If firstTime Then
                        lastDeleteContents.Clear()
                        firstTime = False
                    End If

                    lastDeleteContents.Add(New Point3D(PointArray(j), PointArray(j + 1), PointArray(j + 2)))
                End If
            Next

            ResizeVertices(PointArray.Length - (selectedCount * 3))

            RegenMode = regenType.RegenAndCompile

            selectedCount = 0
        End Sub

        ''' <summary>
        ''' Resizes the entity vertices array
        ''' </summary>
        Private Sub ResizeVertices(newSize As Integer)

            Dim v As Single() = PointArray

            Array.Resize(Of Single)(v, newSize)

            Dim b As Byte() = ColorArray

            Array.Resize(Of Byte)(b, newSize)

            PointArray = v

            ColorArray = b
        End Sub

        ''' <summary>
        ''' Undo deletion.
        ''' </summary>
        Public Sub Undo()
            Dim prevLen As Integer = PointArray.Length

            ResizeVertices(PointArray.Length + (lastDeleteContents.Count * 3))

            ' adds back the points to the point cloud
            Dim i As Integer = 0, j As Integer = 0
            While i < lastDeleteContents.Count * 3
                PointArray(prevLen + i) = CSng(lastDeleteContents(j).X)
                PointArray(prevLen + i + 1) = CSng(lastDeleteContents(j).Y)
                PointArray(prevLen + i + 2) = CSng(lastDeleteContents(j).Z)
                ColorArray(prevLen + i) = 255
                ColorArray(prevLen + i + 1) = BaseColor
                ColorArray(prevLen + i + 2) = BaseColor
                i += 3
                j += 1
            End While

            selectedCount += lastDeleteContents.Count

            lastDeleteContents.Clear()

            RegenMode = regenType.RegenAndCompile
        End Sub

        ''' <summary>
        ''' Unselect points
        ''' </summary>
        Public Sub Unselect()
            ' set the color of each point to black
            For i As Integer = 0 To PointArray.Length - 1 Step 3

                ColorArray(i) = BaseColor
            Next

            selectedCount = 0

            RegenMode = regenType.CompileOnly
        End Sub

    End Class

End Class
