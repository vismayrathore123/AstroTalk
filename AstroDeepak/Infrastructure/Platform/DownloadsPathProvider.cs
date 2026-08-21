using AstroDeepak.Application.Interfaces;

namespace AstroDeepak.Infrastructure.Platform
{
    public class DownloadsPathProvider : IDownloadsPathProvider
    {
        public Task<string> GetDownloadsFolderAsync()
        {
#if WINDOWS
            // .NET has no Environment.SpecialFolder.Downloads, so it's built manually.
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloads = Path.Combine(userProfile, "Downloads");
            Directory.CreateDirectory(downloads);
            return Task.FromResult(downloads);

#elif ANDROID
            string folder;
            try
            {
                // Best effort: the real, public "Downloads" folder that shows up in the
                // Files app / a file manager. Requires WRITE_EXTERNAL_STORAGE with
                // android:maxSdkVersion="28" declared in AndroidManifest.xml for older
                // OS versions (see notes). On API 29+ writing our own new file here
                // generally still works without extra runtime permission.
                var publicDownloads = global::Android.OS.Environment
                    .GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryDownloads)!
                    .AbsolutePath;
                Directory.CreateDirectory(publicDownloads);
                folder = publicDownloads;
            }
            catch
            {
                // Fallback: app-private external "Download" folder. Always writable,
                // needs no permission at all, but is only visible via a file manager
                // with "show app data" turned on - not the public Downloads app.
                var context = global::Android.App.Application.Context;
                var appDownloads = context.GetExternalFilesDir(global::Android.OS.Environment.DirectoryDownloads)!.AbsolutePath;
                Directory.CreateDirectory(appDownloads);
                folder = appDownloads;
            }
            return Task.FromResult(folder);

#elif IOS || MACCATALYST
            // True system Downloads/Files access on iOS requires the user to pick a
            // location through UIDocumentPickerViewController - there is no silent
            // write path into it (Apple sandboxing). The closest silent equivalent is
            // the app's own Documents folder, which appears in the Files app under
            // "On My iPhone/AstroDeepak" once these two Info.plist keys are set to YES:
            //   UIFileSharingEnabled = YES
            //   LSSupportsOpeningDocumentsInPlace = YES
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Directory.CreateDirectory(documents);
            return Task.FromResult(documents);

#else
            var fallback = Path.Combine(FileSystem.AppDataDirectory, "Downloads");
            Directory.CreateDirectory(fallback);
            return Task.FromResult(fallback);
#endif
        }
    }
}