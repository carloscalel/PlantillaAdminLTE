using ASP.NET.MVC_NETFramework.Data;
using ASP.NET.MVC_NETFramework.Models;
using ASP.NET.MVC_NETFramework.Security;
using ASP.NET.MVC_NETFramework.Services;
using System.Net;
using System.Web.Mvc;

namespace ASP.NET.MVC_NETFramework.Controllers
{
    [Authorize]
    [InternalAuthorize(Roles = InternalRoles.CrudReader + "," + InternalRoles.CrudWriter + "," + InternalRoles.CrudAdmin)]
    public class DepartmentsController : Controller
    {
        private readonly DepartmentRepository _repository = new DepartmentRepository();

        public ActionResult Index()
        {
            var departments = _repository.GetAll();
            return View(departments);
        }

        [InternalAuthorize(Roles = InternalRoles.CrudWriter + "," + InternalRoles.CrudAdmin)]
        public ActionResult Create()
        {
            return View(new Department());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [InternalAuthorize(Roles = InternalRoles.CrudWriter + "," + InternalRoles.CrudAdmin)]
        public ActionResult Create(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            _repository.Create(department);
            TempData["SuccessMessage"] = "Departamento creado correctamente.";
            return RedirectToAction("Index");
        }

        [InternalAuthorize(Roles = InternalRoles.CrudWriter + "," + InternalRoles.CrudAdmin)]
        public ActionResult Edit(short? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var department = _repository.GetById(id.Value);
            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [InternalAuthorize(Roles = InternalRoles.CrudWriter + "," + InternalRoles.CrudAdmin)]
        public ActionResult Edit(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            _repository.Update(department);
            TempData["SuccessMessage"] = "Departamento actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [InternalAuthorize(Roles = InternalRoles.CrudAdmin)]
        public ActionResult Delete(short? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var department = _repository.GetById(id.Value);
            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [InternalAuthorize(Roles = InternalRoles.CrudAdmin)]
        public ActionResult DeleteConfirmed(short id)
        {
            _repository.Delete(id);
            TempData["SuccessMessage"] = "Departamento eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
