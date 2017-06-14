using Microsoft.EntityFrameworkCore;
using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using MVCCoreVue.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public async Task<IDictionary<string, object>> AddAsync(PropertyInfo childProp, Guid? parentId)
        {
            var item = typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });

            if (childProp != null && parentId.HasValue)
            {
                typeof(T).GetProperty(childProp.Name + "Id").SetValue(item, parentId.Value);
            }

            // Add any required child objects.
            foreach (var pInfo in typeof(T).GetProperties()
                .Where(p =>
                (p.PropertyType == typeof(DataItem) || p.PropertyType.GetTypeInfo().IsSubclassOf(typeof(DataItem)))
                && p.GetCustomAttribute<RequiredAttribute>() != null))
            {
                var newChild = pInfo.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                pInfo.SetValue(item, newChild);
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

        public async Task AddChildrenToCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> childIds)
        {
            var ptInfo = childProp.PropertyType.GetTypeInfo();
            var mtmType = ptInfo.GenericTypeArguments.FirstOrDefault();
            var add = ptInfo.GetGenericTypeDefinition()
                        .MakeGenericType(mtmType)
                        .GetMethod("Add");
            var mtmCon = mtmType.GetConstructor(Type.EmptyTypes);
            var singulars = childProp.Name.GetSingularForms();
            var mtmChildProp = mtmType.GetProperties().Where(p => singulars.Contains(p.Name)).FirstOrDefault();
            var mtmChildIdProp = mtmType.GetProperty(mtmChildProp.Name + "Id");
            var mtmParentIdProp = mtmType.GetProperties().FirstOrDefault(t => t.PropertyType == typeof(Guid) && t != mtmChildIdProp);

            var parent = await FindItemAsync(id);
            foreach (var childId in childIds)
            {
                var mtm = mtmCon.Invoke(new object[] { });
                mtmChildIdProp.SetValue(mtm, childId);
                mtmParentIdProp.SetValue(mtm, id);
                add.Invoke(childProp.GetValue(parent), new object[] { mtm });
            }

            await _context.SaveChangesAsync();
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

            if (pInfo.PropertyType == typeof(Guid)
                    || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
            {
                fd.Type = "label";
                fd.HideInTable = true;
                fd.Visible = false;
                return fd;
            }

            var hidden = pInfo.GetCustomAttribute<HiddenAttribute>();
            if (hidden?.Hidden == true)
            {
                fd.Visible = false;
                fd.HideInTable = true;
                fd.Type = "label";
                return fd;
            }

            if (hidden?.HideInTable == true)
            {
                fd.HideInTable = true;
            }

            var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
            var step = pInfo.GetCustomAttribute<StepAttribute>();
            var ptInfo = pInfo.PropertyType.GetTypeInfo();
            var nullable = Nullable.GetUnderlyingType(pInfo.PropertyType) != null;

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
                            fd.Step = Math.Abs(step.Step);
                        }
                        else
                        {
                            fd.Step = 0.01;
                        }
                        fd.Validator = "number";
                        fd.Required = !nullable;
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
                            fd.Step = Math.Abs(step.Step);
                        }
                        else
                        {
                            fd.Step = 0.001;
                        }
                        fd.Required = !nullable;
                        break;
                    case DataType.EmailAddress:
                        fd.Type = "vuetifyText";
                        fd.InputType = "email";
                        fd.Validator = "email";
                        break;
                    case DataType.MultilineText:
                        fd.Type = "vuetifyText";
                        fd.InputType = "textArea";
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
                        fd.Pattern = @"1?(?:[.\s-]?[2-9]\d{2}[.\s-]?|\s?\([2-9]\d{2}\)\s?)(?:[1-9]\d{2}[.\s-]?\d{4}\s?(?:\s?([xX]|[eE][xX]|[eE][xX]\.|[eE][xX][tT]|[eE][xX][tT]\.)\s?\d{3,4})?|[a-zA-Z]{7})";
                        fd.Validator = "string_regexp";
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
            else if (pInfo.PropertyType == typeof(bool)
                || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(bool))
            {
                fd.Type = "vuetifyCheckbox";
                fd.Required = !nullable;
            }
            else if (pInfo.PropertyType == typeof(DateTime))
            {
                fd.Type = "vuetifyDateTime";
                fd.InputType = "dateTime";
            }
            else if (pInfo.PropertyType == typeof(TimeSpan)
                || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(TimeSpan))
            {
                fd.Type = "vuetifyTimespan";
                var formatAttr = pInfo.GetCustomAttribute<DisplayFormatAttribute>();
                fd.InputType = formatAttr?.DataFormatString;
                fd.Validator = "timespan";
                if (step != null)
                {
                    fd.Step = Math.Abs(step.Step);
                }
                else
                {
                    fd.Step = 0.001;
                }
                fd.Required = !nullable;
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
                        fd.Step = Math.Abs(step.Step);
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
                fd.Required = !nullable;
            }
            else if (pInfo.PropertyType == typeof(DataItem) || ptInfo.IsSubclassOf(typeof(DataItem)))
            {
                fd.InputType = pInfo.PropertyType.Name.Substring(pInfo.PropertyType.Name.LastIndexOf('.') + 1);

                var inverseAttr = pInfo.GetCustomAttribute<InversePropertyAttribute>();
                PropertyInfo inverseProp = null;
                if (inverseAttr != null)
                {
                    inverseProp = pInfo.PropertyType.GetProperty(inverseAttr.Property);
                    fd.Pattern = inverseProp?.Name;
                }

                // Reverse-navigation properties only allow view/edit. No adding/deleting, since the
                // child object shouldn't add/delete a parent.
                if (pInfo.GetGetMethod().IsVirtual)
                {
                    fd.Type = "objectReference";
                }
                else
                {
                    // Children in a many-to-one relationship (i.e. which can have more than one
                    // parent) can be selected from a list, as well as added/edited/deleted.
                    var inversePTInfo = inverseProp?.PropertyType.GetTypeInfo();
                    if (inversePTInfo?.IsGenericType == true
                        && inversePTInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>)))
                    {
                        fd.Type = "objectSelect";
                    }
                    // Children in a one-to-one relationship are treated as purely nested objects,
                    // and can only be added, edited, and deleted, to prevent any child from being
                    // referenced by more than one parent inappropriately. In fact the child may have
                    // other relationships which result in it not being purely nested, or even be a
                    // MenuClass item, but for this relationship the controls make no assumptions.
                    else
                    {
                        fd.Type = "object";
                    }
                }
            }
            // Children in a one-to-many relationship are manipulated in a table containing only
            // those items in the parent collection. Adding or removing items to/from the collection
            // is accomplished by creating new items or deleting them (which only deletes them fully
            // when appropriate). This handles cases where the child objects are nested child
            // objects, child objects with multiple parent relationships, and also MenuClass items in
            // their own right.
            else if (ptInfo.IsGenericType
                && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
            {
                fd.Type = "objectCollection";

                var name = ptInfo.GenericTypeArguments.FirstOrDefault().Name;
                fd.InputType = name.Substring(name.LastIndexOf('.') + 1);

                var inverseAttr = pInfo.GetCustomAttribute<InversePropertyAttribute>();
                fd.Pattern = inverseAttr?.Property;
            }
            // Children in a many-to-many relationship are manipulated in a table containing all the
            // items of the child type, where items can be added to or removed from the parent's collection.
            else if (ptInfo.IsGenericType
                && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IDataItemMtM)))
            {
                fd.Type = "objectMultiSelect";

                var name = ptInfo.GenericTypeArguments.FirstOrDefault(t => t != typeof(Guid) && !t.Name.EndsWith(pInfo.Name)).Name;
                fd.InputType = name.Substring(name.LastIndexOf('.') + 1);
            }
            else
            {
                fd.Type = "label";
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

            var help = pInfo.GetCustomAttribute<HelpAttribute>();
            if (!string.IsNullOrWhiteSpace(help?.HelpText))
                fd.Help = help?.HelpText;

            if (pInfo.GetCustomAttribute<RequiredAttribute>() != null)
            {
                fd.Required = true;
            }

            if (pInfo.GetCustomAttribute<EditableAttribute>()?.AllowEdit == false)
            {
                if (fd.Type == "vuetifyText")
                {
                    fd.Readonly = true;
                }
                else
                {
                    fd.Disabled = true;
                }
            }

            fd.Default = pInfo.GetCustomAttribute<DefaultAttribute>()?.Default;

            var range = pInfo.GetCustomAttribute<RangeAttribute>();
            fd.Min = range?.Minimum;
            fd.Max = range?.Maximum;

            var pattern = pInfo.GetCustomAttribute<RegularExpressionAttribute>();
            if (!string.IsNullOrEmpty(pattern?.Pattern))
            {
                fd.Pattern = pattern.Pattern;
                fd.Validator = "string_regexp";
            }

            if (fd.Type == "vuetifyText" || fd.Type == "vuetifyCheckbox" || fd.Type == "vuetifySelect")
            {
                fd.Icon = pInfo.GetCustomAttribute<IconAttribute>()?.Icon;
            }

            if (fd.Type == "vuetifyText")
            {
                var textAttr = pInfo.GetCustomAttribute<TextAttribute>();
                fd.Prefix = textAttr?.Prefix;
                fd.Suffix = textAttr?.Suffix;
                fd.Rows = textAttr?.Rows;
                if (fd.Rows < 1)
                {
                    fd.Rows = null;
                }
                if (fd.Rows > 1)
                {
                    fd.InputType = "textArea";
                }
            }

            fd.Validator = pInfo.GetCustomAttribute<ValidatorAttribute>()?.Validator;

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
                    && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                    && (ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem))
                    || ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IDataItemMtM))))
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
                else if (dataType?.DataType == DataType.Duration
                    || pInfo.PropertyType == typeof(TimeSpan)
                    || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(TimeSpan))
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = pInfo.GetValue(item);
                    if (value == null)
                    {
                        vm[name] = value;
                        vm[name + "Formatted"] = "[None]";
                    }
                    else
                    {
                        var ts = (TimeSpan)value;
                        vm[name] = value;
                        vm[name + "Formatted"] = ts.ToString("c");
                    }
                }
                else if (pInfo.PropertyType == typeof(Guid)
                    || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
                {
                    object value = pInfo.GetValue(item);
                    if (value != null && Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
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
            await RemoveItemAsync(item);
        }

        private async Task RemoveItemAsync(DataItem item)
        {
            items.Remove(item as T);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveChildrenFromCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> childIds)
        {
            var mtmType = childProp.PropertyType.GetTypeInfo().GenericTypeArguments.FirstOrDefault();

            foreach (var childId in childIds)
            {
                var mtm = _context.Find(mtmType, id, childId);
                _context.Remove(mtm);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromParentAsync(Guid id, PropertyInfo childProp)
        {
            // If this is a required relationship, removing from the parent is the same as deletion.
            var childFKProp = typeof(T).GetProperty(childProp.Name + "Id");

            if (Nullable.GetUnderlyingType(childFKProp.PropertyType) != null)
            {
                await RemoveAsync(id);
                return;
            }

            // For non-required relationships, null the FK.
            var item = await FindItemAsync(id);
            childProp.SetValue(item, null);
            await _context.SaveChangesAsync();

            // If the child is not a MenuClass item, it should be removed if it's now an orphan (has
            // no remaining relationships).
            if (childProp.PropertyType.GetTypeInfo().GetCustomAttribute<MenuClassAttribute>() == null)
            {
                // Check all navigation properties in the child item to see if it's an orphan.
                var orphan = true;
                foreach (var nav in _context.Entry(item).Navigations)
                {
                    await nav.LoadAsync();
                    if (nav.CurrentValue != null)
                    {
                        orphan = false;
                        break;
                    }
                }
                // If the item is now an orphan, delete it.
                if (orphan)
                {
                    await RemoveItemAsync(item);
                }
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<Guid> ids)
        {
            items.RemoveRange(ids.Select(i => items.Find(i)));
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeFromParentAsync(IEnumerable<Guid> ids, PropertyInfo childProp)
        {
            var childFKProp = typeof(T).GetProperty(childProp.Name + "Id");

            // If this is a required relationship, removing from the parent is the same as deletion.
            if (Nullable.GetUnderlyingType(childFKProp.PropertyType) != null)
            {
                await RemoveRangeAsync(ids);
                return;
            }

            // If the child is not a MenuClass item, it should be removed if it's now an orphan (has
            // no remaining relationships).
            bool removeOrphans = false;
            if (childProp.PropertyType.GetTypeInfo().GetCustomAttribute<MenuClassAttribute>() == null)
            {
                removeOrphans = true;
            }

            // For non-required relationships, null the prop.
            foreach (var id in ids)
            {
                var item = await FindItemAsync(id);
                childProp.SetValue(item, null);
                await _context.SaveChangesAsync();

                if (removeOrphans)
                {
                    // Check all navigation properties in the child item to see if it's now an orphan.
                    var orphan = true;
                    foreach (var nav in _context.Entry(item).Navigations)
                    {
                        await nav.LoadAsync();
                        if (nav.CurrentValue != null)
                        {
                            orphan = false;
                            break;
                        }
                    }
                    // If the item is now an orphan, delete it.
                    if (orphan)
                    {
                        await RemoveItemAsync(item);
                    }
                }
            }
        }

        public async Task ReplaceChildAsync(Guid parentId, Guid newChildId, PropertyInfo childProp)
        {
            var parentRepo = (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(childProp.PropertyType), _context);
            var parent = await parentRepo.FindItemAsync(parentId);
            var oldChildId = (childProp.PropertyType
                .GetProperty(childProp.GetCustomAttribute<InversePropertyAttribute>()?.Property)
                .GetValue(parent) as DataItem).Id;

            var newChild = await FindItemAsync(newChildId);
            typeof(T).GetProperty(childProp.Name + "Id").SetValue(newChild, parentId);

            await RemoveFromParentAsync(oldChildId, childProp);
        }

        public async Task<IDictionary<string, object>> ReplaceChildWithNewAsync(Guid parentId, PropertyInfo childProp)
        {
            var parentRepo = (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(childProp.PropertyType), _context);
            var parent = await parentRepo.FindItemAsync(parentId);
            var oldChildId = (childProp.PropertyType
                .GetProperty(childProp.GetCustomAttribute<InversePropertyAttribute>()?.Property)
                .GetValue(parent) as DataItem).Id;

            var newItem = await AddAsync(childProp, parentId);

            await RemoveFromParentAsync(oldChildId, childProp);

            return newItem;
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
