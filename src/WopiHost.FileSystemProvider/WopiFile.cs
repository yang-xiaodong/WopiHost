using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;
using WopiHost.Abstractions;

namespace WopiHost.FileSystemProvider;

/// <inheritdoc/>
public class WopiFile : IWopiFile
{
    private FileInfo _fileInfo;

    private string FilePath { get; set; }

    private FileInfo FileInfo => _fileInfo ??= new FileInfo(FilePath);

    /// <inheritdoc/>
    public string Identifier { get; }

    /// <inheritdoc />
    public bool Exists => FileInfo.Exists;

    /// <inheritdoc/>
    public string Extension
    {
        get
        {
            var ext = FileInfo.Extension;
            if (ext.StartsWith(".", StringComparison.InvariantCulture))
            {
                ext = ext[1..];
            }
            return ext;
        }
    }

    private static readonly SHA256 Sha = SHA256.Create();

    /// <inheritdoc/>
    public string Sha256
    {
        get
        {
            using var stream = FileInfo.OpenRead();
            var checksum = Sha.ComputeHash(stream);
            return Convert.ToBase64String(checksum);
        }
    }

    /// <inheritdoc/>
    public long Length => FileInfo.Length;

    /// <inheritdoc/>
    public string Name => FileInfo.Name;

    /// <inheritdoc/>
    public DateTime LastWriteTimeUtc => FileInfo.LastWriteTimeUtc;

    /// <summary>
    /// Creates an instance of <see cref="WopiFile"/>.
    /// </summary>
    /// <param name="filePath">Path on the file system the file is located in.</param>
    /// <param name="fileIdentifier">Identifier of a file.</param>
    public WopiFile(string filePath, string fileIdentifier)
    {
        FilePath = filePath;
        Identifier = fileIdentifier;
    }
    
    /// <summary>
    /// A string that uniquely identifies the owner of the file.
    /// Supported only on Windows and Linux.
    /// https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
    /// </summary>
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("windows")]
    public string Owner
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return FileInfo.GetAccessControl().GetOwner(typeof(NTAccount)).ToString();
            }
            else if (OperatingSystem.IsLinux())
            {
                return Mono.Unix.UnixFileSystemInfo.GetFileSystemEntry(FilePath).OwnerUser.UserName; //TODO: test
            }
            else
            {
                return "UNSUPPORTED_PLATFORM";
            }
        }
    }
}
