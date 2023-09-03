using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using DS2S_META.Properties;
using PropertyHook;
using System.Windows.Media;

namespace DS2S_META.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private ObservableCollection<ICommand> _commands;
        public ObservableCollection<ICommand> Commands
        {
            get => _commands;
            set => SetField(ref _commands, value);
        }

        public ViewModelBase()
        {
            Commands = new ObservableCollection<ICommand>();
        }

        public abstract void UpdateViewModel();
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName ?? "");
            return true;
        }

        public DS2SHook? Hook { get; set; }
        public void InitViewModel(DS2SHook hook) { Hook = hook; }
    }
}