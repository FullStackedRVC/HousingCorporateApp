using HouseCom.Models;
using System.Linq.Expressions;
#nullable enable

namespace HouseCom.Repositories
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllHouses(Expression<Func<House, bool>>? filter = null, string? includeProperties = null,
            int pageSize = 0, int pageNumber = 1);
        Task<House?> GetHouse(Expression<Func<House, bool>> filter, bool tracked = true, string? includeProperties = null);
        Task CreateHouse(House house);
        Task UpdateHouse( House entity);
        Task DeleteHouse(House entity);
    }
}
