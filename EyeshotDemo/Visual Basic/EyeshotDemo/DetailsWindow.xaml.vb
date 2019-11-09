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
Imports System.Windows.Shapes

''' <summary>
''' Interaction logic for DetailsWindow.xaml
''' </summary>
Partial Public Class DetailsWindow    
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub CopyToClipboardButton_OnClick(sender As Object, e As RoutedEventArgs)
        Clipboard.SetText(contentTextBox.Text)
    End Sub
End Class
