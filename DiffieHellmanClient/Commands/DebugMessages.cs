using System;

namespace DiffieHellmanClient.Commands
{
    internal class DebugMessages : AbstractCommand
    {
        private CommandsProvider provider;
        bool isOn = false;

        public DebugMessages(CommandsProvider commandsProvider)
            : base(nameof(DebugMessages))
        {
            provider = commandsProvider;
        }

        public override string Info => 
            "Включает вывод сообщений P2PClient.";

        protected override void Action(string[] args)
        {
            isOn =! isOn;
            if (isOn)
            {
                provider.mySystem.OnDebugMessage += OnDebugMessage;
                Console.WriteLine("Debug включён.");
            }
            else
            {
                provider.mySystem.OnDebugMessage -= OnDebugMessage;
                Console.WriteLine("Debug выключен.");
            }
        }

        private void OnDebugMessage(Businesslogic sender, string arg)
        {
            provider.Print(arg);
        }
    }
}