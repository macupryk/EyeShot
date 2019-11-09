Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports devDept.Eyeshot
Imports devDept.Geometry
Imports devDept.Graphics

Class MyModel
	Inherits devDept.Eyeshot.Model
	Private _current As Point3D
	Private _mouseLocation As System.Drawing.Point

	Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
		_mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e))
		_current = ScreenToWorld(RenderContextUtility.ConvertPoint(GetMousePosition(e)))
		' paint the viewport surface
		PaintBackBuffer()
		' consolidates the drawing
		SwapBuffers()
		MyBase.OnMouseMove(e)
	End Sub

	Protected Overrides Sub DrawOverlay(myParams As Model.DrawSceneParams)
		' text drawing
		If _current IsNot Nothing Then
			DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10, "Point Coord: " + _current.ToString(), New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.Black, System.Drawing.ContentAlignment.BottomLeft)
		Else
			DrawText(_mouseLocation.X, Size.Height - _mouseLocation.Y + 10, "Depth for Transparency", New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.Black, System.Drawing.ContentAlignment.BottomLeft)
		End If
		MyBase.DrawOverlay(myParams)
	End Sub
End Class
