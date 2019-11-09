
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities


Public Class MyEntityList
	Inherits ObservableCollection(Of Entity)
	Protected Overrides Sub OnCollectionChanged(e As NotifyCollectionChangedEventArgs)
		MyBase.OnCollectionChanged(e)

		If Model Is Nothing Then
			Return
		End If
		Select Case e.Action
			Case NotifyCollectionChangedAction.Add
				If Not _stopInvalidate Then
					Model.Entities.Add(TryCast(e.NewItems(0), Entity))
					Model.Invalidate()
				End If
				Exit Select
			Case NotifyCollectionChangedAction.Remove
				Model.Entities.Remove(TryCast(e.OldItems(0), Entity))
				If Not _stopInvalidate Then
					Model.Invalidate()
				End If
				Exit Select
		End Select
	End Sub


	' When I add or remove a range of entities, I want to refresh the Model only at the end.
	Private _stopInvalidate As Boolean
	Public Sub AddRange(entities As IEnumerable(Of Entity))
		_stopInvalidate = True

		For Each entity As Entity In entities
			Add(entity)
		Next
		Model.Entities.AddRange(entities)
		Model.Invalidate()

		_stopInvalidate = False
	End Sub

	Public Sub RemoveRange(entities As IEnumerable(Of Entity))
		_stopInvalidate = True

		For Each entity As Entity In entities
			Remove(entity)
		Next
		Model.Invalidate()

		_stopInvalidate = False
	End Sub

	Public Property Model() As Model
		Get
			Return m_Model
		End Get
		Set
			m_Model = Value
		End Set
	End Property
	Private m_Model As Model
End Class
