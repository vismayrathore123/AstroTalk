namespace AstroDeepak.Application.Interfaces
{
    public interface IDownloadsPathProvider
    {
        /// <summary>
        /// Returns a writable folder suitable for "download straight to disk, no dialog".
        ///  - Windows: the user's real Downloads folder.
        ///  - Android: the public Downloads folder (best effort), falling back to the
        ///    app's own external Download folder if the public one can't be written to.
        ///  - iOS: iOS never lets an app silently write into the system Files/Downloads
        ///    location - only the user can do that, via a picker dialog, which would
        ///    break the "no dialog" requirement. The closest silent equivalent is the
        ///    app's own Documents folder, which becomes visible in the Files app under
        ///    "On My iPhone/AstroDeepak" once file sharing is enabled in Info.plist
        ///    (see notes in DownloadsPathProvider.cs).
        /// </summary>
        Task<string> GetDownloadsFolderAsync();
    }
}