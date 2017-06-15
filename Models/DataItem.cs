using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// Represents a database object which can be displayed automatically by the SPA framework.
    /// </summary>
    /// <remarks>
    /// All datatypes used by the framework *must* be a subclass of DataItem in order to function.
    /// Any which are not will be represented merely by text labels. Bear in mind when subclassing
    /// that the framework creates new items by invoking the default parameterless constructor, and
    /// also invoking the constructor for any required child objects.
    /// </remarks>
    public class DataItem
    {
        /// <summary>
        /// The unique ID (primary key) of the item.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// The date/time when the item was created.
        /// </summary>
        /// <remarks>
        /// This is set when the item is created by a user on their client, not at time of database insert.
        /// </remarks>
        [Hidden]
        public DateTime CreationTimestamp { get; set; }

        /// <summary>
        /// The date/time when the item was last updated.
        /// </summary>
        /// <remarks>
        /// This is set when the item is sent for update by a user on their client, not at time of database update.
        /// </remarks>
        [Hidden]
        public DateTime UpdateTimestamp { get; set; }

        /// <summary>
        /// Indicates permissions granted for this item to all users.
        /// </summary>
        /// <remarks>
        /// This may either be <see cref="CustomClaimTypes.PermissionDataAll"/>, or may be any number
        /// of specific operation permission types, delimited by semicolons (;).
        /// </remarks>
        [Hidden]
        public string AllPermissions { get; set; }
    }
}
