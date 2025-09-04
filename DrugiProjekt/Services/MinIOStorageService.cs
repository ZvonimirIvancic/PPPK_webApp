using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;

namespace DrugiProjekt.Services
{
    public class MinIOStorageService : IMinIOStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinIOStorageService> _logger;

        public MinIOStorageService(IMinioClient minioClient, ILogger<MinIOStorageService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task<bool> UploadTSVFileAsync(string bucketName, string objectName, string filePath)
        {
            try
            {
                _logger.LogInformation($"Uploading file {filePath} to bucket {bucketName} as {objectName}");

                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File not found: {filePath}");
                    return false;
                }

                var fileInfo = new FileInfo(filePath);
                var contentType = "text/tab-separated-values";

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                _logger.LogInformation($"Successfully uploaded {objectName} to {bucketName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {filePath} to MinIO");
                return false;
            }
        }

        public async Task<Stream> DownloadTSVFileAsync(string bucketName, string objectName)
        {
            try
            {
                _logger.LogInformation($"Downloading {objectName} from bucket {bucketName}");

                var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(async stream => await stream.CopyToAsync(memoryStream));

                await _minioClient.GetObjectAsync(getObjectArgs);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {objectName} from MinIO");
                throw;
            }
        }

        public async Task<IEnumerable<string>> ListTSVFilesAsync(string bucketName)
        {
            try
            {
                var objects = new List<string>();
                var listObjectsArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithRecursive(true);

                await foreach (var item in _minioClient.ListObjectsAsync(listObjectsArgs))
                {
                    if (item.Key.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase) ||
                        item.Key.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        objects.Add(item.Key);
                    }
                }

                return objects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing files from bucket {bucketName}");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> DeleteTSVFileAsync(string bucketName, string objectName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);

                _logger.LogInformation($"Successfully deleted {objectName} from {bucketName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {objectName} from MinIO");
                return false;
            }
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
                return await _minioClient.BucketExistsAsync(bucketExistsArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if bucket {bucketName} exists");
                return false;
            }
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketExists = await BucketExistsAsync(bucketName);
                if (!bucketExists)
                {
                    var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(makeBucketArgs);
                    _logger.LogInformation($"Successfully created bucket {bucketName}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating bucket {bucketName}");
                return false;
            }
        }

        public async Task<long> GetFileSizeAsync(string bucketName, string objectName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                var objectStat = await _minioClient.StatObjectAsync(statObjectArgs);
                return objectStat.Size;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file size for {objectName} in {bucketName}");
                return 0;
            }
        }

        public async Task<Dictionary<string, object>> GetFileMetadataAsync(string bucketName, string objectName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                var objectStat = await _minioClient.StatObjectAsync(statObjectArgs);

                return new Dictionary<string, object>
                {
                    ["Size"] = objectStat.Size,
                    ["ContentType"] = objectStat.ContentType,
                    ["LastModified"] = objectStat.LastModifiedDateTime,
                    ["ETag"] = objectStat.ETag,
                    ["Metadata"] = objectStat.MetaData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting metadata for {objectName} in {bucketName}");
                return new Dictionary<string, object>();
            }
        }
    }
}
