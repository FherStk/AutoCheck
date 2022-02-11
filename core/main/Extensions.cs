//Source: https://www.ryadel.com/en/how-to-perform-deep-copy-clone-object-asp-net-c-sharp/

using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;

/// <summary>
/// C# extension method for easy file downloading.
/// Source: https://stackoverflow.com/a/66270371
/// </summary>
public static class HttpClientUtils
{
    public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
    {
        using (var s = await client.GetStreamAsync(uri))
        {
            using (var fs = new FileStream(FileName, FileMode.CreateNew))
            {
                await s.CopyToAsync(fs);
            }
        }
    }
}

/// <summary>
/// C# extension method for fast object cloning.
/// Based upon the great net-object-deep-copy GitHub project by Alexey Burtsev, released under the MIT license.
/// 
/// https://github.com/Burtsev-Alexey/net-object-deep-copy
/// </summary>
public static partial class ObjectExtensions
{
    /// <summary>
    /// The Clone Method that will be recursively used for the deep clone.
    /// </summary>
    private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Returns TRUE if the type is a primitive one, FALSE otherwise.
    /// </summary>
    public static bool IsPrimitive(this Type type)
    {
        if (type == typeof(String)) return true;
        return (type.IsValueType & type.IsPrimitive);
    }

    /// <summary>
    /// Returns a Deep Clone / Deep Copy of an object using a recursive call to the CloneMethod specified above.
    /// </summary>
    public static Object DeepClone(this Object obj)
    {
        return DeepClone_Internal(obj, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
    }

    /// <summary>
    /// Returns a Deep Clone / Deep Copy of an object of type T using a recursive call to the CloneMethod specified above.
    /// </summary>
    public static T DeepClone<T>(this T obj)
    {
        return (T)DeepClone((Object)obj);
    }

    private static Object DeepClone_Internal(Object obj, IDictionary<Object, Object> visited)
    {
        if (obj == null) return null;
        var typeToReflect = obj.GetType();
        if (IsPrimitive(typeToReflect)) return obj;
        if (visited.ContainsKey(obj)) return visited[obj];
        if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
        var cloneObject = CloneMethod.Invoke(obj, null);
        if (typeToReflect.IsArray)
        {
            var arrayType = typeToReflect.GetElementType();
            if (IsPrimitive(arrayType) == false)
            {
                Array clonedArray = (Array)cloneObject;
                clonedArray.ForEach((array, indices) => array.SetValue(DeepClone_Internal(clonedArray.GetValue(indices), visited), indices));
            }

        }
        visited.Add(obj, cloneObject);
        CopyFields(obj, visited, cloneObject, typeToReflect);
        RecursiveCopyBaseTypePrivateFields(obj, visited, cloneObject, typeToReflect);
        return cloneObject;
    }

    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
    {
        if (typeToReflect.BaseType != null)
        {
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
    {
        foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
        {
            if (filter != null && filter(fieldInfo) == false) continue;
            if (IsPrimitive(fieldInfo.FieldType)) continue;

            //Concurrent Dictionaries produces infinite recursion
            if(originalObject.GetType().Name.Contains("ConcurrentDictionary")){  
                var args = originalObject.GetType().GetGenericArguments();
                var generic = typeof(ConcurrentDictionary<,>).MakeGenericType(args);
                var clonedDict = Activator.CreateInstance(generic);

                var property = (PropertyInfo)generic.GetMember("Keys")[0];
                var keys = property.GetValue(originalObject);

                //TODO: need the reflected array of keys (cannot be object)

                foreach(var key in keys){
                    // var originalValue = originalDict[key];
                    // var copiedValue = DeepClone_Internal(originalValue, visited);
                    var fake = 0;
                }
            }
            else{
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = DeepClone_Internal(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
    }

    public static void ForEach(this Array array, Action<Array, int[]> action)
    {
        if (array.LongLength == 0) return;
        ArrayTraverse walker = new ArrayTraverse(array);
        do action(array, walker.Position);
        while (walker.Step());
    }
}

internal class ReferenceEqualityComparer : EqualityComparer<Object>
{
    public override bool Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }
    public override int GetHashCode(object obj)
    {
        if (obj == null) return 0;
        return obj.GetHashCode();
    }
}

internal class ArrayTraverse
{
    public int[] Position;
    private int[] maxLengths;

    public ArrayTraverse(Array array)
    {
        maxLengths = new int[array.Rank];
        for (int i = 0; i < array.Rank; ++i)
        {
            maxLengths[i] = array.GetLength(i) - 1;
        }
        Position = new int[array.Rank];
    }

    public bool Step()
    {
        for (int i = 0; i < Position.Length; ++i)
        {
            if (Position[i] < maxLengths[i])
            {
                Position[i]++;
                for (int j = 0; j < i; j++)
                {
                    Position[j] = 0;
                }
                return true;
            }
        }
        return false;
    }
}