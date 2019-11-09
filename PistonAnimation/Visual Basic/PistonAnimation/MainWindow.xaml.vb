Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports devDept.Eyeshot.Translators

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)        
        model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor
        model1.Rendered.EdgeThickness = 1

        model1.GetGrid().Min = New Point3D(-150, -100)
        model1.GetGrid().Max = New Point3D(+200, +100)
        model1.GetGrid().[Step] = 20
        model1.GetGrid().AutoSize = False


        Dim b As New Block("CrankShaft")

        Dim lp As New LinearPath(6)

        lp.Vertices(0) = New Point3D(0, -50, 0)
        lp.Vertices(1) = New Point3D(0, -20, 0)
        lp.Vertices(2) = New Point3D(50, -20, 0)
        lp.Vertices(3) = New Point3D(50, +20, 0)
        lp.Vertices(4) = New Point3D(0, +20, 0)
        lp.Vertices(5) = New Point3D(0, +50, 0)

        lp.ColorMethod = colorMethodType.byEntity
        lp.Color = System.Drawing.Color.Blue
        lp.LineWeightMethod = colorMethodType.byEntity
        lp.LineWeight = 1.5F

        b.Entities.Add(lp)

        model1.Blocks.Add(b)

        model1.Entities.Add(New Rotating("CrankShaft"))


        b = New Block("ConnectingRod")

        Dim XZ As New Plane(Point3D.Origin, Vector3D.AxisX, Vector3D.AxisZ)

        b.Entities.Add(New Circle(XZ, Point2D.Origin, 8))
        b.Entities.Add(New Circle(XZ, Point2D.Origin, 6))
        b.Entities.Add(New Circle(XZ, New Point2D(120, 0), 15))
        b.Entities.Add(New Circle(XZ, New Point2D(120, 0), 10))
        b.Entities.Add(New Line(XZ, New Point2D(6.928, 4), New Point2D(105.543, 4)))
        b.Entities.Add(New Line(XZ, New Point2D(6.928, -4), New Point2D(105.543, -4)))
        b.Entities.Add(New Line(XZ, New Point2D(-2, 0), New Point2D(2, 0)))
        b.Entities.Add(New Line(XZ, New Point2D(0, -2), New Point2D(0, 2)))
        b.Entities.Add(New Line(XZ, New Point2D(120 - 3, 0), New Point2D(120 + 3, 0)))
        b.Entities.Add(New Line(XZ, New Point2D(120, -3), New Point2D(120, 3)))

        For Each ent As Entity In b.Entities
            ent.ColorMethod = colorMethodType.byEntity
            ent.Color = System.Drawing.Color.Red
        Next

        model1.Blocks.Add(b)

        model1.Entities.Add(New Oscillating("ConnectingRod"))


        b = New Block("Axis")

        Dim line As New Line(0, +30, 0, -30)

        line.ColorMethod = colorMethodType.byEntity
        line.Color = System.Drawing.Color.Black

        b.Entities.Add(line)

        model1.Blocks.Add(b)

        model1.Entities.Add(New Translating("Axis"))


        b = New Block("Piston")

        Dim readFile As New ReadFile("../../../../../../dataset/Assets/Piston.eye")
        readFile.DoWork()

        Dim m As Mesh = DirectCast(readFile.Entities(0), Mesh)

        m.EdgeStyle = Mesh.edgeStyleType.Sharp
        m.ColorMethod = colorMethodType.byEntity
        m.Color = System.Drawing.Color.DarkGray

        b.Entities.Add(m)

        model1.Blocks.Add(b)

        model1.Entities.Add(New Translating("Piston"))

        ' Bounding box override
        model1.BoundingBox.Min = New Point3D(-110, -50, -70)
        model1.BoundingBox.Max = New Point3D(+170, +50, +70)
        model1.BoundingBox.OverrideSceneExtents = True

        ' Shadows are not currently supported in animations
        model1.Rendered.ShadowMode = shadowType.None

        model1.StartAnimation(1)

        model1.ZoomFit()

        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

#Region "Animation helper classes"

    Private Class Translating
        Inherits BlockReference

        Private alpha As Double
        Private xPos As Double

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, _
                1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)

            alpha += 2

            If alpha > 359 Then

                alpha = 0
            End If

            ' cranckshaft radius
            Dim r As Double = 50
            ' connecting rod length
            Dim l As Double = 120

            Dim beta As Double = Math.Asin(r * Math.Sin(Utility.DegToRad(alpha)) / l)

            xPos = r * Math.Cos(Utility.DegToRad(alpha)) - l * Math.Cos(beta)

        End Sub

        Private customTransform As Transformation

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            ' 100 + xPos: the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
            customTransform = New Translation(100 + xPos, 0, 0)
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class

    Private Class Oscillating
        Inherits BlockReference

        Private alpha As Double
        Private beta As Double
        Private xPos As Double

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, _
                1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)

            alpha += 2.0F

            If alpha > 359 Then

                alpha = 0
            End If

            ' cranckshaft radius
            Dim r As Double = 50
            ' connecting rod length
            Dim l As Double = 120

            beta = Math.Asin(r * Math.Sin(Utility.DegToRad(alpha)) / l)

            xPos = r * Math.Cos(Utility.DegToRad(alpha)) - l * Math.Cos(beta)

        End Sub

        Private customTransform As Transformation

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            ' 100 + xPos: the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
            customTransform = New Translation(100 + xPos, 0, 0) * New devDept.Geometry.Rotation(beta, New Vector3D(0, 1, 0))
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class

    Private Class Rotating
        Inherits BlockReference

        Private alpha As Double

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, _
                1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)

            alpha += 2.0F

            If alpha > 359 Then

                alpha = 0
            End If
        End Sub

        Private customTransform As Transformation

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            ' the 100 value is added to facilitate the zoom fit for demo purpose, you can safely remove it
            customTransform = New Translation(100, 0, 0) * New devDept.Geometry.Rotation(Utility.DegToRad(alpha), New Vector3D(0, 1, 0))
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class

#End Region
End Class

