Imports System.Collections.Generic
Imports System.Resources.ResourceManager
Imports System.Drawing.Drawing2D
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
Imports devDept.Geometry
Imports devDept.Graphics

Imports System.Windows.Forms
Imports Color = System.Drawing.Color
Imports Pen = System.Drawing.Pen
Imports RectangleF = System.Drawing.RectangleF
Imports Rectangle = System.Drawing.Rectangle
Imports Font = System.Drawing.Font
Imports Bitmap = System.Drawing.Bitmap
Imports Size = System.Drawing.Size

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Private secondCamera As Camera

    Private hdlView1 As MyHiddenLinesViewPrint, hdlView2 As MyHiddenLinesViewPrint

    ' Pens used to draw the lines
    Private PenEdge As Pen, PenSilho As Pen, PenWire As Pen

    Private WithEvents printDocument1 As System.Drawing.Printing.PrintDocument

    Private Font As Font

    Public Sub New()
        InitializeComponent()

        ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        Font = New Font("FontFamily", 14, System.Drawing.FontStyle.Regular)
        printDocument1 = New System.Drawing.Printing.PrintDocument()
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)

        ' Creates the pens
        PenSilho = New Pen(System.Drawing.Color.Black, 3.0F)
        PenEdge = New Pen(System.Drawing.Color.Black, 1.0F)
        PenWire = New Pen(System.Drawing.Color.Black, 1.0F)
        PenEdge.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round)
        PenSilho.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round)
        PenWire.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round)


        model1.GetGrid().AutoSize = True
        model1.GetGrid().[Step] = 50
        model1.Camera.FocalLength = 30
        model1.Camera.ProjectionMode = projectionType.Perspective        
        model1.SetView(viewType.Trimetric)
        model1.ZoomFit()
        model1.Invalidate()

        ' Imports an Ascii model
        Dim rf As devDept.Eyeshot.Translators.ReadFile = New devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/house.eye")
        rf.DoWork()
        model1.Entities.AddRange(rf.Entities, Color.Gray)

        ' Changes the color/material of the fifth entity
        rf.Entities(5).Color = Color.Pink

        model1.ZoomFit()
        comboBoxPrintMode.SelectedIndex = 0

        MyBase.OnContentRendered(e)
    End Sub

    Private Sub printButton_Click(sender As Object, e As EventArgs)
        ' Defines the camera for the second view                        
        secondCamera = New Camera(New Point3D(320, 0, 160), 600, New Quaternion(Vector3D.AxisZ, 90), projectionType.Orthographic, 50, 1)

        If comboBoxPrintMode.SelectedIndex = 0 Then
	        ' Vector printing
	        hdlView1 = New MyHiddenLinesViewPrint(New HiddenLinesViewSettings(model1.Viewports(0), model1, 0.1, True, PenSilho, PenEdge, _
		        PenWire, False))
	        model1.StartWork(hdlView1)
        Else
	        ' Raster printing
	        secondCamera.Move(50, 50, 0)

	        ' Prints the page
	        Print()
        End If
    End Sub

    Private Sub model1_WorkCompleted(sender As Object, e As WorkCompletedEventArgs)
        If Object.ReferenceEquals(e.WorkUnit, hdlView1) Then

            Dim prevCam = model1.Viewports(0).Camera

            model1.Viewports(0).Camera = secondCamera

            hdlView2 = New MyHiddenLinesViewPrint(New HiddenLinesViewSettings(model1.Viewports(0), model1, 0.1, PenSilho, PenEdge, PenWire, False))

            model1.Viewports(0).Camera = prevCam

            ' Runs the hidden lines computation for the second view
            model1.StartWork(hdlView2)

        ElseIf Object.ReferenceEquals(e.WorkUnit, hdlView2) Then
            ' Prints the page
            Print()
        End If

    End Sub
    
    Public Sub Print()
        Dim ppDlg As New PrintPreviewDialog()

        ppDlg.Document = printDocument1

        ' Sets the property to true to have the drawing correctly centered on the page
        printDocument1.OriginAtMargins = True

        Try
            ppDlg.ShowDialog()
        Catch ex As Exception
            System.Windows.MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub printDocument1_PrintPage(sender As Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles printDocument1.PrintPage
        Dim printable As RectangleF = e.MarginBounds

        ' Since PrintDocument.OriginAtMargins = True, sets top-Left corner to (0,0)
        printable.X = 0
        printable.Y = 0

        ' Draws the logo
        Dim logoSize As Integer = 70
        e.Graphics.DrawImage(My.Resources.logo, printable.Right - logoSize, printable.Y, logoSize, logoSize)

        ''
        ' Draws the main title and some text
        ''

        Dim title As String = "Advanced Printing sample"

        Dim titleFont As New Font(Font.FontFamily, 30, System.Drawing.FontStyle.Regular)

        Dim stringSize As System.Drawing.SizeF = e.Graphics.MeasureString(title, titleFont)

        Dim nextY As Single = printable.Top + logoSize / 2

        e.Graphics.DrawString(title, titleFont, System.Drawing.Brushes.Blue, printable.Left + printable.Width / 2 - stringSize.Width / 2, nextY)

        Dim verticalOffset As Integer = 60

        nextY += stringSize.Height + 20

        Dim textFont As New Font(Font.FontFamily, 12, System.Drawing.FontStyle.Regular)

        Dim text As String = "This sample demonstrates how to draw different views of the same model in the proper page area."
        stringSize = e.Graphics.MeasureString(text, textFont)

        e.Graphics.DrawString(text, textFont, System.Drawing.Brushes.Black, New RectangleF(printable.X, nextY, printable.Width, printable.Height))

        nextY += stringSize.Height + verticalOffset

        ' Defines a margin
        Dim marginFromBorder As Integer = 5

        ' Draw the views
        If comboBoxPrintMode.SelectedIndex = 0 Then
            ' Vector
            PrintPageVector(sender, e, nextY, verticalOffset, printable, marginFromBorder)
        Else
            ' Raster
            PrintPageRaster(sender, e, nextY, verticalOffset, printable, marginFromBorder)
        End If

        ''
        ' Draws some other text
        ''

        Dim titleFont2 As New Font(Font.FontFamily, 20, System.Drawing.FontStyle.Bold)
        Dim title2 As String = "Window opening details"

        Dim title2Size As System.Drawing.SizeF = e.Graphics.MeasureString(title2, titleFont2)

        e.Graphics.DrawString(title2, titleFont2, System.Drawing.Brushes.Blue, printable.Left + printable.Width / 2 - title2Size.Width / 2, nextY + verticalOffset)

        nextY += 2 * (marginFromBorder + verticalOffset)

        text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

        e.Graphics.DrawString(text, textFont, System.Drawing.Brushes.Black, New RectangleF(printable.Left, nextY, printable.Width / 2 - 20, printable.Height - nextY))
    End Sub

    Private Sub pageSetupButton_Click(sender As Object, e As RoutedEventArgs)
        Dim pageSetupDialog As New PageSetupDialog()

        pageSetupDialog.Document = printDocument1
        pageSetupDialog.AllowMargins = True

        If pageSetupDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            printDocument1 = pageSetupDialog.Document
        End If
    End Sub

    Private Sub exportToEMFButton_Click(sender As Object, e As RoutedEventArgs)
        
        ' It is possible to save in DWG / DXF with the HiddenLinesViewOnFileAutodesk class available in x86 and x64 dlls

        model1.WriteToFileVector(true, "house.emf")
    End Sub

    Private Sub PrintPageVector(sender As Object, e As System.Drawing.Printing.PrintPageEventArgs, ByRef nextY As Single, verticalOffset As Integer, printable As RectangleF, marginFromBorder As Integer)
        ''
        ' First View
        ''

        hdlView1.PrintRect = New RectangleF(printable.Left, nextY, printable.Width - 20, 200)

        ' Draws the first view
        DrawViewFrame(e.Graphics, "First View", hdlView1.PrintRect, marginFromBorder)

        hdlView1.Print(e)

        nextY += 200

        ''
        ' Second View
        ''

        hdlView2.PrintRect = New RectangleF((printable.Left + printable.Width / 2), nextY + 2 * (marginFromBorder + verticalOffset), printable.Width / 2 - 20, 400)

        ' Draws the second view
        DrawViewFrame(e.Graphics, "Second View", hdlView2.PrintRect, marginFromBorder)

        hdlView2.Print(e)
    End Sub

    Private Sub PrintPageRaster(sender As Object, e As System.Drawing.Printing.PrintPageEventArgs, ByRef nextY As Single, verticalOffset As Integer, printable As RectangleF, marginFromBorder As Integer)
        ''
        ' First View
        ''

        Dim printRect1 As New RectangleF(printable.Left, nextY, printable.Width - 20, 200)

        ' Draws the first view with a 4x resolution for better quality
        Dim bmp1 As Bitmap = model1.RenderToBitmap(New Size(CInt(printRect1.Width) * 4, CInt(printRect1.Height) * 4))

        Dim scaledSize As System.Drawing.Size
        ScaleImageSizeToPrintRect(printRect1, bmp1.Size, scaledSize)

        DrawViewFrame(e.Graphics, "First View", printRect1, marginFromBorder)
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
        e.Graphics.DrawImage(bmp1, CInt(printRect1.Left + (printRect1.Width - scaledSize.Width) / 2), CInt(printRect1.Top + (printRect1.Height - scaledSize.Height) / 2), scaledSize.Width, scaledSize.Height)

        nextY += 200

        ''
        ' Second View
        ''

        Dim oldCamera As Camera = model1.Viewports(0).Camera
        model1.Viewports(0).Camera = secondCamera

        ' Draws the second view
        Dim printRect2 As New RectangleF((printable.Left + printable.Width / 2), nextY + 2 * (marginFromBorder + verticalOffset), printable.Width / 2 - 20, 400)

        ' Set the second view viewport size
        Dim oldsize As Size = model1.Size
        model1.Size = New Size(200, 250)

        ' Draws the second view with a 4x resolution for better quality
        Dim bmp2 As Bitmap = model1.RenderToBitmap(New Size(CInt(printRect2.Width) * 1, CInt(printRect2.Height) * 1))

        ' restore previous view viewport size
        model1.Size = oldsize

        ScaleImageSizeToPrintRect(printRect2, bmp2.Size, scaledSize)

        DrawViewFrame(e.Graphics, "Second View", printRect2, marginFromBorder)

        e.Graphics.DrawImage(bmp2, CInt(printRect2.Left + (printRect2.Width - scaledSize.Width) / 2), CInt(printRect2.Top + (printRect2.Height - scaledSize.Height) / 2), scaledSize.Width, scaledSize.Height)

        ' restore previous camera
        model1.Viewports(0).Camera = oldCamera
        model1.Invalidate()
    End Sub

    Private Sub ScaleImageSizeToPrintRect(printRect As RectangleF, imageSize As System.Drawing.Size, ByRef scaledSize As System.Drawing.Size)
        Dim width As Double, height As Double
        Dim ratio As Double

        ' fit the width of the image inside the width of the print Rectangle
        ratio = printRect.Width / imageSize.Width
        width = imageSize.Width * ratio
        height = imageSize.Height * ratio

        ' fit the other dimension
        If height > printRect.Height Then
            ratio = printRect.Height / height
            width *= ratio
            height *= ratio
        End If

        scaledSize = New System.Drawing.Size(CInt(width), CInt(height))
    End Sub

    Private Sub DrawViewFrame(graphics As System.Drawing.Graphics, title As String, printRect As RectangleF, marginFromBorder As Integer)
        Dim borderRectangle As New Rectangle(CInt(printRect.X - marginFromBorder), CInt(printRect.Y - marginFromBorder), CInt(printRect.Width + 2 * marginFromBorder), CInt(printRect.Height + 2 * marginFromBorder))

        ' Draws the view title
        Dim titleSize As System.Drawing.SizeF = graphics.MeasureString(title, Font)
        graphics.DrawString(title, Font, System.Drawing.Brushes.Black, borderRectangle.Left + borderRectangle.Width / 2 - titleSize.Width / 2, borderRectangle.Top - titleSize.Height)

        ' Draws a shadow rectangle
        graphics.FillRectangle(System.Drawing.Brushes.LightGray, borderRectangle.Left + 10, borderRectangle.Top + 10, borderRectangle.Width, borderRectangle.Height)
        graphics.FillRectangle(System.Drawing.Brushes.White, borderRectangle)

        ' Draws the border of the viewport
        Dim borderPen As New Pen(System.Drawing.Color.Gray, 1)
        graphics.DrawRectangle(borderPen, borderRectangle)

    End Sub

    Private Sub comboBoxPrintMode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim ci As ComboBoxItem = DirectCast(comboBoxPrintMode.SelectedItem, ComboBoxItem)
        Dim value As String = ci.Content.ToString()
        exportToEMFButton.IsEnabled = value.Equals("Vector", StringComparison.InvariantCultureIgnoreCase)
    End Sub

    Private Class MyHiddenLinesViewPrint
        Inherits HiddenLinesViewOnPaper
        Public Sub New(data As HiddenLinesViewSettings)

            MyBase.New(data)
        End Sub

        Protected Overrides Sub WorkCompleted(model As Environment)
            ' Avoid the automatic printing
        End Sub
    End Class

End Class

