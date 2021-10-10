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
        public static readonly int FIELD_DIMEN_MIN = 6;

        public Views.OptionWindow OptionWindow { get; private set; }

        public MainWindow()
        {
            this.Initialized += MainWindow_Initialized;

            this.Closing += MainWindow_Closing;

            InitializeComponent();

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
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
            Width = PlayingField.Width + 42;
            Height = PlayingField.Height + 42;

            this.Show();
        }
    }
}
