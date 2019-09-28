using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Caesar_s_code
{
    static class LettersSupportProvider
    {
        static LettersSupportProvider()
            => LettersSetSettings(false, false, false, false, true, false);

        public static void LettersSetSettings(bool numbers, bool english, bool englishBig, bool marks, bool russian, bool russianBig)
        {
            List<char> let = new List<char>();
            if (russian)
            {
                for (char c = 'а'; c <= 'я'; c++) let.Add(c);
                let.Add('ё');
            }
            if (russianBig)
            {
                for (char c = 'А'; c <= 'Я'; c++) let.Add(c);
                let.Add('Ё');
            }
            if(numbers)
                for (char c = '0'; c <= '9'; c++) let.Add(c);
            if(english)
                for (char c = 'a'; c <= 'z'; c++) let.Add(c);
            if(englishBig)
                for (char c = 'A'; c <= 'Z'; c++) let.Add(c);
            if (marks)
            {
                let.Add('.');
                let.Add(',');
                let.Add(';');
                let.Add('?');
                let.Add('!');
            }
            LettersSupport = new ReadOnlyCollection<char>(let);
        }

        public static ReadOnlyCollection<char> LettersSupport { get; private set; }
    }
}
