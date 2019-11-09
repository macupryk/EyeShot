''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Derived from the demo Camera2 of dhpoware http://www.dhpoware.com/ '
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

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
Imports devDept.Geometry
Imports devDept.Graphics

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    

    Public Sub New()
        InitializeComponent()

        'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        model1.ViewportBorder.Visible = False
        model1.ViewportBorder.CornerRadius = 0

        model1.Camera.Location = New Point3D(0, 0, 1)
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        ' Disable shadows because they are not yet supported with multitexturing
        model1.Rendered.ShadowMode = shadowType.None

        Dim multiTexture As Boolean = False
        'if (model1.Renderer == rendererType.OpenGL)
        '    multiTexture = model1.OpenglExtensions.Contains("ARB_multitexture");

        Dim floor As New Floor(multiTexture)

        model1.Entities.Add(floor)

        model1.Viewports(0).Navigation.Min = New Point3D(floor.BoxMin.X, floor.BoxMin.Y, 0.1)
        model1.Viewports(0).Navigation.Max = New Point3D(floor.BoxMax.X, floor.BoxMax.Y, 4)
        model1.Viewports(0).Navigation.Acceleration = 0.01
        model1.Viewports(0).Navigation.Speed = 0.05
        model1.Viewports(0).Navigation.RotationSpeed = 2
        model1.Viewports(0).Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Walk

        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class

