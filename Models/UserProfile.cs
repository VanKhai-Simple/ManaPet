using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

[Index("UserId", Name = "UQ__UserProf__1788CC4D94505A99", IsUnique = true)]
[Index("Email", Name = "UQ__UserProf__A9D105347F871444", IsUnique = true)]
public partial class UserProfile
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public DateOnly? BirthDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserProfile")]
    public virtual User? User { get; set; }
}
