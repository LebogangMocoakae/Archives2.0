using Archives2._0.Models;
using Archives2._0.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace Archives2._0.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AzureResourceServices _azureVmService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AzureResourceServices azureVmService)
        { 
            _logger = logger;
            _azureVmService = azureVmService;
        }

        public async Task<IActionResult> Index()
        {
            var vms = await _azureVmService.GetVirtualMachinesAsync();
            return View(vms);
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Dashboard()
        {

            return View();
        }

        public async Task<IActionResult> Admin()
        {
            var vms = await _azureVmService.GetVirtualMachinesAsync();
            return View(vms);
        }

        public async Task<IActionResult> VMAdmin()
        {
            var vms = await _azureVmService.GetVirtualMachinesAsync();
            return View(vms);
        }

        public async Task<IActionResult> VMEdit(string id, string action)
        {
            if (action == "Restart")
            {
                // Restart the VM 
                await _azureVmService.RestartVirtualMachineAsync(id);
            }
            else if (action == "Power On")
            {
                // Turn on VM
                await _azureVmService.TurnOnVirtualMachineAsync(id);
            }
            else if (action == "Power Off")
            {
                // Turn on VM
                await _azureVmService.TurnOffVirtualMachineAsync(id);
            }

            // Fetch VM details from the service based on the provided id
            var vm = await _azureVmService.GetVirtualMachineByIdAsync(id);

            if (vm == null)
            {
                // Handle case where VM is not found (optional)
                return NotFound(); // Or handle as appropriate for your application
            }

                // Return the updated VM details to the "VMEdit" view
             return View("VMEdit", vm);
        }

        public async Task<IActionResult> DownloadRdp(String id)
        {
            Helpers.GenerateRdpFileContent fileContent = new Helpers.GenerateRdpFileContent();

            var vm = await _azureVmService.GetVirtualMachineByIdAsync(id);
            if (vm == null)
            {
                return NotFound();
            }
            
            var publicIpAddress = vm.GetPrimaryPublicIPAddress().IPAddress;
            var adminUsername = vm.OSProfile.AdminUsername;
            var rdpContent = fileContent.generateWindowsRdp(publicIpAddress, adminUsername);
            var rdpFileName = $"{vm.Name}.rdp";
            var byteArray = System.Text.Encoding.UTF8.GetBytes(rdpContent);
            var stream = new System.IO.MemoryStream(byteArray);

            return File(stream, "application/x-rdp", rdpFileName);
        }

        public IActionResult VMCreate()
        {

            return View();
        }

        [HttpGet]

        public IActionResult VMDeploy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VMDeploy(string vmName, string adminUsername, string adminPassword, string osType)
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "JSONDeployments", "EntryLevel.json");

            var json = System.IO.File.ReadAllText(jsonFilePath);
            var template = JObject.Parse(json);

            // Update the template with form values
            template["parameters"]["vmName"]["defaultValue"] = vmName;
            template["parameters"]["adminUsername"]["defaultValue"] = adminUsername;
            template["parameters"]["adminPassword"]["defaultValue"] = adminPassword;
            template["parameters"]["osType"]["defaultValue"] = osType;

            // Deploy resources from the template
            await _azureVmService.DeployResourcesFromTemplateAsync(template.ToString());

            return View(); // Create a view to show deployment success
        }
    
    [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }      
    }
}
