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

            _model.InitProxy((bool)cbApiProxy.IsChecked ? true : false);
            _model.InitAccounts();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int countMail = 0;
            int countPrx = 0;
            while (true)
            {
                if (_model.IsAccountInited && _model.IsProxyInited)
                    break;

                if (_model.IsAccountInited && countMail == 0)
                {
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Accounts are ready", UserName = null });
                    countMail++;
                }

                if (_model.IsProxyInited && countPrx == 0)
                {
                    logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Proxy are ready", UserName = null });
                    countPrx++;
                }
            }

            if (countPrx == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Proxy are ready", UserName = null });
            else if (countMail == 0)
                logging.Invoke(LogIO.path, new Log() { Date = DateTime.Now, Method = "MainWindow", LogMessage = "Accounts are ready", UserName = null });

        }

        private void ShowLog(string tmp, Log log)
        {
            this.Dispatcher.Invoke(() => tbLog.Text = tbLog.Text + log + '\n');
        }
    }
}