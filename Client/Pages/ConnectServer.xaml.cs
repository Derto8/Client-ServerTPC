using Client.Data;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    /// Логика взаимодействия для ConnectServer.xaml
    /// </summary>
    public partial class ConnectServer : Page
    {
        MainWindow mainWindow;
        public ConnectServer(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            StaticData.IP = IPAddress.Parse(tbIp.Text);
            StaticData.Port = int.Parse(tbPort.Text);
            MainWindow.SendMessage($"Connect {tbLogin.Text} {tbPass.Text} {tbIp.Text} {tbPort.Text}");
            ViewModelMessage message = MainWindow.GetMessage();
            if (message.Command == "NotEnoughSpace")
            {
                MessageBox.Show("Извините, но на сервере нету места");
                return;
            }
            if (message.Command == "Auth")
            {
                if(message.Data == "Пользователь был заблокирован")
                {
                    MessageBox.Show("Вы не можете войти, так как были забанены");
                    return;
                }
                StaticData.UCode = Guid.Parse(message.Data);
            }

            mainWindow.OpenPage(MainWindow.pages.main);
        }
    }
}
