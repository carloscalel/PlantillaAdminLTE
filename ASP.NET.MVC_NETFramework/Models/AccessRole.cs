using System.ComponentModel.DataAnnotations;

namespace ASP.NET.MVC_NETFramework.Models
{
    public class AccessRole
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Código")]
        public string RoleCode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string RoleName { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }
    }
}
