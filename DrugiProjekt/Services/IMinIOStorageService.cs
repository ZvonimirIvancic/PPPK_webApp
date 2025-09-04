namespace DrugiProjekt.Services
{
    public interface IMinIOStorageService
    {
        Task<bool> UploadTSVFileAsync(string bucketName, string objectName, string filePath);
        Task<Stream> DownloadTSVFileAsync(string bucketName, string objectName);
        Task<IEnumerable<string>> ListTSVFilesAsync(string bucketName);
        Task<bool> DeleteTSVFileAsync(string bucketName, string objectName);
        Task<bool> BucketExistsAsync(string bucketName);
        Task<bool> CreateBucketAsync(string bucketName);
        Task<long> GetFileSizeAsync(string bucketName, string objectName);
        Task<Dictionary<string, object>> GetFileMetadataAsync(string bucketName, string objectName);
    }
}
