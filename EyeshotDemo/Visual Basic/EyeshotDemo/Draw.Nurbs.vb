Imports System.Collections.Generic
Imports System.Drawing
Imports System.Collections
Imports System.IO

Imports devDept.Geometry
Imports devDept.Eyeshot.Labels
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities

Imports Region = devDept.Eyeshot.Entities.Region

#If NURBS Then
Imports devDept.Eyeshot.Translators

Partial Class Draw

	Public Shared Sub HairDryer(model As Model)

		Dim bhd As New BuildHairDryer()
		model.StartWork(bhd)

	End Sub

    Private Class BuildHairDryer
        Inherits WorkUnit

        Private trimTol As Double = 0.001
        Private filletTol As Double = 0.001
        Private offsetTol As Double = 0.1
        Private offsetAmount As Double = -1

        Private totalSteps As Integer = 5

        Private whiteEntList As New List(Of Entity)()
        Private darkEntList As New List(Of Entity)()
        Private offsetEntList As New List(Of Entity)()

        Protected Overrides Sub DoWork(worker As System.ComponentModel.BackgroundWorker, doWorkEventArgs As System.ComponentModel.DoWorkEventArgs)

            ' body -------------------------

            If Not UpdateProgressAndCheckCancelled(1, totalSteps, "Body drawing", worker, doWorkEventArgs) Then

                Return
            End If

            Dim s1 As Surface = DrawBody()

            whiteEntList.Add(s1)

            ' body closure
            Dim a5 As New Arc(80, 0, 0, 110, Math.PI, 1.11 * Math.PI)
            Dim s7 As Surface = a5.RevolveAsSurface(Math.PI, Math.PI, Vector3D.AxisX, Point3D.Origin)(0)
            whiteEntList.Add(s7)

            ' fillet
            Dim f1 As Surface() = Nothing
            Surface.Fillet(s1, s7, 5, filletTol, True, True,
                True, True, True, False, f1)

            whiteEntList.AddRange(f1)

            ' handle ------------------------

            If Not UpdateProgressAndCheckCancelled(2, totalSteps, "Handle drawing", worker, doWorkEventArgs) Then

                Return
            End If

            Dim len1 As Double = 150
            Dim ang1 As Double = -Math.PI / 2.8

            ' back
            Dim a1 As New Arc(Plane.YZ, New Point2D(22, 0), 50, 4 * Math.PI / 5, Math.PI)

            Dim s2 As Surface = a1.ExtrudeAsSurface(len1, 0, 0)(0)

            s2.Rotate(ang1, Vector3D.AxisZ)

            whiteEntList.Add(s2)

            ' front
            Dim a2 As New Arc(Plane.YZ, New Point2D(-30, 0), 30, 0, Math.PI / 5)
            Dim a3 As New Arc(Plane.YZ, New Point2D(-15, -40), 60, 9 * Math.PI / 20, 12 * Math.PI / 20)

            Dim a4 As Arc = Nothing
            Curve.Fillet(a2, a3, 10, False, False, True,
                True, a4)

            Dim cc1 As New CompositeCurve(a2, a4, a3)

            Dim s3 As Surface = cc1.ExtrudeAsSurface(len1, 0, 0)(0)

            s3.Rotate(ang1, Vector3D.AxisZ)

            whiteEntList.Add(s3)

            ' bottom
            Dim ln3 As New Line(0, -125, 100, -125)

            s2.Regen(0.1)

            Dim s6 As Surface = ln3.ExtrudeAsSurface(New Vector3D(0, 0, s2.BoxMax.Z), Utility.DegToRad(2), trimTol)(0)

            whiteEntList.Add(s6)


            ' fillets ------------------------------
            If Not UpdateProgressAndCheckCancelled(3, totalSteps, "Computing fillets", worker, doWorkEventArgs) Then

                Return
            End If

            Dim f2 As Surface() = Nothing, f3 As Surface() = Nothing, f4 As Surface() = Nothing

            ' rear fillet
            Surface.Fillet(New Surface() {s2}, New Surface() {s6}, 10, filletTol, True, True,
                True, True, True, False, f2)
            whiteEntList.AddRange(f2)



            ' along handle fillet
            Surface.Fillet(New Surface() {s3}, New Surface() {s2, s6, f2(0)}, 5, filletTol, True, True,
                True, True, True, False, f3)
            whiteEntList.AddRange(f3)

            ' handle-body fillet
            Surface.Fillet(New Surface() {s1}, New Surface() {s3, s2, s6, f2(0), f3(0)}, 10, filletTol, False, False,
                True, True, True, False, f4)

            For Each surface__1 As Surface In f4
                surface__1.ReverseU()
            Next

            whiteEntList.AddRange(f4)

            ' nozzle ------------------------
            Dim s8 As Surface = DrawNozzle(trimTol)

            darkEntList.Add(s8)

            ' offset ------------------------

            If Not UpdateProgressAndCheckCancelled(4, totalSteps, "Computing offset", worker, doWorkEventArgs) Then

                Return
            End If

            offsetEntList.AddRange(OffsetSurfaces(offsetAmount, offsetTol, whiteEntList))
            offsetEntList.AddRange(OffsetSurfaces(offsetAmount, offsetTol, darkEntList))

            ' mirror ------------------------

            If Not UpdateProgressAndCheckCancelled(5, totalSteps, "Computing mirror", worker, doWorkEventArgs) Then

                Return
            End If

            Dim m As New Mirror(Plane.XY)

            MirrorEntities(m, whiteEntList)

            If Cancelled(worker, doWorkEventArgs) Then
                Return
            End If

            MirrorEntities(m, darkEntList)

            If Cancelled(worker, doWorkEventArgs) Then
                Return
            End If

            MirrorEntities(m, offsetEntList)

            If Cancelled(worker, doWorkEventArgs) Then
                Return
            End If
        End Sub

        Private Shared Function OffsetSurfaces(amount As Double, tol As Double, whiteEntList As IList(Of Entity)) As Entity()
            Dim offSurf As New List(Of Entity)()

            For i As Integer = 0 To whiteEntList.Count - 1
                Dim entity As Entity = whiteEntList(i)
                If TypeOf entity Is Surface Then
                    Dim surf As Surface = DirectCast(entity, Surface)

                    Dim offset As Surface = Nothing
                    If surf.Offset(amount, tol, offset) Then

                        offSurf.Add(offset)
                    End If
                End If
            Next

            Return offSurf.ToArray()
        End Function

        Private Shared Sub MirrorEntities(m As Mirror, entList As IList(Of Entity))
            Dim count As Integer = entList.Count

            For i As Integer = 0 To count - 1
                Dim entity As Entity = entList(i)

                If TypeOf entity Is Surface Then
                    Dim copy As Surface = DirectCast(entity.Clone(), Surface)

                    copy.TransformBy(m)

                    entList.Add(copy)
                End If
            Next
        End Sub

        Private Shared Function DrawNozzle(trimTol As Double) As Surface
            Dim a1 As New Arc(Plane.YZ, Point2D.Origin, 30, 0, Math.PI)
            a1.Translate(81, 0, 0)
            Dim a2 As Curve = New Arc(Plane.YZ, Point2D.Origin, 27, 0, Math.PI).GetNurbsForm()
            a2.Scale(1, 1, 1.15)
            a2.Translate(81 + 10, 0, 0)
            Dim a3 As Curve = New Arc(Plane.YZ, Point2D.Origin, 34, 0, Math.PI).GetNurbsForm()
            a3.Scale(1, 0.5, 1)
            a3.Translate(81 + 30, 0, 0)
            Dim a4 As Curve = New Arc(Plane.YZ, Point2D.Origin, 34, 0, Math.PI).GetNurbsForm()
            a4.Scale(1, 0.5, 1)
            a4.Translate(81 + 40, 0, 0)

            Dim s1 As Surface = Surface.Loft(New ICurve() {a1, a2, a3, a4}, 3)(0)

            s1.ReverseU()

            Dim a5 As New Arc(Plane.ZX, Point2D.Origin, 120, 0, Math.PI)
            a5.Translate(0, -20, 0)

            Dim s2 As Surface = a5.ExtrudeAsSurface(0, 40, 0)(0)

            s1.TrimBy(s2, trimTol, False)

            Return s1
        End Function

        Private Shared Function DrawBody() As Surface
            ' simple
            Dim ln1 As New Line(-30, 32, 80, 32)

            Dim s1 As Surface = ln1.RevolveAsSurface(0, Math.PI, Vector3D.AxisX, Point3D.Origin)(0)

            ' advanced
            Dim a1 As New Arc(Plane.YZ, Point2D.Origin, 32, 0, Math.PI)
            a1.Translate(-30, 0, 0)
            Dim a2 As New Arc(Plane.YZ, Point2D.Origin, 35, 0, Math.PI)
            a2.Translate(-20, 0, 0)
            Dim a3 As New Arc(Plane.YZ, Point2D.Origin, 36, 0, Math.PI)
            a3.Translate(30, 0, 0)
            Dim a4 As New Arc(Plane.YZ, Point2D.Origin, 30, 0, Math.PI)
            a4.Translate(80, 0, 0)

            s1 = Surface.Loft(New ICurve() {a1, a2, a3, a4}, 3)(0)

            s1.ReverseU()

            Return s1

        End Function

        Protected Overrides Sub WorkCompleted(model As Environment)
            model.Entities.AddRange(whiteEntList, "Default", Color.WhiteSmoke)
            model.Entities.AddRange(darkEntList, "Default", Color.FromArgb(31, 31, 31))
            model.Entities.AddRange(offsetEntList, "Default", Color.DarkGray)
            model.SetView(viewType.Trimetric)
            model.ZoomFit()
        End Sub
    End Class

    Public Shared Sub Toolpath(model As Model)
        MainWindow.SetDisplayMode(model, displayType.Shaded)

        '#Region "Surface construction"

        Dim pointList As New List(Of Point3D)()

		pointList.Add(New Point3D(0, 60, 0))
		pointList.Add(New Point3D(0, 40, +10))
		pointList.Add(New Point3D(0, 20, -5))
		pointList.Add(New Point3D(0, 0, 0))

		Dim first As Curve = Curve.GlobalInterpolation(pointList, 3)

		pointList.Clear()
		pointList.Add(New Point3D(40, 55, 0))
		pointList.Add(New Point3D(40, 30, 25))
		pointList.Add(New Point3D(40, 5, 10))

		Dim second As Curve = Curve.GlobalInterpolation(pointList, 2)

		pointList.Clear()
		pointList.Add(New Point3D(80, 60, 0))
		pointList.Add(New Point3D(80, 30, 20))
		pointList.Add(New Point3D(80, 0, -10))

		Dim third As Curve = Curve.GlobalInterpolation(pointList, 2)

		Dim loft As Surface = Surface.Loft(New Curve() {first, second, third}, 2)(0)

		' flips surface direction
		loft.ReverseU()

		'#End Region

		' Coarsenes surface tessellation for faster semi-tranparent pre-processing
		loft.Regen(0.25)

		model.Entities.Add(loft, Color.FromArgb(200, Color.OrangeRed))

		model.ZoomFit()

		Dim btp As New BuildToolpath(loft, 0.01, 5)
		model.StartWork(btp)

	End Sub

    Private Class BuildToolpath
        Inherits WorkUnit

        Private surface As Surface
        Private tolerance As Double
        Private ballToolRadius As Double
        Private toolPath As LinearPath

        Public Sub New(surf As Surface, tol As Double, toolRadius As Double)
            surface = surf
            tolerance = tol
            ballToolRadius = toolRadius
        End Sub

        Protected Overrides Sub DoWork(worker As System.ComponentModel.BackgroundWorker, doWorkEventArgs As System.ComponentModel.DoWorkEventArgs)

            Const passCount As Integer = 50

            If Not UpdateProgressAndCheckCancelled(100, 100, "Triangulating surface 1/1", worker, doWorkEventArgs) Then

                Return
            End If

            Dim m As Mesh = surface.ConvertToMesh(tolerance)

            ' The plane used to slice the surface
            Dim pln As Plane = Plane.YZ

            pln.Rotate(-Math.PI / 4, Vector3D.AxisZ)

            pln.Translate(0, 60, 0)

            Dim pointList As New List(Of Point3D)()

            For i As Integer = 0 To passCount - 1

                pln = pln.Offset(2)

                Dim sectionCurves As ICurve() = m.Section(pln, 0)

                If sectionCurves.Length > 0 Then

                    Dim pass As LinearPath = DirectCast(sectionCurves(0), LinearPath)

                    Dim offsetLp As ICurve() = pass.QuickOffset(ballToolRadius, pln)

                    If offsetLp IsNot Nothing Then

                        If i Mod 2 = 1 Then

                            offsetLp(0).Reverse()
                        End If


                        pointList.AddRange(DirectCast(offsetLp(0), Entity).Vertices)

                    End If
                End If

                If Not UpdateProgressAndCheckCancelled(i, passCount, "Computing passes", worker, doWorkEventArgs) Then

                    Return

                End If
            Next

            ' raises approach and retract
            Dim approach As Point3D = DirectCast(pointList(0).Clone(), Point3D)

            approach.Z += 20

            pointList.Insert(0, approach)

            Dim retract As Point3D = DirectCast(pointList(pointList.Count - 1).Clone(), Point3D)

            retract.Z += 40

            pointList.Add(retract)

            ' return the toolpath as a LinearPath entity
            toolPath = New LinearPath(pointList)

        End Sub

        Protected Overrides Sub WorkCompleted(model As Environment)
            
            model.Entities.Add(toolPath, "Default", Color.DarkBlue)

            '#Region "Tool symbol definition"

            Dim b1 As New Block("ballTool")

            Dim c1 As New Circle(0, 0, 0, ballToolRadius)
            Dim c2 As New Circle(0, 0, 50, ballToolRadius)
            Dim a1 As New Arc(0, 0, 0, ballToolRadius, Math.PI, 2 * Math.PI)
            a1.Rotate(Math.PI / 2, Vector3D.AxisX)
            Dim a2 As Arc = DirectCast(a1.Clone(), Arc)
            a2.Rotate(Math.PI / 2, Vector3D.AxisZ)

            Dim l1 As New Line(-ballToolRadius, 0, 0, -ballToolRadius, 0, 50)

            b1.Entities.Add(c1)
            b1.Entities.Add(c2)
            b1.Entities.Add(a1)
            b1.Entities.Add(a2)
            b1.Entities.Add(l1)

            Dim lp1 As LinearPath = LinearPath.CreateHelix(ballToolRadius, 50, 1, False, 0.1)
            b1.Entities.Add(lp1)

            b1.Entities.Add(lp1)
            For i As Integer = 1 To 3
                Dim cloneLn As Line = DirectCast(l1.Clone(), Line)
                cloneLn.Rotate(i * Math.PI / 2, Vector3D.AxisZ)

                b1.Entities.Add(cloneLn)
            Next

            model.Blocks.Add(b1)

            '#End Region

            ' Adds a reference to the tool symbol
            model.Entities.Add(New BlockReference(toolPath.Vertices(toolPath.Vertices.Length - 1), "ballTool", 1, 1, 1, 0))

            model.ZoomFit()

        End Sub

    End Class

    Public Shared Sub Flange(model As Model)
	    Dim cc1 As New CompositeCurve(New Line(Plane.XZ, 15, 40, 29, 40), New Arc(Plane.XZ, New Point2D(29, 39), 1, 0, Utility.DegToRad(90)), New Line(Plane.XZ, 30, 39, 30, 16), New Arc(Plane.XZ, New Point2D(36, 16), 6, Math.PI, Utility.DegToRad(270)), New Line(Plane.XZ, 36, 10, 79, 10), New Arc(Plane.XZ, New Point2D(79, 9), 1, 0, Utility.DegToRad(90)), _
	    New Line(Plane.XZ, 80, 9, 80, 6), New Arc(Plane.XZ, New Point2D(86, 6), 6, Utility.DegToRad(180), Utility.DegToRad(270)), New Line(Plane.XZ, 86, 0, 130, 0))

        Dim reg As Entities.Region = cc1.OffsetToRegion(5, 0, False)
        
        Dim rev1 As Brep = reg.RevolveAsBrep(Math.PI * 2, Vector3D.AxisZ, Point3D.Origin)
        
        model.Entities.Add(rev1, System.Drawing.Color.Aqua)
        
        Dim cssr1 As Region
        
        cssr1 = Region.CreateCircularSlot(0, Utility.DegToRad(30), 60, 8)
        
        rev1.ExtrudeRemovePattern(cssr1, New Interval(0, 50), Point3D.Origin, Utility.DegToRad(360) / 3, 3)
        
        Dim rr1 As Region
        
        rr1 = Region.CreateRectangle(90, -40, 50, 80)
        
        rev1.ExtrudeRemovePattern(rr1, New Interval(0, 50), Point3D.Origin, Utility.DegToRad(360) / 2, 2)
        
        Dim cr1 As Region
        
        cr1 = Region.CreateCircle(110, 0, 10)
        
        Const  numHoles As Integer = 8
        
        rev1.ExtrudeRemovePattern(cr1, 50, Point3D.Origin, Utility.DegToRad(360) / numHoles, numHoles)
        
        model.Entities.Regen()
    End Sub

    Public Shared Sub Bracket(model As Model)
        Dim rrscc1 As CompositeCurve
        
        rrscc1 = CompositeCurve.CreateRoundedRectangle(Plane.YZ, 40, 120, 12, True)

        Dim sscc1 As CompositeCurve
        
        sscc1 = CompositeCurve.CreateSlot(Plane.YZ, 9, 5.25, True)
        
        sscc1.Translate(0, 0, 43)
        
        Dim sscc2 As CompositeCurve
        
        sscc2 = CompositeCurve.CreateSlot(Plane.YZ, 9, 5.25, True)
        
        sscc2.Rotate(Utility.DegToRad(90), Vector3D.AxisX, Point3D.Origin)
        
        sscc2.Translate(0, 0, -40)
        
        Dim c1 As New Circle(Plane.YZ, 4.25)
        
        Dim r1 As New Entities.Region(rrscc1, sscc1, sscc2, c1)
        
        Dim ext1 As Brep = r1.ExtrudeAsBrep(-4)
        
        model.Entities.Add(ext1, "Default", Color.YellowGreen)
        
        Dim cc1 As New CompositeCurve(New Line(Plane.YZ, 8, -10, 11, -10), New Arc(Plane.YZ, New Point2D(11, -5), 5, Utility.DegToRad(270), Utility.DegToRad(360)), New Line(Plane.YZ, 16, -5, 16, +5), New Arc(Plane.YZ, New Point2D(11, +5), 5, Utility.DegToRad(0), Utility.DegToRad(90)), New Line(Plane.YZ, 11, 10, -11, 10), New Arc(Plane.YZ, New Point2D(-11, +5), 5, Utility.DegToRad(90), Utility.DegToRad(180)), _
        	New Line(Plane.YZ, -16, +5, -16, -5), New Arc(Plane.YZ, New Point2D(-11, -5), 5, Utility.DegToRad(180), Utility.DegToRad(270)), New Line(Plane.YZ, -11, -10, -8, -10))
        
        Dim r2 As Entities.Region = cc1.OffsetToRegion(-2.5, 0, False)
        
        ext1.ExtrudeAdd(r2, 275)
        
        Dim ssr2 As Region
        
        ssr2 = Region.CreateSlot(Plane.XY, 12, 5.25)
        
        ssr2.Translate(9, 0, 0)
        
        ext1.ExtrudeRemovePattern(ssr2, 10, 35, 8, 0, 1)
        
        model.Entities.Regen()
    End Sub

End Class

#End If
