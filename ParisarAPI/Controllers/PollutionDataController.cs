using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OfficeOpenXml;
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
                .Include(x => x.Location)
                .AsNoTracking()
                .ToListAsync();

                return Ok(new ApiResponseDto<List<PollutionData>>
                {
                    Success = true,
                    Message = "Pollution data fetched successfully.",
                    
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
        [HttpPost("uploaddata")]
        public async Task<IActionResult> UploadExcel(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
                return BadRequest("No file uploaded.");

            ExcelPackage.License.SetNonCommercialPersonal("Parisar");

            using var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                return BadRequest("Invalid Excel file.");

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            // ✅ Step 1: Map headers dynamically
            var columnMapping = new Dictionary<string, int>();

            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim().ToLower();

                if (header == "locationid" || header == "location_id" || header=="location")
                    columnMapping["location"] = col;
                else if (header == "date" || header == "date1")
                    columnMapping["date"] = col;
                else if (header == "pm10")
                    columnMapping["pm10"] = col;
                else if (header == "pm25")
                    columnMapping["pm25"] = col;
            }

            // ✅ Mandatory check
            var required = new[] { "location", "date", "pm10", "pm25" };
            var missing = required.Where(x => !columnMapping.ContainsKey(x)).ToList();

            if (missing.Any())
                return BadRequest($"Missing columns: {string.Join(", ", missing)}");

            // ✅ DB data
            var validLocationIds = await _context.Locations
                .Select(l => l.Id)
                .ToListAsync();

            var existingRecords = await _context.PollutionData
                .Select(p => new { p.LocationId, p.Date })
                .ToListAsync();

            var existingSet = new HashSet<string>(
                existingRecords.Select(e => $"{e.LocationId}_{e.Date:yyyyMMdd}")
            );

            var pollutionList = new List<PollutionData>();
            var errorRows = new Dictionary<int, string>();

            int errorColumn = colCount + 1;
            worksheet.Cells[1, errorColumn].Value = "Error";

            // ✅ Step 2: Process rows
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    int locationId = int.Parse(worksheet.Cells[row, columnMapping["location"]].Text);
                    DateTime date = DateTime.Parse(worksheet.Cells[row, columnMapping["date"]].Text);
                    double pm10 = double.Parse(worksheet.Cells[row, columnMapping["pm10"]].Text);
                    double pm25 = double.Parse(worksheet.Cells[row, columnMapping["pm25"]].Text);

                    if (!validLocationIds.Contains(locationId))
                    {
                        errorRows[row] = "Invalid LocationId";
                        continue;
                    }

                    string key = $"{locationId}_{date:yyyyMMdd}";
                    if (existingSet.Contains(key))
                    {
                        errorRows[row] = "Duplicate record";
                        continue;
                    }

                    pollutionList.Add(new PollutionData
                    {
                        LocationId = locationId,
                        Date = date,
                        PM10 = pm10,
                        PM25 = pm25,
                        CreatedBy = "Bulk Upload",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = ""
                    });

                    existingSet.Add(key);
                }
                catch
                {
                    errorRows[row] = "Invalid data format";
                }
            }

            // ✅ Step 3: Mark errors in Excel (like TL code)
            foreach (var err in errorRows)
            {
                int row = err.Key;
                string msg = err.Value;

                worksheet.Cells[row, errorColumn].Value = msg;
                worksheet.Cells[row, errorColumn].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, errorColumn].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
            }

            // ✅ Step 4: Save data

            foreach (var item in pollutionList)
            {
                item.Id = 0; // 🔥 force auto increment
            }

            await _context.PollutionData.AddRangeAsync(pollutionList);
            await _context.SaveChangesAsync();

            // 🔴 If errors exist → return Excel file
            if (errorRows.Any())
            {
                var fileBytes = package.GetAsByteArray();

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "UploadErrors.xlsx"
                );
            }

            // ✅ If no errors → normal response
            return Ok(new
            {
                message = "Upload completed successfully",
                saved = pollutionList.Count,
                skipped = 0
            });
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