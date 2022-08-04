
namespace SawtoothBrowser.Utils
{
    public static class Extensions
    {
        public static string Shorten(this string? text, int upto)
        {
            if (text == null) return "";
            if (text.Length <= upto) return text;
            return text.Substring(0, upto) + "...";
        }
    }
}
