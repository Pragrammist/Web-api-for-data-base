using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Reports")]
    public class Report : Entity
    {
        //public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public virtual User User { get; set; }
        public int? UserId { get; set; }
    }
}
