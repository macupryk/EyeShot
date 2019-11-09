Imports System.Text
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Collections
Imports System.IO
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry


Imports devDept.Eyeshot.Triangulation
Imports Region = devDept.Eyeshot.Entities.Region
Imports devDept.Eyeshot.Translators


Partial Class Draw

	Public Shared Sub MotherBoard(model As Model)
	    Dim rf as ReadFile = New ReadFile(MainWindow.GetAssetsPath() + "Motherboard_ASRock_A330ION.eye")

	    model.StartWork(rf)

	End Sub

	Public Shared Sub Locomotive(model As Model)

		Dim r1 As Region = Region.CreateRectangle(110, 38)
		#If NURBS Then
		Dim r2 As Region = Region.CreateRectangle(0, 19, 8, 19)
		#Else
		Dim el As New Ellipse(0, 19, 0, 8, 19)
		el.Regen(1)
		Dim r2 As New Region(New LinearPath(el.Vertices), Plane.XY, False)
		#End If
		Dim u1 As Region = Region.Union(r1, r2)(0)

		Dim r3 As Region = Region.CreateCircle(17, -6, 9)

		Dim u2 As Region = Region.Union(u1, r3)(0)

		r3.Translate(20, 0, 0)

		Dim u3 As Region = Region.Union(u2, r3)(0)

		Dim r4 As Region = Region.CreateCircle(70, 0, 15)

		Dim u4 As Region = Region.Union(u3, r4)(0)

		Dim r5 As Region = Region.CreateCircle(50, 38, 10)

		Dim u5 As Region = Region.Union(u4, r5)(0)

		Dim r6 As Region = Region.CreateRectangle(79, 36, 44, 14)

		Dim u6 As Region = Region.Union(u5, r6)(0)

		Dim r7 As Region = Region.CreateRectangle(-11, 14, 10, 10)

		Dim u7 As Region = Region.Union(u6, r7)(0)

		Dim r8 As Region = Region.CreatePolygon(New Point2D(-15, -8), New Point2D(4, -8), New Point2D(4, 8))

		Dim u8 As Region = Region.Union(u7, r8)(0)

		Dim r9 As Region = Region.CreatePolygon(New Point2D(20, 20), New Point2D(32, 62), New Point2D(26, 72), New Point2D(14, 72), New Point2D(8, 62))

		Dim u9 As Region = Region.Union(u8, r9)(0)

		model.Entities.Add(u9, Color.IndianRed)

	End Sub

	Public Shared Sub Bunny(model As Model)

	    Dim readFile as ReadFile = New ReadFile(MainWindow.GetAssetsPath() + "Bunny.eye")
	    readFile.DoWork()

	    ' scales file contents by 100
	    For Each entity As Entity In readFile.Entities

	        entity.Scale(100, 100, 100)
	    Next

	    readFile.AddToScene(model)

		If model.Entities.Count > 0 AndAlso TypeOf model.Entities(0) Is FastPointCloud Then

			Dim fpc As FastPointCloud = DirectCast(model.Entities(0), FastPointCloud)

			fpc.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin)

			model.Entities.Regen()

			model.ZoomFit()

			Dim bp As New BallPivoting(fpc)


			model.StartWork(bp)
		End If


	End Sub

    Public Shared Sub Pocket(model As Model)

        Dim pts As Point2D() = New Point2D() {New Point2D(0, 0), New Point2D(40, 0), New Point2D(40, 20), New Point2D(60, 20), New Point2D(60, 10), New Point2D(100, 10),
            New Point2D(100, 60), New Point2D(60, 60), New Point2D(60, 30), New Point2D(40, 30), New Point2D(40, 80), New Point2D(0, 80),
            New Point2D(0, 0)}

        Dim outerContour As New LinearPath(Plane.XY, pts)

        outerContour.LineWeightMethod = colorMethodType.byEntity
        outerContour.LineWeight = 3

        model.Entities.Add(outerContour, Color.OrangeRed)

        Dim innerContour As New Circle(20, 60, 0, 6)

        innerContour.LineWeightMethod = colorMethodType.byEntity
        innerContour.LineWeight = 3

        model.Entities.Add(innerContour, Color.OrangeRed)

        Dim r1 As New Region(New ICurve() {outerContour, innerContour}, Plane.XY, True)

        Dim passes As ICurve() = r1.Pocket(4, cornerType.Round, 0.1)

        Const zStep As Double = 2
        Const stepCount As Integer = 10

        For i As Integer = 1 To stepCount - 1
            For Each crv As Entity In passes
                Dim en As Entity = DirectCast(crv.Clone(), Entity)
                en.Translate(0, 0, -i * zStep)
                model.Entities.Add(en, Color.DarkBlue)
            Next
        Next
    End Sub

    Public Shared Sub Primitives(model As Model)

		model.GetGrid().[Step] = 25
		model.GetGrid().Min.X = -25
		model.GetGrid().Min.Y = -25

		model.GetGrid().Max.X = 125
		model.GetGrid().Max.Y = 175

		Dim deltaOffset As Double = 50
		Dim offsetX As Double = 0
		Dim offsetY As Double = 0

		' First Row

		' Box
		Dim mesh__1 As Mesh = Mesh.CreateBox(40, 40, 30)
		mesh__1.Translate(-20, -20, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		' Cone
		mesh__1 = Mesh.CreateCone(20, 10, 30, 30, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		mesh__1 = Mesh.CreateCone(20, 0, 30, 30, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		' Second Row
		offsetX = 0
		offsetY += deltaOffset

		mesh__1 = Mesh.CreateCone(10, 20, 30, 30, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		mesh__1 = Mesh.CreateCone(20, 10, 30, 3, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		' Sphere
		mesh__1 = Mesh.CreateSphere(20, 3, 3, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)

		' Third Row
		offsetX = 0
		offsetY += deltaOffset

		mesh__1 = Mesh.CreateSphere(20, 8, 6, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		mesh__1 = Mesh.CreateSphere(20, 14, 14, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		mesh__1 = Mesh.CreateTorus(18, 5, 15, 17, Mesh.natureType.Smooth)
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		' Fourth Row
		offsetX = 0
		offsetY += deltaOffset

		Dim lp As LinearPath = LinearPath.CreateHelix(10, 5.3, 10.7, True, 0.25)
		lp.Translate(offsetX, offsetY, 0)
		model.Entities.Add(lp, Draw.Color)
		offsetX += deltaOffset

		mesh__1 = Mesh.CreateSpring(10, 2, 16, 24, 10, 6, _
			True, True, Mesh.natureType.Smooth)
		mesh__1.EdgeStyle = Mesh.edgeStyleType.None
		mesh__1.Translate(offsetX, offsetY, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
		offsetX += deltaOffset

		' Sweep
		Dim z As Double = 30
		Dim radius As Double = 15

		Dim l1 As New Line(0, 0, 0, 0, 0, z)
		Dim a1 As New Arc(New Point3D(radius, 0, z), New Point3D(0, 0, z), New Point3D(radius, 0, z + radius))
		Dim l2 As New Line(radius, 0, z + radius, 30, 0, z + radius)

		Dim composite As New CompositeCurve(l1, a1, l2)
		Dim lpOuter As New LinearPath(10, 16)
		Dim lpInner As New LinearPath(5, 11)
		lpInner.Translate(2.5, 2.5, 0)
		lpInner.Reverse()

		Dim reg As New Region(lpOuter, lpInner)

		mesh__1 = reg.SweepAsMesh(composite, .25)
		mesh__1.Translate(offsetX - 10, offsetY - 8, 0)
		model.Entities.Add(mesh__1, Color.GreenYellow)
	End Sub

	Public Shared Sub TerrainTriangulation(model As Model)

        MainWindow.SetBackgroundStyleAndColor(model)

        Dim sideCount As Integer = 100

		Dim len As Integer = sideCount * sideCount

		Dim pts As Point3D() = New Point3D(len - 1) {}

		Dim rand As New Random(3)

		For j As Integer = 0 To sideCount - 1

			For i As Integer = 0 To sideCount - 1

				Dim x As Double = rand.NextDouble() * sideCount
				Dim y As Double = rand.NextDouble() * sideCount
				Dim z As Double = 0

				Dim _x As Double = x / 2 - 15
				Dim _y As Double = y / 2 - 15

				Dim den As Double = Math.Sqrt(_x * _x + _y * _y)

				If den <> 0 Then

					z = 10 * Math.Sin(Math.Sqrt(_x * _x + _y * _y)) / den
				End If

				Dim R As Integer = CInt(255 * (z + 2) / 12)
				Dim B As Integer = CInt(2.55 * y)

				Utility.LimitRange(Of Integer)(0, R, 255)
				Utility.LimitRange(Of Integer)(0, B, 255)

				Dim pt As New PointRGB(x, y, z, CByte(R), 255, CByte(B))


				pts(i + j * sideCount) = pt
			Next
		Next

	    Dim m As Mesh = UtilityEx.Triangulate(pts)

        model.Entities.Add(m)

        Dim pln As New Plane(New Point3D(0, 20, 20), New Vector3D(20, -30, 10))
        
        Dim pe As New PlanarEntity(pln, 25)
        
        model.Entities.Add(pe, Color.Magenta)
        
        Dim curves As ICurve() = m.Section(pln, 0)
        
        For Each ent As Entity In curves
        	model.Entities.Add(ent)
        Next
	End Sub

	Public Shared Sub CompositeCurveMeshing(model As Model)

        MainWindow.SetDisplayMode(model, displayType.Shaded)

        Dim outer As New CompositeCurve()

		outer.CurveList.Add(New Line(0, 0, 10, 0))
		outer.CurveList.Add(New Line(10, 0, 10, 6))
		outer.CurveList.Add(New Line(10, 6, 0, 6))
		outer.CurveList.Add(New Line(0, 6, 0, 0))

		Dim inner1 As New CompositeCurve()

		inner1.CurveList.Add(New Line(2, 2, 6, 2))
		inner1.CurveList.Add(New Line(6, 2, 2, 3))
		inner1.CurveList.Add(New Line(2, 3, 2, 2))

		Dim inner2 As New CompositeCurve()

		inner2.CurveList.Add(New Circle(8, 4, 0, 1))

		Dim inner3 As New CompositeCurve()

		inner3.CurveList.Add(New Circle(6, 4, 0, 0.75))

        Dim reg As Region = new Region(outer, inner1, inner2, inner3)


        Dim m As Mesh = UtilityEx.Triangulate(reg, .15)

        model.Entities.Add(m, Color.Salmon)
	End Sub


End Class

