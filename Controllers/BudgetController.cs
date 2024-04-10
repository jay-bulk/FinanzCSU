#nullable disable
using FinanzCSU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanzCSU.Controllers
{
    public class RestrictController : Controller
    {
        private readonly Team106DBContext _context;

        public RestrictController(Team106DBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async IActionResult CreateTransaction()
        {
            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(id => id.Type == ClaimTypes.Sid).Value);

            return View(await )

        }

        [Authorize]
        public async Task<IActionResult> MyBudget()
        {
            // Get the user's id from the session store

            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            // Get the users budget

            var budget = _context.UserBudgets
                .Include(bgt => bgt.MonthlyAllocations)
                .Include(bgt => bgt.Transactions)
                .Where(t => t.UserID == userID)
                .OrderBy(t => t.Transactions.monthId);

            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyTransactions()
        {
            // Get the user's id from the session store

            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            // Get the users transactions

            var orderDetails = _context.tblOrderDetails
                .Include(od => od.OrderFKNavigation)
                .Include(od => od.ProductFKNavigation)
                .Where(u => u.OrderFKNavigation.CustomerFK == userPK)
                .OrderBy(d => d.OrderFKNavigation.OrderDate);

            return View(await orderDetails.ToListAsync());
        }

        [Authorize]
        public IActionResult SaveBudget()
        {
            return RedirectToAction("MyBudget", "SaveBudget");
        }

        [Authorize]
        public IActionResult AddTransaction()
        {
            Budget aBudget = GetBudget();

            if (aBudget.LineItems().Any())
            {
                int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

                // insert order
                var transactions = new List<Transaction>();

                foreach (Transaction aTransaction in transactions)
                {
                    Transaction aTransaction = new() { TransactionID = aTransaction.TransactionID, TransactionAmount = aTransaction.TransactionAmount,  = orderPK };
                    _context.Add(aDetail);
                }

                _context.SaveChanges();

                // remove all items from cart

                transactions.ClearCache();

                SaveTransactions(transactions);

                return View(nameof(TransactionsView));
            }

            return RedirectToAction("Budget", "Review");
        }

        private IActionResult ConfirmBudget()
        {
            return View();
        }

        private UserBudget GetBudget()
        {
            UserBudget aBudget = HttpContext.Session.GetObject<UserBudget>("UserBudget") ?? new UserBudget();
            return aBudget;
        }

        private void SaveBudget(UserBudget aBudget)
        {
            HttpContext.Session.SetObject("UserBudget", aBudget);
        }

    }
}
