   M S B A M L   `   `   `  ���� E  APaperSpace, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WpfApplication1WpfApplication1       WpfApplication1.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     i edevDept.Eyeshot.Control.Wpf.v12, Version=12.0.520.0, Culture=neutral, PublicKeyToken=5385d83d2fb4d7d9=ddes2http://schemas.devdept.com/winfx/2008/xaml/control  b ^devDept.Graphics.Wpf.v12, Version=12.0.520.0, Culture=neutral, PublicKeyToken=f3cd437f0d8061b5>ddgr3http://schemas.devdept.com/winfx/2008/xaml/graphics  =ddds2http://schemas.devdept.com/winfx/2008/xaml/dataset  3wpfApplication1clr-namespace:WpfApplication1    � Title$  
MainWindow��$
��1030��$	��700�� +� FocusedElement ��  �� ElementName$ model1�� � 	Resources  �� 
TargetType �� BasedOn �� Property �� Value��%~  '��       7��'	� p     7�'�� �     7���� " ��8   .���  � ��$ 75����  � ��$ 24����  � ��$	 Top=���  � ��$ 0,5,5,0q��� " 	�8   .���  � ��$ 3q���  � �� �	#FFFAFAFA��  � ��$ 2.5 2.5 2.5 2.5q��� " ��8   .���  � {� .  ��  � ��$ 75����  � ��$ Right=���  �@��TextAlignment$ Right=�.��]� ��. $��5q�.+�]� $��Bottom=�$
��Auto��.+��� $
��Left=�$
��Left=� �� Orientation$ 
Horizontal=�.+��� -   $��
openButton��$
��Open���� -   $��
saveButton��$
��Save���� -   $��importButton��$��Import���� -   $��exportButton��$��Export���� -   $��explodeViewsCheckBox��$��Center=�$��0,5,0,0q��� -   $��explodeViewsLabel��$��Explode views��$��0,5,0,0q�]� $��Right=�$��5q�.+�� -   	 �Name$	 progressBar��$	��150��$
��Auto��
 � Minimum$
 0Q� � Maximum$	 100Q� � Value$ 0Q�]� $��Right=�.+��� $	��Top=�$��5 0 0 0q�.+��� $��Format��.���� -    ��Name$ comboBoxFormat��$��0 0 0 0q� �� SelectedIndex$ 1��$��147.5��.@��� $��Bench�� �� 
IsSelected . �� $
��Desk���� $��Dimensions (mm)��.���� .+�]� $��0 3 0 0q�.+��� $
��Left=�$��Width���� -	   $ comboBoxWidth��$��0 0 0 0q�$ 0��$��70��$��Right=�.@��� $
��1600�� . �� $
��1800���� $
��2000���� $
��2200��]� $��0 3 0 0q�.+��� $
��Left=�$��Depth���� -
   $ comboBoxDepth��$��0 0 0 0q�$ 0��$��70��$��Right=�.@��� $	��800�� . �� $	��900���� $
��1000���� $
��1200��]� $��0 3 0 0q�.+��� $
��Left=�$��Heigth���� -   $ comboBoxHeigth��$��0 0 0 0q�$ 0��$��70��$��Right=�.@��� $	��740�� . �� $	��750���� $	��760���� $	��780���� $	��800���� $��Veneer��.���� -   $ comboBoxVeneer��$��0 0 0 0q�$ 0��$��147.5��.@��� $��Oak Bordeaux bright�� . �� $��Lindberg oak���� $��Lambrate���� $��Oak dark���� $��
Oak Torino���� $��Sonoma oak gray���� $��Paint color��.���� $
��Left=�$ 
Horizontal=�.+�	� -    	�Name$ greenColorRadioButton��
���/���$��32��$��32�� d� 	IsChecked$
 TrueY�	� -   $ orangeColorRadioButton��
��� ���$��32��$��32��	� -   $ blueColorRadioButton��
����·�$��32��$��32��	� -   $ pinkColorRadioButton��
�������$��32��$��32���� $��End caps color��.���� $
��Left=�$ 
Horizontal=�.+�	� -   $ blackColorRadioButton��
���   �$��32��$��32��$
 TrueY�	� -   $ whiteColorRadioButton��
�������$��32��$��32���� $��Weight (kg)��$��0 5 0 0q�.���� .+�]� $��0 3 0 0q�.+��� $
��Left=�$��Steel���� -   $��steelTextBox��$��Right=�$��0��]� $��0 3 0 0q�.+��� $
��Left=�$
��Wood���� -   $��woodTextBox��$��Right=�$��0��]� $��0 3 0 0q�.+��� $
��Left=�$��Plastic���� -   $��plasticTextBox��$��Right=�$��0��]� $��0 3 0 0q�.+��� $
��Left=�$��Total���� -   $��totalTextBox��$��Right=�$��0���� $��Bottom=�$��Bottom=�$��5 0 0 5q�.+��� $��Active Object��.���� -   $ comboBoxActiveObject��$��0 0 0 0q�$ 0��$��147.5��.@��� $��Assembly�� . �� $��Frame���� $��Holder���� $	��Top���� $
��Foot���� -   $��tabControl1��$��0,0,0,5q�$
��Left=�$
��auto��.@��� -   $��tabItemModel��$��Model��.��]� ��. $��5q�.+�  devDept.Eyeshot.Model  -   $��model1��   	Viewports   devDept.Eyeshot.Viewport     OriginSymbol "  devDept.Eyeshot.OriginSymbol  *  $devDept.Eyeshot.CoordinateSystemBase   Lighting .     CoordinateSystemIcon *  $devDept.Eyeshot.CoordinateSystemIcon   .     ViewCubeIcon "  devDept.Eyeshot.ViewCubeIcon     Lighting .     
Background )  #devDept.Graphics.BackgroundSettings     	StyleMode$ Solid=�   TopColor
 �����   Grids   devDept.Eyeshot.Grid     Step$ 10Q�   Lighting .     ToolBars 	  devDept.Eyeshot.ToolBar	   	  Buttons '
  !devDept.Eyeshot.HomeToolBarButton
  2  ,devDept.Eyeshot.MagnifyingGlassToolBarButton  -  'devDept.Eyeshot.ZoomWindowToolBarButton  '  !devDept.Eyeshot.ZoomToolBarButton  &   devDept.Eyeshot.PanToolBarButton  )  #devDept.Eyeshot.RotateToolBarButton  *  $devDept.Eyeshot.ZoomFitToolBarButton  �� -   $��tabItemDrawings��$��Drawings��.��)   #WpfApplication1.DrawingsUserControl  -      Name$  drawingsUserControl1��