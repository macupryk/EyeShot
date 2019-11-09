Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports devDept.CustomControls
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Translators
Imports devDept.Geometry
Imports devDept.Graphics
Imports devDept.Serialization
Imports Microsoft.Win32
Imports Brush = System.Windows.Media.Brush
Imports Color = System.Drawing.Color
Imports Region = devDept.Eyeshot.Entities.Region
Imports MColor = System.Windows.Media.Color
Imports Environment = devDept.Eyeshot.Environment

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Private Const Textures As String = "../../../../../../dataset/Assets/Textures/"

    Private _deskBuilder As DeskBuilder
	Private _oldColor As Brush
	Private _blockCallback As Boolean = True
    Private isDrawingModified As Boolean = True
    Private isDrawingToReload As Boolean = True
    Private imported As Boolean = False

    Dim materialsToolTip As ToolTip = New ToolTip()

    Public Sub New()
		InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
        ' drawingsUserControl1.drawings1.Unlock("")
        drawingsUserControl1.drawings1.Turbo.MaxComplexity = Integer.MaxValue
        model1.Turbo.MaxComplexity = drawingsUserControl1.drawings1.Turbo.MaxComplexity

		model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor
		model1.Units = linearUnitsType.Millimeters

		drawingsUserControl1.Model = model1

		AddHandler tabControl1.SelectionChanged, AddressOf TabControl1_SelectedIndexChanged

		AddHandler model1.WorkCompleted, AddressOf Model1OnWorkCompleted
		AddHandler model1.ProgressChanged, AddressOf Model1_ProgressChanged
		AddHandler model1.WorkCancelled, AddressOf Model1_WorkCancelled

		AddHandler drawingsUserControl1.drawings1.WorkCompleted, AddressOf Model1OnWorkCompleted
		AddHandler drawingsUserControl1.drawings1.ProgressChanged, AddressOf Model1_ProgressChanged
		AddHandler drawingsUserControl1.drawings1.WorkCancelled, AddressOf Model1_WorkCancelled

        AddHandler drawingsUserControl1.drawingsPanel1.updateButton.Click, AddressOf UpdateButton_Click

        model1.Rendered.PlanarReflections = True
		model1.Rendered.PlanarReflectionsIntensity = .1F
	End Sub

	Private Sub UpdateButton_Click(ByVal sender As Object, ByVal e As EventArgs)
	    If Not imported Then
		    Dim s = If(drawingsUserControl1.drawings1.GetActiveSheet(), drawingsUserControl1.drawings1.Sheets(0))
		    Dim ld = drawingsUserControl1.AddSampleDimSheet(s, False)
		    Dim i As Integer = 0
		    Do While i < drawingsUserControl1.drawings1.Entities.Count
                If TypeOf drawingsUserControl1.drawings1.Entities(i) Is LinearDim Then
                    drawingsUserControl1.drawings1.Entities.RemoveAt(i)
                    i -= 1
                End If
                i += 1
		    Loop
		    drawingsUserControl1.drawings1.Entities.AddRange(ld)
	    End If
	End Sub

	Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)
		model1.ProgressBar.Visible = False

		model1.GetGrid().Visible = False
		model1.Backface.ColorMethod = backfaceColorMethodType.Cull

		AddMaterials()

		PossibleColors()

		EnableImportExportButtons(False)

		_deskBuilder = New DeskBuilder()
		model1.StartWork(_deskBuilder)

		_blockCallback = False

		MyBase.OnContentRendered(e)
	End Sub


	#Region "Helper"

	Private Shared Function ToMediaColor(ByVal color As Color) As MColor
		Return MColor.FromArgb(color.A, color.R, color.G, color.B)
	End Function

	Private Shared Function ToDrawingColor(ByVal color As MColor) As Color
        Return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)
    End Function

	Private Sub InitializeDrawings()
		drawingsUserControl1.drawings1.Clear()
        drawingsUserControl1.AddSheet("Sheet1", linearUnitsType.Millimeters, formatType.A4_ISO)
        drawingsUserControl1.AddSheet("Sheet2", linearUnitsType.Inches, formatType.A_ANSI)

        ' updates the pattern for the hidden segments.
        drawingsUserControl1.drawings1.LineTypes.AddOrReplace(New LinePattern(drawingsUserControl1.drawings1.HiddenSegmentsLineTypeName, New Single() { 0.8F, -0.4F }))
	End Sub

	Private Sub AddMaterials()
		Dim bmp1 = New Bitmap(Textures & "Oak Bordeaux bright.jpg")
		Dim bmp2 = New Bitmap(Textures & "Lindberg oak.jpg")
		Dim bmp3 = New Bitmap(Textures & "Lambrate.jpg")
		Dim bmp4 = New Bitmap(Textures & "Oak dark.jpg")
		Dim bmp5 = New Bitmap(Textures & "Oak Torino.jpg")
		Dim bmp6 = New Bitmap(Textures & "Sonoma oak gray.jpg")

		bmp1.RotateFlip(RotateFlipType.Rotate90FlipNone)
		bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone)
		bmp3.RotateFlip(RotateFlipType.Rotate90FlipNone)
		bmp4.RotateFlip(RotateFlipType.Rotate90FlipNone)
		bmp5.RotateFlip(RotateFlipType.Rotate90FlipNone)
		bmp6.RotateFlip(RotateFlipType.Rotate90FlipNone)

		Dim mat1 = New Material(Mat1MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp1)
		Dim mat2 = New Material(Mat2MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp2)
		Dim mat3 = New Material(Mat3MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp3)
		Dim mat4 = New Material(Mat4MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp4)
		Dim mat5 = New Material(Mat5MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp5)
		Dim mat6 = New Material(Mat6MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp6)

		model1.Materials.Add(mat1)
		model1.Materials.Add(mat2)
		model1.Materials.Add(mat3)
		model1.Materials.Add(mat4)
		model1.Materials.Add(mat5)
		model1.Materials.Add(mat6)

		'layer for foots
		model1.Layers.Add(New Layer("foots"))
	End Sub

	Private Sub PossibleColors()
        Dim converter = New System.Windows.Media.BrushConverter()

        materialsToolTip.Content = "R20074"
        comboBoxVeneer.ToolTip = materialsToolTip

        'set the default color for the frame
        _oldColor = DirectCast(converter.ConvertFromString("#8B8C7A"), Brush)
		'paint color
		greenColorRadioButton.Background = DirectCast(converter.ConvertFromString("#8B8C7A"), Brush) 'Stone gray
		Dim greenRadioButtonToolTip = New ToolTip With {.Content = "Stone gray, RAL 7030"}
		greenColorRadioButton.ToolTip = greenRadioButtonToolTip

		orangeColorRadioButton.Background = DirectCast(converter.ConvertFromString("#193737"), Brush) 'Pearly opal green
		Dim orangeRadioButtonToolTip = New ToolTip With {.Content = "Pearly opal green, RAL 6036"}
		orangeColorRadioButton.ToolTip = orangeRadioButtonToolTip

		blueColorRadioButton.Background = DirectCast(converter.ConvertFromString("#F6F6F6"), Brush) 'White traffic
		Dim blueRadioButtonToolTip = New ToolTip With {.Content = "White traffic, RAL 9016"}
		blueColorRadioButton.ToolTip = blueRadioButtonToolTip

		pinkColorRadioButton.Background = DirectCast(converter.ConvertFromString("#EAE6CA"), Brush) 'White pearl
		Dim pinkRadioButtonToolTip = New ToolTip With {.Content = "White pearl, RAL 1013"}
		pinkColorRadioButton.ToolTip = pinkRadioButtonToolTip

		'end caps color
		Dim blackRadioButtonToolTip = New ToolTip With {.Content = "Black, RAL 9005"}
		blackColorRadioButton.Background = DirectCast(converter.ConvertFromString("#0A0A0A"), Brush) 'Deep black
		blackColorRadioButton.ToolTip = blackRadioButtonToolTip

		Dim whiteRadioButtonToolTip = New ToolTip With {.Content = "Pure White, RAL 9010"}
		whiteColorRadioButton.ToolTip = blackRadioButtonToolTip
	End Sub

	Private Shared Function HexToColor(ByVal hex As String) As Color
		If hex.StartsWith("#") Then
			hex = hex.Substring(1)
		End If

		If hex.Length <> 6 Then
			Throw New Exception("Hex color not valid")
		End If

		Return Color.FromArgb(Integer.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber), Integer.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber), Integer.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber))
	End Function

	'Materials
	Private Const Mat1MatName As String = "Oak Bordeaux bright"
	Private Const Mat2MatName As String = "Lindberg oak"
	Private Const Mat3MatName As String = "Lambrate"
	Private Const Mat4MatName As String = "Oak dark"
	Private Const Mat5MatName As String = "Oak Torino"
	Private Const Mat6MatName As String = "Sonoma oak gray"

	Private Sub ReloadDrawings()

		If tabControl1.SelectedIndex = 1 Then 'I'm showing the drawings
            If comboBoxActiveObject.SelectedIndex = 2 Then
                drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(3) '1:10
            ElseIf comboBoxActiveObject.SelectedIndex = 4 Then
                drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(2) '1:5
            Else
                drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(5) '1:50
            End If

            'add drawings
            InitializeDrawings()
			UpdateLinearDim()

			'rebuild
			drawingsUserControl1.drawings1.Rebuild(model1, True, False)
		    isDrawingModified = False
		    isDrawingToReload = False
		End If
	End Sub


	'Frame color modifier
	Private Sub ChangeColor(ByVal c As Brush, ByVal foots As Boolean)
		Dim converter = New System.Windows.Media.BrushConverter()
		If foots Then 'get foot color if necessary
			c = If(blackColorRadioButton.IsChecked IsNot Nothing AndAlso CBool(blackColorRadioButton.IsChecked), DirectCast(converter.ConvertFromString("#0A0A0A"), Brush), DirectCast(converter.ConvertFromString("#FFFFFF"), Brush))
		End If
		For Each block In model1.Blocks
			If String.CompareOrdinal(block.Name, "Top") = 0 Then
				Continue For
			End If
			For Each entity In block.Entities
#If NURBS Then
				If Not (TypeOf entity Is Brep) Then
					Continue For
				End If
#Else
				If Not (TypeOf entity Is Mesh) Then
					Continue For
				End If
#End If
				If foots AndAlso String.CompareOrdinal(entity.LayerName, "foots") = 0 OrElse Not foots AndAlso String.CompareOrdinal(entity.LayerName, "foots") <> 0 Then
					entity.Color = ToDrawingColor(CType(c, SolidColorBrush).Color)
				End If
			Next entity
		Next block

		model1.Entities.Regen()
		model1.Refresh()
	End Sub


	Private Function CalcWeight() As Double()
		Const woodDensity As Double = 0.7 * 1e-3, plasticDensity As Double = 1.4 * 1e-3, steelDensity As Double = 7.8 * 1e-3

		Dim steel As Double = 0, wood As Double = 0, plastic As Double = 0, totalWeight As Double = 0
		For Each b In model1.Blocks
			For Each ent In b.Entities

				Dim mp = New VolumeProperties()
#If NURBS Then
				Dim tempVar As Boolean = TypeOf ent Is Brep
				Dim brep As Brep = If(tempVar, CType(ent, Brep), Nothing)
				If Not (tempVar) Then
					Continue For
				End If
				Dim meshes = brep.GetPolygonMeshes()
				For Each m In meshes
					mp.Add(m.Vertices, m.Triangles)
				Next m
#Else
				Dim tempVar2 As Boolean = TypeOf ent Is Mesh
				Dim m As Mesh = If(tempVar2, CType(ent, Mesh), Nothing)
				If Not (tempVar2) Then
					Continue For
				End If
				mp.Add(m.Vertices, m.Triangles)
#End If

				If b.Name = "Top" Then
					'wood
					wood += mp.Volume * woodDensity
				ElseIf b.Name = "Foot" Then
					'plastic
					plastic += mp.Volume * plasticDensity
				Else
					'steel
					steel += mp.Volume * steelDensity
				End If
			Next ent
		Next b

		totalWeight = steel + wood + plastic
		Return {totalWeight, plastic, wood, steel}
	End Function

	Private Sub GetUserDefinedDimensions(ByVal builder As DeskBuilder)
		If comboBoxWidth.SelectedItem IsNot Nothing Then
			builder.Width = Integer.Parse(CType(comboBoxWidth.SelectedItem, ComboBoxItem).Content.ToString())
		End If

		If comboBoxHeigth.SelectedItem IsNot Nothing Then
			builder.Height = Integer.Parse(CType(comboBoxHeigth.SelectedItem, ComboBoxItem).Content.ToString())
			builder.Height -= 4.75 + 25 'desk top height
		End If

		If comboBoxDepth.SelectedItem IsNot Nothing Then
			builder.TableTopDepth = Integer.Parse(CType(comboBoxDepth.SelectedItem, ComboBoxItem).Content.ToString())
		End If

		builder.Depth = builder.TableTopDepth * 2 + 70
		If builder.SingleTable Then
			builder.Depth = builder.TableTopDepth
		End If

		Dim selectedMat = comboBoxVeneer.Text
		If selectedMat IsNot Nothing Then
			builder.SelMat = selectedMat.ToString()
		End If
	End Sub

	'Reload all the scene
	Private Sub ReloadScene()
		'disable buttons
		EnableImportExportButtons(False)
		'set to All
		RemoveHandler comboBoxActiveObject.SelectionChanged, AddressOf ComboBoxActiveObject_OnSelectionChanged
		comboBoxActiveObject.SelectedIndex = 0
		AddHandler comboBoxActiveObject.SelectionChanged, AddressOf ComboBoxActiveObject_OnSelectionChanged
		'store old color
		_oldColor = New SolidColorBrush(ToMediaColor(model1.Blocks(0).Entities(0).Color))
		'remove old blocks
		model1.Blocks.Clear()
		'update user defined fields
		GetUserDefinedDimensions(_deskBuilder)
		'rebuild the desk
		model1.StartWork(_deskBuilder)
	End Sub

	#End Region

	#Region "MainLogic"

	Private Class DeskBuilder
		Inherits WorkUnit

		Public SingleTable As Boolean = False

		Private _frameBlock, _holderBlock, _topBlock, _footBlock As Block

		Public Width As Double = 1600, Height As Double = 740 - 4.75 - 25, TableTopDepth As Double = 800, Depth As Double = 1670, LegWidth As Double = 70

		Public SelMat As String = "Oak Bordeaux bright"

#If NURBS Then
		Public _top As Brep
#Else
		Public _top As Mesh
#End If

		#Region "Logo"
#If NURBS Then
		'Regions for the DevDept logo
		Private Shared Function LogoRegions() As Region()
			Dim entList = New List(Of ICurve)()
			Dim pw0 = New Point4D(18){
				New Point4D(335.788091517279,-256.5327,0,1),
				New Point4D(336.053758010568,-256.926267,0,1),
				New Point4D(388.669329681387,-334.872943,0,1),
				New Point4D(391.642592606276,-339.328100500001,0,1),
				New Point4D(394.374609537259,-343.37788,0,1),
				New Point4D(399.12596191723,-341.0402095,0,1),
				New Point4D(399.149889916625,-341.0283,0,1),
				New Point4D(399.352172411515,-340.9385005,0,1),
				New Point4D(439.515229896911,-323.106495,0,1),
				New Point4D(461.831087833165,-312.727500500001,0,1),
				New Point4D(483.959979774142,-302.365381499999,0,1),
				New Point4D(487.123272694231,-287.8552225,0,1),
				New Point4D(487.138687693841,-287.7822,0,1),
				New Point4D(487.179053692822,-287.6075835,0,1),
				New Point4D(495.180139490697,-252.988104,0,1),
				New Point4D(496.935492946353,-243.776402,0,1),
				New Point4D(498.651558903002,-234.727123,0,1),
				New Point4D(498.659192402809,-232.1893875,0,1),
				New Point4D(498.659187402809,-232.1768,0,1)
			}
			Dim u0 = New Double(22){ 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6}
			entList.Add(New Curve(3, u0, pw0))
			Dim pw1 = New Point4D(39){
				New Point4D(498.659187402809,-232.1768,0,1),
				New Point4D(495.910444972248,-244.324762,0,1),
				New Point4D(469.762880132791,-255.1633665,0,1),
				New Point4D(469.630888136126,-255.2178,0,1),
				New Point4D(469.450350640686,-255.286921,0,1),
				New Point4D(433.602130046289,-269.012957,0,1),
				New Point4D(413.265886060025,-277.017522,0,1),
				New Point4D(413.125407563574,-277.073069,0,1),
				New Point4D(405.436501257812,-280.1481525,0,1),
				New Point4D(399.483438408199,-282.5834885,0,1),
				New Point4D(399.449202409064,-282.59769,0,1),
				New Point4D(399.447277409113,-282.5977095,0,1),
				New Point4D(399.431234409518,-282.6043465,0,1),
				New Point4D(397.110549468143,-283.5464345,0,1),
				New Point4D(395.408665511137,-283.6283925,0,1),
				New Point4D(394.678800529575,-283.605612,0,1),
				New Point4D(394.64888303033,-283.604505,0,1),
				New Point4D(394.621097031032,-283.6044855,0,1),
				New Point4D(394.594534031703,-283.6015995,0,1),
				New Point4D(394.537151033153,-283.5986055,0,1),
				New Point4D(394.491709534301,-283.596695,0,1),
				New Point4D(394.448410035395,-283.5936385,0,1),
				New Point4D(394.052800045389,-283.558473,0,1),
				New Point4D(393.677315054874,-283.4978835,0,1),
				New Point4D(393.328320063691,-283.414495,0,1),
				New Point4D(393.302195564351,-283.40919,0,1),
				New Point4D(393.278785064942,-283.4023005,0,1),
				New Point4D(393.252588065604,-283.3950555,0,1),
				New Point4D(392.869759075275,-283.2996585,0,1),
				New Point4D(392.518719084143,-283.175746,0,1),
				New Point4D(392.188243592491,-283.038671,0,1),
				New Point4D(390.840508626538,-282.4383445,0,1),
				New Point4D(389.810468152559,-281.430575,0,1),
				New Point4D(389.255666166575,-280.7951835,0,1),
				New Point4D(389.219777667481,-280.7506475,0,1),
				New Point4D(382.663502833107,-272.5628525,0,1),
				New Point4D(382.630390333943,-272.5215,0,1),
				New Point4D(382.355587840885,-272.17963,0,1),
				New Point4D(327.94469421542,-204.48937,0,1),
				New Point4D(327.669891722362,-204.1475,0,1)
			}
			Dim u1 = New Double(43){ 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 12, 12, 13, 13, 13, 13}
			entList.Add(New Curve(3, u1, pw1))
			Dim pw2 = New Point4D(12){
				New Point4D(372.117190599529,-204.1475,0,1),
				New Point4D(372.177061098016,-204.1270895,0,1),
				New Point4D(384.03141979855,-200.0858105,0,1),
				New Point4D(384.193338845552,-200.034106,0,1),
				New Point4D(404.452696451485,-193.817484,0,1),
				New Point4D(415.341845943872,-189.8545415,0,1),
				New Point4D(444.368183826766,-211.630902,0,1),
				New Point4D(473.221427545421,-233.4641235,0,1),
				New Point4D(449.928863133841,-240.712943,0,1),
				New Point4D(449.810488636831,-240.749,0,1),
				New Point4D(449.658775140664,-240.804105,0,1),
				New Point4D(419.619502899519,-251.714895,0,1),
				New Point4D(419.467789403352,-251.77,0,1)
			}
			Dim u2 = New Double(16){ 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4}
			entList.Add(New Curve(3, u2, pw2))
			Dim pt3 = New Point3D(1){
				New Point3D(327.669891722362,-204.1475,0),
				New Point3D(335.788091517279,-256.5327,0)
			}
			entList.Add(New LinearPath(pt3))
			Dim pt4 = New Point3D(1){
				New Point3D(419.467789403352,-251.77,0),
				New Point3D(372.117190599529,-204.1475,0)
			}
			entList.Add(New LinearPath(pt4))



			Dim contours() As ICurve = UtilityEx.GetConnectedCurves(entList, 0.1)
			Dim regions() As Region = UtilityEx.DetectRegionsFromContours(contours, 0.1, Plane.XY)
			Return regions
		End Function
#End If
        #End Region

#Region "Builders"
#If NURBS Then
        Private Shared Function FrameBuilder(ByVal width As Double, ByVal height As Double, ByVal tableTopDepth As Double, ByVal singleTable As Boolean) As List(Of Brep)
#Else
        Private Shared Function FrameBuilder(ByVal width As Double, ByVal height As Double, ByVal tableTopDepth As Double, ByVal singleTable As Boolean) As List(Of Mesh)
#End If

            '70 is the space betwee the two desks
            Dim depth = tableTopDepth * 2 + 70
            'with a single table the depth of the desk it's the same as the depth of th table
            If singleTable Then
                depth = tableTopDepth
            End If

            'leg
            Const legDepth As Integer = 40
            Const legWidth As Integer = 70

            'junction
            Const junDepth As Integer = 30
            Const junHeight As Integer = 50

            'leg
            Dim legOuter = CompositeCurve.CreateRoundedRectangle(legWidth, legDepth, 5)
            legOuter.Translate(0, -legDepth)
            Dim legRegion = legOuter.OffsetToRegion(-2, 0, True)

            Dim legRail = New CompositeCurve(New ICurve() {
                New Line(Point3D.Origin, New Point3D(0, 0, height - legDepth - 10)),
                New Line(New Point3D(0, 0, height - legDepth - 10), New Point3D(0, depth - legDepth * 2, height - legDepth - 10)),
                New Line(New Point3D(0, depth - legDepth * 2, height - legDepth - 10), New Point3D(0, depth - legDepth * 2, 0))
            })

#If NURBS Then
            Dim leg = legRegion.SweepAsBrep(legRail, 0.1)
#Else
            Dim leg = legRegion.SweepAsMesh(legRail, 0.1)
#End If
            leg.Translate(0, legDepth)

            Dim rightPlane = New Plane(New Point3D(0, depth / 2, 0), Vector3D.AxisX, Vector3D.AxisZ)
            Dim rightMirr = New Mirror(rightPlane)
            Dim frontPlane = New Plane(New Point3D(width / 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ)
            Dim frontMirr = New Mirror(frontPlane)

#If NURBS Then
            'screw leg junction
            Dim screwHole = New Circle(Plane.ZY, 5.5)
            Dim screwReg = New Region(screwHole)
            screwReg.Translate(legWidth + 1, ((tableTopDepth / 4) + 50), height - 30)

            'holes
            leg.ExtrudeRemove(screwReg, 4)
            screwReg.Translate(0, 100, 0)
            leg.ExtrudeRemove(screwReg, 4)
            screwReg.Translate(0, 100, 0)
            leg.ExtrudeRemove(screwReg, 4)
            'if it's not a single table do 3 more holes
            If Not singleTable Then
                screwReg.TransformBy(rightMirr)
                leg.ExtrudeRemove(screwReg, -4)
                screwReg.Translate(0, 100, 0)
                leg.ExtrudeRemove(screwReg, -4)
                screwReg.Translate(0, 100, 0)
                leg.ExtrudeRemove(screwReg, -4)
            End If

            Dim legFront = DirectCast(leg.Clone(), Brep)
#Else
            Dim legFront = DirectCast(leg.Clone(), Mesh)
#End If
            legFront.TransformBy(frontMirr)

            'junctions
            Dim junctionOuter = CompositeCurve.CreateRoundedRectangle(junHeight, junDepth, 5)
            junctionOuter.Rotate(Math.PI / 2, Vector3D.AxisY)
            Dim junctionRegion = junctionOuter.OffsetToRegion(-2, 0, True)
#If NURBS Then
            Dim junction = junctionRegion.ExtrudeAsBrep(width - (legWidth * 2))
#Else
            Dim junction = junctionRegion.ExtrudeAsMesh(width - (legWidth * 2), 0.1, Mesh.natureType.Plain)
#End If

            junction.Translate(legWidth, (tableTopDepth / 4) - (junDepth \ 2), height)

#If NURBS Then
            'screw junction junctions
            '2 circles
            Dim screwHoleJunctionUp = New Circle(Plane.XY, 3.5)
            Dim screwHoleJunctionDown = DirectCast(screwHoleJunctionUp.Offset(5, Vector3D.AxisZ), Circle)
            'holes
            Dim screwRegJunctionUp = New Region(screwHoleJunctionUp)
            Dim screwRegJunctionDown = New Region(screwHoleJunctionDown)
            'move the regions (1)
            screwRegJunctionUp.Translate(100, (tableTopDepth / 4), height)
            screwRegJunctionDown.Translate(100, (tableTopDepth / 4), height - junHeight)
            'holes
            junction.ExtrudeRemove(screwRegJunctionUp, -2)
            junction.ExtrudeRemove(screwRegJunctionDown, 2)
            'move the regions (2)
            screwRegJunctionUp.Translate(200, 0, 0)
            screwRegJunctionDown.Translate(200, 0, 0)
            'holes
            junction.ExtrudeRemove(screwRegJunctionUp, -2)
            junction.ExtrudeRemove(screwRegJunctionDown, 2)
            'move the regions (3)
            screwRegJunctionUp.TransformBy(frontMirr)
            screwRegJunctionDown.TransformBy(frontMirr)
            'holes
            junction.ExtrudeRemove(screwRegJunctionUp, 2)
            junction.ExtrudeRemove(screwRegJunctionDown, -2)
            'move the regions (4)
            screwRegJunctionUp.Translate(200, 0, 0)
            screwRegJunctionDown.Translate(200, 0, 0)
            'holes
            junction.ExtrudeRemove(screwRegJunctionUp, 2)
            junction.ExtrudeRemove(screwRegJunctionDown, -2)

            Dim innerJunction = DirectCast(junction.Clone(), Brep)
#Else
            Dim innerJunction = DirectCast(junction.Clone(), Mesh)
#End If
            innerJunction.Translate(0, (tableTopDepth / 4) * 2)

#If NURBS Then
            Dim result = New List(Of Brep)
#Else
            'INSTANT VB TODO TASK: Statements that are interrupted by preprocessor statements are not converted by Instant VB:
            Dim result = New List(Of Mesh)
#End If
            result.Add(junction)
            result.Add(innerJunction)
            result.Add(leg)
            result.Add(legFront)

            If singleTable Then
                Return result
            End If
            'Mirror for the other two
#If NURBS Then
            Dim junctionRx = DirectCast(junction.Clone(), Brep)
            Dim innerJunctionRx = DirectCast(innerJunction.Clone(), Brep)
#Else
            Dim junctionRx = DirectCast(junction.Clone(), Mesh)
			Dim innerJunctionRx = DirectCast(innerJunction.Clone(), Mesh)
#End If

            junctionRx.TransformBy(rightMirr)
            innerJunctionRx.TransformBy(rightMirr)

            result.Add(junctionRx)
            result.Add(innerJunctionRx)

            Return result
        End Function

#If NURBS Then
        Private Function HolderBuilder() As Brep
#Else
        Private Function HolderBuilder() As Mesh
#End If
            Dim depth_Conflict As Double = TableTopDepth * 2 + 70

            'Holder
            'Logo side
            Dim leftSide = New Line(New Point3D(0, 0, 8), New Point3D(0, 0, 519 - 16))
            Dim arcTopLeft = New Arc(New Point3D(16, 0, 519 - 16), leftSide.EndPoint, New Point3D(16, 0, 519))
            Dim topSide = New Line(arcTopLeft.EndPoint, New Point3D(250 - 16, 0, 519))
            Dim verticalMirr = New Mirror(New Plane(New Point3D(250 \ 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ))
            Dim arcTopRight = DirectCast(arcTopLeft.Clone(), Arc)
            arcTopRight.TransformBy(verticalMirr)
            Dim rightSide = DirectCast(leftSide.Clone(), Line)
            rightSide.TransformBy(verticalMirr)
            Dim bottomSide = New Line(leftSide.StartPoint, rightSide.StartPoint)
            Dim frontPanel = New CompositeCurve(leftSide, arcTopLeft, topSide, arcTopRight, rightSide, bottomSide)
            Dim frontPanelReg = New Region(frontPanel)
#If NURBS Then
            Dim frontPanelModel = frontPanelReg.ExtrudeAsBrep(4)

            'Logo
            For Each item In LogoRegions()
                item.Rotate(Math.PI / 2, Vector3D.AxisX)
                item.Rotate(Math.PI, Vector3D.AxisZ)
                item.Scale(0.55)
                item.Translate(350, -3, 300)
                frontPanelModel.ExtrudeRemove(item, 10)
            Next item

            'holes
            Dim topHole = CompositeCurve.CreateRoundedRectangle(40, 10, 2)
            topHole.Rotate(Math.PI / 2, Vector3D.AxisX)
            Dim topHoleReg = New Region(topHole)
            topHoleReg.Translate((250 \ 2) - 20, 0, 519 - 60 - 10)
            frontPanelModel.ExtrudeRemove(topHoleReg, -4)
            topHoleReg.Translate(0, 0, -18)
            frontPanelModel.ExtrudeRemove(topHoleReg, -4)
            'screw holes
            Dim screwHoleHolder = New Circle(Plane.XZ, 6.5)
            Dim screwHoleHolderReg = New Region(screwHoleHolder)
            Dim screwHoleHolderCentralReg = DirectCast(screwHoleHolderReg.Clone(), Region)
            screwHoleHolderReg.Translate(25, 0, 519 - 16)
            frontPanelModel.ExtrudeRemove(screwHoleHolderReg, -4)
            screwHoleHolderReg.TransformBy(verticalMirr)
            frontPanelModel.ExtrudeRemove(screwHoleHolderReg, 4)
            screwHoleHolderCentralReg.Translate(250 \ 2, 0, 519 - 16)
            frontPanelModel.ExtrudeRemove(screwHoleHolderCentralReg, -4)
#Else
			Dim frontPanelModel = frontPanelReg.ExtrudeAsMesh(4,0.1,Mesh.natureType.Plain)
#End If


            Dim basement = New Line(New Point3D(0, -4), New Point3D(250, -4))

            'Base side
            'start from left side
            Dim baseConj = New Line(New Point3D(0, -4), New Point3D(0, -6))
            Dim bottArcTopSx = New Arc(New Point3D(-5, -6, 0), 5, -Math.PI / 2)
            Dim bottArcTopLeftExt = New Line(bottArcTopSx.EndPoint, New Point3D(-15, -10, 0))
            Dim bottArcTopLeftOut = New Arc(New Point3D(-15, -15, 0), 5, Math.PI / 2, Math.PI)
            Dim leftSideBott = New Line(bottArcTopLeftOut.EndPoint, New Point3D(-20, -145, 0))
            Dim bottArcBotLeft = New Arc(New Point3D(-5, -145, 0), 15, Math.PI, 3 * (Math.PI / 2))
            Dim bottLineBot = New Line(bottArcBotLeft.EndPoint, New Point3D(255, -160, 0))

            'mirrored parts
            Dim mirrZero = DirectCast(bottArcTopSx.Clone(), Arc)
            mirrZero.TransformBy(verticalMirr)
            Dim mirrTwo = DirectCast(bottArcTopLeftOut.Clone(), Arc)
            mirrTwo.TransformBy(verticalMirr)
            Dim mirrFour = DirectCast(bottArcBotLeft.Clone(), Arc)
            mirrFour.TransformBy(verticalMirr)
            Dim mirrOne = DirectCast(bottArcTopLeftExt.Clone(), Line)
            mirrOne.TransformBy(verticalMirr)
            Dim mirrThree = DirectCast(leftSideBott.Clone(), Line)
            mirrThree.TransformBy(verticalMirr)
            Dim mirrFive = DirectCast(baseConj.Clone(), Line)
            baseConj.TransformBy(verticalMirr)

            Dim botObj = New CompositeCurve(basement, baseConj, bottArcTopSx, bottArcTopLeftExt, bottArcTopLeftOut, leftSideBott, bottArcBotLeft, bottLineBot, mirrZero, mirrOne, mirrTwo, mirrThree, mirrFour, mirrFive)

            Dim baseBotReg = New Region(botObj)

#If NURBS Then
            Dim baseBot = baseBotReg.ExtrudeAsBrep(4)

            'Hole
            Dim baseHole = CompositeCurve.CreateRoundedRectangle(40, 10, 2)
            Dim baseHoleReg = New Region(baseHole)
            baseHoleReg.Translate((255 \ 2) - 20, -164 + 18)
            baseBot.ExtrudeRemove(baseHoleReg, 4)
#Else
			Dim baseBot = baseBotReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain)
#End If

            'Junction
            Dim bottomRect = CompositeCurve.CreateRectangle(250, 4)
            Dim bottRectReg = New Region(bottomRect)
#If NURBS Then
            Dim bottomConj = bottRectReg.RevolveAsBrep(-Math.PI / 2, Vector3D.AxisX, New Point3D(250 \ 2, -4, 0))
#Else
			Dim bottomConj = bottRectReg.RevolveAsMesh(0, -Math.PI / 2, Vector3D.AxisX, New Point3D(250 \ 2, -4, 0), 20, 0.1, Mesh.natureType.Plain)
#End If
            bottomConj.Translate(0, 0, 8)


#If NURBS Then
            baseBot.Add(bottomConj, frontPanelModel)
#Else
			baseBot.MergeWith(bottomConj)
			baseBot.MergeWith(frontPanelModel)
#End If
            Return baseBot
        End Function

#If NURBS Then
        Private Shared Function FootBuilder() As Brep
            Dim footBase = Brep.CreateBox(70, 40, 4.75)
            Dim cc = CompositeCurve.CreateRoundedRectangle(66, 36, 4)
            Dim footInnerRegion = cc.OffsetToRegion(-4, 0, False)
            Dim footInner = footInnerRegion.ExtrudeAsBrep(20)
            footInner.Translate(2, 2, 4.75)
            footBase.Add(footInner)
            Return footBase
        End Function
#Else
		Private Shared Function FootBuilder() As List(Of Mesh)
			Dim footBase = Mesh.CreateBox(70, 40, 4.75)
			Dim cc = CompositeCurve.CreateRoundedRectangle(66, 36, 4)
			Dim footInnerRegion = cc.OffsetToRegion(-4, 0, False)
			Dim footInner = footInnerRegion.ExtrudeAsMesh(20, 0.1, Mesh.natureType.Plain)
			footInner.Translate(2, 2, 4.75)
			Return New List(Of Mesh)() From {footBase, footInner}
		End Function
#End If

#If NURBS Then
        Private Shared Function TopBuilder(ByVal width_Conflict As Double, ByVal height_Conflict As Double, ByVal tableTopDepth_Conflict As Double) As Brep
#Else
		Private Shared Function TopBuilder(ByVal width As Double, ByVal height As Double, ByVal tableTopDepth As Double) As Mesh
#End If
            'var depth = tableTopDepth * 2 + 70;
            Dim tableTopHeigth = 25
#If NURBS Then

            Dim frontPlane = New Plane(New Point3D(width_Conflict / 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ)
            Dim frontMirr = New Mirror(frontPlane)
            'Table top
            Dim tableTopBox = Brep.CreateBox(width_Conflict, tableTopDepth_Conflict, tableTopHeigth)

            'screw holes
            Dim tableTopHole = New Circle(Plane.XY, 4)
            Dim tableTopHoleReg = New Region(tableTopHole)
            tableTopHoleReg.Translate(100, (tableTopDepth_Conflict / 4), 0)

            'Extrude remove pattern
            tableTopBox.ExtrudeRemovePattern(tableTopHoleReg, 14, (tableTopDepth_Conflict / 4), 2, (tableTopDepth_Conflict / 2), 2)
            tableTopHoleReg.TransformBy(frontMirr)
            tableTopBox.ExtrudeRemovePattern(tableTopHoleReg, -14, (tableTopDepth_Conflict / 4), 2, (tableTopDepth_Conflict / 2), 2)
#Else
            Dim tableTopBox = Mesh.CreateBox(width, tableTopDepth, tableTopHeigth)
#End If

            Return tableTopBox
        End Function

#End Region

        Protected Overrides Sub DoWork(ByVal worker As System.ComponentModel.BackgroundWorker, ByVal doWorkEventArgs As System.ComponentModel.DoWorkEventArgs)
            'initialize progress
            UpdateProgress(0, 100, "Rebuilding desk", worker)

            Dim structureColor As Color = HexToColor("8B8C7A"), footColor As Color = Color.White

            UpdateProgress(20, 100, "Creating blocks", worker)

            _frameBlock = New Block("Frame", linearUnitsType.Millimeters)
            _holderBlock = New Block("Holder", linearUnitsType.Millimeters)
            _topBlock = New Block("Top", linearUnitsType.Millimeters)
            _footBlock = New Block("Foot", linearUnitsType.Millimeters)

            UpdateProgress(40, 100, "Creating Frame", worker)
            'Frame
            For Each item In FrameBuilder(Width, Height, TableTopDepth, SingleTable)
                item.ColorMethod = colorMethodType.byEntity
                item.Color = structureColor
                _frameBlock.Entities.Add(item)
            Next item

            UpdateProgress(60, 100, "Creating Holder", worker)
            'Holder
#If NURBS Then
            Dim holder As Brep = HolderBuilder()
#Else
			Dim holder As Mesh = HolderBuilder()
#End If
            holder.ColorMethod = colorMethodType.byEntity
            holder.Color = structureColor
            _holderBlock.Entities.Add(holder)

            UpdateProgress(80, 100, "Creating Foot", worker)
            'Foot
#If NURBS Then
            Dim foot As Brep = FootBuilder()
            foot.LayerName = "foots"
            foot.ColorMethod = colorMethodType.byEntity
            foot.Color = footColor
            _footBlock.Entities.Add(foot)
#Else
			Dim foot As List(Of Mesh) = FootBuilder()
			For Each m As Mesh In foot
				m.LayerName = "foots"
				m.ColorMethod = colorMethodType.byEntity
				m.Color = footColor
				_footBlock.Entities.Add(m)
			Next m
#End If


            UpdateProgressTo100("Creating Top", worker)
            'Top
            _top = TopBuilder(Width, Height, TableTopDepth)
            _top.ColorMethod = colorMethodType.byEntity
            _topBlock.Entities.Add(_top)

            'selected material
#If NURBS Then
            _top.MaterialName = SelMat
#Else
			_top.ApplyMaterial(SelMat, textureMappingType.Cubic, .5, .5)
#End If
        End Sub

        Protected Overrides Sub WorkCompleted(ByVal environment As Environment)
            environment.Blocks.Add(_frameBlock)
            environment.Blocks.Add(_holderBlock)
            environment.Blocks.Add(_topBlock)
            environment.Blocks.Add(_footBlock)

            If Not SingleTable Then
                environment.Entities.Add(New BlockReference(0, Depth - TableTopDepth, Height + 4.75, "Top", 0))
                environment.Entities.Add(New BlockReference(LegWidth + 4, Depth - ((((TableTopDepth / 4) + 50)) + 250 - 25), Height - 519 - 10 - 4 + 4.75, "Holder", Math.PI / 2))
            End If
            environment.Entities.Add(New BlockReference(0, 0, 4.75, "Frame", 0))
            environment.Entities.Add(New BlockReference(0, 0, Height + 4.75, "Top", 0))
            environment.Entities.Add(New BlockReference(Width - LegWidth - 4, (((TableTopDepth / 4) + 50)) + 250 - 25, Height - 519 - 10 - 4 + 4.75, "Holder", -Math.PI / 2))
            environment.Entities.Add(New BlockReference(0, 0, 0, "Foot", 0))
            environment.Entities.Add(New BlockReference(Width - LegWidth, 0, 0, "Foot", 0))
            environment.Entities.Add(New BlockReference(0, Depth - 40, 0, "Foot", 0))
            environment.Entities.Add(New BlockReference(Width - LegWidth, Depth - 40, 0, "Foot", 0))

            'change texture proportions
            Dim facesToScale() As Integer = {0, 2, 4, 5}
            For Each face In facesToScale
#If NURBS Then
                CType(_top.Faces(face).Tessellation(0), Brep.TessellationMesh).TextureScaleV *= 25.0F / CSng(TableTopDepth) 'y scale
#End If
            Next face

            'update top data display
            environment.Entities.Regen()

            ' sets trimetric view
            environment.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            environment.ZoomFit()
        End Sub

    End Class
#End Region

    #Region "UserInput"

    Public Sub ChangeWeight()
        Dim weights = CalcWeight()
        totalTextBox.Text = (Math.Round(weights(0) / 1000)) & " kg"
        plasticTextBox.Text = Math.Round(weights(1)) & " g"
        woodTextBox.Text = Math.Round(weights(2) / 1000) & " kg"
        steelTextBox.Text = Math.Round(weights(3) / 1000) & " kg"
    End Sub

    Public Sub UpdateLinearDim()
        If tabControl1.SelectedIndex <> 1 Then
            Return
        End If
        For Each sheet In drawingsUserControl1.drawings1.Sheets
            model1.Entities.UpdateBoundingBox()
            Dim ld1 = drawingsUserControl1.AddSampleDimSheet(sheet, False)
            Dim j As Integer = 0
            Do While j < sheet.Entities.Count
                If TypeOf sheet.Entities(j) Is LinearDim Then
                    sheet.Entities.RemoveAt(j)
                    j -= 1
                End If
                j += 1
            Loop
            sheet.Entities.AddRange(ld1)
        Next sheet

        If drawingsUserControl1.drawings1.Entities.Count <= 0 Then
            Return
        End If
        model1.Entities.UpdateBoundingBox()
        Dim s = If(drawingsUserControl1.drawings1.GetActiveSheet(), drawingsUserControl1.drawings1.Sheets(0))
        Dim ld = drawingsUserControl1.AddSampleDimSheet(s, False)
        Dim i As Integer = 0
        Do While i < drawingsUserControl1.drawings1.Entities.Count
            If TypeOf drawingsUserControl1.drawings1.Entities(i) Is LinearDim Then
                drawingsUserControl1.drawings1.Entities.RemoveAt(i)
                i -= 1
            End If
            i += 1
        Loop
        drawingsUserControl1.drawings1.Entities.AddRange(ld)
    End Sub

    'Radio button Event Handler
    Private Sub PaintColorClick(ByVal sender As Object, ByVal e As EventArgs)
        isDrawingToReload = True
        Dim rb = DirectCast(sender, RadioButton)
        ChangeColor(rb.Background, False)
        ReloadDrawings()
    End Sub

    Private Sub PaintColorFootsClick(ByVal sender As Object, ByVal e As EventArgs)
        isDrawingToReload = True
        Dim rb = DirectCast(sender, RadioButton)
        ChangeColor(rb.Background, True)
        ReloadDrawings()
    End Sub

    Private Sub ComboBoxVeneer_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If _blockCallback Then
            Return
        End If
        isDrawingToReload = True
        Select Case comboBoxVeneer.SelectedIndex
            Case 1
                materialsToolTip.Content = "R20021"
            Case 2
                materialsToolTip.Content = "R20090"
            Case 3
                materialsToolTip.Content = "R20033"
            Case 4
                materialsToolTip.Content = "R20231"
            Case 5
                materialsToolTip.Content = "R20039"
            Case Else
                materialsToolTip.Content = "R20074"
        End Select
        For Each block In model1.Blocks
            If String.CompareOrdinal(block.Name, "Top") <> 0 Then
                Continue For
            End If
            For Each entity In block.Entities
#If NURBS Then
                If Not (TypeOf entity Is Brep) Then
                    Continue For
                End If
#Else
				If Not (TypeOf entity Is Mesh) Then
					Continue For
				End If
#End If
                'selected material
                Dim selMat = "Oak Bordeaux bright"
                Dim selectedMat = DirectCast(e.AddedItems(0), ComboBoxItem).Content
                If selectedMat IsNot Nothing Then
                    selMat = selectedMat.ToString()
                End If
                entity.MaterialName = selMat
            Next entity
        Next block

        model1.Entities.Regen()
        model1.Invalidate()
        ReloadDrawings()
    End Sub

    Private Sub ComboBoxFormat_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If _blockCallback Then
            Return
        End If
        isDrawingModified = True
        _deskBuilder.SingleTable = comboBoxFormat.SelectedIndex = 1
        ReloadScene()
    End Sub

    Private Sub ComboBoxDim_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If _blockCallback Then
            Return
        End If
        isDrawingModified = True
        ReloadScene()
    End Sub

    Private Sub ComboBoxActiveObject_OnSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If _blockCallback Then
            Return
        End If
        isDrawingModified = True
        EnableImportExportButtons(False)
        model1.Entities.Clear()
        Select Case comboBoxActiveObject.SelectedIndex
            Case 1 'Frame
                model1.Entities.Add(New BlockReference(0, 0, 0, "Frame", 0))
            Case 2 'Holder
                model1.Entities.Add(New BlockReference(270, 10, 0, "Holder", Math.PI))
            Case 3 'Top
                model1.Entities.Add(New BlockReference(0, 0, 0, "Top", 0))
            Case 4 'Foot
                model1.Entities.Add(New BlockReference(0, 0, 0, "Foot", 0))
            Case Else 'display all
                If Not _deskBuilder.SingleTable Then
                    model1.Entities.Add(New BlockReference(0, _deskBuilder.Depth - _deskBuilder.TableTopDepth, _deskBuilder.Height + 4.75, "Top", 0))
                    model1.Entities.Add(New BlockReference(_deskBuilder.LegWidth + 4, _deskBuilder.Depth - ((((_deskBuilder.TableTopDepth / 4) + 50)) + 250 - 25), _deskBuilder.Height - 519 - 10 - 4 + 4.75, "Holder", Math.PI / 2))
                End If
                model1.Entities.Add(New BlockReference(0, 0, 4.75, "Frame", 0))
                model1.Entities.Add(New BlockReference(0, 0, _deskBuilder.Height + 4.75, "Top", 0))
                model1.Entities.Add(New BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth - 4, (((_deskBuilder.TableTopDepth / 4) + 50)) + 250 - 25, _deskBuilder.Height - 519 - 10 - 4 + 4.75, "Holder", -Math.PI / 2))
                model1.Entities.Add(New BlockReference(0, 0, 0, "Foot", 0))
                model1.Entities.Add(New BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth, 0, 0, "Foot", 0))
                model1.Entities.Add(New BlockReference(0, _deskBuilder.Depth - 40, 0, "Foot", 0))
                model1.Entities.Add(New BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth, _deskBuilder.Depth - 40, 0, "Foot", 0))

                'change texture proportions
                Dim facesToScale() As Integer = {0, 2, 4, 5}
                For Each face In facesToScale
#If NURBS Then
                    CType(_deskBuilder._top.Faces(face).Tessellation(0), Brep.TessellationMesh).TextureScaleV *= 25.0F / CSng(_deskBuilder.TableTopDepth) 'y scale
#End If
                Next face
        End Select

        model1.Entities.Regen()
        model1.Invalidate()
        If tabControl1.SelectedIndex = 0 Then
            model1.ZoomFit()
        End If

        'rebuild drawings
        ReloadDrawings()
        're-enable buttons
        EnableImportExportButtons(True)
    End Sub

#End Region

    #Region "Event handlers"

    Public Sub EnableImportExportButtons(ByVal status As Boolean)
        'disable or enable all buttons, combobox, etc.
        openButton.IsEnabled = status
        saveButton.IsEnabled = status
        exportButton.IsEnabled = status
        importButton.IsEnabled = status
        explodeViewsCheckBox.IsEnabled = status
        greenColorRadioButton.IsEnabled = status
        pinkColorRadioButton.IsEnabled = status
        blueColorRadioButton.IsEnabled = status
        orangeColorRadioButton.IsEnabled = status
        whiteColorRadioButton.IsEnabled = status
        blackColorRadioButton.IsEnabled = status
        comboBoxActiveObject.IsEnabled = status
        comboBoxDepth.IsEnabled = status
        comboBoxFormat.IsEnabled = status
        comboBoxHeigth.IsEnabled = status
        comboBoxVeneer.IsEnabled = status
        comboBoxWidth.IsEnabled = status
        tabControl1.IsEnabled = status
        If Not status Then
            drawingsUserControl1.drawings1.ActionMode = actionType.None
            drawingsUserControl1.drawings1.Cursor = Cursors.Wait
        Else
            drawingsUserControl1.drawings1.ActionMode = actionType.SelectVisibleByPick
        End If
    End Sub

    Private Sub ExportButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim exportFileDialog = New SaveFileDialog()
        exportFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" & "Drawing Exchange Format (*.dxf)|*.dxf"
        exportFileDialog.AddExtension = True
        exportFileDialog.Title = "Export"
        exportFileDialog.CheckPathExists = True
        Dim result = exportFileDialog.ShowDialog()
        If result.Equals(True) Then
            Dim explodeViews = explodeViewsCheckBox.IsChecked = True
            Dim wap As New WriteAutodeskParams(model1, drawingsUserControl1.drawings1, False, explodeViews)
            Dim wa As New WriteAutodesk(wap, exportFileDialog.FileName)
            model1.StartWork(wa)

            EnableImportExportButtons(False)
        End If
    End Sub

    Private Sub ImportButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim importFileDialog = New OpenFileDialog()
        importFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" & "Drawing Exchange Format (*.dxf)|*.dxf"
        importFileDialog.Multiselect = False
        importFileDialog.AddExtension = True
        importFileDialog.Title = "Import"
        importFileDialog.CheckFileExists = True
        importFileDialog.CheckPathExists = True
        Dim result = importFileDialog.ShowDialog()
        If result.Equals(True) Then
            model1.Clear()
            drawingsUserControl1.Clear()

            Dim ra As New ReadAutodesk(importFileDialog.FileName)
            ra.SkipLayouts = False
            model1.StartWork(ra)

            EnableImportExportButtons(False)
            imported = True
        End If
    End Sub

    Private Sub Model1OnWorkCompleted(ByVal sender As Object, ByVal e As WorkCompletedEventArgs)
        progressBar.Value = 100

        If TypeOf e.WorkUnit Is ReadFileAsyncWithDrawings Then
            Dim rfa = CType(e.WorkUnit, ReadFileAsyncWithDrawings)
            rfa.AddToScene(model1)
            model1.SetView(viewType.Trimetric, True, False)
            Dim drawings As Drawings = drawingsUserControl1.drawings1

            model1.Units = rfa.Units
            rfa.AddToDrawings(drawings)

            ' If there are no sheets adds a default one to have a ready-to-use paper space.
            If drawings.Sheets.Count = 0 Then
                drawingsUserControl1.AddDefaultSheet()
            End If

            If tabControl1.SelectedIndex = 0 Then
                model1.ZoomFit()
                model1.Invalidate()
            Else
                drawingsUserControl1.EnableUIElements(False)

                drawings.Rebuild(model1, True, True)
            End If
            
            EnableImportExportButtons(False)
            openButton.IsEnabled = True
            saveButton.IsEnabled = True
            exportButton.IsEnabled = True
            importButton.IsEnabled = True
            explodeViewsCheckBox.IsEnabled = True
            tabControl1.IsEnabled = True
        ElseIf TypeOf e.WorkUnit Is DeskBuilder Then
            ChangeWeight()
            'set colors
            ChangeColor(_oldColor, False)
            ChangeColor(_oldColor, True)
            'rebuild drawings
            ReloadDrawings()
            're-enable buttons
            EnableImportExportButtons(True)
            If tabControl1.SelectedIndex = 0 Then
                model1.ZoomFit()
            End If
        End If

        progressBar.Value = 0
    End Sub

    Private _openFileAddOn As OpenFileAddOn
    Private Sub OpenButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        Using openFileDialog As Forms.OpenFileDialog = New Forms.OpenFileDialog()
            openFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
            openFileDialog.Multiselect = False
            openFileDialog.AddExtension = True
            openFileDialog.CheckFileExists = True
            openFileDialog.CheckPathExists = True
            openFileDialog.DereferenceLinks = True

            _openFileAddOn = New OpenFileAddOn()
            AddHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged

            If openFileDialog.ShowDialog(_openFileAddOn, Nothing) = Forms.DialogResult.OK Then
                model1.Clear()
                drawingsUserControl1.Clear()

                Dim readFile As New ReadFile(openFileDialog.FileName, New FileSerializerEx(CType(_openFileAddOn.ContentOption, contentType)))
                model1.StartWork(readFile)

                EnableImportExportButtons(False)
            End If

            RemoveHandler _openFileAddOn.EventFileNameChanged, AddressOf OpenFileAddOn_EventFileNameChanged
            _openFileAddOn.Dispose()
            _openFileAddOn = Nothing
        End Using
    End Sub

    Private Sub OpenFileAddOn_EventFileNameChanged(ByVal sender As Forms.IWin32Window, ByVal filePath As String)
        If System.IO.File.Exists(filePath) Then
            Dim rf As New ReadFile(filePath, True)
            _openFileAddOn.SetFileInfo(rf.GetThumbnail(), rf.GetFileInfo())
        Else
            _openFileAddOn.ResetFileInfo()
        End If
    End Sub

    Private Sub SaveButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        Using saveFileDialog As Forms.SaveFileDialog = New Forms.SaveFileDialog()
            Using saveDialogCtrl = New SaveFileAddOn()
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye"
                saveFileDialog.AddExtension = True
                saveFileDialog.CheckPathExists = True
                saveFileDialog.ShowHelp = True

                If saveFileDialog.ShowDialog(saveDialogCtrl, Nothing) = Forms.DialogResult.OK Then
                    Dim writeFile As WriteFile = New WriteFile(New WriteFileParams(model1, drawingsUserControl1.drawings1) With {
                                                                  .Content = CType(saveDialogCtrl.ContentOption, contentType),
                                                                  .SerializationMode = CType(saveDialogCtrl.SerialOption, serializationType),
                                                                  .SelectedOnly = saveDialogCtrl.SelectedOnly,
                                                                  .Purge = saveDialogCtrl.Purge
                                                                  }, saveFileDialog.FileName, New FileSerializerEx())
                    model1.StartWork(writeFile)

                    EnableImportExportButtons(False)
                End If
            End Using
        End Using
    End Sub

    Private Sub Model1_WorkCancelled(ByVal sender As Object, ByVal e As EventArgs)
        progressBar.Value = 0
    End Sub

    Private Sub Model1_ProgressChanged(ByVal sender As Object, ByVal e As devDept.Eyeshot.ProgressChangedEventArgs)
        progressBar.Value = e.Progress
    End Sub


    Private Sub TabControl1_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Not imported And isDrawingModified Then
            ReloadDrawings()
        ElseIf Not imported And isDrawingToReload Then
            drawingsUserControl1.drawings1.Rebuild(model1, True, False)
        Else
            If tabControl1.SelectedIndex = 1 And drawingsUserControl1.drawings1.Sheets.Count > 0 Then
                drawingsUserControl1.drawings1.Rebuild(model1, True, True)
            End If
        End If
    End Sub

#End Region

End Class
