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
Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)

        '#Region "Black wire cube"

        model1.Entities.Add(New Line(0, 0, 0, 0, 0, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 0, 0, 100, 0, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 100, 0, 100, 100, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(0, 100, 0, 0, 100, 100), System.Drawing.Color.Black)

        model1.Entities.Add(New Line(0, 0, 0, 100, 0, 0), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 0, 0, 100, 100, 0), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 100, 0, 0, 100, 0), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(0, 100, 0, 0, 0, 0), System.Drawing.Color.Black)

        model1.Entities.Add(New Line(0, 0, 100, 100, 0, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 0, 100, 100, 100, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(100, 100, 100, 0, 100, 100), System.Drawing.Color.Black)
        model1.Entities.Add(New Line(0, 100, 100, 0, 0, 100), System.Drawing.Color.Black)

        '#End Region

        '#Region "Front side (with the help of MoveToPlane() method)"

        Dim c1 As New Circle(50, 50, 0, 40)
        Dim t1 As New Text(50, 50, 0, "Front side", 18)

        t1.Rotate(Math.PI / 4, Vector3D.AxisZ, New Point3D(50, 50, 0))
        t1.Alignment = Text.alignmentType.MiddleCenter

        Dim myList As New List(Of Entity)()

        myList.Add(c1)
        myList.Add(t1)

        model1.MoveToPlane(myList, New Plane(Point3D.Origin, New Point3D(100, 0, 0), New Point3D(0, 0, 100)))

        model1.Entities.AddRange(myList, System.Drawing.Color.DarkViolet)

        '#End Region

        '#Region "Right side (using sketch plane)"

        Dim sketchPlane As New Plane(New Point3D(100, 0, 0), New Point3D(100, 100, 0), New Point3D(100, 0, 100))

        Dim c2 As New Circle(sketchPlane, New Point2D(50, 50), 40)
        Dim a2 As New Arc(sketchPlane, New Point2D(50, 50), 45, Math.PI / 2, 2 * Math.PI)
        Dim t2 As New Text(sketchPlane, New Point2D(50, 50), "Right side", 18, Text.alignmentType.MiddleCenter)

        t2.Rotate(Math.PI / 4, Vector3D.AxisX, New Point3D(0, 50, 50))

        model1.Entities.Add(c2, System.Drawing.Color.Red)
        model1.Entities.Add(a2, System.Drawing.Color.Red)
        model1.Entities.Add(t2, System.Drawing.Color.Red)

        '#End Region

        '#Region "Rear side (with the help of Entity.Translate() and Rotate() methods)"

        Dim c3 As New Circle(Point3D.Origin, 40)

        c3.Translate(50, 50, -100)
        c3.Rotate(Math.PI / 2, Vector3D.AxisX)

        model1.Entities.Add(c3, System.Drawing.Color.Blue)

        Dim t3 As New Text(0, 0, 0, "Rear side", 18)

        t3.Alignment = Text.alignmentType.MiddleCenter

        t3.Rotate(Math.PI / 2, Vector3D.AxisX)
        t3.Rotate(Math.PI, Vector3D.AxisZ)
        t3.Rotate(Math.PI / 4, Vector3D.AxisY)
        t3.Translate(50, 100, 50)

        model1.Entities.Add(t3, System.Drawing.Color.Blue)

        '#End Region

        '#Region "Left side (with the help of the Transformation class)"

        Dim frame As New Transformation(New Point3D(0, 50, 50), New Vector3D(0, -1, 0), Vector3D.AxisZ, New Vector3D(-1, 0, 0))

        Dim c4 As New Circle(0, 0, 0, 40)

        c4.TransformBy(frame)

        model1.Entities.Add(c4, System.Drawing.Color.Green)

        Dim t4 As New Text(0, 0, 0, "Left side", 18)

        t4.Alignment = Text.alignmentType.MiddleCenter

        t4.Rotate(Math.PI / 4, Vector3D.AxisZ)

        t4.TransformBy(frame)

        model1.Entities.Add(t4, System.Drawing.Color.Green)

        '#End Region

        ' set trimetric view
        model1.SetView(viewType.Trimetric)

        ' fits the model in the viewport
        model1.ZoomFit()
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class

