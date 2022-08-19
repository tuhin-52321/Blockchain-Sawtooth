using ProtoBuf;
using System.Security.Cryptography;
using System.Text;

namespace Sawtooth.Sdk.Net.Utils
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
 

        /// <summary>
        /// Converts a byte arrat to hex encoded string
        /// </summary>
        /// <returns>The hex string.</returns>
        /// <param name="data">Data.</param>
        public static string ToHexString(this byte[] data) => string.Concat(data.Select(x => x.ToString("x2"))); 

        /// <summary>
        /// Converts a byte arrat to base64 encoded string
        /// </summary>
        /// <returns>The hex string.</returns>
        /// <param name="data">Data.</param>
        public static string ToBase64String(this byte[] data) => Convert.ToBase64String(data);

        /// <summary>
        /// Converts a byte arrat to base64 encoded string
        /// </summary>
        /// <returns>The hex string.</returns>
        /// <param name="data">Data.</param>
        public static byte[] FromBase64String(this string data) => Convert.FromBase64String(data);

        /// <summary>
        /// Hashes the specified byte array using Sha256
        /// </summary>
        /// <returns>The sha256.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToSha256(this byte[] data) => SHA256.Create().ComputeHash(data);

        /// <summary>
        /// Hashes the specified byte array using Sha512
        /// </summary>
        /// <returns>The sha512.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToSha512(this byte[] data) => SHA512.Create().ComputeHash(data);

        /// <summary>
        /// Converts a string to byte array using UTF8 encoding
        /// </summary>
        /// <returns>The byte array.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToByteArray(this string data) => Encoding.UTF8.GetBytes(data);

        public static byte[] ToProtobufByteArray<T>(this T protobuf_obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, protobuf_obj);
                return stream.ToArray();
            }
        }

        public static string Last(this string data, int length)
        {
            if (data.Length > length)  return data.Substring(data.Length - length);
            return data;
        }
        public static string First(this string data, int length)
        {
            if(data.Length>length)  return data.Substring(0, length);
            return data;
        }

    }
}
