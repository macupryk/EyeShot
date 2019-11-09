Imports System.Collections.Generic
	Public Interface ISelect
		' Gets or sets the list of selected triangles 
		Property SelectedSubItems() As List(Of Integer)

		Property DrawSubItemsForSelection() As Boolean

		Sub SelectSubItems(ByVal indices() As Integer)
	End Interface
