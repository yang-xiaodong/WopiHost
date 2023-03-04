using WopiHost.Abstractions;

namespace WopiHost.FileS3Provider;

/// <inheritdoc/>
public record WopiFile : IWopiFile
{
    public string Key { get; init; }

    /// <inheritdoc/>
    public string Identifier { get; init; }

    /// <inheritdoc />
    public bool Exists => !string.IsNullOrEmpty(Key);

    /// <inheritdoc/>
    public string Extension { get; init; }

    /// <inheritdoc/>
    public string Sha256 { get; init; }

    /// <inheritdoc/>
    public long Length { get; init; }

    /// <inheritdoc/>
    public string Name { get; init; }

    /// <inheritdoc/>
    public DateTime LastWriteTimeUtc { get; init; }

    public string Owner { get; init; }

    ///// <summary>
    ///// Creates an instance of <see cref="WopiFile"/>.
    ///// </summary>
    ///// <param name="key">Path on the file system the file is located in.</param>
    ///// <param name="fileIdentifier">Identifier of a file.</param>
    //public WopiFile(string key, string fileIdentifier)
    //{
    //    Key = key;
    //    Identifier = fileIdentifier;
    //}
}
