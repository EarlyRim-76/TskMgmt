using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TskMgmt.Api.Entities
{
    public class Task
    {
        [Key]
        public Guid Id { get; set; }

        //Project can have multiple tasks.
        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;
        
        [Required]
        public Guid AssignedToId { get; set; } // one optional assigned user for the task

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
        public int Priority { get; set; } = 2;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal EstimatedHours { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>(); // Navigation property for the comments associated with the task

    }
}
