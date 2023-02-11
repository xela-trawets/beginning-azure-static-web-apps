using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace StaticWebAppAuthentication.Client;

public static class StaticWebAppAuthenticationExtensions
{
	public static IServiceCollection AddStaticWebAppsAuthentication(this IServiceCollection services)
	{
		return services.AddAuthorizationCore()
			.AddScoped<AuthenticationStateProvider, StaticWebAppsAuthenticationStateProvider>();
	}
}
