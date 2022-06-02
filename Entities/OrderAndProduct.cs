using System;

namespace UserAccountsDataBaseWebApi
{
    public class OrderAndProduct : Entity
    {
        //public int Id { get; set; }
        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public DateTime DateTime { get; set; }
    }
}
