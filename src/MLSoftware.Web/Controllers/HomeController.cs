using Microsoft.AspNetCore.Mvc;
using MLSoftware.Web.Model;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.IO;
using System.Linq;

namespace MLSoftware.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;

        public HomeController(IHostingEnvironment env, ILogger<HomeController> logger)
        {
            _env = env;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var posts = GetPostMetaData();
            return View(posts);
        }

        private IEnumerable<PostFrontMatter> GetPostMetaData()
        {
            var list = new List<PostFrontMatter>();

            var basePath = Path.Combine(_env.WebRootPath, "posts");
            _logger.LogDebug("Reading posts at: {0}", basePath);

            var files = Directory.GetFiles(basePath, "*.md");
            _logger.LogDebug("Found {0} files", files.Length);

            foreach(var file in files)
            {
                _logger.LogDebug("Parsing file: {0}", file);
                var fileInfo = new FileInfo(file);
                using (var reader = fileInfo.OpenText())
                {
                    var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
                    var doc = Markdown.Parse(reader.ReadToEnd(), pipeline);

                    var yaml = doc.Descendants().OfType<YamlFrontMatterBlock>().FirstOrDefault();
                    list.Add(new PostFrontMatter(fileInfo.Name, yaml?.Lines.Lines.Select(l => l.ToString())));
                }
            }

            return list.OrderByDescending(x => x.Published).Take(5);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
