using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WopiHost.Abstractions;
using WopiHost.Core.Models;

namespace WopiHost.Core.Controllers;

/// <summary>
/// Implementation of WOPI server protocol https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/ecosystem/checkecosystem
/// </summary>
[Route("wopi/[controller]")]
	public class EcosystemController : WopiControllerBase
	{
        /// <summary>
        /// Creates an instance of <see cref="EcosystemController"/>.
        /// </summary>
        /// <param name="storageProvider">Storage provider instance for retrieving files and folders.</param>
        /// <param name="securityHandler">Security handler instance for performing security-related operations.</param>
        public EcosystemController(IWopiStorageProvider storageProvider, IWopiSecurityHandler securityHandler) 
        : base(storageProvider, securityHandler)
		{
		}

		/// <summary>
		/// The GetRootContainer operation returns the root container. A WOPI client can use this operation to get a reference to the root container, from which the client can call EnumerateChildren (containers) to navigate a container hierarchy.
		/// Specification: https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/ecosystem/getrootcontainer
		/// Example URL: GET /wopi/ecosystem/root_container_pointer
		/// </summary>
		/// <returns></returns>
		[HttpGet("root_container_pointer")]
		[Produces(MediaTypeNames.Application.Json)]
		public async Task<RootContainerInfo> GetRootContainer() //TODO: fix the path
		{
			var root =await StorageProvider.GetWopiContainer(@".\");
			var rc = new RootContainerInfo
			{
				ContainerPointer = new ChildContainer
				{
					Name = root.Name,
					Url = GetWopiUrl("containers", root.Identifier, AccessToken)
				}
			};
			return rc;
		}
	}
