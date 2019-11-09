Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.Windows.Input
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports System.Collections
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports System.Diagnostics

    Public Class MyModel
        Inherits devDept.Eyeshot.Model
        Private p1 As Point3D, p2 As Point3D, p3 As Point3D
        Private plane As Plane = Plane.XY

        Private current As Point3D

        Private firstClick As Boolean = False


        Public points As New List(Of Point3D)()


        ' Set internal p1, p2, p3 and plane members
        Public Sub SetPlane(p1 As Point3D, p2 As Point3D, p3 As Point3D)

            Me.p1 = p1
            Me.p2 = p2
            Me.p3 = p3

            plane = New Plane(p1, p2, p3)

        End Sub

        ' Every click adds a line
        Protected Overrides Sub OnMouseUp(e As MouseButtonEventArgs)
            If GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) Then
                MyBase.OnMouseUp(e)

                Return
            End If

            If ActionMode = actionType.None AndAlso e.ChangedButton = System.Windows.Input.MouseButton.Left Then

                If firstClick = False Then
                    points.Clear()
                    firstClick = True
                End If

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, current)

                points.Add(current)
            ElseIf e.ChangedButton = System.Windows.Input.MouseButton.Right Then

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, current)
                points.Add(current)

                lp = New LinearPath(points)

                lp.LineWeightMethod = colorMethodType.byEntity
                lp.LineWeight = 2

            Entities.Add(lp, System.Drawing.Color.ForestGreen)
                points.Clear()

                current = Nothing

                Invalidate()
            End If

            MyBase.OnMouseUp(e)
        End Sub

        Public lp As LinearPath
        Private mouseLocation As System.Drawing.Point

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            ' save the current mouse position
            mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e))

            ' if start is valid and actionMode is None and it's not in the toolbar area

            If current Is Nothing OrElse ActionMode <> actionType.None OrElse GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) Then

                MyBase.OnMouseMove(e)

                Return
            End If

            ' paint the viewport surface
            PaintBackBuffer()

            ' consolidates the drawing
            SwapBuffers()

            MyBase.OnMouseMove(e)

        End Sub

        Protected Overrides Sub DrawOverlay(myParams As DrawSceneParams)
            ScreenToPlane(mouseLocation, plane, current)

            ' draw the elastic line
            renderContext.SetLineSize(1)

            ' draw inverted
            renderContext.EnableXOR(True)

            renderContext.SetState(depthStencilStateType.DepthTestOff)

            ' entity drawing in 2D
            lp = New LinearPath(points)

            Dim pts As New List(Of Point3D)()

            ' draw the elastic line
            For i As Integer = 0 To lp.Vertices.Length - 1

                pts.Add(WorldToScreen(lp.Vertices(i)))
            Next

            ' Avoid clipping by camera planes
            For Each pt As Point3D In pts

	            pt.Z = 0
            Next

            If pts.Count > 0 Then
                renderContext.DrawLineStrip(pts.ToArray())
            End If

            If ActionMode = actionType.None AndAlso Not GetToolBar().Contains(mouseLocation) AndAlso lp.Vertices.Length > 0 Then
                Dim pts2 As New List(Of Point3D)()

                ' Draw elastic line
                pts2.Add(WorldToScreen(lp.Vertices(lp.Vertices.Length - 1)))
                pts2.Add(WorldToScreen(current))

                ' cross drawing in 3D
                Dim left As Point3D = WorldToScreen(current.X - (p2.X - p1.X) / 10, current.Y, current.Z)
                Dim right As Point3D = WorldToScreen(current.X + (p2.X - p1.X) / 10, current.Y, current.Z)

                pts2.Add(left)
                pts2.Add(right)

                Dim bottom As Point3D = WorldToScreen(current.X, current.Y - (p3.Y - p1.Y) / 10, current.Z - (p3.Z - p1.Z) / 10)
                Dim top As Point3D = WorldToScreen(current.X, current.Y + (p3.Y - p1.Y) / 10, current.Z + (p3.Z - p1.Z) / 10)

                pts2.Add(bottom)
                pts2.Add(top)

                ' Avoid clipping by camera planes
                For Each pt As Point3D In pts2
    
                    pt.Z = 0
                Next
    
                renderContext.DrawLines(pts2.ToArray())

                ' disables draw inverted
                renderContext.EnableXOR(False)

                ' text drawing
                DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Current point: " + current.X.ToString("f2") + ", " + current.Y.ToString("f2") + ", " + current.Z.ToString("f2"), New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
            Else
                ' disables draw inverted
                renderContext.EnableXOR(False)
            End If

            MyBase.DrawOverlay(myParams)
        End Sub

    End Class

