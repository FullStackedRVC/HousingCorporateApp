using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseCom.Data;
using HouseCom.Models;
using HouseCom.Repositories;
using HouseCom.Models.DTO;
using AutoMapper;

namespace HouseCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousesController : ControllerBase
    {
        private readonly IHouseRepository _context;
        private readonly IMapper _mapper;

        public HousesController(IHouseRepository context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Houses        
        [HttpGet]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<HouseDTO>>> GetHouses()
        {
            var houses = await _context.GetAllHouses();
            var housesDTO = _mapper.Map<IEnumerable<House>,IEnumerable<HouseDTO>>(houses);
            return Ok(housesDTO);

        } 
        // GET: api/Houses/5

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HouseDTO>> GetHouse(int id)
        {
            var house = await _context.GetHouse(id);

            if (house == null)
            {
                return NotFound();
            }
            var houseDTO = _mapper.Map<HouseDTO>(house);
            return Ok(houseDTO);
        }

        // PUT: api/Houses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutHouse( int id, HouseDTO houseDTO)
        {
            
            if (id == 0 || houseDTO == null)
            {
                return BadRequest();
            }
           
            

            var house = _mapper.Map<House>(houseDTO);    
            var resultString = await _context.UpdateHouse(id,house);
            if (resultString == null)
            {
                return NotFound();
            }     
                
           

            return NoContent();
        }

        // POST: api/Houses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<House>> PostHouse([FromBody] HouseCreateDTO createDTO)
        {
            
            House house =_mapper.Map<House>(createDTO);
            await _context.CreateHouse(house);
            

            return CreatedAtAction("GetHouse", new { id = house.Id }, house);
        }

        // DELETE: api/Houses/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteHouse(int id)
        {
            var house = await _context.GetHouse(id);
            if (house == null)
            {
                return NotFound();
            }

            await _context.DeleteHouse(house);          

            return NoContent();
        }

       
    }
}
