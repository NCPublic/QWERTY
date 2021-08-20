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
        private string _startupText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris " +
                "nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in " +
                "reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla " +
                "pariatur. Excepteur sint occaecat cupidatat non proident, sunt in " +
                "culpa qui officia deserunt mollit anim id est laborum";

        private string _templateText = "";

        /// <summary>
        /// The text typed in the seed box
        /// </summary>
        public string TemplateText
        {
            get
            {
                return _templateText;
            }
            set
            {
                _templateText = value;
                OnPropertyChanged(nameof(TemplateText));
            }
        }

        private string _textToType = "";

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

        /// <summary>
        /// The command to populate the sliding text control with the text from the seed box
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            TemplateText = _startupText;
            TextToType = TemplateText;
            SlidingTextControl = new SlidingText();
            RefreshCommand = new RelayCommand(UpdateControls);
        }

        /// <summary>
        /// Reload the control with the text in the seed box
        /// </summary>
        private void UpdateControls()
        {
            TextToType = TemplateText;
            SlidingTextControl.Refresh();
        }
    }
}
