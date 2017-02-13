using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSoftware.Web.ViewModels
{
    public class CommentViewModel
    {
        public CommentViewModel()
        {
            var random = new Random();
            RiddleValue1 = random.Next(1, 20);
            RiddleValue2 = random.Next(1, 20);
        }

        public int PostId { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }

        public int RiddleValue1 { get; set; }
        public int RiddleValue2 { get; set; }

        [Required]
        public int? RiddleResultValue { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; }

        [NotMapped]
        public bool RiddleResult => (RiddleValue1 + RiddleValue2) == RiddleResultValue;
    }
}
