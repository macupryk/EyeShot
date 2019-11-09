Imports System
Imports System.Drawing

Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Eyeshot.Labels
Imports System.Collections.Generic


Partial Class Draw
    Public Shared Color As Color = Color.Black

    Public Shared Sub Jet(model1 As Model)
        '#Region "Jet drawing"

        Dim fuselage As String = "Fuselage"
        model1.Layers.Add(fuselage, Color.DarkGray)
        model1.Entities.Add(New Triangle(+15, -30, 8, 0, -30, 23,
            0, -60, 8), fuselage, Color.DeepSkyBlue)
        model1.Entities.Add(New Triangle(0, -60, 8, 0, -30, 23,
            -15, -30, 8), fuselage, Color.DeepSkyBlue)
        model1.Entities.Add(New Triangle(-15, -30, 8, 0, -30, 23,
            0, +56, 8), fuselage)
        model1.Entities.Add(New Triangle(0, +56, 8, 0, -30, 23,
            15, -30, 8), fuselage)
        model1.Entities.Add(New Quad(0, +56, 8, +15, -30, 8,
            0, -60, 8, -15, -30, 8), fuselage)

        Dim wings As String = "Wings"
        model1.Layers.Add("Wings", Color.CornflowerBlue)
        model1.Entities.Add(New Triangle(0, -27, 10, -60, +8, 10,
            60, +8, 10), wings)
        model1.Entities.Add(New Triangle(60, +8, 10, 0, +8, 15,
            0, -27, 10), wings)
        model1.Entities.Add(New Triangle(60, +8, 10, -60, +8, 10,
            0, +8, 15), wings)
        model1.Entities.Add(New Triangle(0, -27, 10, 0, +8, 15,
            -60, +8, 10), wings)

        Dim tail As String = "Tail"
        model1.Layers.Add(tail, Color.Chartreuse)
        model1.Entities.Add(New Triangle(-30, +57, 7.5, 30, +57, 7.5,
            0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(0, +40, 7.5, 30, +57, 7.5,
            0, +57, 12), tail)
        model1.Entities.Add(New Triangle(0, +57, 12, -30, +57, 7.5,
            0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(30, +57, 7.5, -30, +57, 7.5,
            0, +57, 12), tail)
        model1.Entities.Add(New Triangle(0, +40, 7.5, 3, +57, 8.5,
            0, +65, 33), tail)
        model1.Entities.Add(New Triangle(0, +65, 33, -3, +57, 8.5,
            0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(3, +57, 8.5, -3, +57, 8.5,
            0, +65, 33), tail)

        Dim wires As String= "Wires"
        model1.Layers.Add("Wires")

        Dim axis As New Line(-22, 0, 3, 22, 0, 3)

        axis.LineTypeMethod = colorMethodType.byEntity
        model1.LineTypes.Add("JetAxisPattern", New Single() {5, -1.5F, 0.25F, -1.5F})
        axis.LineTypeName = "JetAxisPattern"

        model1.Entities.Add(axis, wires)

        ' Points
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +12, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +16, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +21, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +27, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +34, 10, 4), wires)

        ' Wheels
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(+20, 0, 3), 3), wires)
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(-20, 0, 3), 3), wires)
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(0, -42, 2), 2), wires)

        ' Wheel crosses
        model1.Entities.Add(New Line(-20, 0, 2, -20, 0, 4), wires)
        model1.Entities.Add(New Line(-20, -1, 3, -20, +1, 3), wires)
        model1.Entities.Add(New Line(+20, 0, 2, +20, 0, 4), wires)
        model1.Entities.Add(New Line(+20, -1, 3, +20, +1, 3), wires)
        model1.Entities.Add(New Line(0, -41, 2, 0, -43, 2), wires)
        model1.Entities.Add(New Line(0, -42, 1, 0, -42, 3), wires)

        '#End Region

        ' Labels        
        model1.Labels.Add(New LeaderAndText(+60, +8, 10, "Left wing", New Font("Tahoma", 8.25F), Draw.Color, New Vector2D(0, 30)))
        model1.Labels.Add(New ImageOnly(0, +65, 33, WpfApplication1.My.Resources.CautionLabel))

        ' Dimensions
        Dim dimPlane1 As Plane = Plane.XY
        dimPlane1.Rotate(Math.PI / 2, Vector3D.AxisZ, Point3D.Origin)
        Dim dimPlane2 As Plane = Plane.YZ
        dimPlane2.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin)

        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, -60, 8), New Point3D(60, 8, 8), New Point3D(70, -26, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, -60, 8), New Point3D(0, 65, 8), New Point3D(80, 0, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, 57, 8), New Point3D(0, 65, 8), New Point3D(70, 80, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane2, New Point3D(0, 57, 8), New Point3D(0, 65, 33), New Point3D(0, 75, 20.5), 4), wires)

        model1.ZoomFit()
    End Sub

End Class
