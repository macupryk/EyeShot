Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
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
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Translators
Imports devDept.Geometry
Imports devDept.Graphics
Imports Microsoft.Win32
Imports Block = devDept.Eyeshot.Block
Imports Environment = devDept.Eyeshot.Environment

''' <summary>
''' Interaction logic for DrawingsUserControl.xaml
''' </summary>
Partial Public Class DrawingsUserControl
	Inherits UserControl

	Private _treeIsDirty As Boolean = True

	''' <summary>
	''' Gets or sets the model.
	''' </summary>
	Public Property Model() As Model

	Public Sub New()
		InitializeComponent()

		' sets data for DrawingsPanel control
		drawingsPanel1.drawings = drawings1

		drawings1.ActionMode = actionType.SelectVisibleByPick

		AddHandler drawings1.WorkCompleted, AddressOf Drawings1OnWorkCompleted

		drawings1.ProgressBar.Visible = False
	End Sub

	Private Sub AddDefaultViews(ByVal sheet As Sheet)
		' this samples uses values in millimeters to add views and it uses this factor to get converted values.
		Dim unitsConversionFactor As Double = Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, sheet.Units)

		Dim scaleFactor As Double = drawingsPanel1.GetScaleComboValue()

		' adds Front vector view
		sheet.Entities.Add(New VectorView(70 * unitsConversionFactor, 230 * unitsConversionFactor, viewType.Top, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Top)))
		' adds Trimetric raster view            
		sheet.Entities.Add(New RasterView(150 * unitsConversionFactor, 230 * unitsConversionFactor, viewType.Trimetric, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Trimetric, True)))
		' adds Top vector view
		sheet.Entities.Add(New VectorView(70 * unitsConversionFactor, 130 * unitsConversionFactor, viewType.Front, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Front)))
		' adds Right vector view
		sheet.Entities.Add(New VectorView(150 * unitsConversionFactor, 130 * unitsConversionFactor, viewType.Right, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Right)))
	End Sub


	Public Function AddSampleDimSheet(ByVal s As Sheet, ByVal addScene As Boolean) As Entity()
		Dim sheet = s
		Dim scaleFactor As Double = drawingsPanel1.GetScaleComboValue()
		Dim unitsConversionFactor As Double = Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, sheet.Units)
		Dim linearDimPos As Point3D = Model.Entities.BoxSize

		' invalid case
		If Double.IsInfinity(linearDimPos.X) OrElse Double.IsInfinity(linearDimPos.Y) OrElse Double.IsInfinity(linearDimPos.Z) Then
			linearDimPos = New Point3D(0, 0, 0)
		End If

		Dim dim1Plane = New Plane(Point3D.Origin, -1 * Vector3D.AxisY, Vector3D.AxisX) 'Plane -Y X;
		Dim toAdd = New List(Of Entity) From {
			New LinearDim(Plane.XY, New Point2D(70 - (linearDimPos.X * scaleFactor) / 2, 230 - (linearDimPos.Y * scaleFactor) / 2), New Point2D(70 + (linearDimPos.X * scaleFactor) / 2, 230 - (linearDimPos.Y * scaleFactor) / 2), New Point2D(70, 220 - (linearDimPos.Y * scaleFactor) / 2), 3),
			New LinearDim(Plane.XY, New Point2D(150 - (linearDimPos.Y * scaleFactor) / 2, 130 - (linearDimPos.Z * scaleFactor) / 2), New Point2D(150 + (linearDimPos.Y * scaleFactor) / 2, 130 - (linearDimPos.Z * scaleFactor) / 2), New Point2D(150, 120 - (linearDimPos.Z * scaleFactor) / 2), 3),
			New LinearDim(New Plane(Point3D.Origin, Vector3D.AxisY, New Vector3D(-1,0,0)), New Point2D(130 - (linearDimPos.Z * scaleFactor) / 2, -70 - (linearDimPos.X * scaleFactor) / 2), New Point2D(130 + (linearDimPos.Z * scaleFactor) / 2, -70 - (linearDimPos.X * scaleFactor) / 2), New Point2D(130, -80 - (linearDimPos.X * scaleFactor) / 2), 3)
		}

		Return AddLinearDimsToSheet(toAdd, sheet, unitsConversionFactor, scaleFactor, addScene)
	End Function

	Private Function AddLinearDimsToSheet(ByVal toAdd As List(Of Entity), ByRef sheet As Sheet, ByVal unitsConversionFactor As Double, ByVal scale As Double, ByVal addScene As Boolean) As Entity()
		For Each ent In toAdd
			Dim tempVar As Boolean = TypeOf ent Is LinearDim
			Dim ld As LinearDim = If(tempVar, CType(ent, LinearDim), Nothing)
			If tempVar Then
				ld.Scale(unitsConversionFactor)
				' sets the same layer as wires segments
				ld.LayerName = drawings1.WiresLayerName
				' sets the linear scale as the inverted of the sheet scale factor.
				ld.LinearScale = 1 / scale
			End If

			If addScene Then
				sheet.Entities.Add(ent)
			End If
		Next ent

		Return toAdd.ToArray()
	End Function


	''' <summary>
	''' Create a new sheet with some default views according to the format type.
	''' </summary>
	''' <param name="name">The name for the sheet.</param>
	''' <param name="units">The measurement system type for the sheet.</param>
	''' <param name="formatType">The <see cref="formatType"/>.</param>        
	''' <remarks>
	''' It builds the format block and it adds the created BlockReference to the Sheet and the block to the Drawings Blocks collection.
	''' </remarks>
	Public Sub AddSheet(ByVal name As String, ByVal units As linearUnitsType, ByVal formatType As formatType, Optional ByVal addDefaultView As Boolean = True)
		Dim size As Tuple(Of Double, Double) = DrawingsPanel.GetFormatSize(units, formatType)
		Dim sheet As New Sheet(units, size.Item1, size.Item2, name)

		Dim block As Block = Nothing
		Dim br As BlockReference = drawingsPanel1.CreateFormatBlock(formatType, sheet, block)
		drawings1.Blocks.Add(block)

		sheet.Entities.Add(br) ' not possible adding the entity to Drawings because the control handle is not created yet. it will be added when this sheet will be set as the active one.
		drawings1.Sheets.Add(sheet)

		' adds a set of default views.
		If addDefaultView Then
			AddDefaultViews(sheet)
		End If
	End Sub

	''' <summary>
	''' Adds a default sheet.
	''' </summary>
	Public Sub AddDefaultSheet()
        AddSheet("Sheet1", linearUnitsType.Millimeters, formatType.A3_ISO, False)
    End Sub

	''' <summary>
	''' Clears the drawings and the treePanel.
	''' </summary>
	Public Sub Clear()
		drawings1.Clear()
		drawingsPanel1.ClearTreeView()
		_treeIsDirty = True
	End Sub

	''' <summary>
	''' Sets the enable status of the input controls.
	''' </summary>
	''' <param name="status"></param>
	Public Sub EnableUIElements(ByVal status As Boolean)
		drawingsPanel1.Enabled = status
		addLinearDimButton.IsEnabled = status
		exportSheetButton.IsEnabled = status
		rebuildButton.IsEnabled = status
		printButton.IsEnabled = status
	End Sub

	#Region "Event Handlers"

	Private Sub DrawingsPanel1OnSelectionChanged(ByVal sender As Object, ByVal e As EntityEventArgs)
		propertyGrid1.SelectedObject = e.Item
	End Sub

	Private Sub DrawingsPanel1OnViewAdded(ByVal sender As Object, ByVal e As EntityEventArgs)
		EnableUIElements(False)
		CType(e.Item, View).Rebuild(Model, drawings1.GetActiveSheet(), drawings1, True)
	End Sub

	Private Sub AddLinearDimButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		If drawings1.DrawingLinearDim Then
			drawings1.DisableDimensioning()
		Else
			drawings1.EnableDimensioning()
		End If
	End Sub

	''' <summary>
	''' Export the active sheet in the model space of the output file.
	''' </summary>
	Private Sub ExportSheetButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim exportFileDialog = New SaveFileDialog()

		exportFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" & "Drawing Exchange Format (*.dxf)|*.dxf"
		exportFileDialog.AddExtension = True
		exportFileDialog.Title = "Export"
		exportFileDialog.CheckPathExists = True

		Dim result = exportFileDialog.ShowDialog()
            If (result = true) Then
				EnableUIElements(False)

				Dim wap As New WriteAutodeskParams(drawings1)
				Dim fileName As String = exportFileDialog.FileName
                Dim wa As WriteAutodesk = New WriteAutodesk(wap, fileName)
				drawings1.StartWork(wa)
			End If

	End Sub

	Public Sub Drawings1OnKeyUp(ByVal sender As Object, ByVal e As KeyEventArgs)
		If e.Key = Key.Delete Then
			drawingsPanel1.PurgeActiveSheet()

			propertyGrid1.SelectedObject = Nothing
		End If
	End Sub

	Public Sub Drawings1OnSelectionChanged(ByVal sender As Object, ByVal e As Environment.SelectionChangedEventArgs)
		Dim selected As Entity = Nothing
		For Each entity In drawings1.Entities
			If entity.Selected Then
				selected = entity ' returns the last object selected
			End If
		Next entity

		propertyGrid1.SelectedObject = selected
	End Sub

	Public Sub Drawings1OnWorkCompleted(ByVal sender As Object, ByVal e As WorkCompletedEventArgs)
		Dim vb As ViewBuilder = TryCast(e.WorkUnit, ViewBuilder)
		If vb IsNot Nothing Then
			vb.AddToDrawings(drawings1)

			If drawings1.GetActiveSheet() Is Nothing Then
				drawingsPanel1.ActivateSheet(drawings1.Sheets(0).Name)

				If _treeIsDirty Then
					drawingsPanel1.SyncTree()
					_treeIsDirty = False
				End If

				drawings1.ZoomFit()
			End If
		End If

		EnableUIElements(True)
	End Sub

	Private Sub PrintButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		If drawings1.PageSetup(True, True, 0) = False Then
			Return
		End If
		drawings1.PrintPreview(New System.Drawing.Size(800, 600))
	End Sub

	Private Sub PropertyGrid1_PropertyValueChanged(ByVal sender As Object, ByVal e As Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs)
		' updates the entities
		drawings1.Entities.Regen()

		' refresh
		drawings1.Invalidate()
	End Sub

	Public Sub RebuildButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		EnableUIElements(False)
		drawings1.GetActiveSheet().Rebuild(Model, drawings1, True) 'reload partially
	End Sub

	#End Region

End Class
