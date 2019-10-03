using System;
using System.Net.Sockets;

namespace DiffieHellmanClient
{
    /// <summary>
    /// Интерфейс модуля шифрования сообщений.
    /// </summary>
    public interface ICrypter
    {
        /// <summary>
        /// True, если соединение защищено. Иначе - false.
        /// </summary>
        /// <param name="client">Соединение, которое надо проверить.</param>
        bool IsConnectionSafe(TcpClient client);

        /// <summary>
        /// Создание безопасного подключчения.
        /// </summary>
        /// <param name="client">Соединение, которое надо защитить.</param>
        void AddUser(TcpClient client);
        /// <summary>
        /// Расшифровка сообщения.
        /// </summary>
        /// <param name="client">Соединение, от которого получено сообщение.</param>
        /// <param name="msg">Сообщение, которое надо расшифровать.</param>
        Memory<byte> Decrypt(TcpClient client, Memory<byte> msg);
        /// <summary>
        /// Шифрование сообщения.
        /// </summary>
        /// <param name="client">Соединение, к которому надо зашифровать сообщение.</param>
        /// <param name="msg">Сообщение, которое надо зашифровать.</param>
        Memory<byte> Encrypt(TcpClient client, Memory<byte> msg);
    }
}
