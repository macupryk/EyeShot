Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Labels
Imports devDept.Geometry
Imports devDept.Graphics
Imports System.Drawing

Class DrawLight
    Private Type As Integer = -1
    ' 0: point, 1:spot, 2: directional, 3: directional Stationary
    Private NumLight As Integer = -1
    ' number of light assigned
    Public Light As LightSettings
    ' Light Settings of assigned light
    Private ModelOriginal As Model
    ' model of the final view
    Private CenterScene As Point3D
    ' center of the world scene in ModelOriginal
    Private Radius As Double
    ' distance radius choosen for drawing directional light(starting from the CenterScene)
    Private DrawnLight As Entity
    ' current Drawn light in the scene editor
    Private LightName As LeaderAndText
    ' Label shown of the drawn light
    Private LayerName As String
    ' The name of the Layer used for drawing the light

    Public Sub New(model As Model, numLight__1 As Integer, centerScene__2 As Point3D, radius__3 As Double, layerName__4 As String)
        ModelOriginal = model
        NumLight = numLight__1
        CenterScene = centerScene__2
        Radius = radius__3
        LayerName = layerName__4

        Select Case NumLight
            Case 1
                Light = ModelOriginal.Light1
                Exit Select
            Case 2
                Light = ModelOriginal.Light2
                Exit Select
            Case 3
                Light = ModelOriginal.Light3
                Exit Select
            Case 4
                Light = ModelOriginal.Light4
                Exit Select
            Case 5
                Light = ModelOriginal.Light5
                Exit Select
            Case 6
                Light = ModelOriginal.Light6
                Exit Select
            Case 7
                Light = ModelOriginal.Light7
                Exit Select
            Case 8
                Light = ModelOriginal.Light8
                Exit Select
        End Select
    End Sub

    Public Sub SetLight(type__1 As Integer, active As System.Nullable(Of Boolean), x As Double, y As Double, z As Double, dx As Double, _
        dy As Double, dz As Double, spotExponent As Double, linearAttenuation As Double, spotAngle As Double, yieldShadow As Boolean)
        If active IsNot Nothing Then
            Light.Active = CBool(active)
        Else
            Light.Active = False
        End If

        Type = type__1
        Light.Stationary = False

        ' sets the Spot Exponent value (used only in spot light)
        Light.SpotExponent = If((spotExponent < 128), spotExponent, 128)

        ' sets the Linear Attenuation value (used only in spot light)
        Light.LinearAttenuation = linearAttenuation

        ' sets the Angle value (used only in spot light)
        Light.SpotHalfAngle = Utility.DegToRad(spotAngle)

        ' sets if YieldShadow is active (only one light at time)
        Light.YieldShadow = yieldShadow

        ' sets the start Position of the light (used only in non-directional light)
        Light.Position = New Point3D(x, y, z)

        ' sets the direction of the light (used only in spot and directional light)
        If New Point3D(dx, dy, dz) <> Point3D.Origin Then
            Light.Direction = New Vector3D(dx, dy, dz)
            Light.Direction.Normalize()
        End If

        If Light.Active Then
            Select Case Type
                Case 0
                    Light.Active = True
                    Light.Type = lightType.Point
                    Light.Stationary = False
                    Exit Select
                Case 1
                    Light.Active = True
                    Light.Type = lightType.Spot
                    Light.Stationary = False
                    Exit Select
                Case 2
                    Light.Active = True
                    Light.Type = lightType.Directional
                    Light.Stationary = False
                    Exit Select
                Case 3
                    Light.Active = True
                    Light.Type = lightType.Directional
                    Light.Stationary = True
                    Exit Select

            End Select
        End If
    End Sub

    Public Sub Draw(model As Model)
        Select Case Type
            Case 0
                ' point
                DrawPoint(DrawnLight, LightName)
                Exit Select
            Case 1
                ' spot
                DrawSpot(DrawnLight, LightName)
                Exit Select
                ' Directional
            Case 2, 3
                ' Directional Stationary
                DrawDirectional(DrawnLight, LightName)
                Exit Select
        End Select

        If Light.Active Then
            DrawnLight.Color = Color.FromArgb(220, Color.Yellow)
        Else
            DrawnLight.Color = Color.FromArgb(100, Color.Gray)
        End If

        model.Entities.Add(DrawnLight, LayerName)
        model.Labels.Add(LightName)

        MoveIfStationary(model)
    End Sub

    Private Sub DrawPoint(ByRef drawnLight As Entity, ByRef lightName As LeaderAndText)
        ' draws point light like a Joint
        drawnLight = New Joint(Light.Position.X, Light.Position.Y, Light.Position.Z, 1, 1)
        drawnLight.ColorMethod = colorMethodType.byEntity

        ' draws name label
        lightName = New LeaderAndText(Light.Position + New Vector3D(0, 0, 1), "Light " + NumLight.ToString(), New Font("Tahoma", 8.25F), Color.White, New Vector2D(0, 15))
    End Sub

    Private Sub DrawSpot(ByRef drawnLight As Entity, ByRef lightName As LeaderAndText)
        Dim distance As Double, kl As Double, kc As Double, kq As Double

        kl = Light.LinearAttenuation
        kc = Light.ConstantAttenuation
        kq = Light.QuadraticAttenuation

        ' sets distance considering attenuation values of the light
        If kq.CompareTo(0.0) <> 0 Then
            distance = (-kl + Math.Sqrt(kl * kl - 4 * kq * kc)) / (2 * kq)
        Else
            distance = ((1 / 0.6) - kc) / kl
        End If

        ' draws spot light like a cone
        drawnLight = Mesh.CreateCone(Math.Tan(Light.SpotHalfAngle) * distance, 0, distance, 10)
        drawnLight.ColorMethod = colorMethodType.byEntity

        ' Aligns the direction of spot to the light direction
        Dim t As Transformation = New Align3D(Plane.XY, New Plane(Light.Direction * -1))
        drawnLight.Translate(0, 0, -distance)
        drawnLight.TransformBy(t)

        ' translates the light spot to choosen position
        drawnLight.Translate(Light.Position.X, Light.Position.Y, Light.Position.Z)

        ' draws name label
        lightName = New LeaderAndText(Light.Position, "Light " + NumLight.ToString(), New Font("Tahoma", 8.25F), Color.White, New Vector2D(0, 15))
    End Sub

    Private Sub DrawDirectional(ByRef drawnLight As Entity, ByRef lightName As LeaderAndText)
        ' sets start position of the drawn light
        Dim startPoint As Point3D = CType(CenterScene.Clone(), Point3D)
        startPoint.TransformBy(New Translation(Light.Direction * (-Radius)))

        ' draws directional light like an arrow
        drawnLight = Mesh.CreateArrow(startPoint, Light.Direction, 0.2, 5, 0.6, 3, _
            10, Mesh.natureType.Smooth, Mesh.edgeStyleType.Free)
        drawnLight.ColorMethod = colorMethodType.byEntity

        ' draws name label
        lightName = New LeaderAndText(startPoint + New Vector3D(0, 0, 0.2), "Light " + NumLight.ToString(), New Font("Tahoma", 8.25F), Color.White, New Vector2D(0, 15))
    End Sub

    Public Sub MoveIfStationary(model As Model)
        If DrawnLight IsNot Nothing AndAlso Light.Stationary Then
            DeletePrevious(model)

            ' gets world direction
            Dim direction As Single(), position As Single()
            Light.GetLightDirection(ModelOriginal.Camera.ModelViewMatrix, direction, position)
            Dim newDirection As New Vector3D(CDbl(direction(0)), CDbl(direction(1)), CDbl(direction(2)))
            newDirection.Negate()

            ' gets start point of the new drawn light
            Dim startNewPoint As Point3D = CType(CenterScene.Clone(), Point3D)
            startNewPoint.TransformBy(New Translation(newDirection * (-Radius)))

            ' draws new direction like an arrow
            DrawnLight = Mesh.CreateArrow(startNewPoint, newDirection, 0.2, 5, 0.6, 3, _
                10, Mesh.natureType.Smooth, Mesh.edgeStyleType.Free)
            DrawnLight.ColorMethod = colorMethodType.byEntity
            DrawnLight.Color = Color.FromArgb(220, Color.Yellow)

            ' draws name label
            LightName = New LeaderAndText(startNewPoint, "Light " + NumLight.ToString(), New Font("Tahoma", 8.25F), Color.White, New Vector2D(0, 15))

            model.Entities.Add(DrawnLight, LayerName)
            model.Labels.Add(LightName)
        End If
    End Sub

    Public Sub DeletePrevious(model As Model)
        If DrawnLight IsNot Nothing Then
            ' deletes previous light
            Dim index As Integer = model.Entities.IndexOf(DrawnLight)
            model.Entities(index).Selected = True
            model.Entities.DeleteSelected()

            ' deletes previous label
            Dim indexL As Integer = model.Labels.IndexOf(LightName)
            model.Labels(indexL).Selected = True
            model.Labels.DeleteSelected()
        End If
    End Sub
End Class

