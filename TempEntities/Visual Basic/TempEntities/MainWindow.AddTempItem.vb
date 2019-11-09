Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports System.Drawing

Namespace WpfApplication1
    Partial Public Class MainWindow
#if NURBS
        Private Enum itemType
    
    Vertex
    
    Edge
    
    Face
    
    None
End Enum

Dim itemMode As itemType = itemType.None
Dim tempItems As List(Of Entity) = New List(Of Entity)
    
    Private Sub addItemToggle_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles addItemToggle.Click
        If (addItemToggle Is Nothing)
            Return
        End If

        If (addItemToggle.IsChecked.HasValue AndAlso addItemToggle.IsChecked.Value) Then
            addItemToggle.Content = "Disable"
            ' sets the item mode
            itemMode = CType(addItemCombo.SelectedIndex,itemType)
            Select Case (itemMode)
                Case itemType.Vertex
                    ' sets selection filter mode in order to get only the vertices under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Vertex
                Case itemType.Edge
                    ' sets selection filter mode in order to get only the edges under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Edge
                Case itemType.Face
                    ' sets selection filter mode in order to get only the faces under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Face
            End Select
            
            ' gets the leafs Brep entities under mouse cursor
            model1.AssemblySelectionMode = devDept.Eyeshot.Environment.assemblySelectionType.Leaf
            ' disables moving action and button
            move = false
            moveToggle.IsEnabled = false
        Else
            addItemToggle.Content = "Enable"
            ' disables the add item action
            itemMode = itemType.None
            ' clears current temporary items
            For Each item As Entity In tempItems
                model1.TempEntities.Remove(item)
            Next
            tempItems.Clear
            ' restores moving action and button
            moveToggle.IsEnabled = true
            move = (moveToggle.IsChecked.HasValue AndAlso moveToggle.IsChecked.Value)
        End If
        
        ' refresh the screen
        model1.Invalidate
    End Sub
    
    Private Sub addItemCombo_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles addItemCombo.SelectionChanged
        If (addItemToggle IsNot Nothing AndAlso addItemToggle.IsChecked.HasValue AndAlso addItemToggle.IsChecked.Value) Then
            ' enables the add item action with the selected item mode
            itemMode = CType(addItemCombo.SelectedIndex,itemType)
            Select Case (itemMode)
                Case itemType.Vertex
                    ' sets selection filter mode in order to get only the vertices under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Vertex
                Case itemType.Edge
                    ' sets selection filter mode in order to get only the edges under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Edge
                Case itemType.Face
                    ' sets selection filter mode in order to get only the faces under mouse cursor
                    model1.SelectionFilterMode = selectionFilterType.Face
            End Select
            
        Else
            itemMode = itemType.None
        End If
        
    End Sub
    
    Private Sub AddEntityItem(ByVal mousePosition As System.Windows.Point)
        If (itemMode = itemType.None) Then
            Return
        End If
        
        ' the tranformation of the parent BlockReference
        Dim trans As Transformation = New Identity
        ' the item under mouse cursor to be added into TempEntities list
        Dim tempItem As Entity = Nothing
        ' gets the vertex under mouse cursor
        Dim selItem As devDept.Eyeshot.Environment.SelectedSubItem = CType(model1.GetItemUnderMouseCursor(RenderContextUtility.ConvertPoint(mousePosition)),devDept.Eyeshot.Environment.SelectedSubItem)
        If (selItem Is Nothing) Then
            Return
        End If
        
        'the Brep entity under mouse cursor
        Dim brep As Brep = CType(selItem.Item,Brep)
        ' gets transformation of the parent BlockReference (there is only one level of hierarchy)
        trans = selItem.Parents.First.Transformation
        Select Case (itemMode)
            Case itemType.Vertex
                ' creates a Point as temp entity that represent the vertex item
                tempItem = New devDept.Eyeshot.Entities.Point(brep.Vertices(selItem.Index), 15)
                tempItem.Color = Color.FromArgb(150, Color.Blue)
            Case itemType.Edge
                ' creates an ICurve as temp entity that represent the edge item
                tempItem = CType(CType(brep.Edges(selItem.Index).Curve,Entity).Clone,Entity)
                tempItem.LineWeight = 10
                tempItem.Color = Color.FromArgb(150, Color.Purple)
            Case itemType.Face
                ' creates a Mesh as temp entity that represent the face item
                tempItem = brep.Faces(selItem.Index).ConvertToMesh(skipEdges:= true)
                tempItem.Color = Color.FromArgb(150, Color.DeepSkyBlue)
        End Select
        
        ' transform the temp entity onto the represented item 
        tempItem.TransformBy(trans)
        ' regens it before to add into TempEntity list
        If (TypeOf tempItem Is ICurve) Then
            tempItem.Regen(0.01)
        End If
        
        ' adds it to the TempEntities list
        model1.TempEntities.Add(tempItem)
        'stores it into tempItems list
        tempItems.Add(tempItem)
    End Sub
    
    Private Sub model1_MouseDownItem(ByVal sender As Object, ByVal e As MouseButtonEventArgs) Handles model1.PreviewMouseDown
        ' add item(vertex, Edge or Face) under mouse cursor as Temporary Entity on the screen
        If ((e.LeftButton = MouseButtonState.Pressed)  _
                    AndAlso ((model1.ActionMode = actionType.None)  _
                    AndAlso Not model1.GetToolBar.Contains(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e))))) Then
            AddEntityItem(model1.GetMousePosition(e))
        End If
        
    End Sub
#End If
    End Class
End Namespace
