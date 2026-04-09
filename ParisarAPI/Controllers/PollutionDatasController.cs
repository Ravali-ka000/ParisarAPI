using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParisarAPI.DTo_s;
using ParisarAPI.Models;

namespace ParisarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollutionDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PollutionDataController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PollutionDatas
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<PollutionData>>>> GetPollutionData()
        {
            try
            {
                var data = await _context.PollutionData
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<PollutionData>>
                {
                    Success = true,
                    Message = "Pollution data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<List<PollutionData>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // GET: api/PollutionDatas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<PollutionData>>> GetPollutionData(int id)
        {
            try
            {
                var data = await _context.PollutionData
                    .Include(p => p.Location)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (data == null)
                {
                    return NotFound(new ApiResponseDto<PollutionData>
                    {
                        Success = false,
                        Message = "Pollution data not found."
                    });
                }

                return Ok(new ApiResponseDto<PollutionData>
                {
                    Success = true,
                    Message = "Pollution data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<PollutionData>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<bool>>> PostPollutionData(PollutionData pollutionData)
        {
            try
            {
                _context.PollutionData.Add(pollutionData);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Pollution data added successfully.",
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

        // PUT
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<PollutionData>>> PutPollutionData(int id, PollutionData pollutionData)
        {
            try
            {
                if (id != pollutionData.Id)
                {
                    return BadRequest(new ApiResponseDto<PollutionData>
                    {
                        Success = false,
                        Message = "ID mismatch."
                    });
                }

                _context.Entry(pollutionData).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<PollutionData>
                {
                    Success = true,
                    Message = "Pollution data updated successfully.",
                    Data = pollutionData
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PollutionData.Any(e => e.Id == id))
                {
                    return NotFound(new ApiResponseDto<PollutionData>
                    {
                        Success = false,
                        Message = "Pollution data not found."
                    });
                }

                return StatusCode(500, new ApiResponseDto<PollutionData>
                {
                    Success = false,
                    Message = "Concurrency error occurred."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<PollutionData>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeletePollutionData(int id)
        {
            try
            {
                var data = await _context.PollutionData.FindAsync(id);

                if (data == null)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Pollution data not found."
                    });
                }

                _context.PollutionData.Remove(data);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Pollution data deleted successfully.",
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