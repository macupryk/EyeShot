Imports System.Collections.Generic
Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports PointF = System.Drawing.PointF

Class Floor
    Inherits Mesh

    Private width As Single = 16
    Private height As Single = 16
    Private tile_S As Single = 8
    Private tile_T As Single = 8

    Private floorTexture As TextureBase
    Private lightMapTexture As TextureBase

    Public multiTexture As Boolean

    Public Sub New(multitexture As Boolean)
        MyBase.New(4, 2, Mesh.natureType.RichPlain)
        Me.multiTexture = multitexture

        Vertices(0) = New Point3D(-width * 0.5, height * 0.5)
        Vertices(1) = New Point3D(width * 0.5, height * 0.5)
        Vertices(2) = New Point3D(width * 0.5, -height * 0.5)
        Vertices(3) = New Point3D(-width * 0.5, -height * 0.5)

        Triangles(0) = New IndexTriangle(0, 1, 2)
        Triangles(1) = New IndexTriangle(0, 2, 3)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()

        floorTexture.Dispose()
        lightMapTexture.Dispose()
    End Sub

    Private Const Textures As String = "../../../../../../dataset/Assets/Textures/"

    Public Overrides Sub Compile(data As CompileParams)
        MyBase.Compile(data)

        floorTexture = data.RenderContext.CreateTexture2D(New Bitmap(Textures + "floor_color_map.jpg"), textureFilteringFunctionType.LinearMipmapLinear)
        lightMapTexture = data.RenderContext.CreateTexture2D(New Bitmap(Textures + "floor_color_map.jpg"), textureFilteringFunctionType.LinearMipmapLinear)

    End Sub

    Protected Overrides Sub Render(data As RenderParams)
        If Not multiTexture Then
            ' Set the texture replace function
            data.RenderContext.SetShader(shaderType.Texture2DNoLights)

            data.RenderContext.SetTexture(floorTexture)
            data.RenderContext.DrawRichPlainQuads(Vertices, New Vector3D() {Vector3D.AxisZ, Vector3D.AxisZ, Vector3D.AxisZ, Vector3D.AxisZ}, New PointF() {New PointF(0, 0), New PointF(tile_S, 0), New PointF(tile_S, tile_T), New PointF(0, tile_T)})
        End If
    End Sub
End Class
