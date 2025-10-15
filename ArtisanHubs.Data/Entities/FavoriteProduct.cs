using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Entities
{
    public class FavoriteProduct
    {
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
