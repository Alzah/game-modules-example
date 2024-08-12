using Project.Extensions;
using System;
using System.Collections.Generic;

namespace Project.Systems.Configs
{
    [Serializable]
    public sealed class ConfigInfo
    {
        public string filename;
        public string url;
        public string etag;
        [NonSerialized]
        public string data;

        public bool IsLoad => !data.IsNullOrEmpty();

        public static ConfigInfo Parse(IDictionary<string, object> config)
        {
            var filenameVal = config["filename"].ToString();
            var urlVal = config["url"].ToString();
            var etagVal = config["etag"].ToString();

            return new ConfigInfo { filename = filenameVal, url = urlVal, etag = etagVal };
        }

        public override int GetHashCode()
        {
            return filename.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ConfigInfo info = obj as ConfigInfo;

            if (info == null)
            {
                return false;
            }
            var refEq = ReferenceEquals(this, obj);
            if (refEq) return true;

            return filename.Equals(info.filename); // && url.Equals(info.url) && etag.Equals(info.etag);
        }
    }
}
