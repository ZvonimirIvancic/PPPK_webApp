using Microsoft.AspNetCore.Components.Forms;

namespace PrviProjekt.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;
        private readonly string _basePath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _basePath = _configuration["FileStorage:BasePath"] ?? "wwwroot/uploads";
            _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 10485760); 
            _allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };

            // Ensure upload directory exists
            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> UploadFileAsync(IBrowserFile file, string uploadPath)
        {
            try
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file));

                if (file.Size > _maxFileSize)
                    throw new InvalidOperationException($"File size ({file.Size} bytes) exceeds maximum allowed size ({_maxFileSize} bytes)");

                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    throw new InvalidOperationException($"File type '{extension}' is not allowed");

                var uniqueFileName = await GenerateUniqueFileName(file.Name);
                var fullUploadPath = Path.Combine(_basePath, uploadPath);

                Directory.CreateDirectory(fullUploadPath);

                var filePath = Path.Combine(fullUploadPath, uniqueFileName);

                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(_maxFileSize).CopyToAsync(fileStream);

                _logger.LogInformation($"File uploaded successfully: {filePath}");
                return Path.Combine(uploadPath, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file?.Name}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted successfully: {fullPath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_basePath, filePath);
            return File.Exists(fullPath);
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    return fileInfo.Length;
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file size: {filePath}");
                return 0;
            }
        }

        public async Task<string> GetContentTypeAsync(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public bool IsValidImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension);
        }

        public bool IsValidMedicalFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public async Task<IEnumerable<string>> GetAllowedExtensions()
        {
            return _allowedExtensions;
        }

        public async Task<string> GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];

            return $"{nameWithoutExtension}_{timestamp}_{guid}{extension}";
        }

        public async Task<byte[]> ReadFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    return await File.ReadAllBytesAsync(fullPath);
                }
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading file: {filePath}");
                throw;
            }
        }
    }
}

