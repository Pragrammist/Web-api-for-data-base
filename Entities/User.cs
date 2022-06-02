using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System;

namespace UserAccountsDataBaseWebApi
{
    [Table("Users")]
    public class User : Entity
    {
        //public int Id { get; set; }
        public string Nick { get; set; }
        public int Password { get; set; }

        public virtual Avatar Avatar { get; set; }
        public int? AvatarId { get; set; }

        [NotMapped]
        public string StrPassword { get; set; }

        public virtual List<Report> Reports { get; set; } = new List<Report>(); 

        public int? GuildId { get; set; }
        public virtual Guild Guild { get; set; }

        public virtual Bill Bill { get; set; }
        public int? BillId { get; set; }

        public override int GetHashCode()
        {
            return Password;
        }
        public static int GetHashCode(string password)
        {


            var hashAlhorithm = SHA256.Create();

            var hash = hashAlhorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
            var res = BitConverter.ToInt32(hash, 0);
            return res;
        }
    }


    
}
