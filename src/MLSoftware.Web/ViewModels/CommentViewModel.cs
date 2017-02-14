using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MLSoftware.Web.Model;

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

        public CommentViewModel(Comment comment) : this()
        {
            Id = comment.Id;
            PostId = comment.PostId;
            Email = comment.Email;
            Created = comment.Created;
            Name = comment.Name;
            Content = comment.Content;  
        }

        public int Id { get; set; }
        public int PostId { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }

        public int RiddleValue1 { get; set; }
        public int RiddleValue2 { get; set; }

        [Required(ErrorMessage = "Please solve the math problem. Let's fight spam together!")]
        public int? RiddleResultValue { get; set; }

        [Required(ErrorMessage = "Please enter your name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Some content would be nice!")]
        [MaxLength(5000)]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [NotMapped]
        public bool RiddleResult => (RiddleValue1 + RiddleValue2) == RiddleResultValue;
    }
}
