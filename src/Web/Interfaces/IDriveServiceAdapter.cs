using System.Threading.Tasks;

namespace Web.Interfaces
{
    public interface IDriveServiceAdapter
    {
        Task<string> CreateFolderAsync(string name);
        Task<string> GetFolderIdByNameAsync(string name);
        Task<string> UploadFileToFolder(string filePath, string folderId, string mimeType);
        Task<bool> AlreadyShared(string fileId);
        Task<string> ShareFile(string fileId, string email);
        Task DeleteAllFiles();
    }
}
