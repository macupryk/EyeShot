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
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Labels
Imports devDept.Geometry
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports Color = System.Drawing.Color

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
            ' model2.Unlock("")
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' hides grids
            model1.GetGrid().Visible = False
            model2.GetGrid().Visible = False

            ' adds entities            
            model1.Entities.Add(New Line(60, 10, 0, 60, 110, 0), System.Drawing.Color.Blue)
            model1.Entities.Add(New Line(10, 60, 0, 110, 60, 0), System.Drawing.Color.Blue)
            model1.Entities.Add(New Circle(60, 60, 0, 40), System.Drawing.Color.Red)
            model1.Entities.Add(New Circle(100, 60, 0, 6), System.Drawing.Color.Red)
            model1.Entities.Add(New Circle(60, 100, 0, 6), System.Drawing.Color.Red)
            model1.Entities.Add(New Circle(60, 20, 0, 6), System.Drawing.Color.Red)
            model1.Entities.Add(New Circle(20, 60, 0, 6), System.Drawing.Color.Red)

            ' adds labels
            Dim label As devDept.Eyeshot.Labels.TextOnly = New TextOnly(30, 95, 0, "CIRCLE", New System.Drawing.Font("Tahoma", 8.25F), Color.Black)
            label.ColorForSelection = model1.SelectionColor
            model1.Labels.Add(label)
            Dim label2 As devDept.Eyeshot.Labels.TextOnly = New TextOnly(40, 60, 0, "LINE", New System.Drawing.Font("Tahoma", 8.25F), Color.Black)
            label2.ColorForSelection = model1.SelectionColor
            model1.Labels.Add(label2)

            ' sets trimetric view            
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport            
            model1.ZoomFit()

            ' refresh the viewports
            model1.Invalidate()
            model2.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub copyButton_Click(sender As Object, e As RoutedEventArgs)
            If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
                model1.Entities.CopySelection()
            End If
            If selectLabelsButton.IsChecked.HasValue AndAlso selectLabelsButton.IsChecked.Value Then
                model1.Labels.CopySelection()
            End If
        End Sub

        Private Sub pasteButton_Click(sender As Object, e As RoutedEventArgs)
            If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
                model2.BoundingBox.OverrideSceneExtents = False
                model2.Entities.Paste()
            End If
            If selectLabelsButton.IsChecked.HasValue AndAlso selectLabelsButton.IsChecked.Value Then
                model2.Labels.Paste()

                ' manually extends BoundingBox to show labels when there are no entities
                If model2.Entities.Count = 0 Then
                    model2.BoundingBox.OverrideSceneExtents = True
                    model2.BoundingBox.Min = New Point3D(10, 10, 0)
                    model2.BoundingBox.Max = New Point3D(110, 110, 0)
                    model2.Entities.UpdateBoundingBox()
                End If
            End If
            model2.Invalidate()
        End Sub

        Private Sub syncButton_Click(sender As Object, e As RoutedEventArgs)
            ' saves the camera from the first model
            Dim savedCamera As Camera
            model1.SaveView(savedCamera)

            ' restores the camera to the second model
            model2.RestoreView(savedCamera)
            model2.Invalidate()
        End Sub

        Private Sub selectButton_Click(sender As Object, e As RoutedEventArgs)
            If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
                If selectLabelsButton.IsChecked.HasValue AndAlso selectLabelsButton.IsChecked.Value Then
                    selectLabelsButton.IsChecked = False
                End If

                model1.ActionMode = actionType.SelectByPick
            Else
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub splitContainer1_MouseDown(sender As Object, e As MouseButtonEventArgs)
            model1.SplitterMoving = True
            model2.SplitterMoving = True
        End Sub

        Private Sub splitContainer1_MouseUp(sender As Object, e As MouseButtonEventArgs)
            model1.SplitterMoving = False
            model2.SplitterMoving = False
        End Sub

        Private Sub selectLabelsButton_Click(sender As Object, e As RoutedEventArgs) Handles selectLabelsButton.Click
            If selectLabelsButton.IsChecked.HasValue AndAlso selectLabelsButton.IsChecked.Value Then
                If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
                    selectButton.IsChecked = False
                End If

                model1.ActionMode = actionType.SelectVisibleByPickLabel
            Else
                model1.ActionMode = actionType.None
            End If
        End Sub

    End Class
End Namespace