Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Input


''' <summary>
''' A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default return value for the CanExecute method is 'true'.
''' Taken from http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
''' </summary>
''' <remarks></remarks>
Public Class RelayCommand
    Implements ICommand
 
#Region "Fields"
    Private ReadOnly _CanExecute As Predicate(Of Object)
    Private ReadOnly _Execute As Action(Of Object)
#End Region

#Region "Constructors"
    Public Sub New(ByVal execute As Action(Of Object))
        Me.New(execute, Nothing)
    End Sub

    Public Sub New(ByVal execute As Action(Of Object), ByVal canExecute As Predicate(Of Object))
        If execute Is Nothing Then
            Throw New ArgumentNullException("execute")
        End If
        _Execute = execute
        _CanExecute = canExecute
    End Sub
#End Region

#Region "ICommand"
    Public Custom Event CanExecuteChanged As EventHandler Implements System.Windows.Input.ICommand.CanExecuteChanged

        AddHandler(ByVal value As EventHandler)
            If _CanExecute IsNot Nothing Then
                AddHandler CommandManager.RequerySuggested, value
            End If
        End AddHandler

        RemoveHandler(ByVal value As EventHandler)

            If _CanExecute IsNot Nothing Then

                RemoveHandler CommandManager.RequerySuggested, value

            End If

        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)

            'This is the RaiseEvent block
            CommandManager.InvalidateRequerySuggested()

        End RaiseEvent

    End Event

    Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute

        If _CanExecute Is Nothing Then
            Return True
        Else
            Return _CanExecute.Invoke(parameter)
        End If
    End Function

    Public Sub Execute(ByVal parameter As Object) Implements System.Windows.Input.ICommand.Execute
        _Execute.Invoke(parameter)
    End Sub

#End Region
End Class

