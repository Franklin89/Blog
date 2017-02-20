using Markdig;

namespace MLSoftware.Markdown
{
    public class Markdown : IMarkdown
    {
        private string _configuration = "common";
        private bool _escapeAt = true;

        public Markdown()
        {
        }

        /// <summary>
        /// Specifies whether the <c>@</c> symbol should be escaped (the default is <c>true</c>).
        /// This is important if the Markdown documents are going to be passed to the Razor module,
        /// otherwise the Razor processor will interpret the unescaped <c>@</c> symbols as code
        /// directives.
        /// </summary>
        /// <param name="escapeAt">If set to <c>true</c>, <c>@</c> symbols are HTML escaped.</param>
        public Markdown EscapeAt(bool escapeAt = true)
        {
            _escapeAt = escapeAt;
            return this;
        }

        /// <summary>
        /// Includes a set of useful advanced extensions, e.g., citations, footers, footnotes, math,
        /// grid-tables, pipe-tables, and tasks, in the Markdown processing pipeline.
        /// </summary>
        public Markdown UseExtensions()
        {
            _configuration = "advanced";
            return this;
        }

        public string Execute(string input)
        {
            string output;

            if (!string.IsNullOrEmpty(input))
            {
                var pipeline = CreatePipeline();
                output = Markdig.Markdown.ToHtml(input, pipeline);
            }
            else
            {
                // Don't do anything if the key doesn't exist
                return input;
            }

            if (_escapeAt)
            {
                output = output.Replace("@", "&#64;");
            }

            return output;
        }

        private MarkdownPipeline CreatePipeline()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();
            pipelineBuilder.Configure(_configuration);
            return pipelineBuilder.Build();
        }
    }
}
