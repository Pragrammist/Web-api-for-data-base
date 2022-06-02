using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Bills")]
    public class Bill : Entity
    {
        //public int Id { get; set; }
        public virtual User User { get; set; }
        public int? UserId { get; set; }

        public virtual List<Order> Orders { get; set; } = new List<Order>();

        public int Money { get; set; }  
        
        public virtual Guild Guild { get; set; }
        public int? GuildId { get; set; }
    }
}
