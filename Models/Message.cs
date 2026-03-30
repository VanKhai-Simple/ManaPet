using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

public partial class Message
{
    [Key]
    public int Id { get; set; }

    public int? ConversationId { get; set; }

    [StringLength(20)]
    public string? SenderType { get; set; }

    public int? SenderId { get; set; }

    public string MessageText { get; set; } = null!;

    public bool? IsRead { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("Messages")]
    public virtual Conversation? Conversation { get; set; }
}
