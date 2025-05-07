namespace WebApplication1.DTO;

public class CreateAppointmentDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PWZ { get; set; } = null!;
    public List<CreateServiceDto> Services { get; set; } = new();
}

public class CreateServiceDto
{
    public string ServiceName { get; set; } = null!;
    public decimal ServiceFee { get; set; }
}