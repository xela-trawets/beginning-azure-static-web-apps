using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Models;
using System.Linq;

namespace Api
{
	public static class BlogPosts
	{
		public static object UriFactory { get; private set; }
		[FunctionName($"{nameof(BlogPosts)}_Get")]
		public static IActionResult GetAllBlogPosts(
			[HttpTrigger(AuthorizationLevel.Anonymous,
			"get", Route ="blogposts")]HttpRequest req,
			[CosmosDB("SwaBlog","BlogContainer",
			Connection = "CosmosDbConnectionString",
			SqlQuery = @"
Select c.id, c.Title, c.Author, c.PublishedDate,
LEFT(c.BlogPostMarkdown,500) 
As BlogPostMarkdown,
Length(c.BlogPostMarkdown) <= 500 
As PreviewIsComplete,
c.Tags
FROM c
WHERE c.Status = 2")]IEnumerable<BlogPost> BlogPosts, //case..
			ILogger log
			)
		{
			return new OkObjectResult(BlogPosts);
		}

		[FunctionName($"{nameof(BlogPosts)}_GetId")]
		public static IActionResult GetBlogPost(
			[HttpTrigger(AuthorizationLevel.Anonymous,
			"get", Route ="blogposts/{author}/{id}")]HttpRequest req,
			[CosmosDB("SwaBlog","BlogContainer",
			Connection = "CosmosDbConnectionString",
			SqlQuery = @"
Select c.id, c.Title, c.Author, c.PublishedDate,
c.BlogPostMarkdown,
c.Status,
c.Tags
FROM c
WHERE c.id = {id} and c.Author = {author}")]IEnumerable<BlogPost> BlogPosts, //case..
			ILogger log
			)
		{
			if (BlogPosts.ToArray().Length == 0) { return new NotFoundResult(); }
			return new OkObjectResult(BlogPosts.First());
		}


		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string name = req.Query["name"];

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			name = name ?? data?.name;

			string responseMessage = string.IsNullOrEmpty(name)
				? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
				: $"Hello, {name}. This HTTP triggered function executed successfully.";

			return new OkObjectResult(responseMessage);
		}
	}
}
