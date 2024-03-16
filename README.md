# Outer Scout

A toolbox for creating and importing cinematic shots from Outer Wilds into Blender!

![thumbnail](thumbnail.png)

## Basic usage example

1. Open Blender and Outer Wilds at the same time
2. Get to the desired location in Outer Wilds and click "create a scene" in the [Outer Scout add-on](https://github.com/Picalines/outer-scout-blender) menu
3. The Blender add-on imports a planet model from Outer Wilds and sets it to the desired position
4. Put the camera in Blender and animate it
5. Click on the record button in the add-on menu!

After that, the footage recorded from the game is imported into your Blender project and installed as the background of the camera. This way you can embed any 3D Blender model into a scene from Outer Wilds!

## Requirements

Download the zip archive of the [Blender add-on](https://github.com/Picalines/outer-scout-blender) from the Releases tab and [install it](https://docs.blender.org/manual/en/latest/editors/preferences/addons.html#installing-add-ons).

[FFmpeg](https://ffmpeg.org/about.html) is required for video recording functionality. The `ffmpeg` command must be available in the PATH (a specific path can be specified in the settings). For developers on Windows, I recommend installing ffmpeg via [scoop](https://bjansen.github.io/scoop-apps/main/ffmpeg)

The add-on and the mod communicate over the HTTP protocol on local port `2209`. You can change the port in the settings of two programs

## How it works

This mod implements the [API](OuterScout.WebApi/resources/openapi.yaml) that can:
- Create one or more cameras of different types (even [360](OuterScout.WebApi/resources/openapi.yaml#L267)!)
- Animate their transform and [perspective](OuterScout.WebApi/resources/openapi.yaml#L854) using [keyframes](OuterScout.WebApi/resources/openapi.yaml#L370)
- Create [recorders](OuterScout.WebApi/resources/openapi.yaml#L420) of their RenderTextures (color or depth)
- [Record](OuterScout.WebApi/resources/openapi.yaml#L119) your scene with a [fixed frame rate](https://docs.unity3d.com/ScriptReference/Time-captureFramerate.html)

All API paths are available in the [swagger-ui](https://github.com/swagger-api/swagger-ui) interface. You can view them by opening a web browser and navigating to `localhost:2209`.

