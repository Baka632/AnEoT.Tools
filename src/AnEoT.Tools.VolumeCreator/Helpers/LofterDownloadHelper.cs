namespace AnEoT.Tools.VolumeCreator.Helpers;

/// <summary>
/// 为 Lofter 的内容下载提供帮助的类。
/// </summary>
public static class LofterDownloadHelper
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<string> GetPageHtml(Uri pageUri, string cookie)
    {
        using HttpResponseMessage response = await GetHttpResponseMessage(pageUri, pageUri, cookie);

        string content = await response.Content.ReadAsStringAsync();
        return content;
    }

    /// <summary>
    /// 获取指定的 Lofter 图像。
    /// </summary>
    /// <param name="imageUri">图像本身的 Uri。</param>
    /// <param name="sourcePageUri">图像所在页面的 Uri。</param>
    /// <param name="cookie">用于登陆 Lofter 的 Cookie。</param>
    /// <returns>一个包含图像内容的 <see cref="Stream"/>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="imageUri"/> 或 <paramref name="sourcePageUri"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="HttpRequestException">HTTP 请求出错。</exception>
    public static async Task<Stream> GetImage(Uri imageUri, Uri sourcePageUri)
    {
        using HttpResponseMessage response = await GetHttpResponseMessage(imageUri, sourcePageUri);

        using Stream content = await response.Content.ReadAsStreamAsync();
        content.Seek(0, SeekOrigin.Begin);

        MemoryStream stream = new();
        content.CopyTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    private static async Task<HttpResponseMessage> GetHttpResponseMessage(Uri targetUri, Uri refererUri, string? cookie = null)
    {
        ArgumentNullException.ThrowIfNull(targetUri);
        ArgumentNullException.ThrowIfNull(refererUri);

        if (!targetUri.IsAbsoluteUri)
        {
            throw new ArgumentOutOfRangeException(nameof(targetUri), "目标 URI 必须为绝对 URI。");
        }

        if (!refererUri.IsAbsoluteUri)
        {
            throw new ArgumentOutOfRangeException(nameof(targetUri), "Referer URI 必须为绝对 URI。");
        }

        HttpRequestMessage message = new(HttpMethod.Get, targetUri);
        message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0");
        message.Headers.Add("Referer", $"{refererUri.Scheme}://{refererUri.Host}");
        if (!string.IsNullOrWhiteSpace(cookie))
        {
            message.Headers.Add("Cookie", cookie);
        }

        HttpResponseMessage response = await HttpClient.SendAsync(message);
        response.EnsureSuccessStatusCode();

        return response;
    }
}
