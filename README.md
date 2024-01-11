# PianoVideoCreator

The PianoVideoCreator is a Unity project designed for creating captivating YouTube videos featuring scrolling piano notes.

## Examples and Usage

Utilize this tool for files with MIDI/MID and corresponding audio files. Below, explore the functionalities of each panel:

### Main Menu
The main menu facilitates navigation of existing functions and allows editing of the following attributes:
1. MIDI files
2. Audio files
3. Delay the start and end of piano notes
4. Set the note falling speed (NoteSpeed)
5. Save and load buttons for preserving settings between projects

![Main Menu](<mainmenu.PNG>)

![Low Note Speed](<slownotespeed.PNG>)
![High Note Speed](<fastnotespeed.PNG>)

### Color Settings
Adjust the colors of each track using the RGB(A) sliders or inputting a hexcode value.
It also includes a Randomize option that allow you to set the colors based on random color pallets.

*Color Settings Interface:*

![Color Settings](<colorsettings.PNG>)

### Text Editor
This menu currently includes functionality for the introductory panel, showcasing the song name, artist, and related elements.

![Text Editor](<titlesettings.PNG>)

### Render Screen
Adjust the aspect ratio (Work in Progress), framerate, and render the final video.

![Render Screen](<rendering.PNG>)

## Badapple
Included are files used to create a Bad Apple video. 
[![IMAGE ALT TEXT HERE](https://youtu.be/aXvx0wUw7TA)](https://youtu.be/aXvx0wUw7TA)

## Used Resources

This project incorporates the following external libraries:

### [drywetmidi](https://github.com/melanchall/drywetmidi)
A library enabling easy reading of MIDI files.

### [Acerola Color Generator](https://acerola.gg/colors.html)
Used for randomizing track colors.

### [FFmpeg](http://ffmpeg.org)
Utilized in the build for video rendering. No modifications were made to this library.

*Note: This software uses code from [FFmpeg](http://ffmpeg.org), licensed under [LGPLv2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html). The source can be downloaded [here](<link_to_your_sources>).* 

## Future Improvements

### UI
Enhance the user interface design, currently utilizing default button, panel, slider, and text looks.

### Background
Work in progress feature to incorporate videos and images in the background.

### Alternate Note Sprites
Develop a system to allow for different note appearances.

### Changing Individual Notes
Future feature to manually change note colors.

### Changing Piano
Piano looks pretty poor, and i would like to add a structure to choose between pre-build presets as well as making it easier to make presets

### Particles?
similar products have particals come from the piano, i dont have much experience with the unity particle system and i think these are very distracting but i'll consider adding it

## Contact

For those interested in contributing, please reach out to tommypower300.swag@gmail.com. (Note: It's an old email, okay -_-)
