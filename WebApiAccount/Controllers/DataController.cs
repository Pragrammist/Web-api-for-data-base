using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAccount.Services;
using WebApiAccount.Models;
using UserAccountsDataBaseWebApi;
using Microsoft.AspNetCore.Authorization;

namespace WebApiAccount.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    [Authorize]
    public abstract class DataController<DataType, StoreType, AgentType> : Controller 
        where DataType : Entity
        where StoreType : IStore<DataType>
        where AgentType : StoreAgentBase<DataType, StoreType>
    {
        protected AgentType _store { get; set; }
        protected DataController(AgentType store)
        {
            _store = store;
            
            
        }


        
        [HttpPost("Add")]
        public virtual async Task<ControllerResult<DataType, DataType, DataType>> Add(DataType data)
        {
            var valid = await _store.Add(data);
            var result = new ControllerResult<DataType, DataType, DataType>(valid.Entity, valid.Entity, valid);
            return result;
        }
        [HttpGet("Get/{id}")]
        public virtual async Task<ControllerResult<DataType, DataType, DataType>> Get(int id)
        {
            var valid = await _store.Get(id);
            var result = new ControllerResult<DataType, DataType, DataType>(valid.Entity, valid.Entity, valid);
            return result;
        }
        [HttpDelete("Delete/{id}")]
        public virtual async Task<ControllerResult<DataType, bool, DataType>> Delete(int id)
        {
            var valid = await _store.Get(id);
            var result = new ControllerResult<DataType, bool, DataType>();
            result.Entity = valid.Entity;
            result.ValidationResult = valid;
            result.Result = valid.IsValidate;
            return result;
        }
    }
}
