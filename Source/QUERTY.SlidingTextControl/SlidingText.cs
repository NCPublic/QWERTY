using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace QUERTY.SlidingTextControl
{
    /// <summary>
    /// Class with logic for the animated-sliding character control, inspired from www.tippenakademie.de
    /// </summary>
    [TemplatePart(Name = TEXTBOX_LISTENING, Type = typeof(TextBox))]
    [TemplatePart(Name = TEXTBLOCK_ANIMATED, Type = typeof(TextBlock))]
    public class SlidingText : Control
    {
        #region Private fields

        /// <summary>
        /// Reference for the name of the textbox that listens for user input
        /// </summary>
        private const string TEXTBOX_LISTENING = "PART_LISTENING_TEXTBOX";

        /// <summary>
        /// Reference to the name of the textblock that slides to the left
        /// </summary>
        private const string TEXTBLOCK_ANIMATED = "PART_ANIMATED_TEXTBLOCK";

        /// <summary>
        /// The actual textbox that listens for user input
        /// </summary>
        private TextBox _listeningTextBox;

        /// <summary>
        /// The actual textblock that slides
        /// </summary>
        private TextBlock _animatedTextBlock;

        /// <summary>
        /// Index to go thorugh the texts characters
        /// </summary>
        private int _index;
        
        /// <summary>
        /// The current index that needs to be typed
        /// </summary>
        public int Index
        {
            get { return _index; }
            set
            {
                if (value < Text.Length)
                    _index = value;
                else
                    TypingFinished?.Invoke();
            }
        }

        /// <summary>
        /// Public event fired when the last charcter has been typed
        /// </summary>
        public event Action TypingFinished = () => { };

        /// <summary>
        /// The number of mistakes made by the user
        /// </summary>
        private int _mistakes = 0;

        /// <summary>
        /// The constant width of a character assuming the font family is mono spaced
        /// </summary>
        private double _characterWidth = 0;

        /// <summary>
        /// The total distance to slide to
        /// </summary>
        private double _slidedDistance = 0;

        /// <summary>
        /// The last input by the user (case sensitive characters and space)
        /// </summary>
        private string _lastUserInput = string.Empty;

        /// <summary>
        /// A list of runs for each character of the text to type
        /// </summary>
        private Run[] _textAsRuns;

        /// <summary>
        /// Flag to notify wether the last input was correct or not
        /// </summary>
        private bool _lastInputCorrect = true;

        /// <summary>
        /// Style the next character to type bold
        /// </summary>
        private Style _nextPending;

        /// <summary>
        /// If the wrong character is pressed, mark it bold and with red foreground
        /// </summary>
        private Style _nextIncorrect;

        /// <summary>
        /// Submitted characters that were correct
        /// </summary>
        private Style _submittedCorrect;

        /// <summary>
        /// Submitted characters that were incorrect
        /// </summary>
        private Style _submittedIncorrect;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// The next that needs to be typed
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Dependency property for the text that needs to be typed, provided by the consumer of this class
        /// Default value is<see cref="string.Empty"/>
        /// </summary>
        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(typeof(SlidingText), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The color of the mark under the next character that needs to be typed
        /// </summary>
        public SolidColorBrush MarkFill
        {
            get { return (SolidColorBrush)GetValue(MarkFillProperty); }
            set { SetValue(MarkFillProperty, value); }
        }

        /// <summary>
        /// Dependency property for color of the mark under the next character that needs to be typed, provided by the consumer of this class
        /// Default value is <see cref="Brushes.CornflowerBlue"/>
        /// </summary>
        public static readonly DependencyProperty MarkFillProperty =
            DependencyProperty.Register(nameof(MarkFill), typeof(SolidColorBrush), typeof(SlidingText), new PropertyMetadata(Brushes.CornflowerBlue));


        /// <summary>
        /// Time the slide animation takes to finish in miliseconds
        /// </summary>
        public int SlideTimeMS
        {
            get { return (int)GetValue(SlideTimeMSProperty); }
            set { SetValue(SlideTimeMSProperty, value); }
        }

        /// <summary>
        /// Dependency property for the time the slide animation takes to finish in miliseconds, provided by the consumer of this class
        /// Default value is <see cref="150"/> miliseconds
        /// </summary>
        public static readonly DependencyProperty SlideTimeMSProperty =
            DependencyProperty.Register(nameof(SlideTimeMS), typeof(int), typeof(SlidingText), new PropertyMetadata(150));


        /// <summary>
        /// The font family for bold characters
        /// </summary>
        public FontFamily BoldFontFamily
        {
            get { return (FontFamily)GetValue(BoldFontFamilyProperty); }
            set { SetValue(BoldFontFamilyProperty, value); }
        }

        /// <summary>
        /// Dependency property for bold letters as custom font family
        /// </summary>
        public static readonly DependencyProperty BoldFontFamilyProperty =
            DependencyProperty.Register(nameof(BoldFontFamily), typeof(FontFamily), typeof(SlidingText), new PropertyMetadata(null));


        /// <summary>
        /// The current index the user is on
        /// </summary>
        public int TypedCharacters
        {
            get { return (int)GetValue(TypedCharactersProperty); }
            set { SetValue(TypedCharactersProperty, value); }
        }

        /// <summary>
        /// Dependency property for the index of the current character that the user is typing
        /// </summary>
        public static readonly DependencyProperty TypedCharactersProperty =
            DependencyProperty.Register(nameof(TypedCharacters), typeof(int), typeof(SlidingText), new PropertyMetadata(0));



        /// <summary>
        /// The number of mistakes that were made by the user
        /// </summary>
        public int TypingMistakes
        {
            get { return (int)GetValue(TypingMistakesProperty); }
            set { SetValue(TypingMistakesProperty, value); }
        }

        /// <summary>
        /// Dependency property for the number of mistakes that were made by the user
        /// </summary>
        public static readonly DependencyProperty TypingMistakesProperty =
            DependencyProperty.Register(nameof(TypingMistakes), typeof(int), typeof(SlidingText), new PropertyMetadata(0));




        #endregion

        #region Initialization

        /// <summary>
        /// Whenever the template is applied, assign all fields and hook into events
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Assign the actual Textbox from the style
            _listeningTextBox = Template.FindName(TEXTBOX_LISTENING, this) as TextBox;
            _animatedTextBlock = Template.FindName(TEXTBLOCK_ANIMATED, this) as TextBlock;

            // Measure the constant character length of a mono spaced font family
            _characterWidth = MeasureString("a").Width;

            InitializeRuns();

            #region Initialize all styles

            _nextPending = new Style(typeof(Run));
            _nextPending.Setters.Add(new Setter(TextBlock.FontFamilyProperty, BoldFontFamily));
            _nextPending.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Black));

            _nextIncorrect = new Style(typeof(Run));
            _nextIncorrect.Setters.Add(new Setter(TextBlock.FontFamilyProperty, BoldFontFamily));
            _nextIncorrect.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Red));

            _submittedCorrect = new Style(typeof(Run));
            _submittedCorrect.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Normal));

            _submittedIncorrect = new Style(typeof(Run));
            _submittedIncorrect.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Normal));
            _submittedIncorrect.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White));
            _submittedIncorrect.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Red));

            #endregion

            #region Hook into all events

            _listeningTextBox.TextChanged += ListeningTextBox_TextChanged;
            _listeningTextBox.PreviewKeyDown += ListeningTextBox_PreviewKeyDown;
            _listeningTextBox.PreviewTextInput += ListeningTextBox_PreviewTextInput;
            this.Loaded += AnimatedSliding_Loaded;

            #endregion

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Initalize a list of Runs for the given text to ease editing
        /// </summary>
        private void InitializeRuns()
        {
            // Make an array of runs for the text to type           
            _textAsRuns = new Run[Text.Length];

            // Populate the array and the textbox, manipulate only the array items
            for (int i = 0; i < Text.Length; i++)
            {
                var nextRun = new Run(Text[i].ToString());
                _textAsRuns[i] = nextRun;
                _animatedTextBlock.Inlines.Add(nextRun);
            }
        }

        /// <summary>
        /// Registers the generic style from the themes folder
        /// </summary>
        static SlidingText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SlidingText), new FrameworkPropertyMetadata(typeof(SlidingText)));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Whenever the text changed, call this method to reset everything
        /// </summary>
        public void Refresh()
        {
            Index = 0;
            TypedCharacters = 0;

            _mistakes = 0;
            TypingMistakes = 0;

            _slidedDistance = 0;

            _animatedTextBlock.Inlines.Clear();

            _lastUserInput = string.Empty;
            _lastInputCorrect = true;
            _textAsRuns = null;

            InitializeRuns();
            AnimateToStart();
            EvaulateInput();
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Make the first character bold when the template is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimatedSliding_Loaded(object sender, RoutedEventArgs e)
        {
            _textAsRuns[Index].Style = _nextPending;
        }

        /// <summary>
        /// Get the actual character typed by the user
        /// </summary>
        /// <param name="sender">The Textbox that fired the <see cref="PreviewTextInput"/> event </param>
        /// <param name="e">Event args that hold the actual character (case sensitive)</param>
        private void ListeningTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Read everything, ignore return
            if (e.Text != "\r")
            {
                _lastUserInput = e.Text;
                EvaulateInput();
            }
        }

        /// <summary>
        /// Get the typed key as system key to register the spacebar.
        /// Spacebar is not recognized by the <see cref="PreviewTextInput"/> event
        /// so we have to use the <see cref="PreviewKeyDown"/> event
        /// </summary>
        /// <param name="sender">The Textbox that fired the <see cref="PreviewKeyDown"/> event </param>
        /// <param name="e">Event args that hold the actual system key</param>
        private void ListeningTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Only act when the enter key was pressed
            if (e.Key == Key.Space)
            {
                _lastUserInput = " ";
                EvaulateInput();
            }
        }

        /// <summary>
        /// React on text changes to start the animation. We use the <see cref="TextChanged"/> event 
        /// because it is fired everytime anything changes
        /// </summary>
        /// <param name="sender">The Textbox that fired the <see cref="TextChanged"/> event</param>
        /// <param name="e">Event args for TextChanged</param>
        private void ListeningTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _listeningTextBox.Clear();
        }

        #endregion

        #region Animations    

        /// <summary>
        /// Called whenever the user input changes, apply text styles and animate the text
        /// </summary>
        private void EvaulateInput()
        {
            // Check wether the next letter was correct
            if (_lastUserInput == Text[Index].ToString())
            {
                // If it wasn't wrong before
                if (_lastInputCorrect)
                {
                    // just submit it
                    _textAsRuns[Index].Style = _submittedCorrect;
                }
                // Otherwise
                else
                {
                    // Make the background red
                    _textAsRuns[Index].Style = _submittedIncorrect;
                    // Reset the flag
                    _lastInputCorrect = true;
                }

                if(Index < Text.Length - 1)
                {
                    _textAsRuns[Index + 1].Style = _nextPending;                    
                }
                AnimateTextBlockSlide();
                IncreaseIndex();
            }
            else
            {
                // Only After refreshing the entire Control
                if (_lastUserInput == string.Empty)
                {
                    _textAsRuns[Index].Style = _nextPending;
                }
                else
                {
                    IncreaseMistakes();
                    _lastInputCorrect = false;
                    if (Text[Index].ToString() == " ")
                    {
                        _textAsRuns[Index].Text = "_";
                    }
                    _textAsRuns[Index].Style = _nextIncorrect;
                }
            }
        }

        /// <summary>
        /// Slides the text to the left by adding a negative value to its margin 
        /// The slide time depends on whether the current margin animation is already finished or not
        /// </summary>
        private void AnimateTextBlockSlide()
        {
            _slidedDistance += _characterWidth;
            var newThickness = new Thickness(-_slidedDistance, 0, 0, 0);

            var timeFactor = _characterWidth / (_slidedDistance - Math.Abs(_animatedTextBlock.Margin.Left));

            var storyboard = new Storyboard();
            var slidingAnimation = new ThicknessAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(SlideTimeMS * timeFactor)),
                To = newThickness,
            };

            Storyboard.SetTargetProperty(slidingAnimation, new PropertyPath("Margin"));
            storyboard.Children.Add(slidingAnimation);

            storyboard.Begin(_animatedTextBlock);
        }

        /// <summary>
        /// Slides the textblock back to the starting position
        /// </summary>
        private void AnimateToStart()
        {
            var newThickness = new Thickness(0);

            var storyboard = new Storyboard();
            var slidingAnimation = new ThicknessAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(SlideTimeMS)),
                To = newThickness,
                DecelerationRatio = 0.9f,
            };

            Storyboard.SetTargetProperty(slidingAnimation, new PropertyPath("Margin"));
            storyboard.Children.Add(slidingAnimation);

            storyboard.Begin(_animatedTextBlock);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Add 1 to the number of mistakes
        /// </summary>
        private void IncreaseMistakes()
        {
            if (Index < Text.Length && _lastInputCorrect)
            {
                _mistakes++;
                TypingMistakes = _mistakes;
            }
        }

        /// <summary>
        /// Add one to the current index
        /// </summary>
        private void IncreaseIndex()
        {
            if (Index < Text.Length)
            {
                Index++;
                TypedCharacters = Index;
            }
        }

        /// <summary>
        /// Determines the actual size of a character with its current properties for fontsize, fontfamily, etc...
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(_listeningTextBox.FontFamily,
                             _listeningTextBox.FontStyle,
                             _listeningTextBox.FontWeight,
                             _listeningTextBox.FontStretch),
                _listeningTextBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            return new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
        }


        #endregion
    }
}
