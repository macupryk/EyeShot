Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Drawing

Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot
Imports devDept.Graphics


	''' <summary>
	''' Methods required to edit different entities interactively like offset, mirror, extend, trim, rotate etc.
	''' </summary>
	Partial  Class MyModel
		''' <summary>
		''' Tries to extend entity upto the selected boundary entity. For a short boundary line, it tries to extend selected
		''' entity upto elongated line.
		''' </summary>
		Private Sub ExtendEntity()
			If firstSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					firstSelectedEntity = Entities(selEntityIndex)
					selEntityIndex = -1
					Return
				End If
			ElseIf secondSelectedEntity Is Nothing Then
				DrawSelectionMark(mouseLocation)

				renderContext.EnableXOR(False)

				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select entity to extend", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
			End If

			If secondSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					secondSelectedEntity = Entities(selEntityIndex)
				End If
			End If

			If firstSelectedEntity IsNot Nothing AndAlso secondSelectedEntity IsNot Nothing Then
				If TypeOf firstSelectedEntity Is ICurve AndAlso TypeOf secondSelectedEntity Is ICurve Then
					Dim boundary As ICurve = TryCast(firstSelectedEntity, ICurve)
					Dim curve As ICurve = TryCast(secondSelectedEntity, ICurve)

					' Check which end of curve is near to boundary
					Dim t1 As Double = Nothing, t2 As Double = Nothing
					boundary.ClosestPointTo(curve.StartPoint, t1)
					boundary.ClosestPointTo(curve.EndPoint, t2)

					Dim projStartPt As Point3D = boundary.PointAt(t1)
					Dim projEndPt As Point3D = boundary.PointAt(t2)

					Dim curveStartDistance As Double = curve.StartPoint.DistanceTo(projStartPt)
					Dim curveEndDistance As Double = curve.EndPoint.DistanceTo(projEndPt)

					Dim success As Boolean = False
					If curveStartDistance < curveEndDistance Then
						If TypeOf curve Is Line Then
							success = ExtendLine(curve, boundary, True)
						ElseIf TypeOf curve Is LinearPath Then
							success = ExtendPolyLine(curve, boundary, True)
						ElseIf TypeOf curve Is Arc Then
							success = ExtendCircularArc(curve, boundary, True)
						ElseIf TypeOf curve Is EllipticalArc Then
							success = ExtendEllipticalArc(curve, boundary, True)
#If NURBS Then
						ElseIf TypeOf curve Is Curve Then
							success = ExtendSpline(curve, boundary, True)
#End If
						End If
					Else
						If TypeOf curve Is Line Then
							success = ExtendLine(curve, boundary, False)
						ElseIf TypeOf curve Is LinearPath Then
							success = ExtendPolyLine(curve, boundary, False)
						ElseIf TypeOf curve Is Arc Then
							success = ExtendCircularArc(curve, boundary, False)
						ElseIf TypeOf curve Is EllipticalArc Then
							success = ExtendEllipticalArc(curve, boundary, False)
#If NURBS Then
						ElseIf TypeOf curve Is Curve Then
							success = ExtendSpline(curve, boundary, False)
#End If
						End If
					End If
					If success Then
						Entities.Remove(secondSelectedEntity)
						Entities.Regen()
					End If
				End If
				ClearAllPreviousCommandData()
			End If
		End Sub

		''' <summary>
		''' Creates an elongated boundary when it is line.
		''' </summary>
		Private Function GetExtendedBoundary(ByVal boundary As ICurve) As ICurve
			If TypeOf boundary Is Line Then
				Dim tempLine As New Line(boundary.StartPoint, boundary.EndPoint)
				Dim dir1 As New Vector3D(tempLine.StartPoint, tempLine.EndPoint)
				dir1.Normalize()
				tempLine.EndPoint = tempLine.EndPoint + dir1 * extensionLength

				Dim dir2 As New Vector3D(tempLine.EndPoint, tempLine.StartPoint)
				dir2.Normalize()
				tempLine.StartPoint = tempLine.StartPoint + dir2 * extensionLength

				boundary = tempLine
			End If
			Return boundary
		End Function

		''' <summary>
		''' Returns closes point from given input point for provided point list.
		''' </summary>
		Private Function GetClosestPoint(ByVal point3D As Point3D, ByVal intersetionPoints() As Point3D) As Point3D
			Dim minsquaredDist As Double = Double.MaxValue
			Dim result As Point3D = Nothing

			For Each pt As Point3D In intersetionPoints
				Dim distSquared As Double = devDept.Geometry.Point3D.DistanceSquared(point3D, pt)
				If distSquared < minsquaredDist AndAlso Not point3D.Equals(pt) Then
					minsquaredDist = distSquared
					result = pt
				End If
			Next pt
			Return result
		End Function

		#Region "Extend Methods"
		' Extends input line upto the provided boundary.
		Private Function ExtendLine(ByVal lineCurve As ICurve, ByVal boundary As ICurve, ByVal nearStart As Boolean) As Boolean
			Dim line As Line = TryCast(lineCurve, Line)

			' Create temp line which will intersect boundary curve depending on which end to extend
			Dim tempLine As Line = Nothing
			Dim direction As Vector3D = Nothing
			If nearStart Then
				tempLine = New Line(line.StartPoint, line.StartPoint)
				direction = New Vector3D(line.EndPoint, line.StartPoint)
			Else
				tempLine = New Line(line.EndPoint, line.EndPoint)
				direction = New Vector3D(line.StartPoint, line.EndPoint)
			End If
			direction.Normalize()
			tempLine.EndPoint = tempLine.EndPoint + direction * extensionLength
#If NURBS Then
			' Get intersection points for input line and boundary
			' If not intersecting and boundary is line, we can try with extended boundary
			Dim intersetionPoints() As Point3D = Curve.Intersection(boundary, tempLine)
			If intersetionPoints.Length = 0 Then
				intersetionPoints = Curve.Intersection(GetExtendedBoundary(boundary), tempLine)
			End If

			' Modify line start/end point as closest intersection point
			If intersetionPoints.Length > 0 Then
				If nearStart Then
					line.StartPoint = GetClosestPoint(line.StartPoint, intersetionPoints)
				Else
					line.EndPoint = GetClosestPoint(line.EndPoint, intersetionPoints)
				End If
				AddAndRefresh(DirectCast(line.Clone(), Entity), DirectCast(lineCurve, Entity).LayerName)
				Return True
			End If
#End If
			Return False
		End Function

		' Method for polyline extension
		Private Function ExtendPolyLine(ByVal lineCurve As ICurve, ByVal boundary As ICurve, ByVal nearStart As Boolean) As Boolean
			Dim line As LinearPath = TryCast(secondSelectedEntity, LinearPath)
			Dim tempVertices() As Point3D = line.Vertices

			' create temp line with proper direction
			Dim tempLine As New Line(line.StartPoint, line.StartPoint)
			Dim direction As New Vector3D(line.Vertices(1), line.StartPoint)

			If Not nearStart Then
				tempLine = New Line(line.EndPoint, line.EndPoint)
				direction = New Vector3D(line.Vertices(line.Vertices.Length - 2), line.EndPoint)
			End If

			direction.Normalize()
			tempLine.EndPoint = tempLine.EndPoint + direction * extensionLength
#If NURBS Then
			Dim intersetionPoints() As Point3D = Curve.Intersection(boundary, tempLine)
			If intersetionPoints.Length = 0 Then
				intersetionPoints = Curve.Intersection(GetExtendedBoundary(boundary), tempLine)
			End If

			If intersetionPoints.Length > 0 Then
				If nearStart Then
					tempVertices(0) = GetClosestPoint(line.StartPoint, intersetionPoints)
				Else
					tempVertices(tempVertices.Length - 1) = GetClosestPoint(line.EndPoint, intersetionPoints)
				End If

				line.Vertices = tempVertices
				AddAndRefresh(DirectCast(line.Clone(), Entity), DirectCast(lineCurve, Entity).LayerName)
				Return True
			End If
#End If
			Return False
		End Function

		' Method for arc extension
		Private Function ExtendCircularArc(ByVal arcCurve As ICurve, ByVal boundary As ICurve, ByVal nearStart As Boolean) As Boolean
			Dim selCircularArc As Arc = TryCast(arcCurve, Arc)
			Dim tempCircle As New Circle(selCircularArc.Plane, selCircularArc.Center, selCircularArc.Radius)
#If NURBS Then
			Dim intersetionPoints() As Point3D = Curve.Intersection(boundary, tempCircle)
			If intersetionPoints.Length = 0 Then
				intersetionPoints = Curve.Intersection(GetExtendedBoundary(boundary), tempCircle)
			End If

			If intersetionPoints.Length > 0 Then
				If nearStart Then
					Dim intPoint As Point3D = GetClosestPoint(selCircularArc.StartPoint, intersetionPoints)
					Dim xAxis As New Vector3D(selCircularArc.Center, selCircularArc.EndPoint)
					xAxis.Normalize()
					Dim yAxis As Vector3D = Vector3D.Cross(Vector3D.AxisZ, xAxis)
					yAxis.Normalize()
					Dim arcPlane As New Plane(selCircularArc.Center, xAxis, yAxis)

					Dim v1 As New Vector2D(selCircularArc.Center, selCircularArc.EndPoint)
					v1.Normalize()
					Dim v2 As New Vector2D(selCircularArc.Center, intPoint)
					v2.Normalize()

					Dim arcSpan As Double = Vector2D.SignedAngleBetween(v1, v2)
					Dim newArc As New Arc(arcPlane, arcPlane.Origin, selCircularArc.Radius, 0, arcSpan)
					AddAndRefresh(newArc, DirectCast(arcCurve, Entity).LayerName)
				Else
					Dim intPoint As Point3D = GetClosestPoint(selCircularArc.EndPoint, intersetionPoints)

					'plane
					Dim xAxis As New Vector3D(selCircularArc.Center, selCircularArc.StartPoint)
					xAxis.Normalize()
					Dim yAxis As Vector3D = Vector3D.Cross(Vector3D.AxisZ, xAxis)
					yAxis.Normalize()
					Dim arcPlane As New Plane(selCircularArc.Center, xAxis, yAxis)

					Dim v1 As New Vector2D(selCircularArc.Center, selCircularArc.StartPoint)
					v1.Normalize()
					Dim v2 As New Vector2D(selCircularArc.Center, intPoint)
					v2.Normalize()

					Dim arcSpan As Double = Vector2D.SignedAngleBetween(v1, v2)
					Dim newArc As New Arc(arcPlane, arcPlane.Origin, selCircularArc.Radius, 0, arcSpan)
					AddAndRefresh(newArc, DirectCast(arcCurve, Entity).LayerName)
				End If
				Return True
			End If
#End If
			Return False
		End Function

		' Method for elliptical arc extension
		Private Function ExtendEllipticalArc(ByVal ellipticalArcCurve As ICurve, ByVal boundary As ICurve, ByVal start As Boolean) As Boolean
			Dim selEllipseArc As EllipticalArc = TryCast(ellipticalArcCurve, EllipticalArc)
			Dim tempEllipse As New Ellipse(selEllipseArc.Plane, selEllipseArc.Center, selEllipseArc.RadiusX, selEllipseArc.RadiusY)
#If NURBS Then
			Dim intersetionPoints() As Point3D = Curve.Intersection(boundary, tempEllipse)
			If intersetionPoints.Length = 0 Then
				intersetionPoints = Curve.Intersection(GetExtendedBoundary(boundary), tempEllipse)
			End If

			Dim newArc As EllipticalArc = Nothing

			If intersetionPoints.Length > 0 Then
				Dim arcPlane As Plane = selEllipseArc.Plane
				If start Then
					Dim intPoint As Point3D = GetClosestPoint(selEllipseArc.StartPoint, intersetionPoints)

					newArc = New EllipticalArc(arcPlane, selEllipseArc.Center, selEllipseArc.RadiusX, selEllipseArc.RadiusY, selEllipseArc.EndPoint, intPoint, False)
					' If start point is not on the new arc, flip needed
					Dim t As Double = Nothing
					newArc.ClosestPointTo(selEllipseArc.StartPoint, t)
					Dim projPt As Point3D = newArc.PointAt(t)
					If projPt.DistanceTo(selEllipseArc.StartPoint) > 0.1 Then
						newArc = New EllipticalArc(arcPlane, selEllipseArc.Center, selEllipseArc.RadiusX, selEllipseArc.RadiusY, selEllipseArc.EndPoint, intPoint, True)
					End If
					AddAndRefresh(newArc, DirectCast(ellipticalArcCurve, Entity).LayerName)
				Else
					Dim intPoint As Point3D = GetClosestPoint(selEllipseArc.EndPoint, intersetionPoints)
					newArc = New EllipticalArc(arcPlane, selEllipseArc.Center, selEllipseArc.RadiusX, selEllipseArc.RadiusY, selEllipseArc.StartPoint, intPoint, False)

					' If end point is not on the new arc, flip needed
					Dim t As Double = Nothing
					newArc.ClosestPointTo(selEllipseArc.EndPoint, t)
					Dim projPt As Point3D = newArc.PointAt(t)
					If projPt.DistanceTo(selEllipseArc.EndPoint) > 0.1 Then
						newArc = New EllipticalArc(arcPlane, selEllipseArc.Center, selEllipseArc.RadiusX, selEllipseArc.RadiusY, selEllipseArc.StartPoint, intPoint, True)
					End If
				End If
				If newArc IsNot Nothing Then
					AddAndRefresh(newArc, DirectCast(ellipticalArcCurve, Entity).LayerName)
					Return True
				End If
			End If
#End If
			Return False
		End Function

		' Method for spline extension
		Private Function ExtendSpline(ByVal curve As ICurve, ByVal boundary As ICurve, ByVal nearStart As Boolean) As Boolean
#If NURBS Then
			Dim originalSpline As Curve = TryCast(curve, Curve)

			Dim tempLine As Line = Nothing
			Dim direction As Vector3D = Nothing
			If nearStart Then
				tempLine = New Line(curve.StartPoint, curve.StartPoint)
				direction = curve.StartTangent
				direction.Normalize()
				direction.Negate()
				tempLine.EndPoint = tempLine.EndPoint + direction * extensionLength
			Else
				tempLine = New Line(curve.EndPoint, curve.EndPoint)
				direction = curve.EndTangent
				direction.Normalize()
				tempLine.EndPoint = tempLine.EndPoint + direction * extensionLength
			End If

			Dim intersetionPoints() As Point3D = devDept.Eyeshot.Entities.Curve.Intersection(boundary, tempLine)
			If intersetionPoints.Length = 0 Then
				intersetionPoints = devDept.Eyeshot.Entities.Curve.Intersection(GetExtendedBoundary(boundary), tempLine)
			End If

			If intersetionPoints.Length > 0 Then
				Dim ctrlPoints As List(Of Point4D) = originalSpline.ControlPoints.ToList()
				Dim newCtrlPoints As New List(Of Point3D)()
				If nearStart Then
					newCtrlPoints.Add(GetClosestPoint(curve.StartPoint, intersetionPoints))
					For Each ctrlPt As Point4D In ctrlPoints
						Dim point As New Point3D(ctrlPt.X, ctrlPt.Y, ctrlPt.Z)
						If Not point.Equals(originalSpline.StartPoint) Then
							newCtrlPoints.Add(point)
						End If
					Next ctrlPt
				Else
					For Each ctrlPt As Point4D In ctrlPoints
						Dim point As New Point3D(ctrlPt.X, ctrlPt.Y, ctrlPt.Z)
						If Not point.Equals(originalSpline.EndPoint) Then
							newCtrlPoints.Add(point)
						End If
					Next ctrlPt
					newCtrlPoints.Add(GetClosestPoint(curve.EndPoint, intersetionPoints))
				End If

				Dim newCurve As New Curve(originalSpline.Degree, newCtrlPoints)
				If newCurve IsNot Nothing Then
					AddAndRefresh(newCurve, DirectCast(curve, Entity).LayerName)
					Return True
				End If
			End If
#End If
			Return False
		End Function
		#End Region

		''' <summary>
		''' Trims selected entity by the cutting entity. Removes portion of the curve near mouse click.
		''' </summary>
		Private Sub TrimEntity()
			If firstSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					firstSelectedEntity = Entities(selEntityIndex)
					selEntityIndex = -1
					Return
				End If
			ElseIf secondSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					secondSelectedEntity = Entities(selEntityIndex)
				Else
					DrawSelectionMark(mouseLocation)
					renderContext.EnableXOR(False)
					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select entity to be trimmed", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
				End If
			End If

			If firstSelectedEntity IsNot Nothing AndAlso secondSelectedEntity IsNot Nothing Then
				If TypeOf firstSelectedEntity Is ICurve AndAlso TypeOf secondSelectedEntity Is ICurve Then
					Dim trimmingCurve As ICurve = TryCast(firstSelectedEntity, ICurve)
					Dim curve As ICurve = TryCast(secondSelectedEntity, ICurve)
#If NURBS Then
					Dim intersetionPoints() As Point3D = devDept.Eyeshot.Entities.Curve.Intersection(trimmingCurve, curve)
					If intersetionPoints.Length > 0 AndAlso points.Count > 0 Then
						Dim parameters As New List(Of Double)()
						For i As Integer = 0 To intersetionPoints.Length - 1
							Dim intersetionPoint = intersetionPoints(i)
							Dim t As Double = CType(intersetionPoint, InterPoint).s
							parameters.Add(t)
						Next i

						Dim distSelected As Double = 1

						Dim trimmedCurves() As ICurve = Nothing
						If parameters IsNot Nothing Then
							parameters.Sort()
							Dim u As Double = Nothing
							curve.ClosestPointTo(points(0), u)
							distSelected = Point3D.Distance(points(0), curve.PointAt(u))
							distSelected += distSelected / 1e3

							If u <= parameters(0) Then
								curve.SplitBy(New Point3D() { curve.PointAt(parameters(0)) }, trimmedCurves)
							ElseIf u > parameters(parameters.Count - 1) Then
								curve.SplitBy(New Point3D() { curve.PointAt(parameters(parameters.Count - 1)) }, trimmedCurves)
							Else
								For i As Integer = 0 To parameters.Count - 2
									If u > parameters(i) AndAlso u <= parameters(i + 1) Then
										curve.SplitBy(New Point3D() { curve.PointAt(parameters(i)), curve.PointAt(parameters(i + 1)) }, trimmedCurves)
									End If
								Next i
							End If
						End If

						Dim success As Boolean = False
						'Decide which portion of curve to be deleted
						For i As Integer = 0 To trimmedCurves.Length - 1
							Dim trimmedCurve As ICurve = trimmedCurves(i)
							Dim t As Double
                            trimmedCurve.ClosestPointTo(points(0), t)

                            If True Then
                                If (t < trimmedCurve.Domain.t0 OrElse t > trimmedCurve.Domain.t1) OrElse Point3D.Distance(points(0), trimmedCurve.PointAt(t)) > distSelected Then
                                    AddAndRefresh(DirectCast(trimmedCurve, Entity), secondSelectedEntity.LayerName)
                                    success = True
                                End If
                            End If
                        Next

						' Delete original entity to be trimmed
						If success Then
							Entities.Remove(secondSelectedEntity)
						End If
					End If
					ClearAllPreviousCommandData()
#End If
				End If
			End If
		End Sub

		''' <summary>
		''' Tries to fit chamfer line between selected curves. Chamfer distance is provided through user input box.
		''' </summary>
		Private Sub CreateChamferEntity()
			If firstSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					firstSelectedEntity = Entities(selEntityIndex)
					selEntityIndex = -1
					Return
				End If
			ElseIf secondSelectedEntity Is Nothing Then
				DrawSelectionMark(mouseLocation)
				renderContext.EnableXOR(False)
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second curve", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
			End If

			If secondSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					secondSelectedEntity = Entities(selEntityIndex)
				End If
			End If

			If TypeOf firstSelectedEntity Is ICurve AndAlso TypeOf secondSelectedEntity Is ICurve Then
				Dim chamferLine As Line = Nothing
				Dim distance As Double = Me.filletRadius
#If NURBS Then
				If Curve.Chamfer(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), distance, False, False, True, True, chamferLine) Then
					AddAndRefresh(chamferLine, ActiveLayerName)
				ElseIf Curve.Chamfer(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), distance, False, True, True, True, chamferLine) Then
					AddAndRefresh(chamferLine, ActiveLayerName)
				ElseIf Curve.Chamfer(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), distance, True, False, True, True, chamferLine) Then
					AddAndRefresh(chamferLine, ActiveLayerName)
				ElseIf Curve.Chamfer(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), distance, True, True, True, True, chamferLine) Then
					AddAndRefresh(chamferLine, ActiveLayerName)
				End If

				ClearAllPreviousCommandData()
#End If
			End If
		End Sub

		''' <summary>
		''' Tries to fit fillet arc between two selected curves. Fillet radius is given from user input box.
		''' </summary>
		Private Sub CreateFilletEntity()
			If firstSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					firstSelectedEntity = Entities(selEntityIndex)
					selEntityIndex = -1
					Return
				End If
			ElseIf secondSelectedEntity Is Nothing Then
				DrawSelectionMark(mouseLocation)
				renderContext.EnableXOR(False)
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second curve", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
			End If

			If secondSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					secondSelectedEntity = Entities(selEntityIndex)
				End If
			End If

			If TypeOf firstSelectedEntity Is ICurve AndAlso TypeOf secondSelectedEntity Is ICurve Then
				If TypeOf firstSelectedEntity Is Line AndAlso TypeOf secondSelectedEntity Is Line Then
					Dim l1 As Line = TryCast(firstSelectedEntity, Line)
					Dim l2 As Line = TryCast(secondSelectedEntity, Line)

					If Vector3D.AreParallel(l1.Direction, l2.Direction) Then
						ClearAllPreviousCommandData()
						Return
					End If
				End If
#If NURBS Then
				Dim filletArc As Arc = Nothing
				Try
					If Curve.Fillet(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), filletRadius, False, False, True, True, filletArc) Then
						AddAndRefresh(filletArc, ActiveLayerName)
					ElseIf Curve.Fillet(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), filletRadius, False, True, True, True, filletArc) Then
						AddAndRefresh(filletArc, ActiveLayerName)
					ElseIf Curve.Fillet(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), filletRadius, True, False, True, True, filletArc) Then
						AddAndRefresh(filletArc, ActiveLayerName)
					ElseIf Curve.Fillet(DirectCast(firstSelectedEntity, ICurve), DirectCast(secondSelectedEntity, ICurve), filletRadius, True, True, True, True, filletArc) Then
						AddAndRefresh(filletArc, ActiveLayerName)
					End If
				Catch
				End Try
#End If
				ClearAllPreviousCommandData()
			End If
		End Sub

		''' <summary>
		''' Creates mirror image of the selected entity for given mirror axis. Mirror axis is formed by selection two points.
		''' </summary>
		Private Sub CreateMirrorEntity()
			' We need to have two reference points selected, might be snapped vertices
			If points.Count < 2 Then
				'If entity is selected, ask user to select mirror line
				renderContext.EnableXOR(False)
				If points.Count = 0 AndAlso Not waitingForSelection Then
					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Start of mirror plane", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
				ElseIf points.Count = 1 Then
					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "End of mirror plane", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
				End If
				DrawInteractiveLines()
			Else
				If points(1).X < points(0).X OrElse points(1).Y < points(0).Y Then
					Dim p0 As Point3D = points(0)
					Dim p1 As Point3D = points(1)

					Utility.Swap(p0, p1)

					points(0) = p0
					points(1) = p1
				End If

				Dim axisX As New Vector3D(points(0), points(1))
				Dim mirrorPlane As New Plane(points(0), axisX, Vector3D.AxisZ)

				Dim mirrorEntity As Entity = DirectCast(selEntity.Clone(), Entity)
				Dim mirror As New Mirror(mirrorPlane)
				mirrorEntity.TransformBy(mirror)
				AddAndRefresh(mirrorEntity, ActiveLayerName)

				ClearAllPreviousCommandData()
			End If
		End Sub

		''' <summary>
		''' Tries to create offset entity for selected entity at the selected location (offset distance) and side.
		''' </summary>
		Private Sub CreateOffsetEntity()
			If selEntity IsNot Nothing AndAlso TypeOf selEntity Is ICurve Then
#If Not NURBS Then
				If TypeOf selEntity Is Ellipse OrElse TypeOf selEntity Is EllipticalArc Then
					Return
				End If
#End If
				If points.Count = 0 Then
					renderContext.EnableXOR(False)
					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Side to offset", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
					Return
				End If

				Dim selCurve As ICurve = TryCast(selEntity, ICurve)
				Dim t As Double = Nothing
				Dim success As Boolean = selCurve.Project(points(0), t)
				Dim projectedPt As Point3D = selCurve.PointAt(t)
				Dim offsetDist As Double = projectedPt.DistanceTo(points(0))

				Dim offsetCurve As ICurve = selCurve.Offset(offsetDist, Vector3D.AxisZ, 0.01, True)
				success = offsetCurve.Project(points(0), t)
				projectedPt = offsetCurve.PointAt(t)
				If projectedPt.DistanceTo(points(0)) > 1e-3 Then
					offsetCurve = selCurve.Offset(-offsetDist, Vector3D.AxisZ, 0.01, True)
				End If

				AddAndRefresh(DirectCast(offsetCurve, Entity), ActiveLayerName)
			End If
		End Sub
		Private Sub CreateTangentEntity()


			If firstSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					firstSelectedEntity = Entities(selEntityIndex)
					selEntityIndex = -1
					Return
				End If
			ElseIf secondSelectedEntity Is Nothing Then
				DrawSelectionMark(mouseLocation)
				renderContext.EnableXOR(False)
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second circle", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
			End If

			If secondSelectedEntity Is Nothing Then
				If selEntityIndex <> -1 Then
					secondSelectedEntity = Entities(selEntityIndex)
				End If
			End If

			If TypeOf firstSelectedEntity Is ICurve AndAlso TypeOf secondSelectedEntity Is ICurve Then
				If TypeOf firstSelectedEntity Is Circle AndAlso TypeOf secondSelectedEntity Is Circle Then
					Dim c1 As Circle = TryCast(firstSelectedEntity, Circle)
					Dim c2 As Circle = TryCast(secondSelectedEntity, Circle)


					Try
						If Me.lineTangents Then
							Dim res() As Line = UtilityEx.GetLinesTangentToTwoCircles(c1, c2)
							For Each line As Line In res
								AddAndRefresh(line, ActiveLayerName)
							Next line

						ElseIf Me.circleTangents Then

                        Dim res As List(Of Circle) = UtilityEx.GetCirclesTangentToTwoCircles(c1, c2, Me.tangentsRadius, Me.trimTangent, Me.flipTangent)
                        For Each circ As Circle In res
                            AddAndRefresh(circ, ActiveLayerName)
                        Next circ


                    Else
							Return
						End If
					Catch
					End Try

				End If
				ClearAllPreviousCommandData()
			End If

		End Sub
		Private Sub DrawCurveOrBlockRef(ByVal tempEntity As Entity)
			If TypeOf tempEntity Is ICurve Then
				Draw(TryCast(tempEntity, ICurve))
			ElseIf TypeOf tempEntity Is LinearDim Then
				Dim [dim] = CType(tempEntity, LinearDim)

				'Draw text
				Draw(New Line([dim].Vertices(6), [dim].Vertices(7)))
				Draw(New Line([dim].Vertices(7), [dim].Vertices(8)))
				Draw(New Line([dim].Vertices(8), [dim].Vertices(9)))
				Draw(New Line([dim].Vertices(9), [dim].Vertices(6)))

				'Draw lines
				Draw(New Line([dim].Vertices(0), [dim].Vertices(1)))
				Draw(New Line([dim].Vertices(2), [dim].Vertices(3)))
				Draw(New Line([dim].Vertices(4), [dim].Vertices(5)))
			ElseIf TypeOf tempEntity Is RadialDim Then
				Dim [dim] = CType(tempEntity, RadialDim)

				'Draw text
				Draw(New Line([dim].Vertices(6), [dim].Vertices(7)))
				Draw(New Line([dim].Vertices(7), [dim].Vertices(8)))
				Draw(New Line([dim].Vertices(8), [dim].Vertices(9)))
				Draw(New Line([dim].Vertices(9), [dim].Vertices(6)))

				Draw(New Line([dim].Vertices(0), [dim].Vertices(5)))
			ElseIf TypeOf tempEntity Is AngularDim Then
				Dim [dim] = CType(tempEntity, AngularDim)

				'Draw text
				Draw(New Line([dim].Vertices(4), [dim].Vertices(5)))
				Draw(New Line([dim].Vertices(5), [dim].Vertices(6)))
				Draw(New Line([dim].Vertices(6), [dim].Vertices(7)))
				Draw(New Line([dim].Vertices(7), [dim].Vertices(4)))

				Draw(New Line([dim].Vertices(0), [dim].Vertices(1)))
				Draw(New Line([dim].Vertices(2), [dim].Vertices(3)))
				Draw([dim].UnderlyingArc)
			ElseIf TypeOf tempEntity Is OrdinateDim Then
				Dim [dim] = CType(tempEntity, OrdinateDim)

				'Draw text
				Draw(New Line([dim].Vertices(4), [dim].Vertices(5)))
				Draw(New Line([dim].Vertices(5), [dim].Vertices(6)))
				Draw(New Line([dim].Vertices(6), [dim].Vertices(7)))
				Draw(New Line([dim].Vertices(7), [dim].Vertices(4)))

				Draw(New Line([dim].Vertices(0), [dim].Vertices(1)))
				Draw(New Line([dim].Vertices(1), [dim].Vertices(2)))
				Draw(New Line([dim].Vertices(2), [dim].Vertices(3)))
			ElseIf TypeOf tempEntity Is Text Then
				Dim txt = CType(tempEntity, Text)

				Draw(New Line(txt.Vertices(0), txt.Vertices(1)))
				Draw(New Line(txt.Vertices(1), txt.Vertices(2)))
				Draw(New Line(txt.Vertices(2), txt.Vertices(3)))
				Draw(New Line(txt.Vertices(3), txt.Vertices(0)))
			ElseIf TypeOf tempEntity Is BlockReference Then
				Dim br As BlockReference = CType(tempEntity, BlockReference)

				Dim entList() As Entity = br.Explode(Me.Blocks)

				For Each item As Entity In entList
					Dim curve As ICurve = TryCast(item, ICurve)
					If curve IsNot Nothing Then
						Draw(curve)
					End If
				Next item

			ElseIf TypeOf tempEntity Is Leader Then
				Dim leader = CType(tempEntity, Leader)

				Draw(New Line(leader.Vertices(0), leader.Vertices(1)))
				Draw(New Line(leader.Vertices(1), leader.Vertices(2)))
			End If
		End Sub

		''' <summary>
		''' Translates selected entity for given movement. User needs to select base point and new location.
		''' </summary>
		Private Sub MoveEntity()
			If points.Count = 0 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select base point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
				Return
			ElseIf points.Count = 1 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)

				' Show temp entity for current movement state
				For Each ent As Entity In Me.selEntities
					Dim tempEntity As Entity = DirectCast(ent.Clone(), Entity)
					Dim tempMovement As New Vector3D(points(0), current)
					tempEntity.Translate(tempMovement)

					If TypeOf tempEntity Is Text Then
						tempEntity.Regen(New RegenParams(0, Me))
					End If

					DrawCurveOrBlockRef(tempEntity)
				Next ent
			End If
		End Sub

		''' <summary>
		''' Scales selected entities for given scale factor and base point. Base point and scale factor is interactively provided
		''' by selecting reference points.
		''' </summary>
		Private Sub ScaleEntity()
			Dim worldToScreenVertices = New List(Of Point3D)()
			For Each v In points
				worldToScreenVertices.Add(WorldToScreen(v))
			Next v

			renderContext.DrawLineStrip(worldToScreenVertices.ToArray())

			If ActionMode = actionType.None AndAlso worldToScreenVertices.Count() > 0 Then
				renderContext.DrawLineStrip(New Point3D() { WorldToScreen(points.First()), WorldToScreen(current) })
			End If

			If points.Count = 0 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select origin", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
			ElseIf points.Count = 1 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select first reference point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
			ElseIf points.Count = 2 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second reference point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)

				scaleFactor = points(0).DistanceTo(current) / points(0).DistanceTo(points(1))

				' Show temp entities for current scale state
				For Each ent As Entity In Me.selEntities
					Dim tempEntity As Entity = DirectCast(ent.Clone(), Entity)
					tempEntity.Scale(points(0),If(scaleFactor = 0, 1, scaleFactor))

					If TypeOf tempEntity Is Text Then
						tempEntity.Regen(New RegenParams(0, Me))
					End If

					DrawCurveOrBlockRef(tempEntity)
				Next ent
			End If
		End Sub

		''' <summary>
		''' Rotates selected entities by given angle about given center of rotation. Angle is computed similar to drawing arc.
		''' </summary>
		Public Sub RotateEntity()
			DrawInteractiveArc()
			If points.Count = 0 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select center of rotation", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
			ElseIf points.Count = 1 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select first reference point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
			ElseIf points.Count = 2 Then
				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "Select second reference point", New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)

				' Show temp entities for current rotation state
				For Each ent As Entity In Me.selEntities
					Dim tempEntity As Entity = DirectCast(ent.Clone(), Entity)
					tempEntity.Rotate(arcSpanAngle, Vector3D.AxisZ, points(0))

					If TypeOf tempEntity Is Text Then
						tempEntity.Regen(New RegenParams(0, Me))
					End If

					DrawCurveOrBlockRef(tempEntity)
				Next ent
			End If
		End Sub

		Private secondSelectedEntity As Entity = Nothing
		Private firstSelectedEntity As Entity = Nothing

		Public lineTangents As Boolean
		Public circleTangents As Boolean

		Public tangentsRadius As Double = 10.0
		Public filletRadius As Double = 10.0
		Public rotationAngle As Double = 45.0
		Public scaleFactor As Double = 1.5
		Private extensionLength As Double = 500

		#Region "Flags indicating current editing mode"

		Public doingMirror As Boolean
		Public doingOffset As Boolean
		Public doingFillet As Boolean
		Public doingChamfer As Boolean
		Public doingTangents As Boolean
		Public doingMove As Boolean
		Public doingRotate As Boolean
		Public doingScale As Boolean
		Public doingTrim As Boolean
		Public doingExtend As Boolean
		Public editingMode As Boolean

		#End Region

		Public flipTangent As Boolean
		Public trimTangent As Boolean
	End Class

