   M S B A M L   `   `   `  ���� L  HInstagram-Checker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Instagram_CheckerInstagram_Checker  )    #Instagram_Checker.ProxyOptionWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     1localclr-namespace:Instagram_Checker     $	��315��$	��310��  � WindowStartupLocation$  CenterScreen=� � WindowStyle$ 
ToolWindow=� � Title$% Настройки прокси��.��� $��7q� � RowDefinitions �� $	��79* ��� $
��146* ��� $	��30* �.+��� .+��� $=��7Управление ссылками на прокси��$��
5, 0, 5, 0q��� -    ��Name$ tbProxyLink��$��
5, 2, 5, 6q��� -    ��Name$ btnSuceedLink��$��Применить��$��
5, 0, 5, 0q��� $��1��
���   �$��1q�$��5q�._��� -   $ tbLinks��
������� �� 
IsReadOnly . � $��2�� � ColumnDefinitions �� $
��0.7* ��� $��* �.+��� -   $��5q�$%��Выгрузить ссылки��� $��1�� �� $��* ��� $��* �.+��� -   $��5q�$��Удалить всё���� -   $��1��$��5q�$��OK��