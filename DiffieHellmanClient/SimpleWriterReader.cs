using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace DiffieHellmanClient
{
	class SimpleWriterReader : IDisposable
	{
		private P2PClient server;
		private readonly Dictionary<ulong, BlockingCollection<dynamic>> messages = new Dictionary<ulong, BlockingCollection<dynamic>>();

		public SimpleWriterReader(P2PClient server)
		{
			this.server = server;
			server.OnMessageSend += OnMessageSend;
			server.OnDisconnect += OnUserDisconnect;
		}

		private void OnMessageSend(P2PClient server, ulong userId, Memory<byte> Package)
			=> GetUserMessages(userId).Add(Package);

		/// <summary>
		/// Получение сообщения от пользователя.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, от которого мы ждём сообщение.</param>
		/// <param name="token">Билет отмены ожидания.</param>
		/// <returns>Прочитанный объект.</returns>
		public dynamic Read(ulong userId, CancellationToken token = default)
			=> GetUserMessages(userId).Take(token);

		/// <summary>
		/// Отправляет сообщение на сервер.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, которому надо отправить сообщение.</param>
		/// <param name="message">Сообщение, которое надо отправить пользователю.</param>
		public void Write(ulong userId, Memory<byte> message)
			=> server.Write(userId, message);

		/// <summary>
		/// Получает сообщения пользователя. Если до этого не было сообщений, то создаётся новый список.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, список которого необходимо получить.</param>
		/// <returns>Коллекция с доступом на добавление и чтения сообщений пользователя.</returns>
		private BlockingCollection<dynamic> GetUserMessages(ulong userId)
		{
			if(!messages.TryGetValue(userId, out BlockingCollection<dynamic> userMessages))
			{
				userMessages = new BlockingCollection<dynamic>();
				messages[userId] = userMessages;
			}
			return userMessages;
		}

		/// <summary>
		/// Происходит при отключении игрока от сервера. Идёт очистка сообщений.
		/// </summary>
		/// <param name="server">Сервер, откуда отключился пользователь.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		private void OnUserDisconnect(P2PClient server, ulong userId)
			=> messages.Remove(userId);

		/// <summary>
		/// Отвязывает этот экземпляр от сервера.
		/// </summary>
		public void Dispose()
		{
			server.OnMessageSend -= OnMessageSend;
			server.OnDisconnect -= OnUserDisconnect;
			server = null;
		}
	}
}