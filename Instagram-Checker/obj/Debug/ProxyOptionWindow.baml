   M S B A M L   `   `   `  ����L  HInstagram-Checker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Instagram_CheckerInstagram_Checker  5   	   )    #Instagram_Checker.ProxyOptionWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:Instagram_Checker     5   	   $	��315��5   	   $	��310��6     � WindowStartupLocation$  CenterScreen=�5	   	    � WindowStyle$ 
ToolWindow=�5
   	    � Title$% Настройки прокси��5   	   .��5      � $��7q�6    � RowDefinitions 5   
   �� 5      $	��79* �6   �� 5      $
��146* �6   �� 5      $	��30* �6   .+�5   
   �� .+�5      �� $=��7Управление ссылками на прокси��6   $��
5, 0, 5, 0q�5      �� 5      -   5         ��Name$ tbProxyLink��5      $��
5, 2, 5, 6q�5      �� 5      -   5         ��Name$ btnSuceedLink��5      $��Применить��5      $��
5, 0, 5, 0q�5      5   3   5      �� 5   
   $��1��6   
���   �$��1q�62   $��5q�6F   ._�5      �� -   5        $ tbLinks��5      
������� �� 
IsReadOnly . 5      5       � 5!   
   $��2��6    � ColumnDefinitions 5"      �� 5#      $
��0.7* �6#   �� 5$      $��* �6#   .+�5&      �� -   5        $��5q�5&      $%��Выгрузить ссылки��5'      5(      � 5)      $��1��6    5*      �� 5+      $��* �6'   �� 5,      $��* �6'   .+�5.      �� -   5        $��5q�5.      $��Удалить всё��5/      50      �� 51      -   5        $��1��51      $��5q�52      $��OK��53      54      55      56      57      58      