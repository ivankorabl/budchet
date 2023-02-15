using budchet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace budchet.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDBContext _context;

        public DashboardController(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index()
        {
            //Последние 7 дней
            DateTime StartData = DateTime.Today.AddDays(-6);
            DateTime EndData = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.category)
                .Where(y => y.dateTime >= StartData && y.dateTime <= EndData)
                .ToListAsync();

            //Доходы
            int TotalIncome = SelectedTransactions
                .Where(i => i.category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString();

            //Расходы
            int TotalExpense = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString();

            //Баланс
            int Balance = TotalIncome - TotalExpense;
            ViewBag.Balance = Balance.ToString();

            //Статистика
            ViewBag.DoughnutCartData = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .GroupBy(j => j.category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().category.Icon + " " + k.First().category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString()
                })
                .OrderByDescending(l => l.amount)
                .ToList();



            //Диаграмма Доходов и Расходов
            //доходы
            List<SplineChartData> IncommeSummary = SelectedTransactions
                .Where(i => i.category.Type == "Income")
                .GroupBy(j => j.dateTime)
                .Select(k => new SplineChartData()
                {
                    day = k.First().dateTime.ToString("dd-MM"),
                    income = k.Sum(l => l.Amount)

                })
                .ToList();
            //расходы
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .GroupBy(j => j.dateTime)
                .Select(k => new SplineChartData()
                {
                    day = k.First().dateTime.ToString("dd-MM"),
                    expense = k.Sum(l => l.Amount)

                })
                .ToList();


            //Расходы и Доходы
            string[] last7days = Enumerable.Range(0, 7)
                .Select(i => StartData.AddDays(i).ToString("dd-MM"))
                .ToArray();
            ViewBag.SplineChartData = from day in last7days
                                      join income in IncommeSummary on day equals income.day
                                      into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day
                                      into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense
                                      };


            //Недавние транзакции
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.category)
                .OrderByDescending(j => j.dateTime)
                .Take(5)
                .ToListAsync();


            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
