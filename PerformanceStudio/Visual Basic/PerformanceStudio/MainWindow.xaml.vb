Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports System.Windows.Threading

Class MainWindow
    
    Const COLUMN_B As Double = 30
    Const COLUMN_H As Double = 30
    Const COLUMN_L As Double = 300
    Const BEAM_B As Double = 30
    Const BEAM_H As Double = 60
    Const BEAM_L As Double = 500
    Const SHELL_TICKNESS As Double = 30
    Const TEXT_PAD As Double = 5
    Const TEXT_HEIGHT As Double = 10

    Public Enum structureType
        Assembly = 0
        Flattened = 1
        SingleMesh = 2
    End Enum

    Private _entityList As New EntityList()
    Private _cols As Integer, _beams As Integer
    Private _bayXValue As Integer = 5
    Private _bayYValue As Integer = 5
    Private _floorsValue As Integer = 5
    Private _shellSubValue As Integer = 3
    Private _buildingMesh As Mesh
    Private _bricks As String = "Bricks"
    Private _concreteMatName As String = "Concrete"
    Private _wallMatName As String = "wallMat"
    Private _treeModify As Boolean

    Public Sub New(renderer As String)
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        Select Case renderer
            Case "DirectX"
                model1.Renderer = rendererType.Direct3D
                shadersCheckBox.IsEnabled = False
                depthCheckBox.IsEnabled = False
            Case "Native"
                model1.Renderer = rendererType.Native
                shadersCheckBox.IsEnabled = False
                depthCheckBox.IsEnabled = False
            Case "OpenGL"
                model1.Renderer = rendererType.OpenGL
        End Select

        model1.AskForAntiAliasing = True
        model1.AntiAliasing = false
        model1.DisplayMode = displayType.Rendered
        model1.GetGrid().Visible = False
        model1.Rendered.PlanarReflections = False
        model1.Rendered.ShadowMode = shadowType.Realistic
        model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never
        model1.Shaded.ShadowMode = shadowType.Realistic
        model1.Shaded.SilhouettesDrawingMode = silhouettesDrawingType.Never
        model1.ShowFps = True
        
        AddHandler floors.ValueChanged, AddressOf numeric_ValueChanged
        AddHandler bayX.ValueChanged, AddressOf numeric_ValueChanged
        AddHandler bayY.ValueChanged, AddressOf numeric_ValueChanged
        AddHandler shellSubdivisions.ValueChanged, AddressOf numeric_ValueChanged

        ' Listens the events to handle the tree synchronization
        AddHandler model1.MouseDown, AddressOf Model1_MouseDown
        AddHandler model1.MouseLeftButtonDown, AddressOf Model1_MouseLeftButtonDown
        AddHandler model1.MouseRightButtonDown, AddressOf Model1_MouseRightButtonDown
        AddHandler treeView1.SelectedItemChanged, AddressOf TreeView1_SelectedItemChanged

        ' Listens the events to handle the deletion of the selected entity
        AddHandler model1.KeyDown, AddressOf Model1_KeyDown
        AddHandler treeView1.KeyDown, AddressOf TreeView1_KeyDown

        ' Sets default values
        displayModeEnumButton.Set(displayType.Rendered)
        shadowModeEnumButton.Set(model1.Rendered.ShadowMode)
        structureModeEnumButton.Set(structureType.Assembly)

        UpdateCounters()

        _clickTimer.Interval = TimeSpan.FromMilliseconds(300)
        AddHandler _clickTimer.Tick, AddressOf ClickTimer
    End Sub

    Private Enum MouseClickType
        LeftClick
        LeftDoubleClick
        RightClick
    End Enum

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        _treeModify = False

        MyBase.OnContentRendered(e)
        model1.ActionMode = actionType.SelectVisibleByPick

        model1.Materials.Add(New Material(_concreteMatName, System.Drawing.Color.FromArgb(25, 25, 25), System.Drawing.Color.LightGray, System.Drawing.Color.FromArgb(31, 31, 31), 0.05F, 0.05F))
        model1.Materials.Add(New Material(_wallMatName, System.Drawing.Color.FromArgb(100, 25, 150, 25)))
        model1.Materials.Add(New Material(_bricks, New Bitmap("../../../../../../dataset/Assets/Textures/Bricks.jpg")))

        UpdateViewport()
        BuildAssembly()

        ' fits the model in the viewport
        model1.ZoomFit()
    End Sub

#Region "Mouse buttons handlers"
    Private ReadOnly _clickTimer As System.Windows.Threading.DispatcherTimer = New DispatcherTimer()
    Private _singleClick As Boolean
    Private _mouseLocation As System.Drawing.Point

    Private Sub Model1_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))
    End Sub

    Private Sub ClickTimer(ByVal sender As Object, ByVal e As EventArgs)
        If _singleClick Then
            StopTimer()
            Debug.WriteLine("Single click")
            Selection(MouseClickType.LeftClick)
        End If
    End Sub

    Private Sub StopTimer()
        _clickTimer.[Stop]()
        _singleClick = False
    End Sub

    Private Sub Model1_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If model1.ActionMode <> actionType.SelectVisibleByPick Then Return

        _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))

        If model1.GetMouseClicks(e) = 2 Then
            StopTimer()
            Debug.WriteLine("Double click")
            Selection(MouseClickType.LeftDoubleClick)
        Else
            _singleClick = True
            _clickTimer.Start()
            Selection(MouseClickType.LeftClick)
        End If
    End Sub

    Private Sub Model1_MouseRightButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If model1.ActionMode <> actionType.None Then Return

        _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))

        Debug.WriteLine("Right click")
        Selection(MouseClickType.RightClick)
    End Sub
#End Region

    Private lastSelectedItem As Model.SelectedItem

    Private Sub Selection(ByVal mouseClickType As MouseClickType)
        If _treeModify Then Return
        _treeModify = True

        If mouseClickType = MouseClickType.RightClick Then
            model1.Entities.SetParentAsCurrent()
        Else

            If lastSelectedItem IsNot Nothing Then
                lastSelectedItem.[Select](model1, False)
                lastSelectedItem = Nothing
            End If

            Dim item = model1.GetItemUnderMouseCursor(_mouseLocation)

            If item IsNot Nothing Then
                lastSelectedItem = item
                TreeViewUtility.CleanCurrent(model1, False)
                item.[Select](model1, True)
            Else
                If mouseClickType = MouseClickType.LeftDoubleClick Then TreeViewUtility.CleanCurrent(model1)
            End If
        End If

        TreeViewUtility.SynchScreenSelection(treeView1, New Stack(Of BlockReference)(model1.Entities.Parents), lastSelectedItem)
        model1.Invalidate()
        _treeModify = False
    End Sub

    Private Sub TreeView1_SelectedItemChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object))
        If _treeModify Then
            Return
        End If

        _treeModify = True
        If lastSelectedItem IsNot Nothing Then lastSelectedItem.[Select](model1, False)
        TreeViewUtility.CleanCurrent(model1)
        lastSelectedItem = TreeViewUtility.SynchTreeSelection(treeView1, model1)
        model1.Invalidate()
        _treeModify = False
    End Sub

    Private Sub TreeView1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        Model1_KeyDown(sender, e)
    End Sub

    Private Sub Model1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs)
        If e.Key = Key.Delete Then
            Dim selectedNode As TreeNode = CType(treeView1.SelectedItem, TreeNode)

            If selectedNode IsNot Nothing Then

                If lastSelectedItem IsNot Nothing AndAlso lastSelectedItem.Item IsNot Nothing AndAlso TypeOf lastSelectedItem.Item Is BlockReference Then
                    Dim br = TryCast(lastSelectedItem.Item, BlockReference)

                    If selectedNode.ParentNode IsNot Nothing Then
                        Dim parent = TryCast(selectedNode.ParentNode.Tag, BlockReference)
                        Dim parentBlockName = parent.BlockName

                        For Each b In model1.Blocks

                            If b.Name = parentBlockName Then
                                Dim toDelete As Entity = Nothing

                                For Each ent In b.Entities
                                    If ReferenceEquals(ent, br) Then toDelete = ent
                                Next

                                If toDelete IsNot Nothing Then b.Entities.Remove(toDelete)
                            End If
                        Next
                    Else
                        treeView1.Items.Remove(selectedNode)
                        model1.Entities.Remove(br)
                        model1.Invalidate()
                    End If
                ElseIf TypeOf lastSelectedItem.Item Is Entity Then
                    Dim entity = TryCast(lastSelectedItem.Item, Entity)

                    For Each b In model1.Blocks
                        If b.Entities.Contains(entity) Then b.Entities.Remove(entity)
                    Next

                    model1.Entities.DeleteSelected()
                End If

                TreeViewUtility.DeleteSelectedNode(treeView1, model1)
                treeView1.Items.Remove(selectedNode)
            End If
        End If
    End Sub

#Region "NumericUpDowns Handler"
    Private Sub numeric_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        _bayXValue = CInt(bayX.Value)
        _bayYValue = CInt(bayY.Value)
        _floorsValue = CInt(floors.Value)
        _shellSubValue = CInt(shellSubdivisions.Value)
        UpdateViewport()
        BuildAssembly()
        UpdateCounters()
        model1.Invalidate()
        UpdateCounters()
    End Sub
#End Region

#Region "Button Handlers"
    Private checkBoxesThatChangeAssembly As String() = New String() {"transparencyCheckBox", "textureCheckBox", "pillarsCheckBox", "labelCheckBox", "showBeamXCheckBox", "showBeamYCheckBox", "shellCheckBox", "nodesCheckBox"}

    Private Sub checkBoxes_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        UpdateViewport()
        Dim checkBox = CType(sender, System.Windows.Controls.Primitives.ToggleButton)
        If checkBoxesThatChangeAssembly.Contains(checkBox.Name) Then BuildAssembly()
        UpdateCounters()
        model1.Invalidate()
    End Sub

    Private Sub displayModeEnumButton_Click(sender As Object, e As EventArgs)
        model1.DisplayMode = displayModeEnumButton.Value
        model1.Invalidate()
    End Sub

    Private Sub shadowModeEnumButton_Click(sender As Object, e As EventArgs)
        model1.Rendered.ShadowMode = shadowModeEnumButton.Value
        model1.Shaded.ShadowMode = shadowModeEnumButton.Value
        model1.Invalidate()
    End Sub

    Private Sub structureModeEnumButton_Click(sender As Object, eventArgs As EventArgs)
        UpdateViewport()
        BuildAssembly()
    End Sub

    Private Sub rendererButton_Click(sender As Object, e As EventArgs)
        Dim startInfo As ProcessStartInfo = Process.GetCurrentProcess().StartInfo

        startInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location
        Dim [exit] = GetType(Application).GetMethod("ExitInternal", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.[Static])

        If rendererButton.Content.Equals("Native") Then
            startInfo.Arguments = "OpenGL"
        Else If rendererButton.Content.Equals("OpenGL") Then
            startInfo.Arguments = "DirectX"
        Else If rendererButton.Content.Equals("DirectX") Then
            startInfo.Arguments = "Native"
        End If
        Dim mBoxResult As MessageBoxResult = System.Windows.MessageBox.Show(Me, "Switching renderer to " + startInfo.Arguments.ToString() + " requires an application restart. Do you wish to proceed?", "Renderer", MessageBoxButton.OKCancel)
        If mBoxResult = MessageBoxResult.OK Then
            Application.Current.Shutdown()
            Process.Start(startInfo)
        End If
    End Sub
#End Region

    Private Sub UpdateCounters()
        If pillarsCheckBox.IsChecked = True Then
            _cols = (_bayXValue + 1) * (_bayYValue + 1) * _floorsValue
        Else
            _cols = 0
        End If

        Dim beamsX As Integer = If((showBeamXCheckBox.IsChecked = True), _bayXValue * (_bayYValue + 1), 0)
        Dim beamsY As Integer = If((showBeamYCheckBox.IsChecked = True), _bayYValue * (_bayXValue + 1), 0)
        _beams = (beamsX + beamsY) * _floorsValue
        lColumns.Content = _cols.ToString()
        lBeams.Content = _beams.ToString()

        If nodesCheckBox.IsChecked = True Then
            lJoints.Content = ((_bayXValue + 1) * (_bayYValue + 1) * (_floorsValue + 1)).ToString()
        Else
            lJoints.Content = "0"
        End If

        If shellCheckBox.IsChecked = True Then
            lShell.Content = (2 * _floorsValue * _bayXValue * _shellSubValue * _shellSubValue + 2 * _floorsValue * _bayYValue * _shellSubValue * _shellSubValue).ToString()
        Else
            lShell.Content = "0"
        End If
    End Sub
    Private Sub UpdateViewport()
        model1.AntiAliasing = If(antiAliasingCheckBox.IsChecked = True, True, False)
        model1.Rendered.PlanarReflections = If(planarCheckBox.IsChecked = True, True, False)
        model1.Rendered.EnvironmentMapping = If(environmentMappingCheckBox.IsChecked = True, True, False)
        model1.UseShaders = If(shadersCheckBox.IsChecked = True, True, False)
        model1.WriteDepthForTransparents = If(depthCheckBox.IsChecked = True, True, False)
        model1.Flat.ShowEdges = If(edgesCheckBox.IsChecked = True, True, False)
        model1.HiddenLines.ShowEdges = If(edgesCheckBox.IsChecked = True, True, False)
        model1.Rendered.ShowEdges = If(edgesCheckBox.IsChecked = True, True, False)
        model1.Shaded.ShowEdges = If(edgesCheckBox.IsChecked = True, True, False)
        model1.Wireframe.ShowEdges = If(edgesCheckBox.IsChecked = True, True, False)
    End Sub
    Private Sub BuildAssembly()
        model1.AntiAliasing = antiAliasingCheckBox.IsChecked
        model1.Rendered.PlanarReflections = planarCheckBox.IsChecked
        model1.UseShaders = shadersCheckBox.IsChecked

        model1.WriteDepthForTransparents = depthCheckBox.IsChecked

        model1.Flat.ShowEdges = edgesCheckBox.IsChecked
        model1.HiddenLines.ShowEdges = edgesCheckBox.IsChecked
        model1.Rendered.ShowEdges = edgesCheckBox.IsChecked
        model1.Shaded.ShowEdges = edgesCheckBox.IsChecked
        model1.Wireframe.ShowEdges = edgesCheckBox.IsChecked

        _entityList.Clear()
        model1.Entities.Clear()
        model1.Blocks.Clear()

        If model1.Materials.Count > 0 Then
            If transparencyCheckBox.IsChecked = True Then
                model1.Materials(_wallMatName).Diffuse = System.Drawing.Color.FromArgb(100, 25, 150, 25)
            Else
                model1.Materials(_wallMatName).Diffuse = System.Drawing.Color.FromArgb(25, 150, 25)
            End If
        End If

        ' Variables for unique Mesh (SingleMesh)
        Dim globalVerts As New List(Of Point3D)()
        Dim globalTris As New List(Of IndexTriangle)()
        Dim offset As Integer = globalVerts.Count

        ' Pillar column block
        Dim column As New devDept.Eyeshot.Block("squareCol")
        ' creates a gray box
        Dim m1 As Mesh = Mesh.CreateBox(COLUMN_B, COLUMN_H, COLUMN_L)

        ' apply texture if Texture is true
        If textureCheckBox.IsChecked = True Then
            m1.ApplyMaterial("Bricks", textureMappingType.Cubic, 1, 1)
        Else
            m1.ColorMethod = colorMethodType.byEntity
            m1.Color = System.Drawing.Color.LightGray
            m1.MaterialName = _concreteMatName
        End If

        For i As Integer = 0 To m1.Vertices.Length - 1
            globalVerts.Add(m1.Vertices(i))
        Next
        For i As Integer = 0 To m1.Triangles.Length - 1
            globalTris.Add(New ColorTriangle(offset + m1.Triangles(i).V1, offset + m1.Triangles(i).V2, offset + m1.Triangles(i).V3, System.Drawing.Color.Gray))
        Next

        Dim p As New Plane(New Vector3D(0, 1, 0))
        Dim at As New devDept.Eyeshot.Entities.Attribute(p, New Point3D(-(TEXT_HEIGHT + TEXT_PAD), COLUMN_B / 2, COLUMN_L / 2), "Name", "Frame", TEXT_HEIGHT)
        at.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BaselineCenter
        at.UpsideDown = True
        column.Entities.Add(at)

        column.Entities.Add(m1)

        ' adds the block to the master block dictionary
        model1.Blocks.Add(column)

        Dim reference As BlockReference

        ' Beam
        Dim beam As New devDept.Eyeshot.Block("beam")
        ' creates a gray box
        Dim m2 As Mesh = Mesh.CreateBox(BEAM_B, BEAM_L, BEAM_H)
        m2.ColorMethod = colorMethodType.byEntity
        m2.Color = System.Drawing.Color.LightGray
        m2.MaterialName = _concreteMatName

        offset = globalVerts.Count
        For i As Integer = 0 To m2.Vertices.Length - 1
            globalVerts.Add(m2.Vertices(i))
        Next
        For i As Integer = 0 To m2.Triangles.Length - 1
            globalTris.Add(New ColorTriangle(offset + m2.Triangles(i).V1, offset + m2.Triangles(i).V2, offset + m2.Triangles(i).V3, System.Drawing.Color.Gray))
        Next

        beam.Entities.Add(m2)

        p = New Plane(New Vector3D(1, 0, 0))
        at = New devDept.Eyeshot.Entities.Attribute(p, New Point3D(BEAM_B / 2, BEAM_L / 2, BEAM_H + TEXT_PAD), "Name", "Frame", TEXT_HEIGHT)
        at.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BaselineCenter
        at.Color = System.Drawing.Color.Green
        at.ColorMethod = colorMethodType.byEntity
        beam.Entities.Add(at)

        ' adds the block to the master block dictionary
        model1.Blocks.Add(beam)

        ' Shell
        Dim shell As New devDept.Eyeshot.Block("shell")
        Dim shellB As Double = BEAM_L / _shellSubValue
        Dim shellH As Double = COLUMN_L / _shellSubValue

        ' Mesh
        Dim m3 As Mesh = Mesh.CreateBox(shellB, SHELL_TICKNESS, shellH)
        m3.ColorMethod = colorMethodType.byEntity
        m3.Color = System.Drawing.Color.LightGreen
        m3.MaterialName = "wallMat"
        shell.Entities.Add(m3)

        ' adds the block to the master block dictionary
        model1.Blocks.Add(shell)

        For k As Integer = 0 To _floorsValue - 1
            For j As Integer = 0 To _bayYValue
                For i As Integer = 0 To _bayXValue
                    If pillarsCheckBox.IsChecked = True Then
                        reference = New BlockReference(i * BEAM_L - COLUMN_B / 2, j * BEAM_L - COLUMN_H / 2, k * COLUMN_L, "squareCol", 1, 1,
                            1, 0)
                        If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                            Dim mm As Mesh = DirectCast(model1.Blocks("squareCol").Entities(1).Clone(), Mesh)
                            mm.Translate(i * BEAM_L - COLUMN_B / 2, j * BEAM_L - COLUMN_H / 2, k * COLUMN_L)
                            offset = globalVerts.Count
                            globalVerts.AddRange(mm.Vertices)
                            For n As Integer = 0 To mm.Triangles.Length - 1
                                globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.Gray))
                            Next
                        End If

                        If labelCheckBox.IsChecked = True Then
                            reference.Attributes.Add("Name", String.Format("Pillar_{0},{1},{2}", i, j, k))
                        End If
                        _entityList.Add(reference)
                    End If

                    If showBeamXCheckBox.IsChecked = True Then
                        If j <= _bayYValue AndAlso i < _bayXValue Then
                            ' Parallel beams to X
                            Dim t As New Transformation()
                            t.Rotation(-Math.PI / 2, Vector3D.AxisZ)
                            Dim t2 As New Transformation()
                            t2.Translation(i * BEAM_L, j * BEAM_L + BEAM_B / 2, (k + 1) * COLUMN_L - BEAM_H / 2)
                            reference = New BlockReference(t2 * t, "beam")

                            If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                                Dim mm As Mesh = DirectCast(model1.Blocks("beam").Entities(0).Clone(), Mesh)
                                mm.TransformBy(t2 * t)

                                offset = globalVerts.Count
                                globalVerts.AddRange(mm.Vertices)
                                For n As Integer = 0 To mm.Triangles.Length - 1
                                    globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.Gray))
                                Next
                            End If

                            If labelCheckBox.IsChecked = True Then
                                reference.Attributes.Add("Name", String.Format("Beam_{0},{1},{2}", i, j, k))
                            End If
                            _entityList.Add(reference)
                        End If
                    End If

                    If showBeamYCheckBox.IsChecked = True Then
                        If i <= _bayXValue AndAlso j < _bayYValue Then
                            ' Parallel beams to X
                            Dim t As New Transformation()
                            t.Translation(i * BEAM_L - BEAM_B / 2, j * BEAM_L, (k + 1) * COLUMN_L - BEAM_H / 2)
                            reference = New BlockReference(t, "beam")

                            If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                                Dim mm As Mesh = DirectCast(model1.Blocks("beam").Entities(0).Clone(), Mesh)
                                mm.TransformBy(t)
                                offset = globalVerts.Count
                                globalVerts.AddRange(mm.Vertices)
                                For n As Integer = 0 To mm.Triangles.Length - 1
                                    globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.Gray))
                                Next
                            End If
                            If labelCheckBox.IsChecked = True Then
                                reference.Attributes.Add("Name", String.Format("Beam_{0},{1},{2}", i, j, k))
                            End If
                            _entityList.Add(reference)
                        End If
                    End If

                    If shellCheckBox.IsChecked = True Then
                        If (j = 0 OrElse j = _bayYValue) AndAlso i < _bayXValue Then
                            For i1 As Integer = 0 To _shellSubValue - 1
                                For j1 As Integer = 0 To _shellSubValue - 1
                                    Dim t As New Transformation()
                                    t.Translation(i * BEAM_L + i1 * shellB, j * BEAM_L - SHELL_TICKNESS / 2, k * COLUMN_L + j1 * shellH)
                                    reference = New BlockReference(t, "shell")

                                    If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                                        Dim mm As Mesh = DirectCast(model1.Blocks("shell").Entities(0).Clone(), Mesh)
                                        mm.TransformBy(t)
                                        offset = globalVerts.Count
                                        globalVerts.AddRange(mm.Vertices)
                                        For n As Integer = 0 To mm.Triangles.Length - 1
                                            globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)))
                                        Next
                                    End If
                                    _entityList.Add(reference)
                                Next
                            Next
                        End If
                    End If
                    If nodesCheckBox.IsChecked = True Then
                        Dim joint1 As New Joint(i * BEAM_L, j * BEAM_L, k * COLUMN_L, 40, 2)
                        joint1.Color = System.Drawing.Color.Blue
                        joint1.ColorMethod = colorMethodType.byEntity
                        Dim joint2 As New Joint(i * BEAM_L, j * BEAM_L, (k + 1) * COLUMN_L, 40, 2)
                        joint2.Color = System.Drawing.Color.Blue
                        joint2.ColorMethod = colorMethodType.byEntity
                        _entityList.Add(joint1)
                        _entityList.Add(joint2)

                    End If
                Next

                If shellCheckBox.IsChecked = True Then
                    If j = 0 Then
                        For l As Integer = 0 To _bayYValue - 1
                            For i1 As Integer = 0 To _shellSubValue - 1
                                For j1 As Integer = 0 To _shellSubValue - 1
                                    Dim t As New Transformation()
                                    t.Translation(l * BEAM_L + i1 * shellB, j * BEAM_L - SHELL_TICKNESS / 2, k * COLUMN_L + j1 * shellH)
                                    Dim t2 As New Transformation()
                                    t2.Rotation(Math.PI / 2, Vector3D.AxisZ)
                                    reference = New BlockReference(t2 * t, "shell")

                                    If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                                        Dim mm As Mesh = DirectCast(model1.Blocks("shell").Entities(0).Clone(), Mesh)
                                        mm.TransformBy(t2 * t)
                                        offset = globalVerts.Count
                                        globalVerts.AddRange(mm.Vertices)
                                        For n As Integer = 0 To mm.Triangles.Length - 1
                                            globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)))
                                        Next
                                    End If
                                    _entityList.Add(reference)
                                Next
                            Next
                        Next
                    End If
                    If j = _bayYValue Then
                        For l As Integer = 0 To _bayYValue - 1
                            For i1 As Integer = 0 To _shellSubValue - 1
                                For j1 As Integer = 0 To _shellSubValue - 1
                                    Dim t As New Transformation()
                                    t.Translation(l * BEAM_L + i1 * shellB, -SHELL_TICKNESS / 2, k * COLUMN_L + j1 * shellH)
                                    Dim t2 As New Transformation()
                                    t2.Rotation(Math.PI / 2, Vector3D.AxisZ)
                                    Dim t3 As New Transformation()
                                    t3.Translation(_bayXValue * BEAM_L, 0, 0)
                                    reference = New BlockReference(t3 * t2 * t, "shell")

                                    If DirectCast(structureModeEnumButton.Value, structureType) = structureType.SingleMesh Then
                                        Dim mm As Mesh = DirectCast(model1.Blocks("shell").Entities(0).Clone(), Mesh)
                                        mm.TransformBy(t3 * t2 * t)
                                        offset = globalVerts.Count
                                        globalVerts.AddRange(mm.Vertices)
                                        For n As Integer = 0 To mm.Triangles.Length - 1
                                            globalTris.Add(New ColorTriangle(offset + mm.Triangles(n).V1, offset + mm.Triangles(n).V2, offset + mm.Triangles(n).V3, System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)))
                                        Next
                                    End If
                                    _entityList.Add(reference)
                                Next
                            Next
                        Next
                    End If
                End If
            Next
        Next

        _buildingMesh = New Mesh(globalVerts, globalTris)
        _buildingMesh.ColorMethod = colorMethodType.byEntity
        model1.Entities.AddRange(_entityList)

        Select Case DirectCast(structureModeEnumButton.Value, structureType)
            Case structureType.Flattened
                If True Then
                    Dim entList As Entity() = model1.Entities.Explode()
                    model1.Entities.Clear()
                    model1.Entities.AddRange(entList)
                    model1.Invalidate()
                    Exit Select
                End If

            Case structureType.SingleMesh
                If True Then
                    model1.Entities.Clear()
                    model1.Entities.Add(_buildingMesh)
                    model1.Invalidate()
                    Exit Select
                End If

            Case structureType.Assembly
                If True Then
                    model1.Invalidate()
                    Exit Select
                End If
        End Select
        TreeViewUtility.PopulateTree(treeView1, model1.Entities.ToList(), model1.Blocks)
    End Sub
End Class