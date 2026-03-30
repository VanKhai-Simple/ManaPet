using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

public partial class Conversation
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? GuestSessionId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsClosed { get; set; }

    [InverseProperty("Conversation")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [ForeignKey("UserId")]
    [InverseProperty("Conversations")]
    public virtual User? User { get; set; }
}
