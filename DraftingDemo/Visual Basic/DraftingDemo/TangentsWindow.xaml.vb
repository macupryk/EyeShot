Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes

	''' <summary>
	''' Interaction logic for Window1.xaml
	''' </summary>
	Partial Public Class TangentsWindow
		Inherits Window
        private _tangentRadius as double=10
		Public Property TangentRadius() As Double
        get
            return _tangentRadius
        End Get
		    Set(value As Double)
                _tangentRadius=value
		    End Set
		End Property

        private _lineTangents as Boolean =true 
		Public Property LineTangents() As Boolean 
       get
           return _lineTangents
       End Get
		    Set(value As Boolean)
                _lineTangents=value
		    End Set
		End Property

		Public Property CircleTangents() As Boolean

		Public Property TrimTangents() As Boolean

		Public Property FlipTangents() As Boolean
		Public Sub New()
			InitializeComponent()
			ResizeMode = ResizeMode.NoResize
			AddHandler linesRadioButton.Click, AddressOf LinesRadioButton_Click
			AddHandler circlesRadioButton.Click, AddressOf circlesRadioButton_Click
			AddHandler trimCheckBox.Checked, AddressOf trimCheckBox_CheckedChanged
			AddHandler trimCheckBox.Unchecked, AddressOf trimCheckBox_CheckedChanged
			AddHandler flipCheckBox.Checked, AddressOf flipCheckBox_CheckedChanged
			AddHandler flipCheckBox.Unchecked, AddressOf flipCheckBox_CheckedChanged
			AddHandler radiusTextBox.TextChanged, AddressOf radiusTextBox_TextChanged
		End Sub

		Private Sub circlesRadioButton_Click( sender As Object,  e As RoutedEventArgs)
			CircleTangents = circlesRadioButton.IsChecked.Value
			LineTangents = linesRadioButton.IsChecked.Value
			radiusTextBox.IsEnabled = circlesRadioButton.IsChecked.Value
			radiusLabel.IsEnabled = circlesRadioButton.IsChecked.Value
			optionsGroupBox.IsEnabled=circlesRadioButton.IsChecked.Value
		End Sub

		Private Sub LinesRadioButton_Click( sender As Object,  e As RoutedEventArgs)
			CircleTangents = circlesRadioButton.IsChecked.Value
			LineTangents = linesRadioButton.IsChecked.Value
			radiusTextBox.IsEnabled = circlesRadioButton.IsChecked.Value
			radiusLabel.IsEnabled = circlesRadioButton.IsChecked.Value
			optionsGroupBox.IsEnabled=circlesRadioButton.IsChecked.Value
		End Sub



		Private Sub radiusTextBox_TextChanged( sender As Object,  e As TextChangedEventArgs)
			Dim val As Double = Nothing
			If Double.TryParse(radiusTextBox.Text, val) Then
				TangentRadius = val
			End If
		End Sub

		Private Sub flipCheckBox_CheckedChanged( sender As Object,  e As RoutedEventArgs)
			FlipTangents = flipCheckBox.IsChecked.Value
		End Sub

		Private Sub trimCheckBox_CheckedChanged( sender As Object,  e As RoutedEventArgs)
			TrimTangents = trimCheckBox.IsChecked.Value
		End Sub



		Private Sub selectButton_OnClick( sender As Object,  e As RoutedEventArgs)
			DialogResult = True
			Close()

		End Sub
	End Class

