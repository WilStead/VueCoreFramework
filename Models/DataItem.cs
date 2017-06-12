using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace MVCCoreVue.Models
{
    public class DataItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Editable(false)]
        [Hidden]
        public Guid Id { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime CreationTimestamp { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime UpdateTimestamp { get; set; }

        [Display(AutoGenerateField = false)]
        public string AllPermissions { get; set; }

        public DataItem()
        {
            // Default constructor attempts to set required properties to
            // a default value, and properties with a min value to that min.

            // This includes invoking the default parameterless constructor
            // for required nested DataItem objects.
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
