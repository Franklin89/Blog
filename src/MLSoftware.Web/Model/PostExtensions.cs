﻿using Markdig;
using Markdig.Renderers;
using System.IO;

namespace MLSoftware.Web.Model
{
    public static class PostExtensions
    {
        public static string Parse(this PostContent postContent)
        {
            var content = postContent.Content;
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            return content.Parse();
        }

        public static string Parse(this string content)
        {
            var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
            var doc = Markdown.Parse(content, pipeline);

            using (var writer = new StringWriter())
            {
                var renderer = new HtmlRenderer(writer);
                pipeline.Setup(renderer);
                renderer.Render(doc);
                writer.Flush();
                return writer.ToString();
            }
        }
    }
}
