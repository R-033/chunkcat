using System;
using System.IO;
using System.Linq;
using System.Text;

namespace chunkcat
{
    class Program
    {
        public static ConsoleColor NormalBG = ConsoleColor.Black;
        public static ConsoleColor NormalFG = ConsoleColor.White;
        public static ConsoleColor OutOfBoundsBG = ConsoleColor.DarkRed;
        public static ConsoleColor ChunkSizeColor = ConsoleColor.Green;
        public static ConsoleColor IntColor = ConsoleColor.DarkCyan;
        public static ConsoleColor FloatColor = NormalFG;
        public static ConsoleColor StringColor = ConsoleColor.Blue;
        public static ConsoleColor ZeroColor = ConsoleColor.DarkGray;

        static bool IfString(byte[] arr, int offset) {
            for (int i = offset; i < offset + 4; i++) {
                if (i != offset && arr[i] == 0x00)
                    continue;
                if (arr[i] >= 0x30 && arr[i] <= 0x39)
                    continue;
                if (arr[i] >= 0x41 && arr[i] <= 0x5A)
                    continue;
                if (arr[i] == 0x5F)
                    continue;
                if (arr[i] >= 0x61 && arr[i] <= 0x7A)
                    continue;
                return false;
            }
            return true;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Please specify file, chunk, line width, content width and chunk entry size");
                return;
            }
            byte[] content = File.ReadAllBytes(args[0]);
            byte[] chunk = Enumerable.Range(0, args[1].Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(args[1].Substring(x, 2), 16))
                        .ToArray();
            for (int initial_pos = 0; initial_pos < content.Length; initial_pos += 4) {
                int pos = 0;
                for (int i = initial_pos; i < content.Length; i += 4) {
                    if (content[i] == chunk[0] && content[i + 1] == chunk[1] && content[i + 2] == chunk[2] && content[i + 3] == chunk[3]) {
                        pos = i;
                        break;
                    }
                }
                int linewidth = int.Parse(args[2]);
                if (linewidth % 4 != 0)
                    throw new Exception("can't be aligned properly with line width " + linewidth);
                int chunksize = BitConverter.ToInt32(content, pos + 4) + 4;
                initial_pos = pos + chunksize;
                Console.WriteLine();
                Console.WriteLine("offset: " + pos);
                int lineamount = (int)Math.Ceiling((float)chunksize / ((float)linewidth * 4f) + 1f);
                int content_width = int.Parse(args[3]);
                int startpos = (int)(pos / (linewidth * 4)) * (linewidth * 4);
                Console.BackgroundColor = NormalBG;
                Console.ForegroundColor = NormalFG;
                int truepos;
                int counter = 0;
                int counter2 = 0;
                int maxcounter = int.Parse(args[4]);
                bool trig = false;
                for (int line = 0; line < lineamount; line++) {
                    for (int charnum = 0; charnum < linewidth; charnum++) {
                        truepos = startpos + line * linewidth * 4 + charnum * 4;
                        if (truepos < pos || truepos > pos + chunksize) {
                            Console.BackgroundColor = OutOfBoundsBG;
                        }
                        if (truepos < pos || content[truepos] == 0x11 && content[truepos + 1] == 0x11 && content[truepos + 2] == 0x11 && content[truepos + 3] == 0x11)
                            counter = -1;
                        trig = false;
                        if (pos >= truepos && pos <= truepos + 3) {
                            Console.BackgroundColor = NormalFG;
                            Console.ForegroundColor = NormalBG;
                            Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                            Console.BackgroundColor = NormalBG;
                            Console.ForegroundColor = NormalFG;
                        } else if (pos + 4 >= truepos && pos + 4 <= truepos + 3) {
                            Console.ForegroundColor = ChunkSizeColor;
                            Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                        } else if (IfString(content, truepos)) {
                            Console.ForegroundColor = StringColor;
                            Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                            counter += 1;
                            if (counter >= maxcounter) {
                                counter = 0;
                                if (truepos < pos + chunksize)
                                trig = true;
                            }
                        } else {
                            string val = BitConverter.ToSingle(content, truepos).ToString();
                            if (val.Contains("e") || val.Contains("E") || val == "NaN") {
                                Console.ForegroundColor = IntColor;
                                Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                            } else if (val == "0") {
                                Console.ForegroundColor = ZeroColor;
                                Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                            } else {
                                Console.ForegroundColor = FloatColor;
                                Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", ""));
                            }
                            counter += 1;
                            if (counter >= maxcounter) {
                                counter = 0;
                                if (truepos < pos + chunksize)
                                trig = true;
                            }
                        }
                        Console.BackgroundColor = NormalBG;
                        Console.ForegroundColor = NormalFG;
                        if (charnum < linewidth - 1)
                            Console.Write(trig ? "|" : " ");
                        else if (trig)
                            Console.Write("|");
                    }
                    Console.Write(trig ? "   " : "    ");
                    for (int charnum = 0; charnum < linewidth; charnum++) {
                        truepos = startpos + line * linewidth * 4 + charnum * 4;
                        if (truepos < pos || truepos > pos + chunksize) {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                        }
                        if (truepos < pos || content[truepos] == 0x11 && content[truepos + 1] == 0x11 && content[truepos + 2] == 0x11 && content[truepos + 3] == 0x11)
                            counter2 = -1;
                        trig = false;
                        string val = BitConverter.ToSingle(content, truepos).ToString();
                        if (pos >= truepos && pos <= truepos + 3) {
                            Console.BackgroundColor = NormalFG;
                            Console.ForegroundColor = NormalBG;
                            Console.Write(BitConverter.ToString(content, truepos, 4).Replace("-", "").PadRight(content_width, ' '));
                            Console.BackgroundColor = NormalBG;
                            Console.ForegroundColor = NormalFG;
                        } else if (pos + 4 >= truepos && pos + 4 <= truepos + 3) {
                            val = BitConverter.ToInt32(content, truepos).ToString();
                            Console.ForegroundColor = ChunkSizeColor;
                            Console.Write(val.Length > content_width ? val.Substring(0, content_width) : val.PadRight(content_width, ' '));
                        } else if (IfString(content, truepos)) {
                            Console.ForegroundColor = StringColor;
                            int nullind = Array.IndexOf(content, (byte)0x00, truepos, 4);
                            Console.Write(Encoding.UTF8.GetString(content, truepos, nullind == -1 ? 4 : (nullind - truepos)).PadRight(content_width, ' '));
                            counter2 += 1;
                            if (counter2 >= maxcounter) {
                                counter2 = 0;
                                if (truepos < pos + chunksize)
                                trig = true;
                            }
                        } else {
                            if (val.Contains("e") || val.Contains("E") || val == "NaN") {
                                val = (BitConverter.ToInt32(content, truepos) / 256f - 32f).ToString();
                                //val = (BitConverter.ToInt16(content, truepos)).ToString() + " " + (BitConverter.ToInt16(content, truepos + 2)).ToString();
                                Console.ForegroundColor = IntColor;
                                Console.Write(val.Length > content_width ? val.Substring(0, content_width) : val.PadRight(content_width, ' '));
                            } else if (val == "0") {
                                Console.ForegroundColor = ZeroColor;
                                Console.Write("0".PadRight(content_width, ' '));
                            } else {
                                Console.ForegroundColor = FloatColor;
                                Console.Write(val.Length > content_width ? val.Substring(0, content_width) : val.PadRight(content_width, ' '));
                            }
                            counter2 += 1;
                            if (counter2 >= maxcounter) {
                                counter2 = 0;
                                if (truepos < pos + chunksize)
                                trig = true;
                            }
                        }
                        Console.BackgroundColor = NormalBG;
                        Console.ForegroundColor = NormalFG;
                        Console.Write(trig ? "| " : "  ");
                    }
                    Console.WriteLine();
                }
                Console.ResetColor();
                Console.ReadLine();
            }
        }
    }
}
