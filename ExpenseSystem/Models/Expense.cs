using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseSystem.Models {
    public class Expense {

        public int Id { get; set; }
        [StringLength(30)]
        public string Description { get; set; }
        [StringLength(15)]
        public string Status { get; set; }
        [Column(TypeName = "Decimal(9,2)")]
        public decimal Total { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public virtual List<ExpenseLine> ExpenseLines { get; set; } = new List<ExpenseLine>();

    }
}
