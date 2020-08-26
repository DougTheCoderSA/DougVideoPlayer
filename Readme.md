# Doug's Video Player

I wanted a simple always-on-top video player that would automatically move out
of the way when the mouse approached it. This player uses VLC to play a video, and 
it will automatically move to a corner of the screen. **Hold** down the **Shift key** 
and you will be able to move the mouse over the video player window.

The player also responds to some keystrokes, but ONLY while the app is focused.

## Main controls

| Key   | Description                  |
|-------|------------------------------|
| Space | Toggle play / pause          |
| m     | Toggle mute                  |
| +     | Increase volume by 10%       |
| -     | Decrease volume by 10%       |
| [     | Increase transparency by 10% |
| ]     | Decrease transparency by 10% |

## Time skipping controls

The numbers 5-1 jump backwards by different amounts, and the numbers 6-0
jump forward by different amounts. If your forward jump would go past the end of
the video, it will instead go to 10 seconds before the end of the video.

| Key | Description     | Key | Description        |
|-----|-----------------|-----|--------------------|
| 5   | Back 10 seconds | 6   | Forward 10 seconds |
| 4   | Back 30 seconds | 7   | Forward 30 seconds |
| 3   | Back 5 minutes  | 8   | Forward 5 minutes  |
| 2   | Back 15 minutes | 9   | Forward 15 minutes |
| 1   | Back 1 hour     | 0   | Forward 1 hour     |

