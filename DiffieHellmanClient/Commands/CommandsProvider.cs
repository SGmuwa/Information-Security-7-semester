using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace DiffieHellmanClient.Commands
{
    class CommandsProvider : IEnumerable<AbstractCommand>
    {
        public readonly BusinessLogic mySystem;
        private readonly History<string> history
            = new History<string>();

        public CommandsProvider(BusinessLogic mySystem = null)
        {
            if (mySystem == null)
                mySystem = new BusinessLogic();
            this.mySystem = mySystem;
            commands = new ReadOnlyCollection<AbstractCommand>(new AbstractCommand[]
            {
                new Help(this),
                new Exit(this),
                new SetLocalPort(this),
                new SendAll(this),
                new AddConnection(this),
                new ReadAllMessages(this),
                new DebugMessages(this)
            });
        }

        public void Start()
        {
            IsNeedStop = false;
            Task.Run(mySystem.Run);
            while (!IsNeedStop)
            {
                while (toPrint.TryDequeue(out string result))
                    Console.WriteLine(result);
                history.Add(GetterText());
                try
                {
                    InvokeText(history.Last.Value);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"{e.Message}\nРекомендуется завершить работу программы.");
                }
            }
            mySystem.Dispose();
        }

        private readonly ConcurrentQueue<string> toPrint = new ConcurrentQueue<string>();

        public void Print(string msg) => toPrint.Enqueue(msg);

        private string GetterText()
        {
            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo info;
            var posStart = new { X = Console.CursorLeft, Y = Console.CursorTop };
            StringBuilder buffer = null;
            buffer = ShowUserCommandText(sb, posStart, buffer);
            do
            {
                info = Console.ReadKey(true);
                if (ConsoleKey.Backspace == info.Key)
                {
                    if (sb.Length > 0)
                        sb.Length--;
                }
                else if (ConsoleKey.Enter == info.Key)
                    break;
                else if (info.Key == ConsoleKey.UpArrow || info.Key == ConsoleKey.DownArrow || info.Key == ConsoleKey.LeftArrow || info.Key == ConsoleKey.RightArrow)
                    sb = new StringBuilder(history.Move(info).Value);
                else
                    sb.Append(info.KeyChar);
                buffer = ShowUserCommandText(sb, posStart, buffer);
            } while (info.Key != ConsoleKey.Enter);
            Console.SetCursorPosition(posStart.X, posStart.Y);
            Console.Write(buffer.ToString());
            if (GetCommandAndArgsFromText(sb.ToString(), out string commandName, out string[] args))
            {
                List<AbstractCommand> recommendCommands = SearchRecommendations.Search(commandName, (t) => t.Name, commands);
                if (recommendCommands.Count == 1)
                    return recommendCommands[0].Name + " " + string.Join(" ", args);
            }
            return sb.ToString();
        }

        private StringBuilder ShowUserCommandText(StringBuilder toInsert, dynamic posStart, StringBuilder stringOld)
        {
            string sbString = toInsert.ToString();
            StringBuilder output = new StringBuilder("> ");
            output.Append(sbString);
            output.AppendLine();
            if (GetCommandAndArgsFromText(sbString, out string commandName, out _))
            {
                output.Append(GetRecommendedInfo(commandName));
                output.AppendLine();
            }
            StringBuilder stringNew = output;
            if (stringOld != null)
            {
                for (int i = 0; i < stringOld.Length; i++)
                    if (stringOld[i] != '\n')
                        stringOld[i] = ' ';
                Console.SetCursorPosition(posStart.X, posStart.Y);
                Console.Write(stringOld.ToString());
            }
            Console.SetCursorPosition(posStart.X, posStart.Y);
            Console.Write(stringNew.ToString());
            Console.SetCursorPosition((posStart.X + sbString.Length + 2) % Console.BufferWidth, (posStart.X + sbString.Length + 2) / Console.BufferWidth + posStart.Y);
            return stringNew;
        }

        private string GetRecommendedInfo(string v)
        {
            if (GetCommandAndArgsFromText(v, out string nameCommand, out string[] args))
            {
                List<AbstractCommand> commands = SearchRecommendations.Search(nameCommand, (t) => t.Name, this.commands);
                if (commands.Count == 0)
                    return "not found.";
                else if (commands.Count == 1)
                    return commands[0].Name + " - " + commands[0].Info;
                else
                    return string.Join(", ", commands);
            }
            else return "(?)";
        }

        /// <summary>
        /// Преобразовывает входной текст в команду и вызывает эту команду.
        /// </summary>
        /// <param name="text">Текст, который надо преобразовать в команду.</param>
        private void InvokeText(string text)
        {
            if (GetCommandAndArgsFromText(text, out string nameCommand, out string[] args))
                InvokeCommands(nameCommand, args);
        }

        private bool GetCommandAndArgsFromText(string text, out string commandName, out string[] args)
        {
            commandName = null;
            args = null;
            if (text == null)
                return false;
            string[] textWithoutSpaces = GetNameAndArgs(text);
            if (textWithoutSpaces.Length < 1)
                return false;
            args = new string[textWithoutSpaces.Length - 1];
            new List<string>(textWithoutSpaces).CopyTo(1, args, 0, textWithoutSpaces.Length - 1);
            commandName = textWithoutSpaces[0].ToLower();
            return true;
        }

        private void InvokeCommands(string nameCommand, string[] args)
        {
            bool found = false;
            foreach (AbstractCommand command in this)
                found |= command.Invoke(nameCommand, args);
            if (!found)
                Console.WriteLine($"Команда \"{nameCommand}\" не найдена.");
        }

        private string[] GetNameAndArgs(string text)
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        public bool IsNeedStop { get; set; } = true;

        private readonly ReadOnlyCollection<AbstractCommand> commands;

        public IEnumerator<AbstractCommand> GetEnumerator()
            => commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => commands.GetEnumerator();
    }
}