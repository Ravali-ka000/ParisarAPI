using Microsoft.AspNetCore.Mvc;
using ParisarAPI.Services;

namespace ParisarAPI.Controllers
{
    [ApiController]
    [Route("api/Dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            // 🔴 1. Required validation
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Both fromDate and toDate are required."
                });
            }

            // 🔴 2. Normalize dates
            var from = fromDate.Value.Date;
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);

            // 🔴 3. Range validation
            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "fromDate cannot be greater than toDate."
                });
            }

            // 🔴 4. Limit range (optional)
            if ((to - from).TotalDays > 365)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Date range cannot exceed 1 year."
                });
            }

            // ✅ 5. Call service
            var data = await _dashboardService.GetDashboardData(from, to);

            return Ok(new
            {
                success = true,
                message = data,


            });
        }
        [HttpGet("monthlydata")]
        public async Task<IActionResult> GetMonthlyTrend(
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                [FromQuery] int? locationId)
        {
            // 🔴 1. Required validation
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Both fromDate and toDate are required."
                });
            }

            // 🔴 2. Normalize dates
            var from = fromDate.Value.Date;
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);

            // 🔴 3. Range validation
            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "fromDate cannot be greater than toDate."
                });
            }

            // 🔴 4. Optional: limit range
            if ((to - from).TotalDays > 365)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Date range cannot exceed 1 year."
                });
            }

            // 🔴 5. Optional: validate locationId
            if (locationId.HasValue)
            {
                var exists = await _dashboardService.LocationExists(locationId.Value);
                if (!exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid locationId."
                    });
                }
            }

            // ✅ 6. Call service
            var data = await _dashboardService.GetMonthlyTrend(from, to, locationId);

            return Ok(new
            {
                success = true,
                message = "Monthly trend fetched successfully",
                data = data
            });

        }
    }

}