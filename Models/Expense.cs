using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petshop_frontend.Models;

public partial class Expense
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string ExpenseCode { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string ExpenseType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
