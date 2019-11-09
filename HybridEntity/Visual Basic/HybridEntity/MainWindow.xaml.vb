Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()
            'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            CreateHybridEntity(model1)

            model1.BoundingBox.Visible = True

            model1.ZoomFit()

            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Public Shared Sub CreateHybridEntity(vp As Model)
            Dim width As Double = 0.5
            Dim profilePoints As Point3D() = New Point3D() {New Point3D(-5, 0, 0), New Point3D(5, 0, 0), New Point3D(5, 0.5, 0), New Point3D(width, width, 0), New Point3D(width, 10 - width, 0), New Point3D(5, 10 - width, 0), _
                New Point3D(5, 10, 0), New Point3D(-5, 10, 0), New Point3D(-5, 10 - width, 0), New Point3D(-width, 10 - width, 0), New Point3D(-width, width, 0), New Point3D(-5, width, 0), _
                New Point3D(-5, 0, 0)}
            
            Dim profileLp As LinearPath = new LinearPath(profilePoints)
            Dim profile As Region = new Region(profileLp)

            Dim length1 As Double = 80

            Dim m As MyHybridMesh = profile.ExtrudeAsMesh(Of MyHybridMesh)(New Vector3D(0, 0, length1), 0.1, Mesh.natureType.Plain)
            m.Rotate(Math.PI / 2, Vector3D.AxisZ)
            m.Translate(5, 0, 0)
            m.wireVertices = BuildWire(length1).ToArray()
            vp.Entities.Add(m, System.Drawing.Color.Red)

            Dim m2 As MyHybridMesh = DirectCast(m.Clone(), MyHybridMesh)
            m2.Rotate(Math.PI, Vector3D.AxisZ)
            m2.Translate(60, 0, 0)
            vp.Entities.Add(m2, System.Drawing.Color.Red)

            Dim length2 As Double = 60
            Dim m3 As MyHybridMesh = profile.ExtrudeAsMesh(Of MyHybridMesh)(New Vector3D(0, 0, length2), 0.1, Mesh.natureType.Plain)
            m3.Rotate(Math.PI / 2, Vector3D.AxisZ)
            m3.Translate(5, 0, 0)
            m3.wireVertices = BuildWire(length2).ToArray()
            m3.Rotate(Math.PI / 2, Vector3D.AxisY)
            m3.Translate(0, 0, length1)
            vp.Entities.Add(m3, System.Drawing.Color.Green)

        End Sub

        Private Shared Function BuildWire(length As Double) As List(Of Point3D)
            Dim wires As New List(Of Point3D)()

            Dim p1 As New Point3D(-20, 0, 0)
            Dim p2 As Point3D = Point3D.Origin

            wires.Add(New Point3D(0, 0, 0))
            wires.Add(New Point3D(0, 0, length))

            wires.Add(p1)
            wires.Add(p2)

            Dim numArrows As Integer = 10
            Dim [step] As Double = length / numArrows

            Dim ptArrow1 As New Point3D(-4, 0, -2)
            Dim ptArrow2 As New Point3D(-4, 0, 2)

            Dim dir As Vector3D = Vector3D.AxisZ

            For i As Integer = 0 To numArrows
                Dim offset As Point3D = dir * [step] * i
                Dim newPos As Point3D = p1 + offset
                Dim newPos2 As Point3D = p2 + offset

                wires.Add(newPos)
                wires.Add(DirectCast(newPos2.Clone(), Point3D))
                wires.Add(DirectCast(newPos2.Clone(), Point3D))
                wires.Add(newPos2 + ptArrow1)
                wires.Add(DirectCast(newPos2.Clone(), Point3D))
                wires.Add(newPos2 + ptArrow2)
            Next

            Return wires
        End Function

        Private wire As Boolean

        Private Sub ChangeNatureButton_OnClick(sender As Object, e As RoutedEventArgs)
            ChangeNature()
        End Sub

        Private Sub ChangeNature()
            For i As Integer = 0 To model1.Entities.Count - 1
                Dim ent As Entity = model1.Entities(i)
                If TypeOf ent Is MyHybridMesh Then
                    Dim mcm As MyHybridMesh = DirectCast(ent, MyHybridMesh)
                    mcm.ChangeNature(If(wire, entityNatureType.Polygon, entityNatureType.Wire))
                    ' to update the values inside the entity
                    ent.UpdateBoundingBox(New TraversalParams(Nothing, model1))
                End If
            Next

            model1.Entities.Regen()

            model1.Entities.UpdateBoundingBox()
            wire = Not wire
            model1.Invalidate()
        End Sub

        Private resetActionMode As Boolean = True
        Private Sub SelectVisibleByPickButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectVisibleByPickButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectVisibleByPick
                resetActionMode = False

                'selectVisibleByPickButton.IsChecked = false;
                selectVisibleByBoxButton.IsChecked = False
                selectByPickButton.IsChecked = False
                selectByBoxButton.IsChecked = False
                selectByBoxEnclButton.IsChecked = False
                resetActionMode = True
            ElseIf resetActionMode Then
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub selectVisibleByBoxButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectVisibleByBoxButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectVisibleByBox
                resetActionMode = False

                selectVisibleByPickButton.IsChecked = False
                ' selectVisibleByBoxButton.IsChecked = false;
                selectByPickButton.IsChecked = False
                selectByBoxButton.IsChecked = False
                selectByBoxEnclButton.IsChecked = False

                resetActionMode = True
            ElseIf resetActionMode Then
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub selectByPickButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectByPickButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectByPick
                resetActionMode = False

                selectVisibleByPickButton.IsChecked = False
                selectVisibleByBoxButton.IsChecked = False
                ' selectByPickButton.IsChecked = false;
                selectByBoxButton.IsChecked = False
                selectByBoxEnclButton.IsChecked = False

                resetActionMode = True
            ElseIf resetActionMode Then
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub selectByBoxButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectByBoxButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectByBox
                resetActionMode = False

                selectVisibleByPickButton.IsChecked = False
                selectVisibleByBoxButton.IsChecked = False
                selectByPickButton.IsChecked = False
                ' selectByBoxButton.IsChecked = false;
                selectByBoxEnclButton.IsChecked = False

                resetActionMode = True
            ElseIf resetActionMode Then
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub selectByBoxEnclButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectByBoxEnclButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectByBoxEnclosed
                resetActionMode = False

                selectVisibleByPickButton.IsChecked = False
                selectVisibleByBoxButton.IsChecked = False
                selectByPickButton.IsChecked = False
                selectByBoxButton.IsChecked = False
                ' selectByBoxEnclButton.IsChecked = false;

                resetActionMode = True
            ElseIf resetActionMode Then
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub clearSelectionButton_OnClick(sender As Object, e As RoutedEventArgs)
            model1.Entities.ClearSelection()
            model1.Invalidate()
        End Sub

        Private Sub invertSelectionButton_OnClick(sender As Object, e As RoutedEventArgs)
            model1.Entities.InvertSelection()
            model1.Invalidate()
        End Sub

        Public Class MyHybridMesh
            Inherits Mesh
            Public wireVertices As Point3D()

            Private wireGraphicsData As new EntityGraphicsData()

            Public Sub New()
                MyBase.New()
            End Sub

            Public Sub New(another As MyHybridMesh)
                MyBase.New(another)
                wireVertices = New Point3D(another.wireVertices.Length - 1) {}
                For i As Integer = 0 To wireVertices.Length - 1
                    wireVertices(i) = DirectCast(another.wireVertices(i).Clone(), Point3D)
                Next
            End Sub

            Public Sub ChangeNature(nature As entityNatureType)
                entityNature = nature
            End Sub

            Public Overrides Sub Compile(data As CompileParams)
                data.RenderContext.Compile(wireGraphicsData, Function(context, params)
                                                                             context.DrawLines(wireVertices)

                                                                         End Function, Nothing)

                MyBase.Compile(data)
            End Sub

            Public Overrides Sub Regen(data As RegenParams)
                Dim currNature As entityNatureType = entityNature

                entityNature = entityNatureType.Polygon
                ' so the regen of the mesh is done correctly
                MyBase.Regen(data)

                entityNature = currNature
            End Sub

            Public Overrides Sub Dispose()
                wireGraphicsData.Dispose()
                MyBase.Dispose()
            End Sub

            Protected Overrides Sub DrawForShadow(renderParams As RenderParams)
                If entityNature <> entityNatureType.Wire Then
                    MyBase.DrawForShadow(renderParams)
                End If
            End Sub

            Protected Overrides Sub Draw(data As DrawParams)
                If entityNature = entityNatureType.Wire Then
                    data.RenderContext.Draw(wireGraphicsData)
                Else
                    MyBase.Draw(data)
                End If
            End Sub


            Protected Overrides Sub Render(data As RenderParams)
                If entityNature = entityNatureType.Wire Then
                    data.RenderContext.Draw(wireGraphicsData)
                Else
                    MyBase.Render(data)
                End If
            End Sub

            Protected Overrides Sub DrawForSelection(data As GfxDrawForSelectionParams)
                If entityNature = entityNatureType.Wire Then
                    data.RenderContext.Draw(wireGraphicsData)
                Else
                    MyBase.DrawForSelection(data)
                End If
            End Sub

            Protected Overrides Sub DrawEdges(data As DrawParams)
                If entityNature <> entityNatureType.Wire Then
                    MyBase.DrawEdges(data)
                End If
            End Sub

            Protected Overrides Sub DrawIsocurves(data As DrawParams)
                If entityNature <> entityNatureType.Wire Then
                    MyBase.DrawIsocurves(data)
                End If
            End Sub

            Protected Overrides Sub DrawHiddenLines(data As DrawParams)
                If entityNature = entityNatureType.Wire Then
                    Draw(data)
                Else
                    MyBase.DrawHiddenLines(data)
                End If
            End Sub

            Protected Overrides Sub DrawNormals(data As DrawParams)
                If entityNature <> entityNatureType.Wire Then
                    MyBase.DrawNormals(data)
                End If
            End Sub

            Protected Overrides Sub DrawSilhouettes(drawSilhouettesParams As DrawSilhouettesParams)
                If entityNature <> entityNatureType.Wire Then
                    MyBase.DrawSilhouettes(drawSilhouettesParams)
                End If
            End Sub


            Protected Overrides Sub DrawWireframe(drawParams As DrawParams)
                If entityNature = entityNatureType.Wire Then
                    Draw(drawParams)
                Else

                    MyBase.DrawWireframe(drawParams)
                End If
            End Sub

            Protected Overrides Sub DrawSelected(drawParams As DrawParams)
                If entityNature = entityNatureType.Wire Then
                    drawParams.RenderContext.Draw(wireGraphicsData)
                Else
                    MyBase.DrawSelected(drawParams)
                End If
            End Sub

            Protected Overrides Sub DrawVertices(drawParams As DrawParams)
                If entityNature = entityNatureType.Wire Then
                    drawParams.RenderContext.DrawPoints(wireVertices)
                Else
                    MyBase.DrawVertices(drawParams)
                End If
            End Sub

            Protected Overrides Function InsideOrCrossingFrustum(ByVal data As FrustumParams) As Boolean
                If entityNature = entityNatureType.Wire Then
                    Dim transform = data.Transformation
                    If transform Is Nothing Then transform = New Identity()
                    For i As Integer = 0 To wireVertices.Length - 1 Step 2
                        If Utility.IsSegmentInsideOrCrossing(data.Frustum, New Segment3D(transform * wireVertices(i), transform * wireVertices(i + 1))) Then Return True
                    Next

                    Return False
                End If

                Return MyBase.InsideOrCrossingFrustum(data)
            End Function

            Protected Overrides Function ThroughTriangle(ByVal data As FrustumParams) As Boolean
                If entityNature = entityNatureType.Wire Then Return False
                    Return MyBase.ThroughTriangle(data)
            End Function

            Public Overrides Sub TransformBy(xform As Transformation)
                If wireVertices IsNot Nothing Then
                    For Each s As Point3D In wireVertices

                        s.TransformBy(xform)
                    Next
                End If

                MyBase.TransformBy(xform)
            End Sub

            Public Overrides Function Clone() As Object
                Return New MyHybridMesh(Me)
            End Function

            Protected Overrides Function AllVerticesInFrustum(data As FrustumParams) As Boolean
                If entityNature = entityNatureType.Wire Then
                    Return UtilityEx.AllVerticesInFrustum(data, wireVertices, wireVertices.Length)
                End If

                Return MyBase.AllVerticesInFrustum(data)
            End Function


            Protected Overrides Function ComputeBoundingBox(data As TraversalParams, ByRef boxMin As Point3D, ByRef boxMax As Point3D) As Boolean
                If entityNature = entityNatureType.Wire Then
                    UtilityEx.ComputeBoundingBox(data.Transformation, wireVertices, boxMin, boxMax)
                Else
                    MyBase.ComputeBoundingBox(data, boxMin, boxMax)
                End If

                Return True
            End Function
        End Class
    End Class
End Namespace
