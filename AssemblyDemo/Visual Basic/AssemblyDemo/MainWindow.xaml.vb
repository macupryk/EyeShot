Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Forms
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Eyeshot.Translators
Imports devDept.Serialization
Imports devDept.CustomControls
Imports Environment = devDept.Eyeshot.Environment
Imports KeyEventArgs = System.Windows.Input.KeyEventArgs
Imports SaveFileDialog = Microsoft.Win32.SaveFileDialog

Namespace WpfApplication1
	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Public Sub New()
			InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.  

            ' sets data for AssemblyBrowser control
            assemblyTreeView1.Model = model1
			assemblyTreeView1.InitializeContextMenu()

			' connects assemblyBrowser to MyEntityList of MyModel
			CType(model1.Entities, MyEntityList).assemblyTree = assemblyTreeView1

			' Listens the events to handle the deletion of the selected entity
			AddHandler model1.KeyDown, AddressOf Model1_KeyDown
            AddHandler assemblyTreeView1.KeyDown, AddressOf assemblyTreeView1_KeyDown

            ' helper for Turbo button color
            AddHandler model1.CameraMoveBegin, AddressOf model1_CameraMoveBegin

			' event needed for asynchronous Read/Write
			AddHandler model1.WorkCompleted, AddressOf model1_WorkCompleted

			model1.ObjectManipulator.ShowOriginalWhileEditing = False

			' settings to improve performance for heavy geometry
			model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never
			model1.Rendered.ShadowMode = shadowType.None
			model1.Turbo.OperatingMode = operatingType.Boxes

			' set combobox defaults
			operatingModeComboBox.DataContext = System.Enum.GetValues(GetType(operatingType))
			operatingModeComboBox.SelectedItem = operatingType.Boxes
			lagLabel.Content = ""

			' to be able to get the center of rotation on not current entities 
			model1.WriteDepthForTransparents = True
			model1.Rotate.ShowCenter = True

		    model1.Backface.ColorMethod = backfaceColorMethodType.Cull
		End Sub

        Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)

            ' Model import
            model1.OpenFile("../../../../../../dataset/Assets/AssemblyDemo.eye")

            assemblyTreeView1.PopulateTree(model1.Entities)
            ' sets selection mode
            model1.ActionMode = actionType.SelectVisibleByPick

            ' sets camera orientation
            model1.SetView(viewType.Isometric)

            ' enables Turbo when the scene exceeds 3000 objects
            model1.Turbo.MaxComplexity = 3000
            _maxComplexity = model1.Turbo.MaxComplexity

            ' Fits the model in the viewport
            model1.ZoomFit()
            model1.Invalidate()

            model1.DisplayMode = displayType.Rendered

            MyBase.OnContentRendered(e)
        End Sub

#Region "Selected entity deletion"
        Private Sub assemblyTreeView1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
			CheckDelete(e.KeyCode)
		End Sub

		Private Sub Model1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
			CheckDelete(CType(KeyInterop.VirtualKeyFromKey(e.Key), Keys))
		End Sub

		Private Sub CheckDelete(ByVal e As Keys)
			If e = Keys.Delete Then
				For i = assemblyTreeView1.SelectedNodes.Count - 1 To 0 Step -1
					Dim selectedNode As TreeNode = assemblyTreeView1.SelectedNodes(i)
					If selectedNode IsNot Nothing Then
						If assemblyTreeView1.SelectedItems(i) IsNot Nothing AndAlso assemblyTreeView1.SelectedItems(i).Item IsNot Nothing Then
							Dim entity = TryCast(assemblyTreeView1.SelectedItems(i).Item, Entity)

                            If selectedNode.Parent IsNot Nothing AndAlso selectedNode.Parent.Tag IsNot Nothing Then
                                Dim parent_Renamed = TryCast(DirectCast(selectedNode.Parent.Tag, AssemblyTreeView.NodeTag).Entity, BlockReference)

                                Dim parentBlockName = parent_Renamed.BlockName

                                ' removes the entity from the block where it's present
                                model1.Blocks(parentBlockName).Entities.Remove(entity)
                            Else
                                ' in case the entity to delete is a root level entity
                                model1.Entities.DeleteSelected()
							End If
						End If

					End If

				Next i

				assemblyTreeView1.DeleteSelectedNodes()

				' update selection data
				assemblyTreeView1.SelectedNodes.Clear()
				assemblyTreeView1.SelectedItems.Clear()

                model1.Entities.UpdateBoundingBox()
				model1.Invalidate()
			End If
		End Sub

		#End Region

		Private Sub leafSelCheckBox_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
			'invert assembly selection mode
			model1.AssemblySelectionMode = If(chkLeafSelection.IsEnabled, Environment.assemblySelectionType.Leaf, Environment.assemblySelectionType.Branch)
			model1.Entities.ClearSelection()
			model1.Invalidate()

			assemblyTreeView1.SelectedNodes.Clear()
			assemblyTreeView1.SelectedItems.Clear()
		End Sub

		#Region "Read/Write"
		Private _yAxisUp As Boolean

		Private Sub model1_WorkCompleted(ByVal sender As Object, ByVal e As devDept.Eyeshot.WorkCompletedEventArgs)
			If TypeOf e.WorkUnit Is ReadFileAsync Then
				assemblyTreeView1.ClearCurrent(True)

				Dim ra As ReadFileAsync = CType(e.WorkUnit, ReadFileAsync)

				If _yAxisUp Then
					ra.RotateEverythingAroundX()
				End If

				' updates model units and its related combo box
				If TypeOf e.WorkUnit Is ReadFileAsyncWithBlocks Then
					model1.Units = CType(e.WorkUnit, ReadFileAsyncWithBlocks).Units
				End If

				ra.AddToScene(model1, New RegenOptions() With {.Async = True})
			ElseIf TypeOf e.WorkUnit Is Regeneration Then
				assemblyTreeView1.PopulateTree(model1.Entities)

				model1.Entities.UpdateBoundingBox()
				UpdateFastZprButton()
				model1.ZoomFit()
				model1.Invalidate()
			End If

			openButton.IsEnabled = True
			saveButton.IsEnabled = True
			importButton.IsEnabled = True
			exportButton.IsEnabled = True
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

        Private Sub importButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using importFileDialog1 = New OpenFileDialog()
                Using importFileAddOn = New ImportFileAddOn()
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

                    If importFileDialog1.ShowDialog(importFileAddOn, Nothing) = System.Windows.Forms.DialogResult.OK Then
                        assemblyTreeView1.ClearTree()
                        If (model1.Entities.IsOpenCurrentBlockReference) Then
                            model1.Entities.CloseCurrentBlockReference()
                        End If
                        model1.Clear()

                        _yAxisUp = importFileAddOn.YAxisUp
                        Dim rfa As ReadFileAsync = getReader(importFileDialog1.FileName)

                        If rfa IsNot Nothing Then
                            model1.StartWork(rfa)

                            model1.SetView(viewType.Trimetric, True, model1.AnimateCamera)

                            openButton.IsEnabled = False
                            saveButton.IsEnabled = False
                            importButton.IsEnabled = False
                            exportButton.IsEnabled = False
                        End If
                    End If
                End Using
            End Using
        End Sub

        Private Sub exportButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			Dim saveFileDialog1 As New SaveFileDialog()

            Dim theFilter As String = "WaveFront OBJ (*.obj)|*.obj|" & "Stereolithography (*.stl)|*.stl|" & "Laser LAS (*.las)|*.las|" & "WebGL (*.html)|*.html"

#If NURBS Then
            theFilter += "|STandard for the Exchange of Product (*.step)|*.step|" & "Initial Graphics Exchange Specification (*.iges)|*.iges"
#End If
            saveFileDialog1.Filter = theFilter

			saveFileDialog1.AddExtension = True
			saveFileDialog1.CheckPathExists = True

			Dim result = saveFileDialog1.ShowDialog()

			If result.Value Then
				Dim wfa As WriteFileAsync = Nothing
				Dim dataParams As WriteParams = Nothing
				Select Case saveFileDialog1.FilterIndex
                    Case 1
                        dataParams = New WriteParamsWithMaterials(model1)
                        wfa = New WriteOBJ(CType(dataParams, WriteParamsWithMaterials), saveFileDialog1.FileName)
                    Case 2
                        dataParams = New WriteParams(model1)
                        wfa = New WriteSTL(dataParams, saveFileDialog1.FileName)
                    Case 3
                        dataParams = Nothing
                        wfa = New WriteLAS(TryCast(model1.Entities.Where(Function(x) TypeOf x Is FastPointCloud).FirstOrDefault(), FastPointCloud), saveFileDialog1.FileName)
                    Case 4
                        dataParams = New WriteParamsWithMaterials(model1)
                        wfa = New WriteWebGL(CType(dataParams, WriteParamsWithMaterials), model1.DefaultMaterial, saveFileDialog1.FileName)
#If NURBS Then
                    Case 5
                        dataParams = New WriteParamsWithUnits(model1)
                        wfa = New WriteSTEP(CType(dataParams, WriteParamsWithUnits), saveFileDialog1.FileName)
                    Case 6
                        dataParams = New WriteParamsWithUnits(model1)
                        wfa = New WriteIGES(CType(dataParams, WriteParamsWithUnits), saveFileDialog1.FileName)
#End If
                End Select

				model1.StartWork(wfa)

                openButton.IsEnabled = False
				saveButton.IsEnabled = False
				importButton.IsEnabled = False
				exportButton.IsEnabled = False
			End If
		End Sub

        Private _openFileAddOn As OpenFileAddOn

        Private Sub openButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using openFileDialog1 = New OpenFileDialog()
                openFileDialog1.Filter = "Eyeshot (*.eye)|*.eye"
                openFileDialog1.Multiselect = False
                openFileDialog1.AddExtension = True
                openFileDialog1.CheckFileExists = True
                openFileDialog1.CheckPathExists = True
                openFileDialog1.DereferenceLinks = True

                _openFileAddOn = New OpenFileAddOn()
                AddHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged

                If openFileDialog1.ShowDialog(_openFileAddOn, Nothing) = System.Windows.Forms.DialogResult.OK Then
                    assemblyTreeView1.ClearTree()
                    If (model1.Entities.IsOpenCurrentBlockReference) Then
                        model1.Entities.CloseCurrentBlockReference()
                    End If
                    model1.Clear()

                    _yAxisUp = False
                    Dim readFile As New ReadFile(openFileDialog1.FileName, CType(_openFileAddOn.ContentOption, contentType))
                    model1.StartWork(readFile)
                    model1.SetView(viewType.Trimetric, True, model1.AnimateCamera)
                    openButton.IsEnabled = False
                    saveButton.IsEnabled = False
                    importButton.IsEnabled = False
                    exportButton.IsEnabled = False

                    model1.Invalidate()
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

        Private Sub saveButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using saveFileDialog = New Forms.SaveFileDialog()
                Using saveFileAddOn = New SaveFileAddOn()
                    saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
                    saveFileDialog.AddExtension = True
                    saveFileDialog.CheckPathExists = True

                    If saveFileDialog.ShowDialog(saveFileAddOn, Nothing) = Forms.DialogResult.OK Then
                        Dim writeFile As WriteFile = New WriteFile(New WriteFileParams(model1) With {
                        .Content = CType(saveFileAddOn.ContentOption, contentType),
                        .SerializationMode = CType(saveFileAddOn.SerialOption, serializationType),
                        .SelectedOnly = saveFileAddOn.SelectedOnly,
                        .Purge = saveFileAddOn.Purge
                    }, saveFileDialog.FileName)
                        model1.StartWork(writeFile)
                        openButton.IsEnabled = False
                        saveButton.IsEnabled = False
                        importButton.IsEnabled = False
                        exportButton.IsEnabled = False
                    End If
                End Using
            End Using
        End Sub
#End Region

#Region "Turbo controls"

        Private _maxComplexity As Integer = 3000
		Private Sub turboCheckBox_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
			If turboCheckBox.IsChecked.Value = False Then
				_maxComplexity = model1.Turbo.MaxComplexity
				model1.Turbo.MaxComplexity = Integer.MaxValue
			Else
				model1.Turbo.MaxComplexity = _maxComplexity
			End If

			operatingModeComboBox.IsEnabled = turboCheckBox.IsChecked.Value
			lagLabel.IsEnabled = turboCheckBox.IsChecked.Value

			model1.Entities.UpdateBoundingBox()
			UpdateFastZprButton()
		End Sub

		Private Sub operatingModeComboBox_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
			If model1 IsNot Nothing Then 'with SelectedIndex="3" (see xaml) this is called before the model initialization
				model1.Turbo.OperatingMode = CType(operatingModeComboBox.SelectedIndex, operatingType)
				model1.Entities.UpdateBoundingBox()
				UpdateFastZprButton()
			End If
		End Sub

		Private Sub UpdateFastZprButton()
			If model1.Turbo.Enabled Then
                turboCheckBox.Style = TryCast(FindResource("FastZprToggleButtonStyle"), Style)
                lagLabel.Content = (model1.Turbo.Lag / 1000.0).ToString("f1") & " s"
			Else
				turboCheckBox.Style = TryCast(FindResource("ToggleButtonStyle"), Style)
				lagLabel.Content = String.Empty
			End If
		End Sub

		Private Sub model1_CameraMoveBegin(ByVal sender As Object, ByVal e As Environment.CameraMoveEventArgs)
			UpdateFastZprButton()
		End Sub

		#End Region

	End Class
End Namespace