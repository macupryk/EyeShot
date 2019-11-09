
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

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' adjusts grid extents ad step
            model1.GetGrid().Max = New Point3D(200, 200)
            model1.GetGrid().[Step] = 25

            Dim nArrows As Integer = 13
            Dim arcRadius As Integer = 100

            Dim arcSpan As Double = 120

            ' adds the arc
            model1.Entities.Add(New Arc(Point3D.Origin, arcRadius, Utility.DegToRad(340), Utility.DegToRad(340 + 150)))

            For i As Integer = 0 To nArrows - 1

                ' angle in rad
                Dim radAngle As Double = Utility.DegToRad(arcSpan * i / nArrows)

                ' Creates a mesh with the arrow shape
                Dim m As Mesh = Mesh.CreateArrow(4, 100 - i * 4, 8, 24, 16, Mesh.natureType.Smooth)

                m.EdgeStyle = Mesh.edgeStyleType.Sharp

                ' Translation transformation
                Dim tra As New Translation(arcRadius * Math.Cos(radAngle), arcRadius * Math.Sin(radAngle), 0)

                ' Rotation transformation
                Dim rot As New devDept.Geometry.Rotation(radAngle, Vector3D.AxisZ)

                ' Combines the two
                Dim combined As Transformation = tra * rot

                ' applies the transformation to the arrow
                m.TransformBy(combined)

                ' adds the arrow to the master entity array                

                model1.Entities.Add(m, System.Drawing.Color.FromArgb(120 + i * 10, 255 - i * 10, 0))
            Next           

            ' sets trimetric view            
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport            
            model1.ZoomFit()

            'refresh the model control
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace