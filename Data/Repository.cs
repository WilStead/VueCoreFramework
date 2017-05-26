using Microsoft.EntityFrameworkCore;
using MVCCoreVue.Data.Attributes;
using MVCCoreVue.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    public class Repository<T> : IRepository<T> where T : DataItem
    {
        private readonly ApplicationDbContext _context;

        private DbSet<T> items;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            items = _context.Set<T>();
        }

        public async Task<T> AddAsync(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await items.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        private bool AnyPropMatch(T item, string search)
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

        public async Task<T> FindAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return await items.FindAsync(id);
        }

        public IEnumerable<T> GetAll()
        {
            return items.AsEnumerable();
        }

        private FieldDefinition GetFieldDefinition(PropertyInfo pInfo)
        {
            var fd = new FieldDefinition
            {
                Model = pInfo.Name
            };

            var dataType = pInfo.GetCustomAttribute<DataTypeAttribute>();
            var step = pInfo.GetCustomAttribute<StepAttribute>();
            if (dataType != null)
            {
                switch (dataType.DataType)
                {
                    case DataType.Currency:
                        fd.Type = "input";
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
                        fd.Type = "dateTime";
                        fd.DateTimePickerOptions = "M/D/YYYY";
                        fd.Validator = "date";
                        break;
                    case DataType.DateTime:
                        fd.Type = "dateTime";
                        fd.DateTimePickerOptions = "M/D/YYYY h:mm A";
                        fd.Validator = "date";
                        break;
                    case DataType.Time:
                        fd.Type = "dateTime";
                        fd.DateTimePickerOptions = "h:mm A";
                        fd.Validator = "date";
                        break;
                    case DataType.Duration:
                        fd.Type = "dateTime";
                        fd.DateTimePickerOptions = "H:mm:ss";
                        fd.Validator = "date";
                        break;
                    case DataType.EmailAddress:
                        fd.Type = "input";
                        fd.InputType = "email";
                        fd.Validator = "email";
                        break;
                    case DataType.MultilineText:
                        fd.Type = "textArea";
                        fd.Rows = pInfo.GetCustomAttribute<RowsAttribute>()?.Rows;
                        fd.Validator = "string";
                        break;
                    case DataType.Password:
                        fd.Type = "input";
                        fd.InputType = "password";
                        fd.Validator = "string";
                        break;
                    case DataType.PhoneNumber:
                        fd.Type = "input";
                        fd.InputType = "telephone";
                        fd.Validator = "string";
                        break;
                    case DataType.PostalCode:
                        fd.Type = "input";
                        fd.InputType = "text";
                        fd.Pattern = @"(^(?!0{5})(\d{5})(?!-?0{4})(|-\d{4})?$)";
                        fd.Validator = "string_regexp";
                        break;
                    case DataType.ImageUrl:
                    case DataType.Url:
                        fd.Type = "input";
                        fd.InputType = "url";
                        fd.Validator = "string";
                        break;
                    default:
                        fd.Type = "input";
                        fd.InputType = "text";
                        fd.Validator = "string";
                        break;
                }
            }
            else if (pInfo.PropertyType == typeof(bool))
            {
                fd.Type = "checkbox";
            }
            else if (pInfo.PropertyType == typeof(DateTime))
            {
                fd.Type = "dateTime";
                fd.DateTimePickerOptions = "M/D/YYYY h:mm A";
                fd.Validator = "date";
            }
            else if (pInfo.PropertyType == typeof(TimeSpan))
            {
                fd.Type = "dateTime";
                fd.DateTimePickerOptions = "H:mm:ss";
                fd.Validator = "date";
            }
            else if (pInfo.PropertyType.GetTypeInfo().IsEnum)
            {
                if (pInfo.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    fd.Type = "select";
                    foreach (var value in Enum.GetValues(pInfo.PropertyType))
                    {
                        fd.Values.Add(new SelectOption
                        {
                            Name = Enum.GetName(pInfo.PropertyType, value),
                            Id = value.ToString()
                        });
                    }
                }
                else
                {
                    fd.Type = "checklist";
                    foreach (var value in Enum.GetValues(pInfo.PropertyType))
                    {
                        fd.Values.Add(new ChecklistOption
                        {
                            Name = Enum.GetName(pInfo.PropertyType, value),
                            Value = value.ToString()
                        });
                    }
                    fd.Validator = "array";
                }
            }
            else if (pInfo.PropertyType.IsNumeric())
            {
                fd.Type = "input";
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
            else
            {
                fd.Type = "input";
                fd.InputType = "text";
                fd.Validator = "string";
            }

            fd.Default = pInfo.GetCustomAttribute<DefaultAttribute>()?.Default;

            if (pInfo.GetCustomAttribute<EditableAttribute>()?.AllowEdit == false)
            {
                if (fd.Type == "input"
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
            fd.GroupName = display?.GroupName;
            fd.Help = display?.Description;
            fd.Hint = display?.ShortName;
            fd.Label = display?.Name;
            fd.Placeholder = display?.Prompt;

            var hidden = pInfo.GetCustomAttribute<HiddenAttribute>();
            if (hidden?.Hidden == true) fd.Visible = false;
            fd.HideInTable = hidden?.HiddenInTable;

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

        public IEnumerable<T> GetPage(string search, string sortBy, bool descending, int page, int rowsPerPage)
        {
            IQueryable<T> filteredItems = items.AsQueryable();
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
                filteredItems = filteredItems.Skip((page - 1) * rowsPerPage).Take(page * rowsPerPage);
            }

            return filteredItems.AsEnumerable();
        }

        public async Task<int> GetTotalAsync()
        {
            return await items.CountAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var item = await FindAsync(id);
            items.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<Guid> ids)
        {
            items.RemoveRange(ids.Select(i => items.Find(i)));
            await _context.SaveChangesAsync();
        }

        public async Task<T> UpdateAsync(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            items.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }
}
