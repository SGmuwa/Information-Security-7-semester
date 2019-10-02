using System;

namespace DiffieHellmanClient.Commands
{
    public struct Result<T>
    {
        /// <summary>
        /// Создать определённый результат.
        /// </summary>
        /// <param name="value">Значение результата.</param>
        public Result(T value)
        {
            Value = value;
            Problem = null;
        }

        /// <summary>
        /// Создать неопределённый результат.
        /// </summary>
        /// <param name="problem">Что послужило проблемой получения результата?</param>
        public Result(Exception problem)
        {
            Value = default;
            Problem = problem;
            if (typeof(T).Equals(typeof(Exception)))
                throw new NotSupportedException($"Класс {nameof(Result<T>)} не может хранить значения {nameof(Value)} типа {nameof(Exception)}.");
        }

        /// <summary>
        /// True, если результат найден. Иначе - False.
        /// </summary>
        public bool Ok => Problem == null;
        /// <summary>
        /// Проблема, связанная с поиском результата.
        /// </summary>
        public readonly Exception Problem;
        /// <summary>
        /// Значение результата.
        /// </summary>
        public readonly T Value;
    }
}