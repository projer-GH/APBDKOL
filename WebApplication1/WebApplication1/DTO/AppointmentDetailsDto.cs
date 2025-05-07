namespace WebApplication1.DTO;


public class AppointmentDetailsDto
{
    public DateTime Date { get; set; }
    public PatientDto Patient { get; set; } = null!;
    public DoctorDto Doctor { get; set; } = null!;
    
    
    public List<ServiceDto> AppointmentServices { get; set; } = new();
}

public class PatientDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string PWZ { get; set; } = null!;
}

public class ServiceDto
{
    public string Name { get; set; } = null!;
    public decimal ServiceFee { get; set; }
}