namespace Owin.HealthCheck
{
    using System.Text;

    /// <summary>Writes the health checks as a simple key/value pair string.</summary>
    internal class SimpleResponseWriter : IResponseWriter
    {
        /// <inheritdoc />
        public string WriteResponse(dynamic results)
        {
            var sb = new StringBuilder();
            foreach (var r in results)
                sb.AppendLine(r.Name + ": " + r.Status.Message);
            return sb.ToString();
        }
    }
}