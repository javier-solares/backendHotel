using System.ComponentModel.DataAnnotations;

namespace dw_backend.Models
{
    public class UserModel
    {
        public int? id { get; set; }
        public int? persona { get; set; }
        public string? username { get; set; }
        public int? rol { get; set; }
        public int? estado { get; set; }
        public string? usuario { get; set; }
    }
}
