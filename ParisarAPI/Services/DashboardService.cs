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
        public async Task<bool> LocationExists(int locationId)
        {
            return await _context.Locations
                .AnyAsync(l => l.Id == locationId);
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
            var query = _context.PollutionData
                .Where(p => p.Date >= fromDate && p.Date <= toDate);

            if (locationId.HasValue)
            {
                query = query.Where(p => p.LocationId == locationId.Value);
            }

            var dbData = await query
                .GroupBy(p => new { p.LocationId, p.Date.Year, p.Date.Month })
                .Select(g => new
                {
                    LocationId = g.Key.LocationId,
                    g.Key.Year,
                    g.Key.Month,
                    PM10 = g.Average(x => x.PM10),
                    PM25 = g.Average(x => x.PM25)
                })
                .ToListAsync();

            var result = new List<MonthlyTrendDto>();

            var current = new DateTime(fromDate.Year, fromDate.Month, 1);
            var end = new DateTime(toDate.Year, toDate.Month, 1);

            while (current <= end)
            {
                var match = dbData.FirstOrDefault(x =>
                    x.Year == current.Year &&
                    x.Month == current.Month &&
                    (!locationId.HasValue || x.LocationId == locationId.Value)
                );

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

        public async Task<DashboardSummaryDto> GetSummary(
        DateTime fromDate,
        DateTime toDate,
        int locationId)
        {
            // 🔹 normalize
            var from = fromDate.Date;
            var to = toDate.Date.AddDays(1).AddTicks(-1);

            // 🔹 total days
            int totalDays = (toDate.Date - fromDate.Date).Days + 1;

            // 🔹 data for selected location
            var locationData = await _context.PollutionData
                .Where(p => p.LocationId == locationId &&
                            p.Date >= from && p.Date <= to)
                .ToListAsync();

            int availableDays = locationData.Count;

            // 🔹 exceeding counts
            int pm25Exceeded = locationData.Count(x => x.PM25 > 60);
            int pm10Exceeded = locationData.Count(x => x.PM10 > 100);

            // 🔹 ranking (this is the tricky part)
            var allLocations = await _context.PollutionData
                .Where(p => p.Date >= from && p.Date <= to)
                .GroupBy(p => p.LocationId)
                .Select(g => new
                {
                    LocationId = g.Key,
                    ExceedDays = g.Count(x => x.PM25 > 60 || x.PM10 > 100)
                })
                .OrderByDescending(x => x.ExceedDays)
                .ToListAsync();

            int rank = allLocations
                .FindIndex(x => x.LocationId == locationId) + 1;

            return new DashboardSummaryDto
            {
                TotalDays = totalDays,
                AvailableDays = availableDays,
                PollutionRank = rank == 0 ? 0 : rank,
                ExceedingPM25Days = pm25Exceeded,
                ExceedingPM10Days = pm10Exceeded
            };
        }
    }
}