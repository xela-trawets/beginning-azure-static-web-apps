﻿using Models;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Components;

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
			var request = new HttpRequestMessage(HttpMethod.Get, "api/blogposts");
			request.SetBrowserRequestMode(BrowserRequestMode.NoCors);
			//request.SetBrowserRequestCache(BrowserRequestCache.NoStore); //optional
			request.Headers.Add("Accept", "application/json");
			var response = await http.SendAsync(request);
			//if (!response.IsSuccessStatusCode) { navigationManager.NavigateTo("404"); return null; }
			Summaries = await response.Content.ReadFromJsonAsync<List<BlogPost>>();
			//if (blogPost is null) { navigationManager.NavigateTo("404"); return null; }
			//Summaries = await http.GetFromJsonAsync<List<BlogPost>>("/api/blogposts");
			foreach (var s in Summaries) { s.Tags ??= Array.Empty<string>(); }
		}
	}
	public void Remove(Guid id, string author)
	{
		if (Summaries == null ||
			!Summaries.Any(summary =>
			summary.Id == id && summary.Author == author)) { return; }
		var summary = Summaries.First(summary =>
			summary.Id == id && summary.Author == author);
		Summaries.Remove(summary);
	}
}
