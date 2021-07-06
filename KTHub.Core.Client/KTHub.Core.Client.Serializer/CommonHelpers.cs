using System;
namespace KTHub.Core.Client.Serializer
{
    internal static class CommonHelpers
    {
        public static bool IsNotNullOrEmpty(this Array array) => array != null && array.Length > 0;

        public static bool IsNotNullOrEmpty(this string input) => !string.IsNullOrEmpty(input);
    }
}
