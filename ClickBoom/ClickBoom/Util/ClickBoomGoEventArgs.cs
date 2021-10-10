using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBoom.Util
{
    partial class ClickBoomGoEventArgs : System.Windows.RoutedEventArgs
    {

        public (int x, int y) FieldSize { get; private set; }

        public ClickBoomGoEventArgs(System.Windows.RoutedEventArgs args, (int x, int y) fieldSize) : base(args.RoutedEvent, args.Source)
        {
            Handled = base.Handled;

            FieldSize = fieldSize;
            
        }

    }
}
