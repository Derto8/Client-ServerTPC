using Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
using Common;
using Newtonsoft.Json;
using System.Printing.IndexedProperties;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OpenPage(pages.connect);
        }

        public enum pages
        {
            connect,
            main
        }

        public void OpenPage(pages _pages)
        {
            if (_pages == pages.connect)
                frame.Navigate(new Pages.ConnectServer(this));
            if (_pages == pages.main)
                frame.Navigate(new Pages.Main(this));
        }

        public static Socket ServerConnect()
        {
            IPEndPoint endPoint = new IPEndPoint(StaticData.IP, StaticData.Port);
            Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            socket.Connect(endPoint);
            return socket;
        }

        public static void SendMessage(string message)
        {
            ViewModelSend viewModelSend = new ViewModelSend(message, StaticData.UCode);
            byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
            StaticData.Socket = ServerConnect();
            int BytesSend = StaticData.Socket.Send(messageByte);
        }

        public static ViewModelMessage GetMessage()
        {
            byte[] bytes = new byte[10485760];
            int BytesRec = StaticData.Socket.Receive(bytes);
            string messageServer = Encoding.UTF8.GetString(bytes, 0, BytesRec);
            ViewModelMessage viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(messageServer);
            return viewModelMessage;
        }
    }
}
