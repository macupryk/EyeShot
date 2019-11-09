Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports System.Collections
Imports System.Linq
Imports System.Threading
Imports devDept.Eyeshot.Translators
Imports System.Windows.Controls
Imports Microsoft.Win32

Namespace WpfApplication1

    Partial Public Class MainWindow

        Private Shared ReadOnly NOT_MODIFIED_COLOR As Color = Color.FromArgb(44, 44, 44)

        Public Sub New()
            InitializeComponent()

            'model1.Unlock("") 'For more details see 'Product Activation' topic in the documentation.
            'model2.Unlock("") 'For more details see 'Product Activation' topic in the documentation.

            model1.Rotate.Enabled = False
            model2.Rotate.Enabled = False

            model1.GetViewCubeIcon().Visible = False
            model2.GetViewCubeIcon().Visible = False

            model1.Camera.ProjectionMode = projectionType.Orthographic
            model2.Camera.ProjectionMode = projectionType.Orthographic
        End Sub

        Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)
            'Sets the view as Top
            model1.SetView(viewType.Top)
            model2.SetView(viewType.Top)
            'Fits the model in the viewport
            model1.ZoomFit()
            model2.ZoomFit()

            model1.Invalidate()
            model2.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub beforeButton_Click(sender As Object, e As EventArgs)
            ColorCompareAndMark(model1, beforePathLabel, model2)
        End Sub

        Private Sub afterButton_Click(sender As Object, e As EventArgs)
            ColorCompareAndMark(model2, afterPathLabel, model1)
        End Sub

        Private Sub ColorCompareAndMark(ByVal modelForFile As Model, ByVal pathLabel As Label, ByVal modelToColor As Model)
            OpenFile(modelForFile, pathLabel)
            ColorEntities(modelToColor.Entities)

            If model1.Entities.Count > 0 AndAlso model2.Entities.Count > 0 Then

                CompareAndMark(model1.Entities, model2.Entities)

                model1.ZoomFit()
                model2.ZoomFit()

                model1.Invalidate()
                model2.Invalidate()
            End If
        End Sub

        Public Function AreEqual(ByVal ent1 As Entity, ByVal ent2 As Entity) As Boolean
            If TypeOf ent1 Is CompositeCurve Then
                Dim cc1 As CompositeCurve = CType(ent1, CompositeCurve)
                Dim cc2 As CompositeCurve = CType(ent2, CompositeCurve)
                If cc1.CurveList.Count = cc2.CurveList.Count Then
                    Dim equalCurvesInListCount As Integer = 0
                    For Each entC As Entity In cc1.CurveList
                        For Each entC2 As Entity In cc2.CurveList
                            If entC.[GetType]() = entC2.[GetType]() Then
                                If CompareIfEqual(entC, entC2) Then
                                    equalCurvesInListCount += 1
                                    Exit For
                                End If
                            End If
                        Next
                    Next

                    If cc1.CurveList.Count = equalCurvesInListCount Then
                        Return True
                    End If
                End If
            ElseIf TypeOf ent1 Is LinearPath Then
                Dim lp1 As LinearPath = CType(ent1, LinearPath)
                Dim lp2 As LinearPath = CType(ent2, LinearPath)
                If lp1.Vertices.Length = lp2.Vertices.Length Then
                    For i As Integer = 0 To lp1.Vertices.Length - 1
                        If Not (lp1.Vertices(i) = lp2.Vertices(i)) Then Return False
                    Next

                    Return True
                End If
            ElseIf TypeOf ent1 Is PlanarEntity Then
                Dim pe1 As PlanarEntity = CType(ent1, PlanarEntity)
                Dim pe2 As PlanarEntity = CType(ent2, PlanarEntity)
                If pe1.Plane.AxisZ = pe2.Plane.AxisZ AndAlso pe1.Plane.AxisX = pe2.Plane.AxisX Then
                    If TypeOf ent1 Is Arc Then
                        Dim arc1 As Arc = CType(ent1, Arc)
                        Dim arc2 As Arc = CType(ent2, Arc)
                        If arc1.Center = arc2.Center AndAlso arc1.Radius = arc2.Radius AndAlso arc1.Domain.Min = arc2.Domain.Min AndAlso arc1.Domain.Max = arc2.Domain.Max Then
                            Return True
                        End If
                    ElseIf TypeOf ent1 Is Circle Then
                        Dim c1 As Circle = CType(ent1, Circle)
                        Dim c2 As Circle = CType(ent2, Circle)
                        If c1.Center = c2.Center AndAlso c1.Radius = c2.Radius Then
                            Return True
                        End If
                    ElseIf TypeOf ent1 Is EllipticalArc Then
                        Dim e1 As EllipticalArc = CType(ent1, EllipticalArc)
                        Dim e2 As EllipticalArc = CType(ent2, EllipticalArc)
                        If e1.Center = e2.Center AndAlso e1.RadiusX = e2.RadiusX AndAlso e1.RadiusY = e2.RadiusY AndAlso e1.Domain.Low = e2.Domain.Low AndAlso e1.Domain.High = e2.Domain.High Then
                            Return True
                        End If
                    ElseIf TypeOf ent1 Is Ellipse Then
                        Dim e1 As Ellipse = CType(ent1, Ellipse)
                        Dim e2 As Ellipse = CType(ent2, Ellipse)
                        If e1.Center = e2.Center AndAlso e1.RadiusX = e2.RadiusX AndAlso e1.RadiusY = e2.RadiusY Then
                            Return True
                        End If
                    ElseIf TypeOf ent1 Is Text Then
                        If TypeOf ent1 Is Dimension Then
                            Dim dim1 As Dimension = CType(ent1, Dimension)
                            Dim dim2 As Dimension = CType(ent2, Dimension)
                            If dim1.InsertionPoint = dim2.InsertionPoint AndAlso dim1.DimLinePosition = dim2.DimLinePosition Then
                                If TypeOf ent1 Is AngularDim Then
                                    Dim ad1 As AngularDim = CType(ent1, AngularDim)
                                    Dim ad2 As AngularDim = CType(ent2, AngularDim)
                                    If ad1.ExtLine1 = ad2.ExtLine1 AndAlso ad1.ExtLine2 = ad2.ExtLine2 AndAlso ad1.StartAngle = ad2.StartAngle AndAlso ad1.EndAngle = ad2.EndAngle AndAlso ad1.Radius = ad2.Radius Then
                                        Return True
                                    End If
                                ElseIf TypeOf ent1 Is LinearDim Then
                                    Dim ld1 As LinearDim = CType(ent1, LinearDim)
                                    Dim ld2 As LinearDim = CType(ent2, LinearDim)
                                    If ld1.ExtLine1 = ld2.ExtLine1 AndAlso ld1.ExtLine2 = ld2.ExtLine2 Then
                                        Return True
                                    End If
                                ElseIf TypeOf ent1 Is DiametricDim Then
                                    Dim dd1 As DiametricDim = CType(ent1, DiametricDim)
                                    Dim dd2 As DiametricDim = CType(ent2, DiametricDim)
                                    If dd1.Distance = dd2.Distance AndAlso dd1.Radius = dd2.Radius AndAlso dd1.CenterMarkSize = dd2.CenterMarkSize Then
                                        Return True
                                    End If
                                ElseIf TypeOf ent1 Is RadialDim Then
                                    Dim rd1 As RadialDim = CType(ent1, RadialDim)
                                    Dim rd2 As RadialDim = CType(ent2, RadialDim)
                                    If rd1.Radius = rd2.Radius AndAlso rd1.CenterMarkSize = rd2.CenterMarkSize Then
                                        Return True
                                    End If
                                ElseIf TypeOf ent1 Is OrdinateDim Then
                                    Dim od1 As OrdinateDim = CType(ent1, OrdinateDim)
                                    Dim od2 As OrdinateDim = CType(ent2, OrdinateDim)
                                    If od1.DefiningPoint = od2.DefiningPoint AndAlso od1.Origin = od2.Origin AndAlso od1.LeaderEndPoint = od2.LeaderEndPoint Then
                                        Return True
                                    End If
                                Else
                                    Console.Write("Type " & ent1.[GetType]().ToString() & " not implemented.")
                                    Return True
                                End If
                            End If
                        ElseIf TypeOf ent1 Is devDept.Eyeshot.Entities.Attribute Then
                            Dim att1 As devDept.Eyeshot.Entities.Attribute = CType(ent1, devDept.Eyeshot.Entities.Attribute)
                            Dim att2 As devDept.Eyeshot.Entities.Attribute = CType(ent2, devDept.Eyeshot.Entities.Attribute)
                            If att1.Value = att2.Value AndAlso att1.InsertionPoint = att2.InsertionPoint Then
                                Return True
                            End If
                        Else
                            Dim tx1 As Text = CType(ent1, Text)
                            Dim tx2 As Text = CType(ent2, Text)
                            If tx1.InsertionPoint = tx2.InsertionPoint AndAlso tx1.TextString = tx2.TextString AndAlso tx1.StyleName = tx2.StyleName AndAlso tx1.WidthFactor = tx2.WidthFactor AndAlso tx1.Height = tx2.Height Then
                                Return True
                            End If
                        End If
                    Else
                        Console.Write("Type " & ent1.[GetType]().ToString() & " not implemented.")
                        Return True
                    End If
                End If
            ElseIf TypeOf ent1 Is Line Then
                Dim line1 As Line = CType(ent1, Line)
                Dim line2 As Line = CType(ent2, Line)
                If line1.StartPoint = line2.StartPoint AndAlso line1.EndPoint = line2.EndPoint Then
                    Return True
                End If
            ElseIf TypeOf ent1 Is devDept.Eyeshot.Entities.Point Then
                Dim point1 As devDept.Eyeshot.Entities.Point = CType(ent1, devDept.Eyeshot.Entities.Point)
                Dim point2 As devDept.Eyeshot.Entities.Point = CType(ent2, devDept.Eyeshot.Entities.Point)
                If point1.Position = point2.Position Then
                    Return True
                End If
#If NURBS Then
            ElseIf TypeOf ent1 Is Curve Then
                Dim cu1 As Curve = CType(ent1, Curve)
                Dim cu2 As Curve = CType(ent2, Curve)

                If cu1.ControlPoints.Length = cu2.ControlPoints.Length AndAlso cu1.KnotVector.Length = cu2.KnotVector.Length AndAlso cu1.Degree = cu2.Degree Then

                    For k As Integer = 0 To cu1.ControlPoints.Length - 1

                        If cu1.ControlPoints(k) <> cu2.ControlPoints(k) Then
                            Return False
                        End If
                    Next

                    For k As Integer = 0 To cu1.KnotVector.Length - 1

                        If cu1.KnotVector(k) <> cu2.KnotVector(k) Then
                            Return False
                        End If
                    Next

                    Return True
                End If
#End If
            Else
                Console.Write("Type " & ent1.[GetType]().ToString() & " not implemented.")
                Return True
            End If

            Return False
        End Function

        Public Function AreEqualAttributes(ByVal ent1 As Entity, ByVal ent2 As Entity) As Boolean
            Return ent1.LayerName = ent2.LayerName AndAlso ent1.GroupIndex = ent2.GroupIndex AndAlso ent1.ColorMethod = ent2.ColorMethod AndAlso ent1.Color = ent2.Color AndAlso ent1.LineWeightMethod = ent2.LineWeightMethod AndAlso ent1.LineWeight = ent2.LineWeight AndAlso ent1.LineTypeMethod = ent2.LineTypeMethod AndAlso ent1.LineTypeName = ent2.LineTypeName AndAlso ent1.LineTypeScale = ent2.LineTypeScale AndAlso ent1.MaterialName = ent2.MaterialName
        End Function


        Public Sub ColorEntities(ByVal list As EntityList)
            For Each ent As Entity In list
                ent.Color = NOT_MODIFIED_COLOR
                ent.ColorMethod = colorMethodType.byEntity
            Next
        End Sub

        Public Sub CompareAndMark(ByVal entList1 As IList(Of Entity), ByVal entList2 As IList(Of Entity))
            Dim equalEntitiesInV2 As Boolean() = New Boolean(entList2.Count - 1) {}

            For i As Integer = 0 To entList1.Count() - 1
                Dim entVp1 As Entity = entList1(i)
                Dim foundEqual As Boolean = False

                For j As Integer = 0 To entList2.Count() - 1
                    Dim entVp2 As Entity = entList2(j)

                    If Not equalEntitiesInV2(j) AndAlso entVp1.[GetType]() = entVp2.[GetType]() AndAlso CompareIfEqual(entVp1, entVp2) Then
                        equalEntitiesInV2(j) = True
                        foundEqual = True
                        Exit For
                    End If
                Next

                If Not foundEqual Then
                    entList1(i).Color = Color.Yellow
                    entList1(i).ColorMethod = colorMethodType.byEntity
                End If
            Next

            For j As Integer = 0 To entList2.Count - 1

                If Not equalEntitiesInV2(j) Then
                    entList2(j).Color = Color.Yellow
                    entList2(j).ColorMethod = colorMethodType.byEntity
                End If
            Next
        End Sub

        Public Function CompareIfEqual(ByVal entVp1 As Entity, ByVal entVp2 As Entity) As Boolean
            Dim areEqualAttributesValue As Boolean = AreEqualAttributes(entVp1, entVp2)
            Dim areEqualValue As Boolean = AreEqual(entVp1, entVp2)

            Return areEqualAttributesValue AndAlso areEqualValue
        End Function

        Public Sub OpenFile(ByVal model As Model, ByVal labelPath As Label)

            Dim openFileDialog1 As OpenFileDialog = New OpenFileDialog() With {
            .Filter = "DWG File|*.dwg",
            .Multiselect = False,
            .AddExtension = True,
            .CheckFileExists = True,
            .CheckPathExists = True
        }

            If openFileDialog1.ShowDialog().Value Then

                beforeButton.IsEnabled = False
                afterButton.IsEnabled = False
                labelPath.Content = "Loading . . ."

                'Force WPF refresh
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, New Action(Function()
                                                                                                                                        End Function))

                Dim rfa = New ReadAutodesk(openFileDialog1.FileName)
                rfa.DoWork()
                model.Clear()
                rfa.AddToScene(model, NOT_MODIFIED_COLOR)
                Dim toAdd As Entity() = model.Entities.Explode()
                model.Entities.AddRange(toAdd, NOT_MODIFIED_COLOR)

                beforeButton.IsEnabled = True
                afterButton.IsEnabled = True

                labelPath.Content = openFileDialog1.FileName

                model.SetView(viewType.Top)

                model.ZoomFit()
                model.Invalidate()
            End If
        End Sub

        Private Sub camera1_MoveEnd(ByVal sender As Object, ByVal e As EventArgs) Handles model1.CameraMoveEnd
            SyncCamera(model1, model2)
        End Sub

        Private Sub camera2_MoveEnd(ByVal sender As Object, ByVal e As EventArgs) Handles model2.CameraMoveEnd
            SyncCamera(model2, model1)
        End Sub

        Private Sub SyncCamera(ByVal modelMovedCamera As Model, ByVal modelCameraToMove As Model)
            Dim savedCamera As Camera = Nothing
            modelMovedCamera.SaveView(savedCamera)
            modelCameraToMove.RestoreView(savedCamera)
            modelCameraToMove.AdjustNearAndFarPlanes()
            modelCameraToMove.Invalidate()
        End Sub

    End Class
End Namespace
