using FileLibrary;
using Instagram_Checker.BLL;
using InstaLog;
using System;
using System.Collections.Generic;
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
            btnLoad.IsEnabled = false;
            logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Start check info", UserName = null });

            _model.InitProxy((bool)cbApiProxy.IsChecked ? true : false, tbProxyKey.Text);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Load_DoWork;
            worker.RunWorkerCompleted += Load_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Start_DoWork;
            worker.RunWorkerCompleted += Start_RunWorkerCompleted;
            worker.RunWorkerAsync(tbProxyKey.Text);

            BackgroundWorker gridWorker = new BackgroundWorker();
            gridWorker.DoWork += GridWorker_DoWork;
            gridWorker.RunWorkerAsync();
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
            while (!_model.IsProgramComplitlyEnded)
            {
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
            string key = (string)e.Argument;
            _model.CheckAllAccounts();
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
            _model.InitAccounts();
            _model.InitAccountsMail();

            int countMail = 0;
            int countPrx = 0;
            int countAcc = 0;
            while (true)
            {
                if (_model.IsAccountInited && _model.IsProxyInited && _model.IsMailsReady)
                    break;

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
            }

            if (countPrx == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Proxy are ready ({_model.GetProxy.CountProxy})", UserName = null });
            else if (countAcc == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Accounts are ready ({_model.GetAccounts.CountUsers})", UserName = null });
            else if (countMail == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = $"Mail accounts are ready ({_model.GetAccountsMail.CountMails})", UserName = null });


            _model.InitObjects();
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