using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class CreaterUser
    {
        [Required]
        public string Login { get; set; }
        [DefaultValue("Alexander")]
        public string FirstName { get; set; }
        [DefaultValue("Somov")]
        public string LastName { get; set; }

    }
}