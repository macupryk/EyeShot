Imports System.Collections.Generic
Imports System.Text

Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports System.Drawing
Imports System.Collections

Public Class MyModel
    Inherits devDept.Eyeshot.Model

    Private p1 As Point3D = Point3D.Origin
    Private p2 As Point3D = Point3D.Origin
    Private p3 As Point3D = Point3D.Origin
    Private plane As Plane = plane.XY

    Private m_wallHeight As Double
    Private m_wallColor As Color = Color.Firebrick

    Private start As Point3D, [end] As Point3D, current As Point3D

    Private firstClick As Boolean = False

#Region "Properties"

    Public Property WallHeight() As Double

        Get
            Return m_wallHeight
        End Get
        Set(value As Double)
            m_wallHeight = value
        End Set
    End Property


    Public Property WallColor() As Color

        Get
            Return m_wallColor
        End Get
        Set(value As Color)
            m_wallColor = value
        End Set
    End Property


#End Region

    ' Set internal p1, p2, p3 and plane members
    Public Sub SetPlane(p1 As Point3D, p2 As Point3D, p3 As Point3D)

        Me.p1 = p1
        Me.p2 = p2
        Me.p3 = p3

        plane = New Plane(p1, p2, p3)

    End Sub

    ' Every click adds a Quad
    Protected Overrides Sub OnMouseUp(e As System.Windows.Input.MouseButtonEventArgs)

        If ActionMode = actionType.None AndAlso Not GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) AndAlso e.ChangedButton = System.Windows.Input.MouseButton.Left Then

            If firstClick = False Then

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, start)

                SnapToGrid(start)


                firstClick = True
            Else

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, [end])

                SnapToGrid([end])

                Dim l As New Line(start, [end])

                Entities.Add(New Quad(l.StartPoint, l.EndPoint, New Point3D(l.EndPoint.X, l.EndPoint.Y, l.EndPoint.Z + m_wallHeight), New Point3D(l.StartPoint.X, l.StartPoint.Y, l.StartPoint.Z + m_wallHeight)), m_wallColor)

                start = [end]



                Invalidate()

            End If
        End If

        MyBase.OnMouseUp(e)

    End Sub

    Private mouseLocation As System.Drawing.Point

    Protected Overrides Sub OnMouseMove(e As System.Windows.Input.MouseEventArgs)
        ' save the current mouse position
        mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e))

        ' if start is valid and actionMode is None
        If ActionMode <> actionType.None OrElse GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) Then

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
        If ActionMode <> actionType.None OrElse GetToolBar().Contains(mouseLocation) Then
            MyBase.DrawOverlay(myParams)
            Return
        End If

        ScreenToPlane(mouseLocation, plane, current)

        SnapToGrid(current)

        ' draw inverted
        renderContext.EnableXOR(True)

        renderContext.SetState(depthStencilStateType.DepthTestOff)

        If firstClick Then
            renderContext.SetLineSize(1)
            Dim screenStart As Point3D = WorldToScreen(start)
            Dim screenCurrent As Point3D = WorldToScreen(current)
            screenStart.Z = 0
            screenCurrent.Z = 0

            Dim pts As New List(Of Point3D)()
            ' elastic line
            pts.Add(screenStart)
            pts.Add(screenCurrent)

            renderContext.DrawLines(pts.ToArray())
        End If

        ' cross drawing in 3D
        renderContext.SetLineSize(3)

        Dim pts2 As New List(Of Point3D)()

        Dim left As Point3D = WorldToScreen(current.X - (p2.X - p1.X) / 20, current.Y, current.Z)
        Dim right As Point3D = WorldToScreen(current.X + (p2.X - p1.X) / 20, current.Y, current.Z)

        pts2.Add(left)
        pts2.Add(right)

        Dim bottom As Point3D = WorldToScreen(current.X, current.Y - (p3.Y - p1.Y) / 20, current.Z - (p3.Z - p1.Z) / 20)
        Dim top As Point3D = WorldToScreen(current.X, current.Y + (p3.Y - p1.Y) / 20, current.Z + (p3.Z - p1.Z) / 20)

        pts2.Add(bottom)
        pts2.Add(top)

        ' Sets the Z to 0 to avoid clipping planes issues
        For i As Integer = 0 To pts2.Count - 1
            pts2(i).Z = 0
        Next

        renderContext.DrawLines(pts2.ToArray())

        ' disables draw inverted
        renderContext.EnableXOR(False)

        renderContext.EnableXORForTexture(True, myParams.ShaderParams)

        ' text drawing
        DrawText(mouseLocation.X, Size.Height - mouseLocation.Y + 10, "Current point: " + current.X.ToString("f2") + ", " + current.Y.ToString("f2") + ", " + current.Z.ToString("f2"), New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)

        renderContext.EnableXORForTexture(False, myParams.ShaderParams)

        MyBase.DrawOverlay(myParams)
    End Sub

    Private Sub SnapToGrid(ByRef p As Point3D)

        p.X = Math.Round(p.X / 10) * 10
        p.Y = Math.Round(p.Y / 10) * 10

    End Sub

End Class


