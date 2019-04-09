using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Instagram_Checker
{
    public partial class ProxyOptionWindow : Window
    {
        private const string _cryPath = "links.prvx";
        public List<string> AllLinks { get; set; }

        public ProxyOptionWindow()
        {
            InitializeComponent();
            AllLinks = new List<string>();
            if (!File.Exists(_cryPath))
                File.Create(_cryPath);
            else
            {
                AllLinks = File.ReadAllLines(_cryPath).ToList();
                tbLinks.Text = File.ReadAllText(_cryPath);
            }
        }


        private void btnSuceedLink_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbProxyLink.Text))
            {
                bool check = true;
                string hrefs = tbLinks.Text;
                var splited = hrefs.Split('\n').ToList();
                if (splited.Contains(tbProxyLink.Text))
                {
                    splited.Remove(tbProxyLink.Text);
                    check = false;
                }
                string str = "";
                foreach (string item in splited)
                {
                    if (!String.IsNullOrEmpty(item))
                        str += item + "\n";
                }
                if (check)
                    str += tbProxyLink.Text + "\n";

                tbLinks.Text = (str);
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            tbLinks.Text = String.Empty;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            string hrefs = tbLinks.Text;
            var encode = Encoding.UTF32;
            File.WriteAllText(_cryPath, hrefs, encode);
            AllLinks = File.ReadAllLines(_cryPath).ToList();
            this.Close();
        }

        private void btnAppendAllLinks_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose file with links";
            ofd.Filter = "Text file (*.txt) | *.txt";
            if (ofd.ShowDialog().Value == true)
                tbLinks.Text = File.ReadAllText(ofd.FileName);
        }
    }
}
