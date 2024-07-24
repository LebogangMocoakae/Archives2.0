using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archives2._0.Services
{
    public class AzureResourceServices
    {
        private readonly IAzure _azure;
       
        public AzureResourceServices(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {
            var credentials = SdkContext.AzureCredentialsFactory
                              .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            _azure = Microsoft.Azure.Management.Fluent.Azure
                    .Authenticate(credentials)
                    .WithSubscription(subscriptionId);
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




    }
}
