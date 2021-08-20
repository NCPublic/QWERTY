using System.ComponentModel;

namespace Querty.WPF
{
    /// <summary>
    /// A base class for all viewmodels that implements the INotifyPropertyChanged interface
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The propertychanged delegate
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The function that is called to raise the PropertyChanged event for a property
        /// </summary>
        /// <param name="propertyName">Name of the property that is changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
