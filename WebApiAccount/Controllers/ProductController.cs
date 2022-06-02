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
    public class ProductController : DataController<Product, ProductStore, ProductsAgent>
    {
        public ProductController(ProductsAgent products) : base(products)
        {
            
        }
        [HttpPost("ChangeName/{id}/{name}")]
        public async Task<ControllerResult<Product, bool, Product>> ChangeName(int id, string name)
        {
            var product = (await _store.Get(id)).Entity;
            var valid = await _store.ChangeName(name, product);
            var res = new ControllerResult<Product, bool, Product>(product, valid.IsValidate, valid);
            return res;
        }
        [HttpPost("ChangePrice/{id}/{sum}")]
        public async Task<ControllerResult<Product, bool, Product>> ChangePrice(int id, int sum)
        {
            var product = (await _store.Get(id)).Entity;
            var valid = await _store.ChangePrice(sum, product);
            var res = new ControllerResult<Product, bool, Product>(product, valid.IsValidate, valid);
            return res;
        }
    }
}
