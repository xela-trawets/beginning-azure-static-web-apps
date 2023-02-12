using Models;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace Client.Services;

public class BlogPostSummaryService
{
	private HttpClient http;
	public List<BlogPost>? Summaries;
	public BlogPostSummaryService(HttpClient http)
	{
		ArgumentNullException.ThrowIfNull(http, nameof(http));
		this.http = http;
	}
	public void Add(BlogPost blogPost)
	{
		ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));
		if (Summaries is null) { return; }
		if (!Summaries.Any(summary => summary.Id == blogPost.Id && summary.Author == blogPost.Author)) {
			var summary = new BlogPost
			{
				Id = blogPost.Id,
				Author = blogPost.Author,
				BlogPostMarkdown = blogPost.BlogPostMarkdown,
				PublishedDate = blogPost.PublishedDate,
				Tags = blogPost.Tags,
				Title = blogPost.Title
			};
			if (summary.BlogPostMarkdown?.Length > 500) { summary.BlogPostMarkdown = summary.BlogPostMarkdown[..500]; }
			Summaries.Add(summary);
		}
	}
	public void Replace(BlogPost blogPost)
	{
		ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));
		int index = blogPostCache.FindIndex(
		if (Summaries == null ||
			Summaries.Any(bp => blogPost.Id == blogPost.Id && blogPost.Author == blogPost.Author)) { return; }
		var summary = Summaries.Find(summary => summary.Id == blogPost.Id && summary.Author == blogPost.Author);
		if (summary is not null) {
			summary.Title = blogPost.Title;
			summary.Tags = blogPost.Tags;
			summary.BlogPostMarkdown = blogPost.BlogPostMarkdown;
			if (summary.BlogPostMarkdown?.Length > 500) {
				summary.BlogPostMarkdown = summary.BlogPostMarkdown[..500];
			}
		}
	}

	public async Task LoadBlogPostSummaries()
	{
		if (Summaries == null) {

			//Summaries = await http.GetFromJsonAsync<List<BlogPost>>("http://localhost:7071/api/blogposts");
			Summaries = await http.GetFromJsonAsync<List<BlogPost>>("api/blogposts");
		}
	}
}
