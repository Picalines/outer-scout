using System;
using System.Diagnostics;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class FFMPEGVideoRenderer : IDisposable
{
    public int Framerate { get; }

    public string OutFilePath { get; }

    public Texture SourceTexture { get; }

    public int FramesRendered { get; private set; } = 0;

    private static byte[] _ManagedFrameBuffer = Array.Empty<byte>();

    private NativeArray<byte> _NativeFrameBuffer;

    private readonly Process _FFMPEGProcess;

    private bool _IsDisposed = false;

    public FFMPEGVideoRenderer(Texture sourceTexture, int framerate, string outFilePath)
    {
        SourceTexture = sourceTexture;
        Framerate = framerate;
        OutFilePath = outFilePath;

        _NativeFrameBuffer = new NativeArray<byte>(
            SourceTexture.width * SourceTexture.height * 4,
            Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        _FFMPEGProcess = new Process()
        {
            StartInfo = new()
            {
                FileName = "ffmpeg",
                WorkingDirectory = Path.GetDirectoryName(OutFilePath),
                Arguments = RenderFFMPEGArguments(),
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
                UseShellExecute = false,
            },
        };

        _FFMPEGProcess.Start();
    }

    public void RenderFrameAsync()
    {
        AsyncGPUReadback.RequestIntoNativeArray(ref _NativeFrameBuffer, SourceTexture, 0, OnReadbackComplete);
    }

    private void OnReadbackComplete(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            UnityEngine.Debug.LogError("GPU readback error");
            return;
        }

        using var pngBytes = ImageConversion.EncodeNativeArrayToPNG(
            _NativeFrameBuffer,
            SourceTexture.graphicsFormat,
            (uint)SourceTexture.width,
            (uint)SourceTexture.height);

        lock (_ManagedFrameBuffer)
        {
            if (pngBytes.Length > _ManagedFrameBuffer.Length)
            {
                const double resizeFactor = 1.75;
                Array.Resize(ref _ManagedFrameBuffer, (int)(pngBytes.Length * resizeFactor));
            }

            CopyToUnsafe(pngBytes, _ManagedFrameBuffer);

            _FFMPEGProcess.StandardInput.BaseStream.Write(_ManagedFrameBuffer, 0, pngBytes.Length);
        }

        FramesRendered++;

        if (FramesRendered % 10 == 0)
        {
            _FFMPEGProcess.StandardInput.BaseStream.Flush();
        }
    }

    public void Dispose()
    {
        if (_IsDisposed)
        {
            return;
        }

        AsyncGPUReadback.WaitAllRequests();

        _FFMPEGProcess.StandardInput.BaseStream.Flush();
        _FFMPEGProcess.StandardInput.BaseStream.Close();

        _FFMPEGProcess.WaitForExit();

        _IsDisposed = true;
    }

    private string RenderFFMPEGArguments()
    {
        return $"-y -framerate {Framerate} -f image2pipe -i - -r {Framerate} -c:v: libx264 -movflags +faststart -pix_fmt yuv420p -crf 19 {OutFilePath}";
    }

    private static unsafe void CopyToUnsafe<T>(in NativeArray<T> nativeArray, T[] array)
        where T : struct
    {
        if (array.Length < nativeArray.Length)
        {
            throw new IndexOutOfRangeException($"{nameof(array)} ({array.Length}) is shorter than {nameof(nativeArray)} ({nativeArray.Length})");
        }

        int byteLength = nativeArray.Length * UnsafeUtility.SizeOf<T>();
        void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
        void* nativeBuffer = nativeArray.GetUnsafePtr();
        UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
    }
}
