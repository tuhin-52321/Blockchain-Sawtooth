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
        public static string ToHexString(this byte[] data) => Convert.ToBase64String(data);

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
    }
}
