using Microsoft.EntityFrameworkCore;
using Server.DataBaseContext;
using Server.Interfaces;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Repository
{
    internal class UserRepository : IUsers
    {
        private ApplicationContext context = new ApplicationContext();

        public User AuthentificationUser(string login, string password)
        {
            User? user = context.User.Where(c => c.Login == login && c.Password == password).FirstOrDefault();
            if (user != null)
            {
                user.ConnectionOpen = DateTime.Now;
                context.Entry(user).State = EntityState.Modified;
                context.SaveChanges();
                return user;
            }
            else
                return user;
        }
        public void BanUser(User user)
        {
            user.Banned = true;
            context.Entry(user).State = EntityState.Modified;
            context.SaveChanges();
        }

        public Guid GetUCode(User user)
        {
            return user.UCode;
        }

        public bool IsBannedUser(User user)
        {
            return context.User.Select(c => c.Banned).FirstOrDefault();
        }

        public User RegistrationUser(User usr)
        {
            User user = new User()
            {
                Login = usr.Login,
                Password = usr.Password,
                Ip = usr.Ip,
                Port = usr.Port,
                ConnectionOpen = DateTime.Now,
                UCode = Guid.NewGuid(),
                Banned = false
            };

            context.Entry(user).State = EntityState.Added;
            context.SaveChanges();
            return user;
        }

        public void UpdateDataConnection(User user)
        {
            user.ConnectionOpen = DateTime.Now;
            context.Entry(user).State = EntityState.Modified;
            context.SaveChanges();
        }

        public User UpdateUCode(User user)
        {
            user.UCode = Guid.NewGuid();
            context.Entry(user).State = EntityState.Modified;
            context.SaveChanges();
            return user;
        }
    }
}
