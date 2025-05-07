using lab09.Exceptions;
using lab09.Model;
using Microsoft.Data.SqlClient;

namespace lab09.Services;

public class DbService : IDbService
{
    private readonly string? _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<VisitDto> GetVisitForId(int visitId)
    {
        var query = @"SELECT v.date, v.mechanic_Id, m.licence_number, c.first_name, c.last_name, c.date_of_birth, 
                    vs.service_fee, s.name FROM Visit v JOIN Mechanic m ON v.mechanic_id = m.mechanic_id 
                    JOIN Client c ON v.client_id = c.client_id JOIN Visit_Service vs ON v.visit_id = vs.visit_id
                    JOIN  Service s ON vs.service_id = s.service_id WHERE v.visit_id = @VisitId";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@VisitId", visitId);
        var reader = await command.ExecuteReaderAsync();


        VisitDto? result = null;

        while (await reader.ReadAsync())
        {
            if (result is null)
            {
                result = new VisitDto
                {
                    date = reader.GetDateTime(0),
                    mechanic = new MechanicDto
                    {
                        mechanic_id = reader.GetInt32(1),
                        licence_number = reader.GetString(2)
                    },
                    client = new ClientDto
                    {
                        first_name = reader.GetString(3),
                        last_name = reader.GetString(4),
                        date_of_birth = reader.GetDateTime(5),
                    },
                    services = new List<ServiceDto>()
                };
            }

            result.services.Add(new ServiceDto
                {
                    base_fee = reader.GetDecimal(6),
                    name = reader.GetString(7),
                }
            );
        }

        if (result is null)
        {
            throw new NotFoundException("Visit not found");
        }
        return result;
    }

    public async Task CreateVisit(AddVisitDto addVisitDto)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.Parameters.Clear();
            command.CommandText = "Select 1 FROM Visit WHERE visit_id = @VisitId";
            command.Parameters.AddWithValue("@VisitId", addVisitDto.visit_id);
            var result = await command.ExecuteScalarAsync();
            if (result is not null)
            {
                throw new InvalidOperationException("Visit already exists");
            }
            command.Parameters.Clear();
            command.CommandText = "Select 1 FROM Client WHERE client_id = @ClientId";
            command.Parameters.AddWithValue("@ClientId", addVisitDto.client_id);
            result = await command.ExecuteScalarAsync();
            if (result is null)
            {
                throw new NotFoundException("Client not found");
            }
            command.Parameters.Clear();
            command.CommandText = "Select mechanic_id FROM Mechanic WHERE licence_number = @LicenceNumber";
            command.Parameters.AddWithValue("@LicenseNumber", addVisitDto.mechanicLicenceNumber);
            result = (int) await command.ExecuteScalarAsync();
            int mechanicId = Convert.ToInt32(result);
            if (result is null)
            {
                throw new NotFoundException("Client not found");
            }
            command.Parameters.Clear();
            
            command.CommandText = "INSERT INTO Visit VALUES (@VisitId, @ClientId, @MechanicId, @Date))";
            command.Parameters.AddWithValue("@VisitId", addVisitDto.visit_id);
            command.Parameters.AddWithValue("@ClientId", addVisitDto.client_id);
            command.Parameters.AddWithValue("@MechanicId", mechanicId);
            command.Parameters.AddWithValue("@Date", DateTime.Now);
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            transaction.RollbackAsync();
            throw;
        }
        
    }
}