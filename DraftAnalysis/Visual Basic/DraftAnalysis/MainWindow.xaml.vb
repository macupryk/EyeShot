
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Private _readFile As devDept.Eyeshot.Translators.ReadFile
    Private Sh As SplitHelper
    Private _offset As Double
    Private _originalIndex As Integer
    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        _readFile = New devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/Piston.eye")
        _readFile.DoWork()
        _readFile.AddToScene(model1, System.Drawing.Color.White)

        ' Inizializes original entity index
        _originalIndex = 0

        Sh = New SplitHelper(model1)
        Sh.DrawNormalDirection(Point3D.Origin, _readFile.Entities(_originalIndex).BoxSize.Diagonal)        

        ' sets trimetric view
        model1.SetView(viewType.Trimetric)
        model1.Camera.ProjectionMode = projectionType.Orthographic
        model1.GetGrid().Visible = False

        ' fits the model in the viewport
        model1.ZoomFit()

        'refresh the model control
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub hideOriginalCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        If model1 IsNot Nothing Then
            If CBool(Me.hideOriginalCheckBox.IsChecked) Then
                model1.Entities(_originalIndex).Visible = False
            Else
                model1.Entities(_originalIndex).Visible = True
            End If

            model1.Invalidate()
        End If
    End Sub

    Private Sub pullDirectionButton_Click(sender As Object, e As EventArgs)
        If model1.Entities.Count > 0 Then
            Enable_Buttons(False)

            Sh.QuickSplit(DirectCast(_readFile.Entities(_originalIndex), Mesh), Sh.direction)

            Enable_Buttons(True)
        End If
    End Sub

    Private Sub model1_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim selEntityIndex As Integer = model1.GetEntityUnderMouseCursor(devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))

            If selEntityIndex <> -1 Then
                Dim entity As Entity = model1.Entities(selEntityIndex)
                Dim pt As Point3D
                Dim tri As Integer
                Try
                    If model1.FindClosestTriangle(DirectCast(entity, IFace), devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), pt, tri) > 0 Then
                        Dim it As IndexTriangle = DirectCast(entity, Mesh).Triangles(tri)

                        ' calculates normal direction of selected triangle
                        Dim pointEnt As Point3D() = DirectCast(entity, Mesh).Vertices
                        Dim selT As New Triangle(pointEnt(it.V1), pointEnt(it.V2), pointEnt(it.V3))
                        selT.Regen(0.1)
                        Sh.direction = selT.Normal

                        ' shows the normal's direction like an arrow
                        Sh.DrawNormalDirection(pt, _readFile.Entities(_originalIndex).BoxSize.Diagonal)

                        model1.Entities.Regen()
                        model1.Invalidate()
                    End If
                Catch
                End Try
            End If
        End If
    End Sub

    Private Sub Enable_Buttons(enabled As Boolean)
        translationSlider.IsEnabled = enabled
        hideOriginalCheckBox.IsEnabled = enabled
        pullDirectionButton.IsEnabled = enabled
        translationSlider.Value = translationSlider.Minimum
        _offset = 0
    End Sub

    Private Sub translationSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        ' Calculates translation offset from entity size
        Dim size As Double = _readFile.Entities(_originalIndex).BoxSize.Diagonal
        Dim tOffset As Double = (translationSlider.Value - _offset) * size / 50
        _offset = translationSlider.Value

        Sh.TranslatingSections(tOffset)
    End Sub

End Class