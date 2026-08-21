    using System.Text;
    using AstroDeepak.Application.Interfaces;

    namespace AstroDeepak.Infrastructure.Logging
    {
        /// <summary>
        /// Writes one log file per day: log-yyyy-MM-dd.txt
        ///
        /// Path resolution:
        ///  - Windows : C:\astrotalk\astrotalkdeepak\Logs   (exactly what was asked for)
        ///  - Android/iOS : there is no C:\ drive and apps cannot write outside their own
        ///    sandbox, so we fall back to the app's private data folder
        ///    (FileSystem.AppDataDirectory)\astrotalkdeepak\Logs. This keeps the same
        ///    "one file per day" behaviour and the same folder name on every platform,
        ///    it just lives in a location each OS actually allows.
        /// </summary>
        public class FileAppLogger : IAppLogger
        {
            private static readonly object _lock = new();
            private readonly string _logDirectory;

            public FileAppLogger()
            {
                _logDirectory = ResolveLogDirectory();
                try
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                catch
                {
                    // If even the folder can't be created, logging silently becomes a no-op
                    // instead of crashing the app.
                }
            }

            private static string ResolveLogDirectory()
            {
    #if WINDOWS
                return @"C:\astrotalk\astrotalkdeepak\Logs";
    #else
                return Path.Combine(FileSystem.AppDataDirectory, "astrotalkdeepak", "Logs");
    #endif
            }

            private string CurrentLogFilePath()
                => Path.Combine(_logDirectory, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

            public void LogInfo(string message, string member = "") => Write("INFO", message, null, member);
            public void LogWarning(string message, string member = "") => Write("WARN", message, null, member);
            public void LogDebug(string message, string member = "") => Write("DEBUG", message, null, member);
            public void LogError(string message, Exception? ex = null, string member = "") => Write("ERROR", message, ex, member);

            private void Write(string level, string message, Exception? ex, string member)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    sb.Append(" [").Append(level).Append(']');
                    sb.Append(" (").Append(member).Append(") ");
                    sb.Append(message);

                    if (ex != null)
                    {
                        sb.Append(" | Exception: ").Append(ex.GetType().Name)
                          .Append(" - ").Append(ex.Message)
                          .Append(Environment.NewLine).Append(ex.StackTrace);
                    }

                    var line = sb.ToString();

                    lock (_lock)
                    {
                        File.AppendAllText(CurrentLogFilePath(), line + Environment.NewLine, Encoding.UTF8);
                    }

                    System.Diagnostics.Debug.WriteLine(line);
                }
                catch
                {
                    // Logging must never throw and take the app down with it.
                }
            }
        }
    }
