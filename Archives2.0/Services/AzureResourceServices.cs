using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Archives2._0.Models;

namespace Archives2._0.Services
{
    public class AzureResourceServices
    {
        private readonly IAzure _azure;
        private readonly string _connectionString;

        public AzureResourceServices(string clientId, string clientSecret, string tenantId, string subscriptionId, string connectionString)
        {
            var credentials = SdkContext.AzureCredentialsFactory
                              .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            _azure = Microsoft.Azure.Management.Fluent.Azure
                    .Authenticate(credentials)
                    .WithSubscription(subscriptionId);

            _connectionString = connectionString;
        }

        public async Task<StorageViewModel> GetItemsAsync(string path)
        {
            var containers = new List<ContainerViewModel>();
            var containerClient = new BlobServiceClient(_connectionString);

            await foreach (var containerItem in containerClient.GetBlobContainersAsync())
            {
                var blobs = new List<ItemViewModel>();
                var blobContainerClient = containerClient.GetBlobContainerClient(containerItem.Name);

                await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
                {
                    blobs.Add(new ItemViewModel
                    {
                        Name = blobItem.Name,
                        Path = blobItem.Name,
                        IsDirectory = false // Azure blobs do not have directories, this is for display purposes
                    });
                }

                containers.Add(new ContainerViewModel
                {
                    Name = containerItem.Name,
                    Items = blobs
                });
            }

            return new StorageViewModel
            {
                Containers = containers
            };
        }

        public async Task<IReadOnlyList<IVirtualMachine>> GetVirtualMachinesAsync()
        {
            var pagedCollection = await _azure.VirtualMachines.ListAsync();
            var vms = new List<IVirtualMachine>();

            foreach (var vm in pagedCollection)
            {
                vms.Add(vm);  
            }

            return vms.AsReadOnly();         
        }

        public async Task<IVirtualMachine> GetVirtualMachineByIdAsync(string vmId)
        {
            return await _azure.VirtualMachines.GetByIdAsync(vmId);
        }

        public async Task RestartVirtualMachineAsync(string vmId)
        {
            await _azure.VirtualMachines.GetById(vmId).RestartAsync();
        }

        public async Task TurnOnVirtualMachineAsync(string vmId)
        {
            await _azure.VirtualMachines.GetById(vmId).StartAsync();
        }

        public async Task TurnOffVirtualMachineAsync(string vmId)
        {
            await _azure.VirtualMachines.GetById(vmId).DeallocateAsync();
        }

        public async Task DeployResourcesFromTemplateAsync(string templateContent)
        {
            string groupName = "RG1";
            string deploymentName = "MyDeployment";

            var deployment = await _azure.Deployments
                                         .Define(deploymentName)
                                         .WithExistingResourceGroup(groupName)
                                         .WithTemplate(templateContent)
                                         .WithParameters("{}")
                                         .WithMode(DeploymentMode.Incremental)
                                         .CreateAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetContainersAndBlobsAsync()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            Dictionary<string, List<string>> containersAndBlobs = new Dictionary<string, List<string>>();

            await foreach (BlobContainerItem container in blobServiceClient.GetBlobContainersAsync())
            {
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container.Name);
                List<string> blobs = new List<string>();

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    blobs.Add(blobItem.Name);
                }

                containersAndBlobs.Add(container.Name, blobs);
            }

            return containersAndBlobs;
        }

        public async Task<BlobDownloadInfo> DownloadBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = new BlobServiceClient(_connectionString).GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.DownloadAsync();
        }

        public async Task<IEnumerable<BlobContainerItem>> GetAllContainersAsync()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            var containers = new List<BlobContainerItem>();

            await foreach (BlobContainerItem container in blobServiceClient.GetBlobContainersAsync())
            {
                containers.Add(container);
            }

            return containers;
        }


    }
}
