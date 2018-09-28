using System;
using Newtonsoft.Json;

namespace UW.Shared
{
    /// <summary>
    /// Object Extensions
    /// </summary>
    public static class ObjExtensions
    {
        public static string ToJson(this object context)
        {
            return JsonConvert.SerializeObject(context, Formatting.Indented);
        }

        public static T DeepClone<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
                return default(T);

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }
}