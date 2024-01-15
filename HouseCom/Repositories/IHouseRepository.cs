using HouseCom.Models;

namespace HouseCom.Repositories
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllHouses();
        Task<House> GetHouse(int id);
        Task CreateHouse(House house);
        Task<House> UpdateHouse(int id, House house);
        Task DeleteHouse(House house);
    }
}
