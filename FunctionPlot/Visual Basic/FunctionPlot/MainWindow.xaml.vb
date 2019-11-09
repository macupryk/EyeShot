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

Namespace FunctionPlot
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()
            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.                        

        End Sub


        Protected Overrides Sub OnContentRendered(e As EventArgs)            
            model1.GetGrid().AutoSize = True
            model1.GetGrid().[Step] = 1

            Const rows As Integer = 50
            Const cols As Integer = 50
            Const scale As Double = 4

            Dim vertices As New List(Of PointRGB)(rows * cols)

            Dim surface As New Mesh()

            surface.NormalAveragingMode = Mesh.normalAveragingType.Averaged

            For j As Integer = 0 To rows - 1

                For i As Integer = 0 To cols - 1

                    Dim x As Double = i / 5.0
                    Dim y As Double = j / 5.0

                    Dim f As Double = 0

                    Dim den As Double = Math.Sqrt(x * x + y * y)

                    If den <> 0 Then

                        f = scale * Math.Sin(Math.Sqrt(x * x + y * y)) / den
                    End If

                    ' generates a random color
                    Dim red As Integer = CInt(255 - y * 20)
                    Dim green As Integer = CInt(255 - x * 20)
                    Dim blue As Integer = CInt(-f * 50)

                    ' clamps color values lat 0-255
                    Utility.LimitRange(Of Integer)(0, red, 255)
                    Utility.LimitRange(Of Integer)(0, green, 255)
                    Utility.LimitRange(Of Integer)(0, blue, 255)


                    vertices.Add(New PointRGB(x, y, f, CByte(red), CByte(green), CByte(blue)))
                Next
            Next

            Dim triangles As New List(Of SmoothTriangle)((rows - 1) * (cols - 1) * 2)

            For j As Integer = 0 To (rows - 1) - 1

                For i As Integer = 0 To (cols - 1) - 1

                    triangles.Add(New SmoothTriangle(i + j * cols, i + j * cols + 1, i + (j + 1) * cols + 1))
                    triangles.Add(New SmoothTriangle(i + j * cols, i + (j + 1) * cols + 1, i + (j + 1) * cols))
                Next
            Next

            surface.Vertices = vertices.ToArray()
            surface.Triangles = triangles.ToArray()

            model1.Entities.Add(surface)

            ' fits the model in the model1
            model1.ZoomFit()
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace

