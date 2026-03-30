using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

[Index("Username", Name = "UQ__Users__536C85E44F56EE66", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    [StringLength(50)]
    public string? Provider { get; set; }

    public string? ExternalId { get; set; }

    public bool? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    [InverseProperty("User")]
    public virtual Cart? Cart { get; set; }

    // Để lưu ID từ bên thứ 3
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }


    [InverseProperty("User")]
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    [InverseProperty("User")]
    public virtual UserProfile? UserProfile { get; set; }
}
