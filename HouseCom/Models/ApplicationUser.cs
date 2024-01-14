using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseCom.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Role {  get; set; }
        [ForeignKey("House")]
        public int HouseID { get; set; }
    }
}
