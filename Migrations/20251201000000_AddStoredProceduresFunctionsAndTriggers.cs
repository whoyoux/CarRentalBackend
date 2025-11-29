using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProceduresFunctionsAndTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Procedura składowana 1: GetMonthlyRevenue - Raport przychodów miesięcznych
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[GetMonthlyRevenue]
                    @Year INT = NULL,
                    @Month INT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF @Year IS NULL SET @Year = YEAR(GETDATE());
                    IF @Month IS NULL SET @Month = MONTH(GETDATE());
                    
                    SELECT 
                        YEAR(r.StartDateTime) AS Year,
                        MONTH(r.StartDateTime) AS Month,
                        COUNT(r.Id) AS TotalReservations,
                        SUM(r.TotalPrice) AS TotalRevenue,
                        AVG(r.TotalPrice) AS AverageReservationValue
                    FROM Reservations r
                    WHERE YEAR(r.StartDateTime) = @Year 
                        AND MONTH(r.StartDateTime) = @Month
                    GROUP BY YEAR(r.StartDateTime), MONTH(r.StartDateTime);
                END
            ");

            // Procedura składowana 2: GetUserReservationHistory - Historia rezerwacji użytkownika
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[GetUserReservationHistory]
                    @UserId UNIQUEIDENTIFIER
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT 
                        r.Id,
                        r.CarId,
                        c.Brand,
                        c.Model,
                        r.StartDateTime,
                        r.EndDateTime,
                        r.TotalPrice,
                        r.CreatedAt,
                        CASE 
                            WHEN r.EndDateTime < GETDATE() THEN 'Completed'
                            WHEN r.StartDateTime > GETDATE() THEN 'Upcoming'
                            ELSE 'Active'
                        END AS Status
                    FROM Reservations r
                    INNER JOIN Cars c ON r.CarId = c.Id
                    WHERE r.UserId = @UserId
                    ORDER BY r.StartDateTime DESC;
                END
            ");

            // Funkcja użytkownika: CalculateDiscount - Obliczanie zniżki dla stałych klientów
            migrationBuilder.Sql(@"
                CREATE FUNCTION [dbo].[CalculateDiscount](@UserId UNIQUEIDENTIFIER)
                RETURNS DECIMAL(5,2)
                AS
                BEGIN
                    DECLARE @ReservationCount INT;
                    DECLARE @Discount DECIMAL(5,2) = 0;
                    
                    SELECT @ReservationCount = COUNT(*)
                    FROM Reservations
                    WHERE UserId = @UserId;
                    
                    -- Zniżka 5% dla 5+ rezerwacji, 10% dla 10+ rezerwacji
                    IF @ReservationCount >= 10
                        SET @Discount = 10.00;
                    ELSE IF @ReservationCount >= 5
                        SET @Discount = 5.00;
                    
                    RETURN @Discount;
                END
            ");

            // Trigger: LogReservationDelete - Logowanie usuniętych rezerwacji
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'LogReservationDelete')
                    DROP TRIGGER [dbo].[LogReservationDelete];
            ");
            
            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[LogReservationDelete]
                ON [dbo].[Reservations]
                AFTER DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO [dbo].[ReservationLogs] (ReservationId, UserId, Action, LogDate)
                    SELECT 
                        d.Id,
                        d.UserId,
                        'Deleted',
                        GETDATE()
                    FROM deleted d;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[LogReservationDelete]");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS [dbo].[CalculateDiscount]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetUserReservationHistory]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetMonthlyRevenue]");
        }
    }
}

