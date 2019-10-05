using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Caesar_s_code
{
    public static class LettersSupportProvider
    {
        public enum TypeLettersSupport
        {
            numbers = 1,
            english = numbers << 1,
            englishBig = english << 1,
            marks = englishBig << 1,
            russian = marks << 1,
            russianBig = russian << 1,
            get_all = numbers | english | englishBig | marks | russian | russianBig
        }


        static LettersSupportProvider()
            => LettersSetSettings(TypeLettersSupport.russian);

        public static void LettersSetSettings(TypeLettersSupport set)
        {
            List<char> let = new List<char>();
            if (set.HasFlag(TypeLettersSupport.russian))
            {
                for (char c = 'а'; c <= 'я'; c++) let.Add(c);
                let.Add('ё');
            }
            if (set.HasFlag(TypeLettersSupport.russianBig))
            {
                for (char c = 'А'; c <= 'Я'; c++) let.Add(c);
                let.Add('Ё');
            }
            if(set.HasFlag(TypeLettersSupport.numbers))
                for (char c = '0'; c <= '9'; c++) let.Add(c);
            if(set.HasFlag(TypeLettersSupport.english))
                for (char c = 'a'; c <= 'z'; c++) let.Add(c);
            if(set.HasFlag(TypeLettersSupport.englishBig))
                for (char c = 'A'; c <= 'Z'; c++) let.Add(c);
            if (set.HasFlag(TypeLettersSupport.marks))
            {
                let.Add('.');
                let.Add(',');
                let.Add(';');
                let.Add('?');
                let.Add('!');
                let.Add(':');
                let.Add('"');
                let.Add('\'');
                let.Add('\\');
                let.Add('/');
                let.Add('(');
                let.Add(')');
                let.Add('[');
                let.Add(']');
                let.Add('{');
                let.Add('}');
            }
            LettersSupport = new ReadOnlyCollection<char>(let);
        }

        public static ReadOnlyCollection<char> LettersSupport { get; private set; }
    }
}
