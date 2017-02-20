using System;
using Xunit;

namespace MLSoftware.Markdown.Tests
{
    public class MarkdownTests
    {
        [Fact]
        public void RendersMarkdown()
        {
            // Given
            string input = @"Line 1
*Line 2*
# Line 3";
            string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
".Replace(Environment.NewLine, "\n");

            Markdown markdown = new Markdown();

            // When
            var html = markdown.Execute(input);

            // Then
            Assert.Equal(output, html);
        }

        [Fact]
        public void RendersCsharpCode()
        {
            // Given
            string input = @"```csharp
public string Foo(string bar)
{
    return bar;
}
```";
            string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
".Replace(Environment.NewLine, "\n");

            Markdown markdown = new Markdown();
            markdown.UseExtensions();

            // When
            var html = markdown.Execute(input);

            // Then
            Assert.Equal(output, html);
        }

        [Fact]
        public void RendersHtmlCode()
        {
            // Given
            string input = @"```html
<p>Hello World!</p>
```";
            string output = "<pre><code class=\"language-html\">&lt;p&gt;Hello World!&lt;/p&gt;</code></pre>".Replace(Environment.NewLine, "\n");

            Markdown markdown = new Markdown();
            markdown.UseExtensions();

            // When
            var html = markdown.Execute(input);

            // Then
            Assert.Equal(output, html);
        }
    }
}
