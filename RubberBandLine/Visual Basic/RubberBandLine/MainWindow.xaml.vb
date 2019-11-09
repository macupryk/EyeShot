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
        ' Adds a beige semi-transparent rectangle
        Dim p1 As New Point3D(0, 0, 0)
        Dim p2 As New Point3D(100, 0, 0)
        Dim p3 As New Point3D(100, 80, 40)
        Dim p4 As New Point3D(0, 80, 40)

        model1.Entities.Add(New Quad(p1, p2, p3, p4), System.Drawing.Color.FromArgb(127, System.Drawing.Color.Beige))

        model1.SetPlane(p1, p2, p4)

        ' Sets trimetric view
        model1.SetView(viewType.Trimetric)

        ' Fits the model in the viewport
        model1.ZoomFit()
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class
