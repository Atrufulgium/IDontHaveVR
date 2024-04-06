IDontHaveVR
===========

Have *you* ever wanted to consume VR content in 3D, without a headset?  
Have *you* ever stared ever so longingly to youtube's "VR 180°" filter, wondering what you could watch?  
Are *you* capable of doing [cross- and parallel-eyed stereograms](https://en.wikipedia.org/wiki/Stereoscopy#Freeviewing)?

Then this project is for you.

..What's that? No one uses youtube's filters? And there's only like four people on this planet even aware of cross- and parallel-eyed stereograms?

Oh well, I made this for myself anyway.

Installation
------------
Grab a release, unzip it, and run `IDontHaveVR.exe`. It should probably maybe work; I mean, it's the Unity export. What could've gone wrong?

Controls
--------

- **O**: Open a file. *(Supported: Any video/images Unity can handle.)*
- **Alt-F4**: Close window. *(There's no proper close, use your OS' provided 'close window' hotkey.)*
- **F**: The jankiest fullscreen toggle you've ever seen.

- **J**, **<**, **>**, **L**: Seek -10s, -5s, +5s, +10s
- **K**, **Space**: Pause
- *Any digit*: Seek that 10% of the video. *(E.g. **6** goes to 60% of the video.)*

- **S**: Swap between cross- and parallel-eyed viewing. *(Assumes content is in Left/Right format.)*
- **R**: Swap between rendering modes `panoramic VR180`, `panoramic VR360`, and `fish-eyed VR180`. *(I have very basic auto-detection, but uhh it often gets it wrong.)*

- *Mouse*: Move the camera around. *(Hope your DPI is similar to mine.)*
- *Scroll* Change the FOV. *(While in proper VR, FOV is obvious, here it isn't. Zoom in to make features take up a more realistic part of your FOV, or zoom out to work with the effective halved screen width.)*

There's no regular video controls like "audio". Unironically use ffmpeg for volume control.

Contributing
------------
In decreasing order of recommendation, here are the possibilities:

1. Don't.
2. Instead, look for someone else who has done it already so you don't have to.
3. Port the code to a VLC extension or something instead.
4. Actually do something with this repo.

If you can't tell by the *clearly very proper commits*, this is a thrown-together-in-an-afternoon type project. There's no proper architecture, tests, or neat design to be found anywhere. I just wanted a working cross-eye VR viewer.

This project was made in Unity 2021.3.21f1 with no external dependencies.

License
-------
The good 'ol reliable MIT license.

(Or the WTFPL if you'd rather.)