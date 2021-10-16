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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClickBoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int FIELD_DIMEN_MAX = 48;
        public static readonly int FIELD_DIMEN_MIN = 8;

        public Controls.ClickBoomField PlayingField { get; private set; }

        public Views.OptionWindow OptionWindow { get; private set; }
        public Views.ResultsWindow ResultsWindow { get; private set; }

        public MainWindow()
        {
            Initialized += MainWindow_Initialized;

            Closing += MainWindow_Closing;

            SizeChanged += MainWindow_SizeChanged;

            PlayingField = new Controls.ClickBoomField();
            PlayingField.HorizontalAlignment = HorizontalAlignment.Center;
            PlayingField.VerticalAlignment = VerticalAlignment.Top;
            PlayingField.ShowResultsScreen += PlayingField_ShowResultsScreen;

            DataContext = this;

            InitializeComponent();

        }

        private void PlayingField_ShowResultsScreen(string headerText, string bodyText)
        {
            ResultsWindow = new Views.ResultsWindow(MainWindow_Closing, ResetButton_Click, NewButton_Click, headerText, bodyText);
            ResultsWindow.Show();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlayingFieldScroller.Width = e.NewSize.Width - 30;//Math.Min(e.NewSize.Width, PlayingField.Width);
            PlayingFieldScroller.Height = e.NewSize.Height - 120;//Math.Min(e.NewSize.Height - 180, PlayingField.Height);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {

            PlayingFieldScroller.Content = PlayingField;
            NewGame();
            
        }

        private void NewGame()
        {
            this.Hide();
            OptionWindow = new Views.OptionWindow(MainWindow_Closing, GoButton_Click);
            OptionWindow.Show();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            Util.ClickBoomGoEventArgs args = (Util.ClickBoomGoEventArgs) e;

            // ensure min and max sizes are respected
            int x = Math.Max(args.FieldSize.x, FIELD_DIMEN_MIN);
            x = Math.Min(x, FIELD_DIMEN_MAX);

            int y = Math.Max(args.FieldSize.y, FIELD_DIMEN_MIN);
            y = Math.Min(y, FIELD_DIMEN_MAX);

            // set up playing field
            PlayingField.Setup(x, y);

            // extra padding for window width/height
            Width = Math.Min(PlayingField.Width + 100, 1280);
            Height = Math.Min(PlayingField.Height + 180, 960);

            this.Show();

            PlayingField.Start();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if(ResultsWindow != null && ResultsWindow.IsVisible)
            {
                ResultsWindow.Closing -= MainWindow_Closing;
                ResultsWindow.Close();
            }

            PlayingField.Reset();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsWindow != null && ResultsWindow.IsVisible)
            {
                ResultsWindow.Closing -= MainWindow_Closing;
                ResultsWindow.Close();
            }

            NewGame();
        }
    }
}
