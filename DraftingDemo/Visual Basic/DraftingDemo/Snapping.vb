Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Drawing

Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports objectSnapType = WpfApplication1.MainWindow.objectSnapType


	''' <summary>
	''' Contains utilities required for grid snapping or model vertex snapping.
	''' </summary>
	Partial  Class MyModel
        ' Current snapped point, which is one of the vertex from model
        Private mySnapPoint As Point3D = Nothing

        ' Flags to indicate current snapping mode
        Public Property objectSnapEnabled() As Boolean

		Public Property gridSnapEnabled() As Boolean

		Public Property waitingForSelection() As Boolean

		Public activeObjectSnap As MainWindow.objectSnapType = MainWindow.objectSnapType.End

		''' <summary>
		''' Finds closest snap point.
		''' </summary>
		''' <param name="snapPoints">Array of snap points</param>
		''' <returns>Closest snap point.</returns>
		Private Function FindClosestPoint(ByVal snapPoints() As SnapPoint) As SnapPoint

			Dim minDist As Double = Double.MaxValue

			Dim i As Integer = 0
			Dim index As Integer = 0

			For Each vertex As SnapPoint In snapPoints
				Dim vertexScreen As Point3D = WorldToScreen(vertex)
				Dim currentScreen As New Point2D(mouseLocation.X, Size.Height - mouseLocation.Y)

				Dim dist As Double = Point2D.Distance(vertexScreen, currentScreen)

				If dist < minDist Then
					index = i
					minDist = dist
				End If

				i += 1
			Next vertex

			Dim snap As SnapPoint = CType(snapPoints.GetValue(index), SnapPoint)
			DisplaySnappedVertex(snap)

			Return snap
		End Function


		''' <summary>
		''' Displays symbols associated with the snapped vertex type
		''' </summary>
		Private Sub DisplaySnappedVertex(ByVal snap As SnapPoint)
			renderContext.SetLineSize(2)

			' white color
			renderContext.SetColorWireframe(Color.FromArgb(0,0,255))
			renderContext.SetState(depthStencilStateType.DepthTestOff)

			Dim onScreen As Point2D = WorldToScreen(snap)

            Me.mySnapPoint = snap

            Select Case snap.Type
				Case MainWindow.objectSnapType.Point
					DrawCircle(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
					DrawCross(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
				Case MainWindow.objectSnapType.Center
					DrawCircle(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
				Case MainWindow.objectSnapType.End
					DrawQuad(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
				Case MainWindow.objectSnapType.Mid
					DrawTriangle(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
				Case MainWindow.objectSnapType.Quad
					renderContext.SetLineSize(3.0F)
					DrawRhombus(New System.Drawing.Point(CInt(Math.Truncate(onScreen.X)), CInt(Math.Truncate(onScreen.Y))))
			End Select

			renderContext.SetLineSize(1)
		End Sub

		''' <summary>
		''' Adds entity to scene on active layer and refresh the screen. 
		''' </summary>
		Private Sub AddAndRefresh(ByVal entity As Entity, ByVal layerName As String)
			' increase dimension of points
			If TypeOf entity Is devDept.Eyeshot.Entities.Point Then
				entity.LineWeightMethod = colorMethodType.byEntity
				entity.LineWeight = 3
			End If

			' avoid dimensions with width bigger than one
			If TypeOf entity Is Dimension OrElse TypeOf entity Is Leader Then
				entity.LayerName = layerName
				entity.LineWeightMethod = colorMethodType.byEntity

				Entities.Add(entity)
			Else
				Entities.Add(entity, layerName)
			End If

			Entities.Regen()
			Invalidate()
		End Sub

		''' <summary>
		''' Tries to snap grid vertex for the current mouse point
		''' </summary>
		Private Function SnapToGrid(ByRef ptToSnap As Point3D) As Boolean
			Dim gridPoint As New Point2D(Math.Round(ptToSnap.X / 10) * 10, Math.Round(ptToSnap.Y / 10) * 10)

			If Point2D.Distance(gridPoint, ptToSnap) < magnetRange Then
				ptToSnap.X = gridPoint.X
				ptToSnap.Y = gridPoint.Y

				Return True
			End If

			Return False
		End Function
        ''' <summary>
        ''' Gets the nested entity inside the BlockReference under mouse cursor and computes its transformation
        ''' </summary>
        Private Function GetNestedEntity(mousePos As Drawing.Point, entList As IList(Of Entity), byref accumulatedParentTransform As Transformation) As Entity
            Dim index() As Integer
            Dim ent As Entity
            index = GetCrossingEntities(New Rectangle(mousePos.X-5,mousePos.Y-5,10,10),entList,True,true,accumulatedParentTransform)
            If (Not index is nothing and index.Length > 0) Then
                If TypeOf entList(index(0)) Is BlockReference Then
                    dim br as BlockReference=entList(index(0))
                    accumulatedParentTransform =accumulatedParentTransform*br.GetFullTransformation(Blocks)
                    ent = GetNestedEntity(mousePos, Blocks(br.BlockName).Entities,accumulatedParentTransform )
                    Return ent
                Else
                    Return entList(index(0))
                End If
            End If
            
            Return Nothing
        End function

		''' <summary>
		''' identify snapPoints of the entity under mouse cursor in that moment, using PickBoxSize as tolerance
		''' </summary>
		Public Function GetSnapPoints(ByVal mouseLocation As System.Drawing.Point) As SnapPoint()
			'changed PickBoxSize to define a range for display snapPoints
			Dim oldSize As Integer = PickBoxSize
			PickBoxSize = 10
    
    'select the entity under mouse cursor
    
      Dim accumulatedParentTransform as Transformation =New Identity()
      Dim ent As Entity = GetNestedEntity(mouseLocation, Entities,accumulatedParentTransform)

    PickBoxSize = oldSize

			Dim snapPoints(-1) As SnapPoint

    If Not ent Is Nothing Then

      'check which type of entity is it and then,identify snap points
      If TypeOf ent Is devDept.Eyeshot.Entities.Point Then
        Dim point As devDept.Eyeshot.Entities.Point = CType(ent, devDept.Eyeshot.Entities.Point)

        Select Case activeObjectSnap
          Case objectSnapType.Point
            Dim point3d As Point3D = point.Vertices(0)
            snapPoints = New SnapPoint() {New SnapPoint(point3d, objectSnapType.Point)}
        End Select
      ElseIf TypeOf ent Is Line Then 'line
        Dim line As Line = CType(ent, Line)

        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {
              New SnapPoint(line.StartPoint, objectSnapType.End),
              New SnapPoint(line.EndPoint, objectSnapType.End)
            }
          Case objectSnapType.Mid
            snapPoints = New SnapPoint() {New SnapPoint(line.MidPoint, objectSnapType.Mid)}
        End Select
      ElseIf TypeOf ent Is LinearPath Then 'polyline
        Dim polyline As LinearPath = CType(ent, LinearPath)
        Dim polyLineSnapPoints As New List(Of SnapPoint)()

        Select Case activeObjectSnap
          Case objectSnapType.End
            For Each point As Point3D In polyline.Vertices
              polyLineSnapPoints.Add(New SnapPoint(point, objectSnapType.End))
            Next point
            snapPoints = polyLineSnapPoints.ToArray()
        End Select
      ElseIf TypeOf ent Is CompositeCurve Then 'composite
        Dim composite As CompositeCurve = CType(ent, CompositeCurve)
        Dim polyLineSnapPoints As New List(Of SnapPoint)()

        Select Case activeObjectSnap
          Case objectSnapType.End
            For Each curveSeg As ICurve In composite.CurveList
              polyLineSnapPoints.Add(New SnapPoint(curveSeg.EndPoint, objectSnapType.End))
            Next curveSeg
            polyLineSnapPoints.Add(New SnapPoint(composite.CurveList(0).StartPoint, objectSnapType.End))
            snapPoints = polyLineSnapPoints.ToArray()
        End Select
      ElseIf TypeOf ent Is Arc Then 'Arc
        Dim arc As Arc = CType(ent, Arc)

        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {
              New SnapPoint(arc.StartPoint, objectSnapType.End),
              New SnapPoint(arc.EndPoint, objectSnapType.End)
            }
          Case objectSnapType.Mid
            snapPoints = New SnapPoint() {New SnapPoint(arc.MidPoint, objectSnapType.Mid)}
          Case objectSnapType.Center
            snapPoints = New SnapPoint() {New SnapPoint(arc.Center, objectSnapType.Center)}
        End Select
      ElseIf TypeOf ent Is Circle Then 'Circle
        Dim circle As Circle = CType(ent, Circle)

        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {New SnapPoint(circle.EndPoint, objectSnapType.End)}
          Case objectSnapType.Mid
            snapPoints = New SnapPoint() {New SnapPoint(circle.PointAt(circle.Domain.Mid), objectSnapType.Mid)}
          Case objectSnapType.Center
            snapPoints = New SnapPoint() {New SnapPoint(circle.Center, objectSnapType.Center)}
          Case objectSnapType.Quad
            Dim quad1 As New Point3D(circle.Center.X, circle.Center.Y + circle.Radius)
            Dim quad2 As New Point3D(circle.Center.X + circle.Radius, circle.Center.Y)
            Dim quad3 As New Point3D(circle.Center.X, circle.Center.Y - circle.Radius)
            Dim quad4 As New Point3D(circle.Center.X - circle.Radius, circle.Center.Y)

            snapPoints = New SnapPoint() {
              New SnapPoint(quad1, objectSnapType.Quad),
              New SnapPoint(quad2, objectSnapType.Quad),
              New SnapPoint(quad3, objectSnapType.Quad),
              New SnapPoint(quad4, objectSnapType.Quad)
            }
        End Select
#If NURBS Then
      ElseIf TypeOf ent Is Curve Then ' Spline
        Dim curve As Curve = CType(ent, Curve)

        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {
              New SnapPoint(curve.StartPoint, objectSnapType.End),
              New SnapPoint(curve.EndPoint, objectSnapType.End)
            }
          Case objectSnapType.Mid
            snapPoints = New SnapPoint() {New SnapPoint(curve.PointAt(0.5), objectSnapType.Mid)}
        End Select
#End If
      ElseIf TypeOf ent Is EllipticalArc Then 'Elliptical Arc
        Dim elArc As EllipticalArc = CType(ent, EllipticalArc)

        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {
              New SnapPoint(elArc.StartPoint, objectSnapType.End),
              New SnapPoint(elArc.EndPoint, objectSnapType.End)
            }
          Case objectSnapType.Center
            snapPoints = New SnapPoint() {New SnapPoint(elArc.Center, objectSnapType.Center)}
        End Select
      ElseIf TypeOf ent Is Ellipse Then 'Ellipse
        Dim ellipse As Ellipse = CType(ent, Ellipse)
        Select Case activeObjectSnap
          Case objectSnapType.End
            snapPoints = New SnapPoint() {New SnapPoint(ellipse.EndPoint, objectSnapType.End)}
          Case objectSnapType.Mid
            snapPoints = New SnapPoint() {New SnapPoint(ellipse.PointAt(ellipse.Domain.Mid), objectSnapType.Mid)}
          Case objectSnapType.Center
            snapPoints = New SnapPoint() {New SnapPoint(ellipse.Center, objectSnapType.Center)}
        End Select
      ElseIf TypeOf ent Is Mesh Then 'Mesh
        Dim mesh As Mesh = CType(ent, Mesh)

        Select Case activeObjectSnap
          Case objectSnapType.End

            snapPoints = New SnapPoint(mesh.Vertices.Length - 1) {}

            For i As Integer = 0 To mesh.Vertices.Length - 1
              Dim pt As Point3D = mesh.Vertices(i)
              snapPoints(i) = New SnapPoint(pt, objectSnapType.End)
            Next i
        End Select
      End If
    End If

    If Not accumulatedParentTransform Is New Identity() Then
      Dim p_tmp As Point3D
      For Each sp As SnapPoint In snapPoints
        p_tmp = accumulatedParentTransform * sp
        sp.X = p_tmp.X
        sp.Y = p_tmp.Y
        sp.Z = p_tmp.Z
      Next
    End If
     
    Return snapPoints

End Function
        
		#Region "SnappingData"

		''' <summary>
		''' Represents a 3D point from model vertices and associated snap type.
		''' </summary>
		Public Class SnapPoint
			Inherits devDept.Geometry.Point3D

			Public Type As MainWindow.objectSnapType

			Public Sub New()
				MyBase.New()
				Type = MainWindow.objectSnapType.None
			End Sub

			Public Sub New(ByVal point3D As Point3D, ByVal objectSnapType As MainWindow.objectSnapType)
				MyBase.New(point3D.X, point3D.Y, point3D.Z)
				Me.Type = objectSnapType
			End Sub

			Public Overrides Function ToString() As String
				Return MyBase.ToString() & " | " & Type.ToString()
			End Function
		End Class
		#End Region

		#Region "Snapping symbols"

		Private Const snapSymbolSize As Integer = 12

		''' <summary>
		''' Draw cross. Drawn with a circle identifies a single point
		''' </summary>
		Public Sub DrawCross(ByVal onScreen As System.Drawing.Point)
			Dim dim1 As Double = onScreen.X + (snapSymbolSize \ 2)
			Dim dim2 As Double = onScreen.Y + (snapSymbolSize \ 2)
			Dim dim3 As Double = onScreen.X - (snapSymbolSize \ 2)
			Dim dim4 As Double = onScreen.Y - (snapSymbolSize \ 2)

			Dim topLeftVertex As New Point3D(dim3, dim2)
			Dim topRightVertex As New Point3D(dim1, dim2)
			Dim bottomRightVertex As New Point3D(dim1, dim4)
			Dim bottomLeftVertex As New Point3D(dim3, dim4)

			renderContext.DrawLines(New Point3D() { bottomLeftVertex, topRightVertex, topLeftVertex, bottomRightVertex})
		End Sub

		''' <summary>
		''' Draw circle with renderContext to rapresent CENTER snap point
		''' </summary>
		Public Sub DrawCircle(ByVal onScreen As System.Drawing.Point)
			Dim radius As Double = snapSymbolSize \2

			Dim x2 As Double = 0, y2 As Double = 0

			Dim pts As New List(Of Point3D)()

			For angle As Integer = 0 To 359 Step 10
				Dim rad_angle As Double = Utility.DegToRad(angle)

				x2 = onScreen.X + radius * Math.Cos(rad_angle)
				y2 = onScreen.Y + radius * Math.Sin(rad_angle)

				Dim circlePoint As New Point3D(x2, y2)
				pts.Add(circlePoint)
			Next angle

			renderContext.DrawLineLoop(pts.ToArray())
		End Sub

		''' <summary>
		''' Draw quad with renderContext to rapresent END snap point
		''' </summary>
		Public Sub DrawQuad(ByVal onScreen As System.Drawing.Point)
			Dim dim1 As Double = onScreen.X + (snapSymbolSize \ 2)
			Dim dim2 As Double = onScreen.Y + (snapSymbolSize \ 2)
			Dim dim3 As Double = onScreen.X - (snapSymbolSize \ 2)
			Dim dim4 As Double = onScreen.Y - (snapSymbolSize \ 2)

			Dim topLeftVertex As New Point3D(dim3, dim2)
			Dim topRightVertex As New Point3D(dim1, dim2)
			Dim bottomRightVertex As New Point3D(dim1, dim4)
			Dim bottomLeftVertex As New Point3D(dim3, dim4)

			renderContext.DrawLineLoop(New Point3D() { bottomLeftVertex, bottomRightVertex, topRightVertex, topLeftVertex })
		End Sub

		''' <summary>
		''' Draw triangle with renderContext to rapresent MID snap point
		''' </summary>
		Private Sub DrawTriangle(ByVal onScreen As System.Drawing.Point)
			Dim dim1 As Double = onScreen.X + (snapSymbolSize \ 2)
			Dim dim2 As Double = onScreen.Y + (snapSymbolSize \ 2)
			Dim dim3 As Double = onScreen.X - (snapSymbolSize \ 2)
			Dim dim4 As Double = onScreen.Y - (snapSymbolSize \ 2)
			Dim dim5 As Double = onScreen.X

			Dim topCenter As New Point3D(dim5, dim2)

			Dim bottomRightVertex As New Point3D(dim1, dim4)
			Dim bottomLeftVertex As New Point3D(dim3, dim4)

			renderContext.DrawLineLoop(New Point3D() { bottomLeftVertex, bottomRightVertex, topCenter })
		End Sub


		Private Sub DrawRhombus(ByVal onScreen As System.Drawing.Point)
			Dim dim1 As Double = onScreen.X + (snapSymbolSize / 1.5)
			Dim dim2 As Double = onScreen.Y + (snapSymbolSize / 1.5)
			Dim dim3 As Double = onScreen.X - (snapSymbolSize / 1.5)
			Dim dim4 As Double = onScreen.Y - (snapSymbolSize / 1.5)

			Dim topVertex As New Point3D(onScreen.X, dim2)
			Dim bottomVertex As New Point3D(onScreen.X, dim4)
			Dim rightVertex As New Point3D(dim1, onScreen.Y)
			Dim leftVertex As New Point3D(dim3, onScreen.Y)

			renderContext.DrawLineLoop(New Point3D() { bottomVertex, rightVertex, topVertex, leftVertex})
		End Sub

		#End Region
	End Class

