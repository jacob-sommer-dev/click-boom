using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClickBoom.Controls
{

    enum State
    {
        BLANK,
        QUESTIONED,
        FLAGGED,
        REVEALED,
    }

    class ClickBoomButton : Button
    {

        public int Contents { get; set; } = 0;

        private State _state = State.BLANK;
        public State State 
        {
            get 
            {
                return _state;
            }
            set 
            {
                _state = value;
                Refresh();
            } 
        }

        public ClickBoomButton(int content)
        {
            Contents = content;

            Refresh();
        }

        public void OnRightClick()
        {
            switch (State)
            {
                case State.BLANK:
                    State = State.FLAGGED;
                    (Parent as ClickBoomField).FlagsPlaced++;
                    break;
                case State.FLAGGED:
                    State = State.QUESTIONED;
                    (Parent as ClickBoomField).FlagsPlaced--;
                    break;
                case State.QUESTIONED:
                    State = State.BLANK;
                    break;
                case State.REVEALED:
                default:
                    // don't do anything
                    break;
            }
        }

        private void Refresh()
        {
            switch (State)
            {
                case State.QUESTIONED:
                    Content = "?";
                    break;
                case State.FLAGGED:
                    Content = "F";
                    break;
                case State.REVEALED:
                    
                    if(Contents == -1)
                    {
                        Content = "☼";
                        Background = Brushes.LightPink;
                    }
                    else
                    {
                        Content = (Contents == 0) ? " " : string.Format("{0}", Contents);
                        Background = Brushes.LightCyan;
                    }
                    break;
                case State.BLANK:
                default:
                    Content = " ";
                    break;
            }
        }

    }
}
