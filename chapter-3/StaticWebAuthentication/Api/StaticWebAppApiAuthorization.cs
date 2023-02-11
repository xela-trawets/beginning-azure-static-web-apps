using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using StaticWebAuthentication.Models;

namespace StaticWebAuthentication.Api;

public class StaticWebAppApiAuthorization
{
	public static ClientPrincipal ParseHttpHeaderForClientPrincipal(IHeaderDictionary headers)
	{
		if (!headers.TryGetValue("x-ms-client-principal", out var header)) {
			return new ClientPrincipal();
		}
		var data = header[0];//empty?
		var decoded = Convert.FromBase64String(data);
		var json = Encoding.UTF8.GetString(decoded);
		var jso = new JsonSerializerOptions(JsonSerializerDefaults.General) { PropertyNameCaseInsensitive = true };
		var principal = JsonSerializer.Deserialize<ClientPrincipal>(json, jso);
		return principal ?? new ClientPrincipal();
	}
}
