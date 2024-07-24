using System.ComponentModel.DataAnnotations;

namespace Archives2._0.Models
{
    public class CreateVmModel
    {
        [Required(ErrorMessage = "Please enter the virtual machine name")]
        [Display(Name = "Virtual Machine Name")]
        public string vmName { get; set; }

        [Required(ErrorMessage = "Please enter the admin username")]
        [Display(Name = "Admin Username")]
        public string adminUsername { get; set; }

        [Required(ErrorMessage = "Please enter the admin password")]
        [Display(Name = "Admin Password")]
        [DataType(DataType.Password)]
        public string adminPassword { get; set; }

        [Required(ErrorMessage = "Please select the operating system type")]
        [Display(Name = "Operating System Type")]
        public string osType { get; set; }
    }
}
