   M S B A M L   `   `   `  ���� H  DDraftingDemo, Version=12.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WpfApplication1WpfApplication1  V RdevDept.CustomControls.Wpf, Version=12.0.0.0, Culture=neutral, PublicKeyToken=nullcHclr-namespace:devDept.CustomControls;assembly=devDept.CustomControls.WpfdevDept.CustomControls      WpfApplication1.MainWindow   X TXceed.Wpf.Toolkit, Version=3.4.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     i edevDept.Eyeshot.Control.Wpf.v12, Version=12.0.520.0, Culture=neutral, PublicKeyToken=5385d83d2fb4d7d9=ddes2http://schemas.devdept.com/winfx/2008/xaml/control  b	 ^devDept.Graphics.Wpf.v12, Version=12.0.520.0, Culture=neutral, PublicKeyToken=f3cd437f0d8061b5>ddgr3http://schemas.devdept.com/winfx/2008/xaml/graphics 	 =ddds2http://schemas.devdept.com/winfx/2008/xaml/dataset 	 3wpfApplication1clr-namespace:WpfApplication1 	 WwinformsHclr-namespace:devDept.CustomControls;assembly=devDept.CustomControls.Wpf 	 -     � Title$  Drafting Demo��$
��Auto��$
��Auto�� � WindowStartupLocation$ CenterScreen=� � Icon$ drafting.ico�� +� FocusedElement ��  �� ElementName$ model1�� � 	Resources  �� 
TargetType �� BasedOn �� Property	 �� Value   ToggleButtonStyle  TurboToggleButtonStyle
 �� 
TargetType �� Triggers P� Property P� Value��%e  '��       7��&	  p     7d�&	 �     7   '�� �    7���� " ��8   .���  � ��$	 24����  � ��$	 80����  � ��$		 Top=���  � ��$	 5 3 0 0q��� " d�8   .���  � ��$	 24����  � ��$	 80����  � ��$		 Top=���  � ��$	 5 3 0 0q��� " d�8   .���  � ��	 �� "
 d�.R��� #��z��#��z��$��1q�._��� $��Center=�$��Center=� P�  �@d�	IsChecked$
 TrueY�. ���  � ��
	 ������ " ��8   .���  � ��$	 64����  �@��TextAlignment$	 Right=���  �@��Padding$	 0 0 3 0q���  � ��
	 �   ���  � ��$	 0��.��]� ��. $��5q�.+��� $��Bottom=���Z� .@��� .���� -    ��Name$ mainStatusLabel��$f��`Middle Mouse Button = Rotate, Ctrl + Middle = Pan, Shift + Middle = Zoom, Mouse Wheel = Zoom +/-��$��Stretch=��� $��Stretch=�.����  �� Orientation$ 
Horizontal=�$��Right=�.+��� 
������.+��� -    ��Name$  rendererVersionStatusLabel�� � ToolTip$ Renderer Version���� ���	#FFF5F5A9$��5 0 0 0q�.+��� -   $ selectedCountStatusLabel��$ Selected count���� ���	#FFA9EFF5$��5 0 0 0q�.+��� -   $ addedCountStatusLabel��$ Added to selection count���� ���	#FFF5A9AF$��5 0 0 0q�.+��� -   $ removedCountStatusLabel��$" Removed from selection count��]� -    ]�Name$ 
rightPanel��$	��Top=���. .+��� $��Right=�$
��Auto=�.���� $��Right=�$��5 0 0 0q�$	��350��.+�9� $��Drafting��.��� .+��� -    ��Name$ pointButton��$��Points���� -	   $ 
lineButton��$
��Line���� -
   $ plineButton��$��PolyLine���� -   $ circleButton��$��Circle���� -   $ 	arcButton��$	��Arc���� -   $ splineButton��$��Spline���� -   $ ellipseButton��$��Ellipse���� -   $ ellipticalArcButton��$��EllipticalArc���� -   $ compositeCurveButton��$��	Composite���� -   $ 
textButton��$
��Text���� -   $ tangentsButton��$��Tangents�� J� RenderTransformOrigin$ 0.485,0.609&�9� $��Dimensioning��$��0 5 0 0q�.��� .+��� -   $ linearDimButton��$��Linear���� -   $ alignedDimButton��$��Aligned���� -   $ radialDimButton��$��Radial���� -   $ diametricDimButton��$��	Diametric���� -   $ angularDimButton��$��Angular���� -   $ ordinateHorizontalButton��$��Ordinate H.���� -   $ ordinateVerticalButton��$��Ordinate V.���� -   $ leaderButton��$��Leader��9� $��Snapping��$��0 5 0 0q�.���  � RowDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$
��Left=�$ 
Horizontal=�.+�d� -    d�Name$ objectSnapCheckBox��#��[  $��Object Snap��d� -   $ gridSnapCheckBox��#��[  $��	Grid Snap���� $��1��$
��Left=�$ 
Horizontal=�$��5 10 0 0q�.+��� -    ��Name$ 	snapPanel��{�.  $
��Left=�$ 
Horizontal=�.+�	� -    	�Name$ endRadioButton��$	��End��	� -   $ midRadioButton��$��10 0 0 0q�$	��Mid��	� -    $ cenRadioButton��$��10 0 0 0q�$	��Cen��	� -!   $ quadRadioButton��$��10 0 0 0q�$
��Quad��	� -"   $ pointRadioButton��$��10 0 0 0q�$��Point��9� $��	Hide/Show��$��0 5 0 0q�.��� .+�d� -#   $ showOriginButton��#��[  $��Origin�� d� 	IsChecked$
 TrueY�d� -$   $ showExtentsButton��#��[  $��Extents��d� -%   $ showVerticesButton��#��[  $��Vertices��d� -&   $ showGridButton��#��[  $
��Grid��$
 TrueY�9� $��	Selection��$��0 5 0 0q�.��� .+��� -'    ��Name$ selectionComboBox��$	��164��$��24��$��5 3 0 0q�$	��Top=�.@��� $��by Pick���� $��by Box���� $��
by Polygon���� $��by Box Enclosed���� $��by Polygon Enclosed���� $��Visible by Pick Dynamic��d� -(   $ selectCheckBox��#��[  $��Select���� -)   $ clearSelectionButton��$��Clear���� -*   $ invertSelectionButton��$��Invert���� -+   $ groupButton��$��Group��9� $��Editing��$��0 5 0 0q�.���  �� $
��Auto ��� $
��Auto �.+�� $��0��$
��Left=� � Orientation$ 
Horizontal=�.+��� -,   $ deleteButton��$��Delete���� --   $ explodeButton��$��Explode���� -.   $ mirrorButton��$��Mirror���� -/   $ offsetButton��$��Offset���� -0   $ 
trimButton��$
��Trim���� -1   $ extendButton��$��Extend���� -2   $ filletButton��$��Fillet���� -3   $ chamferButton��$��Chamfer���� -4   $ 
moveButton��$
��Move���� -5   $ rotateButton��$��Rotate���� -6   $ scaleButton��$��Scale���� $��1��$
��Left=�$ 
Horizontal=�$��0 5 0 0q�.+��� $&�� Fillet Radius / Chamfer Distance���� -7    ��Name$ filletTextBox��$��62��$��18��$
��10.0��9� $"��Inspection / Mass Properties��$��0 5 0 0q�.��� .+�d� -8   $ pickVertexButton��#��[  $��Pick Vertex���� -9   $ 
areaButton��$
��Area���� -:   $ 
dumpButton��$
��Dump��d� -;   $ showCurveDirectionButton��#��[  $��	Show Dir.��9� $��Vector��$��0 5 0 0q�.��� .+��� -<   $  vectorCopyToClipbardButton��$
��Copy���� -=   $ vectorSaveButton��$
��Save���� ->   $ printPreviewButton��$��Print Prev.���� -?   $ printButton��$��Print���� -@   $ pageSetupButton��$��1��$
��Left=�$��
Page Setup��9� $��Miscellaneous��$��0 5 0 0q�.��� .+��� -A   $ 
openButton��$
��Left=�$
��Open���� -B   $ 
saveButton��$
��Left=�$
��Save���� -C   $ importButton��$
��Left=�$��Import���� -D   $ exportButton��$
��Left=�$��Export���� -E   $ websiteButton��$
��Left=�$��Website��d� -F   $ turboButton��#��[  $��Turbo��$
 TrueY�]� $��Bottom=���. $	��140��$��	-1 5 -1 0q�.+�]� $��0����.  � UseLayoutRounding . .+�7  1System.Windows.Forms.Integration.WindowsFormsHost  $	��Top=�   Child. ,  &devDept.CustomControls.TableTabControl  -G   tableTabControl��  WpfApplication1.MyModel  -H     Name$ model1��   WpfApplication1.MyModel    	MouseDown$  Model1_OnMouseDown��$	��Top=�  devDept.Eyeshot.Model!   	Viewports!   devDept.Eyeshot.Viewport  "   InitialView$	" Top=�#   Camera#   devDept.Eyeshot.Camera  $   
ZoomFactor$$ 5Q�%   FocalLength$% 50Q�&   Distance$& 210.31923998986298Q�'   Rotation^
 ZdevDept.Geometry.v12, Version=12.0.520.0, Culture=neutral, PublicKeyToken=9440825e8b4733bc5 
 /devDept.Geometry.Converters.QuaternionConverter$O' I0.12940952255126, 0.224143868042013, 0.482962913144534, 0.836516303737808 (   Target2	 
 ,devDept.Geometry.Converters.Point3DConverter$( 0, 0, 0	 )   OriginSymbol) "
  devDept.Eyeshot.OriginSymbol
  *  $devDept.Eyeshot.CoordinateSystemBase*   Lighting* .  +   CoordinateSystemIcon+ *  $devDept.Eyeshot.CoordinateSystemIcon  * .  ,   
Background, ) 	 #devDept.Graphics.BackgroundSettings  -   	StyleMode$- Solid=�.   TopColor
. �����/   Grids/   devDept.Eyeshot.Grid  0   Step$0 10Q�1   Lighting1 .  2   ToolBars2   devDept.Eyeshot.ToolBar  3   Position$3 HorizontalTopCenter=�4   Buttons4 '  !devDept.Eyeshot.HomeToolBarButton  2  ,devDept.Eyeshot.MagnifyingGlassToolBarButton  -  'devDept.Eyeshot.ZoomWindowToolBarButton  '  !devDept.Eyeshot.ZoomToolBarButton  &   devDept.Eyeshot.PanToolBarButton  )  #devDept.Eyeshot.RotateToolBarButton  *  $devDept.Eyeshot.ZoomFitToolBarButton  �� -I   $ button��$��Button��$	��215��$��0q�$	��Top=�$��75��