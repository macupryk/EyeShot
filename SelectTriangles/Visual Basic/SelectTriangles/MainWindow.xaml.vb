Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports Color = System.Drawing.Color
Imports Environment = devDept.Eyeshot.Environment

	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Public Sub New()
			InitializeComponent()

			' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

			' sets selection  as "ByPick"
			selectionComboBox.SelectedIndex = 0

			AddHandler model1.SelectionChanged, AddressOf Model1_SelectionChanged
			AddHandler model1.PreviewMouseDown, AddressOf Model1_MouseDown

			model1.HiddenLines.ColorMethod = hiddenLinesColorMethodType.EntityColor

			model1.GetOriginSymbol().Visible = False
		End Sub

		Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)
			tabControl1.SelectedIndex = 0
			MeshSpheres()

			model1.Camera.ProjectionMode = projectionType.Orthographic
			model1.SetView(viewType.Dimetric, True, False)
			model1.Invalidate()

			MyBase.OnContentRendered(e)
		End Sub

		Private Sub Model1_MouseDown(ByVal sender As Object, ByVal mouseEventArgs As MouseButtonEventArgs)
			' clears the previous selection

			If mouseEventArgs.LeftButton = MouseButtonState.Pressed Then
				For Each entity As Entity In model1.Entities
					If TypeOf entity Is ISelect Then
						DirectCast(entity, ISelect).SelectedSubItems = New List(Of Integer)(0)
					End If
				Next entity
			End If
		End Sub

		Private Sub Model1_SelectionChanged(ByVal sender As Object, ByVal e As Model.SelectionChangedEventArgs)
			For i As Integer = 0 To model1.Entities.Count - 1
				Dim ent = model1.Entities(i)
				If TypeOf ent Is MyMesh Then
					Dim m = (CType(ent, MyMesh))
					If m.needsCompileSelected Then

						m.CompileSelected(model1.renderContext)
					End If
				End If
			Next i
		End Sub
		Private Sub wireframeButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.DisplayMode = displayType.Wireframe
			model1.Invalidate()
		End Sub

		Private Sub shadedButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.DisplayMode = displayType.Shaded
			model1.Invalidate()
		End Sub

		Private Sub renderedButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.DisplayMode = displayType.Rendered
			model1.Invalidate()
		End Sub

		Private Sub hiddenLinesButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.DisplayMode = displayType.HiddenLines
			model1.Invalidate()
		End Sub
		Private Sub flatButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.DisplayMode = displayType.Flat
			model1.Invalidate()
		End Sub

		Private Sub selectionComboBox_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			If selectCheckBox Is Nothing Then
				Return
			End If

			If selectCheckBox.IsChecked IsNot Nothing AndAlso selectCheckBox.IsChecked.Value Then

				Selection()

			Else

				model1.ActionMode = actionType.None
			End If
		End Sub

		Private Sub selectCheckBox_Click(ByVal sender As Object, ByVal e As EventArgs)
			If selectCheckBox.IsChecked IsNot Nothing AndAlso selectCheckBox.IsChecked.Value Then

				Selection()

			Else

				model1.ActionMode = actionType.None
			End If

		End Sub

		Private Sub Selection()
			Select Case selectionComboBox.SelectedIndex
				Case 0 
				    ' by pick
					model1.ActionMode = actionType.SelectByPick
					' selects only the first triangle 
					model1.firstOnlyInternal = True
				    Exit Select

				Case 1 
				    ' by box
					model1.ActionMode = actionType.SelectByBox
					model1.firstOnlyInternal = False
				    Exit Select

				Case 2 
				    ' by poly
					model1.ActionMode = actionType.SelectByPolygon
					model1.firstOnlyInternal = False
				    Exit Select

				Case 3 
				    ' by box enclosed
					model1.ActionMode = actionType.SelectByBoxEnclosed
					model1.firstOnlyInternal = False
				    Exit Select

				Case 4 
				    ' by poly enclosed
					model1.ActionMode = actionType.SelectByPolygonEnclosed
					model1.firstOnlyInternal = False
				    Exit Select

				Case 5 
				    ' visible by pick
					model1.ActionMode = actionType.SelectVisibleByPick
					' selects only the first triangle 
					model1.firstOnlyInternal = True
				    Exit Select

				Case 6 
				    ' visible by box
					model1.ActionMode = actionType.SelectVisibleByBox
					model1.firstOnlyInternal = False
				    Exit Select

				Case 7 
				    ' visible by poly
					model1.ActionMode = actionType.SelectVisibleByPolygon
					model1.firstOnlyInternal = False
				    Exit Select

				Case Else

					model1.ActionMode = actionType.None
				    Exit Select
			End Select
		End Sub
		 Private Sub tabControl1_OnSelectionChanged(ByVal sender As Object, ByVal e As EventArgs)
			' every time the selected tab changes ...
			If Not model1.IsLoaded Then
				Return
			End If

			' reset all actions
			model1.Focus()
			model1.Entities.Clear()

			Select Case (CType(tabControl1.SelectedItem, TabItem)).Header.ToString()
				Case "Triangles"
					MeshSpheres()
				Case "Lines"
					LinearPathSpheres()
			End Select
			model1.SetView(viewType.Dimetric, True, False)
			model1.Invalidate()
		 End Sub

		Private Sub MeshSpheres()
			' creates the entities
			Dim m1 As New MyMesh(model1, Mesh.CreateSphere(10, 10, 10))
			Dim m2 As New MyMesh(model1, Mesh.CreateSphere(10, 10, 10))

			m2.Translate(25, 0, 0)

			' Adds entities to the scene
			model1.Entities.Add(m1, Color.FromArgb(255, Color.Green))
			model1.Entities.Add(m2, Color.FromArgb(127, Color.Red))
		End Sub

		Private Sub LinearPathSpheres()
			' creates the entities
			Dim slices As Integer = 20
			Dim stacks As Integer = 10
			Dim m2 As New MyLinearPath(model1, New LinearPath(Mesh.CreateSphere(10, slices, stacks).Vertices))

			'computes group of lines
			Dim edges((slices* (stacks -1)) - 1) As IndexLine
			Dim i As Integer = 0
			Do While i < (stacks-1)
				Dim j As Integer = 0
				Do While j < (slices-1)
					Dim v1 As Integer = i*slices + j
					edges(v1) = New IndexLine(v1, v1+ 1)
					j += 1
				Loop
				edges(i*slices +(slices-1)) = New IndexLine(i*slices + (slices-1), i*slices)
				i += 1
			Loop
			m2.Lines = edges

			Dim m1 As MyLinearPath = DirectCast(m2.Clone(), MyLinearPath)
			m2.Translate(25,0, 0)
			m2.LineWeight = 4

			model1.Entities.Add(m1, Color.FromArgb(255, Color.Green))
			model1.Entities.Add(m2, Color.FromArgb(155, Color.Red))
		End Sub

	End Class