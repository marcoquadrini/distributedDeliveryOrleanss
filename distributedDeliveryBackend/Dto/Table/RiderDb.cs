using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace distributedDeliveryBackend.Dto;

public class RiderDb
{
    public RiderDb(string name, string lastName, bool isWorking)
    {
        Name = name;
        LastName = lastName;
        IsWorking = isWorking;
        IsAvaible = false;
    }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; }
    
    public bool IsWorking { get; set; }
    
    public bool IsAvaible { get; set; }
    
    
    
}