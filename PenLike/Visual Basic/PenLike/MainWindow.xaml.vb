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
Imports System.Xaml
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        ' Adds a rectangle
        Dim q As New Quad(0, 0, 0, 20, 0, 0, 20, 30, 0, 0, 30, 0)

        model1.Entities.Add(q, System.Drawing.Color.DarkSlateBlue)

        ' Sets trimetric view
        model1.SetView(viewType.Trimetric)

        ' Fits the model in the viewport            
        model1.ZoomFit()

        ' refresh the viewport
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class

