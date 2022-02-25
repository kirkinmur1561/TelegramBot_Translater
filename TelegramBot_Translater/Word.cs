using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Translater
{
    public class Word
    {
        public Word()
        {
        }

        public Word(string ru, string eng)
        {
            Ru = ru;
            Eng = eng;
        }

        /// <summary>
        /// Слово на русском языке
        /// </summary>
        public string Ru { get; set; }

        /// <summary>
        /// Слово на английском языке
        /// </summary>
        public string Eng { get; set; }
    }
}
