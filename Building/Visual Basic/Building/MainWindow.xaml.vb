Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Labels
Imports devDept.Geometry
Imports devDept.Graphics

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)            
            model1.AutoHideLabels = True

            model1.GetGrid().AutoSize = True
            model1.GetGrid().[Step] = 100

            Dim concreteMatName As String = "Concrete"
            Dim steelMatName As String = "Blue steel"

            model1.Materials.Add(New Material(concreteMatName, System.Drawing.Color.FromArgb(25, 25, 25), System.Drawing.Color.LightGray, System.Drawing.Color.FromArgb(31, 31, 31), 0.05F, 0))
            model1.Materials.Add(New Material(steelMatName, System.Drawing.Color.RoyalBlue))

            ' square column block
            Dim b As New Block("squareCol")

            ' creates a gray box
            Dim m1 As Mesh = Mesh.CreateBox(30, 30, 270)

            m1.ColorMethod = colorMethodType.byEntity
            m1.Color = System.Drawing.Color.Gray
            m1.MaterialName = concreteMatName

            ' adds it to the block
            b.Entities.Add(m1)

            ' creates a new black polyline
            Dim steel As New LinearPath()

            steel.Vertices = New Point3D(7) {}
            steel.Vertices(0) = New Point3D(4, 4, -20)
            steel.Vertices(1) = New Point3D(4, 4, 290)
            steel.Vertices(2) = New Point3D(4, 26, 290)
            steel.Vertices(3) = New Point3D(4, 26, -20)
            steel.Vertices(4) = New Point3D(26, 26, -20)
            steel.Vertices(5) = New Point3D(26, 26, 290)
            steel.Vertices(6) = New Point3D(26, 4, 290)
            steel.Vertices(7) = New Point3D(26, 4, -20)

            steel.ColorMethod = colorMethodType.byEntity
            steel.Color = System.Drawing.Color.Black

            ' adds it to the block
            b.Entities.Add(steel)

            ' creates a price tag
            Dim at As New devDept.Eyeshot.Entities.Attribute(New Point3D(0, -15), "Price", "$25,000", 10)

            ' adds it to the block
            b.Entities.Add(at)

            ' adds the block to the master block dictionary
            model1.Blocks.Add(b)

            ' inserts the "SquareCol" block many times: this is a HUGE memory and graphic resources saving for big models
            Dim reference As BlockReference

            For k As Integer = 0 To 3

                For j As Integer = 0 To 4

                    For i As Integer = 0 To 4

                        If i < 2 AndAlso j < 2 Then

                            System.Diagnostics.Debug.WriteLine("No columns here")
                        Else


                            reference = New BlockReference(i * 500, j * 400, k * 300, "squareCol", 1, 1, _
                                1, 0)

                            ' defines a different price for each one
                            reference.Attributes.Add("Price", "$" + i.ToString() + ",000")


                            model1.Entities.Add(reference)
                        End If
                    Next
                Next
            Next

            ' again as above
            b = New Block("floor")

            Dim width As Double = 2030
            Dim depth As Double = 1630
            Dim dimA As Double = 1000
            Dim dimB As Double = 800
            
            Dim outerPoints As Point2D() = New Point2D(6) {}
            
            outerPoints(0) = New Point2D(0, dimB)
            outerPoints(1) = New Point2D(dimA, dimB)
            outerPoints(2) = New Point2D(dimA, 0)
            outerPoints(3) = New Point2D(width, 0)
            outerPoints(4) = New Point2D(width, depth)
            outerPoints(5) = New Point2D(0, depth)
            outerPoints(6) = DirectCast(outerPoints(0).Clone(), Point2D)
            
            Dim outer As New LinearPath(Plane.XY, outerPoints)
            
            Dim innerPoints As Point2D() = New Point2D(4) {}
            
            innerPoints(0) = New Point2D(1530, 800)
            innerPoints(1) = New Point2D(1530, 950)
            innerPoints(2) = New Point2D(1650, 950)
            innerPoints(3) = New Point2D(1650, 800)
            innerPoints(4) = DirectCast(innerPoints(0).Clone(), Point2D)
            
            Dim inner As New LinearPath(Plane.XY, innerPoints)
            
            Dim reg As New devDept.Eyeshot.Entities.Region(outer, inner)
            
            Dim m2 As Mesh = reg.ExtrudeAsMesh(30, 0.1, Mesh.natureType.Plain)

            m2.ColorMethod = colorMethodType.byEntity
            m2.Color = System.Drawing.Color.White
            m2.MaterialName = concreteMatName

            b.Entities.Add(m2)

            model1.Blocks.Add(b)

            For i As Integer = 0 To 3

                reference = New BlockReference(0, 0, 270 + i * 300, "floor", 1, 1, _
                    1, 0)


                model1.Entities.Add(reference)
            Next

            Dim brickMatName As String = "Wall bricks"

            model1.Materials.Add(New Material(brickMatName, new Bitmap("../../../../../../dataset/Assets/Textures/Bricks.jpg")))

            b = New Block("brickWall")

            Dim rm As Mesh = Mesh.CreateBox(470, 30, 270, Mesh.natureType.RichPlain)

            rm.ApplyMaterial(brickMatName, textureMappingType.Cubic, 1.5, 1.5)

            rm.ColorMethod = colorMethodType.byEntity
            rm.Color = System.Drawing.Color.Chocolate

            b.Entities.Add(rm)

            model1.Blocks.Add(b)

            For j As Integer = 1 To 3

                For i As Integer = 0 To 1

                    reference = New BlockReference(1030 + i * 500, 0, j * 300, "brickWall", 1, 1, _
                        1, 0)


                    model1.Entities.Add(reference)
                Next
            Next


            ' Cylindrical column
            b = New Block("CylindricalCol")

            Dim m3 As Mesh = Mesh.CreateCylinder(20, 270, 32, Mesh.natureType.Smooth)

            m3.ColorMethod = colorMethodType.byEntity
            m3.Color = System.Drawing.Color.RoyalBlue
            m3.MaterialName = steelMatName

            b.Entities.Add(m3)

            model1.Blocks.Add(b)

            For j As Integer = 0 To 1

                For i As Integer = 0 To 2

                    reference = New BlockReference(100 + i * 400, 115 + j * 200, 0, "CylindricalCol", 1, 1, _
                        1, 0)


                    model1.Entities.Add(reference)
                Next
            Next

            ' Roof (not a block this time)
            Dim roof As Mesh = Mesh.CreateBox(880, 280, 30)

            ' Edits vertices to add a slope
            roof.Vertices(4).Z = 15
            roof.Vertices(7).Z = 15

            roof.Translate(60, 75, 270)

            model1.Entities.Add(roof, System.Drawing.Color.DimGray)


            ' Labels            
            Dim lat As New LeaderAndText(New Point3D(0, 800, 1200), "Height = 12 m", New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.White, New Vector2D(0, 20))

            lat.FillColor = System.Drawing.Color.Black
            model1.Labels.Add(lat)


            Dim ff As LeaderAndText

            ff = New LeaderAndText(New Point3D(1000, 1000, 300), "First floor", New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.White, New Vector2D(0, 10))

            ff.FillColor = System.Drawing.Color.Red
            ff.Alignment = System.Drawing.ContentAlignment.BottomCenter

            model1.Labels.Add(ff)

            ' fits the model in the viewport            
            model1.ZoomFit()

            ' refresh the viewport
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace