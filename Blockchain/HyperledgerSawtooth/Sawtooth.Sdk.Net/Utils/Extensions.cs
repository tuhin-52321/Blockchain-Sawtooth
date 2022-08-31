﻿
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using static Message.Types;

namespace Sawtooth.Sdk.Net.Utils
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Wrap the specified message, requestMessage and messageType.
        /// </summary>
        /// <returns>The wrap.</returns>
        /// <param name="message">Message.</param>
        /// <param name="requestMessage">Request message.</param>
        /// <param name="messageType">Message type.</param>
        public static Message Wrap(this IMessage message, Message requestMessage, MessageType messageType)
        {
            return new Message
            {
                MessageType = messageType,
                CorrelationId = requestMessage.CorrelationId,
                Content = message.ToByteString()
            };
        }

        /// <summary>
        /// Wrap the specified message and messageType.
        /// </summary>
        /// <returns>The wrap.</returns>
        /// <param name="message">Message.</param>
        /// <param name="messageType">Message type.</param>
        public static Message Wrap(this IMessage message, MessageType messageType)
        {
            return new Message
            {
                MessageType = messageType,
                CorrelationId = Guid.NewGuid().ToByteArray().ToSha256().ToHexString(),
                Content = message.ToByteString()
            };
        }

        /// <summary>
        /// Unwrap the specified message.
        /// </summary>
        /// <returns>The unwrap.</returns>
        /// <param name="message">Message.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Unwrap<T>(this Message message)
            where T : IMessage, new()
        {
            var request = new T();
            request.MergeFrom(message.Content);
            return request;
        }

        public static T ToProtobufClass<T>(this byte[] payload) where T : IMessage, new()
        {
            T protobuf = new T();
            protobuf.MergeFrom(payload);
            return protobuf;
        }

        public static T ToProtobufClass<T>(this ByteString payload) where T : IMessage, new()
        {
            T protobuf = new T();
            protobuf.MergeFrom(payload);
            return protobuf;
        }

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
