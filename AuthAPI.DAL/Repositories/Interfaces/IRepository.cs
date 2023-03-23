using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthAPI.DTO.Interfaces;

namespace AuthAPI.DAL.Repositories.Interfaces
{
    public interface IRepository
    {
        public Task<bool> Create(IRequestDataTrasferObject requestData);
        public Task<IResposneDataTrasferObject> Read(Guid id);
        public Task<bool> Update(IRequestDataTrasferObject requestData);
        public Task<bool> Delete(IRequestDataTrasferObject requestData); 
    }
}
