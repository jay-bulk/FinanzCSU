#nullable disable
using FinanzCSU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanzCSU.Controllers
{
    public class BudgetController : Controller
    {
        private readonly Team106DBContext _context;

        public BudgetController(Team106DBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBudget()
        {
            if (ModelState.IsValid)
            {
                int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(id => id.Type == ClaimTypes.Sid).Value);

                UserBudget budget = new() { UserID = userID };
                try
                {
                    _context.Add(budget);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    TempData["failure"] = $"Budget was not created";
                    return RedirectToAction("Index", "Home");
                }

                TempData["success"] = $"Budget created";

                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> MyBudget()
        {
            // Get the user's id from the session store

            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            // Get the users budget including all allocations

            var budget = _context.UserBudgets
                .Include(bgt => bgt.MonthlyAllocations)
                .Where(bgt => bgt.UserID == userID)
                .OrderBy(bgt => bgt.MonthlyAllocations);

            // If the user doesn't have a budget send them back 
            if (budget == null)
            {
                TempData["failure"] = $"Looks like you don't have a budget yet. Go create one first";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyTransactions()
        {
            // Get the user's id from the session store
            string monthID = DateTime.Now.ToString("MMM").ToUpper();

            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            // Get the users transactions

            var transactions = _context.Transactions
                .Include(t => t.CategoryID)
                .Where(u => u.UserBudget.UserID == userID)
                .Where(t => t.MonthID == monthID)
                .OrderBy(d => d.TransactionID);

            return View(await transactions.ToListAsync());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTransaction([Bind("TransactionAmount,CategoryID,MonthID")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                _context.SaveChanges();
                TempData["message"] = $"Charge recorded for {transaction.TransactionAmount?.ToString("C")}";
                return View(nameof(MyTransactions));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTransaction(int id, [Bind("TransactionAmount,CategoryID,MonthID")] Transaction transaction)
        {
            if (id != transaction.TransactionID)
            {
                return RedirectToAction(nameof(Index));
            }
            if (ModelState.IsValid)
            {
                _context.Update(transaction);
                await _context.SaveChangesAsync();
                TempData["message"] = $"{transaction.TransactionID} updated";
                return RedirectToAction(nameof(Index));
            }
            ViewData["TransactionID"] = new SelectList(_context.Transactions.OrderBy(t => t.TransactionID), "TransactionAmount", "CategoryName");
            return View(transaction);
        }

        public Task<List<Category>> GetCategories()
        {
            var categories = _context.Categories
                .Include(c => c.CategoryName)
                .ToListAsync();

            return categories;
        }

        private Task<List<Transaction>> GetTransactions(string monthID)
        {
            var transactionList = _context.Transactions
                .Where(t => t.MonthID == monthID)
                .ToListAsync();
            return transactionList;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMonthAllocation([Bind("MonthID,CategoryID,Allocation")] MonthlyAllocation allocation)
        {
            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            var userBudget = _context.UserBudgets.FirstOrDefault(b => b.UserBudgetID == userID);
            string categoryName = _context.Categories.FirstOrDefault(c => c.CategoryID == allocation.CategoryID).CategoryName;

            if (userBudget == null)
            {
                TempData["failure"] = $"User budget needs to be created first";
                return RedirectToAction(nameof(MyBudget));
            }

            if (ModelState.IsValid)
            {
                allocation.UserBudgetID = userBudget.UserBudgetID;

                try
                {
                    _context.Add(allocation);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    TempData["failure"] = $"Failed to save allocation for {categoryName}";
                    return RedirectToAction(nameof(MyBudget));
                }

                TempData["success"] = $"{categoryName} allocation saved for {allocation.MonthID}";
                return RedirectToAction(nameof(MyBudget));
            }
            ViewData["UserBudget"] = userBudget;
            ViewData["Categories"] = CategoryList();

            return View(MyBudget);
        }

        private List<SelectListItem> CategoryList(int categoryID = 0)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();

            foreach (var item in _context.Categories)
            {
                if (item.CategoryID == categoryID)
                {
                    categoryList.Add(new SelectListItem { Text = item.CategoryName, Value = item.CategoryID.ToString(), Selected = true });
                }
                else
                {
                    categoryList.Add(new SelectListItem { Text = item.CategoryName, Value = item.CategoryID.ToString() });
                }
            }

            categoryList.Insert(0, (new SelectListItem { Text = "Select Category Name", Value = "" }));

            return categoryList;
        }

        private List<SelectListItem> MonthList(string MonthID = "JAN")
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();

            foreach (var item in _context.MonthlyAllocations)
            {
                if (item.MonthID == MonthID)
                {
                    MonthList.Add(new SelectListItem { Text = item.MonthID, Value = item.MonthID, Selected = true });
                }
                else
                {
                    MonthList.Add(new SelectListItem { Text = item.MonthID, Value = item.MonthID });
                }
            }

            MonthList.Insert(0, (new SelectListItem { Text = "Select Month Name", Value = "" }));

            return MonthList;
        }
    }
}
