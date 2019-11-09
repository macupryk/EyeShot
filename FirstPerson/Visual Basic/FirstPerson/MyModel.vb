Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Text
Imports System.Windows.Input
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports System.Drawing
Imports System.Collections
Imports KeyEventArgs = System.Windows.Input.KeyEventArgs
Imports Point = System.Drawing.Point

Public Class MyModel
    Inherits Model
    Private displayHelp As Boolean

    Private Sub RenderText()
        Dim helpStrings As New List(Of String)()

        Dim formatString As String = "{0:0.00}"

        If displayHelp Then
            helpStrings.Add("First person camera behavior")
            helpStrings.Add("  Press W and S to move forwards and backwards")
            helpStrings.Add("  Press A and D to strafe left and right")
            helpStrings.Add("  Press E and Q to move up and down")
            helpStrings.Add("  Move mouse to free look")
            helpStrings.Add("")
            helpStrings.Add("Flight camera behavior")
            helpStrings.Add("  Press W and S to move forwards and backwards")
            helpStrings.Add("  Press A and D to yaw left and right")
            helpStrings.Add("  Press E and Q to move up and down")
            helpStrings.Add("  Move mouse to pitch and roll")
            helpStrings.Add("")
            helpStrings.Add("Press M to enable/disable mouse smoothing")
            helpStrings.Add("Press + and - to change camera rotation speed")
            helpStrings.Add("Press , and . to change mouse sensitivity")
            helpStrings.Add("Press SPACE to toggle between flight and first person behavior")
            helpStrings.Add("Press ESC to exit")
            helpStrings.Add("")
            helpStrings.Add("Press H to hide help")
        Else
            helpStrings.Add("Camera")
            helpStrings.Add("  Speed:" + String.Format(formatString, Viewports(0).Navigation.RotationSpeed))

            helpStrings.Add("  Behavior: " + Viewports(0).Navigation.Mode.ToString())
            helpStrings.Add("")
            helpStrings.Add("Press H to display help")
        End If

        Dim myFont As Font = UtilityEx.GetFont(FontFamily, FontStyle, FontWeight, FontSize)

        Dim posY As Integer = CInt(Size.Height) - 2 * myFont.Height

        For i As Integer = 0 To helpStrings.Count - 1
            DrawText(10, posY, helpStrings(i), myFont, Color.White, ContentAlignment.BottomLeft)
            posY -= CInt(1.5) * myFont.Height
        Next
    End Sub

    Protected Overrides Sub DrawOverlay(myParams As DrawSceneParams)
        MyBase.DrawOverlay(myParams)
        RenderText()
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)

        Select Case e.Key
            Case Key.Space
                If Viewports(0).Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Walk Then

                    Viewports(0).Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Fly
                Else

                    Viewports(0).Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Walk
                End If
                Exit Select

            Case Key.H
                displayHelp = Not displayHelp
                Exit Select

            Case Key.Add
                Viewports(0).Navigation.RotationSpeed += 0.2
                If Viewports(0).Navigation.RotationSpeed >= 10 Then
                    Viewports(0).Navigation.RotationSpeed = 10
                End If
                Exit Select

            Case Key.Subtract
                Viewports(0).Navigation.RotationSpeed -= 0.2F
                If Viewports(0).Navigation.RotationSpeed <= 0 Then
                    Viewports(0).Navigation.RotationSpeed = 0.01
                End If

                Exit Select
        End Select

        Invalidate()
    End Sub
End Class


