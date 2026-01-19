using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Garage_2.Extensions
{
    public static class TimeSpanExtensions
    {
        // https://stackoverflow.com/a/1925560
        public static int GetYears(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Days / 365.2425);
        }
    }
}
