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
        public string WelcomeMessage { get; set; } = "Viewmodel connected, Resource dictionaries working";
    }
}
