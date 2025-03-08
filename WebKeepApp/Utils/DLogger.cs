using System.Diagnostics;

namespace WebKeepApp.Utils
{
    public static class DLogger
    {
        private const string DefaultTag = "WebKeepAppDebug";

        public static void Log(string message, Exception? ex = null, string tag = DefaultTag)
        {
            var frame = new StackTrace().GetFrame(1);
            var method = frame?.GetMethod()?.Name ?? "UnknownMethod";

#if ANDROID
            if (ex != null)
            {
                Android.Util.Log.Error(tag, $"{method}: {message}");
                Android.Util.Log.Error(tag, $"Exception: {ex.Message}");
                Android.Util.Log.Error(tag, $"Stack: {ex.StackTrace}");
            }
            else
            {
                Android.Util.Log.Debug(tag, $"{method}: {message}");
            }
#endif
        }
    }
}