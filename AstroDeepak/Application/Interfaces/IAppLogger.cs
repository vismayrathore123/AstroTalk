    using System.Runtime.CompilerServices;

    namespace AstroDeepak.Application.Interfaces
    {
        /// <summary>
        /// Simple file-based logger used across the whole app (services + pages).
        /// Never throws - a logging failure must never crash the app.
        /// </summary>
        public interface IAppLogger
        {
            void LogInfo(string message, [CallerMemberName] string member = "");
            void LogWarning(string message, [CallerMemberName] string member = "");
            void LogError(string message, Exception? ex = null, [CallerMemberName] string member = "");
            void LogDebug(string message, [CallerMemberName] string member = "");
        }
    }
