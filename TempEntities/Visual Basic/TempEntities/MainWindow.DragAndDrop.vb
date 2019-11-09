Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Graphics

Namespace WpfApplication1
    ' This code file shows how to perform a DragAndDrop operation by using TempEntities
    Partial Public Class MainWindow
        Private _selBlockName As String
        Private isDragging As Boolean
        Private dragFrom As Point3D
        Private tempEntity As Entity
        Private currentRef As BlockReference
        Private Sub listView1_SelectedChanged(sender As Object, e As SelectionChangedEventArgs)
            If (Me.listView1.SelectedItems.Count > 0) Then
                ' saves the selected material to be apply
                _selBlockName = CType(Me.listView1.SelectedItems(0),ImageItem).Name

                Try
                ' start a dragdrop anction to listView1
                DragDrop.DoDragDrop(CType(sender,ListView), _selBlockName, DragDropEffects.Copy)
                Catch
                End Try
                ' clear selection
                Me.listView1.SelectedItems.Clear
            End If
        End Sub

        Private Sub listView1_DragEnter(sender As Object, e As DragEventArgs)
            If (Not (_selBlockName) Is Nothing) Then
                ' shows copy cursor inside the listView
                e.Effects = DragDropEffects.Copy
                If Not isDragging Then
                    isDragging = true
                    
                        ' start a drag-drop action to viewport
                        DragDrop.DoDragDrop(model1, _selBlockName, DragDropEffects.Copy)
                End If
    
            End If
        End Sub

        Private Sub ListView_OnMouseUp(sender As Object, e As MouseButtonEventArgs)
            ' reset dragging operation if it ends inside ListView
            isDragging = false
        End Sub

        Private Sub viewport_dragEnter(sender As Object, e As DragEventArgs)
            If (isDragging  _
                AndAlso ((tempEntity Is Nothing)  _
                         AndAlso (Not (_selBlockName) Is Nothing))) Then
                ' shows copy cursor inside the viewport
                e.Effects = DragDropEffects.Copy
                ' creates the tempEntity from the block data
                currentRef = New BlockReference(_selBlockName)
                Dim temp As Entity = GetUniqueEntity(currentRef)
                ' if is checked shows only the axis-aligned bounding box as temp entity
                If bboxCheckBox.IsChecked.Value Then
                    Dim s As Size3D = temp.BoxSize
                    Dim bm As Point3D = temp.BoxMin
                    temp = Mesh.CreateBox(s.X, s.Y, s.Z)
                    temp.Translate(bm.X, bm.Y, bm.Z)
                    temp.Regen(0.1)
                End If
    
                ' adds the temp entity to the viewport
                model1.TempEntities.Add(temp, Color.FromArgb(100, model1.Blocks(_selBlockName).Entities(0).Color))
                tempEntity = temp
                ' saves the start point position of the temp entity
                dragFrom = Plane.XY.PointAt(Plane.XY.Project(((temp.BoxMax + temp.BoxMin)  _
                                                              / 2)))
                ' refresh the screen
                model1.Invalidate
            Else
                e.Effects = DragDropEffects.None
            End If

        End Sub

        Private Sub viewport_dragOver(sender As Object, e As DragEventArgs)
            Dim mouseLocation As System.Drawing.Point = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))
            If ((model1.ActionMode <> actionType.None)  _
                OrElse model1.GetToolBar.Contains(mouseLocation)) Then
                Return
            End If

            If (isDragging  _
                AndAlso (Not (tempEntity) Is Nothing)) Then
                ' current 3D point
                Dim dragTo As Point3D
                model1.ScreenToPlane(mouseLocation, Plane.XY, dragTo)
                Dim delta As Vector3D = Vector3D.Subtract(dragTo, dragFrom)
                ' applies the translation to the temp entity
                tempEntity.Translate(delta)
                tempEntity.Regen(0.1)
                ' saves translations applied
                If (tempEntity.EntityData Is Nothing) Then
                    tempEntity.EntityData = delta
                Else
                    tempEntity.EntityData = (CType(tempEntity.EntityData,Vector3D) + delta)
                End If
    
                ' updates camera Near and Far planes to avoid clipping temp entity on the scene during translation
                model1.TempEntities.UpdateBoundingBox
                ' refresh the screen
                model1.Invalidate
                ' sets start as current
                dragFrom = dragTo
            End If

        End Sub

        Private Sub viewport_dragDrop(sender As Object, e As DragEventArgs)
            If isDragging Then
                'shows default cursor
                e.Effects = DragDropEffects.None
                If (Not (_selBlockName) Is Nothing) Then
                    ' gets current mouse position
                    Dim mouseLocation As System.Drawing.Point = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))
                    ' current 3D point
                    Dim dragTo As Point3D
                    model1.ScreenToPlane(mouseLocation, Plane.XY, dragTo)
                    Dim delta As Vector3D = CType(tempEntity.EntityData,Vector3D)
                    ' translates entity to the temp entity current position
                    currentRef.Transformation = New Translation(delta)
                    ' adds the entity to the viewport
                    model1.Entities.Add(currentRef)
                    ' refresh the entities treeView
                    PopulateTree(treeView1, New List(Of Entity), model1.Blocks)
                End If
    
                FinishDraggingOperation()
            End If

        End Sub

        Private Sub viewport_dragLeave(sender As Object, e As EventArgs)
            If isDragging Then
                
                FinishDraggingOperation()
            End If
        End Sub
        Private Sub FinishDraggingOperation()
            ' removes current dragging tempEntity from the viewport
            model1.TempEntities.Remove(tempEntity)
            ' reset dragging values
            _selBlockName = Nothing
            currentRef = Nothing
            tempEntity = Nothing
            isDragging = false
            ' refresh the screen
            model1.Invalidate
        End Sub
    End Class
End Namespace
