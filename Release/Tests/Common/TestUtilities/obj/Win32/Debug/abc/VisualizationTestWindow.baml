   M S B A M L   `   `   `  ����  TestUtilities+    %TestUtilities.VizualizationTestWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Viz Test Window��5   	   $	��580��6!   $	��500��6.    � Left$ 0��6:   
 � Top$ 0��6C   .��5      �  � RowDefinitions 5   
   �� 5      $��2* �6   �� 5      $��1* �6   .+�5   
   �� -   5        $��label4��5   j   $��0��6   $��Graphic:��6   $��28��60   $
��Left=�6<   $��	10,12,0,0q�6W   $	��Top=�6x   �� 5   
   -   5        $��content��5   T   $��0��6   $
��Left=�6&   $��	14,38,0,0q�6A   $	��Top=�6c   
���   �$��4q�6�   �� 5   
   -   5        $��label1��5   v   $��1��6   $��Expected Description:��6   $��28��6=   $
��Left=�6I   $��10,0,0,0q�6d   $	��Top=�6�   �� 5   
   -   5        $��blockDescription��5   [   $��1��6   $��45��6!   $
��Left=�6-   $��	14,30,0,0q�6H   $��Description��6s   $	��Top=�6�   $	��441��6�   ���	#2B0000FF$
��Wrap=�6�   $
��Bold"�6�   6�   �� 5   
   -   5        $��	buttonYes��5   v   $��1��6   $��Matches Description��6   $��26��6<   $
��Left=�6H   $��	14,85,0,0q�6c   $	��Top=�6�   $	��178��6�    �� 	IsDefault . 6�   �� 5   
   -   5        $��buttonNo��5   m   $��1��6   $��	Different��6   $��26��62   $��Right=�6>   $��	0,85,12,0q�6Z   $	��Top=�6}   $	��182��6�   6�   �� 5   
   -   5        $��labelDescribeDifference��5   w   $��1��6   $��Describe Difference:��6   $��23��6<   $
��Left=�6H   $��
14,110,0,0q�6c   $	��Top=�6�   $	��170��6�   �� 5   
   -	   5        $��textDifference��5   Z   $��1��6   $��24��6   $
��Left=�6+   $��
14,140,0,0q�6F   $	��Top=�6p   $	��442��6�    �� TextWrapping$
 Wrap=�6�   6�   5      5      