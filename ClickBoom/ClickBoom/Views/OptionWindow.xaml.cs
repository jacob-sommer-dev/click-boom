using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClickBoom.Views
{
    /// <summary>
    /// Interaction logic for OptionWindow.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        // regex for one or two digits exclusively
        private static readonly Regex onlyDigitsRegex = new Regex("^\\d{1,2}\\z");

        private RoutedEventHandler mainGoHandler;
        private System.ComponentModel.CancelEventHandler mainCancelHandler;

        public int FieldX { get; set; } = 12;
        public int FieldY { get; set; } = 12;

        /// <summary>
        /// Construct a new Option window. This window lets the user decide how large of a playing field to initialize.
        /// Cancel and click handlers are passed from the main window so that this window can let it know when it's closing
        /// or when the Go! button has been clicked. In the second case, the default event args are wrapped with field
        /// size arguments via a event args subclass <code>Util.ClickBoomGoEventArgs.cs</code>
        /// </summary>
        /// <param name="cancelHandler">The button handler to call if this window is closed</param>
        /// <param name="mainClickHandler">The button handler to call with event args plus field size info</param>
        public OptionWindow(System.ComponentModel.CancelEventHandler cancelHandler, RoutedEventHandler mainClickHandler)
        {
            mainCancelHandler = cancelHandler;
            mainGoHandler = mainClickHandler;

            Closing += cancelHandler;

            DataContext = this;

            InitializeComponent();
        }

        /// <summary>
        /// Click handler for the Go! button. Bundle the event args with the chosen
        /// field X and Y dimensions, then pass them to the main window.
        /// </summary>
        /// <param name="sender">the sender of this event</param>
        /// <param name="e">event arguments</param>
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            (int x, int y) xyInfo = (12, 12); // initial size is small

            if((bool)CustomRB.IsChecked)
            {
                xyInfo = (int.Parse(CustomSizeXTB.Text), int.Parse(CustomSizeYTB.Text));
            }
            else if((bool)LargeRB.IsChecked)
            {
                xyInfo = (24, 24);
            }
            else if ((bool)MediumRB.IsChecked)
            {
                xyInfo = (18, 18);
            }

            // wrap the Go! click args with ours to piggy-back xyInfo to the main window
            mainGoHandler.Invoke(sender, new Util.ClickBoomGoEventArgs(e, xyInfo));

            // clear the close handler so the main window doesn't get it, then close
            Closing -= mainCancelHandler;
            this.Close();
        }

        /// <summary>
        /// Event handler that enables or disables the text boxes when the Custom radio button is checked or not
        /// </summary>
        /// <param name="sender">the sender of this event</param>
        /// <param name="e">event arguments</param>
        private void CustomRB_Checked(object sender, RoutedEventArgs e)
        {
            CustomSizeXTB.IsEnabled = CustomSizeYTB.IsEnabled = (bool)CustomRB.IsChecked;
        }

        /// <summary>
        /// Event handler that ensures text is numerical. If so, enable the Go! button, or
        /// disable it if not.
        /// </summary>
        /// <param name="sender">the sender of this event</param>
        /// <param name="e">event arguments</param>
        private void CustomSizeTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            // have to nullcheck because this gets called during initialization
            if(!(GoButton is null || CustomSizeXTB is null || CustomSizeXTB.Text is null || CustomSizeYTB is null || CustomSizeYTB.Text is null))
            {
                GoButton.IsEnabled = onlyDigitsRegex.IsMatch(CustomSizeXTB.Text) && onlyDigitsRegex.IsMatch(CustomSizeYTB.Text);
            }
        }
    }
}
