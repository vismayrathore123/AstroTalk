namespace AstroDeepak.Application.Interfaces
{
    public interface IDownloadsPathProvider
    {
      Task<string> GetDownloadsFolderAsync();
    }
}