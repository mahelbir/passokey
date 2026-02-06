namespace Application.Common;

public static class UriHelper
{
    public static bool IsValidHttpUri(this Uri uri)
    {
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    public static Uri? ToUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null;
    }

}