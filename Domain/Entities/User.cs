using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; } // SQL: [UserID] [int] IDENTITY(1,1) NOT NULL

        [Required]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty; // SQL: [Username] [nvarchar](255) NOT NULL

        // Navigation properties
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
