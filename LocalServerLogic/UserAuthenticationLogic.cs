﻿using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Timers;
using System.Net.Sockets;
using System.Net;
using System.Data;
using System.Security.Cryptography;
using LocalServerBusinessLogic;

namespace LocalServerLogic
{
    public class UserAuthenticationLogic
    {
        private DatabaseIntialiser _databaseInitialiser;
        public UserAuthenticationLogic(DatabaseIntialiser databaseIntialiser)
        {
            _databaseInitialiser = databaseIntialiser;
        }
        // Retuns the hashed data using the SHA256 algorithm
        private string Hash(string data)
        {
            // Conver the output to a string and return it
            return BitConverter.ToString
                (
                    // Hashing
                    SHA256.Create().ComputeHash
                    (
                        // convert the string into bytes using UTF8 encoding
                        Encoding.UTF8.GetBytes(data)
                    )
                )
                // Convert all the characters in the string to a uppercase characters
                .ToUpper()
                // Remove the '-' from the hashed data
                .Replace("-", "");
        }
        // Generates a random sequence of characters and numbers
        private string GetSalt(string userName)
        {
            StringBuilder salt = new StringBuilder();
            Random random = new Random();
            salt.Append(userName.Substring(0, 6));
            for (int i = 0; i < 10; i++)
            {
                // Add another character into the string builder
                salt.Append
                    (
                        // Converts the output to a char
                        Convert.ToChar
                        (
                            // Generate a random nuber between 0 and 26 and add 65 to it
                            random.Next(0, 26) + 65
                        )
                    );
            }

            // Returns the string builder's string
            return salt.ToString();
        }

        // Checks if the email is on corrent format
        private bool CheckEmail(string email)
        {
            // Check if the email does not constains '@'
            if (email.Contains('@') == false)
                // If the email does not constains '@' it trows axception
                throw new ArgumentException("Email must contain \'@\'");

            // Return true otherwise
            return true;
        }

        // Checks if the password is on corrent format
        private bool CheckPassword(string pass)
        {
            // Checks if the password is between 10 and 32 characters long
            if (pass.Length <= 10 || pass.Length > 32)
                throw new ArgumentException("Password must be between 10 and 32 charcters");


            // Checks if the password contains a space
            if (pass.Contains(" "))
                throw new ArgumentException("Password must not contain spaces");

            // Checks if the password doesn't conatin upper characters
            if (!pass.Any(char.IsUpper))
                throw new ArgumentException("Password must contain at least 1 upper character");

            // Checks if the password doesn't conatin lower characters
            if (!pass.Any(char.IsLower))
                throw new ArgumentException("Password must contain at least 1 lower character");

            // Checks if the password conatins upper any special symbols
            string specialCharacters = @"%!@#$%^&*()?/>.<,:;'\}]{[_~`+=-" + "\"";
            char[] specialCharactersArray = specialCharacters.ToCharArray();
            foreach (char c in specialCharactersArray)
            {
                if (pass.Contains(c))
                    return true;
            }
            throw new ArgumentException("Password must contain at least 1 special character");
        }
        private bool CheckUsername(string userName)
        {
            // Checks if the userName is between 6 and 64 characters long
            if (userName.Length <= 8 || userName.Length > 64)
                throw new ArgumentException("Username must be between 8 and 64 charcters");


            // Checks if the userName contains a space
            if (userName.Contains(" "))
                throw new ArgumentException("Username must not contain spaces");

            return true;
        }
        public void AddAdminRole()
        {
            DataTable dataTable = _databaseInitialiser.Database.Tables.Where(table => table.Name == "Roles")
                .First().Select("Name", "=", "Admin");
            if(dataTable.Rows.Count == 0)
            {
                _databaseInitialiser.Database.Tables.Where(table => table.Name == "Roles").First().Insert("Admin");
                _databaseInitialiser.Database.SaveDatabaseData();
            }
        }

        public int Register(string userName, string email, string password)
        {
            // Add admin role
            AddAdminRole();

            // Checks if the email is in corrent format
            CheckUsername(userName);
            // Checks if the email is in corrent format
            CheckEmail(email);
            // Checks if the password is in corrent format
            CheckPassword(password);
            // Gets the salt
            string salt = GetSalt(userName);
            // Hashes the password combinded with the salt
            string hashPassword = Hash(password + salt);

            _databaseInitialiser.Database.Tables.Where(table => table.Name == "Users")
                .First().Insert(userName, email, hashPassword, salt, "1");
            _databaseInitialiser.Database.SaveDatabaseData();

            DataTable dataTable = _databaseInitialiser.Database.Tables.Where(table => table.Name == "Users")
                .First().Select("UserName", "=", userName);

            return int.Parse(dataTable.Rows[0]["UserId"].ToString());
        }

        public void RegisterMember(string userName, string email, string password, string roleName)
        {
            string roleId = _databaseInitialiser.Database.Tables.Where(table => table.Name == "Roles")
                .First().Select("Name", "=", roleName).Rows[0]["UserId"].ToString();
            // Checks if the email is in corrent format
            CheckUsername(userName);
            // Checks if the email is in corrent format
            CheckEmail(email);
            // Checks if the password is in corrent format
            CheckPassword(password);
            // Gets the salt
            string salt = GetSalt(userName);
            // Hashes the password combinded with the salt
            string hashPassword = Hash(password + salt);

            _databaseInitialiser.Database.Tables.Where(table => table.Name == "Users")
                .First().Insert(userName, email, hashPassword, salt, roleId);
            _databaseInitialiser.Database.SaveDatabaseData();
        }

        public int LogIn(string userName, string password)
        {
            DataTable dataTable = _databaseInitialiser.Database.Tables.Where(table => table.Name == "Users")
                .First().Select("Username", "=", userName);
            if (dataTable.Rows.Count == 0)
            {
                throw new Exception("Wrong credentials");
            }
            string hashPassword = Hash(password + dataTable.Rows[0]["UserId"]);
            if(hashPassword == dataTable.Rows[0]["Password"])
            {
                throw new Exception("Wrong credentials");
            }

            return int.Parse(_databaseInitialiser.Database.Tables.Where(table => table.Name == "Users")
                .First().Select("UserName", "=", userName).Rows[0]["UserId"].ToString());
        }
    }
}
