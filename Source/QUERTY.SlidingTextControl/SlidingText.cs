using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private static int _index = 0;

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

        /// The next character that needs to be typed
        /// </summary>
        private string _nextChar { get { return Text[_index].ToString(); } }

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
            DependencyProperty.Register("MarkFill", typeof(SolidColorBrush), typeof(SlidingText), new PropertyMetadata(Brushes.CornflowerBlue));


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
            DependencyProperty.Register("SlideTimeMS", typeof(int), typeof(SlidingText), new PropertyMetadata(150));


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
            DependencyProperty.Register("BoldFontFamily", typeof(FontFamily), typeof(SlidingText), new PropertyMetadata(null));


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

        static SlidingText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SlidingText), new FrameworkPropertyMetadata(typeof(SlidingText)));
        }

        #endregion

        /// <summary>
        /// Whenever the text changed, call this method to reset everything
        /// </summary>
        public void Refresh()
        {
            _index = 0;
            _slidedDistance = 0;

            _animatedTextBlock.Inlines.Clear();

            _lastUserInput = string.Empty;
            _lastInputCorrect = true;
            _textAsRuns = null;

            InitializeRuns();
            AnimateToStart();
            EvaulateInput();
    }

        #region Event handlers

        /// <summary>
        /// Make the first character bold when the template is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimatedSliding_Loaded(object sender, RoutedEventArgs e)
        {
            _textAsRuns[_index].Style = _nextPending;
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
            if (_lastUserInput == _nextChar)
            {
                if (_lastInputCorrect)
                {
                    _textAsRuns[_index].Style = _submittedCorrect;
                }
                else
                {
                    _textAsRuns[_index].Style = _submittedIncorrect;
                    _lastInputCorrect = true;
                }

                _textAsRuns[_index + 1].Style = _nextPending;
                AnimateTextBlockSlide();
                _index++;
            }
            else
            {
                // After refreshing
                if (_lastUserInput == string.Empty)
                {
                    _textAsRuns[_index].Style = _nextPending;
                }
                else
                { 
                    _lastInputCorrect = false;
                    if (_nextChar == " ")
                    {
                        _textAsRuns[_index].Text = "_";
                    }
                    _textAsRuns[_index].Style = _nextIncorrect;                
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


        private void AnimateToStart()
        {
            var newThickness = new Thickness(0);

            var storyboard = new Storyboard();
            var slidingAnimation = new ThicknessAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                To = newThickness,
            };

            Storyboard.SetTargetProperty(slidingAnimation, new PropertyPath("Margin"));
            storyboard.Children.Add(slidingAnimation);

            storyboard.Begin(_animatedTextBlock);
        }

        #endregion

        #region Private helpers

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
