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
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow

        Private Const Assets As String = "../../../../../../dataset/Assets/"

        Private _interval As Integer = 20 ' medium speed
        Private _animationFrameNumber As Integer = -1

        Public Sub New()
            InitializeComponent()
            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            model1.GetGrid().AutoSize = True
            model1.GetGrid().[Step] = 500

            Dim readFile As New devDept.Eyeshot.Translators.ReadFile(Assets + "160.eye")
            readFile.DoWork()
            model1.Entities.AddRange(readFile.Entities, System.Drawing.Color.DimGray)

            Dim firstBlock As New Block("First")
            AddStlToBlock(firstBlock, "930.eye", System.Drawing.Color.DeepSkyBlue)
            AddStlToBlock(firstBlock, "940.eye", System.Drawing.Color.DeepSkyBlue)

            Dim secondBlock As New Block("Second")
            AddStlToBlock(secondBlock, "570.eye", System.Drawing.Color.DodgerBlue)

            Dim thirdBlock As New Block("Third")
            AddStlToBlock(thirdBlock, "590.eye", System.Drawing.Color.SlateBlue)

            firstBlock.Entities.Add(New TranslatingAlongY("Second"))
            secondBlock.Entities.Add(New TranslatingAlongZ("Third"))

            model1.Blocks.Add(firstBlock)
            model1.Blocks.Add(secondBlock)
            model1.Blocks.Add(thirdBlock)

            model1.Entities.Add(New TranslatingAlongX("First"))

            model1.SetView(viewType.Trimetric)

            model1.ZoomFit()

            ' Turn off silhouettes to increase drawing speed
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never

            ' Shadows are not currently supported in animations
            model1.Rendered.ShadowMode = shadowType.None

            model1.StartAnimation(_interval)
            startButton.IsEnabled = False

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub AddStlToBlock(block As Block, eyeName As String, color As System.Drawing.Color)
            Dim readFile As New devDept.Eyeshot.Translators.ReadFile(Assets + eyeName)

            readFile.DoWork()


            readFile.Entities(0).ColorMethod = colorMethodType.byEntity

            readFile.Entities(0).Color = color

            block.Entities.Add(readFile.Entities(0))
        End Sub

        Private Sub StartButtonEnable(ByVal value As Boolean)
            startButton.IsEnabled = value
            pauseButton.IsEnabled = Not value
            stopButton.IsEnabled = Not value
        End Sub

        Private Function IsAnimationStarted() As Boolean
            Return Not startButton.IsEnabled
        End Function

        ' Starts the animation
        Private Sub StartButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            If _animationFrameNumber <> -1 Then model1.AnimationFrameNumber = _animationFrameNumber
            model1.StartAnimation(_interval)
            StartButtonEnable(False)
        End Sub

        ' After this method the animation starts from where it was stopped, saving animationFrameNumber
        Private Sub PauseButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            _animationFrameNumber = model1.AnimationFrameNumber
            model1.StopAnimation()
            StartButtonEnable(True)
        End Sub

        ' After this method the animation starts from the beginning
        Private Sub StopButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            model1.StopAnimation()
            StartButtonEnable(True)
        End Sub

        ' Changes the speed of the animation by the interval. initially interval=20 (medium)
        Private Sub ChangeSpeedAnimation(ByVal interval As Integer)
            _interval = interval
            If IsAnimationStarted() Then
                _animationFrameNumber = model1.AnimationFrameNumber
                model1.StopAnimation()
                If _animationFrameNumber <> -1 Then model1.AnimationFrameNumber = _animationFrameNumber
                model1.StartAnimation(_interval)
            End If
        End Sub

        Private Sub SlowRadioButton_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
            ChangeSpeedAnimation(99)
        End Sub

        Private Sub MediumRadioButton_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
            ChangeSpeedAnimation(20)
        End Sub

        Private Sub FastRadioButton_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
            ChangeSpeedAnimation(1)
        End Sub

    End Class

    Class TranslatingAlongX
        Inherits BlockReference

        Private xPos As Double
        Private customTransform As Transformation

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, 1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)
            ' frameNumber is incremented each time this function is called
            ' it represents the time passing an can be used to index an array
            ' 3D positions for example.

            ' angle in degrees
            Dim alpha As Double = (frameNumber Mod 359) * 10

            ' circle radius
            Dim radius As Double = 100

            xPos = radius * Math.Cos(Utility.DegToRad(alpha))

            MyBase.Animate(frameNumber)
        End Sub

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            customTransform = New Translation(xPos, 0, 0)
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class

    Class TranslatingAlongY
        Inherits BlockReference
        Private xPos As Double, yPos As Double
        Private customTransform As Transformation

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, 1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)
            ' frameNumber is incremented each time this function is called
            ' it represents the time passing an can be used to index an array
            ' 3D positions for example.

            ' angle in degrees
            Dim alpha As Double = (frameNumber Mod 359) * 10

            ' circle radius
            Dim radius As Double = 100

            yPos = radius * Math.Sin(Utility.DegToRad(alpha))

            MyBase.Animate(frameNumber)
        End Sub

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            customTransform = New Translation(xPos, yPos, 0)
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class

    Class TranslatingAlongZ
        Inherits BlockReference

        Private xPos As Double, yPos As Double, zPos As Double
        Private customTransform As Transformation

        Public Sub New(blockName As String)
            MyBase.New(0, 0, 0, blockName, 1, 1, 1, 0)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)
            ' frameNumber is incremented each time this function is called
            ' it represents the time passing an can be used to index an array
            ' 3D positions for example.

            ' angle in degrees
            Dim alpha As Double = (frameNumber Mod 359) * 10

            ' circle radius
            Dim radius As Double = 100

            zPos = radius * Math.Cos(Utility.DegToRad(alpha))

            MyBase.Animate(frameNumber)
        End Sub

        Public Overrides Sub MoveTo(data As DrawParams)
            MyBase.MoveTo(data)

            customTransform = New Translation(xPos, yPos, zPos)
            data.RenderContext.MultMatrixModelView(customTransform)
        End Sub

        Public Overrides Function IsInFrustum(data As FrustumParams, center As Point3D, radius As Double) As Boolean
            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, customTransform * center, radius)
        End Function

    End Class
End Namespace

