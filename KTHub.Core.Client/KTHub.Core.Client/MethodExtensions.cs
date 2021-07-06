using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KTHub.Core.Client
{
    internal static class MethodExtensions
    {
        public static string GetCallerMemberName([CallerMemberName] string name = "") => name;

        [DebuggerStepThrough]
        public static bool IsNotNullOrEmpty(this string value) => !value.IsNullOrEmpty();

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        [DebuggerStepThrough]
        public static bool IsGenericList(this Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
                return false;
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            return genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(IList<>);
        }

        [DebuggerStepThrough]
        public static bool IsDictionary(this Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        public static bool IsSystemType(this object obj) => obj.GetType().Namespace == "System" || obj.GetType().Namespace.StartsWith("System");

        public static byte[] GetBuffer(this MemoryStream inputStream)
        {
            try
            {
                ArraySegment<byte> buffer;
                inputStream.TryGetBuffer(out buffer);
                return buffer.Array;
            }
            catch (Exception ex)
            {
            }
            return (byte[])null;
        }
    }
}
