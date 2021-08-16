# VRCHeartRate
This mod can let VRChat show your heart rate via BLE heart rate monitor.

Before use this mod, make sure your PC support Bluetooth Low Enegy, and have a BLE heart rate monitor device.
# How to use
This mod rely on [MelonLoader](https://melonwiki.xyz/#/) you need to install it first.
Then put VRCHeartRate.dll in Mods folder.
## Avatar setup
Open up VRCHeartRate.unitypackage you will get these files

![](https://imgur.com/Wv9mOCR.png)

Drag HeartRateMonitor into your avatar, adjust it's transform as your wish.

![](https://imgur.com/ZE8Q4Cf.png)

If you want this monitor follow some part of your avatar, drag Digit_bone into that Bone.

You can also add material to model to make it looking good.

Add a parameter to VRC Expression parameter called HeartRate, also add this parameter to your avatar's FX layer's controller.

Add two layers in that controller like this

![](https://imgur.com/JZlbSiM.png)
![](https://imgur.com/muvHvSM.png)

Then upload your avatar and lunch the game.

Enjoy~

# Thanks
using [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)'s ParamLib.

It's a cool project, go check it!
