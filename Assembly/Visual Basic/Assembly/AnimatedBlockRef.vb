Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

Namespace WpfApplication1
    Class AnimatedBlockRef
        Inherits BlockReference
        Public startOrientation As Quaternion, finalOrientation As Quaternion
        Public animateFlag As Boolean
        Public deltaT As Double
        Private t As Double = 0

        Public translationVect As Vector3D
        Private rotationAxis As Vector3D
        Private rotationAngle As Double
        Public firstPt As Point3D

        Public Sub New(t As Transformation, blockName As String)
            MyBase.New(t, blockName)
        End Sub

        Public Sub Init()
            Dim axis As Vector3D
            Dim angle As Double

            t = 0

            startOrientation.ToAxisAngle(axis, angle)
            axis.Negate()
            Dim inverseQuat As New Quaternion(axis, angle)

            Dim q As Quaternion = finalOrientation * inverseQuat
            q.ToAxisAngle(rotationAxis, rotationAngle)
        End Sub

        Protected Overrides Sub Animate(frameNumber As Integer)
            If Not animateFlag Then
                Return
            End If

            t += deltaT

            If t > 1 Then
                ' Stops the animation
                animateFlag = False
                t = 1
            End If
        End Sub

        Public Overrides Sub MoveTo(data As DrawParams)

            If t <> 0 AndAlso rotationAxis IsNot Nothing Then
                data.RenderContext.TranslateMatrixModelView(t * translationVect.X, t * translationVect.Y, t * translationVect.Z)
                MyBase.MoveTo(data)
                data.RenderContext.TranslateMatrixModelView(firstPt.X, firstPt.Y, firstPt.Z)
                data.RenderContext.RotateMatrixModelView(t * rotationAngle, rotationAxis.X, rotationAxis.Y, rotationAxis.Z)

                data.RenderContext.TranslateMatrixModelView(-firstPt.X, -firstPt.Y, -firstPt.Z)
            Else
                MyBase.MoveTo(data)
            End If
        End Sub

        Public Overrides Function IsInFrustum(ByVal data As FrustumParams, ByVal center As Point3D, ByVal radius As Double) As Boolean
            Dim transfCenter As Point3D = CType(center.Clone(), Point3D)

                If t <> 0 AndAlso rotationAxis IsNot Nothing Then
            
                    ' Get the BlockReference full transformation
                    Dim brFullTransf As Transformation = GetFullTransformation(data.Blocks)

                    ' Apply the inverse of the full transformation to the center to bring it back to original position
                    ' It's necessary because in the MoveTo() the first transformation is applied before tha base method is called.
                    Dim tr As Transformation = CType(brFullTransf.Clone(), Transformation)
                    tr.Invert()
                    transfCenter.TransformBy(tr)

                    ' Compute a transformation equals to the transformation applied in the MoveTo method
                    Dim translation1 As Translation = New Translation(t * translationVect.X, t * translationVect.Y, t * translationVect.Z)
                    Dim translation2 As Translation = New Translation(firstPt.X, firstPt.Y, firstPt.Z)
                    Dim translation3 As Translation = New Translation(-firstPt.X, -firstPt.Y, -firstPt.Z)
                    Dim rotation As Rotation = New Rotation(Utility.DegToRad(t * rotationAngle), rotationAxis)

                    Dim customTransform As Transformation = translation1 * brFullTransf
                    customTransform = customTransform * translation2
                    customTransform = customTransform * rotation
                    customTransform = customTransform * translation3

                    ' Apply transformation to the center
                    transfCenter.TransformBy(customTransform)
                End If

            ' Call the base with the transformed "center", to avoid undesired clipping
            Return MyBase.IsInFrustum(data, transfCenter, radius)
        End Function
    End Class
End Namespace