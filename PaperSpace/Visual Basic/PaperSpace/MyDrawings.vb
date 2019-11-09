Imports System.Drawing
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics

Partial Public Class MyDrawings
    Inherits devDept.Eyeshot.Drawings

    ' current selection/position
    Private _current As Point3D

    Private _cursorOutside As Boolean = True

    ' current mouse position
    Private _mouseLocation As System.Drawing.Point

    ' it always draws on XY plane
    Private _plane As Plane = Plane.XY

    ' array of selected vertices (snapped points)
    Private _points() As Point3D = New Point3D(2) {}
    Private _numPoints As Integer

    ' current drawing plane and extension points required while dimensioning
    Private _drawingPlane As Plane
    Private _extPt1 As Point3D
    Private _extPt2 As Point3D

    Public DrawingColor As System.Drawing.Color = System.Drawing.Color.Black

    ''' <summary>
    ''' Disables dimensioning and restore the default action mode (SelectVisibleByPick)
    ''' </summary>
    Public Sub DisableDimensioning()
        _points = New Point3D(2) {}
        _numPoints = 0
        DrawingLinearDim = False
        ActionMode = actionType.SelectVisibleByPick
    End Sub

    ''' <summary>
    ''' Enable dimensioning and set action mode to none
    ''' </summary>
    Public Sub EnableDimensioning()
        DrawingLinearDim = True
        ActionMode = actionType.None
    End Sub

    Private Function GetUnitsConversionFactor() As Double
        Return Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, GetActiveSheet.Units)
    End Function

    Protected Overrides Sub DrawOverlay(ByVal data As DrawSceneParams)
        If DrawingLinearDim AndAlso (ActionMode = actionType.None) Then
            ScreenToPlane(_mouseLocation, _plane, _current)
            If (Not (_snappedPoint) Is Nothing) Then
                DisplaySnappedVertex()
            End If

            ' set render context for interactive drawing
            renderContext.SetLineSize(1)

            renderContext.EnableXOR(True)

            renderContext.SetState(depthStencilStateType.DepthTestOff)

            ' if cursor is outside from Drawings it does not need to draw anything on overlay
            If Not _cursorOutside Then
                DrawPositionMark(_current)

                If (_numPoints < 2) Then

                    DrawSelectionMark(_mouseLocation)

                    renderContext.EnableXOR(False)
                    Dim text As String = "Select the first point"
                    If (_numPoints = 1) Then
                        text = "Select the second point"
                    End If

                    DrawText(_mouseLocation.X, ((Size.Height - _mouseLocation.Y) + 10), text, New Font("Tahoma", 8.25!), DrawingColor, ContentAlignment.BottomLeft)
                Else
                    DrawInteractiveLinearDim()
                End If

            End If

        End If

        MyBase.DrawOverlay(data)
    End Sub

    Protected Overrides Sub OnMouseEnter(ByVal e As EventArgs)
        _cursorOutside = False
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
        _cursorOutside = True
        MyBase.OnMouseLeave(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(ByVal e As MouseButtonEventArgs)

        Dim mousePosition = RenderContextUtility.ConvertPoint(GetMousePosition(e))

        If DrawingLinearDim AndAlso (ActionMode = actionType.None) Then
            If GetToolBar().Contains(mousePosition) Then
                MyBase.OnMouseDown(e)
                Return
            End If

            If e.ChangedButton = Input.MouseButton.Left Then
                If (_numPoints < 2) Then
                    If (Not (_snappedPoint) Is Nothing) Then
                        _points(_numPoints) = _snappedPoint ' adds the snapped point to the list of points
                        _numPoints = _numPoints + 1

                        If _numPoints = 1 Then
                            Dim index As Integer = GetEntityUnderMouseCursor(_mouseLocation)

                            If index <> -1 Then
                                Dim view = TryCast(Entities(index), devDept.Eyeshot.Entities.View)

                                If view IsNot Nothing Then
                                    _viewScale = view.Scale
                                Else
                                    _viewScale = 1
                                End If
                            End If
                        End If
                    End If

                Else
                    ' the following lines need to add LinearDim to Drawings
                    ScreenToPlane(mousePosition, _plane, _current)
                    Dim unitsConversionFactor As Double = GetUnitsConversionFactor()
                    Dim linearDim = New LinearDim(_drawingPlane, (_points(0) / unitsConversionFactor), (_points(1) / unitsConversionFactor), (_current / unitsConversionFactor), DimTextHeight)
                    linearDim.Scale(unitsConversionFactor)
                    linearDim.LayerName = WiresLayerName
                    linearDim.LinearScale = (1 / _viewScale)
                    Entities.Add(linearDim)
                    Invalidate()

                    DisableDimensioning()
                End If

            ElseIf e.ChangedButton = Input.MouseButton.Right Then
                _points = New Point3D(2) {}
                _numPoints = 0
                _viewScale = 1
            End If

        End If

        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        If (DrawingLinearDim AndAlso (ActionMode = actionType.None)) Then
            ' saves the current mouse position
            _mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e))

            _snappedPoint = Nothing
            Dim index As Integer = GetEntityUnderMouseCursor(_mouseLocation)
            If (index <> -1) AndAlso Not (TypeOf Entities(index) Is RasterView) Then
                FindClosestVertex(_mouseLocation, 50, _snappedPoint)  ' returns the closest snapped point to the cursor
            End If
           
            ' paints the viewport surface
            PaintBackBuffer()

            ' consolidates the drawing
            SwapBuffers()
        End If

        MyBase.OnMouseMove(e)
    End Sub

End Class
