using UnifiedServiceScheduler.Model;
using NPoco;
using Microsoft.Data.SqlClient;
using System.Data;
using Polly;

namespace UnifiedServiceScheduler.DataAccess
{
    public class UnifiedServiceDataAccess : IUnifiesServiceDataAccess
    {
        private readonly IDatabase _db;
        public UnifiedServiceDataAccess(IDatabase db) => _db = db;

        public Task<bool> BookAppointment()
        {
            throw new NotImplementedException();
        }

        public async Task<(long TotalCount, List<DealerDetails> Dealers)> GetDealerDetail(DealerSearch dealerSearch)
        {

            var sql = @"EXEC dbo.GetDealers @0, @1, @2, @3, @4, @5";

            var (dealers, countResults) =
                await _db.FetchMultipleAsync<DealerDetails, DealerCountResult>(
                    sql,
                    dealerSearch.Limit,
                    dealerSearch.OffSet,
                    (object?)dealerSearch.DealerName ?? DBNull.Value,
                    (object?)dealerSearch.City ?? DBNull.Value,
                    (object?)dealerSearch.ServiceType ?? DBNull.Value,
                    string.IsNullOrWhiteSpace(dealerSearch.SortOrder)
                        ? "ASC"
                        : dealerSearch.SortOrder
                );

            long totalCount = countResults.FirstOrDefault()?.TotalCount ?? 0;

            return (totalCount, dealers);

            /*   var result = new List<DealerDetails>();
               long TotalCount = 0;
               var connection = _db.Connection as SqlConnection
              ?? throw new InvalidOperationException(
                  "The configured database connection is not SqlConnection.");

               try
               {
                   if (connection.State != ConnectionState.Open)
                       await connection.OpenAsync();

                   using var command = new SqlCommand(
                       "dbo.GetDealers", connection)
                   {
                       CommandType = CommandType.StoredProcedure,
                       CommandTimeout = 30
                   };

                   command.Parameters.Add("@Limit", SqlDbType.Int).Value = dealerSearch.Limit;

                   command.Parameters.Add("@OffSet", SqlDbType.Int).Value = dealerSearch.OffSet;

                   command.Parameters.Add("@DealerName", SqlDbType.NVarChar, 150).Value = (object?)dealerSearch.DealerName ?? DBNull.Value;

                   command.Parameters.Add("@City", SqlDbType.NVarChar, 80).Value = (object?)dealerSearch.City ?? DBNull.Value;

                   command.Parameters.Add("@ServiceType", SqlDbType.NVarChar, 100).Value = (object?)dealerSearch.ServiceType ?? DBNull.Value;

                   command.Parameters.Add("@SortOrder", SqlDbType.NVarChar, 10).Value = string.IsNullOrWhiteSpace(dealerSearch.SortOrder) ? "ASC" : dealerSearch.SortOrder;

                   var totalCountParam = command.Parameters.Add(
                       "@TotalCount", SqlDbType.Int);

                   totalCountParam.Direction = ParameterDirection.Output;

                   using (var reader = await command.ExecuteReaderAsync())
                   {
                       // Result set 1: dealer records
                       while (await reader.ReadAsync())
                       {
                           result.Add(new DealerDetails
                           {
                               DealerId = reader.GetInt64(
                                   reader.GetOrdinal("DealerId")),

                               DealerName = reader["DealerName"] as string,

                               Address = reader["Address"] as string,

                               City = reader["City"] as string,

                               isActive = reader.GetBoolean(
                                   reader.GetOrdinal("IsActive"))
                           });
                       }


                   }

                   // Output parameters are available after the reader is closed.

                   if (totalCountParam.Value != DBNull.Value)
                   {
                       TotalCount = Convert.ToInt64(totalCountParam.Value);
                   }

                   return (TotalCount, result);

               }
               finally
               {
                   if (connection.State != ConnectionState.Closed)
                   {
                       await connection.CloseAsync();
                   }
               }*/

        }

        public async Task<List<AvailableServiceSlot>> GetAvailableSlotsAsync(long dealerId, string? serviceName, DateTime? startTime)
        {
          
            
                var slots = await _db.FetchAsync<AvailableServiceSlot>(
                    "EXEC dbo.GetAvailableServiceSlots @0, @1, @2",
                    dealerId,
                    serviceName,
                    startTime);

                return slots;
            
        }

        public async Task<long> BookAppointmentAsync(BookAppointmentRequest request)
        {

                return await _db.ExecuteScalarAsync<int>(
                @"EXEC dbo.BookAppointment @0,@1,@2,@3,@4,@5,@6",
                request.UserId,
                request.VehicleId,
                request.DealerId,
                request.ServiceName,
                request.BayId,
                request.TechnicianId,
                request.StartTimeUtc);
           
        }

        public class DealerCountResult
        {
            public long TotalCount { get; set; }
        }
    }
}
