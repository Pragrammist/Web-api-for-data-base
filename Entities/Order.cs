using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Orders")]
    public class Order : Entity
    {
        //public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public virtual List<Product> Products { get; set; } = new List<Product>();
        public virtual Bill Bill { get; set; }
        public int?  BillId { get; set; }
        public int TotalSum { get; set; }
        public virtual List<OrderAndProduct> OrderAndProducts { get; set; } = new List<OrderAndProduct>();
    }
}
