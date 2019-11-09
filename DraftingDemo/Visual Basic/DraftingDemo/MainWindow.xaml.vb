Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
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
Imports devDept.CustomControls
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Translators
Imports devDept.Geometry
Imports devDept.Serialization
Imports Microsoft.Win32
Imports Cursors = System.Windows.Input.Cursors
Imports Environment = devDept.Eyeshot.Environment
Imports System.Windows.Forms


''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Public Property Layers() As ObservableCollection(Of ListViewModelItem)
    Private _tangentsWindow As TangentsWindow
    Public Sub New()
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.  

        ' Event handlers            
        AddHandler tableTabControl.LayerListView.SelectedIndexChanged, AddressOf SelectionChanged
        AddHandler model1.SelectionChanged, AddressOf model1_SelectionChanged
        AddHandler model1.WorkCompleted, AddressOf model1_WorkCompleted
        AddHandler model1.WorkCancelled, AddressOf model1_WorkCancelled
        AddHandler model1.WorkFailed, AddressOf model1_WorkFailed
        AddHandler model1.CameraMoveBegin, AddressOf model1_CameraMoveBegin

        AddHandler endRadioButton.Checked, AddressOf radioButtons_CheckedChanged
        AddHandler midRadioButton.Checked, AddressOf radioButtons_CheckedChanged
        AddHandler cenRadioButton.Checked, AddressOf radioButtons_CheckedChanged
        AddHandler pointRadioButton.Checked, AddressOf radioButtons_CheckedChanged
        AddHandler quadRadioButton.Checked, AddressOf radioButtons_CheckedChanged

#If Not NURBS Then
			extendButton.IsEnabled = False
			trimButton.IsEnabled = False
			filletButton.IsEnabled = False
			chamferButton.IsEnabled = False
			splineButton.IsEnabled = False
#End If
        tableTabControl.FocusProperties(Nothing)
    End Sub

#If SETUP Then
		Private ReadOnly _helper As New BitnessAgnostic()
#End If

    Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)
        model1.Layers.First().LineWeight = 2
        model1.Layers.First().Color = MyModel.DrawingColor
        model1.Layers.TryAdd(New Layer("Dimensions", System.Drawing.Color.ForestGreen))
        model1.Layers.TryAdd(New Layer("Reference geometry", System.Drawing.Color.Red))
        tableTabControl.Environment = model1
        model1.ActiveLayerName = model1.Layers.First().Name

        ' enables FastZPR when the scene exceeds 3000 objects
        model1.Turbo.MaxComplexity = 3000
        _maxComplexity = model1.Turbo.MaxComplexity

        selectionComboBox.SelectedIndex = 0

        rendererVersionStatusLabel.Text = model1.RendererVersion.ToString()

        model1.SetView(viewType.Top)

        model1.Invalidate()

        model1.Focus()
        EnableControls(False)

#If SETUP Then
			Dim fileName As String = String.Format("{0}\Eyeshot {1} {2} Samples\dataset\Assets\Misc\app8.dwg", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _helper.Edition, _helper.Version.Major)
			Dim ra As ReadFileAsync = _helper.GetReadAutodesk(model1, fileName)
#Else
        Dim ra As New ReadAutodesk("../../../../../../dataset/Assets/Misc/app8.dwg")
        ra.HatchImportMode = ReadAutodesk.hatchImportType.BlockReference
#End If
        model1.StartWork(ra)

        MyBase.OnContentRendered(e)
    End Sub

#Region "Hide/Show"

    Private Sub showOriginButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.GetOriginSymbol().Visible = showOriginButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showExtentsButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.BoundingBox.Visible = showExtentsButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showVerticesButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.ShowVertices = showVerticesButton.IsChecked.Value
        model1.Invalidate()
    End Sub

    Private Sub showGridButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.GetGrid().Visible = showGridButton.IsChecked.Value
        model1.Invalidate()
    End Sub
#End Region

#Region "Selection"
    Private Sub selectionComboBox_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        groupButton.IsEnabled = True

        If selectCheckBox.IsChecked.Value Then
            Selection()
        End If
    End Sub

    Private Sub selectCheckBox_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        groupButton.IsEnabled = True

        If selectCheckBox.IsChecked IsNot Nothing AndAlso selectCheckBox.IsChecked.Value Then
            ClearPreviousSelection()
            Selection()
        Else
            model1.ActionMode = actionType.None
        End If
    End Sub

    Private Sub Selection()
        Select Case selectionComboBox.SelectedIndex
            Case 0 ' by pick
                model1.ActionMode = actionType.SelectByPick

            Case 1 ' by box
                model1.ActionMode = actionType.SelectByBox

            Case 2 ' by poly
                model1.ActionMode = actionType.SelectByPolygon

            Case 3 ' by box enclosed
                model1.ActionMode = actionType.SelectByBoxEnclosed

            Case 4 ' by poly enclosed
                model1.ActionMode = actionType.SelectByPolygonEnclosed

            Case 5 ' visible by pick dynamic
                model1.ActionMode = actionType.SelectVisibleByPickDynamic

            Case Else
                model1.ActionMode = actionType.None
        End Select
    End Sub

    Private Sub clearSelectionButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        If model1.ActionMode = actionType.SelectVisibleByPickLabel Then

            model1.Viewports(0).Labels.ClearSelection()

        Else

            model1.Entities.ClearSelection()
        End If

        model1.Invalidate()
    End Sub

    Private Sub invertSelectionButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        If model1.ActionMode = actionType.SelectVisibleByPickLabel Then

            model1.Viewports(0).Labels.InvertSelection()

        Else

            model1.Entities.InvertSelection()
        End If

        model1.Invalidate()
    End Sub

    Private Sub groupButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.GroupSelection()
    End Sub
#End Region

#Region "Editing"

    Private Sub deleteButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.Entities.DeleteSelected()
        model1.Invalidate()
    End Sub

    Private Sub explodeButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        For i As Integer = model1.Entities.Count - 1 To 0 Step -1

            Dim ent As Entity = model1.Entities(i)

            If ent.Selected Then
                If TypeOf ent Is BlockReference Then

                    model1.Entities.RemoveAt(i)

                    Dim br As BlockReference = CType(ent, BlockReference)

                    Dim entList() As Entity = model1.Entities.Explode(br)

                    model1.Entities.AddRange(entList)


                ElseIf TypeOf ent Is CompositeCurve Then

                    model1.Entities.RemoveAt(i)

                    Dim cc As CompositeCurve = CType(ent, CompositeCurve)

                    model1.Entities.AddRange(cc.Explode())


                ElseIf ent.GroupIndex > -1 Then
                    model1.Ungroup(ent.GroupIndex)
                End If
            End If
        Next i

        model1.Invalidate()
    End Sub

    Private Sub trimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingTrim = True
        model1.waitingForSelection = True
    End Sub

    Private Sub extendButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingExtend = True
        model1.waitingForSelection = True
    End Sub

    Private Sub offsetButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingOffset = True
        model1.waitingForSelection = True
    End Sub

    Private Sub mirrorButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingMirror = True
        model1.waitingForSelection = True
    End Sub

    Private Sub filletButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingFillet = True
        model1.waitingForSelection = True
    End Sub

    Private Sub chamferButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.doingChamfer = True
        model1.waitingForSelection = True
    End Sub
    Private Sub moveButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.selEntities.Clear()

        For i As Integer = model1.Entities.Count - 1 To 0 Step -1
            Dim ent As Entity = model1.Entities(i)
            If ent.Selected AndAlso (TypeOf ent Is ICurve OrElse TypeOf ent Is BlockReference OrElse TypeOf ent Is Text OrElse TypeOf ent Is Leader) Then
                model1.selEntities.Add(ent)
            End If
        Next i

        If model1.selEntities.Count = 0 Then
            Return
        End If

        ClearPreviousSelection()
        model1.doingMove = True
        For Each curve As Entity In model1.selEntities
            curve.Selected = True
        Next curve
    End Sub
    Private Sub rotateButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.selEntities.Clear()

        For i As Integer = model1.Entities.Count - 1 To 0 Step -1
            Dim ent As Entity = model1.Entities(i)
            If ent.Selected AndAlso (TypeOf ent Is ICurve OrElse TypeOf ent Is BlockReference OrElse TypeOf ent Is Text OrElse TypeOf ent Is Leader) Then
                model1.selEntities.Add(ent)
            End If
        Next i

        If model1.selEntities.Count = 0 Then
            Return
        End If

        ClearPreviousSelection()
        model1.doingRotate = True
        For Each curve As Entity In model1.selEntities
            curve.Selected = True
        Next curve

    End Sub
    Private Sub scaleButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.selEntities.Clear()

        For i As Integer = model1.Entities.Count - 1 To 0 Step -1
            Dim ent As Entity = model1.Entities(i)
            If ent.Selected AndAlso (TypeOf ent Is ICurve OrElse TypeOf ent Is BlockReference OrElse TypeOf ent Is Text OrElse TypeOf ent Is Leader) Then
                model1.selEntities.Add(ent)
            End If
        Next i

        If model1.selEntities.Count = 0 Then
            Return
        End If

        ClearPreviousSelection()
        model1.doingScale = True
        For Each curve As Entity In model1.selEntities
            curve.Selected = True
        Next curve

    End Sub

#End Region

#Region "Inspection"

    Private inspectVertex As Boolean

    Private Sub pickVertexButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.ActionMode = actionType.None

        inspectVertex = False

        If pickVertexButton.IsChecked.Value Then
            inspectVertex = True

            mainStatusLabel.Content = "Click on the entity to retrieve the 3D coordinates"

        Else
            mainStatusLabel.Content = ""
        End If
    End Sub

    Private Sub Model1_OnMouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)

        ' Checks that we are not using left mouse button for ZPR
        If model1.ActionMode = actionType.None AndAlso e.ChangedButton <> System.Windows.Input.MouseButton.Middle Then

            Dim closest As Point3D = Nothing

            If inspectVertex Then

                If model1.FindClosestVertex(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), 50, closest) <> -1 Then

                    model1.Labels.Add(New devDept.Eyeshot.Labels.LeaderAndText(closest, closest.ToString(), New System.Drawing.Font("Tahoma", 8.25F), MyModel.DrawingColor, New Vector2D(0, 50)))
                End If

            End If

            model1.Invalidate()

        End If
    End Sub

    Private Sub dumpButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        For i As Integer = 0 To model1.Entities.Count - 1
            If model1.Entities(i).Selected Then
                Dim details As String = "Entity ID = " & i & System.Environment.NewLine & "----------------------" & System.Environment.NewLine & model1.Entities(i).Dump()

                Dim rf As New DetailsWindow()

                rf.Title = "Dump"

                rf.contentTextBox.Text = details

                rf.Show()

                Exit For
            End If
        Next i
    End Sub

    Private Sub areaButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim ap As New AreaProperties()

        Dim count As Integer = 0

        For i As Integer = 0 To model1.Entities.Count - 1

            Dim ent As Entity = model1.Entities(i)

            If ent.Selected Then

                Dim itfCurve As ICurve = DirectCast(ent, ICurve)

                If itfCurve.IsClosed Then

                    ap.Add(ent.Vertices)
                End If

                count += 1
            End If

        Next i

        Dim text As New StringBuilder()
        text.AppendLine(count & " entity(ies) selected")
        text.AppendLine("---------------------")

        If ap.Centroid IsNot Nothing Then

            Dim x As Double = Nothing, y As Double = Nothing, z As Double = Nothing
            Dim xx As Double = Nothing, yy As Double = Nothing, zz As Double = Nothing, xy As Double = Nothing, zx As Double = Nothing, yz As Double = Nothing
            Dim world As MomentOfInertia = Nothing, centroid As MomentOfInertia = Nothing

            ap.GetResults(ap.Area, ap.Centroid, x, y, z, xx, yy, zz, xy, zx, yz, world, centroid)

            text.AppendLine("Cumulative area: " & ap.Area & " square " & model1.Units.ToString().ToLower())
            text.AppendLine("Cumulative centroid: " & ap.Centroid.ToString())
            text.AppendLine("Cumulative area moments:")
            text.AppendLine(" First moments")
            text.AppendLine("  x: " & x.ToString("g6"))
            text.AppendLine("  y: " & y.ToString("g6"))
            text.AppendLine("  z: " & z.ToString("g6"))
            text.AppendLine(" Second moments")
            text.AppendLine("  xx: " & xx.ToString("g6"))
            text.AppendLine("  yy: " & yy.ToString("g6"))
            text.AppendLine("  zz: " & zz.ToString("g6"))
            text.AppendLine(" Product moments")
            text.AppendLine("  xy: " & xx.ToString("g6"))
            text.AppendLine("  yz: " & yy.ToString("g6"))
            text.AppendLine("  zx: " & zz.ToString("g6"))
            text.AppendLine(" Area Moments of Inertia about World Coordinate Axes")
            text.AppendLine("  Ix: " & world.Ix.ToString("g6"))
            text.AppendLine("  Iy: " & world.Iy.ToString("g6"))
            text.AppendLine("  Iz: " & world.Iz.ToString("g6"))
            text.AppendLine(" Area Radii of Gyration about World Coordinate Axes")
            text.AppendLine("  Rx: " & world.Rx.ToString("g6"))
            text.AppendLine("  Ry: " & world.Ry.ToString("g6"))
            text.AppendLine("  Rz: " & world.Rz.ToString("g6"))
            text.AppendLine(" Area Moments of Inertia about Centroid Coordinate Axes:")
            text.AppendLine("  Ix: " & centroid.Ix.ToString("g6"))
            text.AppendLine("  Iy: " & centroid.Iy.ToString("g6"))
            text.AppendLine("  Iz: " & centroid.Iz.ToString("g6"))
            text.AppendLine(" Area Radii of Gyration about Centroid Coordinate Axes")
            text.AppendLine("  Rx: " & centroid.Rx.ToString("g6"))
            text.AppendLine("  Ry: " & centroid.Ry.ToString("g6"))
            text.AppendLine("  Rz: " & centroid.Rz.ToString("g6"))

        End If

        Dim rf As New DetailsWindow()

        rf.Title = "Area Properties"

        rf.contentTextBox.Text = text.ToString()

        rf.Show()
    End Sub

#End Region

#Region "File"
    Private _yAxisUp As Boolean = False
    Private _openFileAddOn As OpenFileAddOn

    Private Sub openButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Using openFileDialog = New Forms.OpenFileDialog()
            openFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
            openFileDialog.Multiselect = False
            openFileDialog.AddExtension = True
            openFileDialog.CheckFileExists = True
            openFileDialog.CheckPathExists = True
            openFileDialog.DereferenceLinks = True

            _openFileAddOn = New OpenFileAddOn()
            AddHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged

            If openFileDialog.ShowDialog(_openFileAddOn, Nothing) = System.Windows.Forms.DialogResult.OK Then
                _yAxisUp = False
                model1.Clear()

                EnableControls(False)

#If SETUP Then
					Dim readFile As New ReadFile(openFileDialog.FileName, _helper.GetFileSerializerEx(CType(_openFileAddOn.ContentOption, contentType)))
#Else
                Dim readFile As New ReadFile(openFileDialog.FileName, New FileSerializerEx(CType(_openFileAddOn.ContentOption, contentType)))
#End If
                model1.StartWork(readFile)

            End If

            RemoveHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged
            _openFileAddOn.Dispose()
            _openFileAddOn = Nothing
        End Using
    End Sub

    Private Sub OpenFileAddOn_EventFileNameChanged(ByVal sender As IWin32Window, ByVal filePath As String)
        If System.IO.File.Exists(filePath) Then
            Dim rf As New ReadFile(filePath, True)
            _openFileAddOn.SetFileInfo(rf.GetThumbnail(), rf.GetFileInfo())
        Else
            _openFileAddOn.ResetFileInfo()
        End If
    End Sub

    Private Sub saveButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Using saveFileDialog = New Forms.SaveFileDialog()
            Using saveFileAddOn = New SaveFileAddOn()
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
                saveFileDialog.AddExtension = True
                saveFileDialog.CheckPathExists = True

                If saveFileDialog.ShowDialog(saveFileAddOn, Nothing) = System.Windows.Forms.DialogResult.OK Then
                    EnableControls(False)


                    Dim writeFile As WriteFile = New WriteFile(New WriteFileParams(model1) With {
                        .Content = CType(saveFileAddOn.ContentOption, contentType),
                        .SerializationMode = CType(saveFileAddOn.SerialOption, serializationType),
                        .SelectedOnly = saveFileAddOn.SelectedOnly,
                        .Purge = saveFileAddOn.Purge
                    }, saveFileDialog.FileName, New FileSerializerEx())
                    model1.StartWork(writeFile)
                End If
            End Using
        End Using
    End Sub

    Private Sub importButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Using importFileDialog = New Forms.OpenFileDialog()
            Using importFileAddOn = New ImportFileAddOn()
                importFileDialog.Filter = "CAD drawings (*.dwg)|*.dwg|Drawing Exchange Format (*.dxf)|*.dxf|All compatible file types (*.*)|*.dwg;*.dxf"
                importFileDialog.Multiselect = False
                importFileDialog.AddExtension = True
                importFileDialog.CheckFileExists = True
                importFileDialog.CheckPathExists = True

                If importFileDialog.ShowDialog(importFileAddOn, Nothing) = System.Windows.Forms.DialogResult.OK Then
                    model1.Clear()
                    _yAxisUp = importFileAddOn.YAxisUp

                    EnableControls(False)
#If SETUP Then
					Dim ra As ReadFileAsync = _helper.GetReadAutodesk(model1, importFileDialog.FileName)
#Else
                    Dim ra As New ReadAutodesk(importFileDialog.FileName)
#End If
                    model1.StartWork(ra)
                End If
            End Using
        End Using
    End Sub

    Private Sub exportButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim saveFileDialog As New Forms.SaveFileDialog()
        saveFileDialog.Filter = "CAD drawings (*.dwg)|*.dwg|Drawing Exchange Format (*.dxf)|*.dxf|3D PDF (*.pdf)|*.pdf"
        saveFileDialog.AddExtension = True
        saveFileDialog.CheckPathExists = True

        If saveFileDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            EnableControls(False)
            Dim wfa As WriteFileAsync = Nothing
            Select Case saveFileDialog.FilterIndex
                Case 1, 2
#If SETUP Then
						wfa = _helper.GetWriteAutodesk(model1, saveFileDialog.FileName)
#Else
                    wfa = New WriteAutodesk(New WriteAutodeskParams(model1), saveFileDialog.FileName)
#End If
                Case 3
#If SETUP Then
						wfa = _helper.GetWritePDF(model1, saveFileDialog.FileName)
#Else
                    wfa = New WritePDF(New WritePdfParams(model1, New Size(595, 842), New Rect(10, 10, 575, 822)), saveFileDialog.FileName)
#End If
            End Select

            model1.StartWork(wfa)
        End If
    End Sub

    Private Sub EnableControls(ByVal status As Boolean)
        rightPanel.IsEnabled = status
    End Sub

#End Region

#Region "Event handlers"

    Private _skipZoomFit As Boolean

    Private Sub model1_WorkCompleted(ByVal sender As Object, ByVal e As devDept.Eyeshot.WorkCompletedEventArgs)
        ' checks the WorkUnit type, more than one can be present in the same application 
        If TypeOf e.WorkUnit Is ReadFileAsync Then
            Dim rfa As ReadFileAsync = CType(e.WorkUnit, ReadFileAsync)

            Dim rf As ReadFile = TryCast(e.WorkUnit, ReadFile)
            If rf IsNot Nothing Then
                _skipZoomFit = rf.FileSerializer.FileBody.Camera IsNot Nothing
            Else
                _skipZoomFit = False
            End If

            If rfa.Entities IsNot Nothing AndAlso _yAxisUp Then
                rfa.RotateEverythingAroundX()
            End If

            rfa.AddToScene(model1)

            tableTabControl.Sync()

            If System.IO.Path.GetFileName(rfa.FileName) = "app8.dwg" Then
                For Each ent As Entity In model1.Entities
                    ent.Translate(-170, -400)
                Next
                model1.Entities.Regen()
                model1.Camera.Target = New Point3D(75, 3.5, 288)
                model1.Camera.ZoomFactor = 3
                _skipZoomFit = True
            End If

            If Not _skipZoomFit Then
                model1.SetView(viewType.Top, True, False)
            End If
        End If

        EnableControls(True)
    End Sub

    Private Sub model1_WorkFailed(ByVal sender As Object, ByVal e As WorkFailedEventArgs)
        EnableControls(True)
    End Sub

    Private Sub model1_WorkCancelled(ByVal sender As Object, ByVal e As EventArgs)
        EnableControls(True)
    End Sub

    Private Sub model1_CameraMoveBegin(ByVal sender As Object, ByVal e As Environment.CameraMoveEventArgs)
        ' refresh FastZPR button according to FastZPR enable status.
        UpdateTurboButton()
    End Sub

    Private _maxComplexity As Integer
    Private Sub turboButton_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        turboButton_OnClick()
    End Sub

    Private Sub turboButton_Unchecked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        turboButton_OnClick()
    End Sub
    Private Sub turboButton_OnClick()
        If model1 Is Nothing Then
            Return
        End If

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

    Private Sub websiteButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        System.Diagnostics.Process.Start("www.devdept.com")
    End Sub

    Private Sub model1_SelectionChanged(ByVal sender As Object, ByVal e As Model.SelectionChangedEventArgs)

        Dim count As Integer = 0

        ' counts selected entities
        For Each ent As Entity In model1.Entities

            If ent.Selected Then

                count += 1
            End If
        Next ent

        ' updates count on the status bar
        selectedCountStatusLabel.Text = count.ToString()
        addedCountStatusLabel.Text = e.AddedItems.Count.ToString()
        removedCountStatusLabel.Text = e.RemovedItems.Count.ToString()

    End Sub


    Private Sub radioButtons_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        If endRadioButton.IsChecked IsNot Nothing AndAlso endRadioButton.IsChecked.Value Then

            model1.activeObjectSnap = objectSnapType.End

        ElseIf midRadioButton.IsChecked IsNot Nothing AndAlso midRadioButton.IsChecked.Value Then

            model1.activeObjectSnap = objectSnapType.Mid

        ElseIf cenRadioButton.IsChecked IsNot Nothing AndAlso cenRadioButton.IsChecked.Value Then

            model1.activeObjectSnap = objectSnapType.Center

        ElseIf quadRadioButton.IsChecked IsNot Nothing AndAlso quadRadioButton.IsChecked.Value Then

            model1.activeObjectSnap = objectSnapType.Quad

        ElseIf pointRadioButton.IsChecked IsNot Nothing AndAlso pointRadioButton.IsChecked.Value Then

            model1.activeObjectSnap = objectSnapType.Point
        End If
    End Sub

    Private Sub filletTextBox_OnTextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
        If model1 Is Nothing Then
            Return
        End If

        Dim val As Double = Nothing
        If Double.TryParse(filletTextBox.Text, val) Then
            model1.filletRadius = val
        End If
    End Sub

    Private Sub showCurveDirectionButton_CheckedChanged(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.ShowCurveDirection = showCurveDirectionButton.IsChecked.Value
        model1.Invalidate()
    End Sub

#End Region

#Region "Imaging"

    Private Sub printButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.Print()
    End Sub

    Private Sub printPreviewButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.PrintPreview(New System.Drawing.Size(500, 400))
    End Sub

    Private Sub pageSetupButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.PageSetup()
    End Sub

    Private Sub vectorCopyToClipbardButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.CopyToClipboardVector(False)

        'release mouse capture, otherwise the first mouse click is skipped                        
        vectorCopyToClipbardButton.ReleaseMouseCapture()
    End Sub

    Private Sub vectorSaveButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim mySaveFileDialog As New Microsoft.Win32.SaveFileDialog()

        mySaveFileDialog.Filter = "Enhanced Windows Metafile (*.emf)|*.emf"
        mySaveFileDialog.RestoreDirectory = True

        ' Show save file dialog box            
        If mySaveFileDialog.ShowDialog().Equals(True) Then
            ' To save as dxf/dwg, see the class HiddenLinesViewOnFileAutodesk available in x86 and x64 dlls                
            model1.WriteToFileVector(False, mySaveFileDialog.FileName)

            'release mouse capture, otherwise the first mouse click is skipped                                
            vectorSaveButton.ReleaseMouseCapture()
        End If
    End Sub

#End Region

#Region "Drafting"
    Private Sub pointButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingPoints = True
    End Sub

    Private Sub textButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingText = True
    End Sub

    Private Sub leaderButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingLeader = True
    End Sub

    Private Sub lineButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingLine = True
    End Sub

    Private Sub plineButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingPolyLine = True
    End Sub

    Private Sub arcButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingArc = True
    End Sub

    Private Sub circleButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingCircle = True
    End Sub

    Private Sub splineButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingCurve = True
    End Sub

    Private Sub ellipseButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingEllipse = True
    End Sub

    Private Sub ellipticalArcButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingEllipticalArc = True
    End Sub

    Private Sub compositeCurveButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        model1.CreateCompositeCurve()
    End Sub

    Private Sub tangentsButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)

        _tangentsWindow = New TangentsWindow()
        _tangentsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen
        _tangentsWindow.ShowDialog()
        If _tangentsWindow.DialogResult.Equals(True) Then


            model1.lineTangents = _tangentsWindow.LineTangents
            model1.circleTangents = _tangentsWindow.CircleTangents
            model1.tangentsRadius = _tangentsWindow.TangentRadius
            model1.trimTangent = _tangentsWindow.TrimTangents
            model1.flipTangent = _tangentsWindow.FlipTangents
            ClearPreviousSelection()
            model1.doingTangents = True
            model1.waitingForSelection = True
        End If

    End Sub

#End Region

#Region "Snapping"
    Private Sub objectSnapCheckBox_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        If objectSnapCheckBox.IsChecked.Value Then
            model1.objectSnapEnabled = True
            snapPanel.IsEnabled = True
        Else
            model1.objectSnapEnabled = False
            snapPanel.IsEnabled = False
        End If
    End Sub

    Private Sub gridSnapCheckBox_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        If gridSnapCheckBox.IsChecked.Value Then
            model1.gridSnapEnabled = True
        Else
            model1.gridSnapEnabled = False
        End If
    End Sub
#End Region

#Region "Dimensioning"
    Private Sub linearDimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingLinearDim = True
    End Sub

    Private Sub ordinateVerticalButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingOrdinateDim = True
        model1.drawingOrdinateDimVertical = True
    End Sub

    Private Sub ordinateHorizontalButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingOrdinateDim = True
        model1.drawingOrdinateDimVertical = False
    End Sub

    Private Sub radialDimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingRadialDim = True
        model1.waitingForSelection = True
    End Sub


    Private Sub diametricDimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingDiametricDim = True
        model1.waitingForSelection = True
    End Sub

    Private Sub alignedDimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingAlignedDim = True
    End Sub

    Private Sub angularDimButton_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearPreviousSelection()
        model1.drawingAngularDim = True
        model1.waitingForSelection = True
    End Sub

    Private Sub ClearPreviousSelection()
        model1.SetView(viewType.Top, False, True)
        model1.ClearAllPreviousCommandData()
    End Sub
#End Region

#Region "Layers"


    Private Sub SelectionChanged(ByVal sender As Object, ByVal e As EventArgs)
        If tableTabControl.LayerListView.SelectedItems.Count > 0 Then
            model1.ActiveLayerName = tableTabControl.LayerListView.SelectedItem.Text
        Else ' nothing selected? we force layer zero
            model1.ActiveLayerName = model1.Layers(0).Name
        End If
    End Sub
#End Region

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

    Private Sub Window_Closed(ByVal sender As Object, ByVal e As EventArgs)
        For Each win As Window In Application.Current.Windows
            win.Close()
        Next win
    End Sub
End Class

''' <summary>    
''' This class represent the Model for Layers List.
''' </summary>    
Public Class ListViewModelItem

    Public Sub New(ByVal myLayer As Layer)
        Me.Layer = myLayer
        IsChecked = myLayer.Visible
        ForeColor = RenderContextUtility.ConvertColor(Me.Layer.Color)
    End Sub

    Public Property Layer() As Layer

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

    Public Property IsChecked() As Boolean
End Class
