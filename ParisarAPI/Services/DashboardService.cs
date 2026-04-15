using Microsoft.EntityFrameworkCore;
using ParisarAPI.DTOs;
using ParisarAPI.Models;

namespace ParisarAPI.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardData(DateTime fromDate, DateTime toDate)
        {
            var data = await _context.PollutionData
                .Where(p => p.Date >= fromDate && p.Date <= toDate)
                .GroupBy(p => new { p.LocationId, p.Location.Name })
                .Select(g => new LocationPollutionDto
                {
                    LocationName = g.Key.Name,
                    PM10 = g.Average(x => x.PM10),
                    PM25 = g.Average(x => x.PM25)
                })
                .ToListAsync();

            if (!data.Any())
            {
                return new DashboardDto
                {
                    Message = "No data found for selected date range",
                    BestPM10 = new List<LocationPollutionDto>(),
                    WorstPM10 = new List<LocationPollutionDto>(),
                    BestPM25 = new List<LocationPollutionDto>(),
                    WorstPM25 = new List<LocationPollutionDto>()
                };
            }

            return new DashboardDto
            {
                Message = "Success",
                BestPM10 = data.OrderBy(x => x.PM10).Take(3).ToList(),
                WorstPM10 = data.OrderByDescending(x => x.PM10).Take(3).ToList(),
                BestPM25 = data.OrderBy(x => x.PM25).Take(3).ToList(),
                WorstPM25 = data.OrderByDescending(x => x.PM25).Take(3).ToList()
            };
        }
        public async Task<List<MonthlyTrendDto>> GetMonthlyTrend(
                DateTime fromDate,
                DateTime toDate,
                int? locationId = null)
        { 
            // 🔹 Step 1: Get DB data
            var dbData = await _context.PollutionData
                .Where(p => p.Date >= fromDate && p.Date <= toDate &&
                            (!locationId.HasValue || p.LocationId == locationId))
                .GroupBy(p => new { p.Date.Year, p.Date.Month })
                .Select(g => new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    MonthNumber = g.Key.Month,
                    PM10 = g.Average(x => x.PM10),
                    PM25 = g.Average(x => x.PM25)
                })
                .ToListAsync();

            // 🔹 Step 2: Generate ALL months
            var result = new List<MonthlyTrendDto>();

            var current = new DateTime(fromDate.Year, fromDate.Month, 1);
            var end = new DateTime(toDate.Year, toDate.Month, DateTime.DaysInMonth(toDate.Year, toDate.Month));

            while (current <= end)
            {
                var match = dbData.FirstOrDefault(x =>
                    x.Year == current.Year && x.MonthNumber == current.Month);

                result.Add(new MonthlyTrendDto
                {
                    Year = current.Year,
                    MonthNumber = current.Month,
                    Month = current.ToString("MMM"),
                    PM10 = match?.PM10 ?? 0,
                    PM25 = match?.PM25 ?? 0
                });

                current = current.AddMonths(1);
            }

            return result;
        }
        public async Task<bool> LocationExists(int locationId)
        {
            return await _context.Locations.AnyAsync(l => l.Id == locationId);
        }

    }
}