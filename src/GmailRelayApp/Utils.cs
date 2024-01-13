public static class Utils
{
    // The URL- and filename-safe Base64 encoding described in RFC 4648
    // https://stackoverflow.com/a/11743162/857997
    public static byte[] FromBase64Url(string base64Url)
    {
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}