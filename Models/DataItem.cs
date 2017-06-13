using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// Represents a database object which can be displayed automatically by the framework.
    /// </summary>
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

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// The default constructor attempts to set required properties to a default value, and
        /// properties with a min value to that min. This includes invoking the default parameterless
        /// constructor for required child <see cref="DataItem"/> objects. Because of this behavior,
        /// it is important to mark at least one navigation property in any relationship as virtual,
        /// to avoid an infinite loop.
        /// </remarks>
        public DataItem()
        {
            foreach (var pInfo in GetType().GetTypeInfo().GetProperties())
            {
                // Skip virtual navigation properties to avoid infinite loops.
                if (pInfo.GetGetMethod().IsVirtual) continue;

                var req = pInfo.GetCustomAttribute<RequiredAttribute>();
                object min = pInfo.GetCustomAttribute<RangeAttribute>()?.Minimum;
                if (req != null)
                {
                    if (pInfo.PropertyType == typeof(string))
                    {
                        var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
                        if (dataType?.CustomDataType == "Color")
                        {
                            pInfo.SetValue(this, "#000000");
                        }
                        else
                        {
                            StringBuilder val = new StringBuilder("[None]");
                            if (min != null && (int)min > 6)
                            {
                                for (int i = 0; i < (int)min - 6; i++)
                                {
                                    val.Append(".");
                                }
                            }
                            pInfo.SetValue(this, val.ToString());
                        }
                    }
                    else if (pInfo.PropertyType == typeof(DateTime))
                    {
                        var minDT = min == null ? DateTime.MinValue : DateTime.Parse((string)min);
                        pInfo.SetValue(this, minDT);
                    }
                    else if (pInfo.PropertyType == typeof(DataItem) ||
                        pInfo.PropertyType.GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                    {
                        pInfo.SetValue(this, pInfo.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(new object[] { }));
                    }
                }
                if (min != null)
                {
                    if (pInfo.PropertyType.IsNumeric())
                    {
                        pInfo.SetValue(this, min);
                    }
                    else if (pInfo.PropertyType == typeof(TimeSpan))
                    {
                        var minTS = min == null ? TimeSpan.Zero : TimeSpan.Parse((string)min);
                        pInfo.SetValue(this, minTS);
                    }
                }
            }
        }
    }
}
