using System.Linq;
using System.Web.Mvc;
using WeCart.Models; // Ensure this namespace is correct

namespace WeCart.Controllers
{
    public class HomeController : Controller
    {
        private WeCartDBEntities db = new WeCartDBEntities(); // Your DbContext

        public ActionResult Index()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }
    }
}
