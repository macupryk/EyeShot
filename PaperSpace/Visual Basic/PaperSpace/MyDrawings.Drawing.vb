Imports devDept.Geometry

Partial Public Class MyDrawings

    ''' <summary>
    ''' Draws a plus sign (+) at current mouse location
    ''' </summary>
    Private Sub DrawPositionMark(ByVal current As Point3D, Optional ByVal crossSize As Double = 20)
        Dim currentScreen As Point3D = WorldToScreen(current)

        ' computes the horizontal line direction on screen
        Dim left As Point2D = WorldToScreen((current.X - 1), current.Y, 0)
        Dim dirHorizontal As Vector2D = Vector2D.Subtract(left, currentScreen)
        dirHorizontal.Normalize

        ' computes the horizontal line endpoints position on screen
        left = currentScreen + dirHorizontal * crossSize
        Dim right As Point2D = currentScreen - dirHorizontal * crossSize

        renderContext.DrawLine(left, right)

        ' computes the vertical line direction on screen
        Dim bottom As Point2D = WorldToScreen(current.X, (current.Y - 1), 0)
        Dim dirVertical As Vector2D = Vector2D.Subtract(bottom, currentScreen)
        dirVertical.Normalize

        ' computes  the vertical line endpoints position on screen
        bottom = currentScreen + dirVertical * crossSize
        Dim top As Point2D = currentScreen - dirVertical * crossSize

        renderContext.DrawLine(bottom, top)

        renderContext.SetLineSize(1)
    End Sub

    ''' <summary>
    ''' Draws the pick box at the current mouse location
    ''' </summary>
    Public Sub DrawSelectionMark(ByVal current As System.Drawing.Point)
        ' takes the size of the pick box
        Dim size As Double = PickBoxSize

        Dim x1 As Double = current.X - (size / 2)
        Dim y1 As Double = Height - current.Y - (size / 2)
        Dim x2 As Double = current.X + (size / 2)
        Dim y2 As Double = Height - current.Y + (size / 2)

        Dim bottomLeftVertex As Point3D = New Point3D(x1, y1)
        Dim bottomRightVertex As Point3D = New Point3D(x2, y1)
        Dim topRightVertex As Point3D = New Point3D(x2, y2)
        Dim topLeftVertex As Point3D = New Point3D(x1, y2)

        ' draws the box
        renderContext.DrawLineLoop(New Point3D() {bottomLeftVertex, bottomRightVertex, topRightVertex, topLeftVertex})

        renderContext.SetLineSize(1)
    End Sub

End Class
