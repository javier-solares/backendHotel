using System.ComponentModel.DataAnnotations;

namespace dw_backend.Models.Rol
{
    public class RolModel
    {
        public int? id { get; set; }
        [Required]
        public string nombre { get; set; }
#nullable enable
        public string? codigo { get; set; }
        public string? usuario { get; set; }
        public int? estado { get; set; }
    }
}
