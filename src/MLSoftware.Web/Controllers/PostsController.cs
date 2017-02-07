using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MLSoftware.Web.Model;
using System.IO;
using System.Linq;

namespace MLSoftware.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;

        public PostsController(IHostingEnvironment env, ILogger<PostsController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            var post = GetPost(id);
            ViewData["Title"] = post.Title;
            return View(post);
        }

        private Post GetPost(string filename)
        {
            var filePath = Path.Combine(_env.WebRootPath, "posts", filename);

            if (System.IO.File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);

                using (var reader = fileInfo.OpenText())
                {
                    var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
                    var doc = Markdown.Parse(reader.ReadToEnd(), pipeline);

                    using (var writer = new StringWriter())
                    {
                        var renderer = new HtmlRenderer(writer);
                        pipeline.Setup(renderer);
                        renderer.Render(doc);
                        writer.Flush();

                        var yaml = doc.Descendants().OfType<YamlFrontMatterBlock>().FirstOrDefault();
                        return new Post(fileInfo.Name, yaml?.Lines.Lines.Select(l => l.ToString())) { Content = writer.ToString() };
                    }
                }
            }

            else
            {
                _logger.LogError("Unable to find file {0}", filePath);
                throw new FileNotFoundException();
            }
        }
    }
}
