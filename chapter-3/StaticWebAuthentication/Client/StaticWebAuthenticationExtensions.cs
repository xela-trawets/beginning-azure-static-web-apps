﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace StaticWebAuthentication.Client;

public static class StaticWebAuthenticationExtensions
{
	public static IServiceCollection AddStaticWebAppsAuthentication(this IServiceCollection services)
	{
		return services.AddAuthorizationCore()
			.AddScoped<AuthenticationStateProvider, StaticWebAppsAuthenticationStateProvider>();
	}
}
