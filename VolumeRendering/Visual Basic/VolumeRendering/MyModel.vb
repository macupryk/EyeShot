Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Input
Imports devDept.Eyeshot.Labels
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports System.Drawing
Imports System.Collections
Imports devDept.Geometry
Imports devDept.Eyeshot.Entities
Imports System.Diagnostics
Imports MouseButton = System.Windows.Input.MouseButton
Imports Point = System.Drawing.Point

Public Class MyModel
    Inherits devDept.Eyeshot.Model
    Public Sub New()
        MyBase.New()
        AddHandler CameraMoveEnd, AddressOf MyModel_CameraMoveEnd
    End Sub

    Private _measuring As Boolean

    Private _measureEndPoint As Point3D
    Private _currentPoint As Point3D

    Private _plane As Plane = Plane.XY

    Private _firstClick As Boolean = False


    Private ReadOnly _points As New List(Of Point3D)()

    Public Delegate Sub MeasureCompletedEventHandler(sender As Object, e As EventArgs)
    Public Event MeasureCompleted As MeasureCompletedEventHandler

    Private Sub SetPlane()
        _plane = Camera.NearPlane
    End Sub

    Private Sub MyModel_CameraMoveEnd(sender As Object, e As Model.CameraMoveEventArgs)
        If _measuring Then
            SetPlane()
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseButtonEventArgs)
        Dim location As Point = RenderContextUtility.ConvertPoint(GetMousePosition(e))

        If GetToolBar().Contains(location) Then
            MyBase.OnMouseUp(e)

            Return
        End If

        If _measuring Then
            If e.ChangedButton = MouseButton.Left Then
                If _firstClick = False Then
                    _points.Clear()
                    _firstClick = True
                End If

                If FindClosestPoint(location) = -1 Then
                    StopMeasuring(false)
                Else
                    _points.Add(_measureEndPoint)

                    If _points.Count > 1 Then
                        _line = New Line(_points(0), _points(1)) With { _
                             .LineWeightMethod = colorMethodType.byEntity, _
                             .LineWeight = 1 _
                        }

                        Dim text As String = [String].Format("{0} mm", Math.Round(_line.Length(), 2))
                        Dim [to] = New TextOnly((_line.StartPoint + _line.EndPoint) / 2, text, New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)
                        Entities.Add(_line, Color.WhiteSmoke)
                        Labels.Add([to])

                        Invalidate()

                        StopMeasuring(false)
                    End If
                End If
            ElseIf e.ChangedButton = MouseButton.Right Then
                ResetMeasuring()

            End If
        End If

        MyBase.OnMouseUp(e)
    End Sub

    Private Sub ResetMeasuring()
        _firstClick = False
        _points.Clear()
        _measureEndPoint = Nothing
        _currentPoint = Nothing
    End Sub

    Protected ReadOnly Property DefaultCursor() As Cursor
        Get
            If _measuring Then
                Return Cursors.Cross
            End If

            Return GetDefaultCursor()
        End Get
    End Property

    Public Sub Measure(start As Boolean)
        If start Then
            ActionMode = actionType.None
            _measuring = True
            SetDefaultCursor(DefaultCursor)
            Cursor = DefaultCursor
            SetPlane()
            Focus()
        ElseIf _measuring Then
            StopMeasuring(true)
        End If
    End Sub

    Private Sub StopMeasuring(fromCheckedChanged As Boolean)
        ResetMeasuring()
        _measuring = False
        SetDefaultCursor(DefaultCursor)
        Cursor = DefaultCursor
        If not fromCheckedChanged Then
            RaiseEvent MeasureCompleted(Me, New EventArgs())
        End If
    End Sub

    Private _line As Line
    Private _mouseLocation As System.Drawing.Point

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        ' save the current mouse position
        _mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e))

        ' if start is valid and actionMode is None and it's not in the toolbar area

        If _currentPoint Is Nothing OrElse ActionMode <> actionType.None OrElse GetToolBar().Contains(_mouseLocation) Then

            MyBase.OnMouseMove(e)

            Return
        End If

        MyBase.OnMouseMove(e)

        FindClosestPoint(_mouseLocation)

        ' paint the viewport surface
        PaintBackBuffer()

        ' consolidates the drawing
        SwapBuffers()

    End Sub

    Private Function FindClosestPoint(point As System.Drawing.Point) As Integer
        Dim result As Integer = -1
        Dim entityIndex As Integer = GetEntityUnderMouseCursor(point)

        If entityIndex <> -1 Then
            Dim ent As Entity = Entities(entityIndex)
            If TypeOf ent Is ICurve Then
                result = FindClosestVertex(point, 8, _measureEndPoint)
            End If
        Else
            _measureEndPoint = Nothing
        End If
        Return result
    End Function

    Protected Overrides Sub DrawOverlay(myParams As DrawSceneParams)
        If Not _measuring Then
            MyBase.DrawOverlay(myParams)

            Return
        End If

        ScreenToPlane(_mouseLocation, _plane, _currentPoint)

        ' size line 
        renderContext.SetLineSize(1)

        ' draw inverted
        renderContext.EnableXOR(True)

        renderContext.SetState(depthStencilStateType.DepthTestOff)

        If ActionMode = actionType.None AndAlso Not GetToolBar().Contains(_mouseLocation) Then
            If _points.Count > 0 Then
                Dim pts2 As New List(Of Point3D)()

                ' Draw elastic line
                pts2.Add(WorldToScreen(_points(0)))
                pts2.Add(WorldToScreen(_currentPoint))

                renderContext.DrawLines(pts2.ToArray())
            End If

            ' disables draw inverted
            renderContext.EnableXOR(False)

            If _measureEndPoint IsNot Nothing Then
                ' text drawing
                DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10, "Current point: " + _measureEndPoint.X.ToString("f2") + ", " + _measureEndPoint.Y.ToString("f2") + ", " + _measureEndPoint.Z.ToString("f2"), New Font("Tahoma", 8.25F), Color.Black, ContentAlignment.BottomLeft)

            End If
        Else
            ' disables draw inverted
            renderContext.EnableXOR(False)
        End If

        MyBase.DrawOverlay(myParams)
    End Sub
End Class