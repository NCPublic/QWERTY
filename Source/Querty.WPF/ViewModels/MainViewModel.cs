using System;
using System.Collections.Generic;
using System.Text;

namespace Querty.WPF
{
    /// <summary>
    /// The viewmodel for the main window
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private string _textToType;
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

        public MainViewModel()
        {
            _textToType = "Weit hinten, hinter den Wortbergen, fern der Länder Vokalien und Konsonantien leben die Blindtexte. Abgeschieden wohnen sie in Buchstabhausen an der Küste des Semantik, eines großen Sprachozeans. Ein kleines Bächlein namens Duden fließt durch ihren Ort und versorgt sie mit den nötigen Regelialien. Es ist ein paradiesmatisches Land, in dem";
        }
    }
}
