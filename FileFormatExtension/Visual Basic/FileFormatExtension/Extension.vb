Imports System.Text
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Eyeshot.Translators
Imports devDept.Serialization
Imports ProtoBuf
Imports ProtoBuf.Meta

' Here we use a global namespace to ensure the compatibility between C# and VB.NET samples for the custom object named "CustomData".
Namespace Global.EyeshotExtensions

#Region "MyCircle"
''' <summary>
''' Defines an extension for the Circle entity
''' </summary>
Public Class MyCircle
    Inherits Circle

#Region "Constructors"
    Public Sub New(ByVal plane As Plane, ByVal radius As Double)
        MyBase.New(plane, radius)
    End Sub

    Public Sub New(ByVal another As Circle)
        MyBase.New(another)
    End Sub

#End Region
    Public Property CustomDescription As String

    Public Overrides Function Dump() As String

        Dim sb = New StringBuilder(MyBase.Dump())

        sb.AppendLine("CustomDescription = " & CustomDescription)

        Dim cd = TryCast(EntityData, CustomData)
        If cd IsNot Nothing Then
            sb.AppendLine("----------------------")
            sb.AppendLine("CustomData")
            sb.Append(cd.Dump())
        End If

        Return sb.ToString()

    End Function

#Region "Proprietary file format"
    Public Overrides Function ConvertToSurrogate() As EntitySurrogate
        Return New MyCircleSurrogate(Me)
    End Function

#End Region
End Class

''' <summary>
''' Defines the <see cref="MyCircle"/> surrogate.
''' </summary>
Public Class MyCircleSurrogate
    Inherits CircleSurrogate

    ''' <summary>
    ''' Standard constructor
    ''' </summary>
    Public Sub New(ByVal myCircle As MyCircle)
        MyBase.New(myCircle)
    End Sub

    ''' <summary>
    ''' My custom description
    ''' </summary>
    Public Property CustomDescription As String

    ''' <summary>
    ''' Creates the object related to the surrogate.
    ''' </summary>
    ''' <remarks>
    ''' This method uses the <see cref="MyCircle.MyCircle(Plane, Double)"/> constructor to create the object.
    ''' When the content is <see cref="contentType.Tessellation"/> the resulting object is a <see cref="LinearPath"/>
    ''' </remarks>
    ''' <returns>The object created.</returns>
    Protected Overrides Function ConvertToObject() As Entity

        Dim ent As Entity
        If DeserializationContent = contentType.Tessellation Then
            ' When the content is "Tessellation only" we create a LinearPath instead of a MyCircle.
            ' If the entity was stored without vertices data, we add a Ghost entity as placeholder.

            If CheckSurrogateData(DeserializationContent, String.Empty) Then ' pass string empty so no log is written
                ent = New LinearPath(Vertices)
            Else
                ent = New Ghost("MyCircle without tessellation data.")
                WriteLog("MyCircle without tessellation data has been created as Ghost entity.")
            End If
        Else
            ent = New MyCircle(Plane, Radius)
        End If

        CopyDataToObject(ent)

        Return ent

    End Function

    ''' <summary>
    ''' Copies common data from surrogate to object.
    ''' </summary>
    ''' <remarks>This method is called by the ConvertToObject method after the creation of the object instance.</remarks>
    Protected Overrides Sub CopyDataToObject(ByVal entity As Entity)

        Dim myCircle = TryCast(entity, MyCircle)
        If myCircle IsNot Nothing Then
            myCircle.CustomDescription = CustomDescription
        End If

        MyBase.CopyDataToObject(entity)

    End Sub

    ''' <summary>
    ''' Copies all data from the object to its surrogate.
    ''' </summary>
    ''' <remarks>Use this method to fill ALL the properties of this surrogate. It is called by the empty constructor to initialize the surrogates properties.</remarks>
    Protected Overrides Sub CopyDataFromObject(ByVal entity As Entity)

        Dim myCircle = TryCast(entity, MyCircle)
        If myCircle IsNot Nothing Then
            CustomDescription = myCircle.CustomDescription
        End If

        MyBase.CopyDataFromObject(entity)

    End Sub
    
    ''' <summary>
    ''' Integrity check according to the content type.
    ''' </summary>
    ''' <remarks>
    ''' During the serialization process, this method is called internally before serializing the surrogate. 
    ''' During the deserialization process, it can be used in the ConvertToObject method.
    ''' </remarks>
    Protected Overrides Function CheckSurrogateData(ByVal content As contentType, ByVal Optional logMessage As String = Nothing) As Boolean

        If content = contentType.Tessellation Then
            If Vertices Is Nothing OrElse Vertices.Length = 0 Then
                WriteLog(If(logMessage IsNot Nothing, logMessage, "Warning MyCircle with no vertices."))
                Return False
            End If
        End If
        Return True

    End Function

End Class

#End Region

#Region "Custom data"

''' <summary>
''' Defines a custom object.
''' </summary>
''' <remarks>Compile the project with the conditional symbol "OLDVER" to simulate an old version of this class.</remarks>
Public Class CustomData

#Region "Constructors"
    ''' <summary>
    ''' Constructor for custom version 1.1
    ''' </summary>
    ''' <seealso cref="MyFileSerializer.CustomTag"/>
    Public Sub New(ByVal id As Integer)
        ME.Id = id
    End Sub

#If Not OLDVER Then
    ''' <summary>
    ''' Constructor for custom version 1.2
    ''' </summary>
    ''' <seealso cref="MyFileSerializer.CustomTag"/>
    Public Sub New(ByVal id As Integer, ByVal price As Single)
        Me.Id = id
        Me.Price = price
    End Sub
#End If

#End Region

#Region "Properties"
    Public Property Id As Integer

    Public Property Description As String

#If Not OLDVER Then
    Public Property Price As Single
#End If

#End Region

    Public Overridable Function Dump() As String

        Dim sb As StringBuilder = New StringBuilder()

        sb.AppendLine("Id = " & Id)
        sb.AppendLine("Description = " & Description)

#If Not OLDVER Then
        sb.AppendLine("Price = " & Price)
#End If

        Return sb.ToString()

    End Function

#Region "Proprietary file format"
    Public Overridable Function ConvertToSurrogate() As CustomDataSurrogate
        Return New CustomDataSurrogate(Me)
    End Function
#End Region

End Class

''' <summary>
''' Defines the <see cref="CustomData"/> surrogate.
''' </summary>
Public Class CustomDataSurrogate
    Inherits Surrogate(Of CustomData)

#Region "Constructors"
    Public Sub New(ByVal obj As CustomData)
        MyBase.New(obj) ' The base calls the CopyDataFromObject method.
    End Sub
#End Region

#Region "Properties"
    Public Property Id As Integer
    Public Property Description As String
    Public Property Price As Single

#End Region

#Region "Methods"
    Protected Overrides Function ConvertToObject() As CustomData

        Dim cd As CustomData = Nothing
#If Not OLDVER Then
        ' Here you can use the Tag to handle different behavior for different versions.
        If Tag = "1.1" Then
            cd = New CustomData(Id)
        ElseIf Tag = "1.2" Then
            cd = New CustomData(Id, Price)
        End If
#Else
        cd = New CustomData(Id)
#End If
        CopyDataToObject(cd)
        Return cd

    End Function

    Protected Overrides Sub CopyDataToObject(ByVal cd As CustomData)

        cd.Description = Description
#If Not OLDVER Then
        If Tag = "1.1" Then cd.Price = 100 'I want to force the price for object stored with the old version.
#End If
    End Sub

    Protected Overrides Sub CopyDataFromObject(ByVal cd As CustomData)

        Id = cd.Id
        Description = cd.Description
#If Not OLDVER Then
        Price = cd.Price
#End If
    End Sub

#End Region

#Region "Static Methods"
    
    ''' <summary>
    ''' Converts the surrogate to the related object during the deserialization process.
    ''' </summary>    
    Public Shared Widening Operator CType(ByVal surrogate As CustomDataSurrogate) As CustomData
        If surrogate Is Nothing Then Return Nothing
        Return surrogate.ConvertToObject()
    End Operator
    
    ''' <summary>
    ''' Converts the object to the related surrogate during the serialization process.
    ''' </summary>    
    Public Shared Widening Operator CType(ByVal source As CustomData) As CustomDataSurrogate
        If source Is Nothing Then Return Nothing
        Return source.ConvertToSurrogate()
    End Operator
#End Region

End Class

#End Region

''' <summary>
''' Defines an extension for the Eyeshot proprietary file format.
''' </summary>
''' <remarks>
''' If you want to include special Autodesk objects like LayerEx, CircleEx, PictureEx, etc. you need to derive from FileSerializerEx contained in the x86/x64 Eyeshot assemblies.
''' </remarks>
Public Class MyFileSerializer
    Inherits FileSerializer

    ' Tag used to handle different versions of your custom objects.
#If OLDVER Then
    Public Shared CustomTag As String = "1.1"
#Else
    Public Shared CustomTag As String = "1.2"
#End If

#Region "Constructors"

    ''' <summary>
    ''' Empty constructor used in conjunction with the <see cref="WriteFile"/> class that accepts the <see cref="Model"/> as parameter.
    ''' </summary>
    ''' <remarks>Use this constructor to define the serialization model for the <see cref="WriteFile"/> class that accepts the <see cref="Model"/> as parameter.</remarks>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Constructor used in conjunction with the <see cref="ReadFile"/> class.
    ''' </summary>
    ''' <exception cref="EyeshotException">Thrown if the content type is <see cref="contentType.None"/>.</exception>
    Public Sub New(ByVal contentType As contentType)
        MyBase.New(contentType)
    End Sub

#End Region

    Protected Overrides Sub FillModel()

        MyBase.FillModel()

        ' Adds MyCircle as sub-type of Circle
        ' When you add a sub-type to an Eyeshot object you have to use an id > 1000.
        Model(GetType(Circle)).AddSubType(1001, GetType(MyCircle))

        ' Adds MyCircleSurrogate as sub-type of CircleSurrogate
        Model(GetType(CircleSurrogate)).AddSubType(1001, GetType(MyCircleSurrogate))

        ' Defines properties for MyCircleSurrogate
        Model(GetType(MyCircleSurrogate)).Add(1, "CustomDescription").UseConstructor = False

        ' Adds the CustomData to the protobuf model and defines its surrogate.
        Model.Add(GetType(CustomData), False).SetSurrogate(GetType(CustomDataSurrogate))
        Dim mt As MetaType = Model(GetType(CustomDataSurrogate)).Add(1, "Id").Add(2, "Description")

        ' Use the header tag to handle different definitions for your custom model. 
        If Me.HeaderTag = "1.2" Then
            mt.Add(3, "Price")
        End If

        mt.SetCallbacks(Nothing, Nothing, "BeforeDeserialize", Nothing) ' Fills Version and Tag during the deserialization.
        mt.UseConstructor = False ' Avoids to use the parameterless constructor.

    End Sub

End Class

End Namespace