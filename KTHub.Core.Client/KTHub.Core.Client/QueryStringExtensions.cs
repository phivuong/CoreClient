using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using KTHub.Core.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace KTHub.Core.Client
{
    public static class QueryStringExtensions
    {
        public static T ToObject<T>(this IQueryCollection queryString) where T : new()
        {
            T obj1 = new T();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                StringValues stringValues;
                if (queryString.TryGetValue(property.Name, out stringValues))
                {
                    Type propertyType = property.PropertyType;
                    if (!propertyType.IsDictionary() && !propertyType.IsArray && !propertyType.IsGenericList())
                    {
                        object obj2 = !stringValues.ToString().IsNotNullOrEmpty() || stringValues.Count <= 0 ? (object)null : stringValues.ToString().ParseToObject(property.PropertyType);
                        if (obj2 != null)
                        {
                            property.SetValue((object)obj1, obj2, (object[])null);
                        }
                    }
                }
            }
            return obj1;
        }

        public static T QueryToObject<T>(this string strQuery) where T : new()
        {
            if (strQuery.StartsWith("value="))
            {
                string valueConvertToObject = strQuery.Replace("value=", "");
                try
                {
                    return valueConvertToObject.ParseToObject<T>();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(strQuery);
                T obj1 = new T();
                foreach (PropertyInfo property in typeof(T).GetProperties())
                {
                    StringValues stringValues;
                    if (query.TryGetValue(property.Name, out stringValues))
                    {
                        Type propertyType = property.PropertyType;
                        if (!propertyType.IsDictionary() && !propertyType.IsArray && !propertyType.IsGenericList())
                        {
                            object obj2 = !stringValues.ToString().IsNotNullOrEmpty() || stringValues.Count <= 0 ? (object)null : stringValues.ToString().ParseToObject(property.PropertyType);
                            if (obj2 != null)
                            {
                                property.SetValue((object)obj1, obj2, (object[])null);
                            }
                        }
                    }
                }
                return obj1;
            }
        }

        public static string ToQueryString(this object request, string separator = ",")
        {
            if (request.IsSystemType())
                return "value=" + request;
            if (request == null)
                return string.Empty;
            Dictionary<string, object> dictionary = ((IEnumerable<PropertyInfo>)request.GetType().GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>)(x => x.CanRead)).Where<PropertyInfo>((Func<PropertyInfo, bool>)(x => x.GetValue(request, (object[])null) != null)).ToDictionary<PropertyInfo, string, object>((Func<PropertyInfo, string>)(x => x.Name), (Func<PropertyInfo, object>)(x => !x.PropertyType.Equals(typeof(DateTime)) && !x.PropertyType.Equals(typeof(DateTime?)) ? x.GetValue(request, (object[])null) : (object)((DateTime)x.GetValue(request, (object[])null)).ToServerDatetime()));
            foreach (string key in dictionary.Where<KeyValuePair<string, object>>((Func<KeyValuePair<string, object>, bool>)(x => !(x.Value is string) && x.Value is IEnumerable)).Select<KeyValuePair<string, object>, string>((Func<KeyValuePair<string, object>, string>)(x => x.Key)).ToList<string>())
            {
                Type type1 = dictionary[key].GetType();
                Type type2 = type1.IsGenericType ? type1.GetGenericArguments()[0] : type1.GetElementType();
                if (type2.IsPrimitive || type2 == typeof(string))
                {
                    IEnumerable source = dictionary[key] as IEnumerable;
                    dictionary[key] = (object)string.Join<object>(separator, source.Cast<object>());
                }
            }
            return string.Join("&", dictionary.Select<KeyValuePair<string, object>, string>((Func<KeyValuePair<string, object>, string>)(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value.ToString()))));
        }

        private static object ParseToObject(this string valueConvertToObject, Type dataType) => TypeDescriptor.GetConverter(dataType).ConvertFromString((ITypeDescriptorContext)null, CultureInfo.InvariantCulture, valueConvertToObject);

        private static T ParseToObject<T>(this string valueConvertToObject) => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString((ITypeDescriptorContext)null, CultureInfo.InvariantCulture, valueConvertToObject);

        public static string GetQueryValue(this string strQuery, string fieldName)
        {
            StringValues stringValues;
            QueryHelpers.ParseQuery(strQuery).TryGetValue(fieldName, out stringValues);
            return stringValues.ToString();
        }
    }
}
