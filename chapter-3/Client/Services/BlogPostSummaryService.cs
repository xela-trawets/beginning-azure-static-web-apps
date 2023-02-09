using Models;

using System.Net.Http.Json;

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
	public async Task LoadBlogPostSummaries()
	{
		if (Summaries == null) {

			Summaries = await http.GetFromJsonAsync<List<BlogPost>>("http://localhost:7071/api/blogposts");
			//Summaries = await http.GetFromJsonAsync<List<BlogPost>>("api/blogposts");
		}
	}
}
