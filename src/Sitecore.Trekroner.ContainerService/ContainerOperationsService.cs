using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf.WellKnownTypes;

namespace Sitecore.Trekroner.ContainerService
{
    public class ContainerOperationsService : ContainerOperations.ContainerOperationsBase
    {
        private readonly DockerClient DockerClient;

        public ContainerOperationsService()
        {
            DockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        public override async Task<ListContainersResponse> ListContainers(ListContainersRequest request, ServerCallContext context)
        {
            // determine the current compose project
            var composeProject = await GetComposeProjectName();

            // find containers by compose project name
            var containers = await ListContainers(new Dictionary<string, string>
            {
                { Constants.LabelComposeProject, composeProject }
            });
            var response = new ListContainersResponse
            {
                ProjectName = composeProject,
                Containers = {
                    containers.Select(x => new ListContainersResponseItem
                    {
                        Id = x.ID,
                        Names = { x.Names },
                        ComposeService = x.Labels[Constants.LabelComposeService],
                        ComposeProject = x.Labels[Constants.LabelComposeProject],
                        Image = x.Image,
                        State = x.State,
                        Status = x.Status
                    })
                }
            };
            return response;
        }

        public override async Task<InspectContainerResponse> InspectContainer(InspectContainerRequest request, ServerCallContext context)
        {
            // determine the current compose project
            var composeProject = await GetComposeProjectName();

            // find the container by its compose project name
            var targetContainer = (await ListContainers(new Dictionary<string,string>
            {
                { Constants.LabelComposeService, request.Name },
                { Constants.LabelComposeProject, composeProject }
            })).FirstOrDefault();

            // inspect the container by its id
            var result = await DockerClient.Containers.InspectContainerAsync(targetContainer.ID);
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
                    Health = new ContainerHealth
                    {
                        Status = result.State.Health?.Status ?? string.Empty,
                        FailingStreak = result.State.Health?.FailingStreak ?? 0,
                        Log = {
                            result.State.Health?.Log?.Select(x => new ContainerHealthcheckResult
                            {
                                Start = Timestamp.FromDateTime(x.Start.ToUniversalTime()),
                                End = Timestamp.FromDateTime(x.End.ToUniversalTime()),
                                ExitCode = x.ExitCode,
                                Output = x.Output ?? string.Empty
                            }) ?? Array.Empty<ContainerHealthcheckResult>()
                        }
                    }
                }
            };
        }

        private async Task<string> GetComposeProjectName()
        {
            var thisContainerId = DockerEngineExtensions.GetCurrentContainerId();
            var thisContainer = await DockerClient.Containers.InspectContainerAsync(thisContainerId);
            return thisContainer.GetComposeProjectName();
        }

        private async Task<IList<ContainerListResponse>> ListContainers(IDictionary<string,string> labelValues)
        {
            var labelsDictionary = labelValues.Select(x => new KeyValuePair<string, bool>(
                $"{x.Key}={x.Value}",
                true
            )).ToDictionary(x => x.Key, x => x.Value);

            return await DockerClient.Containers.ListContainersAsync(new Docker.DotNet.Models.ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "label", labelsDictionary
                    }
                }
            });
        }
    }
}
