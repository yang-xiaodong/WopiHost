using System.Globalization;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WopiHost.Abstractions;
using WopiHost.Core.Models;

namespace WopiHost.Core.Controllers;

/// <summary>
/// Implementation of WOPI server protocol https://msdn.microsoft.com/en-us/library/hh659001.aspx
/// </summary>
[Route("wopi/[controller]")]
public class ContainersController : WopiControllerBase
{
    /// <summary>
    /// Creates an instance of <see cref="ContainersController"/>.
    /// </summary>
    /// <param name="storageProvider">Storage provider instance for retrieving files and folders.</param>
    /// <param name="securityHandler">Security handler instance for performing security-related operations.</param>
    public ContainersController(IWopiStorageProvider storageProvider, IWopiSecurityHandler securityHandler) : base(storageProvider, securityHandler)
    {
    }

    /// <summary>
    /// Returns the metadata about a container specified by an identifier.
    /// Specification: https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/containers/checkcontainerinfo
    /// Example URL path: /wopi/containers/(container_id)
    /// </summary>
    /// <param name="id">Container identifier.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<CheckContainerInfo> GetCheckContainerInfo(string id)
    {
        var container =await StorageProvider.GetWopiContainer(id);
        return new CheckContainerInfo
        {
            Name = container.Name
        };
    }

    /// <summary>
    /// The EnumerateChildren method returns the contents of a container on the WOPI server.
    /// Specification: https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/containers/enumeratechildren
    /// Example URL path: /wopi/containers/(container_id)/children
    /// </summary>
    /// <param name="id">Container identifier.</param>
    /// <returns></returns>
    [HttpGet("{id}/children")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<Container> EnumerateChildren(string id)
    {
        var container = new Container();
        var files = new List<ChildFile>();
        var containers = new List<ChildContainer>();

        foreach (var wopiFile in await StorageProvider.GetWopiFiles(id))
        {
            files.Add(new ChildFile
            {
                Name = wopiFile.Name,
                Url = GetWopiUrl("files", wopiFile.Identifier, AccessToken),
                LastModifiedTime = wopiFile.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture),
                Size = wopiFile.Length,
                Version = wopiFile.LastWriteTimeUtc.ToString("s", CultureInfo.InvariantCulture)
            });
        }

        foreach (var wopiContainer in await StorageProvider.GetWopiContainers(id))
        {
            containers.Add(new ChildContainer
            {
                Name = wopiContainer.Name,
                Url = GetWopiUrl("containers", wopiContainer.Identifier, AccessToken)
            });
        }

        container.ChildFiles = files;
        container.ChildContainers = containers;

        return container;
    }
}
