   M S B A M L   `   `   `  ����L  HRevit_Plugin_Rick, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:Revit_Plugin_Rick.UIRevit_Plugin_Rick.UI  5      .    (Revit_Plugin_Rick.UI.SearchCommandWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:Revit_Plugin_Rick.UI     5      -   5        $	��450��5      $	��650��6     � Title$  command finder��6'    � 	Resources 5
      7   1Revit_Plugin_Rick.UI.BitmapToImageSourceConverter    BitmapToImageSourveConverter��%   &	          5   
   6-   5      .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��0.7* �6   �� 5      $��* �6   �� 5      $	��10* �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��命令搜索：��6!   $��
20 10 10 0q�6.   $��13(�6B   $
��Bold"�6P    �� Padding$	 5 0q�6b   �� 5   
   -   5         ��Name$ search_input��5      $��1��5      $��15(�5      $
��10 5q�5      $��	20 5 20 0q�6   ��5      �� *finder.Search_input+ �� Mode$ TwoWay=�5      �� 5   
   -   5         ��Name$ command_list��5      $��2��6   $��	20 1 20 5q�65   ��5!      �� 5"       �� 
TargetType" ��6   .�5#      -   5        5$      ��5&      �� 5'      .R�5(      ��  �� Orientation$ 
Horizontal=�6!   .+�5)      �� $��30��6D   $��30��6O   $��5q�6[   ��6    �� 	 �� Path$	 ImageSource�6[   �� 5*      $��Center=�6?   ��6$   �� $
	 Name�6?   5+      5,      5.      � 5/   
   $��3��6   $��40��6   $��	20 5 20 5q�6(   
 � ColumnDefinitions
 50      �� 51      $��2* �6#   �� 52      $��* �6#   �� 53      $��* �6#   .+�56      �� -   5         ��Name$
 exit��56   0   $��2��6   $��5q�6%   $��取消��6Q   57      58      59      