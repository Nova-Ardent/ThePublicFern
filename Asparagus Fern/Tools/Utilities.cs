#nullable enable

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ENGINE
using UnityEngine;
#endif

public static class Utilities
{
#if UNITY_ENGINE
    const string circularTransformShaderName = "CircularWorldMath";
    static ComputeShader? _circularTransformShader;
    public static ComputeShader? circularTransformShader 
    { 
        get {
            if (_circularTransformShader != null)
            {
                return _circularTransformShader;
            }

            _circularTransformShader = Resources.Load<ComputeShader>("CircularWorldMath");
            return _circularTransformShader;
        }
    }
#endif

    static System.Random _R = new System.Random();

    public static bool Any<T>(this IList<T> source)
    {
        foreach (var val in source)
        {
            return true;
        }

        return false;
    }

    public static bool Any<T>(this IList<T> source, Func<T, bool> condition, out int index)
    {
        for (int i = 0; i < source.Count; i++)
        {
            if (condition(source[i]))
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public static float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Math.Max(angle, 360 + from);
        return Math.Min(angle, to);
    }

    public static double Circumradius(float sideLength, float numSides)
    {
        return sideLength / (2 * Math.Sin(Math.PI / numSides));
    }

    public static int CalcLevenshteinDistance(string a, string b)
    {
        if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
        {
            return 0;
        }
        if (String.IsNullOrEmpty(a))
        {
            return b.Length;
        }
        if (String.IsNullOrEmpty(b))
        {
            return a.Length;
        }
        int lengthA = a.Length;
        int lengthB = b.Length;
        var distances = new int[lengthA + 1, lengthB + 1];
        for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
        for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

        for (int i = 1; i <= lengthA; i++)
            for (int j = 1; j <= lengthB; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                distances[i, j] = Math.Min
                    (
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost
                    );
            }
        return distances[lengthA, lengthB];
    }


    public static bool DegreeInMinRange(float angle, float endPoint1, float endPoint2)
    {
        angle = angle % 360;
        endPoint1 = endPoint1 % 360;
        endPoint2 = endPoint2 % 360;

        if (angle < 0) angle += 360;
        if (endPoint1 < 0) endPoint1 += 360;
        if (endPoint2 < 0) endPoint2 += 360;

        if (Math.Abs(endPoint2 - endPoint1) < 180)
        {
            return InRange(angle, endPoint1, endPoint2);
        }
        else
        {
            return !InRange(angle, endPoint1, endPoint2);
        }
    }

#if UNITY_ENGINE
    public static Vector2 DegreeToVector2(float degree)
    {
        return RadToVector2(degree * Mathf.Deg2Rad);
    }

    public static void Destroy(this IEnumerable<UnityEngine.GameObject> objects)
    {
        foreach (var obj in objects)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }

    public static void Destroy(this IEnumerable<UnityEngine.MonoBehaviour> objects)
    {
        foreach (var obj in objects)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }
#endif

    public static T? FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T? defaultValue) where T : class
    {
        foreach (var value in source)
        {
            if (predicate(value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var enumType = value.GetType();
        var name = Enum.GetName(enumType, value);
        return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
    }

#if UNITY_ENGINE
    public static List<Transform> GetChildrenTransformsRecursive(this Transform transform)
    {
        List<Transform> transforms = new List<Transform>();
        transform.GetChildrenTransformsRecursive(transforms);
        return transforms;
    }

    public static void GetChildrenTransformsRecursive(this Transform transform, List<Transform> childrenList)
    {
        childrenList.Add(transform);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChildrenTransformsRecursive(childrenList);
        }
    }
#endif

    public static IEnumerable<Enum> GetEnums(Type type)
    {
        foreach (var e in Enum.GetValues(type))
        {
            Enum? ret = e as Enum;
            if (ret == null)
                continue;

            yield return ret;
        }
    }

#if UNITY_ENGINE
    public static Vector2 GetMouseFromCenter()
    {
        var mousePos = Input.mousePosition;
        return new Vector2
            (mousePos.x - Screen.width / 2
            , mousePos.y - Screen.height / 2
            ); ;
    }
#endif

    public static int IndexOf<T>(this IList<T> source, Func<T, bool> condition)
    {
        for (int i = 0; i < source.Count; i++)
            if (condition(source[i]))
                return i;

        return -1;
    }

    public static bool InRange(float value, float e1, float e2)
    {
        if (e1 > e2)
        {
            return e1 > value && e2 < value;
        }
        return e2 > value && e1 < value;
    }

    public static double InRadius(float sideLength, float numSides)
    {
        return sideLength / (2 * Math.Tan(Math.PI / numSides));
    }

    public static bool IsSingleChar(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        char first = text[0];
        foreach (var chars in text)
        {
            if (first != chars)
            {
                return false;
            }
        }

        return true;
    }

    public static bool NoVowels(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        foreach (var chars in text.ToLower())
        {
            if ("aeiouAEIOU".IndexOf(chars) >= 0)
            {
                return false;
            }
        }

        return true;
    }

    public static bool StarsWithAny(this string text, List<string> words)
    {
        foreach (var word in words)
        {
            if (text.StartsWith(word))
            {
                return true;
            }
        }

        return false;
    }

#if UNITY_ENGINE
    public static Vector2 PolarToCartesian(float Radius, float Angle)
    {
        return new Vector2(Radius * Mathf.Cos(Angle), Radius * Mathf.Sin(Angle));
    }

    public static Vector2 RadToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
#endif

    public static IEnumerable<int> Repeat(this int n)
    {
        for (int i = 0; i < n; i++)
            yield return n;
    }

    public static T RandomEnum<T>() where T : Enum
    {
        var v = Enum.GetValues (typeof (T));
        return (T) v.GetValue (_R.Next(v.Length));
    }

    public static T RandomLocalizedEnum<T>() where T : Enum
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(_R.Next(v.Length));
    }

    public static bool SameAs<T>(this T[] array1, T[] array2)
    {
        if (array1.Length != array2.Length) return false;

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] == null && array2[i] == null)
            {
                continue;
            }
            if (array1[i] == null || array2[i] == null)
            {
                return false;
            }
            if (array1[i]?.Equals(array2[2]) == false)
            {
                return false;
            }
        }

        return true;
    }

    static void Swap<T>(ref T arg1, ref T arg2)
    {
        T temp = arg1;
        arg1 = arg2;
        arg2 = temp;
    }

#if UNITY_ENGINE
    public static void TransformMeshesToCircle(Transform parentTransform, float circumference, bool recalculateMeshData, int rotationalOffset = 0)
    {
        if (circularTransformShader == null)
        {
            return;
        }

        int totalSize = sizeof(float) * 3;

        Vector3 outValue = Utilities.TransformVertexToPolygon(circumference, parentTransform.position.x, parentTransform.position.y, parentTransform.position.z, rotationalOffset);
        Vector3 difference = outValue - parentTransform.position;
        parentTransform.position = outValue;

        List<Transform> childTransforms = parentTransform.GetChildrenTransformsRecursive();
        foreach (var childTransform in childTransforms)
        {
            if (childTransform.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
            {
                Mesh mesh = meshFilter.mesh;

                Vector3[] data = mesh.vertices;
                ComputeBuffer computeBuffer = new ComputeBuffer(mesh.vertexCount, totalSize);
                computeBuffer.SetData(data);

                circularTransformShader.SetBuffer(0, "vertex", computeBuffer);
                circularTransformShader.SetVector("objectOffset", difference);
                circularTransformShader.SetMatrix("localToWorld", childTransform.localToWorldMatrix);
                circularTransformShader.SetMatrix("worldToLocal", childTransform.worldToLocalMatrix);
                circularTransformShader.SetFloat("circumference", circumference);
                circularTransformShader.SetFloat("rotationalOffset", rotationalOffset);
                circularTransformShader.Dispatch(0, (int)Mathf.Ceil(mesh.vertexCount / 32.0f), 1, 1);

                computeBuffer.GetData(data);
                mesh.SetVertices(data);

                if (recalculateMeshData)
                {
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }

                computeBuffer.Dispose();
            }
        }
    }

    public static Vector3 TransformVertexToPolygon(float circumference, float x, float y, float z, float rotationalOffset = 0)
    {
        float r = circumference / (2 * Mathf.PI);
        Vector2 values = PolarToCartesian(y + r, -(x - rotationalOffset) / r);
        return new Vector3(values.x, values.y, z * ((circumference * Mathf.Sin(Mathf.PI / circumference)) / Mathf.PI));
    }
#endif
}
