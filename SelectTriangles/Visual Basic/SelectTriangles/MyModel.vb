Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Graphics


Public Class MyModel
    Inherits Model

    Private processSubItems As Boolean = False ' enabled triangles selection
    Friend firstOnlyInternal As Boolean = False ' needed for selection by pick
    Friend processVisibleOnly As Boolean = False ' used for visible by pick to find all the triangles selected during GetCrossingEntities
    Public Overrides Sub ProcessSelection(ByVal selectionBox As Rectangle, ByVal firstOnly As Boolean, ByVal invert As Boolean, ByVal eventArgs As SelectionChangedEventArgs, Optional ByVal selectableOnly As Boolean = True)
        ' Selects the entities first
        For Each entity In Entities
            If TypeOf entity Is ISelect Then
                DirectCast(entity, ISelect).DrawSubItemsForSelection = False
            End If
        Next entity

        MyBase.ProcessSelection(selectionBox, firstOnly, invert, eventArgs, selectableOnly)

        ' Now selects the triangles for the selected entities
        processSubItems = True
        SuspendSetColorForSelection = True

        ' Performs the triangles selection one entity at a time
        For Each entity In Entities
            If TypeOf entity Is ISelect AndAlso entity.Selected Then
                DirectCast(entity, ISelect).DrawSubItemsForSelection = True

                UpdateVisibleSelection()

                MyBase.ProcessSelection(selectionBox, firstOnly, invert, eventArgs, selectableOnly)

                UpdateVisibleSelection()
                DirectCast(entity, ISelect).DrawSubItemsForSelection = False

                If firstOnly Then
                    Exit For
                End If
            End If
        Next entity

        SuspendSetColorForSelection = False
        processSubItems = False
    End Sub

    Protected Overrides Function GetCrossingEntities(ByVal selectionBox As Rectangle, ByVal firstOnly As Boolean, Optional ByVal selectableOnly As Boolean = True) As Integer()
        If Not processSubItems Then

            Return MyBase.GetCrossingEntities(selectionBox, firstOnly, selectableOnly)
        End If

        ' Reads the visible triangles from the back buffer and selects them
        For i As Integer = 0 To Entities.Count - 1
            If TypeOf Entities(i) Is ISelect AndAlso Entities(i).Selected Then
                Dim entity As ISelect = TryCast(Entities(i), ISelect)

                ' Selects the triangles
                MyBase.GetCrossingEntities(selectionBox, firstOnly, selectableOnly)

                If Not processVisibleOnly Then
                    ' Removes the selection flag, otherwise the entity will be drawn all selected
                    DirectCast(entity, Entity).Selected = False
                End If

                If firstOnly Then
                    Exit For
                End If
            End If
        Next 

        Return New Integer(){}
    End Function

    Public Overrides Sub ProcessSelectionVisibleOnly(ByVal selectionBox As Rectangle, ByVal firstOnly As Boolean, ByVal invert As Boolean, ByVal eventArgs As SelectionChangedEventArgs, Optional ByVal selectableOnly As Boolean = True, Optional ByVal temporarySelection As Boolean = False)
        ' Selects the entities first
        For Each entity In Entities
            If TypeOf entity Is ISelect Then
                DirectCast(entity, ISelect).DrawSubItemsForSelection = False
            End If
        Next

        MyBase.ProcessSelectionVisibleOnly(selectionBox, firstOnly, invert, eventArgs, selectableOnly, temporarySelection)

        ' Now selects the triangles for the selected entities
        processSubItems = True
        SuspendSetColorForSelection = True
        processVisibleOnly = True

        ' Performs the triangles selection one entity at a time
        For Each entity In Entities
            If TypeOf entity Is ISelect AndAlso entity.Selected Then
                DirectCast(entity, ISelect).DrawSubItemsForSelection = True

                UpdateVisibleSelection()

                ' gets only the triangles in the selection box
                GetCrossingEntities(selectionBox, False)

                MyBase.ProcessSelectionVisibleOnly(selectionBox, firstOnly, invert, eventArgs, selectableOnly, temporarySelection)

                UpdateVisibleSelection()
                DirectCast(entity, ISelect).DrawSubItemsForSelection = False
            End If
        Next

        SuspendSetColorForSelection = False
        processSubItems = False
        processVisibleOnly = False
    End Sub

    Protected Overrides Function GetVisibleEntitiesFromBackBuffer(ByVal viewport As Viewport, ByVal rgbValues() As Byte, ByVal stride As Integer, ByVal bpp As Integer, ByVal selectionBox As Rectangle, ByVal firstOnly As Boolean) As Integer()
        If Not processSubItems Then

            Return MyBase.GetVisibleEntitiesFromBackBuffer(viewport, rgbValues, stride, bpp, selectionBox, firstOnly)
        End If

        ' Reads the visible triangles from the back buffer and selects them
        For i As Integer = 0 To Entities.Count - 1
            If TypeOf Entities(i) Is ISelect AndAlso Entities(i).Selected Then
                Dim entity As ISelect = TryCast(Entities(i), ISelect)

                ' Gets the indices of the triangles to select
                Dim indices() As Integer = MyBase.GetVisibleEntitiesFromBackBuffer(viewport, rgbValues, stride, bpp, selectionBox, firstOnly)

                ' Selects the triangles
                entity.SelectSubItems(indices)

                ' Removes the selection flag, otherwise the entity will be drawn all selected
                DirectCast(entity, Entity).Selected = False

                Exit For
            End If
        Next 

        Return New Integer(){}
    End Function
End Class