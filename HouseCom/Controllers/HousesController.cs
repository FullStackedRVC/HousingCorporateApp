using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseCom.Data;
using HouseCom.Models;

namespace HouseCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Houses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<House>>> GetHouses()
        {
            return await _context.Houses.ToListAsync();
        }

        // GET: api/Houses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<House>> GetHouse(int id)
        {
            var house = await _context.Houses.FindAsync(id);

            if (house == null)
            {
                return NotFound();
            }

            return house;
        }

        // PUT: api/Houses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutHouse( House house)
        {
            
            if ( house == null)
            {
                return BadRequest();
            }

            //var entityToUpdate = await _context.Houses.FindAsync(id);
            //if(entityToUpdate == null)
            //{
            //    return BadRequest();
            //}
            try
            {

                //entityToUpdate.UpdatedDate = DateTime.UtcNow;
                //entityToUpdate.CreatedDate = DateTime.UtcNow;
                //entityToUpdate.Occupancy = house.Occupancy;
                //entityToUpdate.Price = house.Price;
                //entityToUpdate.Details = house.Details;
                //entityToUpdate.Name = house.Name;
                //entityToUpdate.Sqft = house.Sqft;
                //entityToUpdate.ImageUrl = house.ImageUrl;
                //_context.Entry(entityToUpdate).State = EntityState.Modified;
                // did not work
                _context.Houses.Update(house);
                await _context.SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (house.Id == 0)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Houses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<House>> PostHouse(House house)
        {
            _context.Houses.Add(house);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHouse", new { id = house.Id }, house);
        }

        // DELETE: api/Houses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHouse(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house == null)
            {
                return NotFound();
            }

            _context.Houses.Remove(house);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HouseExists(int id)
        {
            return _context.Houses.Any(e => e.Id == id);
        }
    }
}
