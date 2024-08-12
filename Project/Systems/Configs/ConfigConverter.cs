using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Project.Systems.Configs
{
    public sealed class ConfigConverter : JsonConverter<ConfigInfo>
    {
        private bool _isFullConvert = false;

        public ConfigConverter(bool isFullConvert)
        {
            _isFullConvert = isFullConvert;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override ConfigInfo ReadJson(JsonReader reader, Type objectType, ConfigInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var info = new ConfigInfo();
            info.filename = jo[nameof(info.filename)].Value<string>();
            info.url = jo[nameof(info.url)].Value<string>();
            info.etag = jo[nameof(info.etag)].Value<string>();
            if (jo[nameof(info.data)] is not null && _isFullConvert)
            {
                info.data = jo[nameof(info.data)].Value<string>();
            }

            return info;
        }

        public override void WriteJson(JsonWriter writer, ConfigInfo info, JsonSerializer serializer)
        {
            if (info is null) return;

            JObject jo = new();

            jo[nameof(info.filename)] = info.filename;
            jo[nameof(info.url)] = info.url;
            jo[nameof(info.etag)] = info.etag;

            if (_isFullConvert)
            {
                jo[nameof(info.data)] = info.data;
            }

            jo.WriteTo(writer);
        }
    }
}
