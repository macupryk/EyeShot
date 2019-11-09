Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Dragging As Boolean = False
        Public SelMapping As textureMappingType = textureMappingType.Spherical
        Private _selMaterial As String = ""
        Public Sub New()
            InitializeComponent()

            ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
            textBoxU.Text = String.Format("{0:f1}", 1)
            textBoxV.Text = String.Format("{0:f1}", 1)

        End Sub

        Private Sub model1_InitializeScene(sender As Object, e As EventArgs) Handles model1.InitializeScene
            ' Creates material sphere images and fills the listview
            createTexture()

            ' Deletes all entities in the scene
            model1.Entities.Clear()

            ' Adds entities to display
            Primitives()           

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport
            model1.ZoomFit()
        End Sub

        Private Sub viewport_dragEnter(sender As Object, e As DragEventArgs)
            If dragging Then
                ' shows copy cursor inside the viewport
                e.Effects = DragDropEffects.Copy
            End If
        End Sub

        Private Sub viewport_dragDrop(sender As Object, e As DragEventArgs)
            If dragging Then
                e.Effects = DragDropEffects.None
                If _selMaterial <> "" Then
                    ' gets target entity
                    Dim selectedIndex As Integer = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint((model1.GetMousePosition(e))))
                   

                    If selectedIndex <> -1 Then
                        Dim selEntity As Entity = model1.Entities(selectedIndex)
                        If selEntity IsNot Nothing Then
                            ' gets scale u,v value 
                            Dim u As Double, v As Double
                            [Double].TryParse(Me.textBoxU.Text, u)
                            [Double].TryParse(Me.textBoxV.Text, v)

                            ' assigns the material to all triangles and maps the material texture with specific mapping
                            DirectCast(selEntity, Mesh).ApplyMaterial(_selMaterial, selMapping, u, v)
                            model1.Entities.Regen()
                        End If
                    End If
                End If
                dragging = False
                model1.Invalidate()
            End If
        End Sub

        Private Sub listView1_DragEnter(sender As Object, e As DragEventArgs)
            If _selMaterial <> "" Then
                ' shows copy cursor inside the listView
                e.Effects = DragDropEffects.Copy
                If Not dragging Then
                    dragging = True
                    ' start a dragdrop action to viewport
                    DragDrop.DoDragDrop(model1, _selMaterial, DragDropEffects.Copy)
                End If
            End If
        End Sub

        Private Sub listView1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Me.listView1.SelectedItems.Count > 0 Then
                ' saves the selected material to be apply 
                _selMaterial = DirectCast(Me.listView1.SelectedItems(0), ImageItem).Name

                ' start a dragdrop anction to listView1
                Try
                    DragDrop.DoDragDrop(DirectCast(sender, ListView), _selMaterial, DragDropEffects.Copy)
                Catch
                End Try

                ' clear selection
                Me.listView1.SelectedItems.Clear()
            End If
        End Sub

        Private Sub comboBoxMapping_SelectionChanged(sender As Object, e As EventArgs)
            Dim mapping As String = DirectCast(comboBoxMapping.SelectedItem, ComboBoxItem).Name.ToString()
            Select Case mapping
                Case "Spherical"
                    selMapping = textureMappingType.Spherical
                    Exit Select
                Case "Cubic"
                    selMapping = textureMappingType.Cubic
                    Exit Select
                Case "Cylindrical"
                    selMapping = textureMappingType.Cylindrical
                    Exit Select
                Case "Plate"
                    selMapping = textureMappingType.Plate
                    Exit Select
                Case Else
                    selMapping = textureMappingType.Spherical
                    Exit Select
            End Select
        End Sub

        Public Sub Primitives()
            model1.GetGrid().[Step] = 25
            model1.GetGrid().Min.X = -25
            model1.GetGrid().Min.Y = -25

            model1.GetGrid().Max.X = 125
            model1.GetGrid().Max.Y = 175

            Dim deltaOffset As Double = 50
            Dim offsetX As Double = 0
            Dim offsetY As Double = 0

            Dim color As System.Drawing.Color = System.Drawing.Color.SlateGray

            ' First Row

            ' Box
            Dim mesh__1 As Mesh = Mesh.CreateBox(40, 40, 30)
            mesh__1.Translate(-20, -20, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' Cone
            mesh__1 = Mesh.CreateCone(20, 10, 30, 30, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' second cone
            mesh__1 = Mesh.CreateCone(20, 0, 30, 30, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' Second Row
            offsetX = 0
            offsetY += deltaOffset

            ' Cylinder
            mesh__1 = Mesh.CreateCylinder(15, 25, 20, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' prism
            mesh__1 = Mesh.CreateCone(20, 10, 30, 3, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' Sphere
            mesh__1 = Mesh.CreateSphere(20, 3, 3, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)

            ' Third Row
            offsetX = 0
            offsetY += deltaOffset

            ' sphere
            mesh__1 = Mesh.CreateSphere(20, 8, 6, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' second sphere
            mesh__1 = Mesh.CreateSphere(20, 50, 50, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' torus
            mesh__1 = Mesh.CreateTorus(18, 5, 15, 17, Mesh.natureType.Smooth)
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' Fourth Row
            offsetX = 0
            offsetY += deltaOffset

            ' spring
            mesh__1 = Mesh.CreateSpring(10, 2, 16, 24, 10, 6, _
                True, True, Mesh.natureType.Smooth)
            mesh__1.EdgeStyle = Mesh.edgeStyleType.None
            mesh__1.Translate(offsetX, offsetY, 0)
            model1.Entities.Add(mesh__1, color)
            offsetX += deltaOffset

            ' Sweep
            Dim z As Double = 30
            Dim radius As Double = 15

            Dim l1 As New devDept.Eyeshot.Entities.Line(0, 0, 0, 0, 0, z)
            Dim a1 As New Arc(New Point3D(radius, 0, z), New Point3D(0, 0, z), New Point3D(radius, 0, z + radius))
            Dim l2 As New devDept.Eyeshot.Entities.Line(radius, 0, z + radius, 30, 0, z + radius)

            Dim composite As New CompositeCurve(l1, a1, l2)
            Dim lpOuter As New LinearPath(10, 16)
            Dim lpInner As New LinearPath(5, 11)
            lpInner.Translate(2.5, 2.5, 0)
            lpInner.Reverse()

            Dim profile As New Region(lpOuter, lpInner)
            mesh__1 = profile.SweepAsMesh(composite, 0.1)
            mesh__1.Translate(offsetX - 10, offsetY - 8, 0)
            model1.Entities.Add(mesh__1, color)

            ' Hexagon with hole region revolved
            offsetX += deltaOffset

            Dim lp As New LinearPath(7)
            For i As Integer = 0 To 360 Step 60
                lp.Vertices(i / 60) = New Point3D(10 * Math.Cos(Utility.DegToRad(i)), 10 * Math.Sin(Utility.DegToRad(i)), 0)
            Next

            Dim circle As New Circle(New Point3D(0, 0, 0), 7)
            circle.Reverse()
            Dim profile2 As New Region(lp, circle)
            mesh__1 = profile2.RevolveAsMesh(0, Utility.DegToRad(10), Vector3D.AxisX, new Point3D(0, -60, 0), Utility.NumberOfSegments(60, Math.PI / 6, 0.1), 0.1, Mesh.natureType.Smooth)
            mesh__1.FlipNormal()
            mesh__1.Scale(2, 2, 2)

            mesh__1.Translate(offsetX, offsetY, 0)
            mesh__1.EdgeStyle = Mesh.edgeStyleType.Sharp
            mesh__1.SmoothingAngle = Utility.DegToRad(59)
            model1.Entities.Add(mesh__1, color)

        End Sub

    End Class
End Namespace
