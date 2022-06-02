using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Services;
using WebApiAccount.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApiAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GuildController : DataController<Guild, GuildStore, GuildsAgent>
    {
        public GuildController(GuildsAgent guilds) : base(guilds)
        {
            
        }
        [HttpPost("AddBill/{id}")]
        public async Task<ControllerResult<Guild, bool, Bill>> AddBill(int id, Bill bill)
        {
            var guild = (await _store.Get(id)).Entity;
            var valid = await _store.AddBill(guild, bill);
            var res = new ControllerResult<Guild, bool, Bill>(guild, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("ChangeName/{id}/{name}")]
        public async Task<ControllerResult<Guild, bool, Guild>> ChangeName(int id, string name)
        {
            var guild = (await _store.Get(id)).Entity;
            var valid = await _store.ChangeName(name, guild);
            var res = new ControllerResult<Guild, bool, Guild>(guild, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("AddUser/{id}/{userId}")]
        public async Task<ControllerResult<Guild, bool, User>> AddUser(int id, int userId)
        {
            var user = new User { Id = userId };
            var guild = (await _store.Get(id)).Entity;
            var valid = await _store.AddUser(guild, user);
            var res = new ControllerResult<Guild, bool, User>(guild, valid.IsValidate, valid);
            return res;
        }
    }
}
