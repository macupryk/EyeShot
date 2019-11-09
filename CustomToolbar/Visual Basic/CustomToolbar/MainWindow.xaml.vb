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
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Collections.ObjectModel
Imports System.IO
Imports devDept.Eyeshot.Translators

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.            

            ' removes some of the standard Eyeshot buttons
            Dim buttons As List(Of devDept.Eyeshot.ToolBarButton) = New List(Of ToolBarButton)()

            Dim leftBmp As New BitmapImage(GetUriFromResource("previous.png"))
            buttons.Add(New devDept.Eyeshot.ToolBarButton(leftBmp, "PreviousViewButton", "Previous View", devDept.Eyeshot.ToolBarButton.styleType.PushButton, True))

            Dim rightBmp As New BitmapImage(GetUriFromResource("next.png"))
            buttons.Add(New devDept.Eyeshot.ToolBarButton(rightBmp, "NextViewButton", "Next View", devDept.Eyeshot.ToolBarButton.styleType.PushButton, True))

            ' Add a separator button            
            buttons.Add(New devDept.Eyeshot.ToolBarButton(Nothing, "Separator", "", devDept.Eyeshot.ToolBarButton.styleType.Separator, True))

            buttons.Add(model1.GetToolBar().Buttons(0))
            buttons.Add(model1.GetToolBar().Buttons(1))

            ' Add a separator button            
            buttons.Add(New devDept.Eyeshot.ToolBarButton(Nothing, "Separator", "", devDept.Eyeshot.ToolBarButton.styleType.Separator, True))

            Dim usersBmp As New BitmapImage(GetUriFromResource("users.png"))
            buttons.Add(New devDept.Eyeshot.ToolBarButton(usersBmp, "MyPushButton", "MyPushButton", devDept.Eyeshot.ToolBarButton.styleType.PushButton, True))

            Dim gearsBmp As New BitmapImage(GetUriFromResource("gears.png"))
            buttons.Add(New devDept.Eyeshot.ToolBarButton(gearsBmp, "MyToggleButton", "MyToggleButton", devDept.Eyeshot.ToolBarButton.styleType.ToggleButton, True))

            model1.GetToolBar().Buttons = New ToolBarButtonList(model1.GetToolBar(), buttons)
        End Sub

        Private Function GetUriFromResource(resourceFilename As String) As Uri
            Return New Uri(Convert.ToString("pack://application:,,,/Resources/") & resourceFilename)
        End Function

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' adds a custom mesh to the CoordinateSystemIcon
            model1.GetCoordinateSystemIcon().Entities.Clear()

            ' reads the mesh obj (eyeshot file) from the Assets directory
            Dim rf As New devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/figure_Object011.eye")
            rf.DoWork()
            Dim torso As Mesh = DirectCast(rf.Entities(0), Mesh)
            torso.Scale(0.5, 0.5, 0.5)
            torso.NormalAveragingMode = Mesh.normalAveragingType.Averaged
            torso.Regen(0.1)

            ' orients the mesh
            Dim midPoint As Point3D = (torso.BoxMin + torso.BoxMax) / 2
            torso.Translate(-midPoint.X, -midPoint.Y, -midPoint.Z)
            torso.Rotate(Math.PI / 2, Vector3D.AxisX)

            ' sets the color            
            torso.Color = System.Drawing.Color.Pink

            ' sets the model on the CoordinateSystemIcon entities and remove the labels
            Dim csi As CoordinateSystemIcon = model1.GetCoordinateSystemIcon()
            csi.Entities.Add(torso)
            csi.LabelAxisX = ""
            csi.LabelAxisY = ""
            csi.LabelAxisZ = ""
            csi.Lighting = True

            model1.CompileUserInterfaceElements()
            ' sets my event handler to the ToolBarButton.Click event

            ' Previous view
            AddHandler model1.GetToolBar().Buttons(0).Click, AddressOf PreviousViewClickEventHandler

            ' Next view
            AddHandler model1.GetToolBar().Buttons(1).Click, AddressOf NextViewClickEventHandler

            ' MyPushButton
            AddHandler model1.GetToolBar().Buttons(6).Click, AddressOf MyPushButtonClickEventHandler

            ' MyToggleButton
            AddHandler model1.GetToolBar().Buttons(7).Click, AddressOf MyToggleButtonClickEventHandler           

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            model1.ZoomFit()

            'refresh the viewport
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Public Sub PreviousViewClickEventHandler(sender As Object, e As EventArgs)
            model1.PreviousView()
        End Sub

        Public Sub NextViewClickEventHandler(sender As Object, e As EventArgs)
            model1.NextView()
        End Sub

        Public Sub MyPushButtonClickEventHandler(sender As Object, e As EventArgs)
            MessageBox.Show("You clicked the custom PushButton.")
        End Sub

        Public Sub MyToggleButtonClickEventHandler(sender As Object, e As EventArgs)
            If DirectCast(sender, devDept.Eyeshot.ToolBarButton).Pushed Then
                MessageBox.Show("You pressed the custom ToggleButton.")
            Else
                MessageBox.Show("You un-pressed the custom ToggleButton.")
            End If
        End Sub
    End Class
End Namespace