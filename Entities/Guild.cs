using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Guilds")]
    public class Guild : Entity
    {
        //public int Id { get; set; }
        public virtual List<User> Users { get; set; } = new List<User>();
        public string Name { get; set; }
        public virtual Bill Bill { get; set; }
        public int? BillId { get; set; }
    }
}
