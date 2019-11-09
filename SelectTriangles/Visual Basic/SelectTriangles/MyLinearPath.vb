Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports OpenGL

	Public Class MyLinearPath
		Inherits LinearPath
		Implements ISelect

		Private vp As MyModel
		Public Lines() As IndexLine
		Public Sub New(ByVal vp As MyModel, ByVal other As LinearPath)
			MyBase.New(other)
			Me.vp = vp
		End Sub

		Public Sub New(ByVal another As MyLinearPath)
			MyBase.New(another)
			vp = another.vp

			Lines = New IndexLine(another.Lines.Length - 1){}
			Array.Copy(another.Lines, Lines, Lines.Length)
		End Sub

		Public Overrides Function Clone() As Object
			Return New MyLinearPath(Me)
		End Function
    
		Public _selectedSubItems As New List(Of Integer)()

		' Gets or sets the list of selected lines 
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
			Draw(data)
		End Sub

		Public Property DrawSubItemsForSelection() As Boolean Implements ISelect.DrawSubItemsForSelection

		Protected Overrides Sub DrawForSelection(ByVal data As GfxDrawForSelectionParams)
			If DrawSubItemsForSelection Then
				Dim prev = vp.SuspendSetColorForSelection
				vp.SuspendSetColorForSelection = False

				' Draws the lines with the color-coding needed for visibility computation 
				For index As Integer = 0 To SelectedSubItems.Count - 1
					Dim p1 As Point3D = Vertices(Lines(SelectedSubItems(index)).V1)
					Dim p2 As Point3D = Vertices(Lines(SelectedSubItems(index)).V2)


					vp.SetColorDrawForSelection(SelectedSubItems(index))

					Dim tri As IndexLine = Lines(SelectedSubItems(index))
					data.RenderContext.DrawLine(p1, p2)
				Next index

				vp.SuspendSetColorForSelection = prev

				' reset the color to avoid issues with the entities drawn after this one
				data.RenderContext.SetColorWireframe(System.Drawing.Color.White)

			Else
				data.RenderContext.DrawIndexLines(Lines, Vertices)

			End If
		End Sub

		Private Sub DrawSelectedSubItems(ByVal data As DrawParams)
			' Draws the selected lines over the other lines 

			If SelectedSubItems.Count = 0 Then
				Return
			End If

			Dim popState As Boolean = False
			Dim alpha As Integer = 255


			If data.RenderContext.CurrentWireColor.A = 255 Then
				data.RenderContext.PushDepthStencilState()
				data.RenderContext.SetState(depthStencilStateType.DepthTestEqual)
				popState = True
			ElseIf data.RenderContext.LightingEnabled() Then
				data.RenderContext.PushBlendState()
				data.RenderContext.SetState(blendStateType.Blend)
				alpha = data.RenderContext.CurrentMaterial.Diffuse.A
			Else
				data.RenderContext.PushBlendState()
				data.RenderContext.SetState(blendStateType.Blend)
				alpha = data.RenderContext.CurrentWireColor.A
			End If

			If vp.UseShaders Then
				data.RenderContext.PushShader()

				data.RenderContext.SetShader(shaderType.NoLightsThickLines)
			End If

			data.RenderContext.SetLineSize(LineWeight)

			Dim prevCol = data.RenderContext.CurrentWireColor
			data.RenderContext.SetColorWireframe(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Yellow))


			' draws lines
			Dim seLines As New List(Of IndexLine)(SelectedSubItems.Count)
			For i As Integer = 0 To SelectedSubItems.Count - 1
				seLines.Add(Lines(SelectedSubItems(i)))
			Next i
			data.RenderContext.DrawIndexLines(seLines, Vertices)

			' restores previous settings
			If vp.UseShaders Then
				data.RenderContext.PopShader()
			End If
			data.RenderContext.SetColorWireframe(prevCol)
			data.RenderContext.SetLineSize(1)

			If popState Then
				data.RenderContext.PopDepthStencilState()
			Else
				data.RenderContext.PopBlendState()
			End If
		End Sub

		Protected Overrides Sub Draw(ByVal data As DrawParams)
			data.RenderContext.SetLineSize(LineWeight)

			If Color.A <> 255 Then
				' draws only non-selected transparent lines to avoid blended color
				Dim linesToDraw As List(Of IndexLine) = Lines.ToList()
				SelectedSubItems.Sort()
				For i As Integer = SelectedSubItems.Count - 1 To 0 Step -1
					linesToDraw.RemoveAt(SelectedSubItems(i))
				Next i

				data.RenderContext.DrawIndexLines(linesToDraw, Vertices)
			Else
				data.RenderContext.DrawIndexLines(Lines, Vertices)
			End If
			data.RenderContext.SetLineSize(1)

			DrawSelectedSubItems(data)
		End Sub

		Protected Overrides Sub DrawSelected(ByVal data As DrawParams)
			Draw(data)
		End Sub
		Protected Overrides Sub DrawHiddenLines(ByVal data As DrawParams)
			Draw(data)
		End Sub

		Protected Overrides Sub DrawFlat(ByVal data As DrawParams)
			Draw(data)
		End Sub

		Protected Overrides Sub Render(ByVal data As RenderParams)
			Draw(data)
		End Sub

		Protected Overrides Sub DrawWireframe(ByVal data As DrawParams)
			Draw(data)
		End Sub

		Protected Overrides Function IsCrossing(ByVal data As FrustumParams) As Boolean
			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			Dim res As Boolean = InsideOrCrossingFrustum(data)

			Return res
		End Function

		Protected Overrides Function InsideOrCrossingFrustum(ByVal data As FrustumParams) As Boolean
			' Computes the lines that are inside or crossing the selection planes

			Dim insideOrCrossing As Boolean = False

			For i As Integer = 0 To Lines.Length - 1
				If Utility.IsSegmentInsideOrCrossing(data.Frustum, New Segment3D(Vertices(Lines(i).V1), Vertices(Lines(i).V2))) Then
					SelectedSubItems.Add(i)

					insideOrCrossing = True

					'if selection filter is ByPick/VisibleByPick selects only the first line
					If vp.firstOnlyInternal AndAlso Not vp.processVisibleOnly Then
						Return True
					End If
				End If
			Next i

			Return insideOrCrossing
		End Function


		Protected Overrides Function IsCrossingScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			Dim res As Boolean = MyBase.IsCrossingScreenPolygon(data)

			Return res
		End Function

		Protected Overrides Function InsideOrCrossingScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			' Computes the lines that are inside or crossing the screen polygon
			For i As Integer = 0 To Lines.Length - 1
				Dim seg As Segment2D

				Dim line As IndexLine = Lines(i)
				Dim pt1 As Point3D = Vertices(line.V1)
				Dim pt2 As Point3D = Vertices(line.V2)

				Dim screenP1 As Point3D = vp.Camera.WorldToScreen(pt1, data.ViewFrame)
				Dim screenP2 As Point3D = vp.Camera.WorldToScreen(pt2, data.ViewFrame)

				If screenP1.Z > 1 OrElse screenP2.Z > 1 Then
					Return False ' for perspective
				End If

				seg = New Segment2D(screenP1, screenP2)

				If UtilityEx.PointInPolygon(screenP1, data.ScreenPolygon) OrElse UtilityEx.PointInPolygon(screenP2, data.ScreenPolygon) Then

					SelectedSubItems.Add(i)
					Continue For
				End If

				For j As Integer = 0 To data.ScreenSegments.Count - 1
					Dim i0 As Point2D = Nothing
					If Segment2D.Intersection(data.ScreenSegments(j), seg, i0) Then

						SelectedSubItems.Add(i)
						Exit For
					End If
				Next j
			Next i

			Return False
		End Function

		Protected Overrides Function AllVerticesInFrustum(ByVal data As FrustumParams) As Boolean
			' Computes the lines that are completely enclosed to the selection rectangle

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			For i As Integer = 0 To Lines.Length - 1
				Dim line As IndexLine = Lines(i)

				If Camera.IsInFrustum(Vertices(line.V1), data.Frustum) AndAlso Camera.IsInFrustum(Vertices(line.V2), data.Frustum) Then
					SelectedSubItems.Add(i)
				End If
			Next i

			Return False
		End Function

		Protected Overrides Function AllVerticesInScreenPolygon(ByVal data As ScreenPolygonParams) As Boolean
			' Computes the lines that are completely enclosed to the screen polygon

			If vp.processVisibleOnly AndAlso Not Selected Then
				Return False
			End If

			SelectedSubItems = New List(Of Integer)()

			For i As Integer = 0 To Lines.Length - 1
				Dim line As IndexLine = Lines(i)

				If UtilityEx.AllVerticesInScreenPolygon(data, New List(Of Point3D)() From {Vertices(line.V1), Vertices(line.V2)}, 2) Then
					SelectedSubItems.Add(i)
				End If
			Next i

			Return False
		End Function

		Public Sub SelectSubItems(ByVal indices() As Integer) Implements ISelect.SelectSubItems
			' sets as selected all the lines in the indices array
			SelectedSubItems = New List(Of Integer)(indices)
		End Sub
	End Class
