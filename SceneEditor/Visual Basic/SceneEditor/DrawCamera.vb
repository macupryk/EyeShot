Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Collections.Generic
Imports Point = System.Drawing.Point

Class DrawCamera
    Private pNear As Point3D() = New Point3D(3) {}
    Private pFar As Point3D() = New Point3D(3) {}
    Private Camera As Camera
    Private LayerName As String

    Public Sub New(viewport As Viewport, controlHeight As Double, layerName__1 As String)
        Camera = viewport.Camera
        LayerName = layerName__1

        Dim viewFrame As Integer() = viewport.GetViewFrame()

        ' gets Near Plane vertices
        Dim pts As Point3D() = Camera.ScreenToPlane(New List(Of Point)() From { _
            New Point(0, 0), _
            New Point(0, viewport.Size.Height), _
            New Point(viewport.Size.Width, viewport.Size.Height), _
            New Point(viewport.Size.Width, 0) _
        }, Camera.NearPlane.Equation, CInt(controlHeight), viewFrame)

        pNear(0) = pts(0)
        pNear(1) = pts(1)
        pNear(2) = pts(2)
        pNear(3) = pts(3)

        ' gets Far Plane vertices
        pts = Camera.ScreenToPlane(New List(Of Point)() From { _
            New Point(0, 0), _
            New Point(0, viewport.Size.Height), _
            New Point(viewport.Size.Width, viewport.Size.Height), _
            New Point(viewport.Size.Width, 0) _
        }, Camera.FarPlane.Equation, CInt(controlHeight), viewFrame)

        pFar(0) = pts(0)
        pFar(1) = pts(1)
        pFar(2) = pts(2)
        pFar(3) = pts(3)
    End Sub

    Public Sub Draw(model As Model)
        Dim origin As Point3D
        Dim camX As Vector3D, camY As Vector3D, camZ As Vector3D

        Camera.GetFrame(origin, camX, camY, camZ)
        If origin IsNot Nothing Then
            ' Draws the View Volume
            Dim pts As Point3D() = New Point3D(23) {}
            Dim count As Integer = 0

            For i As Integer = 0 To 3
                pts(count) = pNear(i)
                count += 1
                pts(count) = pNear((i + 1) Mod 4)
                count += 1
                pts(count) = pFar((i + 1) Mod 4)
                count += 1
                pts(count) = pFar(i)
                count += 1
                pts(count) = origin
                count += 1
                pts(count) = pNear((i + 1) Mod 4)
                count += 1
            Next

            Dim lp1 As New LinearPath(pts)
            lp1.Color = Color.Gray
            lp1.ColorMethod = colorMethodType.byEntity
            model.Entities.Add(lp1, LayerName)

            'Draws the Camera
            Const widthB As Double = 3, heightB As Double = 5, depthB As Double = 3, heightC As Double = widthB / 2, radiusC As Double = 1.5

            Dim cone As Mesh = Mesh.CreateCone(radiusC, radiusC / 2, heightC, 10)
            cone.ColorMethod = colorMethodType.byEntity
            cone.Color = Color.GreenYellow

            Dim box As Mesh = Mesh.CreateBox(widthB, depthB, heightB)
            box.ColorMethod = colorMethodType.byEntity
            box.Color = Color.GreenYellow

            ' centers the box to the world origin
            box.Translate(-widthB / 2, -depthB / 2, +heightC)

            ' Aligns the Camera to the Camera view
            Dim t As Transformation = New Align3D(Plane.XY, New Plane(origin, camX, camY))
            box.TransformBy(t)
            cone.TransformBy(t)

            model.Entities.Add(cone, LayerName)
            model.Entities.Add(box, LayerName)
        End If
    End Sub

    Public Sub DeletePrevious(model As Model)
        model.Layers.Remove(LayerName)
        model.Layers.Add(LayerName)
    End Sub
End Class
