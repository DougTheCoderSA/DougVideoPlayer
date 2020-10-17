# Doug's Video Player

I wanted a simple always-on-top video player that would automatically move out
of the way when the mouse approached it. This player uses an embedded 
VLC control to play a video, and 
it will automatically move to a corner of the screen. **Hold** down the **Shift key** 
and you will be able to move the mouse over the video player window.

Note that if the window is resized to take up more than half the screen width,
or more than 2/3 of the screen height, then the window stops moving out of the way,
since at that size you are likely to want it to stay put.

You can also make mouse clicks and keystrokes go through to underlying windows when the
main window is transparent. This means that you can decide between having the window
jump out of the way of the mouse cursor, and having the window partially transparent
and be able to keep working underneath the video window. To be able to send clicks
and keystrokes to the video player window again, click on the app in the taskbar
and press the **t** key.

The player also responds to some keystrokes, but ONLY while the app is focused.

## Main controls

| Key   | Description                  |
|-------|------------------------------|
| Space | Toggle play / pause          |
| m     | Toggle mute                  |
| f     | Toggle fullscreen            |
| +     | Increase volume by 10%       |
| -     | Decrease volume by 10%       |
| [     | Increase transparency by 10% |
| ]     | Decrease transparency by 10% |
| t     | Toggle click and keystroke fallthrough |
| /     | Resize the player width to match the playing video aspect ratio |
| b     | Toggle the Menu and Title Bar of the player window |

## Playlist controls

| Key | Description            |
|-----|------------------------|
| ,   | Previous playlist item |
| .   | Next playlist item     |
| <   | First playlist item    |
| >   | Last playlist item     |

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

## Note about the Shift key

You only need to hold down the Shift key until the pointer moves within the
video player window. As long as you keep the mouse cursor within the window,
it won't jump out of the way, but as soon as the mouse leaves the window, it
will once again avoid the mouse cursor.

## Opening and Queueing Media

The player accepts a single **command line** argument, a file path of media to play.
This means you can right-click a media file, go to the Open With menu, browse
for the player executable and open the file that way. You can even associate this
player with that file extension this way if desired.

When opening media using the **File -> Open** menu option, you can select 
multiple files. The first file selected will begin playing immediately, 
and the rest will be added to the queue. When the current video finishes 
playing the next one will automatically begin.

The current playlist is automatically saved. You can clear it by selecting
File -> Clear Playlist.
