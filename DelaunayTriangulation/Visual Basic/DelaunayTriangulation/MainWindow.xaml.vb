Imports devDept.Eyeshot
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities
Imports devDept.Eyeshot.Triangulation
Imports devDept.Geometry

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        Public Sub New()
            InitializeComponent()

            'model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.
        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' defines and add a circle
            Dim c1 As New Circle(0, 0, 0, 8)

            ' regen with our own tolerance
            c1.Regen(0.05)

            ' defines and adds a rect
            Dim r1 As New LinearPath(3, 3)

            r1.Translate(1, -5, 0)


            ' creates an array of points ...
            Dim points As Point3D() = New Point3D(99) {}

            ' ... and fills it
            For y As Integer = 0 To 9

                For x As Integer = 0 To 9

                    Dim p As New Point3D(x, y, 0)

                    points(x + y * 10) = p

                    ' adds the point also to the master entity array                    

                    model1.Entities.Add(New devDept.Eyeshot.Entities.Point(p), System.Drawing.Color.Black)
                Next
            Next

              ' creates an internal constraint
            Dim a1 as Arc = new Arc(0, 0, 0, 5, Utility.DegToRad(120), Utility.DegToRad(220))

                a1.Regen(0.05)

            Dim segments As List(Of Segment3D) = new List(Of Segment3D)

            For i As Integer = 0 To a1.Vertices.Length - 2
                segments.Add(new Segment3D(a1.Vertices(i), a1.Vertices(i+1)))
            Next

            ' computes triangulation and fill the Mesh entity
            Dim m As Mesh = UtilityEx.Triangulate(c1.Vertices, New Point3D()() {r1.Vertices}, points, segments)
            
            model1.Entities.Add(c1, System.Drawing.Color.Red)
            model1.Entities.Add(r1, System.Drawing.Color.Blue)
            model1.Entities.Add(a1, System.Drawing.Color.Green)

            m.EdgeStyle = Mesh.edgeStyleType.Free

            ' moves the mesh up
            m.Translate(0, 0, 5)

            ' adds the mesh to the master entity array            
            model1.Entities.Add(m, System.Drawing.Color.RoyalBlue)

            ' sets the shaded display mode
            model1.DisplayMode = displayType.Shaded

            ' fits the model in the viewport
            model1.ZoomFit()

            ' sets trimetric view
            model1.SetView(viewType.Trimetric)

            ' hides origin symbol
            model1.GetOriginSymbol().Visible = False

            'refresh the viewport
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace