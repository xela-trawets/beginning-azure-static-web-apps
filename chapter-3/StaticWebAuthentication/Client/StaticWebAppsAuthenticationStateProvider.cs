using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;

using StaticWebAppAuthentication.Models;

namespace StaticWebAppAuthentication.Client;

public class StaticWebAppsAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly HttpClient http;
	public StaticWebAppsAuthenticationStateProvider(HttpClient httpClient)
	{
		this.http = new HttpClient() { BaseAddress = new Uri( "http://localhost:4280" )};// ?? throw new ArgumentNullException();
	}
	public static ClaimsPrincipal GetClaimsFromClientPrincipal(ClientPrincipal principal)
	{
		principal.UserRoles = principal.UserRoles?.Except(new[] { "anonymous" }, StringComparer.OrdinalIgnoreCase)
				?? new List<string>();
		if (!principal.UserRoles.Any()) { return new ClaimsPrincipal(); }
		ClaimsIdentity identity = AdaptToClaimsIdentity(principal);
		return new ClaimsPrincipal(identity);
	}

	public async Task<ClientPrincipal> GetClientPrincipal()
	{
		var data = await http.GetFromJsonAsync<AuthenticationData>("/.auth/me");
		var clientPrincipal = data?.ClientPrincipal ?? new ClientPrincipal();
		return clientPrincipal;
	}

	private static ClaimsIdentity AdaptToClaimsIdentity(ClientPrincipal principal)
	{
		var identity = new ClaimsIdentity(principal.IdentityProvider);
		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
		identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
		identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role,r)));
		return identity;
	}
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		try {
			var clientPrincipal = await GetClientPrincipal();
			var claimsPrincipal = GetClaimsFromClientPrincipal(clientPrincipal);
			return new AuthenticationState(claimsPrincipal);
		}
		catch { return new AuthenticationState(new ClaimsPrincipal()); }
	}
}

