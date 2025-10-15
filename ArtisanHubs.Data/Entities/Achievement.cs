using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Entities
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public string Description { get; set; } = null!;
        public int ArtistId { get; set; }
        public virtual Artistprofile Artist { get; set; } = null!;
    }
}
