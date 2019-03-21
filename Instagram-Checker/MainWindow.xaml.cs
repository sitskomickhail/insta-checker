using FileLibrary;
using Instagram_Checker.BLL;
using InstaLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);


        public MainWindow()
        {
            InitializeComponent();
            _model = new Model();
            logging += ShowLog;
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

        private void ShowLog(string tmp, Log log)
        {
            this.Dispatcher.Invoke(() => tbLog.Text = tbLog.Text + log + '\n');
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            _model.CheckAllAccounts();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Start_DoWork;
            worker.RunWorkerCompleted += Start_RunWorkerCompleted;
        }

        private void Start_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Програма успешно завершила свою работу");
            btnLoad.IsEnabled = true;
        }

        private void Start_DoWork(object sender, DoWorkEventArgs e)
        {
            while(_model.IsProgramComplitlyEnded)
            {

            }
        }
    }
}