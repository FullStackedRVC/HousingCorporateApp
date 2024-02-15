#nullable enable
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
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Drawing.Printing;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;

namespace HouseCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class HousesController : ControllerBase
    {
        private readonly IHouseRepository _context;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public HousesController(IHouseRepository context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _response = new();
        }

        // GET: api/Houses        
        [HttpGet]
        // Cache results for 30 seconds
        [ResponseCache(CacheProfileName = "Default30sec")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetHouses([FromQuery(Name = "filterOccupancy")] int? occupancy,
            [FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
        {
            //var houses = await _context.GetAllHouses();
            //var housesDTO = _mapper.Map<IEnumerable<House>,IEnumerable<HouseDTO>>(houses);
            //return Ok(housesDTO);

            try
            {

                IEnumerable<House> houseList;

                if (occupancy > 0)
                {
                    houseList = await _context.GetAllHouses(u => u.Occupancy == occupancy, pageSize: pageSize,
                        pageNumber: pageNumber);
                }
                else
                {
                    houseList = await _context.GetAllHouses(pageSize: pageSize,
                        pageNumber: pageNumber);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    houseList = houseList.Where(u => u.Name.ToLower().Contains(search));
                }
                Pagination pagination = new() { PageNumber = pageNumber, PageSize = pageSize };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
                _response.Result = _mapper.Map<List<HouseDTO>>(houseList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;


        }
        // GET: api/Houses/5

        [HttpGet("{id}")]
        [ResponseCache(CacheProfileName = "Default30sec")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetHouse(int id)
        {
            //var house = await _context.GetHouse(id);

            //if (house == null)
            //{
            //    return NotFound();
            //}
            //var houseDTO = _mapper.Map<HouseDTO>(house);
            //return Ok(houseDTO);

            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var house = await _context.GetHouse(u => u.Id == id);
                if (house == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<HouseDTO>(house);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        // PUT: api/Houses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:int}", Name = "UpdateHouse")]
        // If JWT Token does not have admin role the user will not be authorized to use this
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> PutHouse( int id, HouseDTO houseDTO)
        {
            
            try
            {                
                if (id == 0 || houseDTO == null)
                {
                    return BadRequest();
                }
                
                var house = await _context.GetHouse(u => u.Id == id, tracked: false);


                if (house == null)
                {
                    return BadRequest();
                }
                var houseUpdated = _mapper.Map<House>(houseDTO);
                await _context.UpdateHouse(houseUpdated);
                

            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }



        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialHouse(int id, JsonPatchDocument<HouseDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var house = await _context.GetHouse(u => u.Id == id, tracked: false);

            


            if (house == null)
            {
                return BadRequest();
            }
            HouseDTO houseDTO = _mapper.Map<HouseDTO>(house);
            patchDTO.ApplyTo(houseDTO, ModelState);
            House model = _mapper.Map<House>(houseDTO);

            await _context.UpdateHouse(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }



        // POST: api/Houses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> PostHouse([FromBody] HouseCreateDTO createDTO)
        {
            try
            {

                if(await _context.GetHouse(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already Exists!");
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }

                House house = _mapper.Map<House>(createDTO);
                await _context.CreateHouse(house);
                _response.Result = house;
                _response.StatusCode = HttpStatusCode.Created;
                //return CreatedAtRoute("GetHouse", new { id = house.Id }, _response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages 
                    = new List<string>() { ex.ToString() };

            }
            
            
           

            return _response;
        }

        // DELETE: api/Houses/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> DeleteHouse(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var house = await _context.GetHouse(u => u.Id == id);
                if (house == null)
                {
                    return NotFound();
                }

                await _context.DeleteHouse(house);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }


            return _response;
        }

       
    }
}
