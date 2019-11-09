Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.IO
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
      
        Dim dirName As String = "myPictures"

        Public Sub New()
            InitializeComponent()
            ' model1.Unlock("")
#if Not NURBS
            groupBox1.IsEnabled = false
#End If 
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' creates the element for the ListView
            CreateElements()

            ' clear entities on the scene
            model1.Entities.Clear()

            model1.Entities.Add(new BlockReference("Box"))

            model1.Entities.Add(new BlockReference(-20, 50, 0, "Cylinder", 0))

            model1.Entities.Add(new BlockReference(60, -15, 0, "Slot", 0))

            model1.Entities.Add(new BlockReference(10, 50, 0, "Triangle", 0))

            model1.Entities.Add(new BlockReference(-30, -30, 0, "Weels", 0))

            ' fills the TreeView with the entities in the scene
            PopulateTree( treeView1  , model1.Entities, model1.Blocks)

            ' creates the arrow to display during moving action
            CreateArrowsDirections()

             ' Fits the model in the scene
            model1.ZoomFit()

            ' refresh the screen
            model1.Invalidate()
                
            MyBase.OnContentRendered(e)
        End Sub

        Private Function GetUniqueEntity(ByVal ent As Entity) As Mesh
            ' creates a unique mesh from the entity in input
            Dim uniqueEntity As Mesh = Nothing

            ' if the entity is a BlockReference, then merges all the entities in its block     
#if NURBS
        If (TypeOf ent Is BlockReference) Then
            Dim ents() As Entity = CType(ent,BlockReference).Explode(model1.Blocks, keepTessellation:= true)
            uniqueEntity = CType(ents(0),Brep).ConvertToMesh(weldNow:= false)
            Dim i As Integer = 1
            Do While (i < ents.Length)
                uniqueEntity.MergeWith(CType(ents(i),Brep).ConvertToMesh(weldNow:= false), false)
                i = (i + 1)
            Loop
            
        Else
            uniqueEntity = CType(CType(ent,Brep).ConvertToMesh.Clone,Mesh)
        End If
        
#Else 
            If (TypeOf ent Is BlockReference) Then
                Dim ents() As Entity = CType(ent,BlockReference).Explode(model1.Blocks, keepTessellation:= true)
                uniqueEntity = CType(ents(0),Mesh)
                Dim i As Integer = 1
                Do While (i < ents.Length)
                    uniqueEntity.MergeWith(CType(ents(i),Mesh), false)
                    i = (i + 1)
                Loop
            
            Else
                uniqueEntity = CType(ent.Clone,Mesh)
            End If
        
#End If 
            ' regens data (if needed) before to add it into TempEntities list
            If (uniqueEntity.RegenMode = regenType.RegenAndCompile) Then
                uniqueEntity.Regen(0.01)
            End If
        
            Return uniqueEntity
        End Function

        #Region "ListView"
        Protected Sub CreateElements()
        ' setting scene for saving
        Dim oldColor As System.Windows.Media.Brush = model1.GetBackground.TopColor
        model1.Backface.ColorMethod = backfaceColorMethodType.SingleColor
        model1.GetBackground.TopColor = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
        model1.Viewports(0).GetToolBar.Visible = false
        model1.GetCoordinateSystemIcon.Visible = false
        model1.GetOriginSymbol.Visible = false
        model1.GetViewCubeIcon.Visible = false
        model1.GetGrid.Visible = false
        model1.Flat.EdgeThickness = 10
        model1.Flat.SilhouetteThickness = 10
        model1.DisplayMode = displayType.Flat
        model1.Flat.ColorMethod = flatColorMethodType.EntityMaterial
        ' sets trimetric view
        model1.SetView(viewType.Trimetric)
        ' creates the directory to save material elements
        If Not Directory.Exists(dirName) Then
            Directory.CreateDirectory(dirName)
        Else
            ' deletes all previous files
            For Each filePath As String In Directory.GetFiles(dirName)
                File.Delete(filePath)
            Next
        End If
        
        Dim list() As Entity = New Entity((4) - 1) {}
        ' initialiazes the plane
        Dim p As Plane = New Plane
        ' sets the colors and material of objects
        Dim m As Material = New Material("wood", New Bitmap("../../../../../../dataset/Assets/Textures/Maple.jpg"))
        model1.Materials.Add(m)
        Dim colors() As Color = New Color() {
                                                Color.Gray, 
                                                Color.FromArgb(255, 249, 136, 102), 
                                                Color.FromArgb(255, 255, 66, 14), 
                                                Color.FromArgb(255, 128,189,158), 
                                                Color.FromArgb(255, 137, 218, 89)
                                            }
        ' a set of objects
        #if (NURBS)
        Dim slot As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateRoundedRectangle(60, 20, 5, true)
        Dim circle As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(3.6)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        circle.Translate(-20, 0, 0)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        circle.Translate(40, 0, 0)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        Dim slotMesh As Brep = slot.ExtrudeAsBrep((Vector3D.AxisZ * 5))
        slotMesh.Rotate((Math.PI / 2), Vector3D.AxisZ)
        slotMesh.Color = colors(0)
        slotMesh.MaterialName = "wood"
        slotMesh.ColorMethod = colorMethodType.byEntity
        ' triangle
        Dim trianglePath As LinearPath = New LinearPath(Point3D.Origin, New Point3D(36, 0, 0), New Point3D(18, 0, 25), Point3D.Origin)
        Dim triangleRegion2 As devDept.Eyeshot.Entities.Region = New devDept.Eyeshot.Entities.Region(trianglePath)
        Dim triangleMesh As Brep = triangleRegion2.ExtrudeAsBrep((Vector3D.AxisY * 5))
        triangleMesh.Color = colors(1)
        triangleMesh.ColorMethod = colorMethodType.byEntity
        triangleMesh.Rotate(Utility.DegToRad(90), Vector3D.AxisZ)
        triangleMesh.Translate(52, -3, 0)
        ' weels
        Dim weelAxisMesh As Brep = Brep.CreateCylinder(3, 65)
        weelAxisMesh.MaterialName = "wood"
        weelAxisMesh.Rotate((Math.PI / 2), Vector3D.AxisY)
        weelAxisMesh.Color = colors(2)
        weelAxisMesh.ColorMethod = colorMethodType.byEntity
        Dim outer As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 12)
        Dim inner As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 3)
        Dim weel As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.Difference(outer, inner)(0)
        Dim weelRMesh As Brep = weel.ExtrudeAsBrep(10)
        weelRMesh.Translate(55, 0, 0)
        weelRMesh.Color = colors(2)
        weelRMesh.ColorMethod = colorMethodType.byEntity
        Dim weelLMesh As Brep = weel.ExtrudeAsBrep(-10)
        weelLMesh.Translate(10, 0, 0)
        weelLMesh.Color = colors(2)
        weelLMesh.ColorMethod = colorMethodType.byEntity
        ' cylinder
        Dim cylMesh As Brep = Brep.CreateCylinder(3.5, 40)
        cylMesh.Color = colors(3)
        cylMesh.ColorMethod = colorMethodType.byEntity
        'box
        Dim baseMesh As Brep = Brep.CreateBox(40, 40, 5)
        baseMesh.Color = colors(4)
        baseMesh.ColorMethod = colorMethodType.byEntity
        #Else 
        ' slot
        Dim slot As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateRoundedRectangle(60, 20, 5, true)
        Dim circle As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(3.6)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        circle.Translate(-20, 0, 0)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        circle.Translate(40, 0, 0)
        slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)(0)
        Dim slotMesh As Mesh = slot.ExtrudeAsMesh((Vector3D.AxisZ * 5), 0.01, Mesh.natureType.RichSmooth)
        slotMesh.Rotate((Math.PI / 2), Vector3D.AxisZ)
        slotMesh.Color = colors(0)
        slotMesh.ApplyMaterial("wood", textureMappingType.Cubic, 2, 2)
        slotMesh.ColorMethod = colorMethodType.byEntity
        ' triangle
        Dim trianglePath As LinearPath = New LinearPath(Point3D.Origin, New Point3D(36, 0, 0), New Point3D(18, 0, 25), Point3D.Origin)
        Dim triangleRegion2 As devDept.Eyeshot.Entities.Region = New devDept.Eyeshot.Entities.Region(trianglePath)
        Dim triangleMesh As Mesh = triangleRegion2.ExtrudeAsMesh((Vector3D.AxisY * 5), 0.01, Mesh.natureType.RichSmooth)
        triangleMesh.Color = colors(1)
        triangleMesh.ColorMethod = colorMethodType.byEntity
        triangleMesh.Rotate(Utility.DegToRad(90), Vector3D.AxisZ)
        triangleMesh.Translate(52, -3, 0)
        ' weels
        Dim weelAxisMesh As Mesh = Mesh.CreateCylinder(3, 65, 50, Mesh.natureType.RichSmooth)
        weelAxisMesh.ApplyMaterial("wood", textureMappingType.Cylindrical, 2, 2)
        weelAxisMesh.Rotate((Math.PI / 2), Vector3D.AxisY)
        weelAxisMesh.Color = colors(2)
        weelAxisMesh.ColorMethod = colorMethodType.byEntity
        Dim outer As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 12)
        Dim inner As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 3)
        Dim weel As devDept.Eyeshot.Entities.Region = devDept.Eyeshot.Entities.Region.Difference(outer, inner)(0)
        Dim weelRMesh As Mesh = weel.ExtrudeAsMesh(10, 0.01, Mesh.natureType.RichSmooth)
        weelRMesh.Translate(55, 0, 0)
        weelRMesh.Color = colors(2)
        weelRMesh.ColorMethod = colorMethodType.byEntity
        Dim weelLMesh As Mesh = weel.ExtrudeAsMesh(-10, 0.01, Mesh.natureType.RichSmooth)
        weelLMesh.Translate(10, 0, 0)
        weelLMesh.Color = colors(2)
        weelLMesh.ColorMethod = colorMethodType.byEntity
        ' cylinder
        Dim cylMesh As Mesh = Mesh.CreateCylinder(3.5, 40, 50)
        cylMesh.Color = colors(3)
        cylMesh.ColorMethod = colorMethodType.byEntity
        'box
        Dim baseMesh As Mesh = Mesh.CreateBox(40, 40, 5)
        baseMesh.Color = colors(4)
        baseMesh.ColorMethod = colorMethodType.byEntity
        #End If 
        ' blocks containing the geometry
        Dim baseBlock As Block = New Block("Box")
        baseBlock.Entities.Add(baseMesh)
        Dim redTriangleBlock As Block = New Block("Slot")
        redTriangleBlock.Entities.Add(slotMesh)
        Dim yellowTriangleBlock As Block = New Block("Triangle")
        yellowTriangleBlock.Entities.Add(triangleMesh)
        Dim greenBlock As Block = New Block("Cylinder")
        greenBlock.Entities.Add(cylMesh)
        Dim weelsBlock As Block = New Block("weels")
        weelsBlock.Entities.Add(weelAxisMesh)
        weelsBlock.Entities.Add(weelRMesh)
        weelsBlock.Entities.Add(weelLMesh)
        model1.Blocks.Add(baseBlock)
        model1.Blocks.Add(redTriangleBlock)
        model1.Blocks.Add(yellowTriangleBlock)
        model1.Blocks.Add(greenBlock)
        model1.Blocks.Add(weelsBlock)
        ' saves entities elements
        For Each b As Block In model1.Blocks
            ' deletes previous entities
            model1.Entities.Clear
            ' adds the entity to the viewport
            Dim reference As BlockReference = New BlockReference(b.Name)
            model1.Entities.Add(reference)
            ' fits the model in the viewport
            model1.ZoomFit
            ' save image
            Dim materialSphere As Bitmap = model1.RenderToBitmap(1)
            materialSphere.Save((dirName + ("\" + (b.Name + ".bmp"))))
        Next
        ' fills ListView with saved images
        Me.listView1.ItemsSource = Fill_listView
        ' restores scene
        model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor
        model1.GetBackground.TopColor = oldColor
        model1.Viewports(0).GetToolBar.Visible = true
        model1.GetCoordinateSystemIcon.Visible = true
        model1.GetOriginSymbol.Visible = true
        model1.GetViewCubeIcon.Visible = true
        model1.GetGrid.Visible = true
        model1.Flat.EdgeThickness = 1
        model1.Flat.SilhouetteThickness = 2
        model1.DisplayMode = displayType.Rendered
    End Sub
    
        Public ReadOnly Property Fill_listView() As System.Collections.ObjectModel.ObservableCollection(Of ImageItem)
            Get
                Dim results = New ObservableCollection(Of ImageItem)()
                Dim dir As New DirectoryInfo(dirName)
                For Each file As FileInfo In dir.GetFiles()
                    Dim name As String = file.Name.Split("."c)(0)
                    Dim bitmap As New BitmapImage(New Uri(file.FullName))
                    results.Add(New ImageItem() With { _
                                   .Name = name, _
                                   .Image = bitmap _
                                   })
                Next
                Return results
            End Get
        End Property
        #End Region
    End Class
    Public Class ImageItem
        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(value As String)
                m_Name = value
            End Set
        End Property
        Private m_Name As String

        Public Property Image() As ImageSource
            Get
                Return m_Image
            End Get
            Set(value As ImageSource)
                m_Image = value
            End Set
        End Property
        Private m_Image As ImageSource
    End Class
End Namespace
