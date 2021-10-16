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
        public State state { get; private set; } = State.REVEALED;

        public ClickBoomButton(int content)
        {
            Contents = content;

            Refresh();
        }

        protected override void OnClick()
        {
            base.OnClick();

            state = State.REVEALED;

            Refresh();

            // if the player clicked this one, highlight it red
            if(Contents == -1)
            {
                Background = Brushes.Red;
            }
        }

        private void Refresh()
        {
            switch (state)
            {
                case State.QUESTIONED:
                    Content = "?";
                    break;
                case State.FLAGGED:
                    Content = "F";
                    break;
                case State.REVEALED:
                    Content = (Contents == -1) ? "☼" : (Contents == 0) ? " " : ""+Contents;
                    break;
                case State.BLANK:
                default:
                    Content = " ";
                    break;
            }
        }

    }
}
