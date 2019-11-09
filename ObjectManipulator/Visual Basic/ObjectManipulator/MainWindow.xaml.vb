Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
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
Imports System.Windows.Shapes
Imports devDept.Eyeshot
Imports devDept.Eyeshot.Entities
Imports devDept.Geometry
Imports devDept.Graphics
Imports Block = devDept.Eyeshot.Block

Namespace WpfApplication1
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Private Const Assets As String = "../../../../../../dataset/Assets/"

        Private toolTip1 As New ToolTip()
        Private toolTipText As String

        Private textDragTranslateOnAxis As String = "Drag arrow to translate" & vbLf
        Private textDragTranslateOnView As String = "Drag sphere to translate" & vbLf
       	Private textDragScale As String = "Drag box to scale" & vbLf
		Private textDragScaleUniform As String = "Drag sphere to scale" & vbLf
		Private textDragRotate As String = "Drag arc to rotate"
		Private textDragRotateOnView As String = "Drag sphere to rotate"
		Private textSelectEntity As String = "Click to select" & vbLf & "Double click to set current BlockReference" & vbLf & "Right click to edit"
		Private textApplyOrCancel As String = "Double click to apply transformation" & vbLf & "Right click to cancel editing"
		Private textResetCurrent As String = "Click to deselect" & vbLf & "Double click to reset current BlockReference"

		Public Sub New()
			InitializeComponent()

             ' model1.Unlock("") ' For more details see 'Product Activation' topic in the documentation.

            ' Sets default values
            styleEnumButton.Set(model1.ObjectManipulator.StyleMode)
			ballActionEnumButton.Set(model1.ObjectManipulator.BallActionMode)

			' Add an EventHandler to the ObjectManipulator.MouseOver event to show a ToolTip when the mouse is over a part of the ObjectManipulator.
			AddHandler model1.ObjectManipulator.MouseOver, AddressOf OnObjectManipulatorMouseOver
			AddHandler model1.ObjectManipulator.MouseDrag, AddressOf HideToolTip
		End Sub

		Protected Overrides Sub OnContentRendered(ByVal e As EventArgs)
			' Get the body parts
			 Dim fileNames As String() = New String() {
                                                    Assets + "figure_Object001.eye",
                                                    Assets + "figure_Object002.eye",
                                                    Assets + "figure_Object003.eye",
                                                    Assets + "figure_Object004.eye",
                                                    Assets + "figure_Object005.eye",
                                                    Assets + "figure_Object006.eye",
                                                    Assets + "figure_Object007.eye",
                                                    Assets + "figure_Object008.eye",
                                                    Assets + "figure_Object009.eye",
                                                    Assets + "figure_Object010.eye",
                                                    Assets + "figure_Object011.eye",
                                                    Assets + "figure_Object012.eye",
                                                    Assets + "figure_Object013.eye",
                                                    Assets + "figure_Object014.eye",
                                                    Assets + "figure_Object015.eye",
                                                    Assets + "figure_Object016.eye",
                                                    Assets + "figure_Object017.eye",
                                                    Assets + "figure_Object018.eye",
                                                    Assets + "figure_Object019.eye",
                                                    Assets + "figure_Object020.eye",
                                                    Assets + "figure_Object021.eye",
                                                    Assets + "figure_Object022.eye",
                                                    Assets + "figure_Object023.eye",
                                                    Assets + "figure_Object024.eye"
                                                 }
            Dim partNames As String() = {
                                            "LeftFoot",
                                            "RightFoot",
                                            "LeftAnkle",
                                            "RightAnkle",
                                            "LeftLowerLeg",
                                            "RightLowerLeg",
                                            "LeftKnee",
                                            "RightKnee",
                                            "LeftUpperLeg",
                                            "RightUpperLeg",
                                            "Torso",
                                            "LeftShoulder",
                                            "LeftUpperArm",
                                            "LeftElbow",
                                            "LeftLowerArm",
                                            "LeftWrist",
                                            "LeftHand",
                                            "RightShoulder",
                                            "RightUpperArm",
                                            "RightElbow",
                                            "RightLowerArm",
                                            "RightWrist",
                                            "RightHand",
                                            "Head"
                                        }

			' Create a BlockReference for each body part
			Dim entities(fileNames.Length - 1) As Entity

			For i As Integer = 0 To fileNames.Length - 1

				entities(i) = CreateBlockReference(partNames(i), fileNames(i))
			Next i

			' Creates a dictionary to identify the body part index from its name
			Dim parts As New Dictionary(Of String, Integer)()

			For i As Integer = 0 To partNames.Length - 1

				parts.Add(partNames(i), i)
			Next i

			' Creates BlockReferences for the various groups of entities that form the body parts
			' and set in the EntityData proerty of each one the point to be used as the ObjectManipulator origin

			Dim brTemp, brTemp2 As BlockReference

			SetRotationPoint(entities(parts("LeftWrist")), CType(entities(parts("LeftHand")), BlockReference))
			SetRotationPoint(entities(parts("RightWrist")), CType(entities(parts("RightHand")), BlockReference))
			SetRotationPoint(entities(parts("LeftAnkle")), CType(entities(parts("LeftFoot")), BlockReference))
			SetRotationPoint(entities(parts("RightAnkle")), CType(entities(parts("RightFoot")), BlockReference))

			'Left leg                
			brTemp = BuildBlockReference("BrLeftLowerLeg", New Entity() {entities(parts("LeftFoot")), entities(parts("LeftAnkle")), entities(parts("LeftLowerLeg"))})

			SetRotationPoint(entities(parts("LeftKnee")), brTemp)

			brTemp2 = BuildBlockReference("BrLeftUpperLeg", New Entity() { entities(parts("LeftUpperLeg")), entities(parts("LeftKnee")) })

			Dim br1 As BlockReference = BuildBlockReference("BrLeftLeg", New Entity() { brTemp, brTemp2 })
			br1.EntityData = New Point3D(3.8, 44, 1.6)

			' Right leg
			brTemp = BuildBlockReference("BrRightLowerLeg", New Entity() {entities(parts("RightFoot")), entities(parts("RightAnkle")), entities(parts("RightLowerLeg"))})

			SetRotationPoint(entities(parts("RightKnee")), brTemp)

			brTemp2 = BuildBlockReference("BrRightUpperLeg", New Entity(){ entities(parts("RightUpperLeg")), entities(parts("RightKnee")) })

			Dim br2 As BlockReference = BuildBlockReference("BrRightLeg", New Entity(){ brTemp, brTemp2 })

			br2.EntityData = New Point3D(-3.8, 44, 1.6)

			' Left arm
			brTemp = BuildBlockReference("BrLeftLowerArm", New Entity() {entities(parts("LeftHand")), entities(parts("LeftWrist")), entities(parts("LeftLowerArm"))})

			SetRotationPoint(entities(parts("LeftElbow")), brTemp)

			Dim br3 As BlockReference = BuildBlockReference("BrLefArm", New Entity(){ brTemp, entities(parts("LeftElbow")), entities(parts("LeftUpperArm")), entities(parts("LeftShoulder")) })

			SetRotationPoint(entities(parts("LeftShoulder")), br3)


			' Right arm
			brTemp = BuildBlockReference("BrRightLowerArm", New Entity() {entities(parts("RightHand")), entities(parts("RightWrist")), entities(parts("RightLowerArm"))})

			SetRotationPoint(entities(parts("RightElbow")), brTemp)

			Dim br4 As BlockReference = BuildBlockReference("BrRightArm", New Entity() { brTemp, entities(parts("RightElbow")), entities(parts("RightUpperArm")), entities(parts("RightShoulder")) })

			SetRotationPoint(entities(parts("RightShoulder")), br4)

			' Creates the final BlockReference containing the whole model
			Dim brBody As BlockReference = BuildBlockReference("BrMan", New Entity() { br1, br2, br3, br4, entities(parts("Torso")), entities(parts("Head")) })

			entities(parts("Head")).EntityData = New Point3D(0, 63, .38) ' set the rotation point

			brBody.Rotate(Math.PI / 2, Vector3D.AxisX)

			model1.Entities.Add(brBody, System.Drawing.Color.Pink)

			Dim baseBox As Mesh = Mesh.CreateBox(20, 20, 10)
			baseBox.Translate(50, 50, 0)
			model1.Materials.Add(New Material("wood", New Bitmap(Assets & "Textures/Wenge.jpg")))
			baseBox.ApplyMaterial("wood", textureMappingType.Cubic, 1, 1)
			model1.Entities.Add(baseBox)

			model1.ZoomFit()

			model1.ActionMode = actionType.SelectVisibleByPick
			model1.GetViewCubeIcon().Visible = False
			model1.Rendered.ShadowMode = shadowType.None
			model1.Rendered.ShowEdges = False

			' Hide the original part when editing it
			model1.ObjectManipulator.ShowOriginalWhileEditing = False
			model1.Invalidate()

			MyBase.OnContentRendered(e)
		End Sub

		Private Function CreateBlockReference(ByVal partName As String, ByVal fileName As String) As BlockReference
			Dim readFile As New devDept.Eyeshot.Translators.ReadFile(fileName)
			readFile.DoWork()

			Dim entity As Entity = readFile.Entities(0)

			CType(entity, Mesh).NormalAveragingMode = Mesh.normalAveragingType.Averaged
			entity.ColorMethod = colorMethodType.byParent

			Dim bl As New Block(partName, Point3D.Origin)

			bl.Entities.Add(entity)
			model1.Blocks.Add(bl)

			Dim br As New BlockReference(New Identity(), partName)
			br.ColorMethod = colorMethodType.byParent
			Return br
		End Function

		Private Sub SetRotationPoint(ByVal entity As Entity, ByVal brTemp As BlockReference)
			' Saves the rotation point in the Entity data of the BlockReference

			Dim br As BlockReference = CType(entity, BlockReference)

			Dim boxMin As Point3D = Nothing, boxMax As Point3D = Nothing
			Utility.ComputeBoundingBox(Nothing, model1.Blocks(br.BlockName).Entities(0).Vertices, boxMin, boxMax)

			brTemp.EntityData = (boxMin + boxMax) / 2
		End Sub

		Private Function BuildBlockReference(ByVal newName As String, ByVal entities As IList(Of Entity)) As BlockReference
			' Creates a new BlockReference from the given list of entities
			Dim bl As New Block(newName)
			bl.Entities.AddRange(entities)

			model1.Blocks.Add(bl)

			Dim br As New BlockReference(New Identity(), newName)
			br.ColorMethod = colorMethodType.byParent
			Return br
		End Function

		Private Editing As Boolean = False

		Private Sub model1_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			If model1.GetMouseClicks(e) = 1 AndAlso e.RightButton = MouseButtonState.Pressed Then
				model1_Click()
			End If

			If model1.GetMouseClicks(e) = 2 AndAlso e.LeftButton = MouseButtonState.Pressed Then
				model1_DoubleClick()
			End If

		End Sub

		Private Sub model1_Click()
			If Editing Then
				' Cancels the ObjectManipulator editing
				model1.ObjectManipulator.Cancel()
				Editing = False
			Else
				' Starts the edit the selected parts with the ObjectManipulator
				Dim countSelected As Integer = Nothing
				Dim selectedEnt As Entity = GetSelectedEntity(countSelected)

				If countSelected = 1 Then
					Editing = True

					Dim initialTransformation As Transformation = Nothing
					Dim center As Boolean = True


					' If there is only one selected entity, position and orient the manipulator using the rotation point saved in its
					' EntityData property and its transformation

					Dim rotationPoint As Point3D = Nothing

					If TypeOf selectedEnt.EntityData Is Point3D Then
						center = False
						rotationPoint = CType(selectedEnt.EntityData, Point3D)
					End If

					If rotationPoint IsNot Nothing Then

						initialTransformation = New Translation(rotationPoint.X, rotationPoint.Y, rotationPoint.Z)
					Else

						initialTransformation = New Identity()
					End If

					' Enables the ObjectManipulator to start editing the selected objects
					model1.ObjectManipulator.Enable(initialTransformation, center)
				End If
			End If

			model1.Invalidate()
		End Sub

		Private Function GetSelectedEntity(ByRef countSelected As Integer) As Entity
			countSelected = 0
			Dim selectedEnt As Entity = Nothing

			For Each ent As Entity In model1.Entities
				If ent.Selected Then
					countSelected += 1
					selectedEnt = ent
				End If
			Next ent
			Return selectedEnt
		End Function

		Private Sub model1_DoubleClick()
			If Editing Then
				' Applies the transformation from the ObjectManipulator
				model1.ObjectManipulator.Apply()
				model1.Entities.Regen()
				Editing = False
			Else
				' Sets the selected BlockReference as current
				model1.Entities.SetSelectionAsCurrent()
			End If

			model1.Invalidate()
		End Sub

		Private Sub model1_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
			If model1.ActionMode <> actionType.Rotate AndAlso model1.ActionMode <> actionType.Zoom AndAlso model1.ActionMode <> actionType.Pan Then

				DrawTooltip(e)
			End If
		End Sub

		Private Sub Model1_OnMouseEnter(ByVal sender As Object, ByVal e As MouseEventArgs)
			model1.Focus()
		End Sub

		Private Sub model1_MouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			If model1.ActionMode <> actionType.Rotate AndAlso model1.ActionMode <> actionType.Zoom AndAlso model1.ActionMode <> actionType.Pan Then
				' force a new display of the tooltip
				toolTipText = String.Empty
				DrawTooltip(e)
			End If
		End Sub

		Private Sub DrawTooltip(ByVal e As MouseEventArgs)
			If model1.ObjectManipulator.Visible Then
				Return
			End If

			Dim entId As Integer = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)))

			Dim newString As String = String.Empty

			If entId >= 0 Then

				newString = textSelectEntity

			Else

				newString = textResetCurrent
			End If

			SetToolTipText(newString)
		End Sub

		Private Sub SetToolTipText(ByVal newString As String)
			If String.Compare(newString, toolTipText) <> 0 Then
				toolTip1.IsOpen = False
				toolTip1.Content = newString
				model1.ToolTip = toolTip1
				toolTipText = newString
				toolTip1.IsOpen = True
			End If
		End Sub

		Private Sub OnObjectManipulatorMouseOver(ByVal sender As Object, ByVal args As ObjectManipulator.ObjectManipulatorEventArgs)
			' force a new display of the tooltip
			Dim newString As String = String.Empty

			Select Case args.ActionMode
				Case ObjectManipulator.actionType.Rotate
					newString = textDragRotate

				Case ObjectManipulator.actionType.RotateOnView
					newString = textDragRotateOnView

				Case ObjectManipulator.actionType.TranslateOnAxis
					newString = textDragTranslateOnAxis

				Case ObjectManipulator.actionType.TranslateOnView
					newString = textDragTranslateOnView

				Case ObjectManipulator.actionType.Scale
					newString = textDragScale

				Case ObjectManipulator.actionType.UniformScale
					newString = textDragScaleUniform

				Case ObjectManipulator.actionType.None
					newString = textApplyOrCancel

			End Select

			SetToolTipText(newString)
		End Sub

		Private Sub HideToolTip(ByVal sender As Object, ByVal e As EventArgs)
			toolTip1.IsOpen = False
		End Sub

		Private Sub ComponentButton_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
			' hides/shows components 
			If translatingAxis.IsChecked IsNot Nothing Then
				model1.ObjectManipulator.TranslateX.Visible = translatingAxis.IsChecked.Value
				model1.ObjectManipulator.TranslateY.Visible = translatingAxis.IsChecked.Value
				model1.ObjectManipulator.TranslateZ.Visible = translatingAxis.IsChecked.Value
			End If

			If rotationButton.IsChecked IsNot Nothing Then
				model1.ObjectManipulator.RotateX.Visible = rotationButton.IsChecked.Value
				model1.ObjectManipulator.RotateY.Visible = rotationButton.IsChecked.Value
				model1.ObjectManipulator.RotateZ.Visible = rotationButton.IsChecked.Value
			End If

			If scalingButton.IsChecked IsNot Nothing Then
				model1.ObjectManipulator.ScaleX.Visible = scalingButton.IsChecked.Value
				model1.ObjectManipulator.ScaleY.Visible = scalingButton.IsChecked.Value
				model1.ObjectManipulator.ScaleZ.Visible = scalingButton.IsChecked.Value
			End If

			model1.CompileUserInterfaceElements()
			model1.Invalidate()
		End Sub

		Private Sub sizeBar_ValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
			If model1 IsNot Nothing Then
				model1.ObjectManipulator.Size = CInt(Math.Truncate(sizeBar.Value))
				model1.CompileUserInterfaceElements()
				model1.Invalidate()
			End If
		End Sub

		Private Sub showDraggedOnlyCheckBox_OnClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
			model1.ObjectManipulator.ShowDraggedItemOnly = showDraggedOnlyCheckBox.IsChecked IsNot Nothing AndAlso showDraggedOnlyCheckBox.IsChecked.Value
		End Sub

		Private Sub stepSettings_changed(ByVal sender As Object, ByVal e As EventArgs)
			If model1 Is Nothing Then
				Return
			End If

			Dim [step] As Double = Nothing
			If translationCheckBox.IsChecked IsNot Nothing AndAlso translationCheckBox.IsChecked.Value Then
				translationTextBox.IsEnabled = True
				If Double.TryParse(translationTextBox.Text, [step]) Then
					model1.ObjectManipulator.TranslationStep = [step]
				End If
			Else
				translationTextBox.IsEnabled = False
				model1.ObjectManipulator.TranslationStep = 0
			End If

			If rotationCheckBox.IsChecked IsNot Nothing AndAlso rotationCheckBox.IsChecked.Value Then
				rotationTextBox.IsEnabled = True
				If Double.TryParse(rotationTextBox.Text, [step]) Then
					model1.ObjectManipulator.RotationStep = Utility.DegToRad([step])
				End If
			Else
				rotationTextBox.IsEnabled = False
				model1.ObjectManipulator.RotationStep = 0
			End If

			If scalingCheckBox.IsChecked IsNot Nothing AndAlso scalingCheckBox.IsChecked.Value Then
				scalingTextBox.IsEnabled = True
				If Double.TryParse(scalingTextBox.Text, [step]) Then
					model1.ObjectManipulator.ScalingStep = [step]
				End If
			Else
				scalingTextBox.IsEnabled = False
				model1.ObjectManipulator.ScalingStep = 0
			End If
		End Sub

		Private Sub styleEnumButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.ObjectManipulator.StyleMode = CType(styleEnumButton.Value, ObjectManipulator.styleType)
			model1.CompileUserInterfaceElements()
			model1.Invalidate()
		End Sub

		Private Sub ballActionEnumButton_Click(ByVal sender As Object, ByVal e As EventArgs)
			model1.ObjectManipulator.BallActionMode = CType(ballActionEnumButton.Value, ObjectManipulator.ballActionType)
			model1.Invalidate()
		End Sub
	End Class


End Namespace

