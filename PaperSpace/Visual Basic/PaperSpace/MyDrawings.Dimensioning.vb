Imports System.Drawing
Imports devDept.Geometry

Partial Public Class MyDrawings

    Dim _dimTextHeight As Double = 2.5

    Public Property DimTextHeight As Double
        Get
            Return _dimTextHeight
        End Get
        Set(value As Double)
            _dimTextHeight = value
        End Set
    End Property

    Public Property DrawingLinearDim As Boolean = False

    Dim _viewScale As Double = 1

    ' Draws preview of horizontal/vertical dimension for a line
    Private Sub DrawInteractiveLinearDim()
        ' 2 points needed to draw the interactive LinearDim
        If (_numPoints < 2) Then
            Return
        End If
        
        Dim verticalDim As Boolean = ((_current.X > _points(0).X) AndAlso (_current.X > _points(1).X)) OrElse ((_current.X < _points(0).X) AndAlso (_current.X < _points(1).X))

        Dim axisX As Vector3D

        Dim convertedDimTextHeight As Double = (DimTextHeight * GetUnitsConversionFactor)

        If verticalDim Then
            axisX = Vector3D.AxisY

            _extPt1 = New Point3D(_current.X, _points(0).Y)
            _extPt2 = New Point3D(_current.X, _points(1).Y)

            If ((_current.X > _points(0).X) AndAlso (_current.X > _points(1).X)) Then
                _extPt1.X = _extPt1.X  + (convertedDimTextHeight / 2)
                _extPt2.X = _extPt2.X + (convertedDimTextHeight / 2)
            Else
                _extPt1.X = _extPt1.X - (convertedDimTextHeight / 2)
                _extPt2.X = _extPt2.X - (convertedDimTextHeight / 2)
            End If
            
        Else
            axisX = Vector3D.AxisX

            _extPt1 = New Point3D(_points(0).X, _current.Y)
            _extPt2 = New Point3D(_points(1).X, _current.Y)

            If (_current.Y > _points(0).Y) AndAlso (_current.Y > _points(1).Y) Then
                _extPt1.Y = _extPt1.Y + (convertedDimTextHeight / 2)
                _extPt2.Y = _extPt2.Y + (convertedDimTextHeight / 2)
            Else
                _extPt1.Y = _extPt1.Y - (convertedDimTextHeight / 2)
                _extPt2.Y = _extPt2.Y - (convertedDimTextHeight / 2)
            End If
            
        End If
        
        ' defines the Y axis
        Dim axisY As Vector3D = Vector3D.Cross(Vector3D.AxisZ, axisX)

        Dim pts As List(Of Point3D) = New List(Of Point3D)

        ' draws extension line1
        pts.Add(WorldToScreen(_points(0)))
        pts.Add(WorldToScreen(_extPt1))

        ' draws extension line2
        pts.Add(WorldToScreen(_points(1)))
        pts.Add(WorldToScreen(_extPt2))

        ' draws dimension line
        Dim extLine1 As Segment3D = New Segment3D(_points(0), _extPt1)
        Dim extLine2 As Segment3D = New Segment3D(_points(1), _extPt2)
        Dim pt1 As Point3D = _current.ProjectTo(extLine1)
        Dim pt2 As Point3D = _current.ProjectTo(extLine2)

        pts.Add(WorldToScreen(pt1))
        pts.Add(WorldToScreen(pt2))

        renderContext.DrawLines(pts.ToArray)

        ' stores dimensioning plane
        _drawingPlane = New Plane(_points(0), axisX, axisY)

        ' draws dimension text
        renderContext.EnableXOR(false)

        ' calculates the scaled distance
        Dim scaledDistance As Double = _extPt1.DistanceTo(_extPt2) * (1 / _viewScale)
        Dim dimText As String = "L " + scaledDistance.ToString("f3")
        DrawText(_mouseLocation.X, ((Size.Height - _mouseLocation.Y) + 10), dimText, New Font("Tahoma", 8.25!), DrawingColor, ContentAlignment.BottomLeft)
    End Sub

End Class