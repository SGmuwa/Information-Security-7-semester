using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System;

namespace DiffieHellmanClient
{
	class SimpleWriterReader : IDisposable
	{
		private P2PClient server;
		/// <summary>
		/// Предел ожидания получения пакета. По умолчанию 10 секунд.
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
		private readonly ConcurrentDictionary<ulong, BlockingCollection<dynamic>> messages = new ConcurrentDictionary<ulong, BlockingCollection<dynamic>>();

		/// <summary>
		/// Создаёт новый экземпляр клиента получателя и отправителя сообщений.
		/// </summary>
		/// <param name="server">Сервер, с помощью которого отправляются и получаются пакеты.</param>
		/// <param name="timeout">Максимальный интервал ожидания для получения пакета.</param>
		public SimpleWriterReader(P2PClient server, TimeSpan timeout = default)
		{
			server.DebugInfo($"{this}.SimpleWriterReader = {server}, {timeout}");
			if(timeout != default)
				Timeout = timeout;
			this.server = server;
			server.OnMessageSend += OnMessageSend;
			server.OnDisconnect += OnUserDisconnect;
		}

		private void OnMessageSend(P2PClient server, ulong userId, Memory<byte> Package)
		{
			server.DebugInfo($"{this}.OnMessageSend = {server}, {userId}, {string.Join(", ", Package)}");
			GetUserMessages(userId).Add(Package);
		}

		/// <summary>
		/// Получение сообщения от пользователя. Если от пользователя нет сообщений, то ждёт <see cref="Timeout"/>.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, от которого мы ждём сообщение.</param>
		/// <param name="token">Билет отмены ожидания.</param>
		/// <returns>Прочитанный объект.</returns>
		/// <exception cref="System.OperationCanceledException">Происходит, когда превышено время <see cref="Timeout"/>.</exception>
		public dynamic Read(ulong userId, CancellationToken token = default)
		{
			server.DebugInfo($"{this}.Read = {userId}, {token}");
			if(token == default)
				token = new CancellationTokenSource(Timeout).Token;
			return GetUserMessages(userId).Take(token);
		}

		/// <summary>
		/// Отправляет сообщение на сервер.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, которому надо отправить сообщение.</param>
		/// <param name="message">Сообщение, которое надо отправить пользователю.</param>
		public void Write(ulong userId, Memory<byte> message)
		{
			server.DebugInfo($"{this}.Write = {userId}, {string.Join(", ", message)}");
			server.Write(userId, message);
		}

		/// <summary>
		/// Получает сообщения пользователя. Если до этого не было сообщений, то создаётся новый список.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя, список которого необходимо получить.</param>
		/// <returns>Коллекция с доступом на добавление и чтения сообщений пользователя.</returns>
		private BlockingCollection<dynamic> GetUserMessages(ulong userId)
		{
			server.DebugInfo($"{this}.GetUserMessages = {userId} begin");
			if(!messages.TryGetValue(userId, out BlockingCollection<dynamic> userMessages))
			{
				userMessages = new BlockingCollection<dynamic>();
				messages[userId] = userMessages;
			}
			server.DebugInfo($"{this}.GetUserMessages = {userId} end");
			return userMessages;
		}

		/// <summary>
		/// Происходит при отключении игрока от сервера. Идёт очистка сообщений.
		/// </summary>
		/// <param name="server">Сервер, откуда отключился пользователь.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		private void OnUserDisconnect(P2PClient server, ulong userId)
			=> messages.TryRemove(userId, out _);

		/// <summary>
		/// Отвязывает этот экземпляр от сервера.
		/// </summary>
		public void Dispose()
		{
			server.OnMessageSend -= OnMessageSend;
			server.OnDisconnect -= OnUserDisconnect;
			server = null;
		}

		public override string ToString()
			=> $"{nameof(SimpleWriterReader)} {{server = {server}, Timeout = {Timeout}, messages = [{{{string.Join("}, {", from a in messages from b in a.Value select $"{a.Key}: {b}")}}}]}}";
	}
}