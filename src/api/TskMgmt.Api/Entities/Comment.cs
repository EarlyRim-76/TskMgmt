using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TskMgmt.Api.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        public Task Task { get; set; } = null!;

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(255)]
        public string Body { get; set; } = string.Empty;
        public DateTime Createdt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
