using System;
using System.Net.Sockets;

namespace DiffieHellmanClient
{
    /// <summary>
    /// Информация о полученном пакете.
    /// </summary>
    public struct PackageInfo
    {
        /// <summary>
        /// Содержание пакета.
        /// </summary>
        public dynamic Json;
        /// <summary>
        /// Источник пакета. Кто отправил?
        /// </summary>
        public ulong UserId;
        /// <summary>
        /// Время получения пакета.
        /// </summary>
        public DateTimeOffset Time;

        public PackageInfo(dynamic Json, ulong UserId, DateTimeOffset Time = default)
        {
            this.Json = Json;
            this.UserId = UserId;
            if (Time == default)
                Time = DateTimeOffset.Now;
            this.Time = Time;
        }
    }
}
