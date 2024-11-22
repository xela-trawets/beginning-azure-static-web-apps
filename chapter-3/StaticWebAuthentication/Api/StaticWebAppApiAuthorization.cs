using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net.Http.Headers;
using StaticWebAppAuthentication.Models;

namespace StaticWebAppAuthentication.Api;

public class StaticWebAppApiAuthorization
{
    public static ClientPrincipal ParseHttpHeaderForClientPrincipal(HttpHeadersCollection headers)
    {
        //var header = headers. ["x-ms-client-principal"];
        var ok = headers.TryGetValues("x-ms-client-principal", out var headerValues);
        if (!ok)
        {
            return new ClientPrincipal();
        }
        var data = headerValues.FirstOrDefault("");//empty?
        var decoded = Convert.FromBase64String(data);
        var json = Encoding.UTF8.GetString(decoded);
        var jso = new JsonSerializerOptions(JsonSerializerDefaults.General) { PropertyNameCaseInsensitive = true };
        var principal = JsonSerializer.Deserialize<ClientPrincipal>(json, jso);
        return principal ?? new ClientPrincipal();
    }
    public static ClientPrincipal ParseHttpHeaderForClientPrincipal(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("x-ms-client-principal", out var header))
        {
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
