using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TskMgmt.Api.Entities
{
    public class Project
    {

        [Key]
        public Guid Id { get; set; }

        //Multiple Projects for user.
        [Required]
        public Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))] // Navigation property to the User entity
        public User Owner { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string? Description { get; set; }

        public int Status { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public List<Task> Tasks { get; set; } = new List<Task>(); // Navigation property for the tasks associated with the project>

    }
}