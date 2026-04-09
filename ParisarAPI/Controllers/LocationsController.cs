using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.EntityFrameworkCore;
using ParisarAPI.DTo_s;
using ParisarAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParisarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetLocation()
        {
            try
            {
                var location = await _context.Locations.AsNoTracking().ToListAsync();

                return Ok(new ApiResponseDto<List<Location>>
                {
                    Success = true,
                    Message = "Location fetched successfully.",
                    Data = location
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<Location>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

            // GET: api/Locations/5
            [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<Location>>> GetLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);

                if (location == null)
                {
                    return NotFound(new ApiResponseDto<Location>
                    {
                        Success = false,
                        Message = "Location not found."
                    });
                }

                return Ok(new ApiResponseDto<Location>
                {
                    Success = true,
                    Message = "Location fetched successfully.",
                    Data = location
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<Location>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // PUT: api/Locations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<Location>>> PutLocation(int id,Location location)
        {

            try
            {
                if (id != location.Id)
                {
                    return BadRequest(new ApiResponseDto<Location>
                    {
                        Success = false,
                        Message = "Location ID mismatch."
                    });
                }

                _context.Entry(location).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<Location>
                {
                    Success = true,
                    Message = "Location updated successfully.",
                    Data = location
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Locations .Any(e => e.Id == id))
                {
                    return NotFound(new ApiResponseDto<Location>
                    {
                        Success = false,
                        Message = "Location not found."
                    });
                }

                return StatusCode(500, new ApiResponseDto<Location>
                {
                    Success = false,
                    Message = "Concurrency error occurred while updating."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<Location>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // POST: api/Locations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<Location>>> PostLocation(Location location)
        {
            try
            {
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Location Added successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<Location>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);

                if (location == null)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Location not found."
                    });
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Location deleted successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                });
            }
        }
    }
}


