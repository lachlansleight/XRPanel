# XRPanel

*Proper readme to come!*

### Intro

XRPanel is designed to be a volumetric, spatial UI for VR and AR devices. It's been designed within a couple of key design constraints:

1. Completely device agnostic - no code anywhere tying it to any particular SDK, device, use-case or assumption in any way
    
    The XRPointer is the only component that isn't part of an XRPanel hierarchy. Within reason, you can have as many of these in the scene as you like (including zero) and the script only queries the position of the Transform it's attached to.
2. Touch-only - no requirements for any discrete input like button pressing or gesture recognition, and also no Raycasting!
    
    This has been done by using depth as a discrete input analog. By pressing 'into' a control's bounding rectangle, that control is activated, and so long as the XRPointer doesn't pass back through the control's local XY plane, the control will stay activated.
3. Physical - should be based on real-world controls like sliders, knobs, buttons and toggles
    
    This is to aid intuition, and to keep things broadly consistent with the Unity 4.6 UI paradigms.
4. UnityEvent + Delegate Invocations
   
   This allows the system to be used by programmers by hooking into `OnStateChanged` and `OnValueChanged` delegates, or by designers through the familiar UnityEvent inspector
  
If you are reading this, then XRPanel is still in preview and is definitely **not** finished! I'd love to get your feedback, any bugs you've found, and any design philosophy thoughts you might have on the project!

### Temporary Setup Guide

Check out the examples scenes for the overall gist of things, or follow these steps:

1. Drag an XrpPanel prefab into your scene. 
    - Note that you should treat its scale similarly to a Unity world-space canvas - it should not be thought of as the same thing as the XrpPanel's size. For that, change the width and height values in the XrpPanelGeometry script on the XRPanel prefab.
    - After changing these values, you'll need to right click the XrpPanelGeometry component and choose 'FixGeometry'. This will be done on Awake if you don't do it. This will be done in the Inspector as you change the values later on.
2. Start dragging controls into your XrpPanel. Note that these must be children of the XrpPanel for the system to work. You can have multiple XrpPanels in a single scene.
3. Resize your controls by changing their local scales, then right-click the control component (e.g. XrpSlider) and choose 'FixGeometry' to fix all the cube scales.
    - For XrpIntSlider, XrpIntDial and XrpEnumSlider, the context menu will also contain 'FixMarkerGeometry' which will ensure that there are the correct number of markers for the number of discrete options for these controls.
4. Start adding Unity UI labels to the world-space Canvas that is a child of the XrpPanel prefab
5. Add some XrpPointers to your scene! On any GameObject that you'd like to function as an XrpPointer, add an XrpPointer component to it. That simple! You can add one at a local offset to an Oculus Go controller, or to the tips of all your Leap Motion fingers, or to the tip of your Magic Leap controller. :D
6. Start hooking up control events to your own objects and scripts!
