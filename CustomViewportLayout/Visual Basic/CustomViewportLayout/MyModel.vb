Imports System.Collections.Generic
Imports System.Text
Imports System.Windows
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports System.ComponentModel

Public Class MyModel
    Inherits Model
    Public Sub New()
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        InitializeViewportsByNumber(2)

        MyBase.OnHandleCreated(e)
    End Sub

    Public Overrides Sub UpdateViewportsSizeAndLocation()
        Dim height1 As Integer = CInt(2 * Size.Height / 3.0 - ViewportsGap / 2.0)
        Dim height2 As Integer = CInt(1 * Size.Height / 3.0 - ViewportsGap / 2.0)

        Viewports(0).Size = New System.Drawing.Size(CInt(Size.Width), height1)
        Viewports(1).Size = New System.Drawing.Size(CInt(Size.Width), height2)

        Viewports(1).Location = New System.Drawing.Point(0, height1 + ViewportsGap)
    End Sub

    Private Sub InitializeViewportsByNumber(numberOfViewports As Integer)
        If Viewports.Count > numberOfViewports Then
            While Viewports.Count > numberOfViewports
                Viewports.RemoveAt(Viewports.Count - 1)
            End While
        Else
            While Viewports.Count < numberOfViewports
                Viewports.Add(New Viewport())
            End While
        End If
    End Sub

End Class