using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Interfaces
{
    internal interface IUsers
    {
        /* 
        Методы сервера 
        */

        //регистрирует пользователя на сервере
        public User RegistrationUser (User user);
        //авторизирует пользователя на сервере
        public User AuthentificationUser(string login, string password);
        //забанить пользователя
        public void BanUser(User user);
        //проверяет забанен ли пользователь
        public bool IsBannedUser(User user);
        //обновляет время подключение пользователя
        public void UpdateDataConnection(User user);
        //обновляет уникальный код
        public User UpdateUCode (User user);

        /* 
        Методы для клиента 
        */

        public Guid GetUCode(User user);
    }
}
