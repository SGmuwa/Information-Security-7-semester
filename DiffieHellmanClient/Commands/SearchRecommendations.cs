using System;
using System.Collections.Generic;

namespace DiffieHellmanClient.Commands
{
    /// <summary>
    /// Класс предоставляет инструмент поиска рекомендаций из набора данных.
    /// </summary>
    static class SearchRecommendations
    {
        /// <summary>
        /// Поиск рекомендаций исправления varibleString на основе словаря array.
        /// </summary>
        /// <param name="varibleString">Слово, которое надо исправить.</param>
        /// <param name="array">Словарь правильных комбинаций.</param>
        /// <returns>Лист рекомендаций исправления.</returns>
        public static List<string> Search(string varibleString, IEnumerable<string> array)
            => Search(varibleString, (s) => s, array);

        /// <summary>
        /// Поиск рекомендаций исправления varibleString на основе словаря array.
        /// </summary>
        /// <param name="varibleString">Слово, которое надо исправить.</param>
        /// <param name="array">Словарь правильных комбинаций.</param>
        /// <returns>Лист рекомендаций исправления.</returns>
        public static List<string> Search(string varibleString, params string[] array)
            => Search(varibleString, (s) => s, (IEnumerable<string>)array);

        /// <summary>
        /// Поиск рекомендаций исправления varibleString на основе словаря array.
        /// </summary>
        /// <typeparam name="T">Тип данных, в которых производится поиск.</typeparam>
        /// <param name="varibleString">Слово, которое надо исправить.</param>
        /// <param name="GetString">Функция получения текста из типа T.</param>
        /// <param name="array">Словарь правильных комбинаций.</param>
        /// <returns>Лист рекомендаций исправления.</returns>
        public static List<T> Search<T>(string varibleString, Func<T, string> GetString, params T[] array)
            => Search(varibleString, GetString, (IEnumerable<T>)array);

        /// <summary>
        /// Поиск рекомендаций исправления varibleString на основе словаря array.
        /// </summary>
        /// <typeparam name="T">Тип данных, в которых производится поиск.</typeparam>
        /// <param name="varibleString">Слово, которое надо исправить.</param>
        /// <param name="GetString">Функция получения текста из типа T.</param>
        /// <param name="array">Словарь правильных комбинаций.</param>
        /// <returns>Лист рекомендаций исправления.</returns>
        public static List<T> Search<T>(string varibleString, Func<T, string> GetString, IEnumerable<T> array)
        {
            try
            {
                Dictionary<T, int> pairs = new Dictionary<T, int>();
                foreach (T command in array)
                {
                    pairs[command] = GetDistance(GetString(command), varibleString);
                }
                int minDistance = Minimum(pairs.Values);
                List<T> output = new List<T>();
                foreach (KeyValuePair<T, int> pair in pairs)
                {
                    if (pair.Value <= minDistance)
                        output.Add(pair.Key);
                }
                return output;
            }
            finally
            {
                depth = 0;
            }
        }

        private static int GetDistance(string ConstantText, string UserText)
        {
            if (ConstantText.Equals(UserText, StringComparison.InvariantCultureIgnoreCase))
                return 0;
            if (ConstantText.Contains(UserText, StringComparison.InvariantCultureIgnoreCase))
                return 1;
            else
                return GetLevenshteinDistance(ConstantText.ToLower(), UserText.ToLower()) + 2;
        }

        /// <summary>
        /// Служит для оптимизации.
        /// В случае, если глубина поиска будет более <see cref="MAX_DEPTH"/>,
        /// то поиск не будет заходить глубже.
        /// </summary>
        private static int depth = 0;

        /// <summary>
        /// Максимальная глубина поиска.
        /// </summary>
        private const int MAX_DEPTH = 6;

        private static int GetLevenshteinDistance(string first, string second)
        {
            if (++depth > MAX_DEPTH)
            {
                depth -= depth > 0 ? 1 : 0;
                return int.MaxValue / 2;
            }

            int cost;

            /* base case: empty strings */
            if (first.Length == 0) return second.Length;
            if (second.Length == 0) return first.Length;

            /* test if last characters of the strings match */
            if (first[first.Length - 1] == second[second.Length - 1])
                cost = 0;
            else
                cost = 1;

            /* return minimum of delete char from s, delete char from t, and delete char from both */
            int output = Minimum(GetDistance(first.Substring(0, first.Length - 1), second) + 1,
                           GetDistance(first, second.Substring(0, second.Length - 1)) + 1,
                           GetDistance(first.Substring(0, first.Length - 1), second.Substring(0, second.Length - 1)) + cost);
            depth--;
            return output;
        }

        private static int Minimum(IEnumerable<int> values)
        {
            int min = 0;
            bool first = true;
            foreach (int v in values)
            {
                if (first)
                {
                    min = v;
                    first = false;
                    continue;
                }
                if (v < min)
                    min = v;
            }
            return min;
        }

        private static int Minimum(params int[] values)
        {
            if (values.Length <= 0)
                throw new ArgumentException();
            int min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }
    }
}