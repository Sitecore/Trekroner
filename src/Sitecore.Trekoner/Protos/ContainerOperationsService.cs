using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Docker.DotNet;

namespace Sitecore.Trekroner.Protos
{
    public class ContainerOperationsService : ContainerOperations.ContainerOperationsBase
    {
        public override async Task<InspectContainerResponse> InspectContainer(InspectContainerRequest request, ServerCallContext context)
        {
            var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
            var result = await client.Containers.InspectContainerAsync(request.Id);
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
