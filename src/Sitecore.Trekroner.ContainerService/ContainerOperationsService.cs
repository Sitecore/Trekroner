using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Docker.DotNet;

namespace Sitecore.Trekroner.ContainerService
{
    public class ContainerOperationsService : ContainerOperations.ContainerOperationsBase
    {
        public override async Task<InspectContainerResponse> InspectContainer(InspectContainerRequest request, ServerCallContext context)
        {
            var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
            var thisContainerId = Environment.GetEnvironmentVariable("COMPUTERNAME").ToLower();
            var thisContainer = await client.Containers.InspectContainerAsync(thisContainerId);
            var composeProject = thisContainer.Config.Labels["com.docker.compose.project"];
            var targetContainer = (await client.Containers.ListContainersAsync(new Docker.DotNet.Models.ContainersListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "label", new Dictionary<string, bool>
                        {
                            { $"com.docker.compose.service={request.Name}", true },
                            { $"com.docker.compose.project={composeProject}", true }
                        }
                    }
                }
            })).FirstOrDefault();
            var result = await client.Containers.InspectContainerAsync(targetContainer.ID);
            return new InspectContainerResponse
            {
                Id = result.ID,
                State = new ContainerState
                {
                    Status = result.State.Status,
                    Running = result.State.Running,
                    Paused = result.State.Paused,
                    Restarting = result.State.Paused,
                    OomKilled = result.State.OOMKilled,
                    Dead = result.State.Dead,
                    Pid = result.State.Pid,
                    ExitCode = result.State.ExitCode,
                    Error = result.State.Error,
                    StartedAt = result.State.StartedAt,
                    FinishedAt = result.State.FinishedAt,
                }
            };
        }
    }
}
