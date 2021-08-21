using QUERTY.SlidingTextControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Querty.WPF
{
    /// <summary>
    /// The viewmodel for the main window
    /// </summary>
    public class MainViewModel : BaseViewModel
    {        
        #region Private fields

        private string _startupText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";


        private string _templateText = "";
        private string _textToType = "";
        private SlidingText _slidingTextControl;
        private int _currentIndex;
        private int _mistakes;
        private DispatcherTimer _dispatcherTimer;
        private DateTime _startTime;

        #endregion

        #region Public properties

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

        ///// <summary>
        ///// The text the user has to type
        ///// </summary>
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
        /// The index of the character the user is currently on
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                if (value == 1)
                {
                    _startTime = DateTime.Now;
                    StartTimer();
                }
                _currentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(Accuarcy));
            }
        }

        /// <summary>
        /// The number mistakes the user made while typing this excercise
        /// </summary>
        public int Mistakes
        {
            get
            {
                return _mistakes;
            }
            set
            {
                _mistakes = value;
                OnPropertyChanged(nameof(Mistakes));
                OnPropertyChanged(nameof(Accuarcy));
            }
        }

        /// <summary>
        /// The ratio of correct to incorrect characters
        /// </summary>
        public double Accuarcy
        {
            get
            {
                if (CurrentIndex == 0)
                    return 100.00;
                else
                    return Math.Round(100 * (1 - (double)Mistakes / CurrentIndex), 2);
            }

        }

        /// <summary>
        /// The time the user has been typing 
        /// </summary>
        public TimeSpan EllapsedTime
        {
            get { return DateTime.Now - _startTime; }
        }

        /// <summary>
        /// The words per minute for the current typing session
        /// </summary>
        public double TypingSpeed { get { return CurrentIndex / EllapsedTime.TotalSeconds; } }

        #endregion

        #region Commands

        /// <summary>
        /// The command to populate the sliding text control with the text from the seed box
        /// </summary>
        public ICommand RefreshCommand { get; private set; }


        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            TemplateText = _startupText;
            TextToType = TemplateText;

            SlidingTextControl = new SlidingText();
            SlidingTextControl.TypingFinished += OnTypingFinished;
         

            RefreshCommand = new UpdateCommand(this);
            InitializeTimer();
        }

        private void OnTypingFinished()
        {
            StopTimer();
            MessageBox.Show("Click ok to start again");
            UpdateControls();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Reload the control with the text in the seed box
        /// </summary>
        public void UpdateControls()
        {
            TextToType = TemplateText;
            SlidingTextControl.Refresh();
            StopTimer();
            InitializeTimer();
        }

        /// <summary>
        /// Initialize a new dispatcher timer
        /// </summary>
        private void InitializeTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            _startTime = DateTime.Now;
            OnPropertyChanged(nameof(EllapsedTime));
            OnPropertyChanged(nameof(TypingSpeed));
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        private void StartTimer()
        {
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        private void StopTimer()
        {
            _dispatcherTimer.Stop();
        }

        /// <summary>
        /// With every tick notify the UI that the ellapsed time has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(EllapsedTime));
            OnPropertyChanged(nameof(TypingSpeed));
        }
      
    }
    #endregion
}
