Walkie-Talkie Radio []
----------------------

* FOR PINK MATERIALS // URP // HDRP *
This asset was made in Built-in pipeline but will work in any.
To fix either, 1) Create a new basic material in your pipeline and add the necessary textures
to albedo/ normal/ height, or 2) Simply use Unity's Material Conversion Tools

--------------------------------------------------------------------------------------------
You can find the radio in the Prefabs folder.

To create a new color: duplicate the WalkieBody material and modify its tint
Assign new color to the radio mesh pieces.

The screen is made of 3 pieces, with the LCD_DISPLAY being the piece in the middle that
represents the actual screen. It can be disabled to represent 'off'. The example scene uses
a 2D Texture on a Transparent Material. The GameObject's Transform is most important,
and acts as a container for anything you chose to use, UI elements, RenderTextures, etc.

The mesh, as is, needs a margin around the 2D texture so it doesn't get cut off.
The Walk_E_Display texture could be used as a template.

Happy Developing!
SpawnCampGames

--------------------------------------------------------------------------------------------