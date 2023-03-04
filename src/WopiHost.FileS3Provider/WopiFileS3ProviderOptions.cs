namespace WopiHost.FileS3Provider;

/// <summary>
/// Configuration object for <see cref="WopiFileS3Provider"/>.
/// </summary>
public class WopiFileS3ProviderOptions
{
    public string KeyPrefix { get; set; } = string.Empty;

    public string AccessKey { get; set; }

    public string SecretKey { get; set; }

    public string RegionName { get; set; }

    public string BucketName { get; set; }
}
