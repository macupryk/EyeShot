Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Windows.Input
Imports devDept.CustomControls
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities

Namespace WpfApplication1
    Friend Class MyModel
        Inherits Model

        Public duplicatedBlockDetected As Boolean
        Private WarningMessage As String = "Multiple references to the same block at root level. Limitation on nested instance settings."
        'was the last key pressed the right mouse button?
        Private lastDownWasRight As Boolean = False

        Public Sub New()
            Entities = New MyEntityList()
        End Sub

        Protected Overrides Sub OnMouseUp(ByVal e As MouseButtonEventArgs)
            ' we avoid mouse up actions for the right mouse button click
            ' becuse we need that button just to for the ContextMenu
            If e.RightButton <> MouseButtonState.Released OrElse Not lastDownWasRight Then
                MyBase.OnMouseUp(e)
            End If
        End Sub

        Protected Overrides Sub OnMouseDown(ByVal e As MouseButtonEventArgs)
            ' we avoid mouse down actions for the right mouse button click
            ' becuse we need that button just to for the ContextMenu
            lastDownWasRight = True
            If e.RightButton <> MouseButtonState.Pressed Then
                lastDownWasRight = False
                MyBase.OnMouseDown(e)
            End If
        End Sub

        Protected Overrides Sub DrawOverlay(ByVal data As DrawSceneParams)
            ' display warning message
            If duplicatedBlockDetected AndAlso Not Entities.IsOpenCurrentBlockReference Then
                DrawText(Size.Width - 5, 5, WarningMessage, New Font(System.Drawing.FontFamily.GenericSerif, 1, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel), Color.Red, Color.FromArgb(127, Color.White), ContentAlignment.BottomRight)
            End If
        End Sub
    End Class

    Friend Class MyEntityList
        Inherits EntityList

        ' The tree of the current EntityList assembly
        Friend assemblyTree As AssemblyTreeView

        Public Overrides Sub Paste()
            MyBase.Paste()
            CheckDuplicatedBlockReferences()

            ' if there is an AssemblyBrowser Tree associated, then update the tree
            If assemblyTree IsNot Nothing Then
                assemblyTree.PopulateTree(Me)
            End If
        End Sub

        Public Overrides Sub AddRange(ByVal collection As IEnumerable(Of Entity))
            MyBase.AddRange(collection)
            CheckDuplicatedBlockReferences()
        End Sub

        Public Overrides Sub Add(ByVal entity As Entity)
            MyBase.Add(entity)
            CheckDuplicatedBlockReferences()
        End Sub

        Public Overrides Function Remove(ByVal entity As Entity) As Boolean
            Dim remove_Renamed As Boolean = MyBase.Remove(entity)
            CheckDuplicatedBlockReferences()
            Return remove_Renamed
        End Function

        Public Overrides Sub RemoveRange(ByVal index As Integer, ByVal count As Integer)
            MyBase.RemoveRange(index, count)
            CheckDuplicatedBlockReferences()
        End Sub

        ''' <summary>
        ''' Checks if there are multiple references to the same block in the current EntityList to display an error message on the Model.
        ''' </summary>
        Private Sub CheckDuplicatedBlockReferences()
            If CurrentBlockReference IsNot Nothing Then
                Return
            End If

            Dim blocksNames As New HashSet(Of String)()
            CType(environment, MyModel).duplicatedBlockDetected = False
            For Each entity As Entity In Me
                If TypeOf entity Is BlockReference Then
                    Dim br As BlockReference = CType(entity, BlockReference)
                    If blocksNames.Contains(br.BlockName) Then
                        CType(environment, MyModel).duplicatedBlockDetected = True
                        Exit For
                    End If
                    blocksNames.Add(br.BlockName)
                End If
            Next entity
        End Sub
    End Class
End Namespace
