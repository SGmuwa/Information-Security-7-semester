using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Caesar_s_code
{
    internal static class StreamStringConvector
    {
        public static string ConvertString(this StreamReader input)
        {
            long oldPosition = input.BaseStream.Position;
            input.BaseStream.Position = 0;
            input.DiscardBufferedData();
            string output = input.ReadToEnd();
            input.BaseStream.Position = oldPosition;
            return output;
        }

        public static string ConvertStringAndClose(this StreamWriter input)
        {
            input.Flush();
            using (StreamReader sr = new StreamReader(input.BaseStream))
            {
                return sr.ConvertString();
            }
        }

        public static StreamReader ConvertToStream(this string input)
        {
            StreamWriter sw = new StreamWriter(new MemoryStream());
            sw.Write(input);
            sw.Flush();
            sw.BaseStream.Position = 0;
            return new StreamReader(sw.BaseStream);
        }
    }
}
