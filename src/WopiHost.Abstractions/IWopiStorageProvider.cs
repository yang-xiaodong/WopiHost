namespace WopiHost.Abstractions;

/// <summary>
/// Provides concrete instances of IWopiFiles.
/// </summary>
public interface IWopiStorageProvider
{
    /// <summary>
    /// Returns a concrete instance of an implementation of the <see cref="IWopiFile"/>.
    /// </summary>
    /// <param name="identifier">Generic string identifier of a file (typically some kind of a path).</param>
    /// <returns>Instance of a file.</returns>
    Task<IWopiFile> GetWopiFile(string identifier);

    /// <summary>
    /// Returns a concrete instance of an implementation of the <see cref="IWopiFolder"/>.
    /// </summary>
    /// <param name="identifier">Generic string identifier of a container (typically some kind of a path).</param>
    /// <returns>Instance of a container.</returns>
    Task<IWopiFolder> GetWopiContainer(string identifier = "");

    /// <summary>
    /// Returns all files from the given source.
    /// This method is very likely to change in the future.
    /// </summary>
    /// <param name="identifier">Container identifier (use null for root)</param>
    Task<List<IWopiFile>> GetWopiFiles(string identifier = "");

    /// <summary>
    /// Returns all containers from the given source.
    /// This method is very likely to change in the future.
    /// </summary>
    /// <param name="identifier">Container identifier (use null for root)</param>
    Task<List<IWopiFolder>> GetWopiContainers(string identifier = "");

    /// <summary>
    /// Put the file stream
    /// </summary>
    /// <param name="identifier">Container identifier (use null for root)</param>
    /// <param name="stream">File stream</param>
    Task PutWopiFile(string identifier, Stream stream);

    /// <summary>
    /// Get the file stream
    /// </summary>
    /// <param name="identifier">Container identifier (use null for root)</param>
    Task<Stream> GetWopiFileStream(string identifier);

    /// <summary>
    /// Reference to the root container.
    /// </summary>
    IWopiFolder RootContainerPointer { get; }
}