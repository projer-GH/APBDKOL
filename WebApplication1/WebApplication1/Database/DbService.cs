using Microsoft.Data.SqlClient;
using WebApplication1.DTO;
using WebApplication1.Exceptions;

namespace WebApplication1.Database;


public interface IDbService
{
    Task<AppointmentDetailsDto> GetAppointmentByIdAsync(int id);
    Task AddAppointmentAsync(CreateAppointmentDto dto);
}

public class DbService : IDbService
{
    private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=APBDKOL;Trusted_Connection=True;TrustServerCertificate=True;";
    

    public async Task<AppointmentDetailsDto> GetAppointmentByIdAsync(int id)
    {
        const string query = @"
            SELECT a.date, 
                   p.first_name, p.last_name, p.date_of_birth,
                   d.doctor_id, d.pwz,
                   s.name, aps.service_fee
            FROM Appointment a
            JOIN Patient p ON p.patient_id = a.patient_id
            JOIN Doctor d ON d.doctor_id = a.doctor_id
            JOIN Appointment_Service aps ON aps.appoitment_id = a.appoitment_id
            JOIN Service s ON s.service_id = aps.service_id
            WHERE a.appoitment_id = @Id
        ";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();

        AppointmentDetailsDto? result = null;

        while (await reader.ReadAsync())
        {
            if (result is null)
            {
                result = new AppointmentDetailsDto
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDto
                    {
                        DoctorId = reader.GetInt32(4),
                        PWZ = reader.GetString(5)
                    },
                    AppointmentServices = new List<ServiceDto>()
                };
            }

            result.AppointmentServices.Add(new ServiceDto
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7)
            });
        }

        if (result is null)
            throw new NotFoundException("Appointment not found");

        return result;
    }

    public async Task AddAppointmentAsync(CreateAppointmentDto dto)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = connection.CreateCommand();
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            command.CommandText = "SELECT 1 FROM Appointment WHERE appoitment_id = @Id";
            command.Parameters.AddWithValue("@Id", dto.AppointmentId);
            var exists = await command.ExecuteScalarAsync();
            if (exists != null)
                throw new ConflictException("Appointment already exists");

            command.Parameters.Clear();
            command.CommandText = "SELECT doctor_id FROM Doctor WHERE pwz = @PWZ";
            command.Parameters.AddWithValue("@PWZ", dto.PWZ);
            var doctorIdObj = await command.ExecuteScalarAsync();
            if (doctorIdObj is null)
                throw new NotFoundException("Doctor not found");
            var doctorId = (int)doctorIdObj;

            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @PatientId";
            command.Parameters.AddWithValue("@PatientId", dto.PatientId);
            var patientExists = await command.ExecuteScalarAsync();
            if (patientExists is null)
                throw new NotFoundException("Patient not found");
            
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Appointment (appoitment_id, patient_id, doctor_id, date) VALUES (@Id, @PatientId, @DoctorId, @Date)";
            command.Parameters.AddWithValue("@Id", dto.AppointmentId);
            command.Parameters.AddWithValue("@PatientId", dto.PatientId);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@Date", DateTime.Now);
            await command.ExecuteNonQueryAsync();

            foreach (var service in dto.Services)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT service_id FROM Service WHERE name = @Name";
                command.Parameters.AddWithValue("@Name", service.ServiceName);
                var serviceIdObj = await command.ExecuteScalarAsync();
                if (serviceIdObj is null)
                    throw new NotFoundException($"Service '{service.ServiceName}' not found");
                var serviceId = (int)serviceIdObj;

                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Appointment_Service (appoitment_id, service_id, service_fee) VALUES (@AId, @SId, @Fee)";
                command.Parameters.AddWithValue("@AId", dto.AppointmentId);
                command.Parameters.AddWithValue("@SId", serviceId);
                command.Parameters.AddWithValue("@Fee", service.ServiceFee);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}


