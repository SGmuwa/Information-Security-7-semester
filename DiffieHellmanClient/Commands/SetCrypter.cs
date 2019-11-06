using System;
using System.Net;

namespace DiffieHellmanClient.Commands
{
    internal class SetCrypter : AbstractCommand
    {
		private readonly CommandsProvider provider;

        public SetCrypter(CommandsProvider provider)
            : base(nameof(SetCrypter))
        {
            this.provider = provider;
        }

        public override string Info => 
            "Устанавливает протокол шифрования.\n" +
            "Синтаксис: SetCrypter [RSA|DH] CountBitsKey [isNeedDisconnect]\n" +
			"RSA - Rivest, Shamir и Adleman.\n"+
			"DH - Diffie-Hellman (протокол Ди́ффи — Хе́ллмана)\n"+
			"CountBitsKey - размер ключа в битах. Рекомендуется 2048...4096\n"+
			"isNeedDisconnect: true, если необходим разрыв всех соединений. False, если сохранить соединения. По-умолчанию true.";

        protected override void Action(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine($"Неправильный синтаксис.");
                return;
            }
			if(!ushort.TryParse(args[1], out ushort countBits) || countBits < 8)
				Console.WriteLine("Неудалось распознать число.");
			bool isNeedDisconnect = true;
			if(args.Length > 2)
				if(!bool.TryParse(args[2], out isNeedDisconnect))
					isNeedDisconnect = true;
            switch(args[0].ToUpper())
			{
				case "RSA":
					provider.mySystem.SetCrypter(server => new RSA(server, countBits), isNeedDisconnect);
					break;
				case "DH":
					provider.mySystem.SetCrypter(server => new DiffieHellmanCrypter(server, countBits), isNeedDisconnect);
					break;
				default:
					Console.WriteLine("Данный алгоритм не поддерживается.");
					break;
			}
			
        }
    }
}