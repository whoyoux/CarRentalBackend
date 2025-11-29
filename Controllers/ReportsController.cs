using CarRentalBackend.Data;
using CarRentalBackend.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController(DataContext context, ILogger<ReportsController> logger) : ControllerBase
    {
        [HttpGet("monthly-revenue")]
        public async Task<ActionResult<MonthlyRevenueReportDto>> GetMonthlyRevenue([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                year ??= DateTime.Now.Year;
                month ??= DateTime.Now.Month;

                logger.LogInformation("Generating monthly revenue report for {Year}/{Month}", year, month);

                var yearParam = new SqlParameter("@Year", year);
                var monthParam = new SqlParameter("@Month", month);

                var result = await context.Database
                    .SqlQueryRaw<MonthlyRevenueReportDto>(
                        "EXEC GetMonthlyRevenue @Year, @Month",
                        yearParam, monthParam)
                    .ToListAsync();

                logger.LogInformation("Monthly revenue report generated successfully for {Year}/{Month}. Found {Count} results", year, month, result.Count);

                var report = result.FirstOrDefault() ?? new MonthlyRevenueReportDto
                {
                    Year = year.Value,
                    Month = month.Value,
                    TotalReservations = 0,
                    TotalRevenue = 0,
                    AverageReservationValue = 0
                };

                return Ok(report);
            }
            catch (SqlException sqlEx)
            {
                logger.LogError(sqlEx, "SQL error generating monthly revenue report: {Message}", sqlEx.Message);
                return StatusCode(500, new { error = $"Database error: {sqlEx.Message}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating monthly revenue report: {Message}. Stack trace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet("user-history/{userId}")]
        public async Task<ActionResult<List<UserReservationHistoryDto>>> GetUserReservationHistory(Guid userId)
        {
            try
            {
                logger.LogInformation("Retrieving user reservation history for user {UserId}", userId);

                var userIdParam = new SqlParameter("@UserId", userId);

                var result = await context.Database
                    .SqlQueryRaw<UserReservationHistoryDto>(
                        "EXEC GetUserReservationHistory @UserId",
                        userIdParam)
                    .ToListAsync();

                logger.LogInformation("User reservation history retrieved successfully for user {UserId}. Found {Count} reservations", userId, result.Count);

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                logger.LogError(sqlEx, "SQL error retrieving user reservation history: {Message}", sqlEx.Message);
                return StatusCode(500, new { error = $"Database error: {sqlEx.Message}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user reservation history: {Message}. Stack trace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet("discount/{userId}")]
        public async Task<ActionResult<DiscountDto>> GetUserDiscount(Guid userId)
        {
            try
            {
                logger.LogInformation("Calculating discount for user {UserId}", userId);

                var userIdParam = new SqlParameter("@UserId", userId);

                var discount = await context.Database
                    .SqlQueryRaw<decimal>(
                        "SELECT dbo.CalculateDiscount(@UserId) AS Value",
                        userIdParam)
                    .FirstOrDefaultAsync();

                logger.LogInformation("Discount calculated successfully for user {UserId}: {Discount}%", userId, discount);

                return Ok(new DiscountDto
                {
                    UserId = userId,
                    DiscountPercentage = discount
                });
            }
            catch (SqlException sqlEx)
            {
                logger.LogError(sqlEx, "SQL error calculating discount: {Message}", sqlEx.Message);
                return StatusCode(500, new { error = $"Database error: {sqlEx.Message}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating discount: {Message}. Stack trace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet("reservation-logs")]
        public async Task<ActionResult<List<ReservationLogDto>>> GetReservationLogs()
        {
            try
            {
                logger.LogInformation("Retrieving all reservation logs");

                var logs = await context.ReservationLogs
                    .OrderByDescending(log => log.LogDate)
                    .Select(log => new ReservationLogDto
                    {
                        Id = log.Id,
                        ReservationId = log.ReservationId,
                        UserId = log.UserId.ToString(),
                        Action = log.Action,
                        LogDate = log.LogDate
                    })
                    .ToListAsync();

                logger.LogInformation("Reservation logs retrieved successfully. Found {Count} logs", logs.Count);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving reservation logs: {Message}. Stack trace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }
    }
}

