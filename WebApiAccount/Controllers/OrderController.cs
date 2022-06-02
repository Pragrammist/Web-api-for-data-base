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
    public class OrderController : DataController<Order, OrderStore, OrdersAgent>
    {
        public OrderController(OrdersAgent orders) : base(orders)
        {
            
        }
        [HttpPost("AddProduct/{id}/{productId}")]
        public async Task<ControllerResult<Order, bool, Product>> AddProduct(int id, int productId)
        {
            Product product = new Product { Id = productId };
            var order = (await _store.Get(id)).Entity;

            var valid =  await _store.AddProduct(order, product);
            var res = new ControllerResult<Order, bool, Product>(order, valid.IsValidate, valid);
            return res;
        }
    }
}
