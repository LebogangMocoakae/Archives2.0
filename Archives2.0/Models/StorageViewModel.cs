namespace Archives2._0.Models
{
    public class StorageViewModel
    {
        public List<ContainerViewModel> Containers { get; set; }
    }

    public class ContainerViewModel
    {
        public string Name { get; set; }
        public List<ItemViewModel> Items { get; set; }
    }

    public class ItemViewModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
    }

}
