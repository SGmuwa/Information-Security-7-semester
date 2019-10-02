using DiffieHellmanClient.Commands;

namespace DiffieHellmanClient
{
    public static class Program
    {
        public static void Main()
            => new CommandsProvider().Start();
    }
}
