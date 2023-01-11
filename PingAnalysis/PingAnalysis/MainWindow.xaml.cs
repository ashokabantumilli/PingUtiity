using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
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

namespace PingAnalysis
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool syncping = false;
        private string iporhost;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_start_Click(object sender, RoutedEventArgs e)
        {
            this.button_start.IsEnabled = false;
            syncping = true;
            iporhost = this.IPorHostname.Text;
            await Task.Run( () =>
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "C:\\Windows\\System32\\ping.exe",
                            Arguments = iporhost + " -n 1",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                        while (syncping)
                        {
                            try
                            {
                                process.Start();
                                while (!process.StandardOutput.EndOfStream)
                                {
                                    var output = process.StandardOutput.ReadLine();
                                    Dispatcher.Invoke(() => {
                                        this.listView.Items.Add(output);
                                    });
                                }
                                process.WaitForExit();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            Task.Delay(5000).Wait();
                        }
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    Dispatcher.Invoke(() =>
                    {
                        this.button_start.IsEnabled = true;
                    });
                        
                }
                catch (Exception ex)
                {

                }
            });
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            syncping = false;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void IPorHostname_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}
