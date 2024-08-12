using Project.Extensions;
using Project.Systems.Logging;
using Project.Systems.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Project.Systems.FeatureFlag
{
    public class FeatureFlagConverter : JsonConverter<FeatureFlagConfig>
    {
        public FeatureFlagConverter(Type flagType)
        {
            var actualFlags = flagType.GetEnumValues();

            _actualFlags = actualFlags.Cast<Enum>().Select(x => x.ToString().ToSnakeCase()).ToArray();
        }

        public string[] _actualFlags;

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override FeatureFlagConfig ReadJson(JsonReader reader, Type objectType, FeatureFlagConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var array = jo["features"].ToArray();
            var length = array.Length;
            var flags = new FeatureFlag[length];

            var k = 0;
            foreach (var item in array)
            {
                try
                {
                    var key = item["key"].Value<string>();
                    var version = item["version"].Value<string>();
                    var enable = item["enable"].Value<bool>();

                    if (!_actualFlags.Contains(key))
                    {
                        throw LogSystem.Exception(new Exception($"Unknown feature flag {key} in config"));
                    }

                    var span = new SpanVersion(version);
                    var flag = new FeatureFlag()
                    {
                        Key = key,
                        Version = span,
                        Enable = enable
                    };

                    flags[k++] = flag;
                }
                catch (Exception ex)
                {
                    LogSystem.Error(ex);
                }
            }
            Array.Resize(ref flags, k);

            return new FeatureFlagConfig() { Flags = flags };
        }

        public override void WriteJson(JsonWriter writer, FeatureFlagConfig value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
