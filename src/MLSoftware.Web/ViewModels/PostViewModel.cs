using System.ComponentModel.DataAnnotations;
using MLSoftware.Web.Model;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MLSoftware.Web.ViewModels
{
    public class PostViewModel
    {
        public PostViewModel()
        {
        }

        public PostViewModel(Post post)
        {
            Id = post.Id;
            Author = post.Author;
            Title = post.Title;
            Description = post.Description;
            Published = post.Published;
            Content = post.Content?.Content;
            Tags = post.PostTags?.Select(x => x.Tag.Description).ToList();
            TagsString = string.Join(", ", Tags);
            Comments = post.Comments?.Select(x => new CommentViewModel(x)).ToList();
        }

        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public string Author { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public string Content { get; set; }

        public DateTime? Published { get; set; }

        public bool IsDraft => Published == null;

        public string SuccessMessage { get; set; }

        public bool ShowSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);

        public List<string> Tags { get; set; }

        public string TagsString { get; set; }

        public List<CommentViewModel> Comments { get; set; }
    }
}
