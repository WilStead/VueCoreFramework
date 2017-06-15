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
    /// <summary>
    /// Handles operations with an <see cref="ApplicationDbContext"/> for a particular class.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DataItem"/> class whose operations with the <see cref="ApplicationDbContext"/>
    /// are handled by this <see cref="Repository{T}"/>.
    /// </typeparam>
    public class Repository<T> : IRepository where T : DataItem
    {
        private readonly ApplicationDbContext _context;

        private DbSet<T> items;

        /// <summary>
        /// Initializes a new instance of <see cref="Repository{T}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationDbContext"/> wrapped by this <see cref="Repository{T}"/>.</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            items = _context.Set<T>();
        }

        /// <summary>
        /// Asynchronously creates a new instance of <see cref="T"/> and adds it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/> property.
        /// </param>
        /// <returns>A ViewModel instance representing the newly added entity.</returns>
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

            items.Add(item as T);
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

        /// <summary>
        /// Asynchronously adds an assortment of child entities to a parent entity under the given
        /// navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property to which the children will be added.</param>
        /// <param name="childIds">The primary keys of the child entities which will be added.</param>
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

        /// <summary>
        /// Finds an entity with the given primary key value and returns a ViewModel for that entity.
        /// If no entity is found, an empty ViewModel is returned (not null).
        /// </summary>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <returns>A ViewModel representing the item found, or an empty ViewModel if none is found.</returns>
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

        /// <summary>
        /// Finds an entity with the given primary key value. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <returns>
        /// The item found, or null if none is found.
        /// </returns>
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

        /// <summary>
        /// Enumerates all the entities in the <see cref="ApplicationDbContext"/>'s set, returning a
        /// ViewModel representing each.
        /// </summary>
        /// <returns>ViewModels representing the items in the set.</returns>
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
                // The name is converted to initial-lower-case for use in the SPA framework.
                Model = pInfo.Name.ToInitialLower()
            };

            // Guids are always hidden in the SPA framework.
            if (pInfo.PropertyType == typeof(Guid)
                || Nullable.GetUnderlyingType(pInfo.PropertyType) == typeof(Guid))
            {
                fd.Type = "label";
                fd.HideInTable = true;
                fd.Visible = false;
                return fd;
            }

            // If the property is fully hidden, there is no need to identify its usual type or other
            // attributes, since it will never be visible.
            var hidden = pInfo.GetCustomAttribute<HiddenAttribute>();
            if (hidden?.Hidden == true)
            {
                fd.Type = "label";
                fd.HideInTable = true;
                fd.Visible = false;
                return fd;
            }

            fd.HideInTable = hidden?.HideInTable;

            var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
            var step = pInfo.GetCustomAttribute<StepAttribute>();
            var ptInfo = pInfo.PropertyType.GetTypeInfo();

            // If a property is nullable, it will be marked as not required, unless a Required
            // Attribute explicitly says otherwise.
            var nullable = Nullable.GetUnderlyingType(pInfo.PropertyType) != null;

            if (!string.IsNullOrEmpty(dataType?.CustomDataType))
            {
                if (dataType.CustomDataType == "Color")
                {
                    fd.Type = "vuetifyColor";
                }
                // Any custom data type not recognized as one of the special types handled above is
                // treated as a simple text field.
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
                            // If a step isn't specified, currency uses cents by default.
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
                            // If a step isn't specified, duration uses milliseconds by default.
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
                        // This regex is a permissive test for U.S. phone numbers, accepting letters
                        // and most forms of "ext", but not invalid numbers (e.g. too short, too
                        // long, or with invalid registers).
                        fd.Pattern = @"1?(?:[.\s-]?[2-9]\d{2}[.\s-]?|\s?\([2-9]\d{2}\)\s?)(?:[1-9]\d{2}[.\s-]?\d{4}\s?(?:\s?([xX]|[eE][xX]|[eE][xX]\.|[eE][xX][tT]|[eE][xX][tT]\.)\s?\d{3,4})?|[a-zA-Z]{7})";
                        fd.Validator = "string_regexp";
                        break;
                    case DataType.PostalCode:
                        fd.Type = "vuetifyText";
                        fd.InputType = "text";
                        // This regex accepts both short and long U.S. postal codes.
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
                        // If a data type was specified but not one of those recognized, it is
                        // treated as a simple text field.
                        fd.Type = "vuetifyText";
                        fd.InputType = "text";
                        fd.Validator = "string";
                        break;
                }
            }
            // If a data type isn't specified explicitly, the type is determined by the Type of the property.
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
                    // If a step isn't specified, duration uses milliseconds by default.
                    fd.Step = 0.001;
                }
                fd.Required = !nullable;
            }
            else if (ptInfo.IsEnum)
            {
                fd.Type = "vuetifySelect";
                if (ptInfo.GetCustomAttribute<FlagsAttribute>() == null)
                {
                    // Non-Flags enums are handled with single-selects.
                    fd.InputType = "single";
                }
                else
                {
                    // Flags enums are handled with multiselects.
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
                        // The display text for each option is set to the enum value's description
                        // (name if one isn't explicitly specified).
                        Text = EnumExtensions.GetDescription(pInfo.PropertyType, value),
                        Value = (int)value
                    });
                }
            }
            else if (pInfo.PropertyType.IsNumeric())
            {
                fd.Type = "vuetifyText";
                fd.InputType = "number";
                if (step != null)
                {
                    if (pInfo.PropertyType.IsIntegralNumeric())
                    {
                        // If a step is specified for an integer-type numeric type, ensure it is not
                        // less than 1, and is an integer.
                        fd.Step = Math.Max(1, Math.Abs(Math.Round(step.Step)));
                    }
                    else
                    {
                        // If a step is specified for a real-type numeric type, ensure it is not
                        // equal to or less than 0.
                        fd.Step = Math.Max(double.Epsilon, Math.Abs(step.Step));
                    }
                }
                else
                {
                    if (pInfo.PropertyType.IsRealNumeric())
                    {
                        fd.Step = 0.1;
                    }
                    else
                    {
                        fd.Step = 1;
                    }
                }
                fd.Validator = "number";
                fd.Required = !nullable;
            }
            else if (pInfo.PropertyType == typeof(DataItem) || ptInfo.IsSubclassOf(typeof(DataItem)))
            {
                // The input type for navigation properties is the type name (without the namespace).
                fd.InputType = pInfo.PropertyType.Name.Substring(pInfo.PropertyType.Name.LastIndexOf('.') + 1);

                var inverseAttr = pInfo.GetCustomAttribute<InversePropertyAttribute>();
                PropertyInfo inverseProp = null;
                if (inverseAttr != null)
                {
                    inverseProp = pInfo.PropertyType.GetProperty(inverseAttr.Property);
                    // The pattern for navigation properties is the name of the inverse property.
                    fd.InverseType = inverseProp?.Name;
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
                    // MenuClass item in its own right, but for this relationship the controls make
                    // no assumptions.
                    else
                    {
                        fd.Type = "object";
                    }
                }
            }
            // Children in a one-to-many relationship are manipulated in a table containing only
            // those items in the parent's collection. Adding or removing items to/from the
            // collection is accomplished by creating new items or deleting them (which only deletes
            // them fully when appropriate). This handles cases where the child objects are nested
            // child objects, child objects with multiple parent relationships, and also MenuClass
            // items in their own right.
            else if (ptInfo.IsGenericType
                && ptInfo.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>))
                && ptInfo.GenericTypeArguments.FirstOrDefault().GetTypeInfo().IsSubclassOf(typeof(DataItem)))
            {
                fd.Type = "objectCollection";

                var name = ptInfo.GenericTypeArguments.FirstOrDefault().Name;
                fd.InputType = name.Substring(name.LastIndexOf('.') + 1);

                var inverseAttr = pInfo.GetCustomAttribute<InversePropertyAttribute>();
                fd.InverseType = inverseAttr?.Property;
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
            // Unrecognized types are represented as plain labels.
            else
            {
                fd.Type = "label";
            }

            var display = pInfo.GetCustomAttribute<DisplayAttribute>();
            fd.GroupName = display?.GetGroupName();
            fd.Hint = display?.GetDescription();
            fd.Label = display?.GetName();
            fd.Placeholder = display?.GetPrompt();

            // If no label or placeholder text was set, the property name is used.
            if (fd.Label == null && fd.Placeholder == null)
            {
                if (fd.Type == "vuetifyText" || fd.Type == "vuetifyCheckbox"
                    || fd.Type == "vuetifySelect" || fd.Type == "vuetifyDateTime")
                {
                    // For most Vuetify fields, the placeholder is used.
                    fd.Placeholder = pInfo.Name;
                }
                else
                {
                    // For other field types, the label is used.
                    fd.Label = pInfo.Name;
                }
            }

            fd.Help = pInfo.GetCustomAttribute<HelpAttribute>()?.HelpText;

            // If the Required Attribute is present the field is marked as such, but it is not marked
            // as non-required if the attribute is missing, since the property may have already been
            // assigned a required/non-rquired state based on being a nullable type.
            if (pInfo.GetCustomAttribute<RequiredAttribute>() != null)
            {
                fd.Required = true;
            }

            if (pInfo.GetCustomAttribute<EditableAttribute>()?.AllowEdit == false)
            {
                if (fd.Type == "vuetifyText")
                {
                    // Non-editable text fields are marked read-only.
                    fd.Readonly = true;
                }
                else
                {
                    // Other non-editable field types are disabled.
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
                // Any field with an explicit pattern automatically gets the regex validator (unless
                // another one is explicitly set, which will override this later).
                fd.Validator = "string_regexp";
            }

            // Icons are only checked for relevant field types.
            if (fd.Type == "vuetifyText" || fd.Type == "vuetifyCheckbox" || fd.Type == "vuetifySelect")
            {
                fd.Icon = pInfo.GetCustomAttribute<IconAttribute>()?.Icon;
            }

            // Text field properties are only checked for text fields.
            if (fd.Type == "vuetifyText")
            {
                var textAttr = pInfo.GetCustomAttribute<TextAttribute>();
                fd.Prefix = textAttr?.Prefix;
                fd.Suffix = textAttr?.Suffix;
                fd.Rows = textAttr?.Rows;
                if (fd.Rows < 1)
                {
                    // Row amounts less than 1 are invalid, so the specified amount is disregarded.
                    fd.Rows = null;
                }
                if (fd.Rows > 1)
                {
                    // A row amount greater than 1 automatically indicates a textarea even if the
                    // property wasn't explicitly marked as such with a datatype attribute.
                    fd.InputType = "textArea";
                }
            }

            fd.Validator = pInfo.GetCustomAttribute<ValidatorAttribute>()?.Validator;

            return fd;
        }

        /// <summary>
        /// Generates and enumerates <see cref="FieldDefinition"/> s representing the properties of
        /// <see cref="T"/>.
        /// </summary>
        public IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var type = typeof(T);
            foreach (var pInfo in type.GetProperties())
            {
                yield return GetFieldDefinition(pInfo);
            }
        }

        /// <summary>
        /// Calculates and enumerates the set of entities with the given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="search">
        /// An optional search term which will filter the results. Any string or numeric property
        /// with matching text will be included.
        /// </param>
        /// <param name="sortBy">
        /// An optional property name which will be used to sort the items before calculating the
        /// page contents.
        /// </param>
        /// <param name="descending">
        /// Indicates whether the sort is descending; if false, the sort is ascending.
        /// </param>
        /// <param name="page">The page number requested.</param>
        /// <param name="rowsPerPage">The number of items per page.</param>
        /// <param name="except">
        /// An enumeration of primary keys of items which should be excluded from the results before
        /// caluclating the page contents.
        /// </param>
        public IEnumerable<IDictionary<string, object>> GetPage(string search, string sortBy, bool descending, int page, int rowsPerPage, IEnumerable<Guid> except)
        {
            return GetPageItems(items.Where(i => !except.Contains(i.Id)), search, sortBy, descending, page, rowsPerPage);
        }

        /// <summary>
        /// Calculates and enumerates the given items with the given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="items">The items to filter, sort, and page.</param>
        /// <param name="search">
        /// An optional search term which will filter the results. Any string or numeric property
        /// with matching text will be included.
        /// </param>
        /// <param name="sortBy">
        /// An optional property name which will be used to sort the items before calculating the
        /// page contents.
        /// </param>
        /// <param name="descending">
        /// Indicates whether the sort is descending; if false, the sort is ascending.
        /// </param>
        /// <param name="page">The page number requested.</param>
        /// <param name="rowsPerPage">The number of items per page.</param>
        /// <param name="except">
        /// An enumeration of primary keys of items which should be excluded from the results before
        /// caluclating the page contents.
        /// </param>
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

        /// <summary>
        /// Asynchronously returns a <see cref="long"/> that represents the total number of entities
        /// in the set.
        /// </summary>
        public async Task<long> GetTotalAsync() => await items.LongCountAsync();

        private static IDictionary<string, object> GetViewModel(DataItem item)
        {
            IDictionary<string, object> vm = new Dictionary<string, object>();
            var tInfo = typeof(T).GetTypeInfo();
            foreach (var pInfo in tInfo.GetProperties())
            {
                var ptInfo = pInfo.PropertyType.GetTypeInfo();
                var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();

                // Collection navigation properties are represented as placeholder text, varying
                // depending on whether the collection is empty or not.
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
                // Enum properties are given their actual (integer) value, but are also given a
                // 'Formatted' property in the ViewModel which contains either the description, or
                // placeholder text for unrecognized values (e.g. combined Flags values). This
                // formatted value is used in data tables.
                else if (ptInfo.IsEnum)
                {
                    object value = pInfo.GetValue(item);
                    var name = pInfo.Name.ToInitialLower();
                    vm[name] = (int)value;

                    var desc = EnumExtensions.GetDescription(pInfo.PropertyType, value);
                    vm[name + "Formatted"] = string.IsNullOrEmpty(desc) ? "[...]" : desc;
                }
                // Date properties are given their actual value, but are also given a 'Formatted'
                // property in the ViewModel which contains their short date formatted string. This
                // formatted value is used in data tables.
                else if (dataType?.DataType == DataType.Date)
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("d");
                }
                // Time properties are given their actual value, but are also given a 'Formatted'
                // property in the ViewModel which contains their short time formatted string. This
                // formatted value is used in data tables.
                else if (dataType?.DataType == DataType.Time)
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("t");
                }
                // DateTime properties which are not marked as either Date or Time are given their
                // actual value, but are also given a 'Formatted' property in the ViewModel which
                // contains their general formatted string. This formatted value is used in data tables.
                else if (dataType?.DataType == DataType.DateTime || pInfo.PropertyType == typeof(DateTime))
                {
                    var name = pInfo.Name.ToInitialLower();
                    var value = (DateTime)pInfo.GetValue(item);
                    vm[name] = value;
                    vm[name + "Formatted"] = value.ToString("g");
                }
                // Duration properties are given their actual value, but are also given a 'Formatted'
                // property in the ViewModel which contains their formatted string. This formatted
                // value is used in data tables.
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
                // Guid properties are always hidden in the SPA framework, but are still included in
                // the ViewModel since the framework must reference the keys in order to manage relationships.
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
                // Other recognized types are represented with their ToString equivalent, or
                // placeholder text for a null value. The SPA framework automatically omits values
                // with this placeholder text when sending data back for update, avoiding overwriting
                // previously null values with the placeholder text inappropriately.
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
                // Unsupported types are not displayed with toString, to avoid cases where this only
                // shows the type name. Instead placeholder text is used for any value, only
                // distinguishing between null and non-null values.
                else
                {
                    object value = pInfo.GetValue(item);
                    if (value == null)
                    {
                        vm[pInfo.Name.ToInitialLower()] = "[None]";
                    }
                    else
                    {
                        vm[pInfo.Name.ToInitialLower()] = "[...]";
                    }
                }
            }
            return vm;
        }

        /// <summary>
        /// Asynchronously removes an entity from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="id">The primary key of the entity to remove.</param>
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

        /// <summary>
        /// Asynchronously removes an assortment of child entities from a parent entity under the
        /// given navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property from which the children will be removed.</param>
        /// <param name="childIds">The primary keys of the child entities which will be removed.</param>
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

        /// <summary>
        /// Asynchronously terminates a relationship bewteen two entities. If the child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="id">The primary key of the child entity whose relationship is being severed.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        public async Task RemoveFromParentAsync(Guid id, PropertyInfo childProp)
        {
            var childFKProp = typeof(T).GetProperty(childProp.Name + "Id");

            // If this is a required relationship, removing from the parent is the same as deletion.
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

        /// <summary>
        /// Asynchronously removes a collection of entities from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="ids">An enumeration of the primary keys of the entities to remove.</param>
        public async Task RemoveRangeAsync(IEnumerable<Guid> ids)
        {
            items.RemoveRange(ids.Select(i => items.Find(i)));
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously terminates a relationship for multiple entities. If any child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="ids">
        /// An enumeration of primary keys of child entities whose relationships are being severed.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        public async Task RemoveRangeFromParentAsync(IEnumerable<Guid> ids, PropertyInfo childProp)
        {
            var childFKProp = typeof(T).GetProperty(childProp.Name + "Id");

            // If this is a required relationship, removing from the parent is the same as deletion.
            if (Nullable.GetUnderlyingType(childFKProp.PropertyType) != null)
            {
                await RemoveRangeAsync(ids);
                return;
            }

            // If the children are not MenuClass items, they should be removed if now orphans (have
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

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with another one. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="newChildId">
        /// The primary key of the new child entity entering into the relationship.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
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

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with a new entity. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
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

        /// <summary>
        /// Asynchronously updates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the updated item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <returns>A ViewModel representing the updated item.</returns>
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
