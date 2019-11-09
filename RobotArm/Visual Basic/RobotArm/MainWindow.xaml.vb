Imports System.IO
Imports System.Windows.Controls.Primitives
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Translators
Imports devDept.Geometry
Imports devDept.Graphics
Imports Color = System.Drawing.Color

    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Public Partial Class MainWindow
        Inherits Window

        Private r1 As BlockReference, r2 As BlockReference, r3 As BlockReference, r4 As BlockReference, r5 As BlockReference, r6 As BlockReference        
        Private Shared cd As CollisionDetection = Nothing
        Private _checkMethod As CollisionDetection.collisionCheckType = CollisionDetection.collisionCheckType.OB
        Private firstOnly As Boolean = False
        Private degreeAngle As Double = 9
        Private selectedPart As String = "A1"
        Private previousValue As Integer = 0

#If NURBS
        Private Const FileName As String = "../../../../../../dataset/Assets/RobotArm.eye"
#Else    
        Private Const FileName As String = "../../../../../../dataset/Assets/RobotArm_PRO.eye"
#End If

    Public Sub New()
            InitializeComponent()

            ' Model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
       
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' Builds robot arm model and adds it to the scene
            AddRobotArm()
            ' Builds a generic model to test the collision detection and adds it to the scene
            AddCollisionModel()
        
            ' Hides grid and origin symbol
            Model1.GetGrid().Visible = false
            Model1.GetOriginSymbol().Visible = false

            ' Fits the model in the viewport
            Model1.ZoomFit()

            ' Refreshes the model control
            Model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    
        Public Sub EnableButtons()

            collisionButton.IsEnabled = true
            collisionMethodCombo.IsEnabled = true
            rotate1.IsEnabled = true
            rotate2.IsEnabled = true
            rotate3.IsEnabled = true
            rotate4.IsEnabled = true
            rotate5.IsEnabled = true
            rotate6.IsEnabled = true
            movePosButton.IsEnabled = True
            moveNegButton.IsEnabled = True
            rotateSlider.IsEnabled = True
            firstCheckBox.IsEnabled = true
        End Sub
    
        Public Sub DisableButtons()
            collisionButton.IsEnabled = false
            collisionMethodCombo.IsEnabled = false
            rotate1.IsEnabled = false
            rotate2.IsEnabled = false
            rotate3.IsEnabled = false
            rotate4.IsEnabled = false
            rotate5.IsEnabled = false
            rotate6.IsEnabled = false
            movePosButton.IsEnabled = False
            moveNegButton.IsEnabled = False
            rotateSlider.IsEnabled = False
            firstCheckBox.IsEnabled = false
        End Sub
    
        Private Sub AddCollisionModel()
        Dim baseFoot As Double = 50
        Dim heightFoot As Double = 300
        Dim widthTable As Double = 550
        Dim depthTable As Double = 325
        Dim thickTable As Double = 50
        Dim radius As Double = 10
        Dim heightElem As Double = 50

#If NURBS Then 
        ' Foot table block
        Dim s As Brep = Brep.CreateBox(baseFoot, baseFoot, heightFoot)
        s.ColorMethod = colorMethodType.byParent
        Dim foot As New Block("foot")
        foot.Entities.Add(s)
        model1.Blocks.Add(foot)

        ' Plane table block
        Dim s1 As Brep = Brep.CreateBox(widthTable, depthTable, thickTable)
        s1.ColorMethod = colorMethodType.byParent
        Dim plate As New Block("plate")
        plate.Entities.Add(s1)
        model1.Blocks.Add(plate)

        ' Single elem block
        Dim s2 As Brep = Brep.CreateCylinder(radius, heightElem)
        s2.Translate(radius, radius, 0)
        s2.ColorMethod = colorMethodType.byParent
        Dim elem As New Block("elem")
        elem.Entities.Add(s2)
        model1.Blocks.Add(elem)

#Else  
        ' Foot table block
        Dim s As Mesh = Mesh.CreateBox(baseFoot, baseFoot, heightFoot)
        s.ColorMethod = colorMethodType.byParent
        Dim foot As New Block("foot")
        foot.Entities.Add(s)
        model1.Blocks.Add(foot)

        ' Plane table block
        Dim s1 As Mesh = Mesh.CreateBox(widthTable, depthTable, thickTable)
        s1.ColorMethod = colorMethodType.byParent
        Dim plate As New Block("plate")
        plate.Entities.Add(s1)
        model1.Blocks.Add(plate)

        ' Single elem block
        Dim s2 As Mesh = Mesh.CreateCylinder(radius, heightElem, 20)
        s2.Translate(radius, radius, 0)
        s2.ColorMethod = colorMethodType.byParent
        Dim elem As New Block("elem")
        elem.Entities.Add(s2)
        model1.Blocks.Add(elem)
#End If 
        ' Table block
        Dim table As New Block("table")
        table.Entities.Add(New BlockReference(0, 0, 0, "foot", 0) With { _
                              .ColorMethod = colorMethodType.byParent _
                              })
        table.Entities.Add(New BlockReference(widthTable - baseFoot, 0, 0, "foot", 0) With { _
                              .ColorMethod = colorMethodType.byParent _
                              })
        table.Entities.Add(New BlockReference(widthTable - baseFoot, depthTable - baseFoot, 0, "foot", 0) With { _
                              .ColorMethod = colorMethodType.byParent _
                              })
        table.Entities.Add(New BlockReference(0, depthTable - baseFoot, 0, "foot", 0) With { _
                              .ColorMethod = colorMethodType.byParent _
                              })
        table.Entities.Add(New BlockReference(0, 0, heightFoot, "plate", 0) With { _
                              .ColorMethod = colorMethodType.byParent _
                              })
        model1.Blocks.Add(table)

        ' Elem's grid block
        Dim grid As New Block("grid")
        Dim offset As Double = 100
        Dim offsetX As Double = 25
        Dim offsetY As Double = 25
        While offsetX < widthTable - 5
            grid.Entities.Add(New BlockReference(offsetX, offsetY, 0, "elem", 0) With { _
                                 .ColorMethod = colorMethodType.byParent _
                                 })
            While offsetY < depthTable - 5
                grid.Entities.Add(New BlockReference(offsetX, offsetY, 0, "elem", 0) With { _
                                     .ColorMethod = colorMethodType.byParent _
                                     })
                offsetY += radius * 2 + offset
            End While
            offsetY = 25
            offsetX += radius * 2 + offset
        End While
        model1.Blocks.Add(grid)

        ' Final container block
        Dim container As New Block("container")

        Dim item As New BlockReference(0, 0, 0, "table", 0)
        item.ColorMethod = colorMethodType.byEntity
        item.Color = Color.Gray
        container.Entities.Add(item)

        Dim item2 As New BlockReference(0, 0, heightFoot + thickTable, "grid", 0)
        item2.ColorMethod = colorMethodType.byEntity
        item2.Color = Color.Azure
        container.Entities.Add(item2)

        ' Adds container to the blocks collection
        model1.Blocks.Add(container)

        ' Adds container reference to the scene
        model1.Entities.Add(New BlockReference(250, -125, 0, "container", 0), "default", Color.WhiteSmoke)
    End Sub
    
    Private Sub AddRobotArm()
        Dim robotEntities As Entity() = New Entity(6) {}

        Dim AP1 As Point3D
        Dim AP2 As Point3D
        Dim AP3 As Point3D
        Dim AP4 As Point3D
        Dim AP5 As Point3D
        Dim AP6 As Point3D

        AP1 = New Point3D(0, 0, 0)
        AP2 = New Point3D(25, 0, 400)
        AP3 = New Point3D(25, 0, 855)
        AP4 = New Point3D(25, 0, 890)
        AP5 = New Point3D(445, 0, 890)
        AP6 = New Point3D(525, 0, 890)

        model1.OpenFile(FileName)
        For i As Integer = 0 To model1.Entities.Count - 1
            robotEntities(i) = model1.Entities(i)
        Next
        model1.Entities.Clear()
       
        ' Creates a dictionary to identify the robot arm part index from its name

        Dim robotParts As New Dictionary(Of String, Integer)()

        Dim robotPartNames As String() = New String(6) {}

        robotPartNames(0) = "E0"
        robotPartNames(1) = "E1"
        robotPartNames(2) = "E2"
        robotPartNames(3) = "E3"
        robotPartNames(4) = "E4"
        robotPartNames(5) = "E5"
        robotPartNames(6) = "E6"

        For i As Integer = 0 To robotPartNames.Length - 1
            robotParts.Add(robotPartNames(i), i)
        Next
        
        ' Creates a BlockReference for each group of entities that represents a body part
        ' and uses the EntityData property to store the rotation center.

        r6 = BuildBlockReference("P6", New Entity() {robotEntities(robotParts("E6"))})
        r6.EntityData = New Point3D(AP6.X, AP6.Y, AP6.Z)

        r5 = BuildBlockReference("P56", New Entity() {robotEntities(robotParts("E5")), r6})
        r5.EntityData = New Point3D(AP5.X, AP5.Y, AP5.Z)

        r4 = BuildBlockReference("P456", New Entity() {robotEntities(robotParts("E4")), r5})
        r4.EntityData = New Point3D(AP4.X, AP4.Y, AP4.Z)

        r3 = BuildBlockReference("P3456", New Entity() {robotEntities(robotParts("E3")), r4})
        r3.EntityData = New Point3D(AP3.X, AP3.Y, AP3.Z)

        r2 = BuildBlockReference("P23456", New Entity() {robotEntities(robotParts("E2")), r3})
        r2.EntityData = New Point3D(AP2.X, AP2.Y, AP2.Z)

        r1 = BuildBlockReference("P123456", New Entity() {robotEntities(robotParts("E1")), r2})
        r1.EntityData = New Point3D(AP1.X, AP1.Y, AP1.Z)
        
        Dim brRobot As BlockReference = BuildBlockReference("Robot", New Entity() {robotEntities(robotParts("E0")), r1})
        
        model1.Layers(0).Color = Color.SandyBrown
        model1.Entities.Add(brRobot)
    End Sub

#If NURBS Then   
        Private Function CreateBlockReferenceSTEP(partName As String, stream As Stream) As BlockReference
            Dim readStep As New ReadSTEP(stream, True)
            Model1.DoWork(readStep)

            Dim entity As Entity() = readStep.Entities

            Dim bl As New Block(partName)

            For i As Integer = 0 To entity.Length - 1
                Dim fixedSolid As Brep = CType(entity(i), Brep)
                fixedSolid.FixTopology(fixedSolid)
                bl.Entities.Add(fixedSolid)
            Next

            Model1.Blocks.Add(bl)
            Dim br As New BlockReference(New Identity(), partName)
            br.ColorMethod = colorMethodType.byEntity
            Return br
        End Function
#End If 
        Private Function BuildBlockReference(newName As String, entities As IList(Of Entity)) As BlockReference
            ' Creates a new BlockReference from the given list of entities
            Dim bl As New Block(newName)
            bl.Entities.AddRange(entities)

            Model1.Blocks.Add(bl)

            Dim br As New BlockReference(New Identity(), newName)
            br.ColorMethod = colorMethodType.byEntity
            Return br
        End Function
    
        Private Sub Model1OnWorkCompleted(sender As Object, workCompletedEventArgs As WorkCompletedEventArgs) 
            ' Clears previous selection
            Model1.Entities.ClearSelection()

            If cd.Result IsNot Nothing And cd.Result.Count > 0 Then
                For i As Integer = 0 To cd.Result.Count - 1
                    Dim tuple As Tuple(Of CollisionDetection.CollisionResultItem, CollisionDetection.CollisionResultItem) = cd.Result(i)
                
                    ' Selects the intersecting entities
                    tuple.Item1.Entity.SetSelection(True, tuple.Item1.Parents)
                    tuple.Item2.Entity.SetSelection(True, tuple.Item2.Parents)
                Next
                Me.intersectLabel.Content = "True"
            
                Dim fromMillisecond As New TimeSpan(0, 0, 0, 0, CInt(cd.ExecutionTime))
                timeLabel.Content = String.Format("{0:D2}m:{1:D2}s:{2:D3}ms", fromMillisecond.Minutes, fromMillisecond.Seconds, fromMillisecond.Milliseconds)

                numLabel.Content = cd.Result.Count.ToString()
            Else
                Me.intersectLabel.Content = "False"

                Dim fromMillisecond As New TimeSpan(0, 0, 0, 0, CInt(cd.ExecutionTime))
                timeLabel.Content = String.Format("{0:D2}m:{1:D2}s:{2:D3}ms", fromMillisecond.Minutes, fromMillisecond.Seconds, fromMillisecond.Milliseconds)

                numLabel.Content = "0"
            End If
            EnableButtons()
            Model1.Invalidate()
        End Sub
    
        Private Sub SelectPartToRotate(ByVal sender As Object, ByVal e As EventArgs) 
            selectedPart = CType(sender,RadioButton).Content
            previousValue = 0
            rotateSlider.Value = 0
        End Sub
    
        Private Sub Slider_ValueChanged(ByVal sender As Object, ByVal e As EventArgs) 
            Dim value As Integer = (CType(sender,Slider).Value - previousValue)
            previousValue = CType(sender,Slider).Value
            RotateAxis(selectedPart, (degreeAngle * value))
            ' Refreshes the model control
            model1.Invalidate
        End Sub
    
        Private Sub Slider_DragCompleted(ByVal sender As Object, ByVal e As DragCompletedEventArgs) 
            ' if collision detection is enable starts a collision check
            If (Not (cd) Is Nothing) Then
                model1.StartWork(cd)
                DisableButtons
            End If                    
        End Sub
    
        Private Sub RotateAxis(ByVal Axis As String, ByVal Degree As Double)
            Dim angleInRadians As Double = Utility.DegToRad(Degree)
            Select Case (Axis)
                Case "A1"
                    r1.Rotate(angleInRadians, Vector3D.AxisZ, DirectCast(r1.EntityData, Point3D))
                Case "A2"
                    r2.Rotate(angleInRadians, Vector3D.AxisY, DirectCast(r2.EntityData, Point3D))
                Case "A3"
                    r3.Rotate(angleInRadians, Vector3D.AxisY, DirectCast(r3.EntityData, Point3D))
                Case "A4"
                    r4.Rotate(angleInRadians, Vector3D.AxisX, DirectCast(r4.EntityData, Point3D))
                Case "A5"
                    r5.Rotate(angleInRadians, Vector3D.AxisY, DirectCast(r5.EntityData, Point3D))
                Case "A6"
                    r6.Rotate(angleInRadians, Vector3D.AxisX, DirectCast(r6.EntityData, Point3D))
            End Select
        
            ' Regenerates entities
            model1.Entities.Regen()
        End Sub
    
        Private Sub collisionMethodCombo_OnSelectionChanged(ByVal sender As Object, ByVal e As EventArgs)
            ' Changes the collision accuracy method
            Select Case (collisionMethodCombo.SelectedIndex)
                Case 0
                    'OBB
                    _checkMethod = CollisionDetection.collisionCheckType.OB
                Case 1
                    'OBB with Octree
                    _checkMethod = CollisionDetection.collisionCheckType.OBWithSubdivisionTree
                Case 2
                    'Octree
                    _checkMethod = CollisionDetection.collisionCheckType.SubdivisionTree
                Case 3
                    'Geometric intersection
                    _checkMethod = CollisionDetection.collisionCheckType.Accurate
                Case Else
                    _checkMethod = CollisionDetection.collisionCheckType.OB
            End Select
        
            If cd IsNot Nothing Then
                ' Updates collision detection accuracy method and start a collision
                cd.CheckMethod = _checkMethod
                model1.StartWork(cd)
                DisableButtons
            End If
        
        End Sub
    
        Private Sub collisionButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            If collisionButton.IsChecked Then
                collisionButton.Content = "Disable Collision Detection"
                If (collisionMethodCombo.SelectedIndex = -1) Then
                    collisionMethodCombo.SelectedIndex = 0
                End If
            
                ' Sets a new istance of CollisionDetection
                cd = New CollisionDetection(model1.Entities, model1.Blocks, firstOnly, _checkMethod, maxTrianglesNumForOctreeNode:= 5)
                ' Starts the first collision check
                model1.StartWork(cd)
                DisableButtons()
            Else
                collisionButton.Content = "Enable Collision Detection"
                ' Removes all the objects defined for collision detection before to delete this instance
                cd.ClearCache()
                cd = Nothing
                Model1.Entities.ClearSelection()
                Model1.Invalidate()
            End If
        
        End Sub
    
        Private Sub firstCheckBox_CheckedChanged(sender As Object, e As EventArgs)
            If cd IsNot Nothing Then
                ' If geometryIntesection is true, then set the CollisionDetection.FirstOnly flag to false to be sure to find the first really colliding tuple of entities
                cd.FirstOnly = firstCheckBox.IsChecked.Value
                model1.StartWork(cd)
                DisableButtons()
            End If
        End Sub
    
        Private Sub angleText_TextChanged(sender As Object, e As EventArgs) Handles angleText.TextChanged
            Try 
                ' Changes the degree angle used to rotate the robot arm components
                degreeAngle = Double.Parse(angleText.Text)
            Catch generatedExceptionName As Exception
                MessageBox.Show("Insert a valid degree angle.")
                degreeAngle = 9
            End Try
        End Sub
    
        Private Sub movePosButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles movePosButton.Click
            RotateAxis(selectedPart, degreeAngle)
            ' If collision detection is enable starts a collision check
            If (Not (cd) Is Nothing) Then
                model1.StartWork(cd)
                DisableButtons()
            End If
        
            rotateSlider.Value += 1
            ' Refreshes the model control
            model1.Invalidate
        End Sub
    
        Private Sub moveNegButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles moveNegButton.Click
            RotateAxis(selectedPart, (degreeAngle * -1))
            ' If collision detection is enable starts a collision check
            If (Not (cd) Is Nothing) Then
                model1.StartWork(cd)
                DisableButtons()
            End If
        
            rotateSlider.Value -= 1
            ' Refreshes the model control
            model1.Invalidate
        End Sub
    End Class
