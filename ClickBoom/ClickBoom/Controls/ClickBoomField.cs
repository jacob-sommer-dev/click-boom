using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClickBoom.Controls
{
    public class ClickBoomField : Grid, System.ComponentModel.INotifyPropertyChanged
    {

        public delegate void ShowResultsScreenHandler(string headerText, string bodyText);
        public event ShowResultsScreenHandler ShowResultsScreen;

        private int x, y = 0;

        private int[][] _field;

        private int _booms = 0;
        private int _flagsPlaced = 0;
        public int FlagsPlaced
        {
            get { return _flagsPlaced; }
            set { _flagsPlaced = value; PropertyChanged(this, new PropertyChangedEventArgs("BoomsRemaining")); }
        }

        private object rightCkickedBtn;

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
            Margin = new Thickness(3.0, 3.0, 3.0, 3.0);

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
            this.Children.Clear();

            // we need more random than Random
            // otherwise it might get stuck trying the same spots over and over
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            this.x = x;
            this.y = y;

            _booms = (int)(x * y * .15);
            _flagsPlaced = 0;
            BoomsRemaining = _booms;

            timer.Stop();
            Time = 0;

            Width = x * 30;
            Height = y * 30;
            
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            // Try to add columns and rows a little more efficiently than blind iteration
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
            _field = new int[x][];
            for(int i = 0; i < x; i++)
            {
                _field[i] = new int[y];
            }

            //printGrid("initial grid");

            // pre-weight spots next to boundaries to "have adjacent booms" so we don't put booms in invalid spots
            SetBoundaryWeights(false);

            //printGrid("add preweights");

            int boomsToPlace = _booms;
            byte[] bytes = new byte[2];

            while (boomsToPlace > 0)
            {
                random.GetBytes(bytes);

                // clamp the randoms between 0 and our field lengths
                float frx = Math.Abs(bytes[0]) * (((float)x - 1) / byte.MaxValue);
                float fry = Math.Abs(bytes[1]) * (((float)y - 1) / byte.MaxValue);

                int rx = frx - (int)frx >= 0.5f ? (int)frx + 1 : (int)frx;
                int ry = fry - (int)fry >= 0.5f ? (int)fry + 1 : (int)fry;

                //Debug.WriteLine("Trying [" + rx + "][" + ry + "]");

                if (ValidBoomSpot(rx, ry)) // if it's a valid spot
                {
                    _field[rx][ry] = -1; // -1 is now a boom
                    IncrementAdjacent(rx, ry);// increment adjacent spots
                    boomsToPlace--;

                    //printGrid("boom at [" + rx + "][" + ry + "]");
                }
            }

            // booms are now placed, so we can remove the pre-weights
            SetBoundaryWeights(true);

            //printGrid("remove preweights");

            // now add a ClickBoomButton for each spot on the field
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    ClickBoomButton cbb = new ClickBoomButton(_field[i][j]);
                    Grid.SetColumn(cbb, i);
                    Grid.SetRow(cbb, j);
                    cbb.Click += ClickBoomButton_Click;
                    cbb.MouseRightButtonDown += Cbb_MouseRightButtonDown;
                    cbb.MouseRightButtonUp += Cbb_MouseRightButtonUp;
                    Children.Add(cbb);
                }
            }

            random.Dispose();
        }

        private void Cbb_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (rightCkickedBtn == sender && sender is ClickBoomButton)
            {
                ClickBoomButton btn = sender as ClickBoomButton;
                btn.OnRightClick();
            }
        }

        private void Cbb_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rightCkickedBtn = sender;
        }

        private void ClickBoomButton_Click(object sender, RoutedEventArgs e)
        {
            ClickBoomButton cbb = (ClickBoomButton)sender;

            if(cbb.State == State.FLAGGED || cbb.State == State.QUESTIONED)
            {
                // don't do anything
                return;
            }

            cbb.State = State.REVEALED;

            // don't respond to clicks anymore
            cbb.Click -= ClickBoomButton_Click;

            // if the player clicked this one, highlight it red and end the game
            if (cbb.Contents == -1)
            {
                cbb.Background = Brushes.Red;
                EndGame(false);
                return;
            }
            else if (cbb.Contents == 0)
            {
                // find all the adjacent 0s and reveal them
                int x = Grid.GetColumn(cbb);
                int y = Grid.GetRow(cbb);

                RevealAdjacentZeros(x, y);
            }

            if(CheckGameDone())
            {
                EndGame(true);
            }
        }

        private void SetBoundaryWeights(bool decrement)
        {
            
            if(decrement) // have to be careful not to decrement booms (-1)
            {
                // first, do the corners, weighted at 5.
                // TODO determine if Math.Max or ifs are faster
                
                //_field[0][0] = Math.Max(-1, _field[0][0] - 5);
                //_field[0][_field[0].Length - 1] = Math.Max(-1, _field[0][_field[0].Length - 1] - 5);
                //_field[_field.Length - 1][0] = Math.Max(-1, _field[_field.Length - 1][0] - 5);
                //_field[_field.Length - 1][_field[0].Length - 1] = Math.Max(-1, _field[_field.Length - 1][_field[0].Length - 1] - 5);

                if (_field[0][0] != -1) { _field[0][0] -= 5; }
                if (_field[0][_field[0].Length - 1] != -1) { _field[0][_field[0].Length - 1] -= 5; }
                if (_field[_field.Length - 1][0] != -1) { _field[_field.Length - 1][0] -= 5; }
                if (_field[_field.Length - 1][_field[0].Length - 1] != -1) { _field[_field.Length - 1][_field[0].Length - 1] -= 5; }

                // now do the rest of the boundary spots, weighted at 3
                for (int i = 1; i < _field.Length - 1; i++)
                {
                    // column starts and ends
                    if (_field[i][0] != -1) { _field[i][0] -= 3; }
                    if (_field[i][_field[0].Length - 1] != -1) { _field[i][_field[0].Length - 1] -= 3; }
                }

                for (int j = 1; j < _field[0].Length - 1; j++)
                {
                    // row starts and ends
                    if (_field[0][j] != -1) { _field[0][j] -= 3; }
                    if (_field[_field.Length - 1][j] != -1) { _field[_field.Length - 1][j] -= 3; }
                }

            }
            else // initial increment, just do it
            {
                // first, do the corners, weighted at 5
                _field[0][0]
                    = _field[0][_field[0].Length - 1]
                    = _field[_field.Length - 1][0]
                    = _field[_field.Length - 1][_field[0].Length - 1]
                    = 5;

                // now do the rest of the boundary spots, weighted at 3
                for (int i = 1; i < _field.Length - 1; i++)
                {
                    // column starts and ends
                    _field[i][0] = _field[i][_field[0].Length - 1] = 3;
                }

                for (int j = 1; j < _field[0].Length - 1; j++)
                {
                    // row starts and ends
                    _field[0][j] = _field[_field.Length - 1][j] = 3;
                }
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
        private bool ValidBoomSpot(int x, int y)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    int fx = x + i;
                    int fy = y + j;

                    if (!(fx < 0 || fx >= _field.Length || fy < 0 || fy >= _field[0].Length))
                    {
                        int val = _field[x + i][y + j];

                        if ((fx == x && fy == y && val < 0) || val > 6)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void IncrementAdjacent(int x, int y)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int fx = x + i;
                    int fy = y + j;

                    if (fx >= 0
                        && fx < _field.Length
                        && fy >= 0
                        && fy < _field[0].Length
                        && _field[x + i][y + j] > -1 // don't increment if it's a boom
                        )
                    {
                        _field[x + i][y + j]++;
                    }
                }
            }
        }

        private void RevealAdjacentZeros(int x, int y)
        {
            // maybe this isn't supposed to reveal diagonal adjacents?
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int fx = x + i;
                    int fy = y + j;

                    if (fx >= 0
                        && fx < _field.Length
                        && fy >= 0
                        && fy < _field[0].Length
                        //&& _field[fx][fy] == 0  // don't check here, reveal numbers too
                        )
                    {
                        // reveal the button and recursively look for more
                        ClickBoomButton btn = Children
                            .Cast<ClickBoomButton>()
                            .First(item => Grid.GetColumn(item) == fx && Grid.GetRow(item) == fy);
                        if(btn != null && btn.State != State.REVEALED)
                        {
                            btn.State = State.REVEALED;

                            // don't respond to clicks anymore
                            btn.Click -= ClickBoomButton_Click;

                            btn.Background = Brushes.LightCyan;

                            // if it's 0, reveal neighbors
                            if(_field[fx][fy] == 0)
                            {
                                RevealAdjacentZeros(fx, fy);
                            }
                        }
                        

                    }
                }
            }
        }

        private bool CheckGameDone()
        {
            for (int i = 0; i < _field.Length; i++)
            {
                for (int j = 0; j < _field[0].Length; j++)
                {
                    if(_field[i][j] != -1)
                    {
                        ClickBoomButton btn = Children
                            .Cast<ClickBoomButton>()
                            .First(item => Grid.GetColumn(item) == i && Grid.GetRow(item) == j);

                        if(btn != null && btn.State != State.REVEALED)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
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

        public void Stop()
        {
            timer.Stop();
        }

        public void EndGame(bool win)
        {
            Stop();

            foreach (UIElement e in Children)
            {
                if(e is ClickBoomButton)
                {
                    ClickBoomButton cbb = e as ClickBoomButton;

                    // don't respond to clicks anymore
                    cbb.Click -= ClickBoomButton_Click;

                    if(!win)
                    {
                        // reveal the board one button at a time
                        if (cbb.State != State.REVEALED)
                        {
                            cbb.State = State.REVEALED;
                        }
                    }
                }
            }

            // display results window
            if(win)
            {
                ShowResultsScreen.Invoke("You Win!", string.Format("Your time was: {0}", Time));
            }


        }

        // useful for debugging
        public void PrintGrid(string additional = "")
        {
            Debug.WriteLine("//// GRID: " + additional + " ////");
            for (int j = 0; j < _field[0].Length; j++)
            {
                for (int i = 0; i < _field.Length; i++)
                {
                    Debug.Write("[" + _field[i][j] + "]");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("////////////////////////");
        }
    }
}
