﻿namespace WebApp.DAL.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTime DateRegisterd { get; set; } = DateTime.Now;
    }
}