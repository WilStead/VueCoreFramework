namespace VueCoreFramework.Models.ViewModels
{
    /// <summary>
    /// Used to transfer information about data sharing.
    /// </summary>
    public class ShareViewModel
    {
        /// <summary>
        /// Indicates the type of sharing (e.g. 'permission/data/view').
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// The name of the user or group with whom the data is shared.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates the short form of the sharing type (e.g. 'view').
        /// </summary>
        public string ShortLevel => Level.Substring(Level.LastIndexOf('/') + 1);

        /// <summary>
        /// Indicates whether the data is shared with a user or group.
        /// </summary>
        public string Type { get; set; }
    }
}
