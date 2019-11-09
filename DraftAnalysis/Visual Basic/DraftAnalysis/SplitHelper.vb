Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Geometry
Imports System.Drawing
Imports devDept.Eyeshot.Entities

Public Class SplitHelper
    Private ReadOnly _angularTol As Double = Utility.DegToRad(7)
    Private ReadOnly _smoothingAngle As Double = Utility.DegToRad(10)      ' used to determine the final sides of the draft.
    Private ReadOnly _blue As Block, _red As Block, _yellow As Block
    Private ReadOnly _model1 As Model
    Public Direction As Vector3D
    Public ArrowIndex As Integer

    Public Sub New(model1 As Model)
        _blue = New Block("BlueBlock")
        _red = New Block("RedBlock")
        _yellow = New Block("yellowBlock")
        direction = Vector3D.AxisY * -1
        arrowIndex = -1
        _model1 = model1
    End Sub

    ''' <summary>
    ''' Splits original Entity in three sections (red, blue, yellow) using direction vector.
    ''' </summary>
    Public Sub QuickSplit(originalEntity As Mesh, direction As Vector3D)
        Dim redT As Integer = 0
        Dim blueT As Integer = 0
        Dim yellowT As Integer = 0

        Dim redSection As Mesh = DirectCast(originalEntity.Clone(), Mesh)
        Dim blueSection As Mesh = DirectCast(originalEntity.Clone(), Mesh)
        Dim yellowSection As Mesh = DirectCast(originalEntity.Clone(), Mesh)
        blueSection.Visible = True
        redSection.Visible = True
        yellowSection.Visible = True

        Dim redPoints As Integer() = New Integer(originalEntity.Vertices.Length - 1) {}
        Dim yellowPoints As Integer() = New Integer(originalEntity.Vertices.Length - 1) {}
        Dim bluePoints As Integer() = New Integer(originalEntity.Vertices.Length - 1) {}

        ' gets graph of originalEntity's edges
        Dim sell As LinkedList(Of SharedEdge)()
        Dim res As Integer = Utility.GetEdgesWithoutDuplicates(originalEntity.Triangles, originalEntity.Vertices.Count(), sell)

        'convert original IndexTriangles to MyIndexTriangle
        Dim originalT As MyIndexTriangle() = New MyIndexTriangle(originalEntity.Triangles.Length - 1) {}
        For i As Integer = 0 To originalEntity.Triangles.Length - 1
            originalT(i) = New MyIndexTriangle(originalEntity.Triangles(i).V1, originalEntity.Triangles(i).V2, originalEntity.Triangles(i).V3, False, 0, originalEntity.Vertices)
        Next

        ' gets a first list of triangles
        SplitTrianglesByNormal(originalEntity, originalT, direction, redSection, yellowSection, blueSection, redPoints, yellowPoints, bluePoints, redT, yellowT, blueT)
        
        ' checks yellow triangles
        ReassingYellowTriangles(originalEntity, sell, originalT, direction, redSection, yellowSection, blueSection, redPoints, yellowPoints, bluePoints, redT, yellowT, blueT)

        ' updates triangle section arrays
        Dim blue As IndexTriangle() = New IndexTriangle(blueT - 1) {}
        Dim red As IndexTriangle() = New IndexTriangle(redT - 1) {}
        Dim yellow As IndexTriangle() = New IndexTriangle(yellowT - 1) {}
        Dim redDestCount As Integer = 0
        Dim blueDestCount As Integer = 0
        Dim yellowDestCount As Integer = 0
        For i As Integer = 0 To originalEntity.Triangles.Length - 1
            If redSection.Triangles(i) IsNot Nothing Then
                red(redDestCount) = DirectCast(redSection.Triangles(i).Clone(), IndexTriangle)
                redDestCount += 1
            End If
            If blueSection.Triangles(i) IsNot Nothing Then
                blue(blueDestCount) = DirectCast(blueSection.Triangles(i).Clone(), IndexTriangle)
                blueDestCount += 1
            End If
            If yellowSection.Triangles(i) IsNot Nothing Then
                yellow(yellowDestCount) = DirectCast(yellowSection.Triangles(i).Clone(), IndexTriangle)
                yellowDestCount += 1
            End If
        Next
        redSection.Triangles = red
        blueSection.Triangles = blue
        yellowSection.Triangles = yellow

        'Deletes and reorders Vertices lists
        yellowSection = DeleteUnusedVertices(yellowSection)
        redSection = DeleteUnusedVertices(redSection)
        blueSection = DeleteUnusedVertices(blueSection)

        SetBlockDefinition(blueSection, redSection, yellowSection)
        DrawNormalDirection(Point3D.Origin, originalEntity.BoxSize.Diagonal)

        _model1.Entities.Regen()
        _model1.Invalidate()
    End Sub

    ''' <summary>
    ''' Splits yellow triangles in red/blue section considering the smoothingAngle,
    ''' </summary>
    Private Sub ReassingYellowTriangles(ByVal originalEntity As Mesh, ByVal sell As LinkedList(Of SharedEdge)(), ByVal originalT As MyIndexTriangle(), ByVal direction As Vector3D, ByVal redSection As Mesh, ByVal yellowSection As Mesh, ByVal blueSection As Mesh, ByVal redPoints As Integer(), ByVal yellowPoints As Integer(), ByVal bluePoints As Integer(), ByRef redT As Integer, ByRef yellowT As Integer, ByRef blueT As Integer)
        Dim result As Integer = 0

        For i As Integer = 0 To originalT.Length - 1
            If yellowSection.Triangles(i) IsNot Nothing Then
                'if is a perfect vertical triangle from direction must be yellow
                Dim angle As Double = Vector3D.AngleBetween(direction, originalT(i).Normal)
                If angle <> Math.PI / 2 Then

                    Dim it As IndexTriangle = yellowSection.Triangles(i)
                    ' gets group of triangle i considering his SharedEdges
                    originalT(i).Group = InlineAssignHelper(result, GetFinalDraftTriangles(originalT, sell, i, originalEntity.Vertices))

                    If result > 0 Then
                        'Triangle move from yellow group to red group
                        redT += 1
                        yellowT -= 1
                        redSection.Triangles(i) = DirectCast(it.Clone(), IndexTriangle)
                        blueSection.Triangles(i) = Nothing
                        yellowSection.Triangles(i) = Nothing
                        
                        redPoints(it.V3) = InlineAssignHelper(redPoints(it.V2), InlineAssignHelper(redPoints(it.V1), 1))

                        yellowPoints(it.V1) = InlineAssignHelper(yellowPoints(it.V2), InlineAssignHelper(yellowPoints(it.V3), 0))
                    ElseIf result < 0 Then
                        'Triangle move from yellow group to blue group
                        blueT += 1
                        yellowT -= 1
                        blueSection.Triangles(i) = DirectCast(it.Clone(), IndexTriangle)
                        redSection.Triangles(i) = Nothing
                        yellowSection.Triangles(i) = Nothing
                        
                        bluePoints(it.V3) = InlineAssignHelper(bluePoints(it.V2), InlineAssignHelper(bluePoints(it.V1), 1))

                        yellowPoints(it.V1) = InlineAssignHelper(yellowPoints(it.V2), InlineAssignHelper(yellowPoints(it.V3), 0))
                    End If

                    ' Checks if some Triangles was near to this(ReVisit = true) and sets to the same group
                    For j As Integer = 0 To originalT.Length - 1
                        If originalT(j).Group = 0 AndAlso originalT(j).ReVisit Then
                            If originalT(i).Group <> 0 Then
                                originalT(j).Group = originalT(i).Group
                                originalT(j).ReVisit = False
                            End If
                        End If
                    Next
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Splits the triangles and Vertices in red/yellow/blue sections considering angularTol.
    ''' </summary>
    Private Sub SplitTrianglesByNormal(ByVal originalEntity As Mesh, ByVal originalT As MyIndexTriangle(), ByVal direction As Vector3D, ByVal redSection As Mesh, ByVal yellowSection As Mesh, ByVal blueSection As Mesh, ByVal redPoints As Integer(), ByVal yellowPoints As Integer(), ByVal bluePoints As Integer(), ByRef redT As Integer, ByRef yellowT As Integer, ByRef blueT As Integer)

        For i As Integer = 0 To originalT.Length - 1
            Dim it As MyIndexTriangle = originalT(i)
            Dim t As New Triangle(originalEntity.Vertices(it.V1), originalEntity.Vertices(it.V2), originalEntity.Vertices(it.V3))
            t.Regen(0.1)
            Dim angle As Double = Vector3D.AngleBetween(direction, t.Normal)

            ' red section
            If Math.Abs(angle) < (Math.PI / 2 - _angularTol) Then
                originalT(i).Found = True
                originalT(i).Visited = True
                'sets to yellow group
                originalT(i).Group = 1
                ' if is yellow isn't blue/red
                blueSection.Triangles(i) = Nothing
                yellowSection.Triangles(i) = Nothing
                'found a new red triangle
                redT += 1

                redPoints(it.V3) = InlineAssignHelper(redPoints(it.V2), InlineAssignHelper(redPoints(it.V1), 1))
                ' yellow section
            ElseIf Math.Abs(angle) >= (Math.PI / 2 - _angularTol) AndAlso Math.Abs(angle) <= (Math.PI / 2 + _angularTol) Then
                'sets to yellow group
                originalT(i).Group = 0
                ' if is yellow isn't blue/red
                blueSection.Triangles(i) = Nothing
                redSection.Triangles(i) = Nothing
                'found a new yellow triangle
                yellowT += 1
                
                yellowPoints(it.V3) = InlineAssignHelper(yellowPoints(it.V2), InlineAssignHelper(yellowPoints(it.V1), 1))
            Else
                ' blue section
                originalT(i).Found = True
                'originalT[i].Visited = true;  
                'sets to blue group
                originalT(i).Group = -1
                ' if is blue isn't red/yellow
                redSection.Triangles(i) = Nothing
                yellowSection.Triangles(i) = Nothing
                'found a new blue triangle
                blueT += 1
                
                bluePoints(it.V3) = InlineAssignHelper(bluePoints(it.V2), InlineAssignHelper(bluePoints(it.V1), 1))
            End If
        Next
    End Sub

    ''' <summary>
    ''' Returns the correct group of triangle indexT considering neigthbor triangles groups of indexV vertex that fall in smoothingAngle.
    ''' </summary>
    Private Function CheckTriangles(its As MyIndexTriangle(), indexT As Integer, sel As LinkedList(Of SharedEdge)(), indexV As Integer, vertices As Point3D()) As Integer
        Dim se As LinkedListNode(Of SharedEdge) = sel(indexV).First

        While se IsNot Nothing
            If se.Value.Dad <> indexT Then
                Dim angle2 As Double = Vector3D.AngleBetween(its(indexT).Normal, its(se.Value.Dad).Normal)
                If Math.Abs(angle2) < _smoothingAngle Then
                    ' if Dad triangle is not yellow, indexT must has dad's group
                    If its(se.Value.Dad).Group <> 0 Then
                        Return its(se.Value.Dad).Group
                    ElseIf Not its(se.Value.Dad).Found AndAlso Not its(se.Value.Dad).Visited Then
                        its(se.Value.Dad).Group = GetFinalDraftTriangles(its, sel, se.Value.Dad, vertices)
                        If its(se.Value.Dad).Group <> 0 Then
                            Return its(se.Value.Dad).Group
                        End If
                        ' if Dad is visiting, current Triangle need to be revisit after Dad 
                    ElseIf its(se.Value.Dad).Found AndAlso Not its(se.Value.Dad).Visited Then
                        its(indexT).ReVisit = True
                    End If
                End If
            End If
            If se.Value.Mum <> indexT Then
                Dim angle2 As Double = Vector3D.AngleBetween(its(indexT).Normal, its(se.Value.Mum).Normal)
                If Math.Abs(angle2) < _smoothingAngle Then
                    ' if mum triangle is not yellow, it must be mum's group
                    If its(se.Value.Mum).Group <> 0 Then
                        Return its(se.Value.Mum).Group
                    ElseIf Not its(se.Value.Mum).Found AndAlso Not its(se.Value.Mum).Visited Then
                        its(se.Value.Mum).Group = GetFinalDraftTriangles(its, sel, se.Value.Mum, vertices)
                        If its(se.Value.Mum).Group <> 0 Then
                            Return its(se.Value.Mum).Group
                        End If
                        ' if Mum's visiting, current Triangle need to be revisit after Mum 
                    ElseIf (its(se.Value.Mum).Found AndAlso Not its(se.Value.Mum).Visited) Then
                        its(indexT).ReVisit = True
                    End If
                End If
            End If
            se = se.[Next]
        End While

        ' remains in yellow group
        Return 0
    End Function

    ''' <summary>
    ''' Returns the correct group of indexT triangle considering his neighbours triangles groups.
    ''' </summary>
    Private Function GetFinalDraftTriangles(its As MyIndexTriangle(), sel As LinkedList(Of SharedEdge)(), indexT As Integer, vertices As Point3D()) As Integer
        ' if visited its[indexT].Group already setted.
        If its(indexT).Found Then
            Return its(indexT).Group
        End If
        its(indexT).Found = True

        'check neightbors Triangles of first vertex
        Dim res As Integer = CheckTriangles(its, indexT, sel, its(indexT).V1, vertices)
        If res <> 0 Then
            Return res
        End If

        'check neightbors Triangles of second vertex
        res = CheckTriangles(its, indexT, sel, its(indexT).V2, vertices)
        If res <> 0 Then
            Return res
        End If

        'check neightbors Triangles of third vertex
        res = CheckTriangles(its, indexT, sel, its(indexT).V3, vertices)

        its(indexT).Visited = True
        Return res
    End Function

    ''' <summary>
    ''' Returns a Mesh with unused vertices deleted and vertices references reordered in Triangles list of Mesh m.
    ''' </summary>
    Protected Function DeleteUnusedVertices(m As Mesh) As Mesh
        Dim newV As Integer() = New Integer(m.Vertices.Length - 1) {}
        Dim count As Integer = 1

        For Each it As IndexTriangle In m.Triangles
            If newV(it.V1) = 0 Then
                newV(it.V1) = count
                count += 1
            End If
            it.V1 = newV(it.V1) - 1

            If newV(it.V2) = 0 Then
                newV(it.V2) = count
                count += 1
            End If
            it.V2 = newV(it.V2) - 1

            If newV(it.V3) = 0 Then
                newV(it.V3) = count
                count += 1
            End If
            it.V3 = newV(it.V3) - 1
        Next

        Dim finalV As Point3D() = New Point3D(count - 2) {}
        For i As Integer = 0 To m.Vertices.Length - 1
            If newV(i) <> 0 Then
                finalV(newV(i) - 1) = m.Vertices(i)
            End If
        Next
        m.Vertices = finalV
        Return m
    End Function

    ''' <summary>
    ''' Add normal's direction in Model like an arrow.
    ''' </summary>
    Public Sub DrawNormalDirection(startPt As Point3D, size As Double)
        ' creates arrow that shows split direction choosen
        Dim newArrow As Mesh = Mesh.CreateArrow(0.1, 5, 0.5, 1, 10, Mesh.natureType.Plain)
        newArrow.Rotate(-direction.AngleFromXY, Vector3D.AxisY)
        newArrow.Rotate(direction.AngleInXY, Vector3D.AxisZ)

        newArrow.Scale(size / 50, size / 50, size / 50)
        newArrow.Translate(New Vector3D(startPt.X, startPt.Y, startPt.Z))
        newArrow.Color = Color.Magenta
        newArrow.ColorMethod = colorMethodType.byEntity

        If arrowIndex <> -1 Then
            ' updates previous arrow
            '_model1.Entities[arrowIndex].Color = Color.Magenta;
            _model1.Entities(arrowIndex) = newArrow
        Else
            _model1.Entities.Add(newArrow, Color.Magenta)
            arrowIndex = _model1.Entities.IndexOf(newArrow)
        End If

        _model1.Entities.ClearSelection()
        _model1.Entities.Regen()
        _model1.Invalidate()
    End Sub

    ''' <summary>
    ''' Add groups red, blue, yellow in Model using BlockReference.
    ''' </summary>
    Private Sub SetBlockDefinition(blueMesh As Mesh, redMesh As Mesh, yellowMesh As Mesh)
        ' removes old splitted sections
        If _model1.Blocks.Count > 0 Then
            _model1.Blocks.Remove("BlueBlock")
            _model1.Blocks.Remove("RedBlock")
            _model1.Blocks.Remove("yellowBlock")

            _blue.Entities.RemoveAt(0)
            _red.Entities.RemoveAt(0)
            _yellow.Entities.RemoveAt(0)
        End If

        ' creates block definitions of new sections
        blueMesh.Color = Color.SkyBlue
        blueMesh.Selectable = False
        _blue.Entities.Add(blueMesh)

        redMesh.Color = Color.Salmon
        redMesh.Selectable = False
        _red.Entities.Add(redMesh)

        yellowMesh.Color = Color.Wheat
        yellowMesh.Selectable = False
        _yellow.Entities.Add(yellowMesh)

        _model1.Blocks.Add(_blue)
        _model1.Blocks.Add(_red)
        _model1.Blocks.Add(_yellow)

        ' create block references and adds them to model 
        _model1.Entities.Add(New BlockReference(0, 0, 0, "BlueBlock", 1, 1, _
            1, 0))
        _model1.Entities.Add(New BlockReference(0, 0, 0, "RedBlock", 1, 1, _
            1, 0))
        _model1.Entities.Add(New BlockReference(0, 0, 0, "yellowBlock", 1, 1, _
            1, 0))
    End Sub

    ''' <summary>
    ''' Translates blue and red BlockReferences od offset.
    ''' </summary>
    Public Sub TranslatingSections(offset As Double)
        For Each ent As Entity In _model1.Entities
            If TypeOf ent Is BlockReference Then
                ' traslates red section up
                If DirectCast(ent, BlockReference).BlockName.CompareTo("RedBlock") = 0 Then
                    DirectCast(ent, BlockReference).Translate(direction * offset)
                End If

                ' traslates blue section down
                If DirectCast(ent, BlockReference).BlockName.CompareTo("BlueBlock") = 0 Then
                    DirectCast(ent, BlockReference).Translate(direction * -offset)
                End If

                ent.RegenMode = regenType.RegenAndCompile
            End If
        Next

        _model1.Entities.Regen()
        _model1.Invalidate()
    End Sub
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class

Class MyIndexTriangle
    Inherits IndexTriangle
    Public Found As Boolean
    ' found, but Group value could not be corrected yet
    Public ReVisit As Boolean
    ' needs to be revisited
    Public Visited As Boolean
    ' visit completed, Group value is correct
    Public Group As Integer
    ' blue = -1, yellow = 0, red = 1
    Public Normal As Vector3D

    Public Sub New(v1__1 As Integer, v2__2 As Integer, v3__3 As Integer, visited__4 As Boolean, group__5 As Integer, vertices As Point3D())
        MyBase.New(v1__1, v2__2, v3__3)
        Visited = visited__4
        Group = group__5
        Found = False
        ReVisit = False

        Dim t As New Triangle(vertices(V1), vertices(V2), vertices(V3))
        t.Regen(0.1)
        Normal = t.Normal
    End Sub
End Class
