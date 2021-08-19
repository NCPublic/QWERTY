using QUERTY.SlidingTextControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Querty.WPF
{
    /// <summary>
    /// The viewmodel for the main window
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private string _textToType;

        /// <summary>
        /// The text the user has to type
        /// </summary>
        public string TextToType
        {
            get
            {
                return _textToType;
            }
            set
            {
                _textToType = value;
                OnPropertyChanged(nameof(TextToType));
            }
        }


        private SlidingText _slidingTextControl;

        /// <summary>
        /// The control that displays the text and slides it to the left while the user is typing
        /// </summary>
        public SlidingText SlidingTextControl
        {
            get
            {
                return _slidingTextControl;
            }
            set
            {
                _slidingTextControl = value;
                OnPropertyChanged(nameof(SlidingTextControl));
            }
        }

        public ICommand RefreshCommand { get; private set; }

        public MainViewModel()
        {
            _textToType = "Hello World";
            SlidingTextControl = new SlidingText();
            RefreshCommand = new RelayCommand(() => SlidingTextControl.Refresh() );
        }
    }
}
