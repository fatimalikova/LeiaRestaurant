namespace Leia.Application.Helpers
{
    public static class TimeHelper
    {
        public static DateTime NowLocal() => DateTime.UtcNow.AddHours(4);
    }
}