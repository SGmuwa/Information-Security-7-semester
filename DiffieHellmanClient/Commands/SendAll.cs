namespace DiffieHellmanClient.Commands
{
    class SendAll : AbstractCommand
    {
        private readonly CommandsProvider provider;

        public SendAll(CommandsProvider provider)
            : base(nameof(SendAll)) => this.provider = provider;
        public override string Info =>
            "Отправляет всем сообщение.";

        protected override void Action(string[] args)
        {
            provider.mySystem.SendAll(string.Join(' ', args));
        }
    }
}
