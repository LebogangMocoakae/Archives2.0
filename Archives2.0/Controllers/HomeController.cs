using Archives2._0.Models;
using Archives2._0.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Archives2._0.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AzureResourceServices _azureVmService;
        private readonly ILogger<HomeController> _logger;
        private readonly AzureResourceServices _storageService;

        public HomeController(ILogger<HomeController> logger, AzureResourceServices azureVmService, AzureResourceServices storageService)
        { 
            _logger = logger;
            _azureVmService = azureVmService;
            _storageService = storageService;
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Dashboard()
        {

            return View();
        }

        public IActionResult CreateStorage()
        {
            return View();
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

        public async Task<IActionResult> Index()
        {
            var vms = await _azureVmService.GetVirtualMachinesAsync();
            return View(vms);
        }

        public async Task<IActionResult> Storage(string path = "", string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var model = await _storageService.GetItemsAsync(path);

            // Filter by blob name if searchTerm is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                foreach (var container in model.Containers)
                {
                    container.Items = container.Items
                        .Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            // Paginate the items
            foreach (var container in model.Containers)
            {
                container.Items = container.Items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm = "", string path = "", int pageNumber = 1, int pageSize = 10)
        {
            var model = await _storageService.GetItemsAsync(path);

            // Filter by blob name if searchTerm is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                foreach (var container in model.Containers)
                {
                    container.Items = container.Items
                        .Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            // Paginate the items
            foreach (var container in model.Containers)
            {
                container.Items = container.Items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

            var result = new
            {
                Containers = model.Containers,
                PageNumber = pageNumber,
                HasNextPage = model.Containers.Any(c => c.Items.Count == pageSize)
            };

            return Json(result);
        }

        public async Task<IActionResult> Download(string containerName, string blobName)
        {
            try
            {
                var blobDownloadInfo = await _storageService.DownloadBlobAsync(containerName, blobName);

                if (blobDownloadInfo == null)
                {
                    return NotFound();
                }

                return File(blobDownloadInfo.Content, blobDownloadInfo.ContentType, blobName);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpPost]
        public async Task<IActionResult> VMDeploy(string vmName, string adminUsername, string adminPassword, string osType)
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "JSONDeployments", "EntryLevel.json");

            var json = System.IO.File.ReadAllText(jsonFilePath);
            var template = JObject.Parse(json);


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
