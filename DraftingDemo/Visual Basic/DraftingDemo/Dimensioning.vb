Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Drawing
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports devDept.Graphics


	''' <summary>
	''' Contains methods required for dimensioning different entities.
	''' Linear, aligned, radial and diametric dimensioning is supported as of now.
	''' </summary>
	Partial  Class MyModel

		' Draws preview of horizontal/vertical dimension for a line
		Private Sub DrawInteractiveLinearDim()
			' We need to have two reference points selected, might be snapped vertices
			If points.Count < 2 Then
				Return
			End If

			Dim verticalDim As Boolean = (current.X > points(0).X AndAlso current.X > points(1).X) OrElse (current.X < points(0).X AndAlso current.X < points(1).X)

			Dim axisX As Vector3D

			If verticalDim Then

				axisX = Vector3D.AxisY

				extPt1 = New Point3D(current.X, points(0).Y)
				extPt2 = New Point3D(current.X, points(1).Y)

				If current.X > points(0).X AndAlso current.X > points(1).X Then
					extPt1.X += dimTextHeight / 2
					extPt2.X += dimTextHeight / 2
				Else
					extPt1.X -= dimTextHeight / 2
					extPt2.X -= dimTextHeight / 2
				End If

			Else 'for horizontal

				axisX = Vector3D.AxisX

				extPt1 = New Point3D(points(0).X, current.Y)
				extPt2 = New Point3D(points(1).X, current.Y)

				If current.Y > points(0).Y AndAlso current.Y > points(1).Y Then
					extPt1.Y += dimTextHeight / 2
					extPt2.Y += dimTextHeight / 2
				Else
					extPt1.Y -= dimTextHeight / 2
					extPt2.Y -= dimTextHeight / 2
				End If

			End If

			Dim axisY As Vector3D = Vector3D.Cross(Vector3D.AxisZ, axisX)


			Dim pts As New List(Of Point3D)()

			' Draw extension line1
			pts.Add(WorldToScreen(points(0)))
			pts.Add(WorldToScreen(extPt1))

			' Draw extension line2
			pts.Add(WorldToScreen(points(1)))
			pts.Add(WorldToScreen(extPt2))

			'Draw dimension line
			Dim extLine1 As New Segment3D(points(0), extPt1)
			Dim extLine2 As New Segment3D(points(1), extPt2)
			Dim pt1 As Point3D = current.ProjectTo(extLine1)
			Dim pt2 As Point3D = current.ProjectTo(extLine2)

			pts.Add(WorldToScreen(pt1))
			pts.Add(WorldToScreen(pt2))

			renderContext.DrawLines(pts.ToArray())

			'store dimensioning plane
			drawingPlane = New Plane(points(0), axisX, axisY)

			'draw dimension text
			renderContext.EnableXOR(False)

			Dim dimText As String = "L " & extPt1.DistanceTo(extPt2).ToString("f3")
			DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, dimText, New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
		End Sub

		' Draws preview of aligned dimension
		Private Sub DrawInteractiveAlignedDim()
			' We need to have two reference points selected, might be snapped vertices
			If points.Count < 2 Then
				Return
			End If

			If points(1).X < points(0).X OrElse points(1).Y < points(0).Y Then
				Dim p0 As Point3D = points(0)
				Dim p1 As Point3D = points(1)

				Utility.Swap(p0, p1)

				points(0) = p0
				points(1) = p1
			End If

			Dim axisX As New Vector3D(points(0), points(1))
			Dim axisY As Vector3D = Vector3D.Cross(Vector3D.AxisZ, axisX)

			drawingPlane = New Plane(points(0), axisX, axisY)

			Dim v1 As New Vector2D(points(0), points(1))
			Dim v2 As New Vector2D(points(0), current)

			Dim sign As Double = Math.Sign(Vector2D.SignedAngleBetween(v1, v2))

			'offset p0-p1 at current
			Dim segment As New Segment2D(points(0), points(1))
			Dim offsetDist As Double = current.DistanceTo(segment)
			extPt1 = points(0) + sign * drawingPlane.AxisY * (offsetDist + dimTextHeight /2)
			extPt2 = points(1) + sign * drawingPlane.AxisY * (offsetDist + dimTextHeight /2)
			Dim dimPt1 As Point3D = points(0) + sign * drawingPlane.AxisY * offsetDist
			Dim dimPt2 As Point3D = points(1) + sign * drawingPlane.AxisY * offsetDist

			Dim pts As New List(Of Point3D)()

			' Draw extension line1
			pts.Add(WorldToScreen(points(0)))
			pts.Add(WorldToScreen(extPt1))

			' Draw extension line2
			pts.Add(WorldToScreen(points(1)))
			pts.Add(WorldToScreen(extPt2))

			'Draw dimension line
			pts.Add(WorldToScreen(dimPt1))
			pts.Add(WorldToScreen(dimPt2))

			renderContext.DrawLines(pts.ToArray())

			'draw dimension text
			renderContext.EnableXOR(False)

			Dim dimText As String = "L " & extPt1.DistanceTo(extPt2).ToString("f3")
			DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, dimText, New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
		End Sub

		' Draws preview of ordinate dimension
		Private Sub DrawInteractiveOrdinateDim()
			' We need to have at least one point.
			If points.Count < 1 Then
				Return
			End If

			Dim pts As New List(Of Point3D)()
			Dim leaderEndPoint As Point3D = Nothing
			Dim segments() As Segment3D = OrdinateDim.Preview(devDept.Geometry.Plane.XY, points(0), current, drawingOrdinateDimVertical, dimTextHeight * 3, dimTextHeight, 0.625, 3.0, 0.625, leaderEndPoint)

			For Each segment3D In segments
				pts.Add(WorldToScreen(segment3D.P0))
				pts.Add(WorldToScreen(segment3D.P1))
			Next segment3D

			'draw the segments
			renderContext.DrawLines(pts.ToArray())

			'draw dimension text
			renderContext.EnableXOR(False)

			Dim distance As Double = If(drawingOrdinateDimVertical, Math.Abs(devDept.Geometry.Plane.XY.Origin.X - points(0).X), Math.Abs(devDept.Geometry.Plane.XY.Origin.Y - points(0).Y))

			Dim dimText As String = "D " & distance.ToString("f3")
			DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, dimText, New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
		End Sub

		' Draws preview of radial/diametric dimension with text like R5.25, Ø12.62
		Private Sub DrawInteractiveDiametricDim()
			If selEntityIndex <> -1 Then
				Dim entity As Entity = Me.Entities(selEntityIndex)
				If TypeOf entity Is Circle Then 'arc is a circle
					Dim cicularEntity As Circle = TryCast(entity, Circle)

					'draw center mark
					DrawPositionMark(cicularEntity.Center)

					'draw elastic line between center and cursor point
					renderContext.DrawLine(WorldToScreen(cicularEntity.Center), WorldToScreen(current))

					' disables draw inverted
					renderContext.EnableXOR(False)

					Dim dimText As String = "R" & cicularEntity.Radius.ToString("f3")

					If drawingDiametricDim Then
						dimText = "Ø" & cicularEntity.Diameter.ToString("f3")
					End If
					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, dimText, New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
				End If
			End If
		End Sub

		' Draws preview of radial/diametric dimension with text like R5.25, Ø12.62
		Private Sub DrawInteractiveAngularDim()
			If selEntityIndex <> -1 Then
				Dim entity As Entity = Entities(selEntityIndex)

				If TypeOf entity Is Arc AndAlso Not drawingAngularDimFromLines Then
					Dim selectedArc As Arc = TryCast(entity, Arc)

					'draw center mark
					DrawPositionMark(selectedArc.Center)

					'draw elastic line between center and cursor point
					renderContext.DrawLine(WorldToScreen(selectedArc.Center), WorldToScreen(current))

					' disables draw inverted
					renderContext.EnableXOR(False)

					Dim dimText As String = "A " & Utility.RadToDeg(selectedArc.Domain.Length).ToString("f3") & "°"

					DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, dimText, New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
				End If
			ElseIf drawingAngularDimFromLines AndAlso quadrantPoint IsNot Nothing Then
				'draw quadrant point mark
				DrawPositionMark(quadrantPoint)

				'draw elastic line between quadrant Point and cursor point
				renderContext.DrawLine(WorldToScreen(quadrantPoint), WorldToScreen(current))

				' disables draw inverted
				renderContext.EnableXOR(False)

				DrawText(mouseLocation.X, CInt(Size.Height) - mouseLocation.Y + 10, "", New Font("Tahoma", 8.25F), DrawingColor, ContentAlignment.BottomLeft)
			End If
		End Sub

		#Region "Dimensioning Flags"

		Public drawingLinearDim As Boolean

		Public drawingAlignedDim As Boolean

		Public drawingRadialDim As Boolean

		Public drawingDiametricDim As Boolean

		Public drawingAngularDim As Boolean

		Public drawingAngularDimFromLines As Boolean

		Public drawingOrdinateDim As Boolean
		Public drawingOrdinateDimVertical As Boolean

		Public drawingQuadrantPoint As Boolean

		Public dimTextHeight As Double = 2.5

		#End Region
	End Class

