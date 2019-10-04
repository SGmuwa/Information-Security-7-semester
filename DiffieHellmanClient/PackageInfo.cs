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
        public TcpClient Source;
        /// <summary>
        /// Время получения пакета.
        /// </summary>
        public DateTimeOffset Time;

        public PackageInfo(dynamic Json, TcpClient Source, DateTimeOffset Time = default)
        {
            this.Json = Json;
            this.Source = Source;
            if (Time == default)
                Time = DateTimeOffset.Now;
            this.Time = Time;
        }
    }
}
