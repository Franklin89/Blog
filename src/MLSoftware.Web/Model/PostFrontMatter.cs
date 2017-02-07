using System;
using System.Collections.Generic;
using System.Globalization;

namespace MLSoftware.Web.Model
{
    public class PostFrontMatter
    {
        public PostFrontMatter(string filename, IEnumerable<string> lines)
        {
            Filename = filename;
            Parse(lines);
        }

        private void Parse(IEnumerable<string> lines)
        {
            if (lines != null)
            {
                foreach (var l in lines)
                {
                    var i = l.IndexOf(':');
                    if (i >= 0)
                    {
                        var key = l.Substring(0, i).Trim().ToLower();
                        var value = l.Substring(i + 1).Trim();
                        switch (key)
                        {
                            case "title":
                                Title = value;
                                break;
                            case "author":
                                Author = value;
                                break;
                            case "description":
                                Description = value;
                                break;
                            case "published":
                                Published = DateTime.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            //case "image":
                            //    post.FeaturedImage = _config.ImagePrefix + slug + '/' + value;
                            //    break;
                            case "tags":
                            case "categories":
                                Tags = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public DateTime Published { get; set; }
        public string[] Tags { get; set; }
        public string Filename { get; set; }
    }
}
