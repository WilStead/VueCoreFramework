using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MVCCoreVue.Data
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

        public DataItem()
        {
            // Default constructor attempts to set required properties to
            // a default value. Only basic types can be set this way.
            // Derived classes should override the base constructor with more
            // appropriate defaults.
            foreach (var pInfo in GetType().GetTypeInfo().GetProperties())
            {
                var req = pInfo.GetCustomAttribute<RequiredAttribute>();
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
                            pInfo.SetValue(this, "[None]");
                        }
                    }
                    else if (pInfo.PropertyType == typeof(DateTime))
                    {
                        pInfo.SetValue(this, DateTime.MinValue);
                    }
                }
            }
        }
    }
}
