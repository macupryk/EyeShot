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
Imports devDept.Geometry
Imports devDept.Graphics

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        model1.Camera.ProjectionMode = projectionType.Orthographic

        ' Set the plane of the ruler
        model1.RulerPlaneMode = MyModel.rulerPlaneType.XY

        ' Set the correct orientation
        SetOrientation(model1.RulerPlaneMode)

        Dim circle As New Circle(Point3D.Origin, 20)
        model1.Entities.Add(circle, System.Drawing.Color.Red)

        ' Ruler is for XY plane, so disable rotation
        model1.Rotate.Enabled = False

        ' Disable the view cube because we don't want the user to change view orientation
        model1.GetViewCubeIcon().Visible = False

        model1.ZoomFit()

        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub SetOrientation(planeMode As MyModel.rulerPlaneType)
        Select Case planeMode
            Case MyModel.rulerPlaneType.XY
                model1.SetView(viewType.Top)
                Exit Select

            Case MyModel.rulerPlaneType.YZ
                model1.SetView(viewType.Right)
                Exit Select

            Case MyModel.rulerPlaneType.ZX
                model1.SetView(viewType.Front)
                Exit Select

        End Select

    End Sub
End Class

