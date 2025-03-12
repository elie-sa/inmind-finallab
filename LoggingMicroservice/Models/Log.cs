using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LoggingMicroservice.Models;

public class Log
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public Guid RequestId { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public JsonDocument RequestObject { get; set; }

    [Required]
    public string RouteURL { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }
}