using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private RoutedEventHandler mainResetHandler;
        private RoutedEventHandler mainNewHandler;
        private System.ComponentModel.CancelEventHandler mainCancelHandler;

        private string headerText = "";
        private string bodyText = "";


        public ResultsWindow(System.ComponentModel.CancelEventHandler mainCancelHandler, RoutedEventHandler mainResetHandler, RoutedEventHandler mainNewHandler, string headerText, string bodyText)
        {
            this.mainResetHandler = mainResetHandler;
            this.mainNewHandler = mainNewHandler;
            this.mainCancelHandler = mainCancelHandler;

            this.headerText = headerText;
            this.bodyText = bodyText;

            Closing += mainCancelHandler;

            Initialized += ResultsWindow_Initialized;

            InitializeComponent();
        }

        private void ResultsWindow_Initialized(object sender, EventArgs e)
        {
            ResultsHeaderTB.Text = headerText;
            ResultsBodyTB.Text = bodyText;
        }

        private void ResultsResetButton_Click(object sender, RoutedEventArgs e)
        {
            mainResetHandler.Invoke(sender, e);

            // clear the close handler so the main window doesn't get it, then close
            Closing -= mainCancelHandler;
            this.Close();
        }

        private void ResultsNewButton_Click(object sender, RoutedEventArgs e)
        {
            mainNewHandler.Invoke(sender, e);

            // clear the close handler so the main window doesn't get it, then close
            Closing -= mainCancelHandler;
            this.Close();
        }
    }
}
