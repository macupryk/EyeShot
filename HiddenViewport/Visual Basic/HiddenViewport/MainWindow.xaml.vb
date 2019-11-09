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
Imports devDept.Eyeshot.Translators

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()
        End Sub

        Private model1 As Model
        Protected Overrides Sub OnContentRendered(e As EventArgs)
            model1 = New Model()
            model1.InitializeViewports()
            model1.Viewports(0).Grids.Add(New devDept.Eyeshot.Grid())

            model1.Size = New System.Drawing.Size(300, 300)

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

            model1.CreateControl()

            ' hides grid            
            model1.GetGrid().Visible = False

            ' A triangle fan            
            model1.Entities.Add(New Triangle(-10, -10, 0, 10, -10, 0, 0, 0, 5), System.Drawing.Color.Red)
            model1.Entities.Add(New Triangle(+10, -10, 0, 10, +10, 0, 0, 0, 5), System.Drawing.Color.Green)
            model1.Entities.Add(New Triangle(+10, +10, 0, -10, +10, 0, 0, 0, 5), System.Drawing.Color.Cyan)
            model1.Entities.Add(New Triangle(-10, +10, 0, -10, -10, 0, 0, 0, 5), System.Drawing.Color.Blue)

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            model1.ZoomFit()

            ' refresh the viewport
            model1.Invalidate()

            ' Update the bounding box, needed by many internal operations
            model1.Entities.UpdateBoundingBox()

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub copyImageButton_OnClick(sender As Object, e As RoutedEventArgs)
            model1.CopyToClipboardRaster()
        End Sub

        Private Sub printPreviewButton_OnClick(sender As Object, e As RoutedEventArgs)
            model1.PrintPreview(New System.Drawing.Size(400, 500))
        End Sub

        Private Sub saveStlButton_OnClick(sender As Object, e As RoutedEventArgs)
            Dim stlFile As String = "test.stl"
            Dim wp As New WriteParams(model1)
            Dim ws As New WriteSTL(wp, stlFile)
            ws.DoWork()

            Dim fullPath As String = [String].Format("{0}\{1}", System.Environment.CurrentDirectory, stlFile)
            MessageBox.Show([String].Format("File saved in {0}", fullPath))
        End Sub
    End Class
End Namespace

