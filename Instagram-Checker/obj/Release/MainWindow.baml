   M S B A M L   `   `   `  ���� L  HInstagram-Checker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Instagram_CheckerInstagram_Checker  5clr-namespace:Instagram_CheckerInstagram_Checker  "    Instagram_Checker.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     1localclr-namespace:Instagram_Checker     3Numericclr-namespace:Instagram_Checker     -     � Icon$  instagramIcon.ico�� � WindowStartupLocation$ CenterScreen=� � Title$ Instagram-Checker��$	��685��$	��900�� � 	Resources &    Instagram_Checker.Numeric_UpDown   NumericUpDown��%   &	          .��� $��5q� � ColumnDefinitions �� $��0.44* ��� $��0.22* � � RowDefinitions �� $
��1.6* ��� $
��0.6* �.+�&   System.Windows.Controls.DataGrid  -     Name$ 
dgAccounts��$��
0, 0, 0, 3q�   CanUserAddRows .     CanUserDeleteRows .  �� $��1��$��1q�
���   �$��
0, 2, 0, 0q�._��� -   	 ��Name$	 tbLog��
�������$
��Bold"�
 �� 
IsReadOnly
 . $��12(��� $��1��$��Настройки��.���� .+��� -    ��Name$ 
cbApiProxy��{�. $>��8Использовать ссылочные прокси��$��11(�$��
5, 5, 0, 0q� d� 	IsChecked$
 TrueY��� -    ��Name$ btnProxyOptions��$%��Настройки прокси��$��5q�{�. �� $%��Настройки работы��$��5q�.���  �� $��1* ��� $��1* � �� $��* ��� $��* ��� $��* ��� $��* �.+�  -     Name$ numcThreads��$��5q�   Value$	 500���� $��1��$��18, 5, 0, 0q�.���� $
��Wrap=�$)��#Количество потоков��$��10(� �� Padding$ 2q�  -   $ 	numcDelay��$��5q�$��1���� $��1��$��1��$��23, 10, 0, 0q�.���� $
��Wrap=�$%��Задержка запроса��$��10(�$ 2q�  -   $ numcDelayMail��$��5q�$��2��$ 5���� $��1��$��2��$��13, 10, 0, 0q�.���� $
��Wrap=�$��Задержка Mail��$��10(�$ 20, 2, 2, 2q��� $%��Результат работы��$��5, 15, 5, 5q�.���  �� $
��0.8* ��� $
��0.2* � �� $��* ��� $��* ��� $��* ��� $��* ��� $��* �.+��� 
��� � �.��Успешно�� -	    ��Name$ 	lbSuccess��$��1��
��� � �$��
0, 0, 6, 0q�.��0�� $��1��
����  �.��+)Ожидают подтверждения�� -
   $ lbChallenge��$��1��$��1��
����  �$��
0, 0, 6, 0q�.��0�� $��2��
���  ��.��/-Заблокировано аккаунтов�� -   $ lbBlockedAccs��$��1��$��2��
���  ��$��
0, 0, 6, 0q�.��0�� $��3��
���  ��.��)'Заблокировано прокси�� -   $ lbBlockedProxy��$��1��$��3��
���  ��$��
0, 0, 6, 0q�.��0�� $��4��$��0, 10, 0, 0q�.��64Всего просмотрено аккаунтов�� -   $ lbAllAccountsSwitched��$��1��$��4��$��0, 10, 6, 0q�.��0� $��1��$��1�� �� $
��0.7* ��� $
��0.4* �.+�� $��5q� �� $��0.75* ��� $��0.25* � �� $��* ��� $��* ��� $��* ��� $��* �.+��� .��!Потоков в работе:�� -   $ lbThreadsInWork��$��1��$��24, 4, 0, 0q�.��0�� $��1��.��(&Прокси использовано:�� -   $ lbProxyUsed��$��1��$��1��$��24, 4, 0, 0q�.��0�� $��2��
����  �.��'%Время начала работы:�� -   $ lbStartWorkingTime��$��2��$��1��
����  �.��
00:00:00�� $��3��
����  �.��-+Время окончания работы:�� -   $ lbEndWorkingTime��$��3��$��1��
����  �.��
00:00:00�� $��1��$��1��.+��� -   $ btnLoad��$%��Загрузить данные��$��10q��� -   $ btnStart��$��
Старт��$��10, 0, 10, 10q�{�.  