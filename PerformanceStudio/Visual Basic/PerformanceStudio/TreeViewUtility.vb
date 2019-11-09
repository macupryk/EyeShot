Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities

Module TreeViewUtility
        Sub PopulateTree(ByVal tv As TreeView, ByVal entList As List(Of Entity), ByVal blocks As BlockKeyedCollection, ByVal Optional parentNode As TreeNode = Nothing)
            Dim nodes As ItemCollection

            If parentNode Is Nothing Then
                tv.Items.Clear()
                nodes = tv.Items
            Else
                nodes = parentNode.Items
            End If

            For i As Integer = 0 To entList.Count - 1
                Dim ent As Entity = entList(i)

                If TypeOf ent Is BlockReference Then
                    Dim child As Block
                    Dim blockName As String = (CType(ent, BlockReference)).BlockName

                    If blocks.TryGetValue(blockName, child) Then
                        Dim parentTn As TreeNode = New TreeNode(parentNode, GetNodeName(blockName, i), False)
                        parentTn.Tag = ent
                        nodes.Add(parentTn)
                        PopulateTree(tv, child.Entities, blocks, parentTn)
                    End If
                Else
                    Dim type As String = ent.[GetType]().ToString().Split("."c).LastOrDefault()
                    Dim node = New TreeNode(parentNode, GetNodeName(type, i), True)
                    node.Tag = ent
                    nodes.Add(node)
                End If
            Next
        End Sub

    Private Function GetNodeName(ByVal name As String, ByVal index As Integer) As String
        Return String.Format("{0}", name)
    End Function

    Sub CleanCurrent(ByVal vl As Model, ByVal Optional rootLevel As Boolean = True)
            vl.Entities.ClearSelection()
            If rootLevel AndAlso vl.Entities.CurrentBlockReference IsNot Nothing Then vl.Entities.SetCurrent(Nothing)
        End Sub

        Sub CleanCurrentNodes(ByVal blocks As BlockKeyedCollection, ByVal parentBr As BlockReference)
            Dim toClean As Block

            If blocks.TryGetValue(parentBr.BlockName, toClean) Then

                For i As Integer = 0 To toClean.Entities.Count - 1

                    If TypeOf toClean.Entities(i) Is BlockReference Then
                        toClean.Entities(i).Selected = False
                        CleanCurrentNodes(blocks, CType(toClean.Entities(i), BlockReference))
                    End If
                Next
            End If
        End Sub

        Sub DeleteSelectedNode(ByVal tv As TreeView, ByVal vl As Model)
            If tv.SelectedItem IsNot Nothing Then
                Dim deletedEntity As Entity = TryCast((CType(tv.SelectedItem, TreeNode)).Tag, Entity)
                DeleteNodes(deletedEntity, tv.Items)
                CleanCurrent(vl)
            End If
        End Sub

        Private Sub DeleteNodes(ByVal entity As Entity, ByVal nodes As ItemCollection)
            Dim count As Integer = nodes.Count

            While count > 0
                count -= 1
                Dim node As TreeNode = CType(nodes(count), TreeNode)

                If ReferenceEquals(entity, node.Tag) Then
                    node.Remove()
                    count = -1
                Else
                    DeleteNodes(entity, node.Items)
                End If
            End While
        End Sub

        Function SynchTreeSelection(ByVal tv As TreeView, ByVal vl As Model) As Model.SelectedItem
            Dim parents As Stack(Of BlockReference) = New Stack(Of BlockReference)()
            Dim node As TreeNode = CType(tv.SelectedItem, TreeNode)

            If node IsNot Nothing Then
                Dim entity As Entity = TryCast(node.Tag, Entity)
                node = node.ParentNode

                While node IsNot Nothing
                    Dim ent = TryCast(node.Tag, Entity)
                    If ent IsNot Nothing Then parents.Push(CType(ent, BlockReference))
                    node = node.ParentNode
                End While

                Dim selItem = New Model.SelectedItem(New Stack(Of BlockReference)(parents), entity)
                selItem.[Select](vl, True)
                Return selItem
            End If

            Return Nothing
        End Function

        Sub SynchScreenSelection(ByVal tv As TreeView, ByVal blockReferences As Stack(Of BlockReference), ByVal selectedEntity As Model.SelectedItem)
            If tv.SelectedItem IsNot Nothing Then

                If TypeOf tv.SelectedItem Is TreeViewItem Then
                    CType(tv.SelectedItem, TreeViewItem).IsSelected = False
                Else
                    CType(tv.SelectedItem, TreeNode).IsSelected = False
                End If
            End If

            CollapseAll(tv)

            If selectedEntity IsNot Nothing AndAlso selectedEntity.Parents.Count > 0 Then
                Dim parentsReversed = selectedEntity.Parents.Reverse()
                Dim cumulativeStack = New Stack(Of BlockReference)(blockReferences)

                For Each br In parentsReversed
                    cumulativeStack.Push(br)
                Next

                blockReferences = New Stack(Of BlockReference)(cumulativeStack)
            End If

            SearchNodeInTree(tv, blockReferences, selectedEntity)
        End Sub

        Sub SearchNodeInTree(ByVal tv As TreeView, ByVal blockReferences As Stack(Of BlockReference), ByVal selectedEntity As Model.SelectedItem, ByVal Optional parentTn As TreeNode = Nothing)
            If blockReferences.Count = 0 AndAlso selectedEntity Is Nothing Then Return
            Dim tnc As ItemCollection = tv.Items
            If parentTn IsNot Nothing Then tnc = parentTn.Items

            If blockReferences.Count > 0 Then
                Dim br As BlockReference = blockReferences.Pop()

                For Each tn As TreeNode In tnc

                    If ReferenceEquals(br, tn.Tag) Then

                        If blockReferences.Count > 0 Then
                            tn.IsExpanded = True
                            SearchNodeInTree(tv, blockReferences, selectedEntity, tn)
                        Else

                            If selectedEntity IsNot Nothing Then

                                For Each childNode As TreeNode In tn.Items

                                    If ReferenceEquals(selectedEntity.Item, childNode.Tag) Then
                                        If childNode.ParentNode IsNot Nothing Then childNode.ParentNode.IsExpanded = True
                                        childNode.IsSelected = True
                                        Exit For
                                    End If
                                Next
                            Else
                                tn.IsSelected = True
                            End If
                        End If

                        Return
                    End If
                Next
            Else

                If selectedEntity IsNot Nothing Then

                    For Each childNode As TreeNode In tnc

                        If ReferenceEquals(selectedEntity.Item, childNode.Tag) Then
                            childNode.IsSelected = True
                            Exit For
                        End If
                    Next
                End If
            End If
        End Sub

        Sub CollapseAll(ByVal tv As TreeView)
            For Each i As TreeNode In tv.Items
                i.IsExpanded = False
            Next
        End Sub
    End Module

    Public Class TreeNode
        Inherits FrameworkElement

        Public Sub New(ByVal parent As TreeNode)
            Items = New TreeView().Items
            ParentNode = parent
        End Sub

    Public Sub New(ByVal parent As TreeNode, ByVal _text As String, ByVal isPart As Boolean)
        Me.New(parent)
        Text = _text

        If isPart Then
            Icon = New BitmapImage(GetUriFromResource("part_icon.png"))
        Else
            Icon = New BitmapImage(GetUriFromResource("component_icon.png"))
        End If
    End Sub

    Public Property Text As String
        Public Property Icon As BitmapImage

        Private Shared Function GetUriFromResource(ByVal resourceFilename As String) As Uri
            Return New Uri("pack://application:,,,/Resources/" & resourceFilename)
        End Function

        Public Property ParentNode As TreeNode
        Public Property Items As ItemCollection
        Public Shared ReadOnly IsSelectedProperty As DependencyProperty = DependencyProperty.Register("IsSelected", GetType(Boolean), GetType(TreeNode), New PropertyMetadata(Nothing))

        Public Property IsSelected As Boolean
            Get
                Return CBool(GetValue(IsSelectedProperty))
            End Get
            Set(ByVal value As Boolean)
                SetValue(IsSelectedProperty, value)
            End Set
        End Property

        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register("IsExpanded", GetType(Boolean), GetType(TreeNode), New PropertyMetadata(Nothing))

        Public Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
            Set(ByVal value As Boolean)
                SetValue(IsExpandedProperty, value)
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return Text
        End Function

        Public Sub Remove()
            While Items.Count > 0
                CType(Items(0), TreeNode).Remove()
            End While

            If ParentNode IsNot Nothing Then ParentNode.Items.Remove(Me)
        End Sub
End Class