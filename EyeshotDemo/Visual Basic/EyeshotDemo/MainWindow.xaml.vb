Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Linq
Imports System.Security.AccessControl
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.RoutedEventArgs
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Labels
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports Microsoft.Win32
Imports Cursors = System.Windows.Input.Cursors
Imports MouseButton = System.Windows.Input.MouseButton
Imports devDept.Eyeshot.Triangulation
Imports devDept.Eyeshot.Translators
Imports devDept.Serialization
Imports devDept.CustomControls
Imports Environment = devDept.Eyeshot.Environment

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow

    Private _doingTransform As Boolean = False
    Private _yAxisUp As Boolean = False

    Public Property Layers() As ObservableCollection(Of ListViewModelItem)
        Get
            Return m_Layers
        End Get
        Set(value As ObservableCollection(Of ListViewModelItem))
            m_Layers = value
        End Set
    End Property
    Private m_Layers As ObservableCollection(Of ListViewModelItem)

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.  

        ' Add any initialization after the InitializeComponent() call.            

#If Not PROFESSIONAL Then
        tabControl1.Items.Remove(primitivesTabPage)
        tabControl1.Items.Remove(triangulationTabPage)
        tabControl1.Items.Remove(meshingTabPage)
        tabControl1.Items.Remove(motherBoardTabPage)
        tabControl1.Items.Remove(bunnyTabPage)
        tabControl1.Items.Remove(pocketTabPage)
        tabControl1.Items.Remove(locomotiveTabPage)
        tabControl1.Items.Remove(bracketTabPage)
        tabControl1.Items.Remove(flangeTabPage)

        magGlassCheckBox.IsEnabled = False
#End If

#If Not NURBS Then
        tabControl1.Items.Remove(toolpathTabPage)
        tabControl1.Items.Remove(hairDryerTabPage)
        tabControl1.Items.Remove(bracketTabPage)
        tabControl1.Items.Remove(flangeTabPage)
#End If

#If Not ULTIMATE Then

#End If

#If Not SOLID Then
        tabControl1.Items.Remove(medalTabPage)
        tabControl1.Items.Remove(houseTabPage)
#End If

        tabletabControl.FocusProperties(Nothing)
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)

        model1.MagnifyingGlass.Factor = 3
        model1.MagnifyingGlass.Size = new System.Drawing.Size(200, 200)

        tabControl1.SelectedIndex = 1

        rendererVersionStatusLabel.Text = model1.RendererVersion.ToString()

        ' enables FastZPR when the scene exceeds 3000 objects
        model1.Entities.MaxComplexity = 3000
        _maxComplexity = model1.Entities.MaxComplexity

        model1.Invalidate()
        tableTabControl.Environment = model1

        MyBase.OnContentRendered(e)

    End Sub

    Private Sub tabControl1_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        If model1.IsLoaded = False Then Return

        ' every time the selected tab changes ...
        model1.ActionMode = actionType.None
        ' reset all actions
        model1.Focus()

        perspectiveButton.IsChecked = True
        ' set default projection to perspective
        selectionComboBox.SelectedIndex = 5
        ' set default selection to VisibleByPick
        selectionFilterComboBox.SelectedIndex = 3
        ' set default selection filter to Entity
        selectButton.IsChecked = False
        ' disable selection mode
        model1.StopAnimation()
        ' stop any animation
        model1.Clear()
        ' clear model (entities, blocks, layers, materials, etc.)
        tabletabControl.FocusProperties(Nothing)
        ' clear propertyGrid contents
        If model1.GetLegends().Count > 0 Then
            model1.GetLegends()(0).Visible = False
        End If

        model1.GetGrid().Visible = True
        model1.GetGrid().[Step] = 10


        model1.HiddenLines.Lighting = False
        model1.HiddenLines.ColorMethod = hiddenLinesColorMethodType.SingleColor
        model1.HiddenLines.DashedHiddenLines = False

        model1.AutoHideLabels = True
        model1.DisplayMode = displayType.Rendered

        Select Case DirectCast(tabControl1.SelectedItem, TabItem).Header.ToString()
#If SOLID Then
            Case "Medal"
                Draw.Medal(model1)
                Exit Select
            Case "House"
                model1.HiddenLines.DashedHiddenLines = True
                model1.HiddenLines.DashedHiddenLinesColor = System.Drawing.Color.FromArgb(200, 200, 200)
                Draw.House(model1)
                Exit Select
#End If
            Case "Motherboard"
                Draw.MotherBoard(model1)
                Exit Select
            Case "Locomotive"
                Draw.Locomotive(model1)
                Exit Select
            Case "Bunny"
                Draw.Bunny(model1)
                Exit Select
            Case "Pocket 2.5x"
                Draw.Pocket(model1)
                Exit Select
            Case "Primitives"
                Draw.Primitives(model1)
                Exit Select
#If NURBS Then
            Case "Hair dryer"
                Draw.HairDryer(model1)
                Exit Select
            Case "Toolpath"
                Draw.Toolpath(model1)
                Exit Select
            Case "Bracket"
                Draw.Bracket(model1)
                Exit Select
            Case "Flange"
                Draw.Flange(model1)
                Exit Select
#End If
            Case "Jet"
                Draw.Jet(model1)
                Exit Select

        End Select


        If model1.IsBusy Then
            tabControl1.IsEnabled = False
            importButton.IsEnabled = False
            openButton.IsEnabled = False
            saveButton.IsEnabled = False
        End If

        If model1.IsLoaded Then
            ' Sets trimetric view and fits the model in the main viewport
            model1.SetView(viewType.Trimetric, True, model1.AnimateCamera)

            ' Refresh the model
            model1.Invalidate()

            tableTabControl.Sync()
            UpdateDisplayModeButtons()
        End If

    End Sub
    

#Region "DisplayMode"
    Private Sub UpdateDisplayModeButtons()
        ' syncs the shading buttons with the current display mode.
        Select Case model1.DisplayMode
            Case displayType.Wireframe
                wireframeButton.IsChecked = True
                SetDisplayModeButtonsChecked(wireframeButton)
                Exit Select
            Case displayType.Shaded
                shadedButton.IsChecked = True
                SetDisplayModeButtonsChecked(shadedButton)
                Exit Select
            Case displayType.Rendered
                renderedButton.IsChecked = True
                SetDisplayModeButtonsChecked(renderedButton)
                Exit Select
            Case displayType.Flat
                flatButton.IsChecked = True
                SetDisplayModeButtonsChecked(flatButton)
                Exit Select
            Case displayType.HiddenLines
                hiddenLinesButton.IsChecked = True
                SetDisplayModeButtonsChecked(hiddenLinesButton)
                Exit Select
        End Select
    End Sub
    Private Sub SetDisplayModeButtonsChecked(checkedButton As ToggleButton)
        If Not checkedButton.Equals(wireframeButton) Then
            wireframeButton.IsChecked = Not checkedButton.IsChecked
        End If
        If Not checkedButton.Equals(shadedButton) Then
            shadedButton.IsChecked = Not checkedButton.IsChecked
        End If
        If Not checkedButton.Equals(renderedButton) Then
            renderedButton.IsChecked = Not checkedButton.IsChecked
        End If
        If Not checkedButton.Equals(hiddenLinesButton) Then
            hiddenLinesButton.IsChecked = Not checkedButton.IsChecked
        End If
        If Not checkedButton.Equals(flatButton) Then
            flatButton.IsChecked = Not checkedButton.IsChecked
        End If
    End Sub

    Private Sub wireframeButton_OnClick(sender As Object, e As RoutedEventArgs)
        SetDisplayModeButtonsChecked(DirectCast(sender, ToggleButton))
        SetDisplayMode(model1, displayType.Wireframe)
    End Sub

    Private Sub shadedButton_OnClick(sender As Object, e As RoutedEventArgs)
        SetDisplayModeButtonsChecked(DirectCast(sender, ToggleButton))
        SetDisplayMode(model1, displayType.Shaded)
    End Sub

    Private Sub renderedButton_OnClick(sender As Object, e As RoutedEventArgs)
        SetDisplayModeButtonsChecked(DirectCast(sender, ToggleButton))
        SetDisplayMode(model1, displayType.Rendered)
    End Sub

    Private Sub hiddenLinesButton_OnClick(sender As Object, e As RoutedEventArgs)
        SetDisplayModeButtonsChecked(DirectCast(sender, ToggleButton))
        SetDisplayMode(model1, displayType.HiddenLines)
    End Sub

    Private Sub flatButton_OnClick(sender As Object, e As RoutedEventArgs)
        SetDisplayModeButtonsChecked(DirectCast(sender, ToggleButton))
        SetDisplayMode(model1, displayType.Flat)
    End Sub

    Private Sub showCurveDirectionButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ShowCurveDirection = showCurveDirectionButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Public Shared Sub SetDisplayMode(model As Model, displayType As displayType)
        model.DisplayMode = displayType
        SetBackgroundStyleAndColor(model)
        model.Entities.UpdateBoundingBox() ' Updates simplified representation (when available)
        model.Invalidate()
    End Sub

    Public Shared Sub SetBackgroundStyleAndColor(ByVal model As Model)

        model.GetCoordinateSystemIcon().Lighting = False
        model.GetViewCubeIcon().Lighting = False

        Select Case model.DisplayMode
            Case displayType.HiddenLines
                model.GetBackground().TopColor = RenderContextUtility.ConvertColor(System.Drawing.Color.FromArgb(&HD2, &HD0, &HB9))

                model.GetCoordinateSystemIcon().Lighting = True
                model.GetViewCubeIcon().Lighting = True

                Exit Select
            Case Else
                model.GetBackground().TopColor = RenderContextUtility.ConvertColor(System.Drawing.Color.FromArgb(&HED, &HED, &HED))
                Exit Select
        End Select

        model.CompileUserInterfaceElements()

    End Sub

    Private Sub cullingButton_OnClick(sender As Object, e As RoutedEventArgs)
        If cullingButton.IsChecked.Value Then
            model1.Backface.ColorMethod = backfaceColorMethodType.Cull
        Else
            model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor
        End If

        model1.Invalidate()
    End Sub
#End Region

#Region "ZPR"
    Private Sub zoomButton_OnClick(sender As Object, e As RoutedEventArgs)

        model1.ActionMode = actionType.None

        If zoomButton.IsChecked.Value Then

            model1.ActionMode = actionType.Zoom
        End If

        panButton.IsChecked = False
        rotateButton.IsChecked = False
        zoomWindowButton.IsChecked = False
        selectButton.IsChecked = False
    End Sub

    Private Sub panButton_OnClick(sender As Object, e As EventArgs)

        model1.ActionMode = actionType.None

        If panButton.IsChecked.Value Then

            model1.ActionMode = actionType.Pan
        End If

        zoomButton.IsChecked = False
        rotateButton.IsChecked = False
        zoomWindowButton.IsChecked = False
        selectButton.IsChecked = False
    End Sub

    Private Sub rotateButton_OnClick(sender As Object, e As EventArgs)
        model1.ActionMode = actionType.None

        If rotateButton.IsChecked.Value Then

            model1.ActionMode = actionType.Rotate
        End If

        zoomButton.IsChecked = False
        panButton.IsChecked = False
        zoomWindowButton.IsChecked = False
        selectButton.IsChecked = False
    End Sub

    Private Sub zoomFitButton_OnClick(sender As Object, e As EventArgs)
        model1.ZoomFit()
        model1.Invalidate()
    End Sub

    Private Sub zoomWindowButton_OnClick(sender As Object, e As EventArgs)
        model1.ActionMode = actionType.None

        If zoomWindowButton.IsChecked.Value Then

            model1.ActionMode = actionType.ZoomWindow
        End If

        zoomButton.IsChecked = False
        panButton.IsChecked = False
        rotateButton.IsChecked = False
        selectButton.IsChecked = False
    End Sub

#End Region

#Region "Projection"
    Private Sub parallelButton_OnClick(sender As Object, e As RoutedEventArgs)
        perspectiveButton.IsChecked = Not parallelButton.IsChecked

        model1.Camera.ProjectionMode = projectionType.Orthographic

        model1.AdjustNearAndFarPlanes()

        model1.Invalidate()
    End Sub

    Private Sub perspectiveButton_OnClick(sender As Object, e As RoutedEventArgs)
        parallelButton.IsChecked = Not perspectiveButton.IsChecked

        model1.Camera.ProjectionMode = projectionType.Perspective

        model1.AdjustNearAndFarPlanes()

        model1.Invalidate()
    End Sub
#End Region

#Region "Zoom/Pan/Rotate"

    Private RotateToFace As Boolean = False

    Private Sub rotateToFaceButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ActionMode = actionType.None

        RotateToFace = False

        If rotateToFaceButton.IsChecked.Value Then
            RotateToFace = True
            model1.Cursor = Cursors.Hand
        Else
            RotateToFace = False
            model1.Cursor = Nothing
        End If
    End Sub

#End Region

#Region "Zoom"
    Private Sub zoomSelectionButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ZoomFit(True)
        model1.Invalidate()
    End Sub
#End Region

#Region "View"

    Private Sub isoViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.SetView(viewType.Isometric, True, model1.AnimateCamera)
        model1.Invalidate()
    End Sub

    Private Sub frontViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.SetView(viewType.Front, True, model1.AnimateCamera)
        model1.Invalidate()
    End Sub

    Private Sub sideViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.SetView(viewType.Right, True, model1.AnimateCamera)
        model1.Invalidate()
    End Sub

    Private Sub topViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.SetView(viewType.Top, True, model1.AnimateCamera)
        model1.Invalidate()
    End Sub

    Private Sub prevViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.PreviousView()
        model1.Invalidate()
    End Sub

    Private Sub nextViewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.NextView()
        model1.Invalidate()
    End Sub

    Private Sub animateCameraCheckBox_OnClick(sender As Object, e As RoutedEventArgs)
        model1.AnimateCamera = animateCameraCheckBox.IsChecked.Value
    End Sub
#End Region

#Region "Hide/Show"

    Private Sub showOriginButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.GetOriginSymbol().Visible = showOriginButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showExtentsButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.BoundingBox.Visible = showExtentsButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showVerticesButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ShowVertices = showVerticesButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showGridButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.GetGrid().Visible = showGridButton.IsChecked.Value
        model1.Invalidate()
    End Sub
#End Region

#Region "Selection"
    Private Sub selectionComboBox_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If selectButton Is Nothing Then
            Return
        End If

        groupButton.IsEnabled = True

        If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
            Selection()
        End If
    End Sub

    Private Sub selectCheckBox_OnClick(sender As Object, e As RoutedEventArgs)
        groupButton.IsEnabled = True

        If selectButton.IsChecked.HasValue AndAlso selectButton.IsChecked.Value Then
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
                Exit Select

            Case 1
                ' by box
                model1.ActionMode = actionType.SelectByBox
                Exit Select

            Case 2
                ' by poly
                model1.ActionMode = actionType.SelectByPolygon
                Exit Select

            Case 3
                ' by box enclosed
                model1.ActionMode = actionType.SelectByBoxEnclosed
                Exit Select

            Case 4
                ' by poly enclosed
                model1.ActionMode = actionType.SelectByPolygonEnclosed
                Exit Select

            Case 5
                ' visible by pick
                model1.ActionMode = actionType.SelectVisibleByPick
                Exit Select

            Case 6
                ' visible by box
                model1.ActionMode = actionType.SelectVisibleByBox
                Exit Select

            Case 7
                ' visible by poly
                model1.ActionMode = actionType.SelectVisibleByPolygon
                Exit Select

            Case 8
                ' visible by pick dynamic
                model1.ActionMode = actionType.SelectVisibleByPickDynamic
                Exit Select

            Case 9
                ' visible by pick label
                model1.ActionMode = actionType.SelectVisibleByPickLabel
                groupButton.IsEnabled = False
                Exit Select
            Case Else

                model1.ActionMode = actionType.None
                Exit Select
        End Select
    End Sub

    Private Sub clearSelectionButton_OnClick(sender As Object, e As RoutedEventArgs)
        If model1.ActionMode = actionType.SelectVisibleByPickLabel Then

            model1.Viewports(0).Labels.ClearSelection()
        Else


            model1.Entities.ClearSelection()
        End If

        model1.Invalidate()
    End Sub

    Private Sub invertSelectionButton_OnClick(sender As Object, e As RoutedEventArgs)
        If model1.ActionMode = actionType.SelectVisibleByPickLabel Then

            model1.Viewports(0).Labels.InvertSelection()
        Else


            model1.Entities.InvertSelection()
        End If

        model1.Invalidate()
    End Sub

    Private Sub groupButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.GroupSelection()
    End Sub

    Private Sub selectionFilterComboBox_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        SelectionFilter()

    End Sub

    Private Sub SelectionFilter()
        Select Case selectionFilterComboBox.SelectedIndex
            Case 0
                model1.SelectionFilterMode = selectionFilterType.Vertex
                Exit Select

            Case 1
                model1.SelectionFilterMode = selectionFilterType.Edge
                Exit Select

            Case 2
                model1.SelectionFilterMode = selectionFilterType.Face
                Exit Select

            Case 3
                model1.SelectionFilterMode = selectionFilterType.Entity
                Exit Select
        End Select
    End Sub

    Private Sub setCurrentButton_OnClick(sender As Object, e As RoutedEventArgs)
        For index As Integer = 0 To model1.Entities.Count - 1

            Dim ent As Entity = model1.Entities(index)

            If ent.Selected AndAlso TypeOf ent Is BlockReference Then
                model1.Entities.SetCurrent(DirectCast(ent, BlockReference))
                model1.Invalidate()
            End If
        Next
    End Sub
    Private Sub clearCurrentButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.Entities.SetCurrent(Nothing)
        model1.Invalidate()
    End Sub

#End Region

#Region "Editing"

    Private Sub transformButton_OnClick(sender As Object, e As RoutedEventArgs)
        ' allows transformation for one entity at a time
        Dim temp As Entity = Nothing
        For Each ent As Entity In model1.Entities
            If ent.Selected Then
                temp = ent
                Exit For
            End If
        Next
        If temp IsNot Nothing Then
            _doingTransform = True
            Dim center As Transformation = New Translation(temp.BoxMin.X, temp.BoxMin.Y, temp.BoxMin.Z)
            model1.ObjectManipulator.Enable(center, False)
            model1.ObjectManipulator.ShowOriginalWhileEditing = False
            model1.Invalidate()
        Else
            Return
        End If
    End Sub

    Private Sub duplicateButton_OnClick(sender As Object, e As RoutedEventArgs)
        ' counts selected entities
        Dim count As Integer = 0

        For Each en As Entity In model1.Entities

            If en.Selected Then

                count += 1
            End If
        Next

        ' fills the duplicates array
        Dim duplicates As Entity() = New Entity(count - 1) {}

        count = 0

        For Each en As Entity In model1.Entities

            If en.Selected Then

                duplicates(count) = DirectCast(en.Clone(), Entity)

                en.Selected = False


                count += 1
            End If
        Next


        For Each dup As Entity In duplicates

            dup.Translate(50, 100, 50)


            model1.Entities.Add(dup)
        Next

        model1.Invalidate()
    End Sub

    Private Sub deleteButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.Entities.DeleteSelected()
        model1.Invalidate()
    End Sub

    Private Sub untrimButton_OnClick(sender As Object, e As RoutedEventArgs)
#If NURBS Then
        For Each en As Entity In model1.Entities

            If en.Selected Then

                If TypeOf en Is Surface Then

                    Dim s As Surface = TryCast(en, Surface)

                    s.Untrim()


                    model1.Entities.Regen()

                End If
            End If
        Next

        model1.Invalidate()
#End If
    End Sub

    Private Sub explodeButton_OnClick(sender As Object, e As RoutedEventArgs)
        For i As Integer = model1.Entities.Count - 1 To 0 Step -1

            Dim en As Entity = model1.Entities(i)

            If en.Selected Then
                If TypeOf en Is BlockReference Then

                    model1.Entities.RemoveAt(i)

                    Dim br As BlockReference = DirectCast(en, BlockReference)

                    Dim entList As Entity() = model1.Entities.Explode(br)


                    model1.Entities.AddRange(entList)
                ElseIf TypeOf en Is CompositeCurve Then

                    model1.Entities.RemoveAt(i)

                    Dim cc As CompositeCurve = DirectCast(en, CompositeCurve)

                    model1.Entities.AddRange(cc.Explode())
#If NURBS Then
                ElseIf TypeOf en Is Brep Then

                    model1.Entities.RemoveAt(i)

                    Dim sld As Brep = DirectCast(en, Brep)

                    model1.Entities.AddRange(sld.ConvertToSurfaces())
#End If
                ElseIf en.GroupIndex > -1 Then
                    model1.Ungroup(en.GroupIndex)
                End If
            End If
        Next

        model1.Invalidate()
    End Sub

#End Region

#Region "Inspection"

    Private inspectVertex As Boolean = False
    Private inspectMesh As Boolean = False

    Private Sub pickVertexButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ActionMode = actionType.None

        inspectVertex = False
        inspectMesh = False

        If pickVertexButton.IsChecked.HasValue AndAlso pickVertexButton.IsChecked.Value Then
            inspectVertex = True


            mainStatusLabel.Content = "Click on the entity to retrieve the 3D coordinates"
        Else
            mainStatusLabel.Content = ""
        End If

        pickFaceButton.IsChecked = False
    End Sub

    Private Sub pickFaceButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.ActionMode = actionType.None

        inspectVertex = False
        inspectMesh = False

        If pickFaceButton.IsChecked.HasValue AndAlso pickFaceButton.IsChecked.Value Then

            inspectMesh = True


            mainStatusLabel.Content = "Click on the face to retrieve the 3D coordinates"
        Else

            mainStatusLabel.Content = ""
        End If

        pickVertexButton.IsChecked = False
    End Sub

    Private Sub Model1_MouseDown(sender As Object, e As MouseButtonEventArgs)
        ' Checks that we are not using left mouse button for ZPR
        If model1.ActionMode = actionType.None AndAlso e.ChangedButton <> MouseButton.Middle Then

            Dim closest As Point3D = Nothing

            If inspectVertex Then

                If model1.FindClosestVertex(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), 50, closest) <> -1 Then

                    Dim copy As Point3D = DirectCast(closest.Clone(), Point3D)

                    model1.Labels.Add(New devDept.Eyeshot.Labels.LeaderAndText(copy, closest.ToString(), New System.Drawing.Font("Tahoma", 8.25F), Draw.Color, New Vector2D(0, 50)))

                End If
            ElseIf inspectMesh Then

                Dim item = model1.GetItemUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))

                If item IsNot Nothing Then
                    If TypeOf item.Item Is IFace Then
                        Dim pt As Point3D
                        Dim tri As Integer
                        If model1.FindClosestTriangle(item, RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), pt, tri) > 0 Then
                            ' adds a label with the point elevation
                            model1.Labels.Add(New devDept.Eyeshot.Labels.LeaderAndText(pt, pt.ToString(), New System.Drawing.Font("Tahoma", 8.25F), Draw.Color, New Vector2D(0, 50)))
                        End If
                    End If
                End If
            End If

            If RotateToFace Then

                Dim point As System.Drawing.Point = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))
                Dim index As Integer = model1.GetEntityUnderMouseCursor(point)
                If index <> -1 Then
                    RotateToFaceRecursive(model1.Entities(index), point)
                End If
            End If

            model1.Invalidate()
        End If

        If RotateToFace Then
            rotateToFaceButton.IsChecked = False
        End If

        If e.ChangedButton = System.Windows.Input.MouseButton.Right Then
            ' doing trasformation apply the objectManipulator changes
            If _doingTransform Then
                model1.ObjectManipulator.Apply()
                model1.ObjectManipulator.Cancel()
                model1.Entities.Regen()
                _doingTransform = False
            End If
        End If
    End Sub
    
    Private Sub RotateToFaceRecursive(entity As Entity, point As System.Drawing.Point)
        If TypeOf entity Is BlockReference Then
            Dim blockReference As BlockReference = CType(entity, BlockReference)
            model1.Entities.SetCurrent(blockReference)
            Dim index As Integer = model1.GetEntityUnderMouseCursor(point)
            RotateToFaceRecursive(model1.Entities(index), point)
        Else 
            ' rotates the view perpendicular to the plane under the mouse cursor
            model1.RotateCamera(point)
            model1.Entities.SetCurrent(Nothing)
        End If
    End Sub

    Private Sub dumpButton_OnClick(sender As Object, e As RoutedEventArgs)

        Dim entList As Entity() = model1.Entities.ToArray()

        For i As Integer = 0 To entList.Length - 1
            Dim ent As Entity = entList(i)

            Dim df As DetailsWindow

            Dim sb As New StringBuilder()
#If NURBS Then
            If TypeOf ent Is Brep Then
                Dim solid3D As Brep = DirectCast(ent, Brep)

                Select Case model1.SelectionFilterMode
                    Case selectionFilterType.Vertex
                        For j As Integer = 0 To solid3D.Vertices.Length - 1
                            Dim sv As Brep.Vertex = DirectCast(solid3D.Vertices(j), Brep.Vertex)

                            If solid3D.GetVertexSelection(j) Then
                                sb.AppendLine("Vertex ID: " & j)
                                sb.AppendLine(sv.ToString())
                                sb.AppendLine("----------------------")
                                sb.Append(sv.Dump())
                                Exit For
                            End If
                        Next
                        Exit Select

                    Case selectionFilterType.Edge
                        For j As Integer = 0 To solid3D.Edges.Length - 1
                            Dim se As Brep.Edge = solid3D.Edges(j)

                            If solid3D.GetEdgeSelection(j) Then
                                sb.AppendLine("Edge ID: " & j)
                                sb.AppendLine(se.ToString())
                                sb.AppendLine("----------------------")
                                sb.Append(se.Dump())
                                Exit For
                            End If
                        Next
                        Exit Select

                    Case selectionFilterType.Face

                        For j As Integer = 0 To solid3D.Faces.Length - 1
                            Dim sf As Brep.Face = solid3D.Faces(j)

                            If solid3D.GetFaceSelection(j) Then
                                sb.AppendLine("Face ID: " & j)
                                sb.AppendLine(sf.Surface.ToString())
                                sb.AppendLine("----------------------")
                                sb.Append(sf.Dump())
                                Exit For
                            End If
                        Next
                        Exit Select
                End Select

                If sb.Length > 0 Then
                    df = New DetailsWindow()

                    df.Title = "Dump"

                    df.contentTextBox.Text = sb.ToString()

                    df.Show()
                    Return
                End If
            End If

#End If
            If ent.Selected Then
                sb.AppendLine("Entity ID: " & i)

                sb.Append(ent.Dump())

                df = New DetailsWindow()

                df.Title = "Dump"

                df.contentTextBox.Text = sb.ToString()

                df.Show()

                Exit For
            End If
        Next

    End Sub

    Private Sub statisticsButton_OnClick(sender As Object, e As RoutedEventArgs)
        Dim rf As New DetailsWindow()

        rf.Title = "Statistics"

        rf.contentTextBox.Text = model1.Entities.GetStats(model1.Blocks, True)

        rf.Show()
    End Sub

    Private Function AddAreaProperty(ByVal ap As AreaProperties, ByVal ent As Entity, ByRef blockReferenceNotScaled As Boolean, ByVal Optional isParentSelected As Boolean = False) As Integer
        Dim count As Integer = 0
        blockReferenceNotScaled = True
        If ent.Selected OrElse isParentSelected Then
            If TypeOf ent Is IFace Then
                Dim itfFace As IFace = CType(ent, IFace)
                Dim meshes As Mesh() = itfFace.GetPolygonMeshes()
                For Each mesh As Mesh In meshes
                    ap.Add(mesh.Vertices, mesh.Triangles)
                Next

                count += 1
            ElseIf TypeOf ent Is BlockReference Then
                Dim br = CType(ent, BlockReference)
                If br.GetScaleFactorX() <> 1 AndAlso br.GetScaleFactorY() <> 1 AndAlso br.GetScaleFactorZ() <> 1 Then
                    blockReferenceNotScaled = False
                    Return count
                End If

                For Each e As Entity In br.GetEntities(model1.Blocks)
                    count += AddAreaProperty(ap, e, blockReferenceNotScaled, True)

                    If Not blockReferenceNotScaled Then
                        Return count
                    End If
                Next
            Else
                Dim itfCurve As ICurve = DirectCast(ent, ICurve)

                If itfCurve.IsClosed Then
                    ap.Add(ent.Vertices)
                End If
                count += 1
            End If
#If NURBS Then

        ElseIf TypeOf ent Is Brep Then

            Dim solid3D As Brep = CType(ent, Brep)

            For j As Integer = 0 To solid3D.Faces.Length - 1

                Dim sf As Brep.Face = solid3D.Faces(j)

                If solid3D.GetFaceSelection(j) Then

                    Dim faceTessellation As Mesh() = sf.Tessellation

                    For Each m As Mesh In faceTessellation
                        ap.Add(m.Vertices, m.Triangles)
                    Next

                    count += 1

                End If
            Next
#End If
        End If

        Return count
    End Function

    Private Function AddVolumeProperty(ByVal vp As VolumeProperties, ByVal ent As Entity, ByRef blockReferenceNotScaled As Boolean, ByVal Optional isParentSelected As Boolean = False) As Integer
        Dim count As Integer = 0
        blockReferenceNotScaled = True
        If ent.Selected OrElse isParentSelected Then
            If TypeOf ent Is IFace Then
                Dim itfFace As IFace = CType(ent, IFace)
                Dim meshes As Mesh() = itfFace.GetPolygonMeshes()
                For Each mesh As Mesh In meshes
                    vp.Add(mesh.Vertices, mesh.Triangles)
                Next

                count += 1
            ElseIf TypeOf ent Is BlockReference Then
                Dim br = CType(ent, BlockReference)
                If br.GetScaleFactorX() <> 1 AndAlso br.GetScaleFactorY() <> 1 AndAlso br.GetScaleFactorZ() <> 1 Then
                    blockReferenceNotScaled = False
                    Return count
                End If

                For Each e As Entity In br.GetEntities(model1.Blocks)
                    count += AddVolumeProperty(vp, e, blockReferenceNotScaled, True)

                    If Not blockReferenceNotScaled Then
                        Return count
                    End If
                Next
            End If
        End If

        Return count
    End Function


    Private Sub areaButton_OnClick(sender As Object, e As RoutedEventArgs)
        Dim ap As New AreaProperties()

        Dim count As Integer = 0
        Dim blockReferenceNotScaled As Boolean = True

        For i As Integer = 0 To model1.Entities.Count - 1

            Dim ent As Entity = model1.Entities(i)

            count += AddAreaProperty(ap, ent, blockReferenceNotScaled)

            If Not blockReferenceNotScaled Then Exit For
        Next

        Dim text As New StringBuilder()
        If blockReferenceNotScaled Then

            text.AppendLine(count.ToString() + " entity(ies) selected")
            text.AppendLine("---------------------")

            If ap.Centroid IsNot Nothing Then

                Dim x As Double, y As Double, z As Double
                Dim xx As Double, yy As Double, zz As Double, xy As Double, zx As Double, yz As Double
                Dim world As MomentOfInertia, centroid As MomentOfInertia

                ap.GetResults(ap.Area, ap.Centroid, x, y, z, xx,
                 yy, zz, xy, zx, yz, world,
                 centroid)

                text.AppendLine("Cumulative area: " + ap.Area.ToString() + " square " + model1.Units.ToString().ToLower())
                text.AppendLine("Cumulative centroid: " + ap.Centroid.ToString())
                text.AppendLine("Cumulative area moments:")
                text.AppendLine(" First moments")
                text.AppendLine("  x: " + x.ToString("g6"))
                text.AppendLine("  y: " + y.ToString("g6"))
                text.AppendLine("  z: " + z.ToString("g6"))
                text.AppendLine(" Second moments")
                text.AppendLine("  xx: " + xx.ToString("g6"))
                text.AppendLine("  yy: " + yy.ToString("g6"))
                text.AppendLine("  zz: " + zz.ToString("g6"))
                text.AppendLine(" Product moments")
                text.AppendLine("  xy: " + xx.ToString("g6"))
                text.AppendLine("  yz: " + yy.ToString("g6"))
                text.AppendLine("  zx: " + zz.ToString("g6"))
                text.AppendLine(" Area Moments of Inertia about World Coordinate Axes")
                text.AppendLine("  Ix: " + world.Ix.ToString("g6"))
                text.AppendLine("  Iy: " + world.Iy.ToString("g6"))
                text.AppendLine("  Iz: " + world.Iz.ToString("g6"))
                text.AppendLine(" Area Radii of Gyration about World Coordinate Axes")
                text.AppendLine("  Rx: " + world.Rx.ToString("g6"))
                text.AppendLine("  Ry: " + world.Ry.ToString("g6"))
                text.AppendLine("  Rz: " + world.Rz.ToString("g6"))
                text.AppendLine(" Area Moments of Inertia about Centroid Coordinate Axes:")
                text.AppendLine("  Ix: " + centroid.Ix.ToString("g6"))
                text.AppendLine("  Iy: " + centroid.Iy.ToString("g6"))
                text.AppendLine("  Iz: " + centroid.Iz.ToString("g6"))
                text.AppendLine(" Area Radii of Gyration about Centroid Coordinate Axes")
                text.AppendLine("  Rx: " + centroid.Rx.ToString("g6"))
                text.AppendLine("  Ry: " + centroid.Ry.ToString("g6"))

                text.AppendLine("  Rz: " + centroid.Rz.ToString("g6"))
            End If
        Else
            text.AppendLine("Error: scaled BlockReference is not supported.")
            text.AppendLine("---------------------")
        End If

        Dim rf As New DetailsWindow()

        rf.Title = "Area Properties"

        rf.contentTextBox.Text = text.ToString()

        rf.Show()
    End Sub

    Private Sub volumeButton_OnClick(sender As Object, e As RoutedEventArgs)
        Dim vp As New VolumeProperties()

        Dim count As Integer = 0
        Dim blockReferenceNotScaled As Boolean = True

        For i As Integer = 0 To model1.Entities.Count - 1

            Dim ent As Entity = model1.Entities(i)

            count += AddVolumeProperty(vp, ent, blockReferenceNotScaled)

            If Not blockReferenceNotScaled Then Exit For
        Next

        Dim text As New StringBuilder()
        If blockReferenceNotScaled Then

            text.AppendLine(count.ToString() + " entity(ies) selected")
            text.AppendLine("---------------------")

            If vp.Centroid IsNot Nothing Then

                Dim x As Double, y As Double, z As Double
                Dim xx As Double, yy As Double, zz As Double, xy As Double, zx As Double, yz As Double
                Dim world As MomentOfInertia, centroid As MomentOfInertia

                vp.GetResults(vp.Volume, vp.Centroid, x, y, z, xx,
             yy, zz, xy, zx, yz, world,
             centroid)

                text.AppendLine("Cumulative volume: " + vp.Volume.ToString() + " cubic " + model1.Units.ToString().ToLower())
                text.AppendLine("Cumulative centroid: " + vp.Centroid.ToString())
                text.AppendLine("Cumulative volume moments:")
                text.AppendLine(" First moments")
                text.AppendLine("  x: " + x.ToString("g6"))
                text.AppendLine("  y: " + y.ToString("g6"))
                text.AppendLine("  z: " + z.ToString("g6"))
                text.AppendLine(" Second moments")
                text.AppendLine("  xx: " + xx.ToString("g6"))
                text.AppendLine("  yy: " + yy.ToString("g6"))
                text.AppendLine("  zz: " + zz.ToString("g6"))
                text.AppendLine(" Product moments")
                text.AppendLine("  xy: " + xx.ToString("g6"))
                text.AppendLine("  yz: " + yy.ToString("g6"))
                text.AppendLine("  zx: " + zz.ToString("g6"))
                text.AppendLine(" Volume Moments of Inertia about World Coordinate Axes")
                text.AppendLine("  Ix: " + world.Ix.ToString("g6"))
                text.AppendLine("  Iy: " + world.Iy.ToString("g6"))
                text.AppendLine("  Iz: " + world.Iz.ToString("g6"))
                text.AppendLine(" Volume Radii of Gyration about World Coordinate Axes")
                text.AppendLine("  Rx: " + world.Rx.ToString("g6"))
                text.AppendLine("  Ry: " + world.Ry.ToString("g6"))
                text.AppendLine("  Rz: " + world.Rz.ToString("g6"))
                text.AppendLine(" Volume Moments of Inertia about Centroid Coordinate Axes:")
                text.AppendLine("  Ix: " + centroid.Ix.ToString("g6"))
                text.AppendLine("  Iy: " + centroid.Iy.ToString("g6"))
                text.AppendLine("  Iz: " + centroid.Iz.ToString("g6"))
                text.AppendLine(" Volume Radii of Gyration about Centroid Coordinate Axes")
                text.AppendLine("  Rx: " + centroid.Rx.ToString("g6"))
                text.AppendLine("  Ry: " + centroid.Ry.ToString("g6"))

                text.AppendLine("  Rz: " + centroid.Rz.ToString("g6"))
            End If
        Else
            text.AppendLine("Error: scaled BlockReference not supported.")
            text.AppendLine("---------------------")
        End If

        Dim rf As New DetailsWindow()

        rf.Title = "Volume Properties"

        rf.contentTextBox.Text = text.ToString()

        rf.Show()
    End Sub

#End Region

#Region "Imaging"

    Private Sub rasterCopyToClipboardButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.CopyToClipboardRaster()
    End Sub

    Private Sub rasterSaveButton_OnClick(sender As Object, e As RoutedEventArgs)
        Dim mySaveFileDialog As New SaveFileDialog()

        mySaveFileDialog.InitialDirectory = "."
        mySaveFileDialog.Filter = "Bitmap (*.bmp)|*.bmp|" + "Portable Network Graphics (*.png)|*.png|" + "Windows metafile (*.wmf)|*.wmf|" + "Enhanced Windows Metafile (*.emf)|*.emf"

        mySaveFileDialog.FilterIndex = 2
        mySaveFileDialog.RestoreDirectory = True
        Dim result As Nullable(Of Boolean) = mySaveFileDialog.ShowDialog()
        If result = True Then

            Select Case mySaveFileDialog.FilterIndex

                Case 1
                    model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp)
                    Exit Select
                Case 2
                    model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png)
                    Exit Select
                Case 3
                    model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Wmf)
                    Exit Select
                Case 4
                    model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Emf)
                    Exit Select


            End Select
        End If
    End Sub

    Private Sub printButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.Print()
    End Sub

    Private Sub printPreviewButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.PrintPreview(New System.Drawing.Size(500, 400))
    End Sub

    Private Sub pageSetupButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.PageSetup()
    End Sub

    Private Sub vectorCopyToClipbardButton_OnClick(sender As Object, e As RoutedEventArgs)
        model1.CopyToClipboardVector(False)

        'release mouse capture, otherwise the first mouse click is skipped
        vectorCopyToClipbardButton.ReleaseMouseCapture()
    End Sub

    Private Sub vectorSaveButton_OnClick(sender As Object, e As RoutedEventArgs)
        Dim mySaveFileDialog2 As New Microsoft.Win32.SaveFileDialog()

        mySaveFileDialog2.Filter = "Enhanced Windows Metafile (*.emf)|*.emf"
        mySaveFileDialog2.RestoreDirectory = True

        ' Show save file dialog box        
        If mySaveFileDialog2.ShowDialog() = True Then

            'To save as dxf/dwg, see the class HiddenLinesViewOnFileAutodesk available in x86 and x64 dlls                
            model1.WriteToFileVector(False, mySaveFileDialog2.FileName)

            'release mouse capture, otherwise the first mouse click is skipped								
            vectorSaveButton.ReleaseMouseCapture()
        End If
    End Sub

#End Region

#Region "File"
    Private _openFileAddOn As OpenFileAddOn
    Private Sub openButton_OnClick(sender As Object, e As EventArgs)
        Using openFileDialog1 As New Forms.OpenFileDialog()
            openFileDialog1.Filter = "Eyeshot (*.eye)|*.eye"
            openFileDialog1.Multiselect = False
            openFileDialog1.AddExtension = True
            openFileDialog1.CheckFileExists = True
            openFileDialog1.CheckPathExists = True
            openFileDialog1.DereferenceLinks = True
            _openFileAddOn = New OpenFileAddOn()

            AddHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged

            If openFileDialog1.ShowDialog(_openFileAddOn, Nothing) = Forms.DialogResult.OK Then
                _yAxisUp = False
                model1.Clear()
                Dim readFile As ReadFile = New ReadFile(openFileDialog1.FileName, CType(_openFileAddOn.ContentOption, contentType))
                model1.StartWork(readFile)
                model1.SetView(viewType.Trimetric, True, model1.AnimateCamera)
                openButton.IsEnabled = False
            End If

            RemoveHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged
            _openFileAddOn.Dispose()
            _openFileAddOn = Nothing
        End Using
    End Sub

    Private Sub OpenFileAddOn_EventFileNameChanged(ByVal sender As Forms.IWin32Window, ByVal filePath As String)
        If System.IO.File.Exists(filePath) Then
            Dim rf As ReadFile = New ReadFile(filePath, True)
            _openFileAddOn.SetFileInfo(rf.GetThumbnail(), rf.GetFileInfo())
        Else
            _openFileAddOn.ResetFileInfo()
        End If
    End Sub

    Private Sub SaveButton_OnClick(sender As Object, e As EventArgs)
        Using saveFileDialog As New Forms.SaveFileDialog()
            Using saveFileAddOn As New SaveFileAddOn()
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
                saveFileDialog.AddExtension = True
                saveFileDialog.CheckPathExists = True
                If saveFileDialog.ShowDialog(saveFileAddOn, Nothing) = Forms.DialogResult.OK Then
                    Dim writeFile As WriteFile = New WriteFile(New WriteFileParams(model1) With {.Content = CType(saveFileAddOn.ContentOption, contentType), .SerializationMode = CType(saveFileAddOn.SerialOption, serializationType), .SelectedOnly = saveFileAddOn.SelectedOnly, .Purge = saveFileAddOn.Purge}, saveFileDialog.FileName)
                    model1.StartWork(writeFile)
                    openButton.IsEnabled = False
                    saveButton.IsEnabled = False
                    importButton.IsEnabled = False
                End If
            End Using
        End Using
    End Sub

    Private Sub importButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)

        Using importFileDialog1 As New Forms.OpenFileDialog()
            Using importFileAddOn As New ImportFileAddOn()

                Dim theFilter As String = "All compatible file types (*.*)|*.asc;*.stl;*.obj;*.las;*.3ds"

#If NURBS Then

                theFilter += ";*.igs;*.iges;*.stp;*.step"
#End If

#If SOLID Then

                theFilter += ";*.ifc;*.ifczip"
#End If

                theFilter += "|Points (*.asc)|*.asc|" + "WaveFront OBJ (*.obj)|*.obj|" + "Stereolithography (*.stl)|*.stl|" + "Laser LAS (*.las)|*.las|" + "3D Studio Max (*.3ds)|*.3ds"

#If NURBS Then

                theFilter += "|IGES (*.igs; *.iges)|*.igs; *.iges|" + "STEP (*.stp; *.step)|*.stp; *.step"
#End If

#If SOLID Then

                theFilter += "|IFC (*.ifc; *.ifczip)|*.ifc; *.ifczip"
#End If
                importFileDialog1.Filter = theFilter

                importFileDialog1.Multiselect = False
                importFileDialog1.AddExtension = True
                importFileDialog1.CheckFileExists = True
                importFileDialog1.CheckPathExists = True

                If importFileDialog1.ShowDialog(importFileAddOn, Nothing) = Forms.DialogResult.OK Then

                    model1.Clear()
                    _yAxisUp = importFileAddOn.YAxisUp

                    Dim rfa As ReadFileAsync = getReader(importFileDialog1.FileName)

                    If rfa IsNot Nothing Then
                        model1.StartWork(rfa)
                        model1.SetView(viewType.Trimetric, True, model1.AnimateCamera)
                        openButton.IsEnabled = False
                        saveButton.IsEnabled = False
                        importButton.IsEnabled = False
                    End If
                End If
            End Using
        End Using
    End Sub

    Private Sub exportButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim saveFileDialog As SaveFileDialog = New SaveFileDialog()
        Dim theFilter As String = "WaveFront OBJ (*.obj)|*.obj|" & "Stereolithography (*.stl)|*.stl|" & "Laser LAS (*.las)|*.las|" & "WebGL (*.html)|*.html"

#If NURBS Then
        theFilter += "|STandard for the Exchange of Product (*.step)|*.step|" & "Initial Graphics Exchange Specification (*.iges)|*.iges"
#End If

        saveFileDialog.Filter = theFilter
        saveFileDialog.AddExtension = True
        saveFileDialog.CheckPathExists = True
        Dim result = saveFileDialog.ShowDialog()

        If result = True Then
            Dim wfa As WriteFileAsync = Nothing
            Dim dataParams As WriteParams

            Select Case saveFileDialog.FilterIndex
                Case 1
                    dataParams = New WriteParamsWithMaterials(model1)
                    wfa = New WriteOBJ(CType(dataParams, WriteParamsWithMaterials), saveFileDialog.FileName)
                Case 2
                    dataParams = New WriteParams(model1)
                    wfa = New WriteSTL(dataParams, saveFileDialog.FileName)
                Case 3
                    dataParams = Nothing
                    wfa = New WriteLAS(TryCast(model1.Entities.Where(Function(x) TypeOf x Is FastPointCloud).FirstOrDefault(), FastPointCloud), saveFileDialog.FileName)
                Case 4
                    dataParams = New WriteParamsWithMaterials(model1)
                    wfa = New WriteWebGL(CType(dataParams, WriteParamsWithMaterials), model1.DefaultMaterial, saveFileDialog.FileName)
#If NURBS Then
                Case 5
                    dataParams = New WriteParamsWithUnits(model1)
                    wfa = New WriteSTEP(CType(dataParams, WriteParamsWithUnits), saveFileDialog.FileName)
                Case 6
                    dataParams = New WriteParamsWithUnits(model1)
                    wfa = New WriteIGES(CType(dataParams, WriteParamsWithUnits), saveFileDialog.FileName)
#End If
            End Select

            model1.StartWork(wfa)
        End If
    End Sub

    Private Function getReader(fileName As String) As ReadFileAsync
        Dim ext As String = System.IO.Path.GetExtension(fileName)
        If ext IsNot Nothing Then
            ext = ext.TrimStart("."c).ToLower()
            Select Case ext
                Case "asc"
                    Return New ReadASC(fileName)
                Case "stl"
                    Return New ReadSTL(fileName)
                Case "obj"
                    Return New ReadOBJ(fileName)
                Case "las"
                    Return New ReadLAS(fileName)
                Case "3ds"
                    Return New Read3DS(fileName)
#If NURBS Then
                Case "igs", "iges"
                    Return New ReadIGES(fileName)
                Case "stp", "step"
                    Return New ReadSTEP(fileName)
#End If
#If SOLID Then
                Case "ifc", "ifczip"
                    Return New ReadIFC(fileName)
#End If
            End Select
        End If
        Return Nothing
    End Function

    Private Sub websiteButton_OnClick(sender As Object, e As RoutedEventArgs)
        System.Diagnostics.Process.Start("www.devdept.com")
    End Sub
#End Region

#Region "Event handlers"
    Dim totalSelectedCount As Integer = 0

        Private Sub model1_SelectionChanged(sender As Object, e As Model.SelectionChangedEventArgs) Handles model1.SelectionChanged

        Dim selectedCount As Integer = 0

        ' counts selected entities
        Dim selected As Object() = New Object(e.AddedItems.Count - 1) {}

        selectedCount = 0

        ' fills selected array
        For index As Integer = 0 To e.AddedItems.Count - 1
            Dim item = e.AddedItems(index)

            If TypeOf item Is Model.SelectedFace Then
                Dim faceItem = DirectCast(item, Model.SelectedFace)
                Dim ent = faceItem.Item

                If TypeOf ent Is Mesh Then
                    Dim mesh = DirectCast(ent, Mesh)
                    selected(selectedCount) = mesh.Faces(faceItem.Index)
                    selectedCount += 1

#If NURBS Then
                ElseIf TypeOf ent Is Brep Then
                    Dim sol = DirectCast(ent, Brep)
                    If faceItem.ShellIndex = 0 Then
                        selected(selectedCount) = sol.Faces(faceItem.Index)
                    Else
                        selected(selectedCount) = sol.Inners(faceItem.ShellIndex - 1)(faceItem.Index)
                    End If

                    selectedCount += 1
#End If

#If SOLID Then
                ElseIf TypeOf ent Is Solid Then
                    Dim sol = DirectCast(ent, Solid)
                    selected(selectedCount) = sol.Faces(faceItem.Index)
                    selectedCount += 1
#End If
                End If

#If NURBS Then
            ElseIf TypeOf item Is Model.SelectedEdge Then
                Dim edgeItem = DirectCast(item, Model.SelectedEdge)
                Dim ent = edgeItem.Item
                If TypeOf ent Is Brep Then
                    Dim sol = DirectCast(ent, Brep)
                    selected(selectedCount) = sol.Edges(edgeItem.Index)
                    selectedCount += 1
                End If

            ElseIf TypeOf item Is Model.SelectedVertex Then
                Dim vertexItem = DirectCast(item, Model.SelectedVertex)
                Dim ent = vertexItem.Item
                If TypeOf ent Is Brep Then
                    Dim sol = DirectCast(ent, Brep)
                    selected(selectedCount) = sol.Vertices(vertexItem.Index)
                    selectedCount += 1
                End If
#End If
            Else
                selected(selectedCount) = e.AddedItems(index).Item
                selectedCount += 1
            End If
        Next

        ' updates counters on the status bar
        totalSelectedCount += selectedCount - e.RemovedItems.Count
        selectedCountStatusLabel.Text = totalSelectedCount.ToString()
        addedCountStatusLabel.Text = e.AddedItems.Count.ToString()
        removedCountStatusLabel.Text = e.RemovedItems.Count.ToString()


    End Sub

    Private Sub viewportZero_LabelSelectionChanged(sender As Object, e As Model.SelectionChangedEventArgs)

        Dim count As Integer = 0

        ' counts selected entities
        For Each lbl As devDept.Eyeshot.Labels.Label In model1.Viewports(0).Labels

            If lbl.Selected Then

                count += 1
            End If
        Next
        ' updates count on the status bar
        selectedCountStatusLabel.Text = count.ToString()
        addedCountStatusLabel.Text = e.AddedItems.Count.ToString()
        removedCountStatusLabel.Text = e.RemovedItems.Count.ToString()
    End Sub

    Private Sub model1_WorkCancelled(sender As Object, e As EventArgs) Handles model1.WorkCancelled
            EnableControls()
    End Sub

    Private _skipZoomFit As Boolean = False
    Private Sub model1_WorkCompleted(sender As Object, e As devDept.Eyeshot.WorkCompletedEventArgs) Handles model1.WorkCompleted
        ' checks the WorkUnit type, more than one can be present in the same application 
        If TypeOf e.WorkUnit Is BallPivoting Then
            Dim bp As BallPivoting = DirectCast(e.WorkUnit, BallPivoting)

            Dim m As Mesh = bp.Result

            m.EdgeStyle = Mesh.edgeStyleType.Free
            model1.Entities.Clear()
            model1.Entities.Add(m, "Default", System.Drawing.Color.Beige)
        ElseIf TypeOf e.WorkUnit Is ReadFileAsync Then
            Dim rfa As ReadFileAsync = DirectCast(e.WorkUnit, ReadFileAsync)
            Dim ro As RegenOptions = New RegenOptions()
            ro.Async = _asyncRegen

            Dim rf As ReadFile = TryCast(e.WorkUnit, ReadFile)
            If rf IsNot Nothing Then
                _skipZoomFit = rf.FileSerializer.FileBody.Camera IsNot Nothing
            Else
                _skipZoomFit = False
            End If

            If rfa.Entities IsNot Nothing And _yAxisUp = True Then
                rfa.RotateEverythingAroundX()
            End If

            ' disable sexy features to get the max FPS on imported models
            model1.Shaded.ShadowMode   = shadowType.None
            model1.Rendered.ShadowMode = shadowType.None
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never
            model1.Rendered.PlanarReflections = false

            rfa.AddToScene(model1, ro)

            If _asyncRegen = False And _skipZoomFit = False Then
                model1.ZoomFit()
            End If
        ElseIf TypeOf e.WorkUnit Is Regeneration Then
            model1.Entities.UpdateBoundingBox()
            If Not _skipZoomFit Then
                model1.ZoomFit()
            End If
            model1.Invalidate()
            _skipZoomFit = False
        End If

        EnableControls()

        tableTabControl.Sync()
        UpdateDisplayModeButtons()

    End Sub

    Private Sub model1_WorkFailed(sender As Object, e As WorkFailedEventArgs) Handles model1.WorkFailed
        EnableControls()
    End Sub

    Private Sub model1_CameraMoveBegin(ByVal sender As Object, ByVal e As Environment.CameraMoveEventArgs) Handles model1.CameraMoveBegin
        UpdateTurboButton()
    End Sub

    Private Sub EnableControls()
        tabControl1.IsEnabled = True
        importButton.IsEnabled = True
        openButton.IsEnabled = True
        saveButton.IsEnabled = True
    End Sub

#End Region
    
    Private Sub layerListView_ItemChecked(sender As Object, e As RoutedEventArgs)
        Dim cb = TryCast(sender, CheckBox)
        Dim item = DirectCast(cb.DataContext, ListViewModelItem)

        If item.IsChecked Then

            model1.Layers.TurnOn(item.LayerName)
        Else


            model1.Layers.TurnOff(item.LayerName)
        End If

        ' updates bounding box, shadow and transparency
        model1.Entities.UpdateBoundingBox()

        model1.Invalidate()
    End Sub




    ''' <summary>
    ''' Represents a vetex type from model like center, mid point, etc.
    ''' </summary>
    Public Enum objectSnapType
        None
        Point
        [End]
        Mid
        Center
        Quad
    End Enum


    Private Sub magGlassCheckBox_OnClick(sender As Object, e As RoutedEventArgs)
        If CBool(magGlassCheckBox.IsChecked) Then
            model1.ActionMode = actionType.MagnifyingGlass
        Else
            model1.ActionMode = actionType.None
        End If
        model1.Invalidate()
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        For Each win As Window In Application.Current.Windows
            win.Close()
        Next
    End Sub

    Private Sub clipButton_Click(sender As Object, e As RoutedEventArgs)
        If model1 IsNot Nothing Then
            If CBool(clipButton.IsChecked) Then
                ' enables a clippingPlane
                model1.ClippingPlane1.Edit(Nothing)
                tabControl1.IsEnabled = False
            Else
                ' disables the clippingPlane and its change
                model1.ClippingPlane1.Cancel()
                tabControl1.IsEnabled = True
            End If
            model1.Invalidate()
        End If
    End Sub

    Private Sub OpenCurrentButton_OnClick(sender As Object, e As RoutedEventArgs)
        If openCurrentButton.IsChecked.HasValue AndAlso openCurrentButton.IsChecked.Value Then
            model1.Entities.OpenCurrentBlockReference()
        Else
            model1.Entities.CloseCurrentBlockReference()
        End If

        model1.Invalidate()
    End Sub

    Dim _asyncRegen As Boolean

    Private Sub regenAsyncButton_OnClick(sender As Object, e As RoutedEventArgs)
        _asyncRegen = regenAsyncButton.IsChecked
    End Sub

    Private _maxComplexity As Integer

    Private Sub turboButton_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        turboButton_OnClick()
    End Sub

    Private Sub turboButton_Unchecked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        turboButton_OnClick()
    End Sub

    Private Sub turboButton_OnClick()
        If model1 Is Nothing Then Return

        If turboButton.IsChecked.Value = False Then
            _maxComplexity = model1.Turbo.MaxComplexity
            model1.Turbo.MaxComplexity = Integer.MaxValue
        Else
            model1.Turbo.MaxComplexity = _maxComplexity
        End If

        model1.Entities.UpdateBoundingBox()
        UpdateTurboButton()
        model1.Invalidate()
    End Sub

    Private Sub UpdateTurboButton()
        If model1.Turbo.Enabled Then
            turboButton.Style = TryCast(FindResource("TurboToggleButtonStyle"), Style)
        Else
            turboButton.Style = TryCast(FindResource("ToggleButtonStyle"), Style)
        End If
    End Sub

    Private Shared _assetsPath As String = Nothing

    Public Shared Function GetAssetsPath() As String
        If String.IsNullOrEmpty(_assetsPath) Then
            Dim path As String = "../../../../../../dataset/Assets/"

            If System.IO.Directory.Exists(path) Then
                _assetsPath = path
            Else
                Dim product, title, company, edition As String
                Dim version As Version
                devDept.Eyeshot.Environment.GetAssembly(product, title, company, version, edition)
                _assetsPath = String.Format("{0}\{1} {2} {3} Samples\dataset\Assets\", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), product, edition, version.Major)
            End If
        End If

        Return _assetsPath
    End Function
End Class


''' <summary>    
''' This class represent the Model for Layers List.
''' </summary>    
Public Class ListViewModelItem
    Public Sub New(layer__1 As Layer)
        Layer = layer__1
        IsChecked = layer__1.Visible
        ForeColor = RenderContextUtility.ConvertColor(Layer.Color)
    End Sub

    Public Property Layer() As Layer
        Get
            Return m_Layer
        End Get
        Set(value As Layer)
            m_Layer = value
        End Set
    End Property
    Private m_Layer As Layer

    Public ReadOnly Property LayerName() As String
        Get
            Return Layer.Name
        End Get
    End Property

    Public ReadOnly Property LayerLineWeight() As Single
        Get
            Return Layer.LineWeight
        End Get
    End Property

    Public Property ForeColor() As Brush
        Get
            Return m_ForeColor
        End Get
        Set(value As Brush)
            m_ForeColor = value
        End Set
    End Property
    Private m_ForeColor As Brush

    Public Property IsChecked() As Boolean
        Get
            Return m_IsChecked
        End Get
        Set(value As Boolean)
            m_IsChecked = value
        End Set
    End Property
    Private m_IsChecked As Boolean
End Class

