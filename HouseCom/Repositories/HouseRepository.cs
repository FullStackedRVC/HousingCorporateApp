using HouseCom.Data;
using HouseCom.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseCom.Repositories
{
    public class HouseRepository : IHouseRepository
    {
        private readonly ApplicationDbContext _db;
        public HouseRepository(ApplicationDbContext db) 
        {
            _db = db;
        }
        public async Task CreateHouse(House house)
        {
            await _db.Houses.AddAsync(house);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteHouse(House house)
        {                     
            _db.Houses.Remove(house);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<House>> GetAllHouses()
        {
            IEnumerable<House> houses = await _db.Houses.ToListAsync();
            return houses;
        }

        public async Task<House> GetHouse(int id)
        {
            var house = await _db.Houses.FindAsync(id);
            if (house == null)
            {
                return null;
            }
            return house;
        }

        public async Task<House> UpdateHouse(int id, House house)
        {
            var entityToUpdate = await _db.Houses.FindAsync(id);
            if (entityToUpdate == null)
            {
                return null;
            }
            entityToUpdate.UpdatedDate = DateTime.UtcNow;
            entityToUpdate.CreatedDate = DateTime.UtcNow;
            entityToUpdate.Occupancy = house.Occupancy;
            entityToUpdate.Price = house.Price;
            entityToUpdate.Details = house.Details;
            entityToUpdate.Name = house.Name;
            entityToUpdate.Sqft = house.Sqft;
            entityToUpdate.ImageUrl = house.ImageUrl;

            await _db.SaveChangesAsync();
            entityToUpdate = await _db.Houses.FindAsync(id);
            return entityToUpdate;
        }
    }
}
