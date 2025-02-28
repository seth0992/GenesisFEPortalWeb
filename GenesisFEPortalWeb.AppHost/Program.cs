var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.GenesisFEPortalWeb_ApiService>("apiservice");

builder.AddProject<Projects.GenesisFEPortalWeb_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
