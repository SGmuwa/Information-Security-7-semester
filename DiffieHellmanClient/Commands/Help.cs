using System;

namespace DiffieHellmanClient.Commands
{
    class Help : AbstractCommand
    {
        public Help(CommandsProvider provider)
            : base("help") => Provider = provider ?? throw new ArgumentNullException();

        private readonly CommandsProvider Provider;

        public override string Info
            => "Выводит справочную информацию о всех командах.";

        protected override void Action(string[] args)
        {
            foreach (AbstractCommand command in Provider)
            {
                ConsoleColor fore = Console.ForegroundColor;
                ConsoleColor back = Console.BackgroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(command.Name);
                Console.ForegroundColor = fore;
                Console.BackgroundColor = back;
                Console.WriteLine(" - " + command.Info);
            }
        }
    }
}