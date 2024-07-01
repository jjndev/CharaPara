using Markdig;
using System.Web;

namespace CharaPara.App
{
    public interface IUserInputMessageFormatService
    {
        public Task<string> FormatTextAsync(string input);
    }


    public class UserInputMessageFormatService_MarkDig : IUserInputMessageFormatService
    {
        MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
            .UseEmojiAndSmiley(false)
            .UseAdvancedExtensions().Build();

        public async Task<string> FormatTextAsync(string input)
        {
            var returnString = HttpUtility.HtmlEncode(input);
            
            return Markdown.ToHtml(returnString, markdownPipeline);
        }
    }
}
