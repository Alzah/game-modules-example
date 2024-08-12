using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Project.Extensions
{
    public static class HashExtensions
    {
        private static MD5 s_md5 = MD5.Create();

        public static string Hash(this object data)
        {
            return BitConverter.ToString(data.HashByte());
        }

        private static byte[] HashByte(this object data)
        {
            return s_md5.ComputeHash(ObjectToByteArray(data));
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
