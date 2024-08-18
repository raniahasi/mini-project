using System.Linq;
using System.Web.Mvc;
using WeCart.Models;
using System;

public class CartController : Controller
{
    private WeCartDBEntities db = new WeCartDBEntities();

    private Cart GetCart()
    {
        Cart cart = null;

        if (Session["UserId"] != null)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            cart = db.Carts
                .Include("CartItems.Product")
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }
        }

        if (cart == null)
        {
            cart = new Cart();
        }

        return cart;
    }

    public ActionResult Index()
    {
        var cart = GetCart();
        return View(cart.CartItems.ToList());
    }
    [HttpPost]
    public ActionResult UpdateQuantity(int productId, int quantity)
    {
        var cart = GetCart();
        var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Cart item not found." });
        }

        cartItem.Quantity += quantity;
        if (cartItem.Quantity <= 0)
        {
            db.CartItems.Remove(cartItem);
        }
        db.SaveChanges();

        // Recalculate totals
        decimal subTotal = cart.CartItems.Sum(item => item.Quantity * (item.Product.Price ?? 0));
        decimal discount = ViewBag.Discount ?? 0;
        decimal shipping = ViewBag.ShippingCharge ?? 0;
        decimal tax = ViewBag.EstimatedTax ?? 0;
        decimal total = subTotal - discount + shipping + tax;
        decimal itemTotal = cartItem.Quantity * (cartItem.Product.Price ?? 0);

        return Json(new { success = true, subTotal, total, itemTotal });
    }
    [HttpPost]
    public ActionResult ClearCart()
    {
        var cart = GetCart();

        if (cart != null)
        {
            db.CartItems.RemoveRange(cart.CartItems);
            db.SaveChanges();
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public ActionResult AddToCart(int productId)
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var product = db.Products.Find(productId);
        if (product == null)
        {
            return HttpNotFound();
        }

        var cart = GetCart();

        var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);
        if (cartItem == null)
        {
            cartItem = new CartItem
            {
                ProductId = productId,
                Quantity = 1,
                CartId = cart.CartId,
                Product = product
            };
            cart.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity++;
        }

        db.SaveChanges();

        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    public ActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        var cartItem = cart.CartItems.FirstOrDefault(c => c.ProductId == productId);

        if (cartItem != null)
        {
            db.CartItems.Remove(cartItem);
            db.SaveChanges();
        }

        return Json(new { success = true });
    }

    public ActionResult CartSummary()
    {
        var cart = GetCart();
        var cartItems = cart.CartItems.ToList();
        var cartCount = cartItems.Sum(item => item.Quantity);

        var cartSummary = new
        {
            Count = cartCount,
            Items = cartItems.Select(item => new
            {
                Name = item.Product.Name,
                Quantity = item.Quantity,
                Price = item.Product.Price,
                ImageUrl = item.Product.ImageUrl,
                ProductId = item.ProductId
            }).ToList()
        };

        return Json(cartSummary, JsonRequestBehavior.AllowGet);
    }
}
