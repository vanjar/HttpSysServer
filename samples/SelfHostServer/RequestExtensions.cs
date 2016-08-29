using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace SelfHostServer
{
    public static class RequestExtensions
    {
        public static string GetDigest(this HttpContext context)
        {
            var buf = new StringBuilder();
            buf.AppendLine($"Application received request on {DateTime.Now.ToString("hh:mm:ss")}");

            buf.AppendLine("====== RAW ==================================");
            var request = context.Request;
            buf.AppendLine($"{request.Method} {request.Scheme}{request.Host}{request.Path.Value}{request.QueryString} {request.Protocol}");
            foreach (var header in request.Headers)
            {
                buf.AppendLine($"{header.Key}: {header.Value}");
            }
            buf.AppendLine();
            using (var sr = new StreamReader(request.Body))
            {
                buf.AppendLine(sr.ReadToEnd());
            }

            buf.AppendLine("====== QUERY ================================");
            foreach (var query in request.Query)
            {
                buf.AppendLine($"{query.Key}: {query.Value}");
            }
            buf.AppendLine();

            buf.AppendLine("====== IDENTITY =============================");
            var user = context.User;
            foreach (var identity in user.Identities)
            {
                var authenticated = identity.IsAuthenticated ? "authenticated" : "unauthenticated";
                buf.AppendLine($"Identity: {authenticated} {identity.Name}");
                foreach (var claim in identity.Claims)
                {
                    buf.AppendLine($"  {claim.Type} : {claim.Value}");
                }
            }

            return buf.ToString();
        }
    }
}
