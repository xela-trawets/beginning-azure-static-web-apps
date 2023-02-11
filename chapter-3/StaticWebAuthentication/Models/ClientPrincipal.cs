using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticWebAuthentication.Models
{
	public class ClientPrincipal
	{
		public string IdentityProvider { get; set; }
		public string UserId { get; set; }
		public string UserDetails { get; set; }
		public IEnumerable<string> UserRoles { get; set; }
	}
}
