using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Models;
using WebApiAccount.Services;

namespace WebApiAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : DataController<User, UserStore, UsersAgent>
    {
        
        public UserController(UsersAgent usersAgent) : base(usersAgent)
        {
            
        }
        [HttpPost("AddAvatar/{id}")]
        public async Task<ControllerResult<User, bool, Avatar>> AddAvatar(int id, Avatar avatar)
        {
            var user = (await _store.Get(id)).Entity;
            
            var valid = await _store.AddAvatar(user, avatar);
            var res = new ControllerResult<User, bool, Avatar>(user, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("AddBill/{id}")]
        public async Task<ControllerResult<User, bool, Bill>> AddBill(int id, Bill bill)
        {
            var user = (await _store.Get(id)).Entity;
            var valid = await _store.AddBill(user, bill);
            var res = new ControllerResult<User, bool, Bill>(user, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("AddReport/{id}")]
        public async Task<ControllerResult<User, bool, Report>> AddReport(int id, Report report)
        {
            var user = (await _store.Get(id)).Entity;
            var valid = await _store.AddReport(user, report);
            var res = new ControllerResult<User, bool, Report>(user, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("ChangeNickname/{id}/{nickname}")]
        public async Task<ControllerResult<User, bool, User>> ChangeNickname(int id, string nickname)
        {
            var user = (await _store.Get(id)).Entity;
            var valid = await _store.ChangeNickname(user, nickname);
            var res = new ControllerResult<User, bool, User>(user, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("ChangePassword/{id}/{password}/{newPass}")]
        public async Task<ControllerResult<User, bool, User>> ChangePassword(int id, string password, string newPass)
        {
            var user = (await _store.Get(id)).Entity;
            var valid = await _store.ChangePassword(user, password, newPass);
            var res = new ControllerResult<User, bool, User>(user, valid.IsValidate, valid);
            return res;
        }

    }
}
