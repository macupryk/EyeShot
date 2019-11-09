Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports System.Drawing

Namespace WpfApplication1
    ' This code file shows how to blink an entity (over the other entities) with a defined offset time by using tempEntities.
    ' It shows also how to display and keep update a TreeView of the Entities list on the scene.
    Partial Public Class MainWindow
        Dim showEntity As Boolean = false
Dim blinkTimer As System.Threading.Timer = Nothing
Dim selectedItem As Model.SelectedItem = Nothing
Dim blinkEntity As Mesh = Nothing

    
    Public Sub StartBlink(ByVal intervalMs As Integer)
        If ((selectedItem Is Nothing)  _
                    OrElse (Not (blinkEntity) Is Nothing)) Then
            Return
        End If
        
        ' gets a unique mesh to blink from the selected entity
        blinkEntity = GetUniqueEntity(CType(selectedItem.Item,Entity))
        ' stores blink temp entity reference to keep them synchronized during translation
        If (TypeOf selectedItem.Item Is BlockReference) Then
            CType(selectedItem.Item,Entity).EntityData = blinkEntity
        End If
        
        ' if selected item is not a root element, find the root
        If selectedItem.HasParents Then
            ' transforms temp entity to the real position of the original leaf entity
            Dim t As Transformation = New Identity
            For Each parent As BlockReference In selectedItem.Parents
                t = (parent.Transformation * t)
            Next
            blinkEntity.TransformBy(t)
            ' stores blink temp entity reference into root element
            selectedItem.Parents.Last.EntityData = blinkEntity
        End If
        
        blinkEntity.Color = Color.FromArgb(100, Color.Yellow)
        ' hides edges for blink entity
        blinkEntity.Edges = Nothing
        blinkEntity.EdgeStyle = Mesh.edgeStyleType.None
        ' computes the needed data to draw temp entity
        If (blinkEntity.RegenMode = regenType.RegenAndCompile) Then
            blinkEntity.Regen(0.1)
        End If

            ' starts the blink action
            blinkTimer = New System.Threading.Timer(AddressOf Blink, blinkEntity, 0, intervalMs)
        End Sub
        
        Public Sub StopBlink()
        If (blinkTimer Is Nothing) Then
            Return
        End If
        
        blinkTimer.Dispose
        model1.TempEntities.Remove(blinkEntity)
        showEntity = false
        blinkEntity = Nothing
    End Sub
    
    Private Sub Blink(sender As Object)
        ' Draws the temp entity on the scene alternately at each timer tick (defined by the interval time in ms)
        showEntity = Not showEntity
        If showEntity Then
            model1.TempEntities.Add(CType(sender,Entity))
        Else
            model1.TempEntities.Remove(CType(sender,Entity))
        End If
        
        Dispatcher.Invoke(Function()
            'refresh the screen on the main thread
            model1.Invalidate()
        End Function)
    End Sub
    
    Private Sub blinkToggle_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        If (blinkToggle.IsChecked.HasValue AndAlso blinkToggle.IsChecked.Value) Then
            blinkToggle.Content = "Disable"
            If (treeView1.SelectedItem Is Nothing) Then
                CType(treeView1.Items(0),TreeNode).IsSelected = true
            End If
            
            'starts a new blink action of 500ms
            StartBlink(500)
        Else
            blinkToggle.Content = "Enable"
            ' stops current blink action
            StopBlink
        End If
        
        'refresh the screen
        model1.Invalidate
    End Sub
    
#Region "TreeView methos"
    Public Shared Sub PopulateTree(ByVal tv As TreeView, ByVal entList As IList(Of Entity), ByVal blocks As BlockKeyedCollection, Optional ByVal parentNode As TreeNode = Nothing)
        Dim nodes As ItemCollection
        If (parentNode Is Nothing) Then
            nodes = tv.Items
        Else
            nodes = parentNode.Items
        End If
        
        Dim i As Integer = 0
        Do While (i < entList.Count)
            Dim ent As Entity = entList(i)
            If (TypeOf ent Is BlockReference) Then
                Dim child As Block
                Dim blockName As String = CType(ent,BlockReference).BlockName
                If blocks.TryGetValue(blockName, child) Then
                    Dim parentTn As TreeNode = New TreeNode(parentNode, GetNodeName(blockName, i))
                    parentTn.Tag = ent
                    nodes.Add(parentTn)
                    PopulateTree(tv, child.Entities, blocks, parentTn)
                End If
                
            Else
                Dim type As String = ent.GetType.ToString.Split(Microsoft.VisualBasic.ChrW(46)).LastOrDefault
                Dim node = New TreeNode(parentNode, GetNodeName(type, i))
                node.Tag = ent
                nodes.Add(node)
            End If
            
            i = (i + 1)
        Loop
        
    End Sub
    
    Private Shared Function GetNodeName(ByVal name As String, ByVal index As Integer) As String
        Return String.Format("{0} ({1})", name, index)
    End Function
    
    Private Sub treeView1_SelectedChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object))
        ' creates a selected entity instance from the TreeView selection
        selectedItem = SynchTreeSelection(treeView1, model1)
        If (blinkToggle.IsChecked.HasValue AndAlso blinkToggle.IsChecked.Value) Then
            ' stops current blink if active
            StopBlink
            'start a new blink action with the new selected entity
            StartBlink(500)
        End If
        
        'refresh the screen
        model1.Invalidate
    End Sub
    
    Public Function SynchTreeSelection(ByVal tv As TreeView, ByVal vl As Model) As Model.SelectedItem
        ' Fill a stack of entities and blockreferences starting from the node tags
        Dim parents As Stack(Of BlockReference) = New Stack(Of BlockReference)
        Dim node As TreeNode = CType(tv.SelectedItem,TreeNode)
        Dim entity As Entity = CType(node.Tag,Entity)
        node = node.ParentNode
        
        While (Not (node) Is Nothing)
            Dim ent = CType(node.Tag,Entity)
            If (Not (ent) Is Nothing) Then
                parents.Push(CType(ent,BlockReference))
            End If
            
            node = node.ParentNode
            
        End While
        
        ' The top most parent is the root Blockreference: must reverse the order, creating a new Stack
        Dim stack As Stack(Of BlockReference) = New Stack(Of BlockReference)(parents)
        ' return the selected entity instance
        Return New Model.SelectedItem(stack, entity)
    End Function
        #End Region
    End Class

    ''' <summary>
''' In the XAML markup, I have specified a HierarchicalDataTemplate for the ItemTemplate of the TreeView.
''' This class represent the ViewModel for TreeView's Items.
''' </summary>
Public Class TreeNode
    Inherits FrameworkElement

    Public Sub New(ByVal parent As TreeNode)
        Items = New TreeView().Items
        ParentNode = parent
    End Sub

    Public Sub New(ByVal parent As TreeNode, ByVal text As String)
        Me.New(parent)
        Me.Text = text
    End Sub

    Public Property Text As String
    Public Property ParentNode As TreeNode
    Public Property Items As ItemCollection

    Public Function GetLevel() As Integer
        If ParentNode IsNot Nothing Then
            Return ParentNode.GetLevel() + 1
        End If

        Return 0
    End Function

    Public Shared ReadOnly IsSelectedProperty As DependencyProperty = DependencyProperty.Register("IsSelected", GetType(Boolean), GetType(TreeNode), New PropertyMetadata(Nothing))

    Public Function GetChildNode(ByVal name As String) As TreeNode
        For Each node As TreeNode In Items
            If node.Text.Equals(name) Then Return node

        Next

        Return Nothing
    End Function

    Public Function ContainsChildNode(ByVal name As String) As Boolean
        For Each node As TreeNode In Items
            If node.Text.Equals(name) Then Return True
        Next

        Return False
    End Function

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

End Namespace
