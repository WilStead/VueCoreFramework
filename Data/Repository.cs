using Microsoft.EntityFrameworkCore;
using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using MVCCoreVue.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    public class Repository<T> : IRepository where T : DataItem
    {
        private readonly ApplicationDbContext _context;

        private DbSet<T> items;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            items = _context.Set<T>();
        }

        public async Task<IDictionary<string, object>> AddAsync(DataItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await items.AddAsync(item as T);
            await _context.SaveChangesAsync();
            foreach (var reference in _context.Entry(item).References)
            {
                reference.Load();
            }
            foreach (var collection in _context.Entry(item).Collections)
            {
                collection.Load();
            }
            return GetViewModel(item as T);
        }

        public async Task<IDictionary<string, object>> AddToParentCollectionAsync(DataItem parent, PropertyInfo childProp, IEnumerable<DataItem> children)
        {
            var ptInfo = childProp.PropertyType.GetTypeInfo();
            var add = ptInfo.GetGenericTypeDefinition()
                        .MakeGenericType(ptInfo.GenericTypeArguments.FirstOrDefault())
                        .GetMethod("Add");

            foreach (var child in children)
            {
                add.Invoke(childProp.GetValue(parent), new object[] { child });
            }

            await _context.SaveChangesAsync();
            return GetViewModel(parent as T);
        }

        private static bool AnyPropMatch(DataItem item, string search)
        {
            var type = typeof(T);
            foreach (var pInfo in type.GetProperties())
            {
                // Only strings and numbers are checked, to avoid any
                // potentially expensive ToString operations on
                // potentially many rows.
                if (pInfo.PropertyType == typeof(string))
                {
                    if ((pInfo.GetValue(item) as string).IndexOf(search) != -1)
                    {
                        return true;
                    }
                }
                else if (pInfo.PropertyType == typeof(short)
                    || pInfo.PropertyType == typeof(int)
                    || pInfo.PropertyType == typeof(long)
                    || pInfo.PropertyType == typeof(float)
                    || pInfo.PropertyType == typeof(double)
                    || pInfo.PropertyType == typeof(decimal))
                {
                    if (pInfo.GetValue(item).ToString().IndexOf(search) != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<IDictionary<string, object>> FindAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var item = await items.FindAsync(id);
            foreach (var reference in _context.Entry(item).References)
            {
                reference.Load();
            }
            foreach (var collection in _context.Entry(item).Collections)
            {
                collection.Load();
            }
            return GetViewModel(item);
        }

        public async Task<DataItem> FindItemAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var item = await items.FindAsync(id);
            foreach (var reference in _context.Entry(item).References)
            {
                reference.Load();
            }
            foreach (var collection in _context.Entry(item).Collections)
            {
                collection.Load();
            }
            return item;
        }

        public IEnumerable<IDictionary<string, object>> GetAll()
        {
            IQueryable<T> filteredItems = items.AsQueryable();

            var tInfo = typeof(T).GetTypeInfo();
            foreach (var pInfo in tInfo.GetProperties())
            {
                var ptInfo = pInfo.PropertyType.GetTypeInfo();
                if (pInfo.PropertyType == typeof(DataItem)
                    || ptInfo.IsSubclassOf(typeof(DataItem))
                    || (ptInfo.IsGenericType
                    && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                    && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem))))
                {
                    filteredItems = filteredItems.Include(pInfo.Name);
                }
            }

            return filteredItems.Select(i => GetViewModel(i)).AsEnumerable();
        }

        private FieldDefinition GetFieldDefinition(PropertyInfo pInfo)
        {
            var fd = new FieldDefinition
            {
                Model = pInfo.Name.ToInitialLower()
            };

            var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
            var step = pInfo.GetCustomAttribute<StepAttribute>();
            var ptInfo = pInfo.PropertyType.GetTypeInfo();
            if (!string.IsNullOrEmpty(dataType?.CustomDataType))
            {
                if (dataType.CustomDataType == "Color")
                {
                    fd.Type = "vuetifyColor";
                }
                else
                {
                    fd.Type = "vuetifyText";
                    fd.InputType = "text";
                    fd.Validator = "string";
                }
            }
            else if (dataType != null)
            {
                switch (dataType.DataType)
                {
                    case DataType.Currency:
                        fd.Type = "vuetifyText";
                        fd.InputType = "number";
                        if (step != null)
                        {
                            fd.Step = step.Step;
                        }
                        else
                        {
                            fd.Step = 0.01;
                        }
                        fd.Validator = "number";
                        break;
                    case DataType.Date:
                        fd.Type = "vuetifyDateTime";
                        fd.InputType = "date";
                        break;
                    case DataType.DateTime:
                        fd.Type = "vuetifyDateTime";
                        fd.InputType = "dateTime";
                        break;
                    case DataType.Time:
                        fd.Type = "vuetifyDateTime";
                        fd.InputType = "time";
                        break;
                    case DataType.Duration:
                        fd.Type = "vuetifyTimespan";
                        var formatAttr = pInfo.GetCustomAttribute<DisplayFormatAttribute>();
                        fd.InputType = formatAttr?.DataFormatString;
                        fd.Validator = "timespan";
                        if (step != null)
                        {
                            fd.Step = step.Step;
                        }
                        else
                        {
                            fd.Step = 0.001;
                        }
                        break;
                    case DataType.EmailAddress:
                        fd.Type = "vuetifyText";
                        fd.InputType = "email";
                        fd.Validator = "email";
                        break;
                    case DataType.MultilineText:
                        fd.Type = "vuetifyText";
                        fd.InputType = "textArea";
                        fd.Rows = pInfo.GetCustomAttribute<RowsAttribute>()?.Rows;
                        fd.Validator = "string";
                        break;
                    case DataType.Password:
                        fd.Type = "vuetifyText";
                        fd.InputType = "password";
                        fd.Validator = "string";
                        break;
                    case DataType.PhoneNumber:
                        fd.Type = "vuetifyText";
                        fd.InputType = "telephone";
                        fd.Validator = "string";
                        break;
                    case DataType.PostalCode:
                        fd.Type = "vuetifyText";
                        fd.InputType = "text";
                        fd.Pattern = @"(^(?!0{5})(\d{5})(?!-?0{4})(|-\d{4})?$)";
                        fd.Validator = "string_regexp";
                        break;
                    case DataType.ImageUrl:
                    case DataType.Url:
                        fd.Type = "vuetifyText";
                        fd.InputType = "url";
                        fd.Validator = "string";
                        break;
                    default:
                        fd.Type = "vuetifyText";
                        fd.InputType = "text";
                        fd.Validator = "string";
                        break;
                }
            }
            else if (pInfo.PropertyType == typeof(string))
            {
                fd.Type = "vuetifyText";
                fd.InputType = "text";
                fd.Validator = "string";
            }
            else if (pInfo.PropertyType == typeof(bool))
            {
                fd.Type = "vuetifyCheckbox";
            }
            else if (pInfo.PropertyType == typeof(DateTime))
            {
                fd.Type = "vuetifyDateTime";
                fd.InputType = "dateTime";
            }
            else if (pInfo.PropertyType == typeof(TimeSpan))
            {
                fd.Type = "vuetifyTimespan";
                var formatAttr = pInfo.GetCustomAttribute<DisplayFormatAttribute>();
                fd.InputType = formatAttr?.DataFormatString;
                fd.Validator = "timespan";
                if (step != null)
                {
                    fd.Step = step.Step;
                }
                else
                {
                    fd.Step = 0.001;
                }
            }
            else if (pInfo.PropertyType == typeof(Guid))
            {
                fd.Type = "label";
                fd.HideInTable = true;
                fd.Visible = false;
            }
            else if (ptInfo.IsEnum)
            {
                fd.Type = "vuetifySelect";
                if (ptInfo.GetCustomAttribute<FlagsAttribute>() == null)
                {
                    fd.InputType = "single";
                }
                else
                {
                    fd.InputType = "multiple";
                }
                if (fd.Values == null)
                {
                    fd.Values = new List<ChoiceOption>();
                }
                foreach (var value in Enum.GetValues(pInfo.PropertyType))
                {
                    fd.Values.Add(new ChoiceOption
                    {
                        Text = EnumExtensions.GetDescription(pInfo.PropertyType, value),
                        Value = (int)value
                    });
                }
            }
            else if (pInfo.PropertyType.IsNumeric())
            {
                fd.Type = "vuetifyText";
                fd.InputType = "number";
                if (pInfo.PropertyType.IsRealNumeric())
                {
                    if (step != null)
                    {
                        fd.Step = step.Step;
                    }
                    else
                    {
                        fd.Step = 0.1;
                    }
                }
                else
                {
                    fd.Step = 1;
                }
                fd.Validator = "number";
            }
            else if (pInfo.PropertyType == typeof(DataItem) || ptInfo.IsSubclassOf(typeof(DataItem)))
            {
                fd.InputType = pInfo.PropertyType.Name.Substring(pInfo.PropertyType.Name.LastIndexOf('.') + 1).ToInitialLower();
                var menuAttr = ptInfo.GetCustomAttribute<MenuClassAttribute>();
                if (menuAttr != null)
                {
                    fd.Type = "objectSelect";
                }
                else
                {
                    fd.Type = "object";
                }
            }
            else if (ptInfo.IsGenericType
                && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
            {
                var name = ptInfo.GenericTypeArguments.FirstOrDefault().Name;
                fd.InputType = name.Substring(name.LastIndexOf('.') + 1).ToInitialLower();
                fd.Type = "objectCollection";
            }
            else if (ptInfo.IsGenericType
                && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
            {
                var name = ptInfo.GenericTypeArguments.FirstOrDefault().Name;
                fd.InputType = name.Substring(name.LastIndexOf('.') + 1).ToInitialLower();
                fd.Type = "objectMultiSelect";
            }
            else
            {
                fd.Type = "label";
            }

            if (fd.Type == "vuetifyText")
            {
                var textAttr = pInfo.GetCustomAttribute<TextAttribute>();
                fd.Icon = textAttr?.Icon;
                fd.Prefix = textAttr?.Prefix;
                fd.Suffix = textAttr?.Suffix;
            }
            else if (fd.Type == "vuetifyCheckbox" || fd.Type == "vuetifySelect")
            {
                var textAttr = pInfo.GetCustomAttribute<TextAttribute>();
                fd.Icon = textAttr?.Icon;
            }

            fd.Default = pInfo.GetCustomAttribute<DefaultAttribute>()?.Default;

            if (pInfo.GetCustomAttribute<EditableAttribute>()?.AllowEdit == false)
            {
                if (fd.Type == "vuetifyText"
                    && (fd.InputType == "text"
                    || fd.InputType == "url"
                    || fd.InputType == "telephone"
                    || fd.InputType == "email"
                    || fd.InputType == "password"
                    || fd.InputType == "number"))
                {
                    fd.Readonly = true;
                }
                else
                {
                    fd.Disabled = true;
                }
            }

            var display = pInfo.GetCustomAttribute<DisplayAttribute>();
            fd.GroupName = display?.GetGroupName();
            fd.Hint = display?.GetDescription();
            fd.Label = display?.GetName();
            fd.Placeholder = display?.GetPrompt();
            if (fd.Label == null && fd.Placeholder == null)
            {
                if (fd.Type == "vuetifyText" || fd.Type == "vuetifyCheckbox"
                    || fd.Type == "vuetifySelect" || fd.Type == "vuetifyDateTime")
                {
                    fd.Placeholder = pInfo.Name;
                }
                else
                {
                    fd.Label = pInfo.Name;
                }
            }
            if (display?.GetAutoGenerateField() == false)
            {
                fd.Visible = false;
                fd.HideInTable = true;
            }

            var help = pInfo.GetCustomAttribute<HelpAttribute>();
            if (!string.IsNullOrWhiteSpace(help?.HelpText))
                fd.Help = help?.HelpText;

            var hidden = pInfo.GetCustomAttribute<HiddenAttribute>();
            if (hidden?.Hidden == true) fd.Visible = false;

            var range = pInfo.GetCustomAttribute<RangeAttribute>();
            fd.Min = range?.Minimum;
            fd.Max = range?.Maximum;

            var pattern = pInfo.GetCustomAttribute<RegularExpressionAttribute>();
            if (!string.IsNullOrEmpty(pattern?.Pattern))
            {
                fd.Pattern = pattern.Pattern;
                fd.Validator = "string_regexp";
            }

            if (pInfo.GetCustomAttribute<RequiredAttribute>() != null)
            {
                fd.Required = true;
            }

            return fd;
        }

        public IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var type = typeof(T);
            foreach (var pInfo in type.GetProperties())
            {
                yield return GetFieldDefinition(pInfo);
            }
        }

        public IEnumerable<IDictionary<string, object>> GetPage(string search, string sortBy, bool descending, int page, int rowsPerPage, IEnumerable<Guid> except)
        {
            return GetPageItems(items.Where(i => !except.Contains(i.Id)), search, sortBy, descending, page, rowsPerPage);
        }

        public static IEnumerable<IDictionary<string, object>> GetPageItems(IQueryable<DataItem> items, string search, string sortBy, bool descending, int page, int rowsPerPage)
        {
            IQueryable<DataItem> filteredItems = items;

            var tInfo = typeof(T).GetTypeInfo();
            foreach (var pInfo in tInfo.GetProperties())
            {
                var ptInfo = pInfo.PropertyType.GetTypeInfo();
                if (pInfo.PropertyType == typeof(DataItem)
                    || ptInfo.IsSubclassOf(typeof(DataItem))
                    || (ptInfo.IsGenericType
                    && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                    && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem))))
                {
                    filteredItems = filteredItems.Include(pInfo.Name);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                filteredItems = items.Where(i => AnyPropMatch(i, search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                var sortProp = typeof(T).GetProperty(sortBy);
                if (sortProp == null)
                {
                    throw new ArgumentException($"{sortBy} is not a valid property for this item.", nameof(sortBy));
                }
                if (descending)
                {
                    filteredItems = filteredItems.OrderByDescending(i => sortProp.GetValue(i));
                }
                else
                {
                    filteredItems = filteredItems.OrderBy(i => sortProp.GetValue(i));
                }
            }

            if (rowsPerPage > 0)
            {
                if (page < 1)
                {
                    throw new ArgumentException($"{nameof(page)} cannot be < 1 if {nameof(rowsPerPage)} is > 0.", nameof(page));
                }
                filteredItems = filteredItems.Skip((page - 1) * rowsPerPage).Take(rowsPerPage);
            }

            return filteredItems.ToList().Select(i => GetViewModel(i));
        }

        public async Task<long> GetTotalAsync()
        {
            return await items.LongCountAsync();
        }

        private static IDictionary<string, object> GetViewModel(DataItem item)
        {
            IDictionary<string, object> vm = new Dictionary<string, object>();
            var tInfo = typeof(T).GetTypeInfo();
            foreach (var pInfo in tInfo.GetProperties())
            {
                var ptInfo = pInfo.PropertyType.GetTypeInfo();
                var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
                if (ptInfo.IsGenericType
                    && (ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                    || ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>)))
                    && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                {
                    int count = (int)ptInfo.GetGenericTypeDefinition()
                        .MakeGenericType(ptInfo.GenericTypeArguments.FirstOrDefault())
                        .GetProperty("Count")
                        .GetValue(pInfo.GetValue(item));
                    vm[pInfo.Name.ToInitialLower()] = count > 0 ? "[...]" : "[None]";
                }
                else if (ptInfo.IsEnum)
                {
                    object value = pInfo.GetValue(item);
                    var name = pInfo.Name.ToInitialLower();
                    vm[name] = (int)value;

                    var desc = EnumExtensions.GetDescription(pInfo.PropertyType, value);
                    vm[name + "Formatted"] = string.IsNullOrEmpty(desc) ? "[...]" : desc;
                }
                else if (dataType?.DataType == DataType.Date)
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("d");
                }
                else if (dataType?.DataType == DataType.Time)
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("t");
                }
                else if (dataType?.DataType == DataType.DateTime || pInfo.PropertyType == typeof(DateTime))
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("g");
                }
                else if (dataType?.DataType == DataType.Duration || pInfo.PropertyType == typeof(TimeSpan))
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (TimeSpan)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("c");
                }
                else if (pInfo.PropertyType == typeof(Guid)
                    || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
                {
                    object value = pInfo.GetValue(item);
                    if (Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
                    {
                        value = ((Guid?)value).Value;
                    }
                    if (value == null || (Guid)value == Guid.Empty)
                    {
                        vm[pInfo.Name.ToInitialLower()] = null;
                    }
                    else
                    {
                        vm[pInfo.Name.ToInitialLower()] = value.ToString();
                    }
                }
                else if (pInfo.PropertyType == typeof(string)
                    || pInfo.PropertyType.IsNumeric()
                    || pInfo.PropertyType == typeof(bool)
                    || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(bool)
                    || pInfo.PropertyType == typeof(DataItem)
                    || pInfo.PropertyType.GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                {
                    object value = pInfo.GetValue(item);
                    if (value == null)
                    {
                        vm[pInfo.Name.ToInitialLower()] = "[None]";
                    }
                    else
                    {
                        vm[pInfo.Name.ToInitialLower()] = value.ToString();
                    }
                }
                else
                {
                    object value = pInfo.GetValue(item);
                    if (value == null)
                    {
                        vm[pInfo.Name.ToInitialLower()] = "[None]";
                    }
                    else
                    {
                        // Unsupported types are not displayed with toString, to avoid
                        // cases where this only shows the type name.
                        vm[pInfo.Name.ToInitialLower()] = "[...]";
                    }
                }
            }
            return vm;
        }

        public async Task RemoveAsync(Guid id)
        {
            var item = await FindItemAsync(id);
            items.Remove(item as T);
            await _context.SaveChangesAsync();
        }

        public async Task<IDictionary<string, object>> RemoveChildrenFromCollectionAsync(DataItem parent, PropertyInfo childProp, IEnumerable<DataItem> children)
        {
            var ptInfo = childProp.PropertyType.GetTypeInfo();
            var remove = ptInfo.GetGenericTypeDefinition()
                        .MakeGenericType(ptInfo.GenericTypeArguments.FirstOrDefault())
                        .GetMethod("Remove");

            foreach (var child in children)
            {
                remove.Invoke(childProp.GetValue(parent), new object[] { child });
            }

            await _context.SaveChangesAsync();
            return GetViewModel(parent as T);
        }

        public async Task RemoveRangeAsync(IEnumerable<Guid> ids)
        {
            items.RemoveRange(ids.Select(i => items.Find(i)));
            await _context.SaveChangesAsync();
        }

        public async Task<IDictionary<string, object>> UpdateAsync(DataItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            items.Update(item as T);
            await _context.SaveChangesAsync();
            foreach (var reference in _context.Entry(item).References)
            {
                reference.Load();
            }
            foreach (var collection in _context.Entry(item).Collections)
            {
                collection.Load();
            }
            return GetViewModel(item as T);
        }
    }
}
