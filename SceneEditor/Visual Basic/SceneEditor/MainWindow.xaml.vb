Imports System.Collections.Generic
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Forms
Imports System.Windows.Media
Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Translators
Imports devDept.Geometry
Imports devDept.Graphics
Imports Button = System.Windows.Controls.Button
Imports ComboBox = System.Windows.Controls.ComboBox
Imports RadioButton = System.Windows.Controls.RadioButton
Imports TextBox = System.Windows.Controls.TextBox
Imports System.Globalization

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Private Lights As DrawLight()
    Private Camera As DrawCamera
    Public Sub New()
        InitializeComponent()
        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
        ' model2.Unlock("")

        AddHandler model1.WorkCompleted, AddressOf model1_WorkCompleted
        AddHandler model1.CameraMoveEnd, AddressOf model1_CameraMoveEnd

        ' sets origin symbol color and coordinate system color
        model2.GetOriginSymbol().LabelColor = New SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255))
        model2.GetCoordinateSystemIcon().LabelColor = New SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255))
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
       Dim rf as ReadFile = New ReadFile("../../../../../../dataset/Assets/Motherboard_ASRock_A330ION.eye")

        '''''' model1 settings (View)''''''''''''

        ' hides grids
        model1.GetGrid().Visible = False

        ' hides origin symbol
        model1.GetOriginSymbol().Visible = False

        ' sets trimetric view
        model1.SetView(viewType.Trimetric)

        ' loads the entities on the scene
        model1.StartWork(rf)

        ' shows color of each light
        colorPanel1.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light1.Color.R, model1.Light1.Color.G, model1.Light1.Color.B))
        colorPanel2.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light2.Color.R, model1.Light2.Color.G, model1.Light2.Color.B))
        colorPanel3.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light3.Color.R, model1.Light3.Color.G, model1.Light3.Color.B))
        colorPanel4.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light4.Color.R, model1.Light4.Color.G, model1.Light4.Color.B))
        colorPanel5.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light5.Color.R, model1.Light5.Color.G, model1.Light5.Color.B))
        colorPanel6.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light6.Color.R, model1.Light6.Color.G, model1.Light6.Color.B))
        colorPanel7.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light7.Color.R, model1.Light7.Color.G, model1.Light7.Color.B))
        colorPanel8.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.Light8.Color.R, model1.Light8.Color.G, model1.Light8.Color.B))
        AmbientLightPanel.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(model1.AmbientLight.R, model1.AmbientLight.G, model1.AmbientLight.B))

        ' shows the default lights settings

        Me.activeLight1.IsChecked = True                                      ' light active = true
        Me.lightType1.SelectedIndex = 3                                     ' light Type = DirectionalStationary
        Dim direction As Vector3D = model1.Light1.Direction
        Me.lightDX1.Text = direction.X.ToString(CultureInfo.CurrentCulture) ' X direction of the light
        Me.lightDY1.Text = direction.Y.ToString(CultureInfo.CurrentCulture) ' Y direction of the light
        Me.lightDZ1.Text = direction.Z.ToString(CultureInfo.CurrentCulture) ' Z direction of the light
        Me.yieldShadowRadio1.IsChecked = model1.Light1.YieldShadow   ' shadow projection of the light

        Me.activeLight2.IsChecked = True
        Me.lightType2.SelectedIndex = 3
        direction = model1.Light2.Direction
        Me.lightDX2.Text = direction.X.ToString(CultureInfo.CurrentCulture)
        Me.lightDY2.Text = direction.Y.ToString(CultureInfo.CurrentCulture)
        Me.lightDZ2.Text = direction.Z.ToString(CultureInfo.CurrentCulture)
        Me.yieldShadowRadio2.IsChecked = model1.Light2.YieldShadow

        Me.activeLight3.IsChecked = True
        Me.lightType3.SelectedIndex = 3
        direction = model1.Light3.Direction
        Me.lightDX3.Text = direction.X.ToString(CultureInfo.CurrentCulture)
        Me.lightDY3.Text = direction.Y.ToString(CultureInfo.CurrentCulture)
        Me.lightDZ3.Text = direction.Z.ToString(CultureInfo.CurrentCulture)
        Me.yieldShadowRadio3.IsChecked = model1.Light3.YieldShadow

        Me.lightType4.SelectedIndex = 0
        Me.lightType5.SelectedIndex = 0
        Me.lightType6.SelectedIndex = 0
        Me.lightType7.SelectedIndex = 0
        Me.lightType8.SelectedIndex = 0

        '''''' model2 settings (Scene Editor)''''''''''

        model2.GetGrid().Visible = False
        model2.SetView(viewType.Trimetric)

        ' disables planar reflection
        model2.Rendered.PlanarReflections = False

        ' hides silhouettes drawing
        model2.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never

        ' hides shadows drawing
        model2.Rendered.ShadowMode = shadowType.None

        ' sets the light1 to y direction of camera orientation
        model2.Light1.Color = System.Drawing.Color.LightGray
        model2.Light1.Direction = New Vector3D(0, 1, 0)
        model2.Light1.Stationary = True

        ' turns off Light2 and Light3
        model2.Light2.Active = False
        model2.Light3.Active = False

        ' adds 2 custom layers
        model2.Layers.Add(New Layer("Camera"))
        model2.Layers.Add(New Layer("Lights"))

        ' fits the model in the viewport 
        model2.ZoomFit()

        'refresh the model control
        model2.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub model1_WorkCompleted(sender As Object, e As WorkCompletedEventArgs)
        Dim rfa As ReadFileAsync = CType(e.WorkUnit, ReadFileAsync)

        ' adds MotherBoard Entities and Materials in view scene
        rfa.AddToScene(model1)

        model1.ZoomFit()

        ' updates the bounding box size values of the viewport
        model1.Entities.UpdateBoundingBox()

        ' copies all items inside model1's master collections to model2
        model1.CopyTo(model2)

        ' sets the center and the radius of external content sphere for drawing directional Lights in model2
        Dim max As Point3D = model1.Entities.BoxMax
        Dim min As Point3D = model1.Entities.BoxMin
        Dim center As New Point3D(min.X + (max.X - min.X) / 2, min.Y + (max.Y - min.Y) / 2, min.Z + (max.Z - min.Z) / 2)
        Dim radius As Double = Math.Sqrt(Math.Pow(max.X - min.X, 2) + Math.Pow(max.Y - min.Y, 2) + Math.Pow(max.Z - min.Z, 2))

        ' creates Light editors
        Lights = New DrawLight(7) {}
        For i As Integer = 0 To 7

            Lights(i) = New DrawLight(model1, i + 1, center, radius, "Lights")
        Next

        UpdateLights()

        model1_CameraMoveEnd(Nothing, Nothing)
        model2.ZoomFit()
    End Sub

    Private Sub model1_CameraMoveEnd(sender As Object, e As Model.CameraMoveEventArgs)
        ' removes previous camera drawing
        If Camera IsNot Nothing Then
            Camera.DeletePrevious(model2)
        End If

        ' draws new camera and new view model of model1 in model2
        Camera = New DrawCamera(model1.Viewports(0), model1.Size.Height, "Camera")
        Camera.Draw(model2)

        For i As Integer = 0 To 7

            Lights(i).MoveIfStationary(model2)
        Next

        model2.Entities.Regen()
        model2.Invalidate()
    End Sub

    Private Sub Settings_InputsChanged(sender As Object, e As EventArgs)
        UpdateLights()
    End Sub

    Private Sub EnableControls(active As System.Nullable(Of Boolean), type As ComboBox, x As TextBox, y As TextBox, z As TextBox, dx As TextBox, _
        dy As TextBox, dz As TextBox, spotExp As TextBox, linearAt As TextBox, spotAngle As Slider, colorButton As Button, _
        yieldShadow As RadioButton)
        If active IsNot Nothing AndAlso CBool(active) Then
            type.IsEnabled = True
            colorButton.IsEnabled = True

            Select Case type.SelectedIndex
                Case 0
                    'point light settings
                    x.IsEnabled = True
                    y.IsEnabled = True
                    z.IsEnabled = True
                    dx.IsEnabled = False
                    dy.IsEnabled = False
                    dz.IsEnabled = False
                    spotExp.IsEnabled = False
                    linearAt.IsEnabled = False
                    spotAngle.IsEnabled = False
                    yieldShadow.IsEnabled = False
                    yieldShadow.IsChecked = False
                    Exit Select
                Case 1
                    'spot light settings
                    x.IsEnabled = True
                    y.IsEnabled = True
                    z.IsEnabled = True
                    dx.IsEnabled = True
                    dy.IsEnabled = True
                    dz.IsEnabled = True
                    spotExp.IsEnabled = True
                    linearAt.IsEnabled = True
                    spotAngle.IsEnabled = True
                    yieldShadow.IsEnabled = True
                    Exit Select
                Case 2
                    'directional light settings
                    x.IsEnabled = False
                    y.IsEnabled = False
                    z.IsEnabled = False
                    dx.IsEnabled = True
                    dy.IsEnabled = True
                    dz.IsEnabled = True
                    spotExp.IsEnabled = False
                    linearAt.IsEnabled = False
                    spotAngle.IsEnabled = False
                    yieldShadow.IsEnabled = True
                    Exit Select
                Case 3
                    'directional stationary light settings
                    x.IsEnabled = False
                    y.IsEnabled = False
                    z.IsEnabled = False
                    dx.IsEnabled = True
                    dy.IsEnabled = True
                    dz.IsEnabled = True
                    spotExp.IsEnabled = False
                    linearAt.IsEnabled = False
                    spotAngle.IsEnabled = False
                    yieldShadow.IsEnabled = True
                    Exit Select
            End Select
        Else
            ' Light turn off
            type.IsEnabled = False
            x.IsEnabled = False
            y.IsEnabled = False
            z.IsEnabled = False
            dx.IsEnabled = False
            dy.IsEnabled = False
            dz.IsEnabled = False
            spotExp.IsEnabled = False
            linearAt.IsEnabled = False
            spotAngle.IsEnabled = False
            colorButton.IsEnabled = False
            yieldShadow.IsEnabled = False
            yieldShadow.IsChecked = False
        End If
    End Sub

    Private Sub ChangeSettings(indexLight As Integer, active As System.Nullable(Of Boolean), type As ComboBox, xt As TextBox, yt As TextBox, zt As TextBox, _
        dxt As TextBox, dyt As TextBox, dzt As TextBox, spotExp As TextBox, linearAt As TextBox, spotAngle As Slider, _
        colorButton As Button, yieldShadow As RadioButton)
        ' enables/disables light settings by the type of Light
        EnableControls(active, type, xt, yt, zt, dxt, _
            dyt, dzt, spotExp, linearAt, spotAngle, colorButton, _
            yieldShadow)

        Dim exp As Double, linear As Double, x As Double, y As Double, z As Double, dx As Double, _
            dy As Double, dz As Double

        [Double].TryParse(spotExp.Text, exp)
        [Double].TryParse(linearAt.Text, linear)

        ' position values
        [Double].TryParse(xt.Text, x)
        [Double].TryParse(yt.Text, y)
        [Double].TryParse(zt.Text, z)

        ' direction values
        [Double].TryParse(dxt.Text, dx)
        [Double].TryParse(dyt.Text, dy)
        [Double].TryParse(dzt.Text, dz)

        ' sets the Light values
        Lights(indexLight - 1).SetLight(type.SelectedIndex, active, x, y, z, dx, _
            dy, dz, exp, linear, spotAngle.Value, yieldShadow.IsChecked)

        model1.Invalidate()
        model2.Invalidate()
    End Sub

    Private Sub UpdateLights()
        If Lights Is Nothing Then
            Return
        End If

        ChangeSettings(1, activeLight1.IsChecked, lightType1, lightX1, lightY1, lightZ1, _
            lightDX1, lightDY1, lightDZ1, lightExponent1, lightLinearA1, lightAngle1, _
            colorButton_1, yieldShadowRadio1)
        ChangeSettings(2, activeLight2.IsChecked, lightType2, lightX2, lightY2, lightZ2, _
            lightDX2, lightDY2, lightDZ2, lightExponent2, lightLinearA2, lightAngle2, _
            colorButton_2, yieldShadowRadio2)
        ChangeSettings(3, activeLight3.IsChecked, lightType3, lightX3, lightY3, lightZ3, _
            lightDX3, lightDY3, lightDZ3, lightExponent3, lightLinearA3, lightAngle3, _
            colorButton_3, yieldShadowRadio3)
        ChangeSettings(4, activeLight4.IsChecked, lightType4, lightX4, lightY4, lightZ4, _
            lightDX4, lightDY4, lightDZ4, lightExponent4, lightLinearA4, lightAngle4, _
            colorButton_4, yieldShadowRadio4)
        ChangeSettings(5, activeLight5.IsChecked, lightType5, lightX5, lightY5, lightZ5, _
            lightDX5, lightDY5, lightDZ5, lightExponent5, lightLinearA5, lightAngle5, _
            colorButton_5, yieldShadowRadio5)
        ChangeSettings(6, activeLight6.IsChecked, lightType6, lightX6, lightY6, lightZ6, _
            lightDX6, lightDY6, lightDZ6, lightExponent6, lightLinearA6, lightAngle6, _
            colorButton_6, yieldShadowRadio6)
        ChangeSettings(7, activeLight7.IsChecked, lightType7, lightX7, lightY7, lightZ7, _
            lightDX7, lightDY7, lightDZ7, lightExponent7, lightLinearA7, lightAngle7, _
            colorButton_7, yieldShadowRadio7)
        ChangeSettings(8, activeLight8.IsChecked, lightType8, lightX8, lightY8, lightZ8, _
            lightDX8, lightDY8, lightDZ8, lightExponent8, lightLinearA8, lightAngle8, _
            colorButton_8, yieldShadowRadio8)

        DrawLights()
    End Sub

    Private Sub DrawLights()
        For i As Integer = 0 To 7
            Lights(i).DeletePrevious(model2)
            Lights(i).Draw(model2)
        Next
        model2.Invalidate()
    End Sub

    Private Sub YieldShadowButtons_CheckedChanged(sender As Object, e As EventArgs)
        If Lights Is Nothing Then
            Return
        End If

        Dim indexLight As Integer = 0

        ' sets the yieldShadow to only one Light (not supported in Wpf yet)
        If yieldShadowRadio1.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio1.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio2.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio2.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio3.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio3.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio4.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio4.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio5.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio5.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio6.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio6.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio7.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio7.IsChecked
        End If
        indexLight += 1
        If yieldShadowRadio8.IsChecked IsNot Nothing Then
            Lights(indexLight).Light.YieldShadow = yieldShadowRadio8.IsChecked
        End If

        model1.Invalidate()
    End Sub

    Private Sub colorButtons_Click(sender As Object, e As EventArgs)
        Dim colorDialog As New ColorDialog()
        Dim indexLight As Integer

        ' gets index Light from button Name
        Integer.TryParse(CType(sender, Button).Name.Split("_"c)(1), indexLight)
        colorDialog.Color = Lights(indexLight - 1).Light.Color

        ' gets and sets color of Light
        If colorDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Lights(indexLight - 1).Light.Color = colorDialog.Color
            Select Case indexLight
                Case 1
                    colorPanel1.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 2
                    colorPanel2.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 3
                    colorPanel3.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 4
                    colorPanel4.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 5
                    colorPanel5.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 6
                    colorPanel6.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 7
                    colorPanel7.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
                Case 8
                    colorPanel8.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
                    Exit Select
            End Select
        End If
        model1.Invalidate()
    End Sub

    Private Sub ambientLightButton_Click(sender As Object, e As EventArgs)
        Dim colorDialog As New ColorDialog()

        ' gets and sets AmbientLight color
        If colorDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            model1.AmbientLight = colorDialog.Color
            AmbientLightPanel.Fill = New SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B))
        End If
        model1.Invalidate()
    End Sub
End Class
