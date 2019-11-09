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
Imports devDept.Eyeshot.Triangulation
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()
            'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        End Sub

        Private mc As MarchingCubes

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' adjusts viewport grid size and step
            model1.GetGrid().Min = New Point3D(-5, -5)
            model1.GetGrid().Max = New Point3D(+5, +5)
            model1.GetGrid().[Step] = 0.5           

            ' declare the function to be used to evaluate the 3D scalar field
            Dim func As New ScalarField3D(AddressOf myScalarField)

            ' initialize marching cube algorithm
            mc = New MarchingCubes(New Point3D(0, -2.5, 0), 50, 0.1, 25, 0.1, 25, _
                0.1, func)

            mc.IsoLevel = trackBar1.Value

            mc.DoWork()

            ' iso surface generation
            Dim isoSurf As Mesh = mc.Result

            ' adds the surface to the entities collection with Magenta color            
            model1.Entities.Add(isoSurf, System.Drawing.Color.Magenta)

            ' updates the iso level label
            isoLevelLabel.Content = "Iso level = " + mc.IsoLevel.ToString()

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            model1.ZoomFit()

            ' refresh the viewport
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Private Function myScalarField(x1 As Single, y1 As Single, z1 As Single) As Single

            Dim den1 As Single = x1 * x1 + y1 * y1 + z1 * z1
            Dim den2 As Single = (x1 - 3) * (x1 - 3) + y1 * y1 + z1 * z1

            Dim part1 As Single = If(den1 <> 0, 100 / den1, 0)
            Dim part2 As Single = If(den2 <> 0, 100 / den2, 0)

            Return part1 + part2

        End Function

        Private Sub trackBar1_OnValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
            If mc Is Nothing Then
                Return
            End If

            model1.Entities.Clear()
            While model1.Entities.Count > 0
                model1.Entities.RemoveAt(0)
            End While

            mc.IsoLevel = trackBar1.Value

            mc.DoWork()

            model1.Entities.Add(mc.Result, System.Drawing.Color.Magenta)
            isoLevelLabel.Content = "Iso level = " + CInt(mc.IsoLevel).ToString()

            model1.Invalidate()
        End Sub
    End Class
End Namespace


