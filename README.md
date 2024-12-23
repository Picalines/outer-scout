# Outer Scout

A toolbox for creating and importing cinematic shots from Outer Wilds into Blender!

![thumbnail](thumbnail.png)

The Outer Scout mod allows other programs to make advanced video recordings from Outer Wilds. It opens a software interface with which you can animate cameras and other objects, and then export video files and other recorded game data

> [!IMPORTANT]
> This repository contains only the backend part for the [Outer Scout Blender addon](https://github.com/Picalines/outer-scout-blender/blob/master/README.md) - visit its page to see more sample videos that you can make with this mod!

## Requirements

> [!IMPORTANT]
> These requirements relate specifically to the mod, not the add-on for Blender

[FFmpeg](https://ffmpeg.org/about.html) is required for video recording functionality. The `ffmpeg` command must be available in the PATH (a specific path to the executable can be specified in the settings). On Windows I recommend installing ffmpeg via [scoop](https://bjansen.github.io/scoop-apps/main/ffmpeg)

The add-on and the mod communicate over the HTTP protocol on local port `2209`. You can change the port in the settings of two programs

## How it works

This mod implements the [API](OuterScout.WebApi/resources/openapi.yaml) that can:
- Create custom Unity GameObjects
- Add different types of cameras to them (even [360](OuterScout.WebApi/resources/openapi.yaml#L806)!)
- Animate their transform and [perspective](OuterScout.WebApi/resources/openapi.yaml#L758) using [keyframes](OuterScout.WebApi/resources/openapi.yaml#L471)
- Create [recorders](OuterScout.WebApi/resources/openapi.yaml#L529) of their RenderTextures (color or depth)
- [Record](OuterScout.WebApi/resources/openapi.yaml#L119) your scene with a [fixed frame rate](https://docs.unity3d.com/ScriptReference/Time-captureFramerate.html)

## Documentation

All API endpoints are available in the [swagger-ui](https://github.com/swagger-api/swagger-ui) interface. You can view them by opening a web browser and navigating to `localhost:2209` while you game's running

## A more in-depth API example

> [!WARNING]
> I may forget to update this section if I change the API. Please see the specific documentation only in the OpenAPI schema

Let's do something like the [dolly zoom](https://en.wikipedia.org/wiki/Dolly_zoom) effect. First we need to create an Outer Scout scene:

```json5
// POST /scene
{
    "hidePlayerModel": true,
    "origin": {
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
        // scene.origin is a special object,
        // located in the coordinates that we specified above
    },
}
```

```json5
// POST /objects/mainCamera/camera
{
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
