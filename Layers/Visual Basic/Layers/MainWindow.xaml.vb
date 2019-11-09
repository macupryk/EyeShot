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
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Labels
Imports devDept.Geometry
Imports devDept.Graphics

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow

    Private Const fuselage As String = "fuselage", wings As String = "Wings", tail As String = "Tail", wires As String = "Wires"  

    Private tip As ToolTip
    Private lastIndex As Integer = -1
    Private cameraIsMoving As Boolean = False

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        tip = New ToolTip()
        AddHandler model1.MouseMove, AddressOf model1_MouseMove
        AddHandler model1.CameraMoveBegin, AddressOf model1_CameraMoveBegin
        AddHandler model1.CameraMoveEnd, AddressOf model1_CameraMoveEnd
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        ' edits default layer
        model1.Layers(0).Name = fuselage
        model1.Layers(0).Color = System.Drawing.Color.LightGray

        ' additional layers            
        model1.Layers.Add(wings, System.Drawing.Color.CornflowerBlue)
        model1.Layers.Add(tail, System.Drawing.Color.Chartreuse)
        model1.Layers.Add(wires)
        layerListView.SyncLayers()

        '#Region "Jet drawing"

        model1.Entities.Add(New Triangle(+15, -30, 8, 0, -30, 23, 0, -60, 8), fuselage, System.Drawing.Color.DeepSkyBlue)
        model1.Entities.Add(New Triangle(0, -60, 8, 0, -30, 23, -15, -30, 8), fuselage, System.Drawing.Color.DeepSkyBlue)
        model1.Entities.Add(New Triangle(-15, -30, 8, 0, -30, 23, 0, +56, 8), fuselage)
        model1.Entities.Add(New Triangle(0, +56, 8, 0, -30, 23, 15, -30, 8), fuselage)
        model1.Entities.Add(New Quad(0, +56, 8, +15, -30, 8, 0, -60, 8, -15, -30, 8), fuselage)


        model1.Entities.Add(New Triangle(0, -27, 10, -60, +8, 10, 60, +8, 10), wings)
        model1.Entities.Add(New Triangle(60, +8, 10, 0, +8, 15, 0, -27, 10), wings)
        model1.Entities.Add(New Triangle(60, +8, 10, -60, +8, 10, 0, +8, 15), wings)
        model1.Entities.Add(New Triangle(0, -27, 10, 0, +8, 15, -60, +8, 10), wings)


        model1.Entities.Add(New Triangle(-30, +57, 7.5, 30, +57, 7.5, 0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(0, +40, 7.5, 30, +57, 7.5, 0, +57, 12), tail)
        model1.Entities.Add(New Triangle(0, +57, 12, -30, +57, 7.5, 0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(30, +57, 7.5, -30, +57, 7.5, 0, +57, 12), tail)
        model1.Entities.Add(New Triangle(0, +40, 7.5, 3, +57, 8.5, 0, +65, 33), tail)
        model1.Entities.Add(New Triangle(0, +65, 33, -3, +57, 8.5, 0, +40, 7.5), tail)
        model1.Entities.Add(New Triangle(3, +57, 8.5, -3, +57, 8.5, 0, +65, 33), tail)



        Dim axis As New Line(-22, 0, 3, 22, 0, 3)

        axis.LineTypeMethod = colorMethodType.byEntity
        model1.LineTypes.Add("JetAxisPattern", New Single() {5, -1.5F, 0.25F, -1.5F})
        axis.LineTypeName = "JetAxisPattern"

        model1.Entities.Add(axis, wires)

        ' Points
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +12, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +16, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +21, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +27, 10, 4), wires)
        model1.Entities.Add(New devDept.Eyeshot.Entities.Point(-60, +34, 10, 4), wires)

        ' Wheels
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(+20, 0, 3), 3), wires)
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(-20, 0, 3), 3), wires)
        model1.Entities.Add(New Circle(Plane.YZ, New Point3D(0, -42, 2), 2), wires)

        ' Wheel crosses
        model1.Entities.Add(New Line(-20, 0, 2, -20, 0, 4), wires)
        model1.Entities.Add(New Line(-20, -1, 3, -20, +1, 3), wires)
        model1.Entities.Add(New Line(+20, 0, 2, +20, 0, 4), wires)
        model1.Entities.Add(New Line(+20, -1, 3, +20, +1, 3), wires)
        model1.Entities.Add(New Line(0, -41, 2, 0, -43, 2), wires)
        model1.Entities.Add(New Line(0, -42, 1, 0, -42, 3), wires)

        '#End Region

        ' Labels                        
        model1.Labels.Add(New LeaderAndText(+60, +8, 10, "Left wing", New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.Black, _
            New Vector2D(0, 30)))
        model1.Labels.Add(New ImageOnly(0, +65, 33, My.Resources.CautionLabel))

        ' Dimensions
        Dim dimPlane1 As Plane = Plane.XY
        dimPlane1.Rotate(Math.PI / 2, Vector3D.AxisZ, Point3D.Origin)
        Dim dimPlane2 As Plane = Plane.YZ
        dimPlane2.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin)

        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, -60, 8), New Point3D(60, 8, 8), New Point3D(70, -26, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, -60, 8), New Point3D(0, 65, 8), New Point3D(80, 0, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane1, New Point3D(0, 57, 8), New Point3D(0, 65, 8), New Point3D(70, 80, 8), 4), wires)
        model1.Entities.Add(New LinearDim(dimPlane2, New Point3D(0, 57, 8), New Point3D(0, 65, 33), New Point3D(0, 75, 20.5), 4), wires)

        model1.ZoomFit()
        model1.Invalidate()

        layerListView.Environment = model1

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub model1_CameraMoveBegin(sender As Object, e As Model.CameraMoveEventArgs)
        cameraIsMoving = True
    End Sub

    Private Sub model1_CameraMoveEnd(sender As Object, e As Model.CameraMoveEventArgs)
        cameraIsMoving = False
    End Sub

    Private Sub model1_MouseMove(sender As Object, e As MouseEventArgs)
        If cameraIsMoving Then
            Return
        End If

        Dim index As Integer = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))
        If index <> -1 AndAlso index <> lastIndex Then
            'hide the tooltip
            tip.IsOpen = False

            'get the entity
            Dim ent As Entity = model1.Entities(index)

            'get the entity type                
            Dim entType As String = ent.[GetType]().ToString().Split("."c).LastOrDefault()

            'show the tooltip with the entity info
            tip.Content = (entType & Convert.ToString(" ID: ")) + index.ToString()
            ToolTipService.SetToolTip(model1, tip)
            tip.IsOpen = True


            lastIndex = index
        End If
    End Sub

    Private Sub Model1_OnMouseLeave(sender As Object, e As MouseEventArgs)
        tip.IsOpen = false
    End Sub
End Class

