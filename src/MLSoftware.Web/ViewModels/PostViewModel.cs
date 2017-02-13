using System.ComponentModel.DataAnnotations;

namespace MLSoftware.Web.ViewModels
{
    public class PostViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Content { get; set; }

        public bool IsDraft { get; set; }

        public string SuccessMessage { get; set; }

        public bool ShowSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);
    }
}
