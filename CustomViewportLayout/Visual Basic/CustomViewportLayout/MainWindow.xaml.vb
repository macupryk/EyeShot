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
Imports devDept.Eyeshot.Entities
Imports devDept.Graphics

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()

        'myModel1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        Dim cylinder As Mesh = Mesh.CreateCylinder(10, 20, 36)

        cylinder.Translate(20, 20, 0)

        myModel1.Entities.Add(cylinder, System.Drawing.Color.DarkGoldenrod)

        Dim cone As Mesh = Mesh.CreateCone(15, 0, 10, 36)

        myModel1.Entities.Add(cone, System.Drawing.Color.Khaki)

        Dim box As Mesh = Mesh.CreateBox(12, 12, 12)

        box.Translate(-10, 20, 0)

        myModel1.Entities.Add(box, System.Drawing.Color.DarkRed)

        ' sets trimetric view for main viewport            
        myModel1.Viewports(0).SetView(viewType.Trimetric)
        myModel1.Viewports(0).Grid.Visible = False
        myModel1.Viewports(0).ZoomFit()

        ' sets top view for secondary viewport            
        myModel1.Viewports(1).SetView(viewType.Top)
        myModel1.Viewports(1).DisplayMode = displayType.Shaded
        myModel1.Viewports(1).Camera.ProjectionMode = projectionType.Orthographic
        myModel1.Viewports(1).ZoomFit()

        myModel1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class