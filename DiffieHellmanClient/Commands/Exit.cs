namespace DiffieHellmanClient.Commands
{
    internal class Exit : AbstractCommand
    {
        private readonly CommandsProvider commandsProvider;

        public Exit(CommandsProvider commandsProvider) : base("exit")
            => this.commandsProvider = commandsProvider;

        public override string Info
            => "Завершение программы.";

        protected override void Action(string[] args)
        {
            commandsProvider.IsNeedStop = true;
        }
    }
}