Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Linq
Imports System.Text
Imports System.Windows.Media

''' <summary>    
''' This class represent the ViewModel for Layers List.
''' </summary>    
Public Class LayersListViewModel
    Public Sub New(layerName__1 As String)
        LayerName = layerName__1
    End Sub

    Public Sub New(layerName As String, layerLineWeight__1 As Integer, foregroundColor As Brush)
        Me.New(layerName)
        LayerLineWeight = layerLineWeight__1
        ForeColor = foregroundColor
    End Sub

    Public Property LayerName() As String
        Get
            Return m_LayerName
        End Get
        Set(value As String)
            m_LayerName = value
        End Set
    End Property
    Private m_LayerName As String
    Public Property LayerLineWeight() As Single
        Get
            Return m_LayerLineWeight
        End Get
        Set(value As Single)
            m_LayerLineWeight = value
        End Set
    End Property
    Private m_LayerLineWeight As Single
    Public Property Checked() As Boolean
        Get
            Return m_Checked
        End Get
        Set(value As Boolean)
            m_Checked = value
        End Set
    End Property
    Private m_Checked As Boolean
    Public Property ForeColor() As Brush
        Get
            Return m_ForeColor
        End Get
        Set(value As Brush)
            m_ForeColor = value
        End Set
    End Property
    Private m_ForeColor As Brush

    Public Overrides Function ToString() As String
        Return LayerName
    End Function

End Class

