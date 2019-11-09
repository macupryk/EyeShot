Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Navigation
Imports devDept.Eyeshot
Imports devDept.Graphics

Public Class ColorTableToStringConverter
    Implements IValueConverter
    Public Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim retVal As String = String.Empty
        If TypeOf value Is ObservableCollection(Of Brush) Then
            Select Case DirectCast(value, ObservableCollection(Of Brush)).Count
                Case 9
                    If True Then
                        retVal = "Red to Blue 9"
                        Exit Select
                    End If
                Case 17
                    If True Then
                        retVal = "Red to Blue 17"
                        Exit Select
                    End If
                Case 33
                    If True Then
                        retVal = "Red to Blue 33"
                        Exit Select
                    End If
            End Select
        End If
        Return retVal
    End Function
        
    Public Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotSupportedException()
    End Function
End Class

Public Class ViewportEnumsToStringConverter
    Implements IValueConverter
    Public Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If TypeOf value Is backgroundStyleType Then
            Return DirectCast(value, backgroundStyleType).ToString()
        End If

        If TypeOf value Is coordinateSystemPositionType Then
            Return DirectCast(value, coordinateSystemPositionType).ToString()
        End If

        If TypeOf value Is originSymbolStyleType Then
            Return DirectCast(value, originSymbolStyleType).ToString()
        End If

        If TypeOf value Is ToolBar.positionType Then
            Return DirectCast(value, ToolBar.positionType).ToString()
        End If

        If TypeOf value Is displayType Then
            Return DirectCast(value, displayType).ToString()
        End If

        If TypeOf value Is actionType Then
            Return DirectCast(value, actionType).ToString()
        End If

        If TypeOf value Is colorThemeType Then
            Return DirectCast(value, colorThemeType).ToString()
        End If

        Return [String].Empty
    End Function

    Public Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotSupportedException()
    End Function
End Class

<ValueConversion(GetType(Boolean), GetType(Boolean))> _
Public Class InverseBooleanConverter
	Implements IValueConverter
	Public Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
		If targetType <> GetType(Boolean) Then
			Throw New InvalidOperationException("The target must be a boolean")
		End If

		Return Not CBool(value)
	End Function

	Public Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
		Throw New NotSupportedException()
	End Function
End Class

Public Class ColorToBrushConverter
    Implements IValueConverter
    Public Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If TypeOf value Is System.Windows.Media.Color Then
            Return Helper.ConvertColor(DirectCast(value, System.Windows.Media.Color))
        End If
        Return Nothing
    End Function

    Public Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotSupportedException()
    End Function
End Class

Public Class ColorToDrawingColorConverter
    Implements IValueConverter
    Public Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If TypeOf value Is System.Windows.Media.Color Then
            Return Helper.ConvertDrawingColor(DirectCast(value, System.Windows.Media.Color))
        End If
        Return Nothing
    End Function

    Public Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotSupportedException()
    End Function
End Class

Public NotInheritable Class Helper
    Private Sub New()
    End Sub
    Public Shared Function ConvertColor(brush As Brush) As Color
        Dim newBrush As SolidColorBrush = DirectCast(brush, SolidColorBrush)
        Return newBrush.Color
    End Function

    Public Shared Function ConvertColor(color As Color) As Brush
        Return New SolidColorBrush(color)
    End Function

    Public Shared Function ConvertDrawingColor(color As Color) As System.Drawing.Color
        Return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)
    End Function

    ''' <summary>
    ''' Converts hex color string to <see cref="System.Drawing.Color"/>
    ''' </summary>
    ''' <param name="hexColor">Hex color like "#FF434752"</param>
    ''' <returns>The <see cref="System.Drawing.Color"/></returns>
    Public Shared Function ConvertColor(hexColor As String) As System.Drawing.Color
	    Return System.Drawing.ColorTranslator.FromHtml(hexColor)
    End Function

    Public Shared Function GetUriFromResource(resourceFilename As String) As Uri
        Return New Uri(Convert.ToString("pack://application:,,,/Resources/") & resourceFilename)
    End Function
End Class



