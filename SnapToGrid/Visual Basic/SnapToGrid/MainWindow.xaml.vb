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
    End Sub

    Private Sub Model1OnInitializeScene(sender As Object, eventArgs As EventArgs) Handles model1.InitializeScene
        model1.WallHeight = CDbl(heightNumericUpDown.Value)

        ' Adds a rect
        Dim p1 As New Point3D(0, 0, 0)
        Dim p2 As New Point3D(80, 0, 0)
        Dim p3 As New Point3D(0, 80, 0)

        ' Set plane points
        model1.SetPlane(p1, p2, p3)

        model1.SetView(viewType.Trimetric)

        model1.GetGrid().AutoSize = False

        colorPicker.SelectedColor = System.Windows.Media.Color.FromArgb(model1.WallColor.A, model1.WallColor.R, model1.WallColor.G, model1.WallColor.B)
    End Sub

    Private Sub heightNumericUpDown_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        If model1 IsNot Nothing Then
            model1.WallHeight = CDbl(heightNumericUpDown.Value)
        End If
    End Sub

    Private Sub colorPicker_SelectedColorChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Color?))
        Dim color = e.NewValue.Value
        model1.WallColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)
    End Sub
End Class
