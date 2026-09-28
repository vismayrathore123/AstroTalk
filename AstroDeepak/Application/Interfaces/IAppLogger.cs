    using System.Runtime.CompilerServices;

    namespace AstroDeepak.Application.Interfaces
    {
       public interface IAppLogger
        {
            void LogInfo(string message, [CallerMemberName] string member = "");
            void LogWarning(string message, [CallerMemberName] string member = "");
            void LogError(string message, Exception? ex = null, [CallerMemberName] string member = "");
            void LogDebug(string message, [CallerMemberName] string member = "");
        }
    }
