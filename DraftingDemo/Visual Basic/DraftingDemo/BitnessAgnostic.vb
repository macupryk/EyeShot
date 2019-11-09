Imports devDept.Eyeshot.Translators
Imports devDept.Serialization
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports devDept.Eyeshot


	''' <summary>
	''' Helper class for dynamic loading of the Eyeshot x86 or x64 assembly.
	''' </summary>
	Friend Class BitnessAgnostic
		Public Sub New()
			devDept.Eyeshot.Environment.GetAssembly(Product, Title, Company, Version, Edition)

			Dim target As String = "x86"
			' Use the following code to check Operating System bitness
			' string target = System.Environment.Is64BitProcess ? "x64" : "x86";

			Dim platform As String = If(Title.ToLower().Contains("wpf"), "Wpf", "Win")

			AssemblyPath = String.Format("{0}\Bin\{1}\devDept.Eyeshot.Control.{1}.{2}.v{3}.dll", GetInstallFolderFromRegistry(), target, platform, Version.Major)

			If System.IO.File.Exists(AssemblyPath) Then
				Assembly = System.Reflection.Assembly.LoadFrom(AssemblyPath)
			End If
		End Sub

		Public ReadOnly AssemblyPath As String
		Public ReadOnly Version As Version
		Public ReadOnly Product, Title, Company, Edition As String
		Public ReadOnly Assembly As System.Reflection.Assembly

		''' <summary>
		''' Gets an instance of the ReadAutodesk class.
		''' </summary>        
		Public Function GetReadAutodesk(ByVal model As Model, ByVal fileName As String) As ReadFileAsync
			If Assembly Is Nothing Then
				Return Nothing
			End If

			Dim foundType = GetObjectType("ReadAutodesk")

			If foundType IsNot Nothing Then
				' invokes the constructor                
				Dim constructor As System.Reflection.ConstructorInfo = foundType.GetConstructor(New Type() { GetType(String), GetType(String), GetType(Boolean), GetType(Boolean), GetType(Boolean) })
				If constructor IsNot Nothing Then
					' parameters are: fileName, password, fixErrors, skipHatches
					Dim reader As Object = constructor.Invoke(New Object() { fileName, Nothing, False, False, False })

					Return TryCast(reader, ReadFileAsync)
				End If
			End If

			Return Nothing
		End Function

		''' <summary>
		''' Gets an instance of the WriteAutodesk class.
		''' </summary>        
		Public Function GetWriteAutodesk(ByVal model As Model, ByVal fileName As String) As WriteFileAsync
			If Assembly Is Nothing Then
				Return Nothing
			End If

			Dim writeAutodeskParamsType = GetObjectType("WriteAutodeskParams")

			If writeAutodeskParamsType IsNot Nothing Then
				' invokes the WriteAutodeskParams constructor                
				Dim constructor As System.Reflection.ConstructorInfo = writeAutodeskParamsType.GetConstructor(New Type() { GetType(Model), GetType(Drawings), GetType(Boolean), GetType(Boolean), GetType(Double) })
				If constructor IsNot Nothing Then
					' parameter is model
					Dim writeAutodeskParams As Object = constructor.Invoke(New Object() { model, Nothing, False, False, 1 })

					Dim writeAutodeskType = GetObjectType("WriteAutodesk")

					If writeAutodeskType IsNot Nothing Then
						' invokes the WriteAutodesk constructor                
						constructor = writeAutodeskType.GetConstructor(New Type() { writeAutodeskParamsType, GetType(String) })
						If constructor IsNot Nothing Then
							' parameters are: writeAutodeskParams, fileName
							Dim writer As Object = constructor.Invoke(New Object() { writeAutodeskParams, fileName })

							Return TryCast(writer, WriteFileAsync)
						End If
					End If
				End If
			End If

			Return Nothing
		End Function

		''' <summary>
		''' Gets an instance of the WritePDF class.
		''' </summary>        
		Public Function GetWritePDF(ByVal model As Model, ByVal fileName As String) As WriteFileAsync
			If Assembly Is Nothing Then
				Return Nothing
			End If

			' wfa = new WritePDF(new WritePdfParams(model1, new Size(595, 842), new Rectangle(10, 10, 575, 822), Color.White), saveFileDialog.FileName);

			Dim writePdfParamsType = GetObjectType("WritePdfParams")

			If writePdfParamsType IsNot Nothing Then
				' invokes the WritePdfParams constructor                
				Dim constructor As System.Reflection.ConstructorInfo = writePdfParamsType.GetConstructor(New Type() { GetType(Model), GetType(Size), GetType(Rect) })
				If constructor IsNot Nothing Then
					' parameter is Model, Size, Rectangle, Color
					Dim writePdfParams As Object = constructor.Invoke(New Object() { model, New Size(595, 842), New Rect(10, 10, 575, 822) })

					Dim writePDFType = GetObjectType("WritePDF")

					If writePDFType IsNot Nothing Then
						' invokes the WritePDF constructor                
						constructor = writePDFType.GetConstructor(New Type() { writePdfParamsType, GetType(String) })
						If constructor IsNot Nothing Then
							' parameters are: WritePdfParams, fileName
							Dim writer As Object = constructor.Invoke(New Object() { writePdfParams, fileName })

							Return TryCast(writer, WriteFileAsync)
						End If
					End If
				End If
			End If

			Return Nothing
		End Function

		''' <summary>
		''' Gets an instance of the FileSerializerEx class.
		''' </summary>        
		Public Function GetFileSerializerEx(Optional ByVal contentType? As contentType = Nothing) As FileSerializer
			If Assembly Is Nothing Then
				Return Nothing
			End If

			Dim foundType = GetObjectType("FileSerializerEx")

			If foundType IsNot Nothing Then
				' invokes the constructor                
				Dim constructor As System.Reflection.ConstructorInfo = If(contentType Is Nothing, foundType.GetConstructor(New Type(){}), foundType.GetConstructor(New Type() { GetType(contentType) }))
				If constructor IsNot Nothing Then
					' parameters are: fileName, password, fixErrors, skipHatches
					Dim serializer As Object = If(contentType Is Nothing, constructor.Invoke(New Object(){}), constructor.Invoke(New Object() { contentType }))

					Return TryCast(serializer, FileSerializer)
				End If
			End If

			Return Nothing
		End Function

		#Region "Helper methods"

		Private Function GetInstallFolderFromRegistry() As String
			' Open a subKey as read-only
			Using sk1 As Microsoft.Win32.RegistryKey = GetdevDeptRegistryKey(Microsoft.Win32.Registry.LocalMachine)

				Try
					Return DirectCast(sk1.GetValue("Install folder " & Version.Major), String)
				Catch e1 As Exception
					Return Nothing
				End Try
			End Using
		End Function

		Private Shared Function GetdevDeptRegistryKey(ByVal baseKey As Microsoft.Win32.RegistryKey) As Microsoft.Win32.RegistryKey
			Try
                If System.Environment.Is64BitProcess Then
                    Return baseKey.OpenSubKey("Software\Wow6432Node\devDept Software\")
                End If

                Return baseKey.OpenSubKey("Software\devDept Software\")
			Catch ex As Exception
				MessageBox.Show(ex.Message)
				Return Nothing
			End Try

		End Function

		Private Function GetObjectType(ByVal className As String) As Type
			Dim foundType As Type = Nothing
			Dim types() As Type = Assembly.GetExportedTypes()

			If types.Length > 0 Then
				For Each type As Type In types
					If type.Name.Equals(className, StringComparison.OrdinalIgnoreCase) Then
						foundType = type
						Exit For
					End If
				Next type
			End If

			Return foundType
		End Function

		#End Region ' Helper methods
	End Class

