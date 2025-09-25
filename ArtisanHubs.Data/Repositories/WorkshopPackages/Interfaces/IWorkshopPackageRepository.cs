using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces
{
    public interface IWorkshopPackageRepository : IGenericRepository<Workshoppackage>
    {
        Task<Workshoppackage?> GetWorshopPackageByIdIdAsync(int id);
        Task<IEnumerable<Workshoppackage>> GetAllWorshopPackageAsync();
    }
}
