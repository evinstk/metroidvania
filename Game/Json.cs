using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    static class Json
    {
        static Dictionary<string, object> _cache = new Dictionary<string, object>();

        public static T ReadJson<T>(string source)
        {
            var key = source.ToUpper();
            if (_cache.TryGetValue(key, out var cachedObject))
            {
                if (cachedObject is T)
                {
                    return (T)cachedObject;
                }
                throw new System.Exception("Cache key used with different type.");
            }

            using (var stream = TitleContainer.OpenStream(source))
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var json = new JsonSerializer().Deserialize<T>(jsonTextReader);
                _cache.Add(key, json);
                return json;
            }
        }
    }
}
