using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pract1
{
    public class MyException : Exception
    {
        public MyException(string s) : base(s)
        { }
    }

    class ShannonEnthropy
    {
        private int _variation;

        //1 or 2
        public ShannonEnthropy(int variant)
        {
            _variation = variant;
        }

        private double MPlogP(double p)
        {
            //вызывает NaN из-за подсчёта логарифма на слишком маленьком значении
            //поэтому фиксим
            double v = -p * Math.Log(p) / Math.Log(2);
            if(Double.IsNaN(v))
                return 0;
            return v;
        }

        private double GetTheoreticalEnthropy(double[] probabilities)
        {
            double enthropy = 0.0;
            if(_variation == 1)
            {
                foreach (double d in probabilities)
                    enthropy += MPlogP(d);
            }
            else if(_variation == 2)
            {
                for(int i = 0; i < probabilities.Length; i++)
                {
                    for(int j = 0; j < probabilities.Length; j++)
                    {
                        enthropy += MPlogP(probabilities[i]*probabilities[j]);
                    }
                }
                enthropy /= 2;
            }
            else if(_variation == 3)
            {
                for (int i = 0; i < probabilities.Length; i++)
                {
                    for (int j = 0; j < probabilities.Length; j++)
                    {
                        for (int k = 0; k < probabilities.Length; k++)
                        {
                            enthropy += MPlogP(probabilities[i] * probabilities[j] * probabilities[k]);
                        }
                    }
                }
                enthropy /= 3;
            }
            else
                Console.WriteLine("Неверно инициализирован экземпляр класса.");
            return enthropy;
        }

        public KeyValuePair<double, double> CalculateEnthropy(char[] symbols, string filename, double[] probabilities = null)
        {
            double enthropy;

            int symCount = symbols.Length;

            if (probabilities == null)
            {
                probabilities = new double[symCount];
                for (int i = 0; i < symCount; i++)
                {
                    probabilities[i] = (1.0 / (double)(symCount));
                }
            }

            double[] values = new double[probabilities.Length + 1];
            values[0] = 0.0;
            double sum = 0.0;
            for (int i = 0; i < symCount; i++)
            {
                sum += probabilities[i];
                values[i + 1] = sum;
            }
            if (Math.Abs(sum - 1.0) > 0.0000001)
            {
                Console.WriteLine("Неверные вероятности - сумма не равна 1.");
                return new KeyValuePair<double, double>();
            }
            string data;
            try
            {
                data = System.IO.File.ReadAllText(filename);
            }
            catch(System.IO.IOException)
            {
                Console.WriteLine("Файл не был открыт.");
                return new KeyValuePair<double, double>();
            }

            if(_variation == 1)
            {
                double[] calculatedProbabilities = new double[symCount];

                foreach(char c in data)
                {
                    for(int i = 0; i < symCount; i++)
                    {
                        if(c == symbols[i])
                        {
                            calculatedProbabilities[i]++;
                            break;
                        }
                    }
                }
                sum = 0.0;
                for (int i = 0; i < symCount; i++)
                {
                    calculatedProbabilities[i] /= data.Length;
                    sum += MPlogP(calculatedProbabilities[i]);
                }
                enthropy = sum;
            }
            else if(_variation == 2)
            {
                Dictionary<string, int> pairCounts = new Dictionary<string, int>();
                string currentPair;
                for(int i = 1; i < data.Length; i++)
                {
                    currentPair = data[i - 1].ToString() + data[i].ToString();
                    if (pairCounts.ContainsKey(currentPair))
                    {
                        pairCounts[currentPair]++;
                    }
                    else
                    {
                        pairCounts.Add(currentPair, 1);
                    }
                }
                sum = 0.0;
                foreach(KeyValuePair<string,int> pair in pairCounts)
                {
                    sum += MPlogP((double)pair.Value / (double)(data.Length - 1));
                }
                enthropy = sum / 2;
            }
            else if(_variation == 3)
            {
                Dictionary<string, int> pairCounts = new Dictionary<string, int>();
                string currentPair;
                for (int i = 2; i < data.Length; i++)
                {
                    currentPair = data[i - 2].ToString() + data[i - 1].ToString() + data[i].ToString();
                    if (pairCounts.ContainsKey(currentPair))
                    {
                        pairCounts[currentPair]++;
                    }
                    else
                    {
                        pairCounts.Add(currentPair, 1);
                    }
                }
                sum = 0.0;
                foreach (KeyValuePair<string, int> pair in pairCounts)
                {
                    sum += MPlogP((double)pair.Value / (double)(data.Length - 2));
                }
                enthropy = sum / 3;
            }
            else
            {
                Console.WriteLine("Неверно инициализирован экземпляр класса.");
                return new KeyValuePair<double, double>();
            }

            return new KeyValuePair<double, double>(enthropy, GetTheoreticalEnthropy(probabilities));
        }
    }

    class FileGenerator
    {
        private Random _rng;
        private int _fileLength;

        public FileGenerator(int lengthBytes)
        {
            _rng = new Random();
            _fileLength = lengthBytes;
        }

        public void GenerateFile(char[] symbols, string filename, double[] probabilities = null)
        {
            int symCount = symbols.Length;

            if (probabilities == null)
            {
                probabilities = new double[symCount];
                for(int i = 0; i < symCount; i++)
                {
                    probabilities[i] = (1.0 / (double)(symCount));
                }
            }

            double[] values = new double[probabilities.Length + 1];
            values[0] = 0.0;
            double sum = 0.0, randValue;
            for (int i = 0; i < symCount; i++)
            {
                sum += probabilities[i];
                values[i + 1] = sum;
            }
            if (Math.Abs(sum - 1.0) > 0.0000001)
            {
                Console.WriteLine("Неверные вероятности - сумма не равна 1.");
                return;
            }

            string content = "";

            for(int i = 0; i < _fileLength; i++)
            {
                //0.0 - 1.0
                randValue = _rng.NextDouble();
                for(int j = 1; j < values.Length; j++)
                {
                    if (randValue >= values[j - 1] && randValue <= values[j])
                    {
                        content += symbols[j - 1];
                        break;
                    }
                }
            }

            try
            {
                System.IO.File.WriteAllText(filename, content);
            }
            catch
            {
                Console.WriteLine("Файл не был сформирован.");
                return;
            }
            
            Console.WriteLine("Сгенерирован " + filename + ", размер: " + (double)_fileLength / 1024.0 + " Kb");
            Console.Write("Вероятности появления символов: ");
            foreach (double d in probabilities)
                Console.Write(d + "    ");
            Console.Write("\n\n");
        }
    }

    class LiteraryTextHandler
    {
        public void CreateFixedText(string source, string target, char[] symbols)
        {
            string data, prepairedData="";
            try
            {
                data = System.IO.File.ReadAllText(source);
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Файл не был открыт.");
                return;
            }

            data = data.Replace("ъ", "ь").Replace("ё", "е").ToLower();

            HashSet<char> alphabet = new HashSet<char>();
            foreach (char c in symbols)
                alphabet.Add(c);

            foreach(char c in data)
            {
                if (alphabet.Contains(c))
                    prepairedData += c;
            }

            try
            {
                System.IO.File.WriteAllText(target, prepairedData);
            }
            catch
            {
                Console.WriteLine("Файл не был сформирован.");
                return;
            }
        }
    }

    class ShannonCode
    {
        private static double EPS = 0.0000000001;

        public static string Convert(double number, int numSystem, int accuracy)
        {
            if (accuracy < 0 || accuracy > 24)
                throw new MyException("Неверное значение точности!");
            if (!(numSystem >= 2 && numSystem <= 16))
                throw new MyException("Неверная система счисления!");

            string value = "";
            long integerPart = (long)(Math.Abs(number)), tmp;
            double doublePart = Math.Round(Math.Abs(number) - integerPart, accuracy), tmp1;
            tmp = integerPart;
            while (tmp != 0)
            {
                long t = tmp % numSystem;
                if (t < 10)
                    value = (char)(t + (int)('0')) + value;
                else
                    value = (char)(t + (int)('A' - 10)) + value;
                tmp /= numSystem;
            }

            if (doublePart > EPS)
            {
                value += ".";
                tmp1 = doublePart * numSystem;
                tmp = accuracy;
                while (tmp > 0)
                {
                    long t = (long)tmp1 % numSystem;
                    if (t < 10)
                        value += (char)(t + (int)('0'));
                    else
                        value += (char)(t + (int)('A' - 10));
                    tmp--;
                    tmp1 *= numSystem;
                }
            }
            if (number < 0)
                value = "-" + value;
            return value;
        }

        private Dictionary<char, double> CalculateProbabilities(string s)
        {
            Dictionary<char, int> symbols = new Dictionary<char, int>();
            Dictionary<char, double> probabilities = new Dictionary<char, double>();

            foreach(char c in s)
            {
                if (!symbols.ContainsKey(c))
                    symbols.Add(c, 0);
                symbols[c]++;
            }

            double sum = 0.0;
            foreach(KeyValuePair<char, int> pair in symbols)
            {
                probabilities.Add(pair.Key, (double)pair.Value / (double)s.Length);
                sum += probabilities[pair.Key];
            }

            if (Math.Abs(sum - 1.0) > EPS)
            {
                Console.WriteLine("Критическая ошибка при подсчёте вероятностей символов " + sum);
                return null;
            }
            return probabilities;
        }

        public double EncodeText(string source, string target, int symbolCount = 2)
        {
            string data;
            try
            {
                data = System.IO.File.ReadAllText(source);
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Файл не был открыт.");
                return 0;
            }

            Dictionary<char, double> probs = CalculateProbabilities(data);
            Dictionary<double, char> sortedByValueProbs = new Dictionary<double, char>();
            List<string> code = new List<string>();
            foreach (KeyValuePair<char, double> p in probs)
            {
                sortedByValueProbs.Add(p.Value, p.Key);
            }
            var descSortedByValueProbs = sortedByValueProbs.OrderByDescending(i => i.Key);
            if(symbolCount == 2)
                Console.WriteLine("\nДвоичный код Шеннона для " + source+":");
            else
                Console.WriteLine("\n" + symbolCount + "-ичный код Шеннона для " + source + ":");
            Console.WriteLine("a[i]\tp[i]\tL[i]\tsum\tcode");
            double sum = 0.0, avg = 0.0;
            int L = 0;
            foreach(KeyValuePair<double, char> pair in descSortedByValueProbs)
            {
                while (Math.Pow(2.0, -L) > pair.Key)
                {
                    L++;
                }
                //перевод суммы в n-ичное число
                if (sum > EPS)
                    code.Add(Convert(sum, symbolCount, L + 2).Substring(1, L));
                else
                    code.Add(new string('0', L));

                Console.WriteLine(pair.Value + "\t" + pair.Key.ToString("0.#####") + "\t" + 
                    L + "\t" + sum.ToString("0.#####") + "\t" + code.Last());

                sum += pair.Key;
                avg += L * pair.Key;
            }

            sum = avg;
            Console.WriteLine("\nСредняя длина кодового слова: " + sum);

            //проверка, что код префиксный
            string[] arrayCode = code.ToArray();

            for(int i = 0; i < arrayCode.Length; i++)
            {
                for(int j = i + 1; j < arrayCode.Length; j++)
                {
                    if(arrayCode[j].Substring(0, arrayCode[i].Length) == arrayCode[i])
                    {
                        Console.WriteLine("\nКРИТИЧЕСКАЯ ОШИБКА - КОД НЕ ПРЕФИКСНЫЙ!");
                        return 0;
                    }
                }
            }
            Console.WriteLine("\nПроверка показала, что код является префиксным. Кодируем выходной файл...");
            
            L = 0;
            foreach(KeyValuePair<double, char> p in descSortedByValueProbs)
            {
                data = data.Replace(p.Value.ToString(), arrayCode[L]);
                L++;
            }
            try
            {
                System.IO.File.WriteAllText(target, data);
            }
            catch
            {
                Console.WriteLine("Файл не был сформирован.");
                return 0;
            }
            Console.WriteLine("Готово.");
            return sum;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            #region lab1
            /*ЛАБ 1*/

            char[] symbols = new char[] { 'a', 'O', '~', 'd' };
            //double[] probs = new double[] { 0.01, 0.09, 0.3, 0.6 }; 
            double[] probs = new double[] { 0.1, 0.2, 0.3, 0.4 };
            FileGenerator gen = new FileGenerator(20000);
            gen.GenerateFile(symbols, "file1.txt");
            gen.GenerateFile(symbols, "file2.txt", probs);

            ShannonEnthropy var1 = new ShannonEnthropy(1);
            ShannonEnthropy var2 = new ShannonEnthropy(2);
            Console.WriteLine("[фактическое значение, теоретическое значение]");
            Console.WriteLine("Энтропия по символам для file1.txt: " + var1.CalculateEnthropy(symbols, "file1.txt"));
            Console.WriteLine("Энтропия по парам для file1.txt: " + var2.CalculateEnthropy(symbols, "file1.txt"));
            Console.WriteLine("\nЭнтропия по символам для file2.txt: " + var1.CalculateEnthropy(symbols, "file2.txt", probs));
            Console.WriteLine("Энтропия по парам для file2.txt: " + var2.CalculateEnthropy(symbols, "file2.txt", probs));
            #endregion

            #region lab2
            /*ЛАБ 2*/

            char[] literarySymbols = new char[] { 'й','ц','у','к','е','н','г','ш','щ','з','х','ф','ы',
                'в','а','п','р','о','л','д','ж','э','я','ч','с','м','и','т','ь','б','ю',' '};

            LiteraryTextHandler handler = new LiteraryTextHandler();
            handler.CreateFixedText("literary.txt", "literary1.txt",literarySymbols);
            Console.WriteLine("Размер алфавита: " + literarySymbols.Length);
            Console.WriteLine("Энтропия по символам для literary1.txt: " + var1.CalculateEnthropy(literarySymbols, "literary1.txt"));
            Console.WriteLine("Энтропия по парам для literary1.txt: " + var2.CalculateEnthropy(literarySymbols, "literary1.txt"));
            #endregion

            #region lab3
            /*ЛАБ 3*/

            char[] binarySymbols = new char[] { '0', '1' };
            ShannonCode binShannon = new ShannonCode();
            ShannonEnthropy var3 = new ShannonEnthropy(3);

            double r1 = binShannon.EncodeText("file1.txt", "file1_decoded.txt");
            double r2 = binShannon.EncodeText("file2.txt", "file2_decoded.txt");
            double r3 = binShannon.EncodeText("literary1.txt", "literary1_decoded.txt");

            double v1 = var1.CalculateEnthropy(symbols, "file1.txt").Key;
            double v2 = var1.CalculateEnthropy(symbols, "file2.txt").Key;
            double v3 = var1.CalculateEnthropy(literarySymbols, "literary1.txt").Key;

            Console.WriteLine("Энтропия file1.txt:\t" + v1 +
                "\nЭнтропия file2.txt:\t" + v2 + "\nЭнтропия literary1.txt:\t" + v3);

            Console.WriteLine("\nИзбыточность кодирования для для file1_decoded.txt: " + (r1 - v1).ToString());
            Console.WriteLine("Избыточность кодирования для для file2_decoded.txt: " + (r2 - v2).ToString());
            Console.WriteLine("Избыточность кодирования для для literary1_decoded.txt: " + (r3 - v3).ToString());

            Console.WriteLine("\n[фактическое значение, теоретическое значение]");
            Console.WriteLine("Энтропия по символам для file1_decoded.txt: " + var1.CalculateEnthropy(binarySymbols, "file1_decoded.txt"));
            Console.WriteLine("Энтропия по парам для file1_decoded.txt: " + var2.CalculateEnthropy(binarySymbols, "file1_decoded.txt"));
            Console.WriteLine("Энтропия по тройкам для file1_decoded.txt: " + var3.CalculateEnthropy(binarySymbols, "file1_decoded.txt"));

            Console.WriteLine("\nЭнтропия по символам для file2_decoded.txt: " + var1.CalculateEnthropy(binarySymbols, "file2_decoded.txt"));
            Console.WriteLine("Энтропия по парам для file2_decoded.txt: " + var2.CalculateEnthropy(binarySymbols, "file2_decoded.txt"));
            Console.WriteLine("Энтропия по тройкам для file2_decoded.txt: " + var3.CalculateEnthropy(binarySymbols, "file2_decoded.txt"));

            Console.WriteLine("\nЭнтропия по символам для literary1_decoded.txt: " + var1.CalculateEnthropy(binarySymbols, "literary1_decoded.txt"));
            Console.WriteLine("Энтропия по парам для literary1_decoded.txt: " + var2.CalculateEnthropy(binarySymbols, "literary1_decoded.txt"));
            Console.WriteLine("Энтропия по тройкам для literary1_decoded.txt: " + var3.CalculateEnthropy(binarySymbols, "literary1_decoded.txt"));
            #endregion

            #region lab4
            /*ЛАБ 4*/

            char[] octalSymbols = new char[] { '0', '1' , '2', '3', '4', '5', '6', '7'};
            r1 = binShannon.EncodeText("file1.txt", "file1_decoded1.txt", 8);
            r2 = binShannon.EncodeText("file2.txt", "file2_decoded1.txt", 8);
            r3 = binShannon.EncodeText("literary1.txt", "literary1_decoded1.txt", 8);

            Console.WriteLine("\nИзбыточность кодирования для для file1_decoded1.txt: " + (r1 - v1).ToString());
            Console.WriteLine("Избыточность кодирования для для file2_decoded1.txt: " + (r2 - v2).ToString());
            Console.WriteLine("Избыточность кодирования для для literary1_decoded1.txt: " + (r3 - v3).ToString());

            Console.WriteLine("\n[фактическое значение, теоретическое значение]");
            Console.WriteLine("Энтропия по символам для file1_decoded1.txt: " + var1.CalculateEnthropy(octalSymbols, "file1_decoded1.txt"));
            Console.WriteLine("Энтропия по парам для file1_decoded1.txt: " + var2.CalculateEnthropy(octalSymbols, "file1_decoded1.txt"));
            Console.WriteLine("Энтропия по тройкам для file1_decoded1.txt: " + var3.CalculateEnthropy(octalSymbols, "file1_decoded1.txt"));

            Console.WriteLine("\nЭнтропия по символам для file2_decoded1.txt: " + var1.CalculateEnthropy(octalSymbols, "file2_decoded1.txt"));
            Console.WriteLine("Энтропия по парам для file2_decoded1.txt: " + var2.CalculateEnthropy(octalSymbols, "file2_decoded1.txt"));
            Console.WriteLine("Энтропия по тройкам для file2_decoded1.txt: " + var3.CalculateEnthropy(octalSymbols, "file2_decoded1.txt"));

            Console.WriteLine("\nЭнтропия по символам для literary1_decoded1.txt: " + var1.CalculateEnthropy(octalSymbols, "literary1_decoded1.txt"));
            Console.WriteLine("Энтропия по парам для literary1_decoded1.txt: " + var2.CalculateEnthropy(octalSymbols, "literary1_decoded1.txt"));
            Console.WriteLine("Энтропия по тройкам для literary1_decoded1.txt: " + var3.CalculateEnthropy(octalSymbols, "literary1_decoded1.txt"));
            #endregion

            /*Ожидание ввода*/
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}
