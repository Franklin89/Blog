using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MLSoftware.Web.TagHelpers
{
    [HtmlTargetElement("gravatar", Attributes = EmailAttributeName)]
    public class GravatarTagHelper : TagHelper
    {
        private const string SizeAttributeName = "image-size";
        private const string EmailAttributeName = "gravatar-email";
        private const string AltTextAttributeName = "alt";
        
        const string GravatarBaseUrl = "http://www.gravatar.com/avatar/";

        [HtmlAttributeName(EmailAttributeName)]
        public string Email { get; set; }

        [HtmlAttributeName(AltTextAttributeName)]
        public string AltText { set; get; }

        [HtmlAttributeName(SizeAttributeName)]
        public int? Size { get; set; }

        private string ToGravatarHash(string email)
        {
            var encoder = new UTF8Encoding();
            var md5 = MD5.Create();
            var hashedBytes = md5.ComputeHash(encoder.GetBytes(email.ToLower()));

            var sb = new StringBuilder(hashedBytes.Length * 2);
            for (var i = 0; i < hashedBytes.Length; i++)
            {
                sb.Append(hashedBytes[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        private string ToGravatarUrl(string email, int? size)
        {
            var url = new StringBuilder(GravatarBaseUrl, 90);

            url.Append(ToGravatarHash(email));
            var isFirst = true;

            Action<string, string> addParam = (p, v) =>
            {
                url.Append(isFirst ? '?' : '&');
                isFirst = false;
                url.Append(p);
                url.Append('=');
                url.Append(v);
            };

            if (size.HasValue)
            {
                if (size < 1 || size > 512)
                {
                    throw new ArgumentOutOfRangeException("size", size, "Must be null or between 1 and 512, inclusive.");
                }
                addParam("s", size.Value.ToString());
            }

            addParam("d", "identicon");

            return url.ToString();
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var str = new StringBuilder();
            var url = ToGravatarUrl(this.Email, this.Size);
            str.AppendFormat("<img src='{0}' alt='{1}' />", url, AltText);
            output.Content.AppendHtml(str.ToString());
        }
    }
}
