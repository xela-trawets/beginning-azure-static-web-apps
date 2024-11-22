using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
//using System.ComponentModel;
using Microsoft.Azure.Cosmos;
using Models;
using StaticWebAppAuthentication.Api;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore;
using System.Net;
using Azure.Core;
//using Microsoft.AspNetCore.Mvc;
using Azure;
using StaticWebAppAuthentication.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace dotnet9funcApp
{
    public class SavedBlogPostResponse
    {
        //[QueueOutput("neworders", Connection = "AzureWebJobsStorage")]
        //public NewOrderMessage? Message { get; set; }
        [HttpResult]
        public Microsoft.AspNetCore.Http.HttpResults.Results<
            Ok<BlogPost>,
            NoContent,
            BadRequest<string>,
            NotFound<string>> HttpResult
        { get; set; }
        //public IActionResult HttpResult { get; set; }
        //public IResult HttpResult { get; set; }
        //public HttpResponseData? responseData { get; set; }
        //public HttpResponse? HttpResponse { get; set; }
        [CosmosDBOutput("SwaBlog", "BlogContainer", Connection = "CosmosDbConnectionString")]
        public dynamic? SavedBlogPost { get; set; }
        //[CosmosDBOutput("azurefuncs", "orders", Connection = "CosmosDbConnection")]
        //public OrderDocument? OrderDocument { get; set; }
    }
    public class BlogPosts
    {
        private readonly ILogger<BlogPosts> _logger;

        public BlogPosts(ILogger<BlogPosts> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public Results<Ok<string>, BadRequest<string>> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            return TypedResults.Ok<string>("Welcome to Azure Functions!");
        }
        public static object UriFactory { get; private set; }

        [Function($"{nameof(BlogPosts)}_Get")]
        public static
            //Results<Blo>
            //Ok<IEnumerable<BlogPost>>//, BadRequest<string>>
            Results<Ok<IEnumerable<BlogPost>>, BadRequest<string>>
            GetAllBlogPosts(
            [HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route ="blogposts")]HttpRequest request,
            [CosmosDBInput("SwaBlog","BlogContainer",
            Connection = "CosmosDbConnectionString",
            SqlQuery = @"
Select c.id, c.Title, c.Author, c.PublishedDate,
LEFT(c.BlogPostMarkdown,500) 
As BlogPostMarkdown,
Length(c.BlogPostMarkdown) <= 500 
As PreviewIsComplete,
c.Tags
FROM c
WHERE c.Status = 2")]List<BlogPost> BlogPostsDyn, //case..
            ILogger log
            )
        {
            //var response = request.CreateResponse(HttpStatusCode.OK);
            //await response.WriteAsJsonAsync(BlogPosts);
            //return response;
            //return new ResponseMessage(HttpStatusCode.OK, request);//
            var d= BlogPostsDyn.First();
//            var bp0 = JsonSerializer.Deserialize<BlogPost>(d);
            var bpost = new BlogPost();

            //var bpostAuthor = d.Author;
            //var bpostId = d.id.ToString();
            //    bpost.Title = d.Title;ValueKind = Object : "{"id":"48360f2b-be2d-4130-9064-3f84225fd3b0","Title":"Making your first static web app","Author":"Freya Christal Garfield","PublishedDate":"2022-01-01T12:00:00","BlogPostMarkdown":"**Nullam ullamcorper** ipsum felis, nec sollicitudin eros facilisis sit amet. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Curabitur risus elit, pharetra non blandit sed, pretium sed ipsum. Vivamus vel metus sit amet odio commodo dictum in vitae mi. Sed nunc massa, tincidunt eget facilisis eu, euismod et nisi. Donec est justo, ullamcorper eget massa ut, finibus posuere risus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas vitae ","PreviewIsComplete":false,"Tags":["C#","Blazor","Azure","Functions","Static Web Apps"]}"
            //    bpost.Tags = d.Tags;
            //bpost.PublishedDate = DateTime.Parse(d.PublishedDate);


            var BlogPosts = BlogPostsDyn;
            //var BlogPosts = BlogPostsDyn.Select(d => JsonSerializer.Deserialize<BlogPost>(d)).ToList();
            //new BlogPost()
            //{
            //    PublishedDate = d.PublishedDate,
            //    Author = d.Author,
            //    Id = d.id,
            //    Title = d.Title,
            //    Tags = d.Tags
            //}).ToList();
            //var result = TypedResults.Ok<IEnumerable<BlogPost>>([]);
            var result = TypedResults.Ok<IEnumerable<BlogPost>>(BlogPosts);
            return result;
        }

        //Write a function to post a blog post
        public static
            //IActionResult
            SavedBlogPostResponse
            PostBlogPostOG(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blogposts")] BlogPost blogPost,
            HttpRequestData request,
            //[CosmosDBOutput("SwaBlog", "BlogContainer", Connection = "CosmosDbConnectionString")] out dynamic savedBlogPost,
            ILogger log)
        {
            if (blogPost.Id != default)
            {
                //var result = TypedResults.BadRequest("id must be null");
                var responseData = request.CreateResponse(HttpStatusCode.BadRequest);
                responseData.WriteString("id must be null");
                //var responseData = request.FunctionContext.GetHttpResponseData();
                //response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                //response.WriteString("id must be null");
                //HttpResponse response = request.FunctionContext.GetHttpContext().Response;
                BadRequest<string> resultBadRequest = TypedResults.BadRequest("id must be null");
                SavedBlogPostResponse savedBlogPostResponseBadRequest = new()
                {
                    HttpResult = resultBadRequest,
                    SavedBlogPost = null
                };
                return savedBlogPostResponseBadRequest;
            }

            //var dict = request.FunctionContext.GetHttpContext().Request.Headers;
            var dict = request.Headers.ToDictionary((kvp) => kvp.Key, kvp => new StringValues(kvp.Value.ToArray()));
            var clientPrincipal = StaticWebAppApiAuthorization.ParseHttpHeaderForClientPrincipal(request.Headers);

            //HeaderDictionary headers = new HeaderDictionary(dict);
            //var ClientPrincipal = StaticWebAppApiAuthorization.ParseHttpHeaderForClientPrincipal(headers);
            blogPost.Id = Guid.NewGuid();
            blogPost.Author = clientPrincipal.UserDetails;

            var savedBlogPost = new
            {
                id = blogPost.Id,
                Title = blogPost.Title,
                Author = blogPost.Author,
                PublishedDate = blogPost.PublishedDate,
                tags = blogPost.Tags,
                BlogPostMarkdown = blogPost.BlogPostMarkdown,
                Status = 2
            };
            SavedBlogPostResponse savedBlogPostResponse = new() { HttpResult = TypedResults.Ok(blogPost), SavedBlogPost = savedBlogPost };
            return savedBlogPostResponse;
        }

        [Function($"{nameof(BlogPosts)}_Post")]
        public static SavedBlogPostResponse PostBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blogposts")] BlogPost blogPost,
            HttpRequest request,
            ILogger log
            )
        {
            if (blogPost.Id != default)
            {
                BadRequest<string> resultBadRequest = TypedResults.BadRequest("id must be null");
                SavedBlogPostResponse savedBlogPostResponseBadRequest = new()
                {
                    HttpResult = resultBadRequest,
                    SavedBlogPost = null
                };
                return savedBlogPostResponseBadRequest;
            }
            //{ savedBlogPost = null; return new BadRequestObjectResult("id must be null"); }
            var clientPrincipal =
                StaticWebAppApiAuthorization.ParseHttpHeaderForClientPrincipal(request.Headers);
            blogPost.Id = Guid.NewGuid();
            blogPost.Author = clientPrincipal.UserDetails;
            var savedBlogPost = new
            {
                id = blogPost.Id,
                Title = blogPost.Title,
                Author = blogPost.Author,
                PublishedDate = blogPost.PublishedDate,
                tags = blogPost.Tags,
                BlogPostMarkdown = blogPost.BlogPostMarkdown,
                Status = 2
            };
            SavedBlogPostResponse savedBlogPostResponse = new() { HttpResult = TypedResults.Ok(blogPost), SavedBlogPost = blogPost };
            return savedBlogPostResponse;
            //            return new OkObjectResult(blogPost);
        }

        [Function($"{nameof(BlogPosts)}_Put")]
        public static SavedBlogPostResponse PutBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous,"put",Route ="blogposts")]//Route ="blogposts/{author}/{id}"
        HttpRequest request,
            //[Microsoft.Azure.Functions.Worker.FromBodyAttribute] 
        BlogPost updatedBlogPost,
            [CosmosDBInput("SwaBlog","BlogContainer",Connection ="CosmosDbConnectionString",Id ="{Id}",PartitionKey ="{Author}")]
        BlogPosts currentBlogPost,
            ILogger log
            )
        {
            //updatedBlogPost
            if (currentBlogPost is null)
            {
                string id = request.Query["id"];//or Id, Author ?
                string author = request.Query["author"];
                NotFound<string> resultNotfound = TypedResults.NotFound($"BlogPost author {author}, id {id} not found");
                SavedBlogPostResponse savedBlogPostResponseBadRequest = new()
                {
                    HttpResult = resultNotfound,
                    SavedBlogPost = null
                };
                return savedBlogPostResponseBadRequest;
                //savedBlogPost = null; 
                //return new NotFoundResult(); 
            }
            var savedBlogPost = new
            {
                id = updatedBlogPost.Id,
                Title = updatedBlogPost.Title,
                Author = updatedBlogPost.Author,
                PublishedDate = updatedBlogPost.PublishedDate,
                tags = updatedBlogPost.Tags,
                BlogPostMarkdown = updatedBlogPost.BlogPostMarkdown,
                Status = 2
            };
            SavedBlogPostResponse savedBlogPostResponse = new()
            {
                HttpResult = TypedResults.NoContent(),
                SavedBlogPost = savedBlogPost
            };
            return savedBlogPostResponse;
        }

        [Function($"{nameof(BlogPosts)}_Delete")]
        public static async Task<IResult> DeleteBlogPost(

            [HttpTrigger(AuthorizationLevel.Anonymous,"delete",
                Route ="blogPosts/{author}/{id}")]
            HttpRequest request,

            string author,
            string id,

            [ CosmosDBInput("SwaBlog","BlogContainer",
            Connection = "CosmosDbConnectionString",
            Id="{id}", PartitionKey="{author}")]
            BlogPosts currentBlogPost/**/,

            [FromServices] CosmosClient client,
            ILogger log)
        {
            if (currentBlogPost is null)
            {
                return TypedResults.NoContent();
            }
            Container container =
                client.GetDatabase("SwaBlog")
            .GetContainer("BlogContainer");
            await container
                .DeleteItemAsync<BlogPosts>(id, new PartitionKey(author));
            return TypedResults.NoContent();
        }

        [Function($"{nameof(BlogPosts)}_GetId")]
        public static IActionResult GetBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous,
            "get", Route ="blogposts/{author}/{id}")]HttpRequest req,
            [CosmosDBInput("SwaBlog","BlogContainer",
            Connection = "CosmosDbConnectionString",
            SqlQuery = @"
Select c.id, c.Title, c.Author, c.PublishedDate,
c.BlogPostMarkdown,
c.Status,
c.Tags
FROM c
WHERE c.id = {id} and c.Author = {author}")]IEnumerable<BlogPosts> BlogPosts, //case..
            ILogger log
            )
        {
            if (BlogPosts.ToArray().Length == 0) { return new NotFoundResult(); }
            return new OkObjectResult(BlogPosts.First());
        }
    }
}
