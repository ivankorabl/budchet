using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace budchet.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        
        [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }
        public Category? category { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Значение не может быть равно 0 ")]
        public int Amount { get; set; }

       
        [Column(TypeName = "nvarchar(1500)")]
        public string? Note { get; set; }

        public DateTime dateTime { get; set; } = DateTime.Now;

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return category == null ? "" : category.Icon + "" + category.Title;
            }
        }

        [NotMapped]
        public string? FormattedAmount
        {

            get
            {
                return ((category == null || category.Type == "Expense") ? "- " : "+ ") + Amount.ToString();
            }

        }

    }
}
