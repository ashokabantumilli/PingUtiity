using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
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
        Dictionary<string, List<int> > pingresult = new Dictionary<string, List<int> >();
        public MainWindow()
        {
            Thread tid = new Thread(new ThreadStart(() => {
                string fileName = @"ip_" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
                int minuteCount = 0;
                while (true)
                {
                    Thread.Sleep(60000);
                    minuteCount++;
                    foreach (var host in pingresult)
                    {
                        var times = host.Value;
                        times.Sort();
                        using (StreamWriter sw = File.AppendText(fileName))
                        {
                            sw.WriteLine("Host = " + host.Key + " " + "Min = " + times[0].ToString() + " " 
                                + "Average = " + times[times.Count/2].ToString() + " " 
                                + "Max = " + times[times.Count - 1].ToString());
                        }

                        Dispatcher.Invoke(() =>
                        {
                            this.button.Content = times[0].ToString();
                            this.button1.Content = times[times.Count / 2].ToString();
                            this.button2.Content = times[times.Count - 1].ToString();
                            this.button3.Content = minuteCount;
                        });
                    }
                }
            }));
            tid.Start();
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
                            Arguments = iporhost + " -n 1 -4",
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
                                    if (output != null && output.Contains("Reply from"))
                                    {
                                        var tokens = output.Split(' ');
                                        foreach(var token in tokens)
                                        {
                                            if (token.Contains("time"))
                                            {
                                                if (token.Contains("="))
                                                {
                                                    var parse = token.Split("=");
                                                    try
                                                    {
                                                        if (pingresult[tokens[2]] == null)
                                                        {
                                                            pingresult[tokens[2]] = new List<int>();
                                                        }
                                                    }
                                                    catch (Exception ex) 
                                                    {
                                                        pingresult[tokens[2]] = new List<int>();
                                                    }
                                                    pingresult[tokens[2]].Add(Int32.Parse(parse[1].ToString().TrimEnd('s').TrimEnd('m')));
                                                }
                                                else if (token.Contains("<")) {
                                                    var parse = token.Split("<");
                                                    try
                                                    {
                                                        if (pingresult[tokens[2]] == null)
                                                        {
                                                            pingresult[tokens[2]] = new List<int>();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        pingresult[tokens[2]] = new List<int>();
                                                    }
                                                    pingresult[tokens[2]].Add(Int32.Parse(parse[1].ToString().TrimEnd('s').TrimEnd('m')));
                                                }
                                            }
                                        }
                                    }
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
