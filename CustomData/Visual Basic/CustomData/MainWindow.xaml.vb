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
Imports devDept.Graphics
Imports devDept.Eyeshot.Entities

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow        
        <Serializable> _
        Private Structure CustomData

            Public id As UInteger
            Public price As Single
            Public description As String

            Public Sub New(id As UInteger, price As Single, description As String)
                Me.id = id
                Me.price = price
                Me.description = description
            End Sub
        End Structure

        Public Sub New()
            InitializeComponent()

            '  model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

        End Sub

        Protected Overrides Sub OnContentRendered(e As EventArgs)
            ' hides grid
            model1.GetGrid().Visible = False

            ' Our red line
            Dim myLine As New Line(0, 0, 0, 50, 10, 0)

            ' The red line custom data
            myLine.EntityData = New CustomData(8321, 6.99F, "Steel wire")


            ' Our blue triangle
            Dim myTri As New Triangle(0, 10, 0, 40, 40, 0, _
                10, 70, 0)

            ' The blue triangle custom data
            myTri.EntityData = New CustomData(9876, 18.99F, "Plastic panel")


            ' We add both to the master entity array            
            model1.Entities.Add(myLine, System.Drawing.Color.Red)
            model1.Entities.Add(myTri, System.Drawing.Color.Blue)

            ' sets trimetric view            
            model1.SetView(viewType.Trimetric)

            ' fits the model in the viewport            
            model1.ZoomFit()

            'refresh the model control
            model1.Invalidate()

            MyBase.OnContentRendered(e)
        End Sub

        Private Sub selectButton_OnClick(sender As Object, e As RoutedEventArgs)
            If selectButton.IsChecked IsNot Nothing AndAlso selectButton.IsChecked.Value Then
                model1.ActionMode = actionType.SelectByPick
            Else
                model1.ActionMode = actionType.None
            End If
        End Sub

        Private Sub clearSelectionButton_Click(sender As Object, e As RoutedEventArgs)
            model1.Entities.ClearSelection()
            model1.Invalidate()
        End Sub

        Private Sub model1_SelectionChanged(sender As Object, e As Model.SelectionChangedEventArgs)
            For Each ent As Entity In model1.Entities
                If ent.Selected Then

                    If TypeOf ent.EntityData Is CustomData Then

                        Dim cd As CustomData = CType(ent.EntityData, CustomData)

                        MessageBox.Show(Convert.ToString("ID = " + cd.id.ToString() + System.Environment.NewLine + "Price = $" + cd.price.ToString() + System.Environment.NewLine + "Description = ") & cd.description, "CustomData")
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace