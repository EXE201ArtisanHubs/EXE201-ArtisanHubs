using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.WorkshopPackages.Implements
{
    public class WorkshopPackageRepository : GenericRepository<Workshoppackage>, IWorkshopPackageRepository
    {

        private readonly ArtisanHubsDbContext _context;
        public WorkshopPackageRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workshoppackage>> GetAllWorshopPackageAsync()
        {
            return await _context.Workshoppackages.ToListAsync();
            
        }

        public async Task<Workshoppackage?> GetWorshopPackageByIdIdAsync(int id)
        {
            retur
        }
    }
}
