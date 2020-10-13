using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Huffman
{
    class Constants
    {
        public static readonly string freqFile = "../../freq.txt";
        public static readonly string etalonFile = "../../etalon.txt";
        public static readonly string etalonArcFile = "../../etalon.arc";
        public static readonly string etalonDecFile = "../../etalon_dec.txt";
    }
    class HuffmanTree
    {
        public char ch { get; private set; }
        public double freq { get; private set; }
        public bool isTerminal { get; private set; }
        public HuffmanTree left {get; private set;}
        public HuffmanTree rigth {get; private set;}
        public HuffmanTree(char c, double frequency)
        {
            ch = c;
            freq = frequency;
            isTerminal = true;
            left = rigth = null;
        }
        public HuffmanTree(HuffmanTree l, HuffmanTree r)
        {
            freq = l.freq + r.freq;
            isTerminal = false;
            left = l;
            rigth = r;
        }
    }
    class HuffmanInfo
    {
		HuffmanTree Tree; // дерево кода Хаффмана, потребуется для распаковки
        Dictionary<char, string> Table; // словарь, хранящий коды всех символов, будет удобен для сжатия

        private List<HuffmanTree> laters = new List<HuffmanTree>();
        public HuffmanInfo(string fileName)
        {   
			string line;		
            StreamReader sr = new StreamReader(fileName, Encoding.Unicode);
            // считать информацию о частотах символов
			while ((line = sr.ReadLine()) != null)
			{
				if (line.Length == 0)
                {
                    var line2 = sr.ReadLine().Split(' ');
                    laters.Add(new HuffmanTree('\n', double.Parse(line2[1])));
                }
				else
                {
                    var line2 = line.Substring(2);
                    laters.Add(new HuffmanTree(line[0], double.Parse(line2)));
                }
			}
            sr.Close();
            laters.Add(new HuffmanTree('\0', 0));
            while (laters.Count > 1)
            {
                laters.Sort(srav);
                laters.Add(new HuffmanTree(laters[0], laters[1]));
                laters.RemoveAt(0);
                laters.RemoveAt(0);
            }

            Tree = laters[0];
			Table = new Dictionary<char, string>();
            dfs(Tree, "");
        }
        public void Compress(string inpFile, string outFile)
        {
            var sr = new StreamReader(inpFile, Encoding.Unicode);
            var sw = new ArchWriter(outFile); //нужна побитовая запись, поэтому использовать StreamWriter напрямую нельзя
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                foreach (var ch in line)
                {
                    sw.WriteWord(Table[ch]);
                }
                sw.WriteWord(Table['\n']);
            }
            sr.Close();
            sw.WriteWord(Table['\0']); // записываем признак конца файла
            sw.Finish();
        }

        public void Decompress(string archFile, string txtFile)
        {
            var sr = new ArchReader(archFile); // нужно побитовое чтение
            var sw = new StreamWriter(txtFile, false, Encoding.Unicode);
            byte curBit;
            while (sr.ReadBit(out curBit))
            {
                var cur = Tree;
                while (true)
                {
                    if (curBit.ToString() == "0")
                        cur = cur.left;
                    else
                        cur = cur.rigth;
                    if (cur.isTerminal)
                        break;
                    sr.ReadBit(out curBit);
                }
                if (cur.ch == '\n')
                    sw.WriteLine();
                else
                    sw.Write(cur.ch);
            }
            sr.Finish();
            sw.Close();
        }

        private int srav(HuffmanTree a, HuffmanTree b)
        {
            return a.freq.CompareTo(b.freq);
        }

        private void dfs(HuffmanTree item, string cod)
        {
            if (item.isTerminal)
            {
                Table.Add(item.ch, cod);
                return;
            }

            dfs(item.left, cod + '0');
            dfs(item.rigth, cod + '1');
        }
    }

    class Huffman
    {
        static void Main(string[] args)
        {
            if (!File.Exists(Constants.freqFile))
            {
                Console.WriteLine("Не найден файл с частотами символов!");
                return;
            }
            if (args.Length == 0)
            {
                var hi = new HuffmanInfo(Constants.freqFile);
                hi.Compress(Constants.etalonFile, Constants.etalonArcFile);
                hi.Decompress(Constants.etalonArcFile, Constants.etalonDecFile);
                return;
            }
            if (args.Length != 3 || args[0] != "zip" && args[0] != "unzip")
            {
                Console.WriteLine("Синтаксис:");
                Console.WriteLine("Huffman.exe zip <имя исходного файла> <имя файла для архива>");
                Console.WriteLine("либо");
                Console.WriteLine("Huffman.exe unzip <имя файла с архивом> <имя файла для текста>");
                Console.WriteLine("Пример:");
                Console.WriteLine("Huffman.exe zip text.txt text.arc");
                return;
            }
            var HI = new HuffmanInfo(Constants.freqFile);
            if (args[0] == "zip")
            {
                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Не найден файл с исходным текстом!");
                    return;
                }
                HI.Compress(args[1], args[2]);
            }
            else
            {
                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Не найден файл с архивом!");
                    return;
                }
                HI.Decompress(args[1], args[2]);
            }
            Console.WriteLine("Операция успешно завершена!");
        }
    }
}
