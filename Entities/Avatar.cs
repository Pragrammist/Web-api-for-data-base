using System.ComponentModel.DataAnnotations.Schema;

namespace UserAccountsDataBaseWebApi
{
    [Table("Avatars")]
    public class Avatar : Entity
    {
        //public int Id { get; set; }

        public virtual User User { get; set; }
        public int? UserId { get; set; }


        public byte[] Avatars { get; set; }

    }

}
