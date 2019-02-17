using System;
using System.Collections.Generic;
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

using System.ComponentModel;

namespace DomainToIPConverter_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Task testtask;

        Random rand = new Random();

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            char[] charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\n ".ToCharArray();

            testtask = Task.Run(async () =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Log += charset[rand.Next(charset.Length)];
                    });

                    await Task.Delay(1000);
                }
            });
        }
    }
}
