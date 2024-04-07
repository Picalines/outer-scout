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
- Create custom Unity GameObjects
- Add different types of cameras to them (even [360](OuterScout.WebApi/resources/openapi.yaml#L806)!)
- Animate their transform and [perspective](OuterScout.WebApi/resources/openapi.yaml#L758) using [keyframes](OuterScout.WebApi/resources/openapi.yaml#L471)
- Create [recorders](OuterScout.WebApi/resources/openapi.yaml#L529) of their RenderTextures (color or depth)
- [Record](OuterScout.WebApi/resources/openapi.yaml#L119) your scene with a [fixed frame rate](https://docs.unity3d.com/ScriptReference/Time-captureFramerate.html)

All API paths are available in the [swagger-ui](https://github.com/swagger-api/swagger-ui) interface. You can view them by opening a web browser and navigating to `localhost:2209`.

## A more in-depth API example

Let's do something like the [dolly zoom](https://en.wikipedia.org/wiki/Dolly_zoom) effect. First we need to create an Outer Scout scene:

```json5
// POST /scene
{
    "hidePlayerModel": true
}
```

The example will be easier to read if we create an additional "origin of the scene" object:

```json5
// POST /objects
{
    "name": "scene.origin",
    "transform": {
        "parent": "TimberHearth_Body",
        "position": [20.8809223, -41.51514, 186.207733],
        "rotation": [0.461064935, -0.4372242, -0.6413971, 0.429958254]
    }
}
```

Now we need a camera. The mod can create several cameras of different types, but we will only need one regular (perspective) camera:

```json5
// POST /objects
{
    "name": "mainCamera",
    "transform": {
        "parent": "scene.origin"
    },
}
```

```json5
// POST /objects/mainCamera/camera
{
    "id": "main",
    "type": "perspective",
    "gateFit": "horizontal",
    "resolution": {
        "width": 1920,
        "height": 1080
    },
    "perspective": {
        // See the Unity documentation on the physical properties of cameras
        // https://docs.unity3d.com/Manual/PhysicalCameras.html
        "focalLength": 20,
        "sensorSize": [36, 24],
        "lensShift": [0, 0],
        "nearClipPlane": 0.1,
        "farClipPlane": 5000
    }
}
```

Now we need to animate the camera position and focal length:

```json5
// PUT /objects/mainCamera/keyframes
{
    "properties": {
        "transform.position.z": {
            "keyframes": {
                "1": { "value": -2 },
                "60": { "value": 2 }
            }
        },
        "camera.perspective.focalLength": {
            "keyframes": {
                "1": { "value": 40 },
                "60": { "value": 10 }
            }
        }
    }
}
```

Then we need to specify which file on the disk to save the video to. To do this, we need to create a camera recorder:

```json5
// POST /objects/mainCamera/recorders
{
    "property": "camera.renderTexture.color",
    "outputPath": "D:\\assets\\color.mp4",
    "format": "mp4"
}
```

And finally, we can record the created scene:

```json5
// POST /scene/recording
{
    "startFrame": 1,
    "endFrame": 60,
    "frameRate": 60
}
```

https://github.com/Picalines/outer-scout/assets/42614422/7c9d7ad6-5e62-48a0-9215-d89a342c56e5
