using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A base class for database objects. Provides a GUID primary key.
    /// </summary>
    /// <remarks>
    /// The SPA framework doesn't require using a base class like this, it's just used for
    /// convenience in the examples.
    /// </remarks>
    public class DataItem
    {
        /// <summary>
        /// The unique ID (primary key) of the item.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
    }
}
