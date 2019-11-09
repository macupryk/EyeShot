Imports System.Drawing
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Eyeshot.Translators
Imports devDept.Serialization
Imports devDept.CustomControls
Imports Microsoft.Win32
Imports Rotation = devDept.Geometry.Rotation
Imports Block = devDept.Eyeshot.Block
Imports EyeshotExtensions

Namespace WpfApplication1

    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Public ReadOnly Property AsyncRegen As Boolean
            Get
                Return regenAsyncChk.IsChecked
            End Get
        End Property

        Public ReadOnly Property OpenSaveAsync As Boolean
            Get
                Return asyncCheckBox.IsChecked
            End Get
        End Property

        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.  

        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)

            ' Hides grid
            model1.GetGrid().Visible = False

            ' Sets display mode settings
            model1.DisplayMode = displayType.Rendered
            model1.Rendered.ShowEdges = False     
            model1.Rendered.ShadowMode = shadowType.None
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.LastFrame
            model1.Rendered.PlanarReflections = True
            model1.Rendered.EnvironmentMapping = True

            ' Adds line types
            Dim lineTypeDash = "Dash"
            model1.LineTypes.Add(lineTypeDash, New Single() {0.2F, -0.15F})
            Dim lineTypeDashDot = "DashDot"
            model1.LineTypes.Add(lineTypeDashDot, New Single() {0.5F, -0.15F, 0.025F, -0.15F})

            ' Adds a block
            model1.Blocks.Add(BuildBlock())

            Dim entities = New List(Of Entity)()

            ' Creates mesh with texture
            entities.Add(BuildMeshWithTexture())

            ' Adds a block reference
            Dim br = New BlockReference(8, -12, 0, "ArrowWithAttributes", Utility.DegToRad(60))
            br.Attributes.Item("att1") = New AttributeReference("Plate")
            br.Attributes.Item("att2") = New AttributeReference("With Holes")
            br.ColorMethod = colorMethodType.byEntity
            br.Color = System.Drawing.Color.SandyBrown

#If Not OLDVER Then
            br.EntityData = New CustomData(1, 40) With {.Description = "This is a BlockRef with price"}
#Else
            br.EntityData = new CustomData(1) With { .Description = "This is BlockRef" }
#End If
            entities.Add(br)

            ' Creates MyCircle
            Dim myCircle = New MyCircle(Plane.XY, 3)
            myCircle.CustomDescription = "Custom circle with dash-dot"
            myCircle.ColorMethod = colorMethodType.byEntity
            myCircle.Color = System.Drawing.Color.RoyalBlue
            myCircle.LineTypeMethod = colorMethodType.byEntity
            myCircle.LineTypeName = lineTypeDashDot
            myCircle.TransformBy(New Rotation(Utility.DegToRad(45), Vector3D.AxisY) * New Translation(-5, 10, 0))
#If Not OLDVER Then
            myCircle.EntityData = New CustomData(2, 25) With {.Description = "This is MyCircle with price"}
#Else
            myCircle.EntityData = new CustomData(2) With { .Description = "This is MyCircle"}
#End If
            entities.Add(myCircle)

            ' Creates a linearpath             
            Dim lp = New LinearPath({New Point3D(0, -1.25), New Point3D(1.875, -2.5), New Point3D(0.9375, -0.3125), New Point3D(2.5, 0.9375), New Point3D(0.625, 0.9375), New Point3D(0, 2.5), New Point3D(-0.625, 0.9375), New Point3D(-2.5, 0.9375), New Point3D(-0.9375, -0.3125), New Point3D(-1.875, -2.5), New Point3D(0, -1.25)})

            lp.ColorMethod = colorMethodType.byEntity
            lp.Color = System.Drawing.Color.Green
            lp.LineTypeMethod = colorMethodType.byEntity
            lp.LineTypeName = lineTypeDash
            lp.GlobalWidth = 0.1
            lp.TransformBy(New Translation(3, 5, 0) * New Rotation(Utility.DegToRad(45), Vector3D.AxisX))

#If Not OLDVER Then
            lp.EntityData = New CustomData(3, 30) With {.Description = "This is a LinearPath with price"}
#Else
            lp.EntityData = new CustomData(3) With { .Description = "This is a LinearPath" }
#End If
            entities.Add(lp)

#If SOLID Then
            ' Creates solid
            Dim cone = Solid.CreateCone(5, 2, 4, 30)
            cone.ColorMethod = colorMethodType.byEntity
            cone.Color = System.Drawing.Color.FromArgb(124, System.Drawing.Color.Green)
            cone.Translate(3, 15, 2)
            entities.Add(cone)
#End If

#If NURBS Then
            ' Creates solid3D
            entities.Add(BuildBrep())

            ' Creates Surface
            entities.Add(BuildSurface())
#End If

            ' Creates region
            entities.Add(BuildRegion())

            ' adds all the entities to the scene
            model1.Entities.AddRange(entities)

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            model1.ZoomFit()

            MyBase.OnContentRendered(e)

        End Sub

#Region "Async event handlers"

        Private Sub model1_WorkCompleted(ByVal sender As System.Object, ByVal e As devDept.Eyeshot.WorkCompletedEventArgs) Handles model1.WorkCompleted

            ' Checks the WorkUnit type, more than one can be present in the same application 
            If TypeOf e.WorkUnit Is WriteFile Then
                ShowLog("WriteFile log", DirectCast(e.WorkUnit, WriteFile).Log)
                SetButtonEnabled(True)
            ElseIf TypeOf e.WorkUnit Is ReadFile Then
                Dim rf = DirectCast(e.WorkUnit, ReadFile)
                AddToScene(rf)
                ShowLog(rf)
            ElseIf TypeOf e.WorkUnit Is Regeneration Then
                model1.Entities.UpdateBoundingBox()
                EnableButtonsAndRefresh()
            End If

        End Sub

        Private Sub model1_WorkFailed(sender As Object, e As WorkFailedEventArgs) Handles model1.WorkFailed

            SetButtonEnabled(True)

        End Sub

        Private Sub model1_WorkCancelled(ByVal sender As System.Object, ByVal e As EventArgs) Handles model1.WorkCancelled

            SetButtonEnabled(True)

        End Sub

#End Region

#Region "Helper methods"

        Private Sub SetButtonEnabled(ByVal value As Boolean)

            openButton.IsEnabled = value
            saveButton.IsEnabled = value

        End Sub

        Private _skipZoomFit As Boolean = False
        Private Sub AddToScene(ByVal rfa As ReadFile)

            Dim ro = New RegenOptions()
            ro.Async = AsyncRegen

            rfa.AddToScene(model1, ro)

            _skipZoomFit = rfa.FileSerializer.FileBody.Camera IsNot Nothing

            If (Not AsyncRegen) Then
                EnableButtonsAndRefresh()
            End If

        End Sub

        Private Sub EnableButtonsAndRefresh()

            If (_skipZoomFit = False) Then
                model1.ZoomFit()
            End If
            SetButtonEnabled(True)
            model1.Invalidate()

        End Sub

        Private Sub ShowLog(ByVal rf As ReadFile)
            If String.IsNullOrEmpty(rf.Log) Then Return
            Dim msg As String = String.Format("{0}{1}Log{1}----------------------{1}{2}", rf.GetFileInfo(), System.Environment.NewLine, rf.Log)
            ShowLog("File info", msg)
        End Sub

        Private Sub ShowLog(ByVal title As String, ByVal log As String)
            If Not String.IsNullOrEmpty(log) Then
                Dim df = New DetailsWindow()
                df.contentTextBox.Text = String.Format("{0}{1}----------------------{1}{2}", title, System.Environment.NewLine, log)
                df.Show()
            End If
        End Sub

#End Region


        Private Function BuildMeshWithTexture() As Mesh

            Dim mat = New Material("Marble", new Bitmap("../../../../../../dataset/Assets/Textures/Maple.jpg"))
            model1.Materials.Add(mat)
            Const tol = 0.01
            Const radius = 0.4
            Dim profile As ICurve = New CompositeCurve(New Line(0, 0, 8, 0), New Arc(New Point3D(8, 2, 0), 2, 6 * Math.PI / 4, 2 * Math.PI), New Line(10, 2, 10, 6), New Line(10, 6, 0, 6), New Line(0, 6, 0, 0))
            Dim holes = New ICurve(19) {}
            Dim count = 0
            For y = 0 To 4 - 1
                For x = 0 To 5 - 1
                    holes(count) = New Circle(New Point3D(1 + x * 1, 1 + y * 1, 0), radius)
                    holes(count).Reverse()
                    count += 1
                Next
            Next

            Dim contours = New List(Of ICurve)() From {profile}
            contours.AddRange(holes)
            Dim region = New devDept.Eyeshot.Entities.Region(contours, Plane.XY, False)
            Dim plate As Mesh = region.ExtrudeAsMesh(New Vector3D(0, 0, 0.25), tol, Mesh.natureType.RichSmooth)
            plate.FlipNormal()
            plate.NormalAveragingMode = Mesh.normalAveragingType.AveragedByAngle
            plate.ApplyMaterial(mat.Name, textureMappingType.Cubic, 1, 1)
            plate.Translate(10, 0, 0)
            Return plate

        End Function
#If NURBS Then
        Private Function BuildBrep() As Brep

            Dim height As Double = 12
            Dim radius As Double = 4
            Dim offset As Double = 1.5
            Dim vertices = New Point3D(3) {}
            vertices(0) = New Brep.Vertex(radius, 0, 0)
            vertices(1) = New Brep.Vertex(radius, 0, height)
            Dim edges = New Brep.Edge(5) {}
            Dim faces = New Brep.Face(2) {}
            Dim c1 = New Circle(Plane.XY, radius)
            edges(0) = New Brep.Edge(c1, 0, 0)
            Dim c2 = New Circle(Plane.XY, radius)
            c2.Translate(0, 0, height)
            edges(1) = New Brep.Edge(c2, 1, 1)
            Dim l1 = New Line(CType(vertices(0).Clone(), Point3D), CType(vertices(1).Clone(), Point3D))
            edges(2) = New Brep.Edge(l1, 0, 1)
            Dim bottomLoop = New Brep.OrientedEdge(0) {}
            bottomLoop(0) = New Brep.OrientedEdge(0, False)
            Dim sideLoop = New Brep.OrientedEdge(3) {}
            sideLoop(0) = New Brep.OrientedEdge(0)
            sideLoop(1) = New Brep.OrientedEdge(2)
            sideLoop(2) = New Brep.OrientedEdge(1, False)
            sideLoop(3) = New Brep.OrientedEdge(2, False)
            Dim topLoop = New Brep.OrientedEdge(0) {}
            topLoop(0) = New Brep.OrientedEdge(1)
            Dim top = New PlanarSurf(vertices(1), Vector3D.AxisZ, Vector3D.AxisX)
            Dim side = New CylindricalSurf(Point3D.Origin, Vector3D.AxisZ, Vector3D.AxisX, radius)
            Dim bottom = New PlanarSurf(vertices(0), Vector3D.AxisZ, Vector3D.AxisX)
            faces(0) = New Brep.Face(bottom, New Brep.[Loop](bottomLoop), False)
            faces(1) = New Brep.Face(side, New Brep.[Loop](sideLoop))
            faces(2) = New Brep.Face(top, New Brep.[Loop](topLoop))
            Dim solid3D = New Brep(vertices, edges, faces)
            vertices(2) = New Brep.Vertex(radius - offset, 0, offset)
            vertices(3) = New Brep.Vertex(radius - offset, 0, height - offset)
            Dim c11 = New Circle(New Point3D(0, 0, offset), radius - offset)
            edges(3) = New Brep.Edge(c11, 2, 2)
            Dim c22 = New Circle(New Point3D(0, 0, height - offset), radius - offset)
            edges(4) = New Brep.Edge(c22, 3, 3)
            Dim l7 = New Line(CType(vertices(3).Clone(), Point3D), CType(vertices(2).Clone(), Point3D))
            edges(5) = New Brep.Edge(l7, 2, 3)
            Dim voidBottomLoop = New Brep.OrientedEdge() {New Brep.OrientedEdge(3)}
            Dim voidSideLoop = New Brep.OrientedEdge() {New Brep.OrientedEdge(3, False), New Brep.OrientedEdge(5), New Brep.OrientedEdge(4), New Brep.OrientedEdge(5, False)}
            Dim voidTopLoop = New Brep.OrientedEdge() {New Brep.OrientedEdge(4, False)}
            Dim voidBottom = New PlanarSurf(vertices(2), Vector3D.AxisZ, Vector3D.AxisX)
            Dim voidSide = New CylindricalSurf(CType(c11.Center.Clone(), Point3D), Vector3D.AxisZ, Vector3D.AxisX, radius - offset)
            Dim voidTop = New PlanarSurf(vertices(3), Vector3D.AxisZ, Vector3D.AxisX)
            Dim innerVoid = New Brep.Face() {New Brep.Face(voidBottom, New Brep.[Loop](voidBottomLoop)), New Brep.Face(voidSide, New Brep.[Loop](voidSideLoop), False), New Brep.Face(voidTop, New Brep.[Loop](voidTopLoop), False)}
            solid3D.Inners = New Brep.Face()() {innerVoid}
            solid3D.ColorMethod = colorMethodType.byEntity
            solid3D.Color = System.Drawing.Color.FromArgb(124, System.Drawing.Color.Red)
            solid3D.Translate(28, 15, -5)
            Return solid3D

        End Function

        Public Function BuildSurface() As Surface

            Dim array = New List(Of Point3D)()
            array.Add(New Point3D(0, 0, 0))
            array.Add(New Point3D(0, 2, 1.5))
            array.Add(New Point3D(0, 4, 0))
            array.Add(New Point3D(0, 6, 0.5))
            Dim firstU As Curve = Curve.GlobalInterpolation(array, 3)
            array.Clear()
            array.Add(New Point3D(4, 0, 1))
            array.Add(New Point3D(4, 3, 1.5))
            array.Add(New Point3D(4, 5, 0))
            Dim secondU As Curve = Curve.GlobalInterpolation(array, 2)
            array.Clear()
            array.Add(New Point3D(8, 0, 0))
            array.Add(New Point3D(8, 3, 2))
            array.Add(New Point3D(8, 6, 0))
            Dim thirdU As Curve = Curve.GlobalInterpolation(array, 2)
            array.Clear()
            array.Add(New Point3D(0, 0, 0))
            array.Add(New Point3D(4, 0, 1))
            array.Add(New Point3D(8, 0, 0))
            Dim firstV As Curve = Curve.GlobalInterpolation(array, 2)
            array.Clear()
            array.Add(New Point3D(0, 6, 5))
            array.Add(New Point3D(4, 5, 0))
            array.Add(New Point3D(6, 4, 0))
            array.Add(New Point3D(8, 6, 0))
            Dim secondV As Curve = Curve.GlobalInterpolation(array, 2)
            Dim ptGrid = New Point3D(2, 1) {}
            ptGrid(0, 0) = New Point3D(0, 0, 0)
            ptGrid(0, 1) = New Point3D(0, 6, 0.5)
            ptGrid(1, 0) = New Point3D(4, 0, 1)
            ptGrid(1, 1) = New Point3D(4, 5, 0)
            ptGrid(2, 0) = New Point3D(8, 0, 0)
            ptGrid(2, 1) = New Point3D(8, 6, 0)
            Dim surface As Surface = Surface.Gordon(New Curve() {firstU, secondU, thirdU}, New Curve() {firstV, secondV}, ptGrid)
            surface.ColorMethod = colorMethodType.byEntity
            surface.Color = System.Drawing.Color.MediumPurple
            surface.Translate(24, -4, 0)
            Return surface

        End Function
#End If
        Private Function BuildRegion() As devDept.Eyeshot.Entities.Region

            Dim outer = New Circle(Point3D.Origin, 6)
            Dim inner1 = New Ellipse(New Point3D(-2, 2, 0), 0.5, 1)
            Dim inner2 = New Ellipse(New Point3D(2, 2, 0), 0.5, 1)
            Dim circle = New Circle(Point3D.Origin, 4)
            Dim i1 As Point3D
            Dim i2 As Point3D
            CompositeCurve.IntersectionLineCircle3D(New Line(-10, 0, 10, 0), circle, i1, i2)
            Dim arc1 = New Arc(New Point3D(0, 2, 0), i1, i2)
            Dim segments As ICurve()
            circle.SplitBy(New List(Of Point3D) From {i1, i2}, segments)
            Dim inner3 = New CompositeCurve(New List(Of ICurve)() From {arc1, segments(1)})
            Dim reg = New devDept.Eyeshot.Entities.Region(New List(Of ICurve) From {outer, inner1, inner2, inner3})
            reg.ColorMethod = colorMethodType.byEntity
            reg.Color = System.Drawing.Color.FromArgb(255, 180, 30)
            reg.TransformBy(New Translation(12, 25, 0) * New Rotation(Math.PI / 2, Vector3D.AxisX))
            Return reg

        End Function

        Private Function BuildBlock() As Block

            Dim myBlock = New Block("ArrowWithAttributes")
            Dim l = New Line(0, 0, 10, 0)
            Dim t = New Triangle(New Point3D(0, -1.5), New Point3D(0, 1.5), New Point3D(1.5, 0))

            t.Translate(10, 0, 0)
            l.ColorMethod = t.ColorMethod = colorMethodType.byParent
            Dim a1 = New devDept.Eyeshot.Entities.Attribute(5, 0.2, 0, "att1", "Top Text", 0.8)
            a1.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BottomCenter
            Dim a2 = New devDept.Eyeshot.Entities.Attribute(5, -0.4, 0, "att2", "Bottom Text", 0.8)
            a2.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.TopCenter

            l.ColorMethod = colorMethodType.byParent
            t.ColorMethod = colorMethodType.byParent
            a1.ColorMethod = colorMethodType.byParent
            a2.ColorMethod = colorMethodType.byParent

            myBlock.Entities.Add(l)
            myBlock.Entities.Add(t)
            myBlock.Entities.Add(a1)
            myBlock.Entities.Add(a2)
            Return myBlock

        End Function

        Private _openFileAddOn As OpenFileAddOn
        Private Sub openButton_Click(ByVal sender As Object, ByVal e As EventArgs)

            Using openFileDialog1 = New Forms.OpenFileDialog()

                openFileDialog1.Filter = "Eyeshot (*.eye)|*.eye"
                openFileDialog1.Multiselect = False
                openFileDialog1.AddExtension = True
                openFileDialog1.CheckFileExists = True
                openFileDialog1.CheckPathExists = True
                openFileDialog1.DereferenceLinks = True
                openFileDialog1.ShowHelp = True

                _openFileAddOn = New OpenFileAddOn()
                AddHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged

                If openFileDialog1.ShowDialog(_openFileAddOn, Nothing) = Forms.DialogResult.OK Then
                    model1.Clear()
                    Dim readFile = New ReadFile(openFileDialog1.FileName, New MyFileSerializer(CType(_openFileAddOn.ContentOption, contentType)))
                    If OpenSaveAsync Then
                        model1.StartWork(readFile)
                        SetButtonEnabled(False)
                    Else
                        readFile.DoWork()
                        AddToScene(readFile)
                        ShowLog(readFile)
                    End If
                End If

                RemoveHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged
                _openFileAddOn.Dispose()
                _openFileAddOn = Nothing
            End Using

        End Sub

        Private Sub OpenFileAddOn_EventFileNameChanged(sender As Forms.IWin32Window, filePath As String)
            If System.IO.File.Exists(filePath) Then
                Dim rf As ReadFile = New ReadFile(filePath, True)
                _openFileAddOn.SetFileInfo(rf.GetThumbnail(), rf.GetFileInfo())
            Else
                _openFileAddOn.ResetFileInfo()
            End If
        End Sub

        Private Sub saveButton_Click(ByVal sender As Object, ByVal e As EventArgs)

            Using saveFileDialog = New Forms.SaveFileDialog()
                Using saveFileAddOn = New SaveFileAddOn()

                    saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
                    saveFileDialog.AddExtension = True
                    saveFileDialog.CheckPathExists = True
                    saveFileDialog.ShowHelp = True

                    If saveFileDialog.ShowDialog(saveFileAddOn, Nothing) = Forms.DialogResult.OK Then
                        Dim writeFile = New WriteFile(New WriteFileParams(model1) With {.Content = CType(saveFileAddOn.ContentOption, contentType), .SerializationMode = CType(saveFileAddOn.SerialOption, serializationType), .SelectedOnly = saveFileAddOn.SelectedOnly, .Purge = saveFileAddOn.Purge, .Tag = MyFileSerializer.CustomTag}, saveFileDialog.FileName, New MyFileSerializer())
                        If OpenSaveAsync Then
                            model1.StartWork(writeFile)
                            SetButtonEnabled(False)
                        Else
                            writeFile.DoWork()
                            ShowLog("WriteFile log", writeFile.Log)
                        End If
                    End If
                End Using
            End Using

        End Sub

        Private _prevAction As actionType = actionType.None
        Private Sub selectChk_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)

            If selectChk.IsChecked Then
                _prevAction = model1.ActionMode
                model1.ActionMode = actionType.SelectVisibleByPick
            Else
                model1.ActionMode = _prevAction
            End If

        End Sub

        Private Sub dumpButton_Click(ByVal sender As Object, ByVal e As EventArgs)

            Dim entList As Entity() = model1.Entities.ToArray()

            For i = 0 To entList.Length - 1
                Dim ent As Entity = entList(i)

                Dim df As DetailsWindow

                Dim sb = New StringBuilder()
#If NURBS Then
                If TypeOf ent Is Brep Then
                    Dim solid3D As Brep = CType(ent, Brep)
                    Select Case model1.SelectionFilterMode
                        Case selectionFilterType.Vertex
                            For j As Integer = 0 To solid3D.Vertices.Length - 1
                                Dim sv As Brep.Vertex = CType(solid3D.Vertices(j), Brep.Vertex)

                                If solid3D.GetVertexSelection(j) Then
                                    sb.AppendLine("Vertex ID: " & j)
                                    sb.AppendLine(sv.ToString())
                                    sb.AppendLine("----------------------")
                                    sb.Append(sv.Dump())
                                    Exit For
                                End If
                            Next
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

                    End Select

                    If sb.Length > 0 Then
                        df = New DetailsWindow()

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

                    df.contentTextBox.Text = sb.ToString()

                    df.Show()

                    Exit For
                End If
            Next

        End Sub

        Private Sub statsButton_Click(ByVal sender As Object, ByVal e As EventArgs)

            Dim rf = New DetailsWindow()
            rf.Title = "Statistics"
            rf.contentTextBox.Text = model1.Entities.GetStats(True, True, True)
            rf.Show()
        End Sub
    End Class
End Namespace