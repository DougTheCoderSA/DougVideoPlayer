# Doug's Video Player

I wanted a simple always-on-top video player that would automatically move out
of the way when the mouse approached it. This player uses an embedded 
VLC control to play a video, and 
it will automatically move to a corner of the screen. **Hold** down the **Shift key** 
and you will be able to move the mouse over the video player window.

Note that if the window is resized to take up more than half the screen width,
or more than 2/3 of the screen height, then the window stops moving out of the way,
since at that size you are likely to want it to stay put.


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

You can add new videos to the **queue** without interrupting the currently playing
one by selecting **File -> Enqueue** and opening one or more files. These will be
added to the queue.

Right now there is no way to view or manage the playlist, nor can it be saved
as a text file - but this functionality should not too tricky to implement.

I'm still of 2 minds whether a queue is the best data structure - once an item
is popped off the queue it is gone, meaning there is no way to go to a previous
video in the queue. I will probably end up changing this to a list structure
or similar.