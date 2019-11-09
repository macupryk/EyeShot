Imports devDept.Eyeshot
Imports devDept.Graphics
Imports System.Drawing
Imports devDept.Geometry

Public Class MyModel
    Inherits Model

    Private stroke As New List(Of Point)()

    Private leftButtonDown As Boolean = False

    Public Sub New()
        MyBase.New()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseButtonEventArgs)
        MyBase.OnMouseDown(e)

        If GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) OrElse ActionMode <> actionType.None Then
            Return
        End If

        stroke.Clear()
        Invalidate()

        If e.LeftButton = MouseButtonState.Pressed Then
            leftButtonDown = True
        End If
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        If leftButtonDown Then
            stroke.Add(RenderContextUtility.ConvertPoint(GetMousePosition(e)))

            ' Repaints the scene and draws the strokes in the DrawOverlay
            PaintBackBuffer()
            SwapBuffers()
        End If

    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseButtonEventArgs)
        MyBase.OnMouseUp(e)

        If e.LeftButton = MouseButtonState.Released Then
            leftButtonDown = False
        End If

    End Sub

    Protected Overrides Sub DrawOverlay(myParams As DrawSceneParams)
        MyBase.DrawOverlay(myParams)

        DrawLines()

    End Sub

    Private Sub DrawLines()
        ' Sets the shader for the thick lines
        renderContext.SetShader(shaderType.NoLightsThickLines)

        ' Sets the line size
        renderContext.SetLineSize(4)

        ' Sets the pen color
        renderContext.SetColorWireframe(Color.Red)

        For i As Integer = 0 To stroke.Count - 2

            DrawLine(i)
        Next

        renderContext.SetLineSize(1)
    End Sub

    Private Sub DrawLine(i As Integer)
        Dim current As Point = stroke(i)
        Dim [next] As Point = stroke(i + 1)
        renderContext.DrawLine(New Point2D(current.X, Size.Height - current.Y), New Point2D([next].X, Size.Height - [next].Y))
    End Sub
End Class