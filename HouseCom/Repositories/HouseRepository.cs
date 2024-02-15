using HouseCom.Data;
using HouseCom.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
#nullable enable

namespace HouseCom.Repositories
{
    public class HouseRepository : IHouseRepository
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<House> dbSet;
        
        public HouseRepository(ApplicationDbContext db) 
        {
            _db = db;
            this.dbSet=_db.Set<House>();
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

        public async Task<IEnumerable<House>> GetAllHouses(Expression<Func<House, bool>>? filter = null, string? includeProperties = null,
            int pageSize = 0, int pageNumber = 1)
        {
            //IEnumerable<House> houses = await _db.Houses.ToListAsync();
            //return houses;

            IQueryable<House> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                //skip0.take(5)
                //page number- 2     || page size -5
                //skip(5*(1)) take(5)
                query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<House?> GetHouse(Expression<Func<House, bool>> filter , bool tracked = true, string? includeProperties = null)
        {
            //Old implementation
            //var house = await _db.Houses.FindAsync(id);
            //if (house == null)
            //{
            //    return null;
            //}
            //return house;

            IQueryable<House> query = dbSet;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
          
                query = query.Where(filter);
            

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            
            House? house = await query.FirstOrDefaultAsync();
            
            if (house == null)
            {  return null; }               
               
            return house;
        }

        public async Task UpdateHouse(House entityToUpdate)
        {
            //var entityToUpdate = await _db.Houses.FindAsync(id);
            //if (entityToUpdate == null)
            //{
            //    return null;
            //}
            entityToUpdate.UpdatedDate = DateTime.UtcNow;
            entityToUpdate.CreatedDate = DateTime.UtcNow;
            //entityToUpdate.Occupancy = house.Occupancy;
            //entityToUpdate.Price = house.Price;
            //entityToUpdate.Details = house.Details;
            //entityToUpdate.Name = house.Name;
            //entityToUpdate.Sqft = house.Sqft;
            //entityToUpdate.ImageUrl = house.ImageUrl;
            _db.Houses.Update(entityToUpdate);
            await _db.SaveChangesAsync();
            //entityToUpdate = await _db.Houses.FindAsync(id);
            
        }
    }
}
