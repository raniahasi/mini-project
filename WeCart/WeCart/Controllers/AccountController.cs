using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeCart.Models;

public class AccountController : Controller
{
    private WeCartDBEntities db = new WeCartDBEntities();

    // GET: Account/Login
    public ActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public ActionResult Login(string email, string password)
    {
        var user = db.Users.SingleOrDefault(u => u.Email == email && u.Password == password);
        if (user != null)
        {
            // Set session for logged-in user
            Session["UserId"] = user.UserId;
            Session["UserName"] = user.FirstName + " " + user.LastName; // Combine first and last name
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // If login fails, set ViewBag.LoginFailed to true and provide a custom message
            ViewBag.LoginFailed = true;
            ViewBag.LoginFailedMessage = "Wrong password. Please try again.";
            return View();
        }
    }

    // GET: Account/Register
    public ActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public ActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            db.Users.Add(user);
            db.SaveChanges();

            // Automatically log in the user after registration
            Session["UserId"] = user.UserId;
            Session["UserName"] = user.FirstName + " " + user.LastName;

            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // GET: Account/Logout
    public ActionResult Logout()
    {
        Session.Clear(); // Clear the session
        return RedirectToAction("Login");
    }

    // GET: Account/Profile
    public ActionResult Profile()
    {
        int userId = Convert.ToInt32(Session["UserId"]);
        var user = db.Users.Find(userId);
        if (user == null)
        {
            return HttpNotFound();
        }

        return View(user);
    }

    // POST: Account/UpdateProfile
    [HttpPost]
    public ActionResult UpdateProfile(HttpPostedFileBase Photo, string Password)
    {
        int userId = Convert.ToInt32(Session["UserId"]);
        var user = db.Users.Find(userId);
        if (user == null)
        {
            return HttpNotFound();
        }

        if (Photo != null && Photo.ContentLength > 0)
        {
            var fileName = Path.GetFileName(Photo.FileName);
            var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
            Photo.SaveAs(path);
            user.PhotoUrl = "/Content/images/" + fileName;
        }

        if (!string.IsNullOrEmpty(Password))
        {
            user.Password = Password; // Ideally, the password should be hashed before storing it.
        }

        db.SaveChanges();

        // Refresh the page with the updated user data
        return RedirectToAction("Profile");
    }

    // POST: Account/UploadProfilePicture
    [HttpPost]
    public JsonResult UploadProfilePicture(HttpPostedFileBase Photo)
    {
        int userId = Convert.ToInt32(Session["UserId"]);
        var user = db.Users.Find(userId);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        if (Photo != null && Photo.ContentLength > 0)
        {
            var fileName = Path.GetFileName(Photo.FileName);
            var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
            Photo.SaveAs(path);
            user.PhotoUrl = "/Content/images/" + fileName;

            db.SaveChanges();

            // Return the new image URL to be updated on the client side
            return Json(new { success = true, photoUrl = user.PhotoUrl });
        }

        return Json(new { success = false, message = "Invalid file." });
    }
}
