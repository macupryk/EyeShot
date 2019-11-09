Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Labels
Imports devDept.Graphics
Imports devDept.Geometry

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow    
    Public Sub New()
        InitializeComponent()
        'model1.Unlock("") '  For more details see 'Product Activation' topic in the documentation.
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        model1.GetGrid().AutoSize = True
        model1.GetGrid().[Step] = 5

        ' for correct volume calculation, during modeling it is useful to have this = true
        model1.ShowNormals = False

        Dim tol As Double = 0.25
        Dim holeDia As Double = 10
        Dim thickness As Double = 5

        Dim hole As ICurve = New Circle(40, 20, 0, holeDia)
        hole.Reverse()

        ' Bottom face
        ' to see the model at this point UNComment 
        ' the two lines just after this code block
        Dim baseProfile As New CompositeCurve(New Line(0, 0, 40, 0), New Arc(New Point3D(40, 20, 0), 20, 6 * Math.PI / 4, 2 * Math.PI), New Line(60, 20, 60, 40), New Line(60, 40, 0, 40), New Line(0, 40, 0, 0))

        Dim faceRegion As Region = new Region(baseProfile, hole)
        Dim part1 As Mesh = faceRegion.ConvertToMesh(tol)

        part1.FlipNormal()

        'model1.Entities.Add(part1);
        'return;

        ' Extrudes of some profile entities
        ' to see the model at this point UNComment 
        ' the two lines just after this code block
        Dim face As Mesh

        For i As Integer = 1 To 2

            face = baseProfile.CurveList(i).ExtrudeAsMesh(0, 0, thickness, tol, Mesh.natureType.Smooth)

            part1.MergeWith(face)
        Next

        face = hole.ExtrudeAsMesh(New Vector3D(0, 0, thickness), tol, Mesh.natureType.Smooth)
     
        part1.MergeWith(face)

        'model1.Entities.Add(part1);
        'return;        

        ' Top face
        ' to see the model at this point UNComment 
        ' the two lines just after this code block
        baseProfile = New CompositeCurve(New Line(thickness, 0, 40, 0), New Arc(New Point3D(40, 20, 0), 20, 6 * Math.PI / 4, 2 * Math.PI), New Line(60, 20, 60, 40), New Line(60, 40, thickness, 40), New Line(thickness, 40, thickness, 0))

        faceRegion = new Region(baseProfile, hole)
        face = faceRegion.ConvertToMesh(tol)

        ' Translates it to Z = 10
        face.Translate(0, 0, thickness)

        part1.MergeWith(face)

        'model1.Entities.Add(part1);
        'return;


        ' Top vertical profile
        ' to see the model at this point UNComment 
        ' the two lines just after this code block
        Dim pl As New LinearPath(4)

        pl.Vertices(0) = New Point3D(thickness, 0, thickness)
        pl.Vertices(1) = New Point3D(thickness, 0, 30)
        pl.Vertices(2) = New Point3D(0, 0, 30)
        pl.Vertices(3) = New Point3D(0, 0, 0)

        face = pl.ExtrudeAsMesh(0, 40, 0, tol, Mesh.natureType.Smooth)

        face.FlipNormal()

        part1.MergeWith(face)

        'model1.Entities.Add(part1);
        'return;


        ' Front 'L' shaped face
        ' to see the model at this point UNComment 
        ' the two lines just after this code block
        Dim frontProfile As Point3D() = New Point3D(6) {}

        frontProfile(0) = Point3D.Origin
        frontProfile(1) = New Point3D(40, 0, 0)
        frontProfile(2) = New Point3D(40, 0, thickness)
        frontProfile(3) = New Point3D(thickness, 0, thickness)
        frontProfile(4) = New Point3D(thickness, 0, 30)
        frontProfile(5) = New Point3D(0, 0, 30)
        frontProfile(6) = Point3D.Origin

        ' This profile is in the wrong direction, we use true as last parameter
        face = Mesh.CreatePlanar(frontProfile, Mesh.natureType.Smooth)

        ' makes a deep copy of this face
        Dim rearFace As Mesh = DirectCast(face.Clone(), Mesh)

        part1.MergeWith(face)

        ' model1.Entities.Add(part1);
        ' return;


        ' Rear 'L' shaped face
        ' to see the model at this point UNComment 
        ' the two lines just after this code block

        ' Translates it to Y = 40
        rearFace.Translate(0, 40, 0)

        ' Stretches it
        For i As Integer = 0 To rearFace.Vertices.Length - 1

            If rearFace.Vertices(i).X > 10 Then

                rearFace.Vertices(i).X = 60
            End If
        Next

        rearFace.FlipNormal()
        part1.MergeWith(rearFace)

        'model1.Entities.Add(part1);
        'return;


        ' Set the normal averaging and edge style mode
        part1.NormalAveragingMode = Mesh.normalAveragingType.AveragedByAngle
        part1.EdgeStyle = Mesh.edgeStyleType.Sharp

        model1.Layers.Add("Brakets", System.Drawing.Color.Crimson)

        ' Adds the mesh to the model1
        model1.Entities.Add(part1, "Brakets")

        Dim mp As New VolumeProperties(part1.Vertices, part1.Triangles)

        ' Adds the volume label            
        model1.Labels.Add(New LeaderAndText(60, 40, thickness, "Volume = " + mp.Volume.ToString("f3") & " cubic " & model1.Units, New System.Drawing.Font("Tahoma", 8.25F), System.Drawing.Color.Black, New Vector2D(0, 50)))

        ' fits the model in the model1            
        model1.ZoomFit()

        ' refresh the viewport
        model1.Invalidate()

        MyBase.OnContentRendered(e)
    End Sub
End Class

