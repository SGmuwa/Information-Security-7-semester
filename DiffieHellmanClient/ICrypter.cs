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
        /// <param name="message">В случае false, message используется для создания безопасного подключения.</param>
        bool IsConnectionSafe(ulong client, Memory<byte> message);

        /// <summary>
        /// Создание безопасного подключчения.
        /// </summary>
        /// <param name="client">Соединение, которое надо защитить.</param>
        void AddUser(ulong client);
        /// <summary>
        /// Расшифровка сообщения.
        /// </summary>
        /// <param name="client">Соединение, от которого получено сообщение.</param>
        /// <param name="msg">Сообщение, которое надо расшифровать.</param>
        Memory<byte> Decrypt(ulong client, Memory<byte> msg);
        /// <summary>
        /// Шифрование сообщения.
        /// </summary>
        /// <param name="client">Соединение, к которому надо зашифровать сообщение.</param>
        /// <param name="msg">Сообщение, которое надо зашифровать.</param>
        Memory<byte> Encrypt(ulong client, Memory<byte> msg);
    }
}
