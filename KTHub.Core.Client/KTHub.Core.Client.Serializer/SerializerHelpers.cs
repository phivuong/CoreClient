using System;
using System.Collections.Generic;
using System.IO;
using KTHub.Core.Logging;
using KTHub.Core.Security;

namespace KTHub.Core.Client.Serializer
{
    public sealed class SerializerHelpers
    {
        public static object LockSerialize = new object();
        public static object LockDeserialize = new object();
        public static object LockListDeserialize = new object();
        public static object LockRedisSerialize = new object();
        public static object LockRedisDeserialize = new object();

        public static byte[] Serialize<T>(T obj)
        {
            if ((object)obj == null)
            {
                return (byte[])null;
            }
            lock (SerializerHelpers.LockSerialize)
            {
                try
                {
                    byte[] array;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new Wire.Serializer().Serialize((object)obj, (Stream)memoryStream);
                        array = memoryStream.ToArray();
                    }
                    return array;
                }
                catch (Exception ex)
                {
                    InvokeLogging.WriteLog(ex);
                    return (byte[])null;
                }
            }
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            if (!bytes.IsNotNullOrEmpty())
            {
                return default(T);
            }
            lock (SerializerHelpers.LockDeserialize)
            {
                try
                {
                    T obj;
                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                        obj = new Wire.Serializer().Deserialize<T>((Stream)memoryStream);
                    return obj;
                }
                catch (Exception ex)
                {
                    InvokeLogging.WriteLog(ex);
                    return default(T);
                }
            }
        }

        public static byte[] Serialize<T>(T obj, string saltValue)
        {
            if ((object)obj == null)
            {
                return (byte[])null;
            }
            lock (SerializerHelpers.LockRedisSerialize)
            {
                try
                {
                    byte[] plainTextBytes = SerializerHelpers.Serialize<T>(obj);
                    return saltValue.IsNotNullOrEmpty() ? KTHubCrytography.Encrypt(plainTextBytes, saltValue) : plainTextBytes;
                }
                catch (Exception ex)
                {
                    InvokeLogging.WriteLog(ex);
                    return (byte[])null;
                }
            }
        }

        public static T Deserialize<T>(byte[] byteArray, string saltValue)
        {
            if (!byteArray.IsNotNullOrEmpty())
            {
                return default(T);
            }
            lock (SerializerHelpers.LockRedisDeserialize)
            {
                try
                {
                    if (saltValue.IsNotNullOrEmpty())
                        byteArray = KTHubCrytography.Decrypt(byteArray, saltValue);
                    return SerializerHelpers.Deserialize<T>(byteArray);
                }
                catch (Exception ex)
                {
                    InvokeLogging.WriteLog(ex);
                    return default(T);
                }
            }
        }

        public static T Deserialize<T>(byte[] byteArray, string saltValue, ref bool status)
        {
            if (!byteArray.IsNotNullOrEmpty())
            {
                return default(T);
            }
            lock (SerializerHelpers.LockRedisDeserialize)
            {
                try
                {
                    if (saltValue.IsNotNullOrEmpty())
                        byteArray = KTHubCrytography.Decrypt(byteArray, saltValue);
                    status = true;
                    return SerializerHelpers.Deserialize<T>(byteArray);
                }
                catch (Exception ex)
                {
                    InvokeLogging.WriteLog(ex);
                    return default(T);
                }
            }
        }

        public static List<T> ByteArrayToListObject<T>(byte[][] byteDatas)
        {
            lock (SerializerHelpers.LockListDeserialize)
            {
                if (byteDatas == null)
                {
                    return new List<T>();
                }
                Wire.Serializer serializer = new Wire.Serializer();
                List<T> objList = new List<T>();
                foreach (byte[] byteData in byteDatas)
                {
                    using (MemoryStream memoryStream = new MemoryStream(byteData))
                    {
                        objList.Add(serializer.Deserialize<T>((Stream)memoryStream));
                    }
                }
                return objList;
            }
        }

        public static string SerializeToString(object objItem)
        {
            if (objItem == null)
            {
                return string.Empty;
            }
            byte[] numArray = SerializerHelpers.Serialize<object>(objItem);
            return numArray.IsNotNullOrEmpty() ? Convert.ToBase64String(numArray) : string.Empty;
        }

        public static T DeserializeFromString<T>(string base64Str)
        {
            switch (base64Str)
            {
                case "":
                    return default(T);
                case null:
                    return default(T);
                default:
                    return SerializerHelpers.Deserialize<T>(Convert.FromBase64String(base64Str));
            }
        }
    }
}
