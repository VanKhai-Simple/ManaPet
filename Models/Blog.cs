using System;
using System.ComponentModel.DataAnnotations;

namespace Petshop_frontend.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [Required]
        [StringLength(250)]
        public string Slug { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}