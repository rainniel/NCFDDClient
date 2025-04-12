namespace NCFDDClient.Utils
{
    internal static class Logger
    {
        private static string _lastLog = string.Empty;
        private static bool _logMuted = false;

        public static void LogInfo(string message)
        {
            Log($"INFO: {message}");
        }

        public static void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }

        public static void LogError(string message)
        {
            Log($"ERROR: {message}");
        }

        private static void Log(string message)
        {
            if (_lastLog == message)
            {
                if (!_logMuted)
                {
                    _logMuted = true;
                    Console.WriteLine(message);
                    Console.WriteLine("Log message repeated, muting repeating logs.");
                }
            }
            else
            {
                _logMuted = false;
                Console.WriteLine(message);
            }

            _lastLog = message;
        }
    }
}