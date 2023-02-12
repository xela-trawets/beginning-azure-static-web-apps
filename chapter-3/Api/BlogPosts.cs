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
using StaticWebAppAuthentication.Api;
using StaticWebAppAuthentication.Models;
using Microsoft.Azure.Cosmos;

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

		[FunctionName($"{nameof(BlogPosts)}_Post")]
		public static IActionResult PostBlogPost(
			[HttpTrigger(AuthorizationLevel.Anonymous,"post",Route ="blogposts")]
		BlogPost blogPost,
			HttpRequest request,
			[CosmosDB("SwaBlog","BlogContainer",Connection ="CosmosDbConnectionString")]
			out dynamic savedBlogPost,
			ILogger log
			)
		{
			if (blogPost.Id != default) { savedBlogPost = null; return new BadRequestObjectResult("id must be null"); }
			var clientPrincipal =
				StaticWebAppApiAuthorization.ParseHttpHeaderForClientPrincipal(request.Headers);
			blogPost.Id = Guid.NewGuid();
			blogPost.Author = clientPrincipal.UserDetails;
			savedBlogPost = new
			{
				id = blogPost.Id,
				Title = blogPost.Title,
				Author = blogPost.Author,
				PublishedDate = blogPost.PublishedDate,
				tags = blogPost.Tags,
				BlogPostMarkdown = blogPost.BlogPostMarkdown,
				Status = 2
			};
			return new OkObjectResult(blogPost);
		}

		[FunctionName($"{nameof(BlogPosts)}_Put")]
		public static IActionResult PutBlogPost(
			[HttpTrigger(AuthorizationLevel.Anonymous,"put",Route ="blogposts")]
		BlogPost updatedBlogPost,
			[CosmosDB("SwaBlog","BlogContainer",Connection ="CosmosDbConnectionString",Id ="{Id}",PartitionKey ="{Author}")]
		BlogPost currentBlogPost,
			[CosmosDB("SwaBlog","BlogContainer",Connection ="CosmosDbConnectionString")]
			out dynamic savedBlogPost,
			ILogger log
			)
		{
			//updatedBlogPost
			if (currentBlogPost is null) { savedBlogPost = null; return new NotFoundResult(); }
			savedBlogPost = new
			{
				id = updatedBlogPost.Id,
				Title = updatedBlogPost.Title,
				Author = updatedBlogPost.Author,
				PublishedDate = updatedBlogPost.PublishedDate,
				tags = updatedBlogPost.Tags,
				BlogPostMarkdown = updatedBlogPost.BlogPostMarkdown,
				Status = 2
			};
			return new NoContentResult();
		}
		[FunctionName($"{nameof(BlogPosts)}_Delete")]
		public static async Task<IActionResult> DeleteBlogPost(

			[HttpTrigger(AuthorizationLevel.Anonymous,"delete",
				Route ="blogPosts/{author}/{id}")]
			HttpRequest request,

			string author,
			string id,

			[ CosmosDB("SwaBlog","BlogContainer",
			Connection = "CosmosDbConnectionString",
			Id="{id}", PartitionKey="{author}")]
			BlogPost currentBlogPost,

			[CosmosDB(Connection = "CosmosDbConnectionString")] CosmosClient client,

			ILogger log)
		{
			if (currentBlogPost is null) {
				return new NoContentResult();
			}
			Container container =
				client.GetDatabase("SwaBlog")
				.GetContainer("BlogContainer");
			await container
				.DeleteItemAsync<BlogPost>(id, new PartitionKey(author));
			return NoContentResult();
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
