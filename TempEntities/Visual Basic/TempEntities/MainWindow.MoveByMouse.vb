Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Graphics

Namespace WpfApplication1
    'This code file shows how to move entity in the scene with the mouse.
    'It displays also some temp arrow entities showing the moving plane direction.
    Partial Public Class MainWindow
        Dim entityIndex As Integer = -1
        Dim move As Boolean
        Dim xyzPlane As Plane
        Dim moveFrom As Point3D
        Dim centerOfArrows As Point3D
        Dim tempArrows As Mesh()
         Private Sub CreateArrowsDirections()
        ' removes previous arrows if present
        If (Not (tempArrows) Is Nothing) Then
            model1.TempEntities.Remove(tempArrows(0))
            model1.TempEntities.Remove(tempArrows(1))
            model1.TempEntities.Remove(tempArrows(2))
            model1.TempEntities.Remove(tempArrows(3))
        End If
        
        'creates 4 temporary arrows on the current moving plane to display when the mouse is over an entity
        tempArrows = New Mesh((4) - 1) {}
        Dim arrowShape As devDept.Eyeshot.Entities.Region = New devDept.Eyeshot.Entities.Region(New LinearPath(xyzPlane, New Point2D() {New Point2D(0, -2), New Point2D(4, -2), New Point2D(4, -4), New Point2D(10, 0), New Point2D(4, 4), New Point2D(4, 2), New Point2D(0, 2), New Point2D(0, -2)}), xyzPlane)
        'right arrow
        tempArrows(0) = arrowShape.ExtrudeAsMesh(2, 0.1, Mesh.natureType.Plain)
        tempArrows(0).Regen(0.1)
        tempArrows(0).Color = Color.FromArgb(100, Color.Red)
        'top arrow
        tempArrows(1) = CType(tempArrows(0).Clone,Mesh)
        tempArrows(1).Rotate((Math.PI / 2), xyzPlane.AxisZ)
        tempArrows(1).Regen(0.1)
        'left arrow
        tempArrows(2) = CType(tempArrows(0).Clone,Mesh)
        tempArrows(2).Rotate(Math.PI, xyzPlane.AxisZ)
        tempArrows(2).Regen(0.1)
        'bottom arrow
        tempArrows(3) = CType(tempArrows(0).Clone,Mesh)
        tempArrows(3).Rotate(((Math.PI / 2)  _
                        * -1), xyzPlane.AxisZ)
        tempArrows(3).Regen(0.1)
        Dim diagonalV As Vector3D = New Vector3D(tempArrows(0).BoxMin, tempArrows(0).BoxMax)
        Dim offset As Double = Math.Max(Vector3D.Dot(diagonalV, xyzPlane.AxisX), Vector3D.Dot(diagonalV, xyzPlane.AxisY))
        Dim translateX As Vector3D = (xyzPlane.AxisX  _
                    * (offset / 2))
        Dim translateY As Vector3D = (xyzPlane.AxisY  _
                    * (offset / 2))
        tempArrows(0).Translate(translateX)
        tempArrows(1).Translate(translateY)
        tempArrows(2).Translate(((1 * translateX)  _
                        * -1))
        tempArrows(3).Translate(((1 * translateY)  _
                        * -1))
        centerOfArrows = Point3D.Origin
    End Sub
    
    Private Sub TranslateAndShowArrows(ByVal mouseLocation As System.Drawing.Point)
        ' gets the entity index under mouse cursor
        entityIndex = model1.GetEntityUnderMouseCursor(mouseLocation)
        If (entityIndex < 0) Then
            ' removes previous temporary arrows if present
            If (Not (tempArrows) Is Nothing) Then
                model1.TempEntities.Remove(tempArrows(0))
                model1.TempEntities.Remove(tempArrows(1))
                model1.TempEntities.Remove(tempArrows(2))
                model1.TempEntities.Remove(tempArrows(3))
            End If
            
            'refresh the screen
            model1.Invalidate
            Return
        End If
        
        ' gets the center of the entity bounding box
        Dim ent As Entity = model1.Entities(entityIndex)
        Dim center As Point3D = ((ent.BoxMax + ent.BoxMin)  _
                    / 2)
        ' gets translation from arrows center position to entity center position
        Dim trans As Vector3D = New Vector3D(centerOfArrows, center)
        ' translates arrows
        tempArrows(0).Translate(trans)
        tempArrows(2).Translate(trans)
        tempArrows(1).Translate(trans)
        tempArrows(3).Translate(trans)
        ' updates center position
        centerOfArrows = center
        ' if not already added, adds them to TempEntities list
        If (model1.TempEntities.Count < 4) Then
            model1.TempEntities.Add(tempArrows(0))
            model1.TempEntities.Add(tempArrows(1))
            model1.TempEntities.Add(tempArrows(2))
            model1.TempEntities.Add(tempArrows(3))
            ' updates camera Near and Far planes to avoid clipping temp entity on the scene during translation
            model1.TempEntities.UpdateBoundingBox
        End If
        
        'refresh the screen
        model1.Invalidate
    End Sub
        Private Sub planeCombo_SelectedIndexChanged(sender As Object, e As EventArgs)
            ' changes moving plane 
            Select Case (planeCombo.SelectedIndex)
                Case 0
                    xyzPlane = Plane.XY
                Case 1
                    xyzPlane = Plane.ZX
                Case 2
                    xyzPlane = Plane.YZ
                Case Else
                    xyzPlane = Plane.XY
            End Select

            ' creates arrows lying on the chosen plane
            CreateArrowsDirections
        End Sub

        Private Sub moveCheckBox_CheckedChanged(sender As Object, e As EventArgs)
            If (moveToggle Is Nothing)
                Return
            End If

            If(moveToggle.IsChecked.HasValue AndAlso moveToggle.IsChecked.Value) Then
                moveToggle.Content = "Disable"
                move = true
            Else
                moveToggle.Content = "Enable"
                move = false
            End If
        End Sub

        Private Sub model1_MouseDown(sender As Object, e As MouseEventArgs)
            Dim mousePos As System.Drawing.Point = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))
            If (Not move  _
                OrElse ((e.LeftButton <> MouseButtonState.Pressed)  _
                        OrElse ((model1.ActionMode <> actionType.None)  _
                                OrElse model1.GetToolBar.Contains(mousePos)))) Then
                Return
            End If

            ' gets the entity index
            entityIndex = model1.GetEntityUnderMouseCursor(mousePos)
            If (entityIndex < 0) Then
                Return
            End If

            ' gets 3D start point
            model1.ScreenToPlane(mousePos, xyzPlane, moveFrom)
        End Sub

        Private Sub model1_MouseMove(sender As Object, e As MouseEventArgs)
            ' if moving action is enabled, then draws temporary arrows when the mouse is hover an entity
            If (move  _
                AndAlso ((e.LeftButton = MouseButtonState.Released)  _
                         AndAlso ((e.RightButton = MouseButtonState.Released)  _
                                  AndAlso (e.MiddleButton = MouseButtonState.Released)))) Then
                TranslateAndShowArrows(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))
            End If

            If (Not move  _
                OrElse ((e.LeftButton <> MouseButtonState.Pressed)  _
                        OrElse ((model1.ActionMode <> actionType.None)  _
                                OrElse model1.GetToolBar.Contains(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))))) Then
                Return
            End If

            If (moveFrom Is Nothing) Then
                Return
            End If

' if we found an entity and the left mouse button is down
            If ((entityIndex <> -1)  _
                AndAlso (e.LeftButton = MouseButtonState.Pressed)) Then
                ' removes temp arrows during translation, if present
                model1.TempEntities.Remove(tempArrows(0))
                model1.TempEntities.Remove(tempArrows(1))
                model1.TempEntities.Remove(tempArrows(2))
                model1.TempEntities.Remove(tempArrows(3))
                ' gets the entity reference
                Dim entity As Entity = CType(model1.Entities(entityIndex),Entity)
                ' current 3D point
                Dim moveTo As Point3D
                model1.ScreenToPlane(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), xyzPlane, moveTo)
                Dim delta As Vector3D = Vector3D.Subtract(moveTo, moveFrom)
                ' sets start as current
                moveFrom = moveTo
                ' applies the translation
                entity.Translate(delta)
                ' regens entities that need it
                model1.Entities.Regen
                ' refresh the screen
                model1.Invalidate
                ' sets start as current
                moveFrom = moveTo
                'updates blinked entity if present
                If (Not (entity.EntityData) Is Nothing) Then
                    CType(entity.EntityData,Entity).Translate(delta)
                    CType(entity.EntityData,Entity).Regen(0.01)
                End If
    
            End If
        End Sub

        Private Sub model1_MouseUp(sender As Object, e As MouseEventArgs)
            entityIndex = -1
        End Sub
    End Class
End Namespace
