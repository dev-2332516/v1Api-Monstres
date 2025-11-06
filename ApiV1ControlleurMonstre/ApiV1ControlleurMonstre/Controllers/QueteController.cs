using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiV1ControlleurMonstre.Controllers
{
    public class QueteController : Controller
    {
        // GET: QueteController
        public ActionResult Index()
        {
            return View();
        }

        // GET: QueteController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: QueteController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QueteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: QueteController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: QueteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: QueteController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: QueteController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
