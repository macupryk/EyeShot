
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
Imports devDept.Eyeshot.Entities
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
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never
            model1.Rendered.EnvironmentMapping = False

            Dim matName As String = "Globe mat"

            ' defines a new material using a texture
            Dim mat As New Material(matName, New Bitmap("../../../../../../dataset/Assets/Textures/EarthMap.jpg"))

            ' more accurate texture scaling (optional and slower)
            mat.MinifyingFunction = textureFilteringFunctionType.Linear

            ' adds the material to the viewport's master material collection
            model1.Materials.Add(mat)

            ' Creates a new RichMesh sphere with earth radius, slices and stacks
            Dim rm As Mesh = Mesh.CreateSphere(6356.75, 100, 50, Mesh.natureType.RichSmooth)

            ' assigns the material to all triangles and maps the material texture spherically
            rm.ApplyMaterial(matName, textureMappingType.Spherical, 1, 1)

            ' adds the mesh to the viewport's master entities array
            model1.Entities.Add(rm)           

            ' fits the model in the viewport
            model1.ZoomFit()

            ' hides origin symbol
            model1.GetOriginSymbol().Visible = False
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace

