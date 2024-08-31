using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace distributedDeliveryBackend.Dto;

public class OrderDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Lastname { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Address { get; set; }
    
    [Required]
    [StringLength(100)]
    public string City { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ZipCode { get; set; }
    
    [Required]
    [StringLength(100)]
    public string IdArticle { get; set; }
}