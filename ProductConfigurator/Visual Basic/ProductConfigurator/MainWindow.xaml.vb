Imports System.Collections.Generic
Imports System.Drawing
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
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Windows.Controls.Primitives
Imports devDept.Eyeshot.Labels
Imports Font = System.Drawing.Font
Imports devDept.Eyeshot.Translators

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    

    Private Const Textures As String = "../../../../../../dataset/Assets/Textures/"

    Const mapleMatName As String = "Maple"
    Const cherryMatName As String = "Cherry"
    Const plasticMatName As String = "Plastic"    

    Private Enum materialEnum
        Maple = 0
        Cherry = 1
    End Enum
    Private currentFrameMaterial As materialEnum

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        model1.GetGrid().Visible = False
        model1.Backface.ColorMethod = backfaceColorMethodType.Cull        

        currentFrameMaterial = materialEnum.Maple

        Dim mapleMat As New Material(mapleMatName, System.Drawing.Color.FromArgb(100, 100, 100), System.Drawing.Color.White, 1, new Bitmap(Textures + "Maple.jpg"))

        mapleMat.Density = 0.7 * 0.001
        ' set maple density
        model1.Materials.Add(mapleMat)

        Dim cherryMat As New Material(cherryMatName, System.Drawing.Color.FromArgb(100, 100, 100), System.Drawing.Color.White, 1, new Bitmap(Textures + "Cherry.jpg"))

        cherryMat.Density = 0.8 * 0.001
        ' set cherry density
        model1.Materials.Add(cherryMat)

        model1.Layers.Add(plasticMatName, System.Drawing.Color.GreenYellow)

        Dim plasticLayerMat As New Material(plasticMatName, System.Drawing.Color.GreenYellow)

        model1.Layers(plasticMatName).MaterialName = plasticMatName

        plasticLayerMat.Density = 1.4 * 0.001
        ' set plastic density
        model1.Materials.Add(plasticLayerMat)

        RebuildChair()

        ' sets trimetric view
        model1.SetView(viewType.Trimetric)

        ' fits the model in the viewport
        model1.ZoomFit()

        ' refresh the viewport
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub seatColor_Click(sender As Object, e As RoutedEventArgs)
        Dim rb As ToggleButton = DirectCast(sender, ToggleButton)

        model1.Layers(plasticMatName).Color = RenderContextUtility.ConvertColor(rb.Background)
        ' affects edges color
        model1.Materials(plasticMatName).Diffuse = RenderContextUtility.ConvertColor(rb.Background)
        ' affects faces color
        model1.Invalidate()
    End Sub

    Private Sub woodEssence_Click(sender As Object, e As RoutedEventArgs)
        If mapleEssenceRadioButton.IsChecked.HasValue AndAlso mapleEssenceRadioButton.IsChecked.Value Then
            currentFrameMaterial = materialEnum.Maple
        ElseIf cherryEssenceRadioButton.IsChecked.HasValue AndAlso cherryEssenceRadioButton.IsChecked.Value Then
            currentFrameMaterial = materialEnum.Cherry
        End If

        RebuildChair()

        model1.Invalidate()
    End Sub

    Private Sub sizeTrackBar_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If model1 Is Nothing Then
            Return
        End If
        model1.Entities.Clear()
        RebuildChair()
        model1.Invalidate()
    End Sub

    Private Sub exportStlButton_Click(sender As Object, e As RoutedEventArgs)
        Dim stlFile As String = "chair.stl"
        Dim ws As New WriteSTL(New WriteParams(model1), stlFile, True)
        ws.DoWork()

        Dim fullPath As String = [String].Format("{0}\{1}", System.Environment.CurrentDirectory, stlFile)
        MessageBox.Show([String].Format("File saved in {0}", fullPath))
    End Sub
    Private Sub exportObjButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim objFile As String = "chair.obj"
        Dim ws As New WriteOBJ(New WriteParamsWithMaterials(model1), objFile)
        ws.DoWork()

        Dim fullPath As String = [String].Format("{0}\{1}", System.Environment.CurrentDirectory, objFile)
        MessageBox.Show([String].Format("File saved in {0}", fullPath))
    End Sub

    Private Sub RebuildChair()
        Dim currentMatName As String = If((currentFrameMaterial = materialEnum.Cherry), cherryMatName, mapleMatName)

        model1.Entities.Clear()
        model1.Labels.Clear()

        Dim legDepth As Double = 3
        Dim legWidth As Double = 3
        Dim legHeight As Double = 56.5

        Dim seatDepth As Double = 29
        Dim seatWidth As Double = sizeTrackBar.Value
        Dim seatHeight As Double = 1.6
        Dim seatY As Double = 27.4

        '
        ' Build the legs
        '
        Dim leg1 As Mesh = MakeBox(legWidth, legDepth, legHeight)
        Dim leg4 As Mesh = DirectCast(leg1.Clone(), Mesh)


        Dim leg2 As Mesh = MakeBox(legWidth, legDepth, seatY)
        Dim leg3 As Mesh = DirectCast(leg2.Clone(), Mesh)

        leg2.Translate(seatDepth - legDepth, 0, 0)
        leg3.Translate(seatDepth - legDepth, seatWidth - legWidth, 0)
        leg4.Translate(0, seatWidth - legWidth, 0)

        AddEntityWithMaterial(leg1, currentMatName)
        AddEntityWithMaterial(leg2, currentMatName)
        AddEntityWithMaterial(leg3, currentMatName)
        AddEntityWithMaterial(leg4, currentMatName)

        '
        ' Build the seat
        '
        Dim dx As Double = 0.3
        Dim dy As Double = 0.2
        Dim delta As Double = 0.1
        Dim seatPartDepth As Double = 4.5
        Dim seatPartOffset As Double = 0.5

        Dim seatFirstPartPoints As Point3D() = {New Point3D(legDepth + delta, 0, 0), New Point3D(seatPartDepth, 0, 0), New Point3D(seatPartDepth, seatWidth, 0), New Point3D(legDepth + delta, seatWidth, 0), New Point3D(legDepth + delta, seatWidth - legWidth + dy, 0), New Point3D(-dx, seatWidth - legWidth, 0), _
            New Point3D(-dx, legWidth, 0), New Point3D(legDepth + delta, legWidth + dy, 0), New Point3D(legDepth + delta, 0, 0)}

        Dim seatFirstPart As New Entities.Region(new LinearPath(seatFirstPartPoints))
        Dim seatPart0 As Mesh = seatFirstPart.ExtrudeAsMesh(New Vector3D(0, 0, seatHeight), 0.01, Mesh.natureType.Smooth)

        seatPart0.Translate(0, -dy, seatY)
        Dim seatPart1 As Mesh = MakeBox(seatWidth + 2 * dx, seatPartDepth, seatHeight)
        seatPart1.Translate(seatPartDepth + seatPartOffset, -dy, seatY)

        Dim seatPart2 As Mesh = DirectCast(seatPart1.Clone(), Mesh)
        seatPart2.Translate(seatPartDepth + seatPartOffset, 0, 0)

        Dim seatPart3 As Mesh = DirectCast(seatPart2.Clone(), Mesh)
        seatPart3.Translate(seatPartDepth + seatPartOffset, 0, 0)

        Dim seatPart4 As Mesh = DirectCast(seatPart3.Clone(), Mesh)
        seatPart4.Translate(seatPartDepth + seatPartOffset, 0, 0)

        Dim seatPart5 As Mesh = DirectCast(seatPart4.Clone(), Mesh)
        seatPart5.Translate(seatPartDepth + seatPartOffset, 0, 0)

        model1.Entities.Add(seatPart0, plasticMatName)
        model1.Entities.Add(seatPart1, plasticMatName)
        model1.Entities.Add(seatPart2, plasticMatName)
        model1.Entities.Add(seatPart3, plasticMatName)
        model1.Entities.Add(seatPart4, plasticMatName)
        model1.Entities.Add(seatPart5, plasticMatName)

        '
        ' Build the bars under the seat
        '
        Dim underSeatXBarWidth As Double = legWidth * 0.8
        Dim underSeatXBarDepth As Double = seatDepth - 2 * legDepth
        Dim underSeatXBarHeight As Double = 5.0

        Dim underSeatYBarWidth As Double = seatWidth - 2 * legWidth
        Dim underSeatYBarDepth As Double = legDepth * 0.8
        Dim underSeatYBarHeight As Double = underSeatXBarHeight

        Dim barUnderSeatLeft As Mesh = MakeBox(underSeatXBarWidth, underSeatXBarDepth, underSeatXBarHeight)
        barUnderSeatLeft.Translate(legDepth, (legWidth - underSeatXBarWidth) / 2, seatY - underSeatXBarHeight)

        Dim barUnderSeatRight As Mesh = DirectCast(barUnderSeatLeft.Clone(), Mesh)
        barUnderSeatRight.Translate(0, seatWidth - legWidth, 0)

        Dim barUnderSeatBack As Mesh = MakeBox(seatWidth - 2 * legWidth, legDepth * 0.8, underSeatYBarHeight)
        barUnderSeatBack.Translate((legDepth - underSeatYBarDepth) / 2, legWidth, seatY - underSeatYBarHeight)

        Dim barUnderSeatFront As Mesh = DirectCast(barUnderSeatBack.Clone(), Mesh)
        barUnderSeatFront.Translate(seatDepth - legDepth, 0, 0)

        AddEntityWithMaterial(barUnderSeatLeft, currentMatName)
        AddEntityWithMaterial(barUnderSeatRight, currentMatName)
        AddEntityWithMaterial(barUnderSeatFront, currentMatName)
        AddEntityWithMaterial(barUnderSeatBack, currentMatName)

        '
        ' Build the two cylinders on the sides
        '
        Dim CylinderRadius As Double = legWidth / 3
        Dim cylinderY As Double = 14.5
        Dim leftCylinder As Mesh = MakeCylinder(CylinderRadius, seatDepth - 2 * legDepth, 16)
        leftCylinder.ApplyMaterial(currentMatName, textureMappingType.Cylindrical, 0.25, 1)
        leftCylinder.Rotate(Math.PI / 2, New Vector3D(0, 1, 0))
        leftCylinder.Translate(legDepth, legWidth / 2, cylinderY)

        model1.Entities.Add(leftCylinder)

        Dim rightCylinder As Mesh = DirectCast(leftCylinder.Clone(), Mesh)
        rightCylinder.Translate(0, seatWidth - legWidth, 0)

        model1.Entities.Add(rightCylinder)


        '
        '  Build the chair back
        '
        Dim chairBackHorizHeight As Double = 4
        Dim chairBackHorizDepth As Double = 2
        Dim horizHeight1 As Double = seatY + seatHeight + 7
        Dim chairBackHorizontal1 As Mesh = MakeBox(seatWidth - 2 * legWidth, chairBackHorizDepth, chairBackHorizHeight)
        chairBackHorizontal1.Translate((legDepth - chairBackHorizDepth) / 2.0, legWidth, horizHeight1)

        Dim cylinderHeight As Double = 12
        Dim horizHeight2 As Double = cylinderHeight + chairBackHorizHeight
        Dim chairBackHorizontal2 As Mesh = DirectCast(chairBackHorizontal1.Clone(), Mesh)
        chairBackHorizontal2.Translate(0, 0, horizHeight2)

        AddEntityWithMaterial(chairBackHorizontal1, currentMatName)
        AddEntityWithMaterial(chairBackHorizontal2, currentMatName)

        Dim chairBackCylinderRadius As Double = chairBackHorizDepth / 4.0
        Dim chairBackCylinderHeight As Double = horizHeight2 - chairBackHorizHeight
        Dim chairBackCylinder As Mesh = MakeCylinder(chairBackCylinderRadius, chairBackCylinderHeight, 16)
        chairBackCylinder.Translate(legDepth / 2.0, legWidth, horizHeight1 + chairBackHorizHeight)

        Dim chairBackWidth As Double = seatWidth - 2 * legWidth
        Dim cylinderOffset As Double = 7
        Dim nCylinders As Integer = CInt(chairBackWidth / cylinderOffset)
        Dim offset As Double = (chairBackWidth - (nCylinders + 1) * cylinderOffset) / 2.0
        offset += cylinderOffset

        Dim i As Integer = 0
        While i < nCylinders
            Dim cyl As Mesh = DirectCast(chairBackCylinder.Clone(), Mesh)
            cyl.ApplyMaterial(currentMatName, textureMappingType.Cylindrical, 0.25, 1)
            cyl.Translate(0, offset, 0)
            model1.Entities.Add(cyl)
            i += 1
            offset += cylinderOffset
        End While

        '
        ' Add the linear dimension
        ' 
        Dim dimCorner As New Point3D(0, 0, legHeight)
        Dim myPlane As Plane = Plane.YZ
        myPlane.Origin = dimCorner

        Dim ad As New LinearDim(myPlane, New Point3D(0, 0, legHeight + 1), New Point3D(0, seatWidth, legHeight + 1), New Point3D(seatDepth + 10, seatWidth / 2, legHeight + 10), 3)

        ad.TextSuffix = " cm"
        model1.Entities.Add(ad)

        model1.Entities.UpdateBoundingBox()

        '
        ' Update extents
        '
        widthTextBox.Text = model1.Entities.BoxSize.X.ToString("f2") + " cm"
        depthTextBox.Text = model1.Entities.BoxSize.Y.ToString("f2") + " cm"
        heightTextBox.Text = model1.Entities.BoxSize.Z.ToString("f2") + " cm"

        '
        ' Update weight
        '      
        Dim totalWeight As Double = CalcWeight()

        weightTextBox.Text = totalWeight.ToString("f2") + " kg"

        '
        ' Product ID label
        '
        Dim prodIdLabel As devDept.Eyeshot.Labels.LeaderAndText = New LeaderAndText(seatDepth / 2, seatWidth, 25, "Product ID goes here", New Font("Tahoma", 8.25F), System.Drawing.Color.Black, _
            New Vector2D(20, 0))
        model1.Labels.Add(prodIdLabel)

    End Sub

    Private Function CalcWeight() As Double

        Dim totalWeight As Double = 0

        For Each ent As Entity In model1.Entities
            ' Volume() method is defined in the IFace interface
            If TypeOf ent Is Mesh Then

                Dim mesh As Mesh = DirectCast(ent, Mesh)

                Dim mp As New VolumeProperties(mesh.Vertices, mesh.Triangles)

                If ent.LayerName = plasticMatName Then
                    ' Gets plastic layer material density

                    totalWeight += model1.Materials(plasticMatName).Density * mp.Volume
                Else

                    If TypeOf ent Is Mesh Then

                        Dim m As Mesh = DirectCast(ent, Mesh)

                        Dim mat As Material = model1.Materials(ent.MaterialName)

                        totalWeight += mat.Density * mp.Volume

                    End If

                End If

            End If
        Next

        Return totalWeight
    End Function

    Private Function MakeBox(Width As Double, Depth As Double, height As Double) As Mesh
        Return Mesh.CreateBox(Depth, Width, height, Mesh.natureType.Smooth)
    End Function

    Private Function MakeCylinder(radius As Double, height As Double, slices As Integer) As Mesh
        Return Mesh.CreateCylinder(radius, height, slices, Mesh.natureType.Smooth)
    End Function

    Private Sub AddEntityWithMaterial(ByRef m As Mesh, matName As String)
        m.ApplyMaterial(matName, textureMappingType.Cubic, 1, 1)
        model1.Entities.Add(m)
    End Sub
End Class

