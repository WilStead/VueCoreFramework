namespace VueCoreFramework.Models.ViewModels
{
    /// <summary>
    /// Used to transfer information about data sharing.
    /// </summary>
    public class ShareViewModel
    {
        public string Level { get; set; }
        public string Name { get; set; }
        public string ShortLevel => Level.Substring(Level.LastIndexOf('/') + 1);
        public string Type { get; set; }
    }
}
