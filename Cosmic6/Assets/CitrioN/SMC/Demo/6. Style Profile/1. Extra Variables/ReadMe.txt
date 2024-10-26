This demo scene showcases the use of additional/custom variables.
If you take a look at the StyleProfile at Settings/UGUI/Menu/StyleProfile_UGUI_InputElement_Background
you will find the variables 'Primary Image Roundness' and 'Input Element Background Image Roundness' at the bottom.
Those were not on the generated style profile but added later.
In this case the variables already exist on the prefabs so they will have an effect right away.
You can add your own variables to your StyleProfile and use a StyleListener on your prefabs to implement what should
happen when your variable changes. Take a look at the numerous prefabs at Settings/UGUI/Elements/Prefabs and how
they implement StyleListeners to change things like text colors or sizes. 
In this example menu we also use the text underlay variables in combination with the underlay dilate and softness to
simulate a text glow effect often found in scifi games.

For more information about the StyleProfile system check out the documentation at Packages/CitrioN - StyleProfile/Documentation.