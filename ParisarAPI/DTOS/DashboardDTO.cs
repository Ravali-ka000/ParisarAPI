namespace ParisarAPI.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class LocationPollutionDto
    {
        public string LocationName { get; set; }
        public double PM10 { get; set; }
        public double PM25 { get; set; }
    }

    public class DashboardDto
    {
        public string Message { get; set; }

        public List<LocationPollutionDto> BestPM10 { get; set; }
        public List<LocationPollutionDto> WorstPM10 { get; set; }
        public List<LocationPollutionDto> BestPM25 { get; set; }
        public List<LocationPollutionDto> WorstPM25 { get; set; }
    }
    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int MonthNumber { get; set; } 

        public string Month { get; set; }
        public double PM10 { get; set; }
        public double PM25 { get; set; }
    }
    public class DashboardSummaryDto
    {
        public int TotalDays { get; set; }
        public int AvailableDays { get; set; }
        public int PollutionRank { get; set; }
        public int ExceedingPM25Days { get; set; }
        public int ExceedingPM10Days { get; set; }
    }
}