Imports System.Collections.Generic
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
Imports System.IO
Imports System.Collections.ObjectModel
Imports System.Drawing

Namespace WpfApplication1
    Partial Class MainWindow        
        Private Const Textures As String = "../../../../../../dataset/Assets/Textures/"
        Private dirName As String = "myPictures"

        Protected Sub CreateTexture()
            ' setting scene for saving
            Dim oldStyle As backgroundStyleType = model1.GetBackground().StyleMode
            model1.GetBackground().StyleMode = backgroundStyleType.Solid
            model1.Backface.ColorMethod = backfaceColorMethodType.SingleColor
            Dim oldColor As System.Windows.Media.Brush = model1.GetBackground().TopColor
            model1.GetBackground().TopColor = new SolidColorBrush(Colors.White)
            model1.Shaded.ShadowMode = shadowType.None
            model1.Viewports(0).GetToolBar().Visible = False
            model1.GetCoordinateSystemIcon().Visible = False
            model1.Rendered.PlanarReflections = False
            model1.GetOriginSymbol().Visible = False
            model1.GetViewCubeIcon().Visible = False


            ' sets rendered only display mode
            model1.DisplayMode = displayType.Rendered

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' defines a new material using a texture
            Dim list As New MaterialKeyedCollection()

            Dim mat As New Material("Cherry", new Bitmap(Textures + "Cherry.jpg"))
            list.Add(mat)
            mat = New Material("Bricks", new Bitmap(Textures + "Bricks.jpg"))
            list.Add(mat)
            mat = New Material("Maple", new Bitmap(Textures + "Maple.jpg"))
            list.Add(mat)
            mat = New Material("Floor", new Bitmap(Textures + "floor_color_map.jpg"))
            list.Add(mat)
            mat = New Material("Wenge", new Bitmap(Textures + "Wenge.jpg"))
            list.Add(mat)
            mat = New Material("Marble", new Bitmap(Textures + "marble.jpg"))
            list.Add(mat)
            mat = Material.Chrome
            mat.Environment = 0.7F
            mat.Name = "Chrome"
            list.Add(mat)
            mat = Material.Emerald
            mat.Name = "Glass"
            list.Add(mat)
            mat = Material.Gold
            mat.Environment = 0.05F
            mat.Name = "Gold"
            list.Add(mat)
            mat = New Material("Strips", new Bitmap(Textures + "strips.png"))
            mat.Diffuse = System.Drawing.Color.FromArgb(254, System.Drawing.Color.White)
            list.Add(mat)

            ' creates the directory to save material elements
            If Not System.IO.Directory.Exists(dirName) Then
                System.IO.Directory.CreateDirectory(dirName)
            Else
                ' deletes all previous files
                For Each filePath As String In System.IO.Directory.GetFiles(dirName)
                    System.IO.File.Delete(filePath)
                Next
            End If

            ' saves material elements
            For Each m As Material In list
                createMaterialSphere(m)
            Next

            ' fills ListView with previous saved images
            Me.listView1.ItemsSource = fill_listView

            ' restores scene
            model1.GetBackground().StyleMode = oldStyle
            model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor
            model1.GetBackground().TopColor = oldColor
            model1.Viewports(0).GetToolBar().Visible = True
            model1.GetCoordinateSystemIcon().Visible = True
            model1.Rendered.PlanarReflections = True
            model1.GetOriginSymbol().Visible = True
            model1.GetViewCubeIcon().Visible = True
        End Sub

        Public Sub CreateMaterialSphere(mat As Material)

            ' adds the material to the viewport's master material collection
            model1.Materials.Add(mat)

            ' Creates a new RichMesh sphere with earth radius, slices and stacks
            Dim rm As Mesh = Mesh.CreateSphere(6000, 36, 18, Mesh.natureType.RichSmooth)

            ' assigns the material to all triangles and maps the material texture spherically
            rm.ApplyMaterial(mat.Name, textureMappingType.Spherical, 1, 1)

            ' deletes previous entities
            model1.Entities.Clear()

            ' adds the mesh to the viewport
            model1.Entities.Add(rm)

            ' fits the model in the viewport
            model1.ZoomFit()

            ' save image
            Dim materialSphere As Bitmap = model1.RenderToBitmap(1)
            materialSphere.Save((Convert.ToString(dirName & Convert.ToString("\")) & mat.Name) + ".bmp")
        End Sub

        Public ReadOnly Property Fill_listView() As System.Collections.ObjectModel.ObservableCollection(Of ImageItem)
            Get
                Dim results = New ObservableCollection(Of ImageItem)()
                Dim dir As New DirectoryInfo(dirName)
                For Each file As FileInfo In dir.GetFiles()
                    Dim name As String = file.Name.Split("."c)(0)
                    Dim bitmap As New BitmapImage(New Uri(file.FullName))
                    'bitmap.BaseUri.Segments[bitmap.BaseUri.Segments.Length - 1];
                    results.Add(New ImageItem() With { _
                        .Name = name, _
                        .Image = bitmap _
                    })
                Next
                Return results
            End Get
        End Property
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

