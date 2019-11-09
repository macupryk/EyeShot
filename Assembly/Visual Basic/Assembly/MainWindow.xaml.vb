Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Private inners As LinearPath()
        Private shapes As Entity()

        Public Sub New()
            InitializeComponent()
            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            comboBoxAnimation.SelectedIndex = 0

            model1.ZoomFit()

            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub buttonHexagon_Click(sender As Object, e As RoutedEventArgs)
            ' if there is an active animation don't do anything
            If model1.AnimationFrameNumber > 0 Then
                Return
            End If

            ' animate one shape at a time            
            If Not animating Then
                MoveShapes(0)
                buttonHexagon.IsEnabled = False
            End If
        End Sub

        Private Sub buttonTriangle_Click(sender As Object, e As RoutedEventArgs)
            ' if there is an active animation don't do anything
            If model1.AnimationFrameNumber > 0 Then
                Return
            End If

            ' animate one shape at a time
            If Not animating Then
                MoveShapes(1)
                buttonTriangle.IsEnabled = False
            End If
        End Sub

        Private Sub buttonReset_Click(sender As Object, e As RoutedEventArgs)
            Reset()
        End Sub

        Private Sub comboBoxAnimation_OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Reset()
        End Sub

        Private Sub Reset()

            ' Clear the viewport and build the shapes
            model1.Entities.Clear()
            model1.Materials.Clear()
            model1.Blocks.Clear()

            BuildShapesAndTransformations()

            ' Enable buttons
            buttonTriangle.IsEnabled = True
            buttonHexagon.IsEnabled = True

            If animating Then
                animating = False
                myTimer.[Stop]()
            End If

            model1.Invalidate()
        End Sub

        Private Sub BuildShapesAndTransformations()
        	inners = New LinearPath(1) {}
        
        	startOrientation = New Quaternion(1) {}
        	finalOrientation = New Quaternion(1) {}
        
        	' Hexagon
        	Dim c As CompositeCurve = CompositeCurve.CreateHexagon(5)
            c.Regen(0)
        	inners(0) = New LinearPath(c.Vertices)
        	inners(0).Reverse()
        
        	startOrientation(0) = New Quaternion(Vector3D.AxisZ, 180 / 2.0)
        	Dim transf As Transformation = New Translation(7, 0, 0) * New Rotation(Math.PI / 2, Vector3D.AxisZ)
        	inners(0).TransformBy(transf)
        
        	' Triangle
        	inners(1) = New LinearPath(New Point3D() {New Point3D(0, 0, 0), New Point3D(7, 0, 0), New Point3D(3.5, 7, 0), New Point3D(0, 0, 0)})
        
        	inners(1).Reverse()
        	transf = New Translation(23, 0, 0) * New Rotation(Math.PI / 3, Vector3D.AxisZ)
        	startOrientation(1) = New Quaternion(Vector3D.AxisZ, 180 / 3.0)
        	inners(1).TransformBy(transf)
        
        	' Extrude the 2 inner profiles to build 2 shapes
        	shapes = New Entity(1) {}
        
        	Dim firstInnerReg As New devDept.Eyeshot.Entities.Region(inners(0), Plane.XY, False)
        
        	shapes(0) = firstInnerReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain)
        	shapes(0).ColorMethod = colorMethodType.byEntity
        	shapes(0).Color = System.Drawing.Color.Green
        
        	Dim secondInnerReg As New devDept.Eyeshot.Entities.Region(inners(1), Plane.XY, False)
        
        	shapes(1) = secondInnerReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain)
        	shapes(1).ColorMethod = colorMethodType.byEntity
        	shapes(1).Color = System.Drawing.Color.Gainsboro
        
        
        	' Save the original shapes for the animation
        	originalShapes = New Entity() {DirectCast(shapes(0).Clone(), Entity), DirectCast(shapes(1).Clone(), Mesh)}
        
        	Dim outer As New LinearPath(New Point3D() {New Point3D(0, -10, 0), New Point3D(30, -10, 0), New Point3D(30, 10, 0), New Point3D(0, 10, 0), New Point3D(0, -10, 0)})
        
        	Dim plate As New devDept.Eyeshot.Entities.Region(New ICurve() {outer, inners(0), inners(1)}, Plane.XY, False)
        
        	' Build a mesh with 2 holes
        	Dim m As Mesh = plate.ExtrudeAsMesh(3, 0.1, Mesh.natureType.Plain)
        
        	' Transform the mesh and the the 2 inner profiles, to position them in the exact place of the holes
        	transf = New Translation(0, 3, 10) * New Rotation(Math.PI / 2, Vector3D.AxisX)
        	m.TransformBy(transf)
        	model1.Entities.Add(m, System.Drawing.Color.Brown)
        
        	inners(0).TransformBy(transf)
        	inners(1).TransformBy(transf)
        
        	' Rotation quaternion of the 2 inners
        	Dim q As New Quaternion(Vector3D.AxisX, 90)
        	finalOrientation(0) = q * startOrientation(0)
        	finalOrientation(1) = q * startOrientation(1)
        
        	' Define a Transformation for the 2 shapes, and store the rotation Quaternion
        	transf = New Translation(20, -25, 0) * New Rotation(Math.PI / 9, Vector3D.AxisZ)
        	startOrientation(0) = New Quaternion(Vector3D.AxisZ, 180 / 9.0) * startOrientation(0)
        
        	shapesTransform = New Transformation(1) {}
        	shapesTransform(0) = transf
        
        	transf = New Translation(-10, -44, 0) * New Rotation(Math.PI / 5, Vector3D.AxisZ)
        	shapesTransform(1) = transf
        	startOrientation(1) = New Quaternion(Vector3D.AxisZ, 180 / 5.0) * startOrientation(1)
        
        	If comboBoxAnimation.SelectedIndex = 1 Then
        		' Block Reference Animation
        
        		' Add the Blocks to the viewport and create the BlockReferences for the animation
        		AddBlockDefinition(New Block("B1"), 0)
        		AddBlockDefinition(New Block("B2"), 1)
        	Else
        		' Transform the shapes
        		shapes(0).TransformBy(shapesTransform(0))
        		shapes(1).TransformBy(shapesTransform(1))
        	End If
        
        	' Add the entities to the viewport
        	model1.Entities.Add(shapes(0))
        	model1.Entities.Add(shapes(1))
        End Sub

        Private Sub MoveShapes(index As Integer)
            Select Case comboBoxAnimation.SelectedIndex
                Case 0
                    Transformation(index)
                    Exit Select

                Case 1
                    Animation(index)
                    Exit Select

                Case 2
                    Direct(index)
                    Exit Select
            End Select
        End Sub

#Region "Transformation"

        Private originalShapes As Entity()
        Private shapesTransform As Transformation()
        Private startOrientation As Quaternion(), finalOrientation As Quaternion()
        Private stepTranslation As Vector3D
        Private rotationAxis As Vector3D
        Private stepAngle As Double
        Private shapeIndex As Integer

        ' Animation steps and time
        Private animationSteps As Integer = 40
        Private animationtime As Integer = 1000
        Private myTimer As DispatcherTimer

        Private animating As Boolean

        Private Sub Transformation(index As Integer)
            Dim axis As Vector3D
            Dim angle As Double

            ' Compute the quaternion necessary to rotate from the start orientation to the final orientation
            startOrientation(index).ToAxisAngle(axis, angle)
            axis.Negate()
            Dim inverseQuat As New Quaternion(axis, angle)
            Dim q As Quaternion = finalOrientation(index) * inverseQuat

            q.ToAxisAngle(rotationAxis, stepAngle)

            ' Angle of rotation for each animation frame
            stepAngle /= animationSteps

            ' Index of the shape to animate
            shapeIndex = index

            ' Compute the translation factor for each frame of animation, from the initial position to the final position
            Dim translationVect As Vector3D = Vector3D.Subtract(inners(index).Vertices(0), shapes(index).Vertices(0))
            stepTranslation = translationVect / animationSteps

            animationFrame = 0
            animating = True

            ' Start a timer for the animation
            myTimer = New DispatcherTimer()
            myTimer.Interval = New TimeSpan(animationtime \ animationSteps)
            AddHandler myTimer.Tick, AddressOf TransformShape
            myTimer.Start()
        End Sub

        Private animationFrame As Integer = 0

        Private Sub TransformShape(sender As Object, e As EventArgs)
            animationFrame += 1

            ' remove the old shape from the viewport
            model1.Entities.Remove(shapes(shapeIndex))

            ' work on the original cloned shape
            Dim m As Mesh = DirectCast(originalShapes(shapeIndex).Clone(), Mesh)

            ' translate the shape to the origin
            Dim firstPt As Point3D = DirectCast(m.Vertices(0).Clone(), Point3D)
            Dim t As Transformation = New Translation(-firstPt.X, -firstPt.Y, -firstPt.Z)

            ' rotate it by an angle proportional to the frame number
            t = New Rotation(Utility.DegToRad(animationFrame * stepAngle), rotationAxis) * t

            ' translate it back to the original position
            t = New Translation(firstPt.X, firstPt.Y, firstPt.Z) * t

            ' apply the transformations to the shape
            t = shapesTransform(shapeIndex) * t

            ' translate it towards the final position by an amount proportional to the frame number
            t = New Translation(stepTranslation * animationFrame) * t

            m.TransformBy(t)

            shapes(shapeIndex) = m

            ' add the new shape to the viewport
            model1.Entities.Add(m)
            model1.Invalidate()

            If animationFrame = animationSteps Then
                myTimer.[Stop]()
                animating = False
            End If

        End Sub
#End Region

#Region "Animation"

        Private Sub Animation(index As Integer)

            model1.StartAnimation(1, animationSteps)
            Dim br As AnimatedBlockRef = DirectCast(shapes(index), AnimatedBlockRef)
            br.animateFlag = True
            br.deltaT = 1.0 / animationSteps

        End Sub

        Private Sub AddBlockDefinition(block As Block, index As Integer)
            block.Entities.Add(shapes(index))
            model1.Blocks.Add(block)

            ' Create a blockReference
            Dim br As New AnimatedBlockRef(shapesTransform(index), block.Name)

            ' Store the orientation quaternions
            br.startOrientation = startOrientation(index)
            br.finalOrientation = finalOrientation(index)

            ' Store the translation vector between initial and final positions
            br.translationVect = Vector3D.Subtract(inners(index).Vertices(0), br.Transformation * block.Entities(0).Vertices(0))

            ' Store the first point (for correct rotation around origin)
            br.firstPt = DirectCast(block.Entities(0).Vertices(0).Clone(), Point3D)

            ' Initialize the data for animation
            br.Init()

            ' Put the BlockReference in the shapes array
            shapes(index) = br
        End Sub
#End Region

#Region "Direct"
        Private Sub Direct(index As Integer)

            ' Transform the shape for the initial position to the final position in one shot
            Dim initialPlane As New Plane(shapes(index).Vertices(0), shapes(index).Vertices(1), shapes(index).Vertices(2))
            Dim finalPlane As New Plane(inners(index).Vertices(0), inners(index).Vertices(1), inners(index).Vertices(2))

            Dim t As New Transformation()
            t.Rotation(initialPlane, finalPlane)

            shapes(index).TransformBy(t)

            model1.Entities.Regen()
            model1.Invalidate()

        End Sub
#End Region

    End Class
End Namespace