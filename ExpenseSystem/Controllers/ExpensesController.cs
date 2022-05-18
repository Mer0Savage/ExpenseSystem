using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseSystem.Models;

namespace ExpenseSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const string APPROVED = "Approved";
        private const string REJECTED = "Rejected";
        private const string REVIEW = "Review";
        private const string PAID = "PAID";

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PaidExpenses(int EmpId) {
            var emp = await _context.Employees.FindAsync(EmpId);
            if (emp == null) {
                throw new Exception($"Employee Not Found! - {EmpId} does not exist!");
            }
            emp.ExpensesPaid = (from e in _context.Expenses
                               where e.EmployeeId == EmpId & e.Status == PAID
                               select new { LineTotal = e.Total }).Sum(x => x.LineTotal);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public async Task<IActionResult> AddExpensesDue(int EmpId) {
            var emp = await _context.Employees.FindAsync(EmpId);
            if (emp == null) {
                throw new Exception($"Employee Not Found! - {EmpId} does not exist!");
            }
            emp.ExpensesDue = (from e in _context.Expenses
                               where e.EmployeeId == EmpId & e.Status == APPROVED
                               select new { LineTotal = e.Total }).Sum(x => x.LineTotal);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
          if (_context.Expenses == null)
          {
              return NotFound();
          }
            return await _context.Expenses.Include(x => x.Employee).ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
          if (_context.Expenses == null)
          {
              return NotFound();
          }
            var expense = await _context.Expenses.Include(x => x.Employee)
                                            .Include(x => x.ExpenseLines)
                                            .SingleOrDefaultAsync(x => x.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        // GET: api/Expenses/Approved
        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetApprovedExpenses() {
            if (_context.Expenses == null) {
                return NotFound();
            }
            return await _context.Expenses.Where(x => x.Status == APPROVED).ToListAsync();
        }

        // PUT: api/Expenses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, Expense expense){
            if (id != expense.Id){
                return BadRequest();
            }
            _context.Entry(expense).State = EntityState.Modified;

            try{
                await _context.SaveChangesAsync();
                await AddExpensesDue(expense.EmployeeId);
                await PaidExpenses(expense.EmployeeId);
            } catch (DbUpdateConcurrencyException){
                if (!ExpenseExists(id)){
                    return NotFound();
                }else{
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Expenses/Paid/5
        [HttpPut("paid/{id}")]
        public async Task<IActionResult> PutPaidExpense(int id, Expense expense) {
            var PrevStat = await _context.Expenses.FindAsync(id);
            if (PrevStat.Status != APPROVED) {
                throw new Exception("You can't pay an expense under review or rejected.");
            }
            expense.Status = PAID;
            return await PutExpense(id, expense);
        }

        // PUT: api/Expenses/Approve/5
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> PutApproveExpense(int id, Expense expense) {
            expense.Status = APPROVED;
            return await PutExpense(id, expense);
        }

        // PUT: api/Expenses/Reject/5
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> PutRejectExpense(int id, Expense expense) {
            expense.Status = REJECTED;
            return await PutExpense(id, expense);
        }

        // PUT: api/Expenses/Review/5
        [HttpPut("review/{id}")]
        public async Task<IActionResult> PutApprovedExpense(int id, Expense expense) {
            expense.Status = (expense.Total <= 75 ? APPROVED : REVIEW);
            return await PutExpense(id, expense);
        }

        // POST: api/Expenses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Expense>> PostExpense(Expense expense)
        {
          if (_context.Expenses == null)
          {
              return Problem("Entity set 'AppDbContext.Expenses'  is null.");
          }
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            await AddExpensesDue(expense.EmployeeId);
            await PaidExpenses(expense.EmployeeId);

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            if (_context.Expenses == null)
            {
                return NotFound();
            }
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            await AddExpensesDue(expense.EmployeeId);
            await PaidExpenses(expense.EmployeeId);

            return NoContent();
        }

        private bool ExpenseExists(int id)
        {
            return (_context.Expenses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
