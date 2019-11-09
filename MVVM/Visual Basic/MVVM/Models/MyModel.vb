
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports devDept.Eyeshot
Imports devDept.Graphics

Public Class MyModel
	Inherits Model    

	Public Shared ReadOnly MyEntityListProperty As DependencyProperty = DependencyProperty.Register("MyEntityList", GetType(MyEntityList), GetType(MyModel), New PropertyMetadata(Nothing, AddressOf OnMyEntityListChanged))

	Private Shared Sub OnMyEntityListChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
		Dim vp = DirectCast(d, MyModel)
		Dim oldValue = DirectCast(e.OldValue, MyEntityList)
		If oldValue IsNot Nothing Then
			oldValue.Model = Nothing
		End If
		Dim newValue = DirectCast(e.NewValue, MyEntityList)
		If newValue IsNot Nothing Then
			newValue.Model = vp
		End If
	End Sub

	Public Property MyEntityList() As MyEntityList
		Get
			Return DirectCast(GetValue(MyEntityListProperty), MyEntityList)
		End Get
		Set
			SetValue(MyEntityListProperty, value)
		End Set
	End Property
End Class
