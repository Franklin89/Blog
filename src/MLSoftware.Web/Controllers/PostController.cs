using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using MLSoftware.Web.Model;
using System.IO;
using System.Linq;
using System;

namespace MLSoftware.Web.Controllers
{
    [Route("[controller]")]
    public class PostController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;

        public PostController(IHostingEnvironment env, IMemoryCache cache, ILogger<PostController> logger)
        {
            _env = env;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        [Route("[action]/{id}")]
        public IActionResult Details(string id)
        {
            var post = _cache.Get<Post>(id);

            if(post == null)
            {
                _logger.LogDebug("Parsing markdown file and saving it to cache...");
                post = GetPost(id);

                _cache.Set(id, post, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.MaxValue
                });
            }

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
