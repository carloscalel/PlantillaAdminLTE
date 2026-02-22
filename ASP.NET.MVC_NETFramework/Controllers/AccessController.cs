using ASP.NET.MVC_NETFramework.Data;
using ASP.NET.MVC_NETFramework.Models;
using ASP.NET.MVC_NETFramework.Security;
using ASP.NET.MVC_NETFramework.Services;
using System.Net;
using System.Web.Mvc;

namespace ASP.NET.MVC_NETFramework.Controllers
{
    [Authorize]
    [InternalAuthorize(Roles = InternalRoles.CrudAdmin)]
    public class AccessController : Controller
    {
        private readonly AccessRepository _repository = new AccessRepository();

        public ActionResult Index()
        {
            return View(_repository.GetUsers());
        }

        public ActionResult Create()
        {
            ViewBag.Roles = new MultiSelectList(_repository.GetRoles(), "RoleId", "RoleCode");
            return View(new AccessUser { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccessUser model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new MultiSelectList(_repository.GetRoles(), "RoleId", "RoleCode", model.SelectedRoleIds);
                return View(model);
            }

            _repository.CreateUser(model, User.Identity.Name);
            TempData["SuccessMessage"] = "Acceso creado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = _repository.GetUserById(id.Value);
            if (user == null) return HttpNotFound();
            ViewBag.Roles = new MultiSelectList(_repository.GetRoles(), "RoleId", "RoleCode", user.SelectedRoleIds);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AccessUser model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new MultiSelectList(_repository.GetRoles(), "RoleId", "RoleCode", model.SelectedRoleIds);
                return View(model);
            }

            _repository.UpdateUser(model, User.Identity.Name);
            TempData["SuccessMessage"] = "Acceso actualizado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = _repository.GetUserById(id.Value);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteUser(id);
            TempData["SuccessMessage"] = "Acceso eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
