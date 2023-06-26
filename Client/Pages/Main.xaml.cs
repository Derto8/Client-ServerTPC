using Client.Data;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        MainWindow mainWindow;
        Thread threadCheckBan;
        Thread threadCheckDisconnect;
        public Main(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            threadCheckDisconnect = new Thread(() =>
            {
                CheckDisconnect();
            })
            { IsBackground = true };

            threadCheckDisconnect.Start();

            threadCheckBan = new Thread(() =>
            {
                CheckBan();
            })
            { IsBackground = true };
            threadCheckBan.Start();
        }

        private void GetInformation(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lbInformation.Items.Clear();
                MainWindow.SendMessage("Information");
                Dictionary<string, string> information = JsonConvert.DeserializeObject<Dictionary<string, string>>(MainWindow.GetMessage().Data);
                foreach (KeyValuePair<string, string> i in information)
                {
                    lbInformation.Items.Add($"{i.Key}{i.Value}");
                }
            });
        }

        private void CheckBan()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(3500);
                    MainWindow.SendMessage("IsBanUser");
                    ViewModelMessage message = MainWindow.GetMessage();
                    if (message.Data == "True")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Вы были забанены");
                            mainWindow.OpenPage(MainWindow.pages.main);
                        });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckDisconnect()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    MainWindow.SendMessage("CheckUCodeBlocked");
                    ViewModelMessage message = MainWindow.GetMessage();
                    if (message.Command == "NotUpdateUCode") continue;
                    if(message.Command == "UpdateUCode")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StaticData.Socket.Close();
                            MessageBox.Show("Вы были отключены от сервера, для смены вашего уникального кода");
                            mainWindow.OpenPage(MainWindow.pages.connect);
                        });
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.SendMessage("Disconnect");
            StaticData.Socket.Close();
            threadCheckDisconnect.Interrupt();
            threadCheckBan.Interrupt();
            mainWindow.Close();
        }
    }
}
