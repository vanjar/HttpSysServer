using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Net.Http.Server;

namespace HelloWorld
{
    public static class RequestExtensions
    {
        private static readonly ISet<string> IgnoreRequestProperties = new HashSet<string> { "Body", "Headers" };
        private static readonly ISet<string> IgnoreContextProperties = new HashSet<string> { "Request", "Response", "DisconnectToken", "User" };

        public static string ToPrintable(this RequestContext context)
        {
            var properties = new List<Tuple<string, object>>();

            CollectProperties(context.Request, properties);
            CollectProperties(context.Request.Headers, properties);
            CollectProperties(context, properties);

            return Format(properties, context.Request);
        }

        private static void CollectProperties(Request request, List<Tuple<string, object>> properties)
            => properties.AddRange(typeof(Request).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !IgnoreRequestProperties.Contains(prop.Name))
                .OrderBy(prop => prop.Name)
                .Select(prop => Tuple.Create("Req." + prop.Name, prop.GetValue(request) ?? string.Empty)));

        private static void CollectProperties(HeaderCollection headers, List<Tuple<string, object>> properties)
            => properties.AddRange(headers.OrderBy(h => h.Key).Select(h => Tuple.Create("Hdr." + h.Key, h.Value.ToString() as object)));

        private static void CollectProperties(RequestContext context, List<Tuple<string, object>> properties)
            => properties.AddRange(typeof(RequestContext).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !IgnoreContextProperties.Contains(prop.Name))
                .OrderBy(prop => prop.Name)
                .Select(prop => Tuple.Create("Cxt." + prop.Name, prop.GetValue(context) ?? string.Empty)));

        /// <summary>
        /// Print a dictionary into a table with header
        /// </summary>
        private static string Format(List<Tuple<string, object>> properties, Request request)
        {
            var buf = new StringBuilder();

            // widths of the columns
            var c1 = Math.Min(30, properties.Max(p => p.Item1.Length));
            var c2 = Math.Min(80, properties.Max(p => p.Item2.ToString().Length));

            var header = $"  {{0,-{c1}}} | {{1,-{c2}}}\n";
            var separator = string.Join("-", Enumerable.Repeat(string.Empty, c1 + c2 + 6));

            buf.AppendLine(separator);
            buf.AppendLine("    NEW REQUEST");
            buf.AppendLine(separator);

            foreach (var pair in properties)
            {
                buf.AppendFormat(header, pair.Item1, pair.Item2);
            }

            if (request.HasEntityBody)
            {
                buf.AppendLine(separator);
                buf.AppendLine("    BODY");
                buf.AppendLine(separator);

                using (var sr = new StreamReader(request.Body))
                {
                    buf.AppendLine(sr.ReadToEnd());
                }
            }

            buf.AppendLine(separator);

            return buf.ToString();
        }
    }
}
