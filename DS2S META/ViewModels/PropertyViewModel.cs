using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.ViewModels
{
    public class PropertyViewModel
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public List<PropertyViewModel> Children { get; set; }

        private const int MaxDepth = 5;

        public PropertyViewModel(string name, object value)
            : this(name, value, new HashSet<object>(), 0)
        {
        }

        private PropertyViewModel(string name, object value, HashSet<object> visited, int depth)
        {
            Name = name;
            // Check if the value is an IEnumerable (but not a string)
            if (value is IEnumerable && !(value is string))
                Value = ""; // don't print long System.Array types that nobody cares about
            else
                Value = value;
            Children = new List<PropertyViewModel>();

            // Add extra helpful item description every time its printed
            if (value is int itemid)
            {
                Value = $"{Value} {itemid.AsMetaName()}";
            } else if (value?.GetType() == typeof(ITEMID))
            {
                int idval = (int)value;
                Value = $"{Value} {idval.AsMetaName()}";
            }

            // values are handled, no further props for these
            if (value == null || value is string || value.GetType().IsPrimitive)
                return;

            if (depth >= MaxDepth)
            {
                Children.Add(new PropertyViewModel("Max depth reached", null));
                return;
            }

            if (visited.Contains(value))
            {
                Children.Add(new PropertyViewModel("Circular reference detected", null));
                return;
            }

            visited.Add(value);

            // Check if the value is an IEnumerable (but not a string)
            if (value is IEnumerable enumerable && !(value is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    // Add each item in the collection as a child
                    Children.Add(new PropertyViewModel($"[{index++}]", item, new HashSet<object>(visited), depth + 1));
                }
                return;  // Exit early since we're handling the enumeration here
            }



            var type = value.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                try
                {
                    var childValue = prop.GetValue(value);
                    Children.Add(new PropertyViewModel(prop.Name, childValue, new HashSet<object>(visited), depth + 1));
                }
                catch
                {
                    Children.Add(new PropertyViewModel(prop.Name, "Unable to read", new HashSet<object>(visited), depth + 1));
                }
            }
        }

        public override string ToString() => $"{Name}: {Value}";

    }
}
