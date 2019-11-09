Imports devDept.Geometry
Imports devDept.Graphics

Partial Public Class MyDrawings

    ' current snapped point which is one of the vertex of the view
    Dim _snappedPoint As Point3D
    Dim SnapQuadSize As Integer = 12
    
    ''' <summary>
    ''' Draws the quad that defines the snapped point
    ''' </summary>
    Private Sub DrawQuad(ByVal onScreen As System.Drawing.Point)
        Dim x1 As Double = onScreen.X - (SnapQuadSize / 2)
        Dim y1 As Double = onScreen.Y - (SnapQuadSize / 2)
        Dim x2 As Double = onScreen.X + (SnapQuadSize / 2)
        Dim y2 As Double = onScreen.Y + (SnapQuadSize / 2)

        Dim bottomLeftVertex As Point3D = New Point3D(x1, y1)
        Dim bottomRightVertex As Point3D = New Point3D(x2, y1)
        Dim topRightVertex As Point3D = New Point3D(x2, y2)
        Dim topLeftVertex As Point3D = New Point3D(x1, y2)

        renderContext.DrawLineLoop(New Point3D() {bottomLeftVertex, bottomRightVertex, topRightVertex, topLeftVertex})
    End Sub
    
    ''' <summary>
    ''' Displays the snapped point
    ''' </summary>
    Private Sub DisplaySnappedVertex()
        renderContext.SetLineSize(2)

        ' blue color
        renderContext.SetColorWireframe(System.Drawing.Color.FromArgb(0, 0, 255))
        renderContext.SetState(depthStencilStateType.DepthTestOff)

        Dim onScreen As Point2D = WorldToScreen(_snappedPoint)

        DrawQuad(New System.Drawing.Point(CType(onScreen.X,Integer), CType(onScreen.Y,Integer)))
        renderContext.SetLineSize(1)
    End Sub

End Class
