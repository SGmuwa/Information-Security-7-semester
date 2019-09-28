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
    }
}
