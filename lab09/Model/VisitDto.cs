using System.ComponentModel.DataAnnotations;

namespace lab09.Model;

public class VisitDto
{
    public DateTime date { get; set; }
    public ClientDto client { get; set; }
    public MechanicDto mechanic { get; set; }
    public List<ServiceDto> services { get; set; }
}

public class ClientDto
{
    public string first_name { get; set; }
    public string last_name { get; set; }
    public DateTime date_of_birth { get; set; }
}

public class MechanicDto
{
    public int mechanic_id { get; set; }
    public string licence_number { get; set; }
}

public class ServiceDto
{
    public string name { get; set; }
    public decimal base_fee { get; set; }
}

public class AddVisitDto
{
    [Required]
    public int visit_id { get; set; }
    [Required]
    public int client_id { get; set; }
    [Required]
    [MaxLength(14)]
    public string mechanicLicenceNumber { get; set; }
    public List<ServiceDto> services { get; set; }
}

public class ServiceAddDto
{
    public 
}
