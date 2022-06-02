using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAccount.Services;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApiAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BillController : DataController<Bill, BillStore, BillsAgent>
    {
        public BillController(BillsAgent billAgent) : base(billAgent)
        {
            
        }
        
        [HttpPost("AddSum/{id}/{sum}")]
        public async Task<ControllerResult<Bill, bool, Bill>> AddSum(int id, int sum)
        {
            var bill = (await _store.Get(id)).Entity;

            var valid = await _store.AddSum(sum, bill);
            var result = new ControllerResult<Bill, bool, Bill>(bill, valid.IsValidate, valid);
            return result;
        }
        [HttpPost("AddOrder/{id}/{orderId}")]
        public async Task<ControllerResult<Bill, bool, Order>> AddOrder(int id, int orderId)
        {
            var order = new Order { Id = orderId };
            var bill = (await _store.Get(id)).Entity;
            var valid = await _store.AddOrder(bill, order);
            var res = new ControllerResult<Bill, bool, Order>(bill, valid.IsValidate, valid);
            return res;
        }
    }
   
}
