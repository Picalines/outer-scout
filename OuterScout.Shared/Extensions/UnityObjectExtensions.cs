﻿using System.Runtime.CompilerServices;

namespace OuterScout.Shared.Extensions;

public static class UnityObjectExtensions
{
    public static T? OrNull<T>(this T? unityObject)
        where T : UnityEngine.Object
    {
        return unityObject == null // Unity objects have an equality overload!
            ? null
            : unityObject;
    }

    public static T OrThrow<T>(
        this T? unityObject,
        [CallerArgumentExpression(nameof(unityObject))] string errorMessage = ""
    )
        where T : UnityEngine.Object
    {
        return unityObject == null
            ? throw new InvalidOperationException(errorMessage)
            : unityObject;
    }
}
