using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
	public class BlogPost
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Author{ get; set; }
        //[JsonPropertyName("PublishedDate")]
		public DateTime PublishedDate{ get; set; }
		public string[] Tags{ get; set;}
		public string BlogPostMarkdown{ get; set; }
		public bool PreviewIsComplete { get; set; }

	}
}
