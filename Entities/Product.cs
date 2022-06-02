using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Products")]
    public class Product :Entity
    {
        //public int Id { get; set; }
        public virtual List<Order> Orders { get; set; }
        public virtual List<OrderAndProduct> OrderAndProducts { get; set; } = new List<OrderAndProduct>();
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
