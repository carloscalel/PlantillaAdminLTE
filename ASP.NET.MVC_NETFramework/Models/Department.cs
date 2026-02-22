using System;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET.MVC_NETFramework.Models
{
    public class Department
    {
        public short DepartmentID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El grupo es obligatorio")]
        [StringLength(50, ErrorMessage = "El grupo no puede superar 50 caracteres")]
        [Display(Name = "Grupo")]
        public string GroupName { get; set; }

        [Display(Name = "Fecha modificación")]
        public DateTime ModifiedDate { get; set; }
    }
}
