using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseSystem.Models {
    [Index(nameof(Email), IsUnique = true)]
    public class Employee {

        public int Id { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        [StringLength(30)]
        public string Email { get; set; }
        [StringLength(30)]
        public string password { get; set; }
        public bool Admin { get; set; }
    }
}
