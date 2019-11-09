Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Input
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports System.Drawing

Public Class MyModel
    Inherits devDept.Eyeshot.Model
    Private lowerLeftWorldCoord As Point2D, upperRightWorldCoord As Point2D
    Private originScreen As Point2D

    Public Enum rulerPlaneType
        ZX
        YZ
        XY
    End Enum

    Public RulerPlaneMode As rulerPlaneType
    Private rulerPlane As Plane

    Protected Overrides Sub DrawViewport(myParams As DrawSceneParams)
        MyBase.DrawViewport(myParams)

        Select Case RulerPlaneMode
            Case rulerPlaneType.XY
                rulerPlane = Plane.XY
                Exit Select
            Case rulerPlaneType.ZX
                rulerPlane = Plane.ZX
                Exit Select
            Case rulerPlaneType.YZ
                rulerPlane = Plane.YZ
                Exit Select
        End Select

        ' Get the world coordinates of the corners and the screen position of the origin for reference

        Dim ptUpperRight As Point3D, ptLowerLeft As Point3D
        ScreenToPlane(New System.Drawing.Point(CInt(Size.Width), rulerSize), rulerPlane, ptUpperRight)
        ScreenToPlane(New System.Drawing.Point(rulerSize, CInt(Size.Height)), rulerPlane, ptLowerLeft)

        originScreen = WorldToScreen(0, 0, 0)

        Select Case RulerPlaneMode
            Case rulerPlaneType.XY
                upperRightWorldCoord = New Point2D(ptUpperRight.X, ptUpperRight.Y)
                lowerLeftWorldCoord = New Point2D(ptLowerLeft.X, ptLowerLeft.Y)
                Exit Select
            Case rulerPlaneType.ZX
                upperRightWorldCoord = New Point2D(ptUpperRight.X, ptUpperRight.Z)
                lowerLeftWorldCoord = New Point2D(ptLowerLeft.X, ptLowerLeft.Z)
                Exit Select
            Case rulerPlaneType.YZ
                upperRightWorldCoord = New Point2D(ptUpperRight.Y, ptUpperRight.Z)
                lowerLeftWorldCoord = New Point2D(ptLowerLeft.Y, ptLowerLeft.Z)
                Exit Select
        End Select

    End Sub

    Private rulerSize As Integer = 35

    Protected Overrides Sub DrawOverlay(myParams As DrawSceneParams)
        Dim Height As Double = Size.Height
        Dim Width As Double = Size.Width

        renderContext.SetState(depthStencilStateType.DepthTestOff)
        renderContext.SetState(blendStateType.Blend)

        ' Draw the transparent ruler
        renderContext.SetColorWireframe(Color.FromArgb(CInt(0.4 * 255), 255, 255, 255))
        renderContext.SetState(rasterizerStateType.CCW_PolygonFill_NoCullFace_NoPolygonOffset)

        ' Vertical Ruler
        renderContext.DrawQuad(New RectangleF(0, 0, rulerSize, CSng(Height - rulerSize)))

        ' Horizontal Ruler
        renderContext.DrawQuad(New RectangleF(rulerSize, CSng(Height - rulerSize), CSng(Width - rulerSize), rulerSize))

        renderContext.SetState(blendStateType.NoBlend)

        ' choose a string format with 2 decimal numbers
        Dim formatString As String = "{0:0.##}"

        Dim worldToScreen As Double = (Height - rulerSize) / (upperRightWorldCoord.Y - lowerLeftWorldCoord.Y)

        Dim stepWorldX As Double = 5, stepWorldY As Double = 5

        Dim worldHeight As Double = upperRightWorldCoord.Y - lowerLeftWorldCoord.Y
        Dim nlinesH As Double = (worldHeight / stepWorldY)

        Dim worldWidth As Double = upperRightWorldCoord.X - lowerLeftWorldCoord.X
        Dim nlinesW As Double = (worldWidth / stepWorldX)

        RefineStep(nlinesH, worldHeight, stepWorldY)
        RefineStep(nlinesW, worldWidth, stepWorldX)

        Dim stepWorld As Double = Math.Min(stepWorldX, stepWorldY)

        Dim stepScreen As Double = stepWorld * worldToScreen

        '''////////////////////////
        ' Vertical ruler
        '''////////////////////////

        ' First line Y world coordinate
        Dim startYWorld As Double = stepWorld * Math.Floor(lowerLeftWorldCoord.Y / stepWorld)

        Dim firstLineScreenPositionY As New Point2D(0, originScreen.Y + startYWorld * worldToScreen)
        Dim currentScreenY As Double = firstLineScreenPositionY.Y
        Dim shorterLineXPos As Double = (firstLineScreenPositionY.X + rulerSize) / 2

        ' draw a longer line each 5 lines. The origin must be a longer line.
        Dim countShortLinesY As Integer = CInt(Math.Round((currentScreenY - originScreen.Y) / stepScreen)) Mod 5

        ' Draw the ruler lines
        renderContext.SetLineSize(1)

        Dim left As Double

        Dim y As Double = startYWorld

        Dim myFont As Font = UtilityEx.GetFont(FontFamily, FontStyle, FontWeight, FontSize)

        While y < upperRightWorldCoord.Y
            If countShortLinesY Mod 5 = 0 Then
                left = firstLineScreenPositionY.X
            Else
                left = shorterLineXPos
            End If



            renderContext.SetColorWireframe(Color.Black)

            renderContext.DrawLine(New Point2D(left, currentScreenY), New Point2D(rulerSize, currentScreenY))

            DrawText(CInt(firstLineScreenPositionY.X), CInt(currentScreenY), String.Format(formatString, y), myFont, Color.Black, ContentAlignment.BottomLeft)

            countShortLinesY += 1
            y += stepWorld
            currentScreenY += stepScreen
        End While


        '''////////////////////////
        ' Horizontal ruler
        '''////////////////////////

        ' First line X world coordinate
        Dim startXWorld As Double = stepWorld * Math.Ceiling(lowerLeftWorldCoord.X / stepWorld)

        Dim firstLineScreenPositionX As New Point2D(originScreen.X + startXWorld * worldToScreen, 0)
        Dim currentScreenX As Double = firstLineScreenPositionX.X
        Dim shorterLineYPos As Double = (firstLineScreenPositionX.Y + rulerSize) / 2

        Dim countShortLinesX As Integer = CInt(Math.Round((currentScreenX - originScreen.X) / stepScreen)) Mod 5

        Dim top As Double
        Dim x As Double = startXWorld
        While x < upperRightWorldCoord.X
            If countShortLinesX Mod 5 = 0 Then
                top = firstLineScreenPositionX.Y
            Else
                top = shorterLineYPos
            End If

            renderContext.SetColorWireframe(Color.Black)

            renderContext.DrawLine(New Point2D(currentScreenX, Height - top), New Point2D(currentScreenX, Height - rulerSize))

            DrawText(CInt(currentScreenX), CInt(Height - rulerSize - firstLineScreenPositionX.Y), String.Format(formatString, x), myFont, Color.Black, ContentAlignment.BottomLeft)

            countShortLinesX += 1
            x += stepWorld
            currentScreenX += stepScreen
        End While


        ' Draw a red line in correspondence with the mouse position

        renderContext.SetColorWireframe(Color.Red)

        If mousePos.Y > rulerSize Then
            renderContext.DrawLine(New Point3D(0, Height - mousePos.Y, 0), New Point3D(rulerSize, Height - mousePos.Y, 0))
        End If

        If mousePos.X > rulerSize Then
            renderContext.DrawLine(New Point3D(mousePos.X, Height, 0), New Point3D(mousePos.X, Height - rulerSize, 0))
        End If

        ' Draw the logo image at the bottom right corner         
        Dim logo As new Bitmap("../../../../../../dataset/Assets/Pictures/Logo.png")
        DrawImage(Size.Width - logo.Width - 20, 20, logo)

        ' call the base function
        MyBase.DrawOverlay(myParams)
    End Sub

    Private Shared Sub RefineStep(nlines As Double, worldLength As Double, ByRef stepWorld As Double)
        ' Refine the step if there are too many ruler lines, or too few
        If nlines > 20 Then
            Do
                stepWorld *= 2
                nlines = (worldLength / stepWorld)
            Loop While nlines > 20
        ElseIf nlines < 10 Then
            Do
                stepWorld /= 2
                nlines = (worldLength / stepWorld)
            Loop While nlines < 10 AndAlso nlines > 0
        End If
    End Sub

    Dim mousePos as System.Drawing.Point 

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        mousePos = RenderContextUtility.ConvertPoint(GetMousePosition(e))

        PaintBackBuffer()
        SwapBuffers()
    End Sub
End Class

