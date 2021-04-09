using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Sitecore.Trekroner.ContainerService
{
    public static class DockerEngineExtensions
    {

        /// <summary>
        /// Gets the id of the currently running container. Not actually an extension method.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentContainerId()
        {
            return Environment.GetEnvironmentVariable(Constants.ComputerNameEnvironmentVariable).ToLower();
        }

        public static string GetComposeProjectName(this ContainerInspectResponse containerInspectResponse)
        {
            return containerInspectResponse.Config.Labels[Constants.LabelComposeProject];
        }
    }
}
