using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace ClickBoom.Controls
{
    public class ClickBoomField : Grid, System.ComponentModel.INotifyPropertyChanged
    {

        private int x, y = 0;

        private int[,] _field;

        private int _booms = 0;
        private int _flagsPlaced = 0;
        public int BoomsRemaining
        {
            get { return _booms - _flagsPlaced; }
            private set
            {
                PropertyChanged(this, new PropertyChangedEventArgs("BoomsRemaining"));
            }
        }

        private int _time = 0;
        public int Time
        {
            get { return _time; }
            private set
            {
                _time = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Time"));
            }
        }

        private Timer timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ClickBoomField() : base()
        {
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time++;
        }

        /// <summary>
        /// Set up playing field with the specified number of spaces
        /// </summary>
        /// <param name="x">number of columns</param>
        /// <param name="y">number of rows</param>
        public void Setup(int x, int y)
        {
            this.x = x;
            this.y = y;

            _booms = BoomsRemaining = (int)(x * y * .2);

            timer.Stop();
            Time = 0;

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
            // if diff is 0, we're done

            // generate a [x][y] field, scattering ~.2 of total spaces-worth of booms
            _field = new int[x, y];

            int boomsToPlace = _booms;
            Random rand = new Random();

            // TODO: SLOOOOWWWW. Figure out a better way
            while (boomsToPlace > 0)
            {
                int rx = rand.Next(0, x);
                int ry = rand.Next(0, y);

                if (validBoomSpot(rx, ry)) // don't completely surround
                {
                    _field[rx, ry] = -1; // -1 is a boom
                    boomsToPlace--;
                }
            }

            // now count the adjacent booms and add a ClickBoomButton for each spot on the field
            for (int i = 0; i < x; i++)
            {
                for(int j = 0; j < y; j++)
                {
                    _field[i, j] = _field[i, j] == -1 ? -1 : countAdjacentBooms(i, j);

                    ClickBoomButton cbb = new ClickBoomButton(_field[i, j]);
                    Grid.SetColumn(cbb, i);
                    Grid.SetRow(cbb, j);
                    cbb.Click += ClickBoomButton_Click;
                    this.Children.Add(cbb);
                }
            }


        }

        private void ClickBoomButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ClickBoomButton cbb = (ClickBoomButton)sender;

            if(cbb.Contents == -1)
            {

            }
        }

        /// <summary>
        /// Determines whether or not the spot specified by x, y is a valid spot to
        /// place a boom. A spot is considered valid if the number of adjacent bombs
        /// and boundary spots is 6 or less for the current and neighboring spots.
        /// Limit is 6, so if the boom were to be added, adjacent would be 7,
        /// so not surrounded.
        /// </summary>
        /// <param name="x">column of space to check</param>
        /// <param name="y">row of space to check</param>
        /// <returns>bool true if valid place for a boom</returns>
        private bool validBoomSpot(int x, int y)
        {
            return _field[x, y] != -1
                && countAdjacentBooms(x - 1, y - 1, true) < 7
                && countAdjacentBooms(x, y - 1, true) < 7
                && countAdjacentBooms(x + 1, y - 1, true) < 7
                && countAdjacentBooms(x - 1, y, true) < 7
                && countAdjacentBooms(x, y, true) < 7
                && countAdjacentBooms(x + 1, y, true) < 7
                && countAdjacentBooms(x - 1, y + 1, true) < 7
                && countAdjacentBooms(x, y + 1, true) < 7
                && countAdjacentBooms(x + 1, y + 1, true) < 7;
        }

        /// <summary>
        /// Counts the number of booms adjacent to the space at x,y 
        /// </summary>
        /// <param name="x">column of space to check</param>
        /// <param name="y">row of space to check</param>
        /// <param name="countBoundaries">default: false. Count boundaries as bombs, useful for generation algorithm</param>
        /// <returns>int number of booms (and boundary spots) to the current spot</returns>
        private int countAdjacentBooms(int x, int y, bool countBoundaries = false)
        {
            int adjacent = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    try
                    {
                        int val = _field[x + i, y + j]; // IndexOutOfRange should short circuit here if needed

                        if (val == -1)
                        {
                            adjacent++;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        // don't do anything unless we're counting boundaries
                        if (countBoundaries)
                        {
                            adjacent++;
                        }
                    }
                }
            }
            

            //// if it's in a corner, increment by 5
            //if ((x == 0 && y == 0) 
            //    || (x == 0 && y == this.y)
            //    || (x == this.x && y == 0)
            //    || (x == this.x && y == this.y)
            //    )
            //{
            //    adjacent += 5;
            //}
            //// it's not in a corner, but it's along a side, increment by 3
            //else if (x == 0 || x == this.x || y == 0 || y == this.y)
            //{
            //    adjacent += 3;
            //}


            return adjacent;
        }

        public void Reset()
        {
            Setup(x, y);
            Start();
        }

        public void Start()
        {
            timer.Start();
        }
    }
}
