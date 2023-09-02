namespace build.Commands;

using System.CommandLine;
using System.CommandLine.IO;
using Configuration;
using Docker.DotNet;
using Docker.DotNet.Models;
using static SimpleExec.Command;

public sealed class DevDependenciesCommand : Command
{
    private const string InitFile = ".init.build.dev-dependencies.user";

    public readonly Option<bool> InitializeProfileOption = new(new[] { "--force-init", "-i" },
        "Execute the docker compose files using the 'init' profile to initialize resources");

    public readonly Option<string[]> NetworksOption = new(new[] { "--networks", "-n" },
        () => new[] { "local-shared" },
        "Required docker networks for local development");

    // ref https://github.com/dotnet/command-line-api/issues/840#issuecomment-609517717
    public readonly Option<Dictionary<string, string>> ServicesOption = new(new[] { "--services", "-s" },
        () => new Dictionary<string, string>
        {
            { "postgres-jobs", "postgres" },
            { "grafana-jobs", "monitoring" },
            { "rabbitmq-jobs", "rabbitmq" }
        },
        "Required docker resources for local development");

    public readonly Option<bool> TeardownOption = new(new[] { "--teardown", "-t" },
        "Teardown resources");

    public DevDependenciesCommand() : base("dev-dependencies", "Dev dependencies")
    {
        AddOption(InitializeProfileOption);
        AddOption(TeardownOption);
        AddOption(NetworksOption);
        AddOption(ServicesOption);

        this.SetHandler(async context =>
        {
            // pre-processing
            var requiredNetworks = context.ParseResult.GetValueForOption(NetworksOption);
            var requiredServices = context.ParseResult.GetValueForOption(ServicesOption);
            var teardown = context.ParseResult.GetValueForOption(TeardownOption);
            var forceInit = context.ParseResult.GetValueForOption(InitializeProfileOption);

            var workingDirectory = context.GetWorkingDirectory();
            var dockerServicesRoot = Path.Join(workingDirectory, "docker");
            var initializationArtifactPath = Path.Join(dockerServicesRoot, InitFile);

            using var client = new DockerClientConfiguration().CreateClient();
            var networkList = await client.Networks.ListNetworksAsync();
            var serviceList = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            if (teardown)
            {
                foreach (var service in requiredServices!)
                {
                    var path = Path.Join(workingDirectory, "docker", service.Value, "docker-compose.yaml");
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    // there's no API for Docker Compose, must run from CLI
                    // ref https://github.com/dotnet/Docker.DotNet/issues/222
                    context.Console.Out.WriteLine($"Shutting down and removing service '{service.Key}' ({path})");
                    await RunAsync("docker-compose", $"-f {path} down -v");
                }

                File.Delete(initializationArtifactPath);
                return;
            }

            // ensure networks
            foreach (var network in requiredNetworks!)
            {
                if (networkList.Select(response => response.Name).Contains(network))
                {
                    context.Console.Out.WriteLine($"Docker network '{network}' exists");
                }
                else
                {
                    context.Console.Out.WriteLine($"Creating Docker network '{network}'");
                    await client.Networks.CreateNetworkAsync(new NetworksCreateParameters
                    {
                        Name = network
                    });
                }
            }

            // ensure services

            // detect initialization

            if (forceInit || !File.Exists(initializationArtifactPath))
            {
                await File.Create(initializationArtifactPath).DisposeAsync();
                forceInit = true;
            }
            else
            {
                context.Console.Out.WriteLine(
                    $"Service initialization detected. To force initialization, delete the file '{InitFile}' or pass the '--force-init' argument through this command.");
            }

            foreach (var service in requiredServices!)
            {
                var path = Path.Join(workingDirectory, "docker", service.Value, "docker-compose.yaml");
                if (!File.Exists(path))
                {
                    continue;
                }

                // ref: https://docs.docker.com/engine/api/v1.43/#tag/Container/operation/ContainerList
                var container = serviceList.FirstOrDefault(response =>
                    response.State == "running"
                    && response.Labels.TryGetValue("com.docker.compose.service", out var name)
                    && name == service.Key);

                if (container == null)
                {
                    // there's no API for Docker Compose, must run from CLI
                    // ref https://github.com/dotnet/Docker.DotNet/issues/222
                    var initString = forceInit ? " --profile init" : string.Empty;
                    var initMessage = forceInit ? " and initializing" : string.Empty;
                    context.Console.Out.WriteLine($"Starting{initMessage} service '{service.Key}' ({path})");
                    await RunAsync("docker-compose", $"{initString} -f {path} up -d");
                }
                else
                {
                    context.Console.Out.WriteLine(
                        $"Docker service '{service.Key}' ({string.Join(',', container.Names)}) is running");
                }
            }
        });
    }
}