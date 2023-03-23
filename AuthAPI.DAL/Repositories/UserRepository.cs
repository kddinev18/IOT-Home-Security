using AuthAPI.DAL.Repositories.Interfaces;
using AuthAPI.DTO.Interfaces;
using AuthAPI.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.DAL.Repositories
{
    public class UserRepository : IRepository
    {
        private AuthenticationAPIDbContext _context;
        public UserRepository() { }
        public UserRepository(IAuthenticationAPIDbContext context)
        {
            
        }
        public Task<bool> Create(IRequestDataTrasferObject requestData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(IRequestDataTrasferObject requestData)
        {
            throw new NotImplementedException();
        }

        public Task<IResposneDataTrasferObject> Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(IRequestDataTrasferObject requestData)
        {
            throw new NotImplementedException();
        }
    }
}
