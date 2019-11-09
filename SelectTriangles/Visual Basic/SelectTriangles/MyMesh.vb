Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

	Public Class MyMesh
		Inherits Mesh
		Implements ISelect

		Private vp As MyModel

		Public Sub New(ByVal vp As MyModel, ByVal other As Mesh)
			MyBase.New(other)
			Me.vp = vp
		End Sub
    
		Public _selectedSubItems As New List(Of Integer)()

		' Gets or sets the list of selected triangles 
		Public Property SelectedSubItems() As List(Of Integer) Implements ISelect.SelectedSubItems
			Get
				Return _selectedSubItems
			End Get
			Set(ByVal value As List(Of Integer))
				_selectedSubItems = value
				RegenMode = regenType.CompileOnly
			End Set
		End Property

		Protected Overrides Sub DrawForShadow(ByVal data As RenderParams)
			DrawSelected(data)
		End Sub

		Public Property DrawSubItemsForSelection() As Boolean Implements ISelect.DrawSubItemsForSelection

		Protected Overrides Sub DrawForSelection(ByVal data As GfxDrawForSelectionParams)
			If DrawSubItemsForSelection Then
				Dim prev = vp.SuspendSetColorForSelection
				vp.SuspendSetColorForSelection = False

				' Draws the triangles with the color-coding needed for visibility computation 
				For index As Integer = 0 To _selectedSubItems.Count - 1
					'draws only the triangles with normals directions towards the Camera
					Dim i As Integer = _selectedSubItems(index)

					Dim p1 As Point3D = Vertices(Triangles(i).V1)

					Dim p2 As Point3D = Vertices(Triangles(i).V2)
					Dim p3 As Point3D = Vertices(Triangles(i).V3)

					Dim u() As Double = { p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z }
					Dim v() As Double = { p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z }

					' cross product
					Dim Normal As New Vector3D(u(1) * v(2) - u(2) * v(1), u(2) * v(0) - u(0) * v(2), u(0) * v(1) - u(1) * v(0))
					Normal.Normalize()

					If Vector3D.Dot(vp.Camera.NearPlane.AxisZ, Normal) <= 0 Then
						Continue For
					End If


					vp.SetColorDrawForSelection(i)

					Dim tri As IndexTriangle = Triangles(i)
					data.RenderContext.DrawTriangles(New Point3D() { Vertices(tri.V1), Vertices(tri.V2), Vertices(tri.V3)}, Vector3D.AxisZ)
				Next index

				vp.SuspendSetColorForSelection = prev

				' reset the color to avoid issues with the entities drawn after this one
				data.RenderContext.SetColorWireframe(System.Drawing.Color.White)

			Else
				MyBase.DrawForSelection(data)
			End If
		End Sub

		Protected Overrides Sub DrawIsocurves(ByVal data As DrawParams)
			data.RenderContext.Draw(drawData)
		End Sub

		Protected Overrides Sub DrawSelected(ByVal drawParams As DrawParams)
			MyBase.Draw(drawParams)
		End Sub

		Private Sub DrawSelectedSubItems(ByVal data As DrawParams)
			' Draws the selected triangles over the other triangles 
			If SelectedSubItems.Count = 0 Then
				Return
			End If

			Dim popState As Boolean = False
			Dim alpha As Integer = 255
			If data.RenderContext.LightingEnabled() Then
				If data.RenderContext.CurrentMaterial.Diffuse.A = 255 Then
					data.RenderContext.PushDepthStencilState()
					data.RenderContext.SetState(depthStencilStateType.DepthTestEqual)
					popState = True
				Else
					alpha = data.RenderContext.CurrentMaterial.Diffuse.A
				End If
			Else
				If data.RenderContext.CurrentWireColor.A = 255 Then
					data.RenderContext.PushDepthStencilState()
					data.RenderContext.SetState(depthStencilStateType.DepthTestEqual)
					popState = True
				Else
					alpha = data.RenderContext.CurrentWireColor.A
				End If
			End If

			Dim prevCol = data.RenderContext.CurrentWireColor
			Dim prevMatFront = data.RenderContext.CurrentMaterial.Diffuse
			Dim prevMatBack = data.RenderContext.CurrentBackMaterial.Diffuse

			If data.RenderContext.LightingEnabled() Then

				data.RenderContext.SetMaterialFrontAndBackDiffuse(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow), True)

			Else
				data.RenderContext.SetColorWireframe(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow))
			End If

			' to properly support multicolor mesh during triangles selection
			If vp.UseShaders Then
				data.RenderContext.PushShader()

				Select Case vp.DisplayMode
					Case displayType.Flat, displayType.HiddenLines, displayType.Wireframe
						data.RenderContext.SetShader(shaderType.NoLights)
					Case Else
						data.RenderContext.SetShader(shaderType.Standard)
				End Select
			End If

			data.RenderContext.Draw(drawSelectedData)

			If vp.UseShaders Then
				data.RenderContext.PopShader()
			End If

			data.RenderContext.SetColorWireframe(prevCol)
			data.RenderContext.SetMaterialFrontDiffuse(prevMatFront)
			data.RenderContext.SetMaterialBackDiffuse(prevMatBack)

			If popState Then
				data.RenderContext.PopDepthStencilState()
			End If
		End Sub

		Public Sub CompileSelected(ByVal renderContext As RenderContextBase)
			Dim vboP As New VBOParams()
			Dim pts() As Point3D = Nothing
			Dim selNormals() As Vector3D = Nothing
			GetSelectedData(pts, selNormals)

			vboP.vertices = ConvertToFloatArray(pts)
			vboP.normals = ConvertToFloatArray(selNormals)
			vboP.primitiveMode = primitiveType.TriangleList

			renderContext.CompileVBO(drawSelectedData, AddressOf CompileSelectedSubItems, vboP)

			renderContext.Compile(drawSelectedEdgesData, AddressOf CompileSelectedEdges, renderContext)


			needsCompileSelected = False
		End Sub

		Private Sub CompileSelectedSubItems(ByVal renderContext As RenderContextBase, ByVal myParams As Object)
			' Compiles the selected triangles
			Dim pts() As Point3D = Nothing
			Dim selNormals() As Vector3D = Nothing
			GetSelectedData(pts, selNormals)

			renderContext.DrawTriangles(pts, selNormals)
		End Sub

		Private drawSelectedEdgesData As New EntityGraphicsData()

		Private Sub CompileSelectedEdges(ByVal renderContext As RenderContextBase, ByVal myParams As Object)
			Dim pts = New Point3D((_selectedSubItems.Count * 6) - 1){}

			Dim i As Integer = 0
			Dim count As Integer = 0
			While i < SelectedSubItems.Count
				Dim tri1 = Triangles(SelectedSubItems(i))
			    pts(count) = Vertices(tri1.V1)
			    System.Threading.Interlocked.Increment(count)
			    pts(count) = Vertices(tri1.V2)
			    System.Threading.Interlocked.Increment(count)
			    pts(count) = Vertices(tri1.V2)
			    System.Threading.Interlocked.Increment(count)
			    pts(count) = Vertices(tri1.V3)
			    System.Threading.Interlocked.Increment(count)
			    pts(count) = Vertices(tri1.V3)
			    System.Threading.Interlocked.Increment(count)
			    pts(count) = Vertices(tri1.V1)
			    System.Threading.Interlocked.Increment(count)
				i += 1
			End While

			DirectCast(myParams, RenderContextBase).DrawLines(pts)
		End Sub

		Private Sub GetSelectedData(ByRef pts() As Point3D, ByRef selNormals() As Vector3D)
			pts = New Point3D((SelectedSubItems.Count*3) - 1){}
			selNormals = New Vector3D(pts.Length - 1){}

			Dim count As Integer = 0

			If TypeOf Triangles(0) Is ITriangleSupportsNormals Then
				Dim i As Integer = 0
				While i < SelectedSubItems.Count
					Dim tri1 = Triangles(SelectedSubItems(i))

					pts(count) = Vertices(tri1.V1)
					pts(count + 1) = Vertices(tri1.V2)
					pts(count + 2) = Vertices(tri1.V3)

					Dim tri As ITriangleSupportsNormals = DirectCast(tri1, ITriangleSupportsNormals)
					selNormals(count) = Normals(tri.N1)
					selNormals(count + 1) = Normals(tri.N2)
					selNormals(count + 2) = Normals(tri.N3)
					i += 1
					count += 3
				End While
			Else
				Dim i As Integer = 0
				While i < SelectedSubItems.Count
					Dim tri1 = Triangles(SelectedSubItems(i))

					pts(count) = Vertices(tri1.V1)
					pts(count + 1) = Vertices(tri1.V2)
					pts(count + 2) = Vertices(tri1.V3)

					selNormals(count) = Normals(SelectedSubItems(i))
					selNormals(count + 1) = Normals(SelectedSubItems(i))
					selNormals(count + 2) = Normals(SelectedSubItems(i))
					i += 1
					count += 3
				End While
			End If
		End Sub

		Private Function ConvertToFloatArray(ByVal pts() As Point3D) As Single()
			Dim a((pts.Length * 3) - 1) As Single

			Dim count As Integer = 0
			For i As Integer = 0 To pts.Length - 1
			    a(count) = CSng(pts(i).X)
			    System.Threading.Interlocked.Increment(count)
			    a(count) = CSng(pts(i).Y)
			    System.Threading.Interlocked.Increment(count)
			    a(count) = CSng(pts(i).Z)
			    System.Threading.Interlocked.Increment(count)
			Next
			Return a
		End Function

		Protected Overrides Sub Draw(ByVal data As DrawParams)
			MyBase.Draw(data)

			DrawSelectedSubItems(data)
		End Sub

		Protected Overrides Sub DrawHiddenLines(ByVal data As DrawParams)
			MyBase.DrawHiddenLines(data)

			DrawSelectedSubItems(data)
		End Sub

		Protected Overrides Sub DrawFlat(ByVal data As DrawParams)
			MyBase.DrawFlat(data)

			DrawSelectedSubItems(data)
		End Sub

		Protected Overrides Sub Render(ByVal data As RenderParams)
			Draw(data)
		End Sub

		Protected Overrides Sub DrawWireframe(ByVal data As DrawParams)
			If Edges IsNot Nothing AndAlso Edges.Length > 0 Then
			data.RenderContext.Draw(drawEdgesData)
			End If

			If SelectedSubItems.Count > 0 Then
				data.RenderContext.PushDepthStencilState()
				data.RenderContext.SetState(depthStencilStateType.DepthTestLessEqual)
				data.RenderContext.SetColorWireframe(System.Drawing.Color.Yellow)

				data.RenderContext.Draw(drawSelectedEdgesData)

				data.RenderContext.PopDepthStencilState()
			End If
		End Sub

		Protected Overrides Function IsCrossing(ByVal data As FrustumParams) As Boolean
			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			Dim res As Boolean = InsideOrCrossingFrustum(data)

			If data.DisplayMode <> displayType.Wireframe AndAlso ThroughTriangle(data) Then
				res = True
			End If

			UpdateCompileSelection()

			Return res
		End Function

		Protected Overrides Function InsideOrCrossingFrustum(ByVal data As FrustumParams) As Boolean
			' Computes the triangles that are inside or crossing the selection planes

			Dim insideOrCrossing As Boolean = False

			For i As Integer = 0 To Triangles.Length - 1
				Dim verts = GetTriangleVertices(Triangles(i))
				If Utility.InsideOrCrossingFrustum(verts(0), verts(1), verts(2), data.Frustum) Then
					SelectedSubItems.Add(i)

					insideOrCrossing = True

					'if selection filter is ByPick/VisibleByPick selects only the first triangle
					If vp.firstOnlyInternal AndAlso Not vp.processVisibleOnly Then
						Return True
					End If
				End If
			Next 

			Return insideOrCrossing
		End Function

		Protected Overrides Function ThroughTriangle(ByVal data As FrustumParams) As Boolean
			SelectedSubItems.Sort()

			'if selection filter is ByPick/VisibleByPick selects only the first triangle
			If vp.firstOnlyInternal AndAlso Not vp.processVisibleOnly AndAlso SelectedSubItems.Count > 0 Then
				Return False
			End If

			Dim through As Boolean = False

			For i As Integer = 0 To Triangles.Length - 1
				If SelectedSubItems.BinarySearch(i) >= 0 Then
					Continue For
				End If

				If ThroughTriangle(data, GetTriangleVertices(Triangles(i))) Then
					SelectedSubItems.Add(i)

					through = True

					If vp.firstOnlyInternal AndAlso Not vp.processVisibleOnly Then
						Return True
					End If
				End If
			Next 

			Return through
		End Function

		' Gets the list of the vertices of the triangle
		Private Function GetTriangleVertices(ByVal tri As IndexTriangle) As Point3D()
			Return New Point3D() {Vertices(tri.V1), Vertices(tri.V2), Vertices(tri.V3)}
		End Function

		Private Overloads Function ThroughTriangle(ByVal data As FrustumParams, ByVal vertices() As Point3D) As Boolean
			Dim transform As Transformation = data.Transformation

			If transform Is Nothing Then
				If FrustumEdgesTriangleIntersection(data.SelectionEdges, vertices(0), vertices(1), vertices(2)) Then

					Return True
				End If
			Else
				If FrustumEdgesTriangleIntersection(data.SelectionEdges, transform* vertices(0), transform* vertices(1), transform* vertices(2)) Then

					Return True
				End If
			End If

			Return False
		End Function

		Protected Overrides Function IsCrossingScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			' Computes the triangles that are crossing the screen polygon

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			Dim res As Boolean = MyBase.IsCrossingScreenPolygon(data)

			UpdateCompileSelection()

			Return res
		End Function

		Private Sub UpdateCompileSelection()
			needsCompileSelected = SelectedSubItems.Count > 0
		End Sub

		Public needsCompileSelected As Boolean

		Protected Overrides Function InsideOrCrossingScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			' Computes the triangles that are inside or crossing the screen polygon

			For i As Integer = 0 To Triangles.Length - 1
				Dim verts = GetTriangleVertices(Triangles(i))

				If UtilityEx.InsideOrCrossingScreenPolygon(verts(0), verts(1), verts(2), data) Then
					SelectedSubItems.Add(i)
				End If
			Next 

			Return False
		End Function

		Protected Overrides Function ThroughTriangleScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			SelectedSubItems.Sort()

			For i As Integer = 0 To Triangles.Length - 1
				If SelectedSubItems.BinarySearch(i) >= 0 Then
					Continue For
				End If

				Dim verts = GetTriangleVertices(Triangles(i))
				If ThroughTriangleScreenPolygon(verts(0), verts(1), verts(2), data) Then
					SelectedSubItems.Add(i)
				End If
			Next
			Return False
		End Function

		Protected Overrides Function AllVerticesInFrustum(ByVal data As FrustumParams) As Boolean
			' Computes the triangles that are completely enclosed to the selection rectangle

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			For i As Integer = 0 To Triangles.Length - 1
				Dim tri As IndexTriangle = Triangles(i)

				If Camera.IsInFrustum(Vertices(tri.V1), data.Frustum) AndAlso Camera.IsInFrustum(Vertices(tri.V2), data.Frustum) AndAlso Camera.IsInFrustum(Vertices(tri.V3), data.Frustum) Then
					SelectedSubItems.Add(i)
				End If
			Next 

			UpdateCompileSelection()
			Return False
		End Function

		Protected Overrides Function AllVerticesInScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			' Computes the triangles that are completely enclosed to the screen polygon

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			For i As Integer = 0 To Triangles.Length - 1
				Dim verts = GetTriangleVertices(Triangles(i))

				If UtilityEx.AllVerticesInScreenPolygon(data, verts, 3) Then
					SelectedSubItems.Add(i)
				End If
			Next 

			UpdateCompileSelection()

			Return False
		End Function

		Public Sub SelectSubItems(ByVal indices() As Integer) Implements ISelect.SelectSubItems
			' sets as selected all the triangles in the indices array
			SelectedSubItems = New List(Of Integer)(indices)
			UpdateCompileSelection()
		End Sub
	End Class
