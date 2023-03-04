using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using WopiHost.Abstractions;

namespace WopiHost.FileS3Provider;

/// <summary>
/// Provides files and folders based on a base64-encoded paths.
/// </summary>
public class WopiFileS3Provider : IWopiStorageProvider
{
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;
    private readonly string _ownerId;

    private string KeyPrefix { get; }
    public IWopiFolder RootContainerPointer => new WopiFolder(KeyPrefix, EncodeIdentifier(KeyPrefix));

    /// <summary>
    /// Creates a new instance of the <see cref="WopiFileS3Provider"/> based on the provided hosting environment and configuration.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    public WopiFileS3Provider(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var s3Options = configuration.GetSection(WopiConfigurationSections.STORAGE_OPTIONS).Get<WopiFileS3ProviderOptions>();

        KeyPrefix = s3Options.KeyPrefix;
        _s3Client = new AmazonS3Client(
            new BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey),
            RegionEndpoint.GetBySystemName(s3Options.RegionName));

        _bucketName = s3Options.BucketName;

        _ownerId = _s3Client.GetACLAsync(_bucketName).GetAwaiter().GetResult().AccessControlList.Owner.Id;
    }

    /// <summary>
    /// Gets a file using an identifier.
    /// </summary>
    /// <param name="identifier">A base64-encoded file path.</param>
    public async Task<IWopiFile> GetWopiFile(string identifier)
    {
        var key = KeyPrefix + DecodeIdentifier(identifier);

        var metaResp = await _s3Client.GetObjectMetadataAsync(_bucketName, key);

        return new WopiFile
        {
            Key = key,
            Identifier = identifier,
            Name = Path.GetFileName(key),
            Length = metaResp.ContentLength,
            Extension = Path.GetExtension(key)[1..],
            LastWriteTimeUtc = metaResp.LastModified,
            Owner = _ownerId,
            Sha256 = metaResp.ETag
        };
    }

    /// <summary>
    /// Gets a folder using an identifier.
    /// </summary>
    /// <param name="identifier">A base64-encoded folder path.</param>
    public Task<IWopiFolder> GetWopiContainer(string identifier = "")
    {
        var keyPath = DecodeIdentifier(identifier);
        return Task.FromResult<IWopiFolder>(new WopiFolder(KeyPrefix + keyPath, identifier));
    }

    /// <summary>
    /// Gets all files in a folder.
    /// </summary>
    /// <param name="identifier">A base64-encoded folder path.</param>
    public async Task<List<IWopiFile>> GetWopiFiles(string identifier = "")
    {
        var folderPath = DecodeIdentifier(identifier);
        var files = new List<IWopiFile>();

        var fileInfoList = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request()
        {
            BucketName = _bucketName,
            Prefix = KeyPrefix + folderPath,
            MaxKeys = 1000
        });

        foreach (var fileInfo in fileInfoList.S3Objects.Where(x => x.Size > 0))
        {
            var extension = Path.GetExtension(fileInfo.Key)?[1..];
            if (new [] { "pdf", "docx", "xlsx", "pptx" }.Contains(extension))
            {
                files.Add(new WopiFile
                {
                    Key = fileInfo.Key,
                    Identifier = EncodeIdentifier(fileInfo.Key),
                    Name = Path.GetFileName(fileInfo.Key),
                    Length = fileInfo.Size,
                    Extension = extension,
                    LastWriteTimeUtc = fileInfo.LastModified,
                    Owner = _ownerId,
                    Sha256 = fileInfo.ETag
                });
            }
        }
        return files;
    }

    /// <summary>
    /// Gets all sub-folders of a folder.
    /// </summary>
    /// <param name="identifier">A base64-encoded folder path.</param>
    public async Task<List<IWopiFolder>> GetWopiContainers(string identifier = "")
    {
        var folderPath = DecodeIdentifier(identifier);
        var folders = new List<IWopiFolder>();

        var fileInfoList = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request()
        {
            BucketName = _bucketName,
            Prefix = KeyPrefix + folderPath
        });

        foreach (var directory in fileInfoList.CommonPrefixes)
        {
            var folderId = EncodeIdentifier(directory);
            folders.Add(await GetWopiContainer(folderId));
        }
        return folders;
    }

    /// <summary>
    /// Put the file to container
    /// </summary>
    public async Task PutWopiFile(string identifier, Stream stream)
    {
        var filePath = DecodeIdentifier(identifier);
        var key = KeyPrefix + filePath;

        await _s3Client.PutObjectAsync(new PutObjectRequest()
        {
            InputStream = stream,
            AutoCloseStream = true,
            BucketName = _bucketName,
            Key = key,
            //ChecksumAlgorithm = ChecksumAlgorithm.SHA256,

        });
    }

    /// <summary>
    /// Get file stream
    /// </summary>
    public async Task<Stream> GetWopiFileStream(string identifier)
    {
        var filePath = DecodeIdentifier(identifier);
        var key = KeyPrefix + filePath;
        var s3ObjResp = await _s3Client.GetObjectAsync(_bucketName, key);
        return s3ObjResp.ResponseStream;
    }

    private static string DecodeIdentifier(string identifier)
    {
        var bytes = Convert.FromBase64String(identifier);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string EncodeIdentifier(string path)
    {
        var bytes = Encoding.UTF8.GetBytes(path);
        return Convert.ToBase64String(bytes);
    }
}
