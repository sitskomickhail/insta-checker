﻿using FileLibrary;
using Instagram_Checker.BLL;
using InstaLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Instagram_Checker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model _model;
        private ProxyOptionWindow _proxyWindow;
        ObservableCollection<ShowCollection> _grid;
        private Color _color;
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);


        public MainWindow()
        {
            InitializeComponent();
            _model = new Model();
            logging += ShowLog;
            _grid = new ObservableCollection<ShowCollection>();
            dgAccounts.ItemsSource = _grid;
        }

        private void ShowLog(string tmp, Log log)
        {
            this.Dispatcher.Invoke(() => tbLog.Text = tbLog.Text + log + '\n');
        }


        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            int threadsCount = numcThreads.Value;
            int splitCount = numcAccsInThread.Value;

            if (threadsCount > 0 && splitCount > 0)
            {


                BackgroundWorker controlWorker = new BackgroundWorker();
                controlWorker.DoWork += ControlWorker_DoWork;
                controlWorker.RunWorkerAsync();

                btnLoad.IsEnabled = false;
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Start check info", UserName = null });

                _model.InitProxy((bool)cbApiProxy.IsChecked ? true : false, tbProxyKey.Text);

                int[] obj = new int[2] { threadsCount, splitCount };

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Load_DoWork;
                worker.RunWorkerCompleted += Load_RunWorkerCompleted;
                worker.RunWorkerAsync(obj);
            }
            else
                MessageBox.Show("Количество потоков или аккаунтов \nв потоке не может быть меньше 1", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (numcDelay.Value > 0)
            {
                if (numcDelay.Value < 6)
                {
                    MessageBoxResult res = MessageBox.Show("Маленькая задержка может привести к некорректной\nотправке запросов. Вы действительно хотите продолжить?", "Внимание",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.No)
                        return;
                }

                btnStart.IsEnabled = false;

                object[] objs = new object[2] { tbProxyKey.Text, numcDelay.Value };

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Start_DoWork;
                worker.RunWorkerCompleted += Start_RunWorkerCompleted;
                worker.RunWorkerAsync(objs);

                BackgroundWorker gridWorker = new BackgroundWorker();
                gridWorker.DoWork += GridWorker_DoWork;
                gridWorker.RunWorkerAsync();

                BackgroundWorker controlWorker = new BackgroundWorker();
                controlWorker.DoWork += ControlWorker_DoWork;
                controlWorker.RunWorkerAsync();
            }
            else
                MessageBox.Show("Задержка не может быть отрицательным числом\nПожалуйста измените её значение", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ControlWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_model.IsProgramComplitlyEnded)
            {
                Dispatcher.Invoke(() => 
                {
                    lbThreadsInWork.Content = Process.GetCurrentProcess().Threads.Count.ToString();
                });
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    lbAllAccountsSwitched.Content = _model.AccsSwitched.ToString();
                });
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    lbProxyUsed.Content = _model.ProxySwitched.ToString();
                });
                Thread.Sleep(500);
                Dispatcher.Invoke(() =>
                {
                    lbBlockedProxy.Content = _model.ProxyBlocked.ToString();
                });
                Thread.Sleep(500);
                Dispatcher.Invoke(() =>
                {
                    lbBlockedAccs.Content = _model.AccsBlocked.ToString();
                });
            }
        }

        private void dgAccounts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow item = e.Row as DataGridRow;
            var col = e.Row.Item as ShowCollection;
            if (item != null && col != null)
            {
                item.Background = new SolidColorBrush(_color);
            }
            else
                item.Background = new SolidColorBrush(Colors.White);
        }

        private void GridWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int pos = 1;

            int countChallenge = 0;
            int countSuccess = 0;
            DateTime time = DateTime.Now;
            while (!_model.IsProgramComplitlyEnded)
            {
                if (DateTime.Now.Minute - time.Minute >= 5)
                {
                    logging.Invoke("EasyLog.log", new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Update proxy... updated = {_model.GetProxy.CountProxy}", UserName = null });

                }

                lock (_model.locker)
                {
                    if (_model.AccountInfoDataSet_Required.Count > 0)
                    {
                        string[] str = _model.AccountInfoDataSet_Required[0].Split(':');
                        _model.AccountInfoDataSet_Required.Remove(_model.AccountInfoDataSet_Required[0]);
                        _color = Colors.LightBlue;
                        Dispatcher.Invoke(() =>
                        {
                            _grid.Add(new ShowCollection() { ID = $"{pos}", Login = str[0], Password = str[1], Status = "Ожидается подтверждение" });
                            dgAccounts.ItemsSource = _grid;
                            countChallenge++;
                            lbChallenge.Content = countChallenge.ToString();
                        });
                        pos++;
                    }
                    if (_model.AccountInfoDataSet_Success.Count > 0)
                    {
                        string[] str = _model.AccountInfoDataSet_Success[0].Split(':');
                        _model.AccountInfoDataSet_Success.Remove(_model.AccountInfoDataSet_Success[0]);
                        _color = Colors.LightGreen;
                        Dispatcher.Invoke(() =>
                        {
                            _grid.Add(new ShowCollection() { ID = $"{pos}", Login = str[0], Password = str[1], Status = "Успешно", Email = str[2], EmailPassword = str[3] });
                            dgAccounts.ItemsSource = _grid;
                            countSuccess++;
                            lbSuccess.Content = countSuccess.ToString();
                        });
                        pos++;
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void Start_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Програма успешно завершила свою работу");
            btnLoad.IsEnabled = true;
        }
        private void Start_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] objs = (object[])e.Argument;

            string key = objs[0].ToString();
            int delay = (int)objs[1];

            _model.CheckAllAccounts(delay);
            DateTime time = DateTime.Now;

            while (!_model.IsProgramComplitlyEnded)
            {
                Thread.Sleep(10000);
                if (DateTime.Now.Minute - time.Minute > 5 || _model.NeedMoreProxy == true)
                {
                    _model.UpdateProxy(key);
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Update proxy... updated = {_model.GetProxy.CountProxy}", UserName = null });
                    time = DateTime.Now;
                    _model.NeedMoreProxy = false;
                }
                else if (DateTime.Now.Hour > time.Hour)
                {
                    time = DateTime.Now;
                }
            }
        }

        private void Load_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }
        private void Load_DoWork(object sender, DoWorkEventArgs e)
        {
            int threadsCount = ((int[])e.Argument)[0];
            int splitCount = ((int[])e.Argument)[1];

            _model.InitAccounts();
            _model.InitAccountsMail();

            int countMail = 0;
            int countPrx = 0;
            int countAcc = 0;
            while (true)
            {
                if (_model.IsAccountInited && countAcc == 0)
                {
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Accounts are ready ({_model.GetAccounts.CountUsers})", UserName = null });
                    countAcc++;
                }

                if (_model.IsProxyInited && countPrx == 0)
                {
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Proxy are ready ({_model.GetProxy.CountProxy})", UserName = null });
                    countPrx++;
                }

                if (_model.IsMailsReady && countMail == 0)
                {
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Mail accounts are ready ({_model.GetAccountsMail.CountMails})", UserName = null });
                    countMail++;
                }

                if (_model.IsAccountInited && _model.IsProxyInited && _model.IsMailsReady && countMail == 1 && countPrx == 1 && countAcc == 1)
                    break;
            }

            if (countPrx == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Proxy are ready ({_model.GetProxy.CountProxy})", UserName = null });
            else if (countAcc == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Accounts are ready ({_model.GetAccounts.CountUsers})", UserName = null });
            else if (countMail == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Mail accounts are ready ({_model.GetAccountsMail.CountMails})", UserName = null });


            _model.InitObjects(threadsCount, splitCount);
            logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Определяем точное количество потоков...", UserName = null });

            while (!_model.IsObjectsReady) { }
            logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Objects are resolved", UserName = null });

        }

        private void btnProxyOptions_Click(object sender, RoutedEventArgs e)
        {
            _proxyWindow = new ProxyOptionWindow();
            _proxyWindow.Show();
        }
    }
}