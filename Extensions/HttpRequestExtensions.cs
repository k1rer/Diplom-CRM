namespace Diplom_CRM.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsHtmxRequest(this HttpRequest request)
            => request.Headers.ContainsKey("HX-Request");
    }
}
