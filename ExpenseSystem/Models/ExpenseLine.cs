using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExpenseSystem.Models {
    public class ExpenseLine {

        public int Id { get; set; }
        public int quantity { get; set; }
        
        public int ExpenseId { get; set; }
        [JsonIgnore]
        public virtual Expense? Expense { get; set; }

        public int ItemId { get; set; }
        [JsonIgnore]
        public virtual Item? Item { get; set; }
    }
}
