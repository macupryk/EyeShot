Imports System.ComponentModel
Imports System.Text
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Collections
Imports System.IO

Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Eyeshot.Labels
Imports devDept.Graphics


#If SOLID Then


Partial Class Draw

    Public Shared Sub Medal(model As Model)

        ' materials
        Dim alu As Material = Material.Aluminium

        alu.Diffuse = Color.White
        alu.Environment = 0.4F

        Dim medalMatName As String = "Alu"

        model.Materials.Add(alu)

        alu.Name = medalMatName


        Dim woodMatName As String = "Wood"

        Dim wood As New Material(woodMatName, New Bitmap(MainWindow.GetAssetsPath() + "Textures/Wenge.jpg"))
        
        model.Materials.Add(wood)



        ' medal 
        Dim sphere As Solid = Solid.CreateSphere(200, 120, 60)

        sphere.Rotate(Math.PI / 2, Vector3D.AxisY)

        sphere.Translate(0, 0, -190)

        Dim cylinder As Solid = Entities.Region.CreateCircle(Plane.XY, 0, 0, 50).ExtrudeAsSolid(100, 0.1)

        Dim intersection As Solid() = Solid.Intersection(sphere, cylinder)

        Dim lens As Solid = intersection(0)

        Dim eyeshotText As New Text(-45.5, -8, 0, "eyeshot", 19)

        eyeshotText.Translate(0, 0, 2)

        Dim solidItems As New List(Of Solid)()

        solidItems.Add(lens)

        solidItems.AddRange(model.ExtrudeText(eyeshotText, 0.01, New Vector3D(0, 0, 10), True))

        Dim medal__1 As Solid = Solid.Union(solidItems.ToArray())(0)

        medal__1.ColorMethod = colorMethodType.byEntity
        medal__1.MaterialName = "alu"
        medal__1.Translate(0, 0, 2)

        model.Entities.Add(medal__1, Color.White)


        ' jewel case
        Dim b1 As Solid = Solid.CreateBox(140, 140, 12)
        b1.Translate(-70, -70, 0)

        Dim b2 As Solid = Solid.CreateBox(108, 108, 12)
        b2.Translate(-54, -54, 2)

        Dim diff1 As Solid() = Solid.Difference(b1, b2)

        Dim pln As Plane = Plane.YZ

        Dim ln1 As New Line(pln, 0, 0, 4, 0)
        Dim ln2 As New Line(pln, 4, 0, 4, 4)
        Dim ln3 As New Line(pln, 4, 4, 8, 4)
        Dim a1 As New Arc(pln, New Point2D(12, 4), 4, Math.PI / 2, Math.PI)
        Dim ln4 As New Line(pln, 12, 8, 12, 12)
        Dim ln5 As New Line(pln, 12, 12, 0, 12)
        Dim ln6 As New Line(pln, 0, 12, 0, 0)

        Dim sect As New CompositeCurve(ln1, ln2, ln3, a1, ln4, ln5,
            ln6)

        sect.Translate(0, -70, 0)

        Dim sectReg As New devDept.Eyeshot.Entities.Region(sect)

        Dim rail As New LinearPath(New Point3D() {New Point3D(0, -70, 0), New Point3D(70, -70, 0), New Point3D(70, +70, 0), New Point3D(-70, +70, 0), New Point3D(-70, -70, 0), New Point3D(0, -70, 0)})

        Dim frame As Solid = sectReg.SweepAsSolid(rail, 0.1)

        Dim diff2 As Solid() = Solid.Difference(diff1(0), frame)

        Dim jewelCase As Solid = diff2(0)

        jewelCase.ApplyMaterial(woodMatName, textureMappingType.Cubic, 1, 1)

        model.Entities.Add(jewelCase, Color.FromArgb(32, 0, 0))

    End Sub

    Public Shared Sub House(model As Model)
        Dim outer As Entities.Region
        
        outer = Entities.Region.CreatePolygon(New Point3D() {New Point3D(0, 0), New Point3D(460, 0), New Point3D(460, 100), New Point3D(600, 100), New Point3D(600, 400), New Point3D(0, 400)})

        ' House's extruded outer profile
        Dim body As Solid = outer.ExtrudeAsSolid(400, 0)

        ' Big room at origin
        Dim bigRoom As Solid = Solid.CreateBox(400, 340, 400)

        ' Moves big room in place
        bigRoom.Translate(30, 30, 0)

        ' Cuts the big room from the house's body
        Dim firstCut As Solid() = Solid.Difference(body, bigRoom)

        ' Small room
        Dim smallRoom As Solid = Solid.CreateBox(130, 240, 400)

        ' Moves small room in place
        smallRoom.Translate(440, 130, 0)

        ' Cuts the small room from the house's body
        Dim secondCut As Solid() = Solid.Difference(firstCut(0), smallRoom)

        ' Draws the main door profile on a vertical plane
        Dim pln As New Plane(New Point3D(100, 40, 0), Vector3D.AxisX, Vector3D.AxisZ)

        Dim l1 As New Line(pln, 0, 180, 0, 0)
        Dim l2 As New Line(pln, 0, 0, 120, 0)
        Dim l3 As New Line(pln, 120, 0, 120, 180)
        Dim a1 As New Arc(pln, New Point2D(60, 155), New Point2D(120, 180), New Point2D(0, 180))

        Dim reg As New devDept.Eyeshot.Entities.Region(New CompositeCurve(l1, l2, l3, a1))

        ' Cuts the main door profile from the house's body
        secondCut(0).ExtrudeRemove(reg, 50, 1)

        ' central horizontal beam
        Dim beam1 As Solid = Solid.CreateBox(680, 30, 40)

        ' moves in place
        beam1.Translate(-40, 185, 360)

        ' cut the house's body
        Dim thirdCut As Solid() = Solid.Difference(secondCut(0), beam1)

        ' same for other two horizontal beams
        Dim beam2 As Solid = Solid.CreateBox(680, 20, 40)

        beam2.Translate(-40, 0, 280)

        Dim fourthCut As Solid() = Solid.Difference(thirdCut(0), beam2)

        Dim beam3 As Solid = Solid.CreateBox(680, 20, 40)

        beam3.Translate(-40, 380, 280)

        Dim fifthCut As Solid() = Solid.Difference(fourthCut(0), beam3)

        ' Intersection tool loop
        outer = Entities.Region.CreatePolygon(Plane.YZ, New Point2D() {New Point2D(0, 0), New Point2D(400, 0), New Point2D(400, 300), New Point2D(200, 400), New Point2D(0, 300)})

        ' Tool body
        Dim intersectionTool As Solid = outer.ExtrudeAsSolid(Vector3D.AxisX * 680, 0)

        ' Moves the tool in place
        intersectionTool.Translate(-40, 0, 0)

        ' Intersects the house's body with the tool
        Dim firstInters As Solid() = Solid.Intersection(fifthCut(0), intersectionTool)

        ' Intersects the horizontal beams with the tool
        Dim secondInters As Solid() = Solid.Intersection(beam1, intersectionTool)
        Dim thirdInters As Solid() = Solid.Intersection(beam2, intersectionTool)
        Dim fourthInters As Solid() = Solid.Intersection(beam3, intersectionTool)

        ' Adds beams to the scene
        model.Entities.AddRange(secondInters, Color.SaddleBrown)
        model.Entities.AddRange(thirdInters, Color.SaddleBrown)
        model.Entities.AddRange(fourthInters, Color.SaddleBrown)

        ' Basement sweep rail
        Dim rail As New LinearPath(New Point3D() {New Point3D(220, 0), New Point3D(460, 0), New Point3D(460, 100), New Point3D(600, 100), New Point3D(600, 400), New Point3D(0, 400),
            New Point3D(0, 0), New Point3D(100, 0)})

        ' Basement sweep section
        Dim section As Entities.Region 
        
        section = Entities.Region.CreatePolygon(New Point3D() {New Point3D(220, 0, 0), New Point3D(220, -7.5, 0), New Point3D(220, 0, 75)})

        ' Sweep solid
        Dim basement As Solid = section.SweepAsSolid(rail, 0)

        ' Merges sweep with the house's body
        Dim firstUnion As Solid() = Solid.Union(firstInters(0), basement)

        ' Internal door
        Dim door As Solid = Solid.CreateBox(30, 80, 210)

        ' Moves internal door in place
        door.Translate(420, 140, 0)

        ' Cuts the internal door from the house's body
        Dim sixthCut As Solid() = Solid.Difference(firstUnion(0), door)

        Dim beam10 As Solid = Solid.CreateBox(10, 120, 20)
        beam10.Translate(430, 120, 210)
        model.Entities.Add(beam10, Color.Gray)

        Dim seventhCut As Solid() = Solid.Difference(sixthCut(0), beam10)

        ' Window
        Dim window As Solid = Solid.CreateBox(90, 50, 140)

        ' Moves window in place
        window.Translate(280, -10, 90)

        ' Cuts the window from the house's body
        Dim eighthCut As Solid() = Solid.Difference(seventhCut(0), window)

        Dim windowLedge As Solid = Solid.CreateBox(100, 35, 5)
        windowLedge.Translate(275, -5, 85)
        model.Entities.Add(windowLedge, Color.Gray)

        Dim sixthCut3 As Solid() = Solid.Difference(eighthCut(0), windowLedge)

        sixthCut3(0).SmoothingAngle = Utility.DegToRad(1)

        model.Entities.AddRange(sixthCut3, Color.WhiteSmoke)


        ' Oblique beam loop
        Dim obliqueLoop As Entities.Region
        
        obliqueLoop = Entities.Region.CreatePolygon(Plane.YZ, New Point2D() {New Point2D(200, 0), New Point2D(-60, -130), New Point2D(-60, -150), New Point2D(200, -20)})

        ' Oblique beam
        Dim oblique As Solid = obliqueLoop.ExtrudeAsSolid(10, 0)

        ' Moves in place
        oblique.Translate(-40, 0, 420)

        ' A list of entities we need to mirror
        Dim toBeMirrored As New List(Of Entity)()

        toBeMirrored.Add(oblique)

        ' Copies and adds the oblique beam
        For i As Integer = 0 To 6

            Dim clone As Entity = DirectCast(oblique.Clone(), Entity)

            clone.Translate((((680 - 8 * 10) / 7.0) + 10) * (i + 1), 0, 0)


            toBeMirrored.Add(clone)
        Next

        ' Copies and mirrors
        Dim count As Integer = toBeMirrored.Count

        Dim mirrorPlane As Plane = Plane.ZX

        mirrorPlane.Origin.Y = 200

        Dim m As New Mirror(mirrorPlane)

        For i As Integer = 0 To count - 1
            Dim clone As Entity = DirectCast(toBeMirrored(i).Clone(), Entity)

            clone.TransformBy(m)

            toBeMirrored.Add(clone)
        Next

        ' Adds all the array items to the scene
        model.Entities.AddRange(toBeMirrored, Color.SaddleBrown)


    End Sub

End Class

#End If
