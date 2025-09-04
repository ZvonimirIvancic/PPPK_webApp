using Microsoft.AspNetCore.Components.Forms;

namespace PrviProjekt.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IBrowserFile file, string uploadPath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);
        Task<string> GetContentTypeAsync(string fileName);
        bool IsValidImageFile(string fileName);
        bool IsValidMedicalFile(string fileName);
        Task<IEnumerable<string>> GetAllowedExtensions();
        Task<string> GenerateUniqueFileName(string originalFileName);
        Task<byte[]> ReadFileAsync(string filePath);
    }
}
