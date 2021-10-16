# click-boom

#### C#/WPF/.NET 4.8 recreation of a famous game where you click to reveal safe spots while avoiding traps

Progress:
1. Options window for selecting playfield size
2. Options window passes *close* and *start* button events to main (playing field) window
3. Pseudo-random trap placement algorithm that respects trap placement rules
    - Don't surround a spot with traps
    - Place a certain number of traps, determined by a ratio of the total number of spots (currently .15)
4. Playing field is displayed, spots represented by buttons
5. Reset button generates a new field with the current size
6. Timer counts seconds from start time, resets when game is reset
7. Counter for traps in play
8. DONE Placement algorithm is *slow* - speed it up
9. DONE All spots are revealed immediately (to check placement algorithm). Make them hidden by default
10. DONE Add left and right click handlers to the buttons (left = reveal spot, right = flag or ? spot)
11. DONE Flagged spots should decrement trap counter
12. Done Add game end state. When all non-trap spots are revealed, stop the timer, display a message to the user with their time. Allow restart/new game

TO-DOs
1. Timer (or update binding) is inconsistent - determine issue and fix
2. Clean up and add additional comments
