namespace Owin.HealthCheck
{
    /// <summary>The Response Writer interface.</summary>
    public interface IResponseWriter
    {
        /// <summary>Formats the response to the client.</summary>
        /// <param name="results">The results.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string WriteResponse(dynamic results);
    }
}
