using AuthAPI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.DAL
{
    public interface IAuthenticationAPIDbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
