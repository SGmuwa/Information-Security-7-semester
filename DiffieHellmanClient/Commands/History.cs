using System;
using System.Collections.Generic;
using System.Text;

namespace DiffieHellmanClient.Commands
{
    internal class History<T>
    {
        private readonly List<T> list
            = new List<T>();

        public Result<T> Last
            => list.Count > 0
            ? new Result<T>(list[list.Count - 1])
            : new Result<T>(new IndexOutOfRangeException("list.Count = 0"));

        /// <summary>
        /// Индекс, показывающий на текущее положение.
        /// </summary>
        private int indexCurrent = -1;

        public void Add(T v)
        {
            list.Add(v);
            indexCurrent = list.Count;
            if (list.Count > 1000)
                RemovePast();
        }

        public void RemovePast()
        {
            if (list.Count > 0)
            {
                list.RemoveAt(0);
                indexCurrent--;
            }
        }

        public Result<T> Move(ConsoleKeyInfo info)
        {
            switch (info.Key)
            {
                case ConsoleKey.UpArrow:
                    return Move(HistoryMove.UP);
                case ConsoleKey.DownArrow:
                    return Move(HistoryMove.DOWN);
                default:
                    return new Result<T>(new NotSupportedException(info.Key.ToString()));
            }
        }

        public Result<T> Move(HistoryMove info)
        {
            if (list.Count < 1)
                return new Result<T>(new IndexOutOfRangeException("История пуста."));
            switch (info)
            {
                case HistoryMove.UP:
                    indexCurrent -= indexCurrent > 0 ? 1 : 0;
                    break;
                case HistoryMove.DOWN:
                    indexCurrent += indexCurrent < list.Count ? 1 : 0;
                    break;
            }
            if (indexCurrent < list.Count)
                return new Result<T>(list[indexCurrent]);
            else
                return new Result<T>(default(T));
        }
    }

    enum HistoryMove
    {
        UP,
        DOWN
    }
}