using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClickBoom.Controls
{
    class ClickBoomField : Grid
    {

        public ClickBoomField() : base()
        {

        }

        /// <summary>
        /// Set up playing field with the specified number of spaces
        /// </summary>
        /// <param name="x">number of columns</param>
        /// <param name="y">number of rows</param>
        public void Setup(int x, int y)
        {
            Width = x * 26 + 4; // each button is 24 plus 1 padding on each side, plus 4 padding on row ends
            Height = y * 26 + 4; // each button is 24 plus 1 padding on top and bottom, plus 4 padding on column ends
            
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            // Try to add columns and rows a little more efficiently than blind interation
            int square = Math.Min(x, y); // smallest square we can make. add rows AND cols here

            for (int i = 0; i < square; i++)
            {
                RowDefinitions.Add(new RowDefinition());
                ColumnDefinitions.Add(new ColumnDefinition());
            }

            // calcualte the difference in the number of columns vs rows
            int diff = x - y;
            // If positive, add more columns. Negative, add more rows
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
            else if (diff < 0)
            {
                for (int i = diff; i < 0; i++)
                {
                    RowDefinitions.Add(new RowDefinition());
                }
            }
            // if diff is 0, do nothing

            //TODO
            // 1. generate a [x][y] field, scattering ~ .25 to .4 of spaces-worth of booms
            // 2. add ClickBoomButtons for every space in the above field
        }
    }
}
