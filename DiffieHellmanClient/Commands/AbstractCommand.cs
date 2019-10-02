using System;

namespace DiffieHellmanClient.Commands
{
    abstract class AbstractCommand
    {
        protected AbstractCommand(string name)
            => Name = name ?? throw new ArgumentNullException();

        /// <summary>
        /// Имя команды.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Запуск выполнения команды с проверкой соответсвия её имени.
        /// </summary>
        /// <param name="name">Имя команды.</param>
        /// <param name="args">Аргументы команды.</param>
        /// <returns>True, если входной текст соответсвует названию команды. Иначе - false.</returns>
        public bool Invoke(string name, string[] args)
        {
            if (!Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return false;
            Action(args);
            return true;
        }

        /// <summary>
        /// Запуск тела команды.
        /// </summary>
        /// <param name="args">Аргументы команды.</param>
        /// <exception cref="ArgumentNullException">args равен null.</exception>
        protected abstract void Action(string[] args);

        /// <summary>
        /// Получение информации о команде.
        /// </summary>
        public abstract string Info { get; }

        public override bool Equals(object obj)
        {
            return Name.ToLower().Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.ToLower().GetHashCode();
        }

        public override string ToString()
            => Name;
    }
}