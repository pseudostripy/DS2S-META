using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS2S_META.ViewModels
{
    public class PropertyViewModel : INotifyPropertyChanged
    {
        private bool _areSubPropertiesLoaded = false;
        public bool AreSubPropertiesLoaded
        {
            get => _areSubPropertiesLoaded;
            set
            {
                if (_areSubPropertiesLoaded != value)
                {
                    _areSubPropertiesLoaded = value;
                    OnPropertyChanged(nameof(AreSubPropertiesLoaded));
                    OnPropertyChanged(nameof(SubProperties));
                }
            }
        }
        
        public override string ToString()
        {
            if (Value is IEnumerable enumerable && !(Value is string))
                return $"{Name}"; // remove annoying type info
            return $"{Name}: {Value}";
        }

        private ObservableCollection<PropertyViewModel> _subProperties;
        public ObservableCollection<PropertyViewModel> SubProperties
        {
            get
            {
                return _subProperties;
            }
        }

        public string Name { get; set; }
        public object? Value { get; set; }

        // Constructor (initial loading of properties)
        public PropertyViewModel(string name, object? value)
        {
            Name = name;
            Value = value;
            
            // values are handled, no further props for these
            if (value == null || value is string || value.GetType().IsPrimitive)
            {
                FixItemInts();
                _subProperties = new ObservableCollection<PropertyViewModel>();
                AreSubPropertiesLoaded = true;
                return;
            }
            else
            {
                // setup lazy load via expansion
                _subProperties = new() { new PropertyViewModel("", "") };
                AreSubPropertiesLoaded = false;
            }   
        }

        private void FixItemInts()
        {
            if (Value is int itemid)
            {
                Value = $"{Value} {itemid.AsMetaName()}";
            }
            else if (Value?.GetType() == typeof(ITEMID))
            {
                int idval = (int)Value;
                Value = $"{Value} {idval.AsMetaName()}";
            }
        }


        // Loads sub-properties (this could be a more expensive operation)
        public void LoadSubProperties()
        {
            _subProperties.Clear();
            var subPropertiesToAdd = new List<PropertyViewModel>();


            // handle arrays directly:
            // Check if the value is an IEnumerable [Note string can't get here already]
            if (Value is IEnumerable enumerable)
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    // Add each item in the collection as a "subproperty"
                    subPropertiesToAdd.Add(new PropertyViewModel($"[{index++}]", item));
                }
            }
            else
            {
                // not an array (probably an ordinary object since it's not a primitive either)
                var type = Value?.GetType();
                var props = type?.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                int i = 0;
                if (props != null)
                {
                    foreach (var prop in props)
                    {
                        var subPropValue = prop.GetValue(Value);
                        subPropertiesToAdd.Add(new PropertyViewModel($"{prop.Name}", subPropValue));
                        i++;
                    }
                }
            }

            // Make the new things
            foreach (var subProperty in subPropertiesToAdd)
            {
                _subProperties.Add(subProperty);  // Add the sub-property to the list
            }

            // After loading all sub-properties, set the flag to loaded
            //AreSubPropertiesLoaded = true;

            //// Update the collection and mark the properties as loaded
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    foreach (var subProperty in subPropertiesToAdd)
            //    {
            //        _subProperties.Add(subProperty);  // Add the sub-property to the list
            //    }

            //    // After loading all sub-properties, set the flag to loaded
            //    AreSubPropertiesLoaded = true;
            //});   
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    //public class PropertyViewModel
    //{
    //    public string Name { get; set; }
    //    public object Value { get; set; }
    //    public List<PropertyViewModel> Children { get; set; }

    //    private const int MaxDepth = 5;

    //    public PropertyViewModel(string name, object value)
    //        : this(name, value, new HashSet<object>(), 0)
    //    {
    //    }

    //    private PropertyViewModel(string name, object value, HashSet<object> visited, int depth)
    //    {
    //        Name = name;
    //        // Check if the value is an IEnumerable (but not a string)
    //        if (value is IEnumerable && !(value is string))
    //            Value = ""; // don't print long System.Array types that nobody cares about
    //        else
    //            Value = value;
    //        Children = new List<PropertyViewModel>();

    //        // Add extra helpful item description every time its printed
    //        if (value is int itemid)
    //        {
    //            Value = $"{Value} {itemid.AsMetaName()}";
    //        } else if (value?.GetType() == typeof(ITEMID))
    //        {
    //            int idval = (int)value;
    //            Value = $"{Value} {idval.AsMetaName()}";
    //        }

    //        // values are handled, no further props for these
    //        if (value == null || value is string || value.GetType().IsPrimitive)
    //            return;

    //        if (depth >= MaxDepth)
    //        {
    //            Children.Add(new PropertyViewModel("Max depth reached", null));
    //            return;
    //        }

    //        if (visited.Contains(value))
    //        {
    //            Children.Add(new PropertyViewModel("Circular reference detected", null));
    //            return;
    //        }

    //        visited.Add(value);

    //        // Check if the value is an IEnumerable (but not a string)
    //        if (value is IEnumerable enumerable && !(value is string))
    //        {
    //            int index = 0;
    //            foreach (var item in enumerable)
    //            {
    //                // Add each item in the collection as a child
    //                Children.Add(new PropertyViewModel($"[{index++}]", item, new HashSet<object>(visited), depth + 1));
    //            }
    //            return;  // Exit early since we're handling the enumeration here
    //        }



    //        var type = value.GetType();
    //        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    //        foreach (var prop in props)
    //        {
    //            try
    //            {
    //                var childValue = prop.GetValue(value);
    //                Children.Add(new PropertyViewModel(prop.Name, childValue, new HashSet<object>(visited), depth + 1));
    //            }
    //            catch
    //            {
    //                Children.Add(new PropertyViewModel(prop.Name, "Unable to read", new HashSet<object>(visited), depth + 1));
    //            }
    //        }
    //    }

    //    public override string ToString() => $"{Name}: {Value}";

    //}
}
