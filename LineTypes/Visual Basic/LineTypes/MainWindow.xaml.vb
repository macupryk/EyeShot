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
Imports devDept.Eyeshot.Entities
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

            '#Region "Frame drawing"

            Dim jointsLabel As String= "Joints"
            Dim barsLabel As String= "Bars"

            model1.Layers.Add("Joints", System.Drawing.Color.Red)
            model1.Layers.Add("Bars", System.Drawing.Color.DimGray)

            model1.Entities.Add(New Joint(-40, -20, 0, 2.5, 2), jointsLabel)
            model1.Entities.Add(New Joint(-40, +20, 0, 2.5, 2), jointsLabel)
                                                                         
            model1.Entities.Add(New Joint(+40, -20, 0, 2.5, 2), jointsLabel)
            model1.Entities.Add(New Joint(+40, +20, 0, 2.5, 2), jointsLabel)

            model1.Entities.Add(New Bar(-40, -20, 0, +40, -20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(-40, +20, 0, +40, +20, 0, 1, 8), barsLabel)

            model1.Entities.Add(New Joint(+40, 0, 40, 2.5, 2), jointsLabel)
            model1.Entities.Add(New Joint(-40, 0, 40, 2.5, 2), jointsLabel)

            model1.Entities.Add(New Bar(-40, 0, 40, +40, 0, 40, 1, 8), barsLabel)

            model1.Entities.Add(New Bar(-40, -20, 0, -40, +20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(-40, -20, 0, -40, 0, 40, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(-40, +20, 0, -40, 0, 40, 1, 8), barsLabel)

            model1.Entities.Add(New Bar(+40, -20, 0, +40, +20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+40, -20, 0, +40, 0, 40, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+40, +20, 0, +40, 0, 40, 1, 8), barsLabel)

            model1.Entities.Add(New Bar(-40, -20, 0, +40, +20, 0, 1, 8), barsLabel)

            model1.Entities.Add(New Bar(+40, -20, 0, +120, -20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+40, +20, 0, +120, +20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+40, 0, 40, +120, +20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+40, 0, 40, +120, -20, 0, 1, 8), barsLabel)
            model1.Entities.Add(New Bar(+120, +20, 0, +120, -20, 0, 1, 8), barsLabel)

            model1.Entities.Add(New Bar(+40, +20, 0, +120, -20, 0, 1, 8), barsLabel)

            model1.Entities.Add(New Joint(120, -20, 0, 2.5, 2), jointsLabel)
            model1.Entities.Add(New Joint(120, +20, 0, 2.5, 2), jointsLabel)

            '#End Region

            ' adds cable layer and entities                        
            model1.LineTypes.Add("Dash", New Single() {2, -1})
            Dim cableLayer As String = "Cable"
            model1.Layers.Add(cableLayer, System.Drawing.Color.Teal, "Dash")

            Dim xz As New Plane(New Point3D(110, 0, -10), Vector3D.AxisX, Vector3D.AxisZ)

            Dim l1 As New Line(-60, 0, -10, 120, 0, -10)
            Dim a1 As New Arc(xz, New Point3D(120, 0, -15), 5, 0, Math.PI / 2)
            Dim l2 As New Line(125, 0, -15, 125, 0, -50)

            model1.Entities.AddRange(New Entity() {l1, a1, l2}, cableLayer)

            ' adds pulley layer and entities            
            Dim pulleyLayer As String = "Pulley"
            model1.Layers.Add(pulleyLayer, System.Drawing.Color.Magenta)

            Dim c1 As New Circle(xz, New Point3D(120, 0, -15), 5)
            Dim c2 As New Circle(xz, New Point3D(120, 0, -15), 7)
            Dim c3 As New Circle(xz, New Point3D(120, 0, -15), 2)

            model1.Entities.AddRange(New Entity() {c1, c2, c3}, pulleyLayer)

            ' axes on default layer with their own line style
            Dim l3 As New Line(110, 0, -15, 130, 0, -15)

            model1.LineTypes.Add("DashDot", New Single() {5, -1.5F, 0.25F, -1.5F})            

            l3.LineTypeMethod = colorMethodType.byEntity
            l3.LineTypeName = "DashDot"

            Dim l4 As New Line(120, 0, -5, 120, 0, -25)
            l4.LineTypeMethod = colorMethodType.byEntity
            l4.LineTypeName = "DashDot"

            model1.Entities.Add(l3)
            model1.Entities.Add(l4)

            model1.ZoomFit()

            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub
    End Class
End Namespace


