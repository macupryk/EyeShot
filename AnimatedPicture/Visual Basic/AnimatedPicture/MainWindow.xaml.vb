Imports System.Drawing
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports System.Drawing.Image
Imports System.Windows.Threading

Class MainWindow
    Inherits Window
    Private _timer As System.Windows.Threading.DispatcherTimer
    Private _imageIndex As Integer = 0
    Private _pct As Picture

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        ' Hides the edges
        model1.Rendered.ShowEdges = False        

        ' Adds the pattern for the lines
        Model1.LineTypes.Add("DashDot", New Single() {5, -1, 1, -1})
        
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)

        ' Adds the picture
        _pct = New Picture(Plane.XY, 100, 100, new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic1.png"))
        _pct.Lighted = False
        model1.Entities.Add(_pct)

        ' Adds the custom circle
        Dim c1 As New MyCircle(68, 11, 1, 46)
        model1.Entities.Add(c1, System.Drawing.Color.Red)

        ' Adds the custom lines        
        Dim myLn1 As New MyLine(c1.Center.X - c1.Radius * 1.1, c1.Center.Y, c1.Center.X + c1.Radius * 1.1, c1.Center.Y)
        myLn1.LineTypeMethod = colorMethodType.byEntity
        myLn1.LineTypeName = "DashDot"
        model1.Entities.Add(myLn1, System.Drawing.Color.Green)

        Dim myLn2 As New MyLine(c1.Center.X, c1.Center.Y - c1.Radius * 1.1, c1.Center.X, c1.Center.Y + c1.Radius * 1.1)        
        myLn2.LineTypeMethod = colorMethodType.byEntity
        myLn2.LineTypeName = "DashDot"                
        model1.Entities.Add(myLn2, System.Drawing.Color.Green)

        ' Sets top view and fits the model in the viewport
        model1.SetView(viewType.Top, True, False)        

        ' Refreshes the model control
        model1.Invalidate()

        ' Starts the timer to update the picture
        _timer = new DispatcherTimer(DispatcherPriority.Normal)
        AddHandler _timer.Tick, AddressOf Timer_Tick1
        _timer.Interval = TimeSpan.FromMilliseconds(120)
        _timer.Start()

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub Timer_Tick1(sender As Object, e As EventArgs)

        Select Case _imageIndex
            Case 0
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic1.png")
                Exit Select
            Case 1
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic2.png")
                Exit Select
            Case 2
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic3.png")
                Exit Select
            Case 3
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic4.png")
                Exit Select
            Case 4
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic5.png")
                Exit Select
            Case 5
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic6.png")
                Exit Select
            Case 6
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic7.png")
                Exit Select
            Case 7
                _pct.Image = new Bitmap("../../../../../../dataset/Assets/Pictures/AnimPic8.png")
                _imageIndex = -1
                Exit Select
        End Select

        _imageIndex += 1
        
        ' Compiles the picture in the main thread to avoid an access violation exception.
        Dispatcher.BeginInvoke(
            Sub()
                RefreshPicture()
            End Sub
            )
    End Sub

    Private Sub RefreshPicture()
        ' Compiles the picture and refreshes the Model.
        _pct.RegenMode = regenType.CompileOnly
        _pct.Compile(New CompileParams(model1))
        model1.Invalidate()
    End Sub
    

    Private Sub Window_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs)
        ' Stops the timer
        _timer.Stop()
    End Sub
End Class

#Region "Custom classes"

Friend Class MyCircle
    Inherits Circle
    Public Sub New(x As Double, y As Double, z As Double, radius As Double)
        MyBase.New(x, y, z, radius)
    End Sub

    Protected Overrides Sub Draw(data As DrawParams)
        data.RenderContext.EndDrawBufferedLines()

        data.RenderContext.PushDepthStencilState()
        data.RenderContext.SetState(depthStencilStateType.DepthTestAlways)

        MyBase.Draw(data)

        data.RenderContext.EndDrawBufferedLines()
        data.RenderContext.PopDepthStencilState()
    End Sub
End Class


Friend Class MyLine
    Inherits devDept.Eyeshot.Entities.Line    

    Public Sub New(x1 As Double, y1 As Double, x2 As Double, y2 As Double)
        MyBase.New(x1, y1, x2, y2)
    End Sub

    Protected Overrides Sub Draw(data As DrawParams)
        data.RenderContext.EndDrawBufferedLines()

        data.RenderContext.PushDepthStencilState()
        data.RenderContext.SetState(depthStencilStateType.DepthTestAlways)

        MyBase.Draw(data)

        data.RenderContext.EndDrawBufferedLines()
        data.RenderContext.PopDepthStencilState()
    End Sub
End Class

#End Region
