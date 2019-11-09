Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics

' Values for hitting the targets
' Target 1 --> Launch angle 34, Direction angle: 48, Fire power: 10
' Target 2 --> Launch angle: 30, Direction angle: 9, Fire power: 13
' Target 3 --> Launch angle: 23, Direction angle: -42, Fire power: 12


''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow

    Private _numEntityInScene As Integer = 10

    Private _target1 As Target = New Target()
    Private _target2 As Target = New Target()
    Private _target3 As Target = New Target()

    Private _initialSlidersValue As Integer() = New Integer(2) {}

    Private _lastLaunchAngleSlider_Value, _lastDirectionAngleSlider_Value As Double

    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        model1.GetOriginSymbol().Visible = False

        model1.DisplayMode = displayType.Rendered

        ' display mode settings
        model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor
        model1.Rendered.EdgeThickness = 1
        model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never
        model1.Rendered.PlanarReflections = False
        ' shadows are Not currently supported in animations
        model1.Rendered.ShadowMode = devDept.Graphics.shadowType.None

        ' grid settings
        model1.GetGrid().Visible = False

        model1.Camera.FocalLength = 20

        ' bounding box override
        model1.BoundingBox.OverrideSceneExtents = True
        model1.BoundingBox.Min = New Point3D(-200, -200, -100)
        model1.BoundingBox.Max = New Point3D(+200, +200, +100)

        ' sets shadows and lights
        model1.Rendered.ShadowMode = shadowType.Realistic
        model1.Rendered.RealisticShadowQuality = realisticShadowQualityType.High
        model1.Light2.Active = false
        model1.Light3.Active = false
        model1.Light4.Active = false
        model1.Light5.Active = false
        model1.Light6.Active = false
        model1.Light7.Active = false
        model1.Light8.Active = false
        model1.Light1.Type = lightType.Directional
        model1.Light1.Stationary = false

    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)

        model1.ZoomFit()

        Dim rocketBlock As Block = CreateRocketBlock()
        Dim forestBlock As Block = CreateForestTreeBlock()

        model1.Blocks.Add(rocketBlock)
        model1.Blocks.Add(forestBlock)

        ' creates rocket block reference
        Dim rocketBlockReference As BlockReference = New BlockReference(0, 0, 0, "Rocket", 1, 1, 1, 0)
        model1.Entities.Add(rocketBlockReference)

        ' creates 3 targets
        model1.Entities.Add(_target1.CreateTarget(60, 70), Color.Red)
        model1.Entities.Add(_target2.CreateTarget(140, 20), Color.Red)
        model1.Entities.Add(_target3.CreateTarget(80, -70), Color.Red)

        ' creates 3 hit regions that cover the targets when the rocket hit them
        model1.Entities.Add(_target1.CreateHitRegion(60, 70), Color.Red)
        model1.Entities.Add(_target2.CreateHitRegion(140, 20), Color.Red)
        model1.Entities.Add(_target3.CreateHitRegion(80, -70), Color.Red)

        ' sets visibility of hit regions, initially false. 
        model1.Entities(4).Visible = False
        model1.Entities(5).Visible = False
        model1.Entities(6).Visible = False

        ' creates the forest block
        Dim forestBlockReference As BlockReference = New BlockReference(0, 0, 0, "Forest", 1, 1, 1, 0)
        model1.Entities.Add(forestBlockReference)

        ' create the ground
        CreateGround()

        ' initializes WPF graphic elements 
        resetButton.IsEnabled = False

        _initialSlidersValue(0) = launchAngleSlider.Value
        _initialSlidersValue(1) = directionAngleSlider.Value
        _initialSlidersValue(2) = firePowerSlider.Value

        launchAngleNumLabel.Content = launchAngleSlider.Value.ToString()
        directionAngleNumLabel.Content = directionAngleSlider.Value.ToString()
        firePowerNumLabel.Content = firePowerSlider.Value.ToString()

        ' sets view
        model1.SetView(viewType.Trimetric, True, False)

        model1.ZoomFit()

        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

    Private Function CreateTreeBlock() As Block
        Dim block As Block = New Block("Tree")

        Dim trunk As Mesh = Mesh.CreateCylinder(3, 8, 30)
        trunk.Translate(0, 0, -3)
        trunk.Color = Color.Brown
        trunk.ColorMethod = colorMethodType.byEntity

        Dim points As IList(Of Point3D) = New List(Of Point3D)()
        points.Add(New Point3D(0, 0, 5))
        points.Add(New Point3D(0, 0, 41))
        points.Add(New Point3D(-8, 0, 29))
        points.Add(New Point3D(-3, 0, 29))
        points.Add(New Point3D(-13, 0, 17))
        points.Add(New Point3D(-3, 0, 17))
        points.Add(New Point3D(-18, 0, 5))
        points.Add(New Point3D(0, 0, 5))

        Dim leaves As Mesh = Mesh.CreatePlanar(points, Mesh.natureType.Smooth)
        leaves.Color = Color.Green
        leaves.ColorMethod = colorMethodType.byEntity

        Dim leaves1 As Mesh = CType(leaves.Clone(), Mesh)
        leaves1.Rotate((2 * Math.PI) / 3, Vector3D.AxisZ)
        leaves1.Color = Color.Green

        Dim leaves2 As Mesh = CType(leaves.Clone(), Mesh)
        leaves2.Rotate((4 * Math.PI) / 3, Vector3D.AxisZ)
        leaves2.Color = Color.Green

        block.Entities.Add(trunk)
        block.Entities.Add(leaves)
        block.Entities.Add(leaves1)
        block.Entities.Add(leaves2)

        Return block
    End Function

    Private Function CreateForestTreeBlock() As Block
        Dim block As Block = New Block("Forest")

        Dim treeBlock As Block = CreateTreeBlock()

        model1.Blocks.Add(treeBlock)

        Dim treeBlockReference As BlockReference = New BlockReference(-75, 50, 0, "Tree", 0.5, 0.5, 0.5, 0)
        block.Entities.Add(treeBlockReference)

        Dim treeBlockReference1 As BlockReference = New BlockReference(-70, 45, 0, "Tree", 1, 1, 1, 0)
        block.Entities.Add(treeBlockReference1)

        Dim treeBlockReference2 As BlockReference = New BlockReference(-68, 60, 0, "Tree", 0.7, 0.7, 0.7, 0)
        block.Entities.Add(treeBlockReference2)

        Dim treeBlockReference3 As BlockReference = New BlockReference(150, -50, 0, "Tree", 0.5, 0.5, 0.5, 0)
        block.Entities.Add(treeBlockReference3)

        Dim treeBlockReference4 As BlockReference = New BlockReference(160, -45, 0, "Tree", 0.7, 0.7, 0.7, 0)
        block.Entities.Add(treeBlockReference4)

        Dim treeBlockReference5 As BlockReference = New BlockReference(155, -70, 0, "Tree", 0.5, 0.5, 0.5, 0)
        block.Entities.Add(treeBlockReference5)

        Dim treeBlockReference6 As BlockReference = New BlockReference(-70, -55, 0, "Tree", 1, 1, 1, 0)
        block.Entities.Add(treeBlockReference6)

        Dim treeBlockReference7 As BlockReference = New BlockReference(-75, -80, 0, "Tree", 0.7, 0.7, 0.7, 0)
        block.Entities.Add(treeBlockReference7)

        Dim treeBlockReference8 As BlockReference = New BlockReference(110, 85, 0, "Tree", 1, 1, 1, 0)
        block.Entities.Add(treeBlockReference8)

        Dim treeBlockReference9 As BlockReference = New BlockReference(120, 70, 0, "Tree", 0.7, 0.7, 0.7, 0)
        block.Entities.Add(treeBlockReference9)

        Dim treeBlockReference10 As BlockReference = New BlockReference(180, 75, 0, "Tree", 1, 1, 1, 0)
        block.Entities.Add(treeBlockReference10)

        Dim treeBlockReference11 As BlockReference = New BlockReference(150, 65, 0, "Tree", 1, 1, 1, 0)
        block.Entities.Add(treeBlockReference11)

        Return block
    End Function

    Public Function CreateRocketBlock() As Block
        Dim rocketBlock As Block = New Block("Rocket")

        ' missile bottom
        Dim lpMissileBottom As LinearPath = New LinearPath(New Point3D(0, 0, 1), New Point3D(1.5, 0, 1), New Point3D(1.5, 0, 1.5), New Point3D(0, 0, 1.5), New Point3D(0, 0, 1))

        Dim missileBottom As Mesh = lpMissileBottom.RevolveAsMesh(0, 2 * Math.PI, New Vector3D(0, 0, 1), New Point3D(0, 0, 1), 20, 0.1, Mesh.natureType.Smooth)
        missileBottom.ColorMethod = colorMethodType.byEntity
        missileBottom.Color = Color.Red
        missileBottom.Weld()
        rocketBlock.Entities.Add(missileBottom)

        ' missile body
        Dim lpMissileBody As LinearPath = New LinearPath(New Point3D(0, 0, 1.5), New Point3D(1.5, 0, 1.5), New Point3D(1.95, 0, 3.5), New Point3D(2.15, 0, 4.25), New Point3D(2.25, 0, 5.75), New Point3D(2.25, 0, 7.25), New Point3D(2.15, 0, 8.75), New Point3D(1.95, 0, 10.25), New Point3D(1.75, 0, 11), New Point3D(1.15, 0, 13), New Point3D(0, 0, 13), New Point3D(0, 0, 0))

        Dim missileBody As Mesh = lpMissileBody.RevolveAsMesh(0, 2 * Math.PI, New Vector3D(0, 0, 1), New Point3D(0, 0, 1.5), 20, 0.1, Mesh.natureType.Smooth)
        missileBody.ColorMethod = colorMethodType.byEntity
        missileBody.Color = Color.White
        missileBody.Weld()
        rocketBlock.Entities.Add(missileBody)

        ' missile edge
        Dim lpMissileEdge As LinearPath = New LinearPath(New Point3D(0, 0, 13), New Point3D(1.15, 0, 13), New Point3D(0.85, 0, 13.5), New Point3D(0.65, 0, 13.75), New Point3D(0.45, 0, 13.85), New Point3D(0.25, 0, 13.92), New Point3D(0.05, 0, 13.992), New Point3D(0, 0, 14), New Point3D(0, 0, 14))

        Dim missileEdge As Mesh = lpMissileEdge.RevolveAsMesh(0, 2 * Math.PI, New Vector3D(0, 0, 1), New Point3D(0, 0, 11), 50, 0.1, Mesh.natureType.Smooth)
        missileEdge.ColorMethod = colorMethodType.byEntity
        missileEdge.Color = Color.Red
        missileEdge.Weld()
        rocketBlock.Entities.Add(missileEdge)

        ' missile wings
        Dim lpMissileWing As LinearPath = New LinearPath(New Point3D(2.15, 0, 4.25), New Point3D(4, 0, 2), New Point3D(4, 0, 0), New Point3D(1.5, 0, 1.5), New Point3D(1.95, 0, 3.5), New Point3D(2.15, 0, 4.25))
        Dim regionWing As devDept.Eyeshot.Entities.Region = New devDept.Eyeshot.Entities.Region(lpMissileWing, Plane.XZ)
        Dim missileWing1 As Mesh = regionWing.ExtrudeAsMesh(0.15, 0.1, Mesh.natureType.Plain)
        missileWing1.ColorMethod = colorMethodType.byEntity
        missileWing1.Color = Color.Red
        missileWing1.Weld()
        rocketBlock.Entities.Add(missileWing1)

        Dim missileWing2 As Mesh = CType(missileWing1.Clone(), Mesh)
        missileWing2.Rotate(Math.PI / 2, Vector3D.AxisZ)
        rocketBlock.Entities.Add(missileWing2)

        Dim missileWing3 As Mesh = CType(missileWing2.Clone(), Mesh)
        missileWing3.Rotate(Math.PI / 2, Vector3D.AxisZ)
        rocketBlock.Entities.Add(missileWing3)

        Dim missileWing4 As Mesh = CType(missileWing3.Clone(), Mesh)
        missileWing4.Rotate(Math.PI / 2, Vector3D.AxisZ)
        rocketBlock.Entities.Add(missileWing4)

        Return rocketBlock
    End Function

    Private Sub CreateGround()
        Const rows As Integer = 5
        Const cols As Integer = 5

        Dim vertices As PointRGB() = New PointRGB(rows * cols - 1) {}

        Dim surfaceOffset As Double = -3

        Dim random As Random = New Random()
        Dim maxHeightSurface As Double = 3
        Dim minHeightSurface As Double = -3

        Dim indexArray As Integer = 0
        For j As Integer = 0 To rows - 1
            For i As Integer = 0 To cols - 1

                ' values 87.5 and 50 for dividing in 4 parts the grid(350x200)
                Dim x As Double = i * 87.5 - 150
                Dim y As Double = j * 50 - 100

                Dim z As Double = random.NextDouble() * (maxHeightSurface - minHeightSurface) + minHeightSurface

                ' sets saddlebrown color 
                Dim red As Integer = 139
                Dim green As Integer = 69
                Dim blue As Integer = 19

                If x = -62.5 AndAlso y = 0 OrElse x = 25 AndAlso y = 0 Then z = -surfaceOffset

                If (i Mod 2 = 0) AndAlso (j Mod 2 = 0) Then
                    ' sets greenforest color
                    red = 34
                    green = 139
                    blue = 34
                End If

                vertices(Math.Min(System.Threading.Interlocked.Increment(indexArray), indexArray - 1)) = New PointRGB(x, y, z, CByte(red), CByte(green), CByte(blue))
            Next
        Next

        Dim triangles As IndexTriangle() = New IndexTriangle(((rows - 1) * (cols - 1) * 2) - 1) {}
        indexArray = 0
        For j As Integer = 0 To (rows - 1) - 1
            For i As Integer = 0 To (cols - 1) - 1
                triangles(Math.Min(System.Threading.Interlocked.Increment(indexArray), indexArray - 1)) = (New IndexTriangle(i + j * cols, i + j * cols + 1, i + (j + 1) * cols + 1))
                triangles(Math.Min(System.Threading.Interlocked.Increment(indexArray), indexArray - 1)) = (New IndexTriangle(i + j * cols, i + (j + 1) * cols + 1, i + (j + 1) * cols))
            Next
        Next

        Dim surface As Mesh = New Mesh()
        surface.NormalAveragingMode = Mesh.normalAveragingType.Averaged

        surface.Vertices = vertices
        surface.Triangles = triangles

        ' sets surface lower than the grid
        surface.Translate(0, 0, surfaceOffset)

        model1.Entities.Add(surface)

        ' fits the model in the model1
        model1.ZoomFit()
    End Sub

    ' utility functions
    Private Sub ChangeStateLaunchButtons(ByVal status As Boolean)
        resetButton.IsEnabled = Not status
        fireButton.IsEnabled = status
        firePowerSlider.IsEnabled = status
        directionAngleSlider.IsEnabled = status
        launchAngleSlider.IsEnabled = status
    End Sub

    Private Sub ResetSlidersLabelsValue()
        ' resets Sliders values
        launchAngleSlider.Value = _initialSlidersValue(0)
        directionAngleSlider.Value = _initialSlidersValue(1)
        firePowerSlider.Value = _initialSlidersValue(2)

        ' resets number labels values
        launchAngleNumLabel.Content = launchAngleSlider.Value.ToString()
        directionAngleNumLabel.Content = directionAngleSlider.Value.ToString()
        firePowerNumLabel.Content = firePowerSlider.Value.ToString()
    End Sub

    ' resets the scene after resetButton has been clicked
    Private Sub ResetScene()
        Dim rocket As Entity = model1.Entities(0)

        ' resets rocket direction angle at initial value
        rocket.Rotate(-_lastDirectionAngleSlider_Value, Vector3D.AxisZ)

        ' resets rocket launch angle at initial value
        rocket.Rotate(-_lastLaunchAngleSlider_Value, New Vector3D(-Math.Sin(Utility.DegToRad(directionAngleSlider.Value)), Math.Cos(Utility.DegToRad(directionAngleSlider.Value)), 0))

        ' makes the rocket visible again in the scene
        rocket.Visible = True
        _lastDirectionAngleSlider_Value = 0
        _lastLaunchAngleSlider_Value = 0

        ' makes the targets invisible again in the scene
        model1.Entities(4).Visible = False
        model1.Entities(5).Visible = False
        model1.Entities(6).Visible = False

    End Sub

    ' slider that controls the angle for launching the rocket
    Private Sub LaunchAngleSlider_ValueChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of Double))
        If model1 Is Nothing Then Return
        Dim rocket As Entity = model1.Entities(0)
        rocket.Rotate(Utility.DegToRad(90 - launchAngleSlider.Value) - _lastLaunchAngleSlider_Value, New Vector3D(-Math.Sin(Utility.DegToRad(directionAngleSlider.Value)), Math.Cos(Utility.DegToRad(directionAngleSlider.Value)), 0))
        model1.Entities.Regen()
        model1.Invalidate()
        _lastLaunchAngleSlider_Value = Utility.DegToRad(90 - launchAngleSlider.Value)
        launchAngleNumLabel.Content = launchAngleSlider.Value.ToString()
    End Sub

    ' slider that controls the direction for launching the rocket
    Private Sub DirectionAngleSlider_ValueChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of Double))
        If model1 Is Nothing Then Return
        Dim rocket As Entity = model1.Entities(0)
        rocket.Rotate(Utility.DegToRad(directionAngleSlider.Value) - _lastDirectionAngleSlider_Value, Vector3D.AxisZ)
        model1.Entities.Regen()
        model1.Invalidate()
        _lastDirectionAngleSlider_Value = Utility.DegToRad(directionAngleSlider.Value)
        directionAngleNumLabel.Content = directionAngleSlider.Value.ToString()
    End Sub

    ' slider that controls the fire power for launching the rocket
    Private Sub FirePowerSlider_ValueChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of Double))
        If model1 Is Nothing Then Return
        firePowerNumLabel.Content = firePowerSlider.Value.ToString()
    End Sub

    ' this function fires the rocket
    Private Sub FireButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim rocketBlockReference As BlockReference = CType(model1.Entities(0), BlockReference)
        rocketBlockReference.Visible = False

        Dim trajectory As Parabolating = New Parabolating("Rocket", model1, Utility.DegToRad(launchAngleSlider.Value), Utility.DegToRad(directionAngleSlider.Value), firePowerSlider.Value)

        trajectory.vp.targets.Add(_target1)
        trajectory.vp.targets.Add(_target2)
        trajectory.vp.targets.Add(_target3)

        model1.Entities.Add(trajectory)

        If model1.Entities.Count > _numEntityInScene Then model1.Entities.RemoveAt(_numEntityInScene - 1)

        model1.StartAnimation(50)

        model1.Entities.Regen()
        model1.Invalidate()

        ChangeStateLaunchButtons(False)
    End Sub

    ' this function deletes the rocket already thrown and resets the rocket's position
    Private Sub ResetButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        If model1.Entities.Count > _numEntityInScene - 1 Then model1.Entities.RemoveAt(_numEntityInScene - 1)
        ResetSlidersLabelsValue()
        ChangeStateLaunchButtons(True)
        ResetScene()
        model1.Entities.Regen()
        model1.Invalidate()
    End Sub

End Class



Class Parabolating
    Inherits BlockReference

    Public tip As Point3D

    Private alpha As Double
    Public xPos, yPos, zPos As Double
    Private theta, phi, v As Double
    Private a As Double = -0.5
    Private time As Double
    Public vp As MySingleModel
    Public timeHitsGround As Integer

    Public Sub New(ByVal blockName As String, ByVal vp As MySingleModel, ByVal theta As Double, ByVal phi As Double, ByVal v As Double)
        MyBase.New(0, 0, 0, blockName, 1, 1, 1, 0)
        Me.vp = vp
        Me.theta = theta
        Me.phi = phi
        Me.v = v
        timeHitsGround = TimeRocketHitsGround(a, v, theta, 14) ' rocket's height = 14 --> look CreateRocket()
    End Sub

    ' this function calculates when the tip hits the ground. precision is limited by the fact frame number is an integer and not a double.
    Private Function TimeRocketHitsGround(ByVal a As Double, ByVal v As Double, ByVal theta As Double, ByVal c As Double) As Integer
        Dim b As Double = v * Math.Sin(theta)
        Dim delta As Double = b * b - 4 * a * c
        Dim result1 As Double = (-b + Math.Sqrt(delta)) / (2 * a)
        Dim result2 As Double = (-b - Math.Sqrt(delta)) / (2 * a)
        If result1 >= 0 Then Return CInt(result1) Else Return CInt(result2)
    End Function

    Protected Overrides Sub Animate(ByVal frameNumber As Integer)
        time = frameNumber

        xPos = v * Math.Cos(theta) * Math.Cos(phi) * time
        yPos = v * Math.Cos(theta) * Math.Sin(phi) * time
        zPos = v * Math.Sin(theta) * time + a * time * time

        vp.xPos = xPos
        vp.yPos = yPos
        vp.zPos = zPos

        Dim vXY As Double = v * Math.Cos(theta)
        Dim vZ As Double = v * Math.Sin(theta) + a * time * 2

        alpha = -Utility.RadToDeg(Math.Atan(vZ / vXY))

        tip = New Point3D(0, 0, 14)

        Dim t1 As Translation = New Translation(xPos, yPos, zPos)
        Dim t2 As Rotation = New Rotation(Utility.DegToRad(alpha + 90), New Vector3D(-Math.Sin(phi), Math.Cos(phi), 0))

        tip.TransformBy(t1 * t2)
    End Sub

    Public Overrides Sub MoveTo(ByVal data As DrawParams)
        MyBase.MoveTo(data)

        data.RenderContext.TranslateMatrixModelView(xPos, yPos, zPos)
        data.RenderContext.RotateMatrixModelView(alpha + 90, -Math.Sin(phi), Math.Cos(phi), 0)
    End Sub

    ' makes rocket always visible. Actual rocket position may go out of frustum.
    Public Overrides Function IsInFrustum(ByVal data As FrustumParams, ByVal center As Point3D, ByVal radius As Double) As Boolean
        Return True
    End Function
End Class

Public Class MySingleModel
    Inherits Model

    Public xPos, yPos, zPos As Double
    Public targets As List(Of Target) = New List(Of Target)()
    Private time As Integer

    Protected Overrides Sub OnAnimationTimerTick(ByVal stateInfo As Object)
        time += 1
        MyBase.OnAnimationTimerTick(stateInfo)

        Dim rocketLaunched As BlockReference = CType(Entities(9), BlockReference)

        Dim parabolating As Parabolating = CType(rocketLaunched, Parabolating)

        If parabolating IsNot Nothing AndAlso (parabolating.timeHitsGround - 2) = time Then
            StopAnimation()
            time = 0

            rocketLaunched.Visible = True
            Dim targetsArray As Target() = targets.ToArray()

            For i As Integer = 0 To 3 - 1
                If xPos > (targetsArray(i).xTarget - 7) AndAlso xPos < (targetsArray(i).xTarget + 5) AndAlso yPos > (targetsArray(i).yTarget - 7) AndAlso yPos < (targetsArray(i).yTarget + 5) Then
                    Entities(i + 4).Visible = True
                    Dispatcher.BeginInvoke(Sub() Invalidate())
                    Exit For
                End If
            Next
        End If
    End Sub
End Class

Public Class Target

    Public xTarget, yTarget As Double
    Private lpTarget As LinearPath

    Public Sub New()
    End Sub

    Public Function CreateTarget(ByVal xTarget As Double, ByVal yTarget As Double) As Entity
        Me.xTarget = xTarget
        Me.yTarget = yTarget

        lpTarget = New LinearPath(New Point3D(3, 3, 0), New Point3D(3, 8, 0), New Point3D(-3, 8, 0), New Point3D(-3, 3, 0), New Point3D(-8, 3, 0), New Point3D(-8, -3, 0), New Point3D(-3, -3, 0), New Point3D(-3, -8, 0), New Point3D(3, -8, 0), New Point3D(3, -3, 0), New Point3D(8, -3, 0), New Point3D(8, 3, 0), New Point3D(3, 3, 0))

        lpTarget.Rotate(-Math.PI / 4, Vector3D.AxisZ)
        lpTarget.Translate(xTarget, yTarget, 0)

        Return lpTarget
    End Function

    Public Function CreateHitRegion(ByVal xTarget As Double, ByVal yTarget As Double) As Entity
        Me.xTarget = xTarget
        Me.yTarget = yTarget

        Dim hitRegion As devDept.Eyeshot.Entities.Region = New devDept.Eyeshot.Entities.Region(lpTarget, Plane.XY)
        Return hitRegion
    End Function
End Class