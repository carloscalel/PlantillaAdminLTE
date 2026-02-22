using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET.MVC_NETFramework.Models
{
    public class AccessUser
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Usuario dominio")]
        public string UserName { get; set; }

        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string DisplayName { get; set; }

        [StringLength(150)]
        [EmailAddress]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }

        public IList<int> SelectedRoleIds { get; set; } = new List<int>();

        public string RolesSummary { get; set; }
    }
}
