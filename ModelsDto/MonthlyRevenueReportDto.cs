namespace CarRentalBackend.ModelsDto
{
    public class MonthlyRevenueReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageReservationValue { get; set; }
    }
}

