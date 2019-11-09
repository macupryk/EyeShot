Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private _mainWindow As MainWindow

    Private Sub App_OnStartup(sender As Object, e As StartupEventArgs)
        Dim renderer As String = "Native"

        If e.Args.Length > 0 Then
            If e.Args(0).Contains("DirectX") Then
                renderer = "DirectX"
            ElseIf e.Args(0).Contains("OpenGL") Then
                renderer = "OpenGL"
            End If
        End If

        _mainWindow = New MainWindow(renderer)
        _mainWindow.rendererButton.Content = renderer
        _mainWindow.Show()
    End Sub

End Class
