using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;
using Newtonsoft.Json;
using Server.Interfaces;
using Server.Models;
using Server.Repository;

namespace Server
{
    internal class Program
    {
        private static IPAddress IpAdress;
        private static int Port;
        private static IUsers IUsers = new UserRepository();
        private static List<User> Users = new List<User>();
        private static int DisconnectTime;
        private static int CountConnectionUsers;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Введите Ip-адрес");
            string ip = Console.ReadLine();
            Console.WriteLine("Введите порт");
            string port = Console.ReadLine();
            Console.WriteLine("Введите количество возможных подключений");
            CountConnectionUsers = int.Parse(Console.ReadLine());
            Console.WriteLine("Введите время отключения пользователя (в секундах)");
            DisconnectTime = int.Parse(Console.ReadLine());
            //DisconnectTime = 10;
            //CountConnectionUsers = 1;
            //string ip = "127.0.0.1";
            //string port = "5000";

            if (int.TryParse(port, out Port) && IPAddress.TryParse(ip, out IpAdress))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Данные введены, запускаю сервер");
                new Thread(() =>
                {
                    StartPrintUserData();
                })
                { IsBackground = true }.Start();
                StartServer();
            }
            Console.Read();
        }

        public static User AuthUser(string login, string password, IPAddress ip, int port)
        {
            User user;
            user = IUsers.AuthentificationUser(login, password);
            if (user == null)
                user = IUsers.RegistrationUser(new User() { Login = login, Password = password, Ip = ip, Port = port });
            if (user.Banned)
                return null;
            Users.Add(user);
            return user;
        }

        public static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IpAdress, Port);
            Socket sListener = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            sListener.Bind(endPoint);
            sListener.Listen(5);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Сервер запущен");
            while (true)
            {
                try
                {
                    Socket Handler = sListener.Accept();
                    string Data = null;
                    byte[] Bytes = new byte[10485760];
                    int BytesRec = Handler.Receive(Bytes);
                    Data += Encoding.UTF8.GetString(Bytes, 0, BytesRec);
                    if(!Data.Contains("CheckUCodeBlocked") && !Data.Contains("IsBanUser"))
                        Console.WriteLine("Сообщение от пользователя: " + Data + "\n");
                    string Reply = "";
                    ViewModelSend? viewModelSend = JsonConvert.DeserializeObject<ViewModelSend>(Data);
                    if(viewModelSend != null )
                    {
                        ViewModelMessage viewModelMessage;
                        string[] DataCommand = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                        if (DataCommand[0] == "Connect")
                        {
                            if (CountConnectUser())
                            {
                                string[] DataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                User user = AuthUser(DataMessage[1], DataMessage[2], IPAddress.Parse(DataMessage[3]), int.Parse(DataMessage[4]));
                                if (user is null)
                                    viewModelMessage = new ViewModelMessage("Auth", "Пользователь был заблокирован");
                                else
                                    viewModelMessage = new ViewModelMessage("Auth", user.UCode.ToString());
                                Reply = JsonConvert.SerializeObject(viewModelMessage);
                                byte[] message = Encoding.UTF8.GetBytes(Reply);
                                Handler.Send(message);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("NotEnoughSpace", null);
                                Reply = JsonConvert.SerializeObject(viewModelMessage);
                                byte[] message = Encoding.UTF8.GetBytes(Reply);
                                Handler.Send(message);
                            }
                        }
                        if (DataCommand[0] == "Information")
                        {
                            Dictionary<string, string> information = new Dictionary<string, string>();
                            User user = FindUser(viewModelSend.Id);
                            if (user is null)
                                continue;
                            TimeSpan diffDates = DateTime.Now.Subtract(user.ConnectionOpen);
                            string[] diifD = diffDates.ToString().Split(".");
                            information.Add("Время подключения: ", user.ConnectionOpen.ToString());
                            information.Add("Продолжительность подключения: ", diifD[0]);
                            information.Add("Ip-адрес: ", user.Ip.ToString());
                            information.Add("Порт: ", user.Port.ToString());
                            information.Add("Уникальный код подключения: ", user.UCode.ToString());
                            viewModelMessage = new ViewModelMessage("informationUser", JsonConvert.SerializeObject(information));
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        if (DataCommand[0] == "CheckUCodeBlocked")
                        {
                            try
                            {
                                User user = FindUser(viewModelSend.Id);
                                int diffD = DateTime.Now.Subtract(user.ConnectionOpen).Seconds;
                                if (diffD > DisconnectTime)
                                {
                                    user = IUsers.UpdateUCode(user);
                                    Users.Remove(user);
                                    viewModelMessage = new ViewModelMessage("UpdateUCode", user.UCode.ToString());
                                    Reply = JsonConvert.SerializeObject(viewModelMessage);
                                    byte[] message = Encoding.UTF8.GetBytes(Reply);
                                    Handler.Send(message);
                                }
                                else
                                {
                                    viewModelMessage = new ViewModelMessage("NotUpdateUCode", user.UCode.ToString());
                                    Reply = JsonConvert.SerializeObject(viewModelMessage);
                                    byte[] message = Encoding.UTF8.GetBytes(Reply);
                                    Handler.Send(message);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка: " + ex.Message);
                            }
                        }
                        if (DataCommand[0] == "IsBanUser")
                        {

                            User user = FindUser(viewModelSend.Id);
                            if (user is null)
                                continue;
                            string isBan = user.Banned.ToString();
                            viewModelMessage = new ViewModelMessage("IsBanUser", isBan);
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        if (DataCommand[0] == "Disconnect")
                        {
                            User user = FindUser(viewModelSend.Id);
                            Users.Remove(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }
        }

        private static void StartPrintUserData()
        {
            try
            {
                while (true)
                {
                    if (Users.Count > 0)
                    {
                        UserData(DisconnectTime / 4);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }

        private static void UserData(int time)
        {
            try
            {
                Console.WriteLine($"Количество пользователей: {Users.Count}");
                Console.WriteLine();
                foreach (var user in Users)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"Уникальный идентификатор: {user.UCode}");
                    Console.WriteLine($"Ip-адрес: {user.Ip}");
                    Console.WriteLine($"Порт: {user.Port}");
                    Console.WriteLine($"Логин: {user.Login}");
                    Console.WriteLine($"Пароль: {user.Password}");
                    Console.WriteLine($"Время подключения: {user.ConnectionOpen}");
                    TimeSpan diffDates = DateTime.Now.Subtract(user.ConnectionOpen);
                    string[] diifD = diffDates.ToString().Split(".");
                    Console.WriteLine($"Продолжительность подключения: {diifD[0]}");
                    Console.WriteLine();
                }
                Thread.Sleep(time * 1000);
            }
            catch
            {

            }
        }

        private static User FindUser(Guid code)
        {
            User user = Users.Where(c => c.UCode == code).FirstOrDefault();
            return user;
        }

        private static bool CountConnectUser()
        {
            if (Users.Count < CountConnectionUsers)
                return true;
            return false;
        }
    }
}