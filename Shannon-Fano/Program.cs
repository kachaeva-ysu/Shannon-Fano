using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Shannon_Fano
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите имя файла:");
            var fileName = Console.ReadLine();
            string text;
            using (var sr = new StreamReader(fileName))
                text = sr.ReadToEnd();

            var statistics = GetStatistics(text);
            statistics = statistics.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            Console.WriteLine("Статистика:");
            foreach (var item in statistics)
                Console.WriteLine(item.Key + " " + item.Value);

            double[] frequencies = statistics.Values.ToArray();
            StringBuilder[] codes = new StringBuilder[frequencies.Length];
            for (int i = 0; i < codes.Length; i++)
                codes[i] = new StringBuilder();
            GetCodes(frequencies, codes, 1);
            var codesDictionary = new Dictionary<char, StringBuilder>();
            int index = 0;
            foreach (var item in statistics)
            {
                codesDictionary.Add(item.Key, codes[index]);
                index++;
            }
            Console.WriteLine("Словарь:");
            foreach (var item in codesDictionary)
                Console.WriteLine(item.Key + " " + item.Value);
            Console.WriteLine();

            Console.WriteLine("Кодированный текст:");
            var code = Code(codesDictionary, text);
            Console.WriteLine(code);

            Console.WriteLine("Декодированный текст:");
            var decode = Decode(codesDictionary, code);
            Console.WriteLine(decode);
            Console.WriteLine();

            var rate = GetRate(statistics, codesDictionary);
            Console.WriteLine("Среднее количество битов на символ: " + rate);

            var entropy = GetEntropy(statistics);
            Console.WriteLine("Энтропия: "+entropy);

            var coeff = GetCompressionCoefficient(code, text);
            Console.WriteLine("Коэффициент сжатия: " + coeff);

            Console.ReadLine();
        }
        static Dictionary<char, double> GetStatistics(string text)
        {
            var statistics = new Dictionary<char, double>();
            foreach (var symbol in text)
            {
                if (statistics.ContainsKey(symbol))
                    statistics[symbol] = (statistics[symbol] * text.Length + 1) / text.Length;
                else
                    statistics.Add(symbol, (double)1 / text.Length);
            }
            return statistics;
        }
        static void GetCodes(double[] frequencies, StringBuilder[] codes, double sum)
        {
            if (frequencies.Length == 1)
                return;
            double leftSum = 0;
            int index = 0;
            while (Math.Abs(leftSum - sum / 2) > Math.Abs(leftSum + frequencies[index] - sum / 2))
            {
                leftSum += frequencies[index];
                index++;
            }
            double[] leftHalf = new double[index];
            double[] rightHalf = new double[frequencies.Length - index];
            for (int i = 0; i < index; i++)
            {
                codes[i].Append('0');
                leftHalf[i] = frequencies[i];
            }
            for (int i = index; i < frequencies.Length; i++)
            {
                codes[i].Append('1');
                rightHalf[i - index] = frequencies[i];
            }
            StringBuilder[] leftCodes = new StringBuilder[leftHalf.Length];
            StringBuilder[] rightCodes = new StringBuilder[rightHalf.Length];
            for (int i = 0; i < leftHalf.Length; i++)
                leftCodes[i] = new StringBuilder();
            for (int i = 0; i < rightHalf.Length; i++)
                rightCodes[i] = new StringBuilder();
            GetCodes(leftHalf, leftCodes, leftSum);
            GetCodes(rightHalf, rightCodes, sum - leftSum);
            for (int i = 0; i < index; i++)
            {
                codes[i].Append(leftCodes[i]);
            }
            for (int i = index; i < frequencies.Length; i++)
            {
                codes[i].Append(rightCodes[i - index]);
            }
        }
        static StringBuilder Code(Dictionary<char, StringBuilder> codesDictionary, string text)
        {
            StringBuilder code = new StringBuilder();
            foreach (var symbol in text)
                code.Append(codesDictionary[symbol]);
            return code;
        }
        static StringBuilder Decode(Dictionary<char, StringBuilder> codesDictionary, StringBuilder code)
        {
            StringBuilder decode = new StringBuilder();
            Dictionary<string, char> reversedDictionary = new Dictionary<string, char>();
            foreach (var item in codesDictionary)
                reversedDictionary.Add(item.Value.ToString(), item.Key);
            int index = 0;
            StringBuilder symbol = new StringBuilder();
            while (index < code.Length)
            {
                if (reversedDictionary.ContainsKey(symbol.ToString()))
                {
                    decode.Append(reversedDictionary[symbol.ToString()]);
                    symbol = new StringBuilder();
                }
                else
                {
                    symbol.Append(code[index]);
                    index++;
                }
            }
            decode.Append(reversedDictionary[symbol.ToString()]);
            return decode;
        }
        static double GetRate(Dictionary<char, double> statistics, Dictionary<char, StringBuilder> codesDictionary)
        {
            double rate = 0;
            foreach (var symbol in statistics.Keys)
                rate += statistics[symbol] * codesDictionary[symbol].Length;
            return rate;
        }
        static double GetEntropy(Dictionary<char, double> statistics)
        {
            double entropy = 0;
            foreach(var item in statistics)
                entropy += item.Value * Math.Log(item.Value, 2);
            return Math.Round(-entropy,2);
        }
        static double GetCompressionCoefficient(StringBuilder code,string text)
        {
            return Math.Round((double)text.Length * 8 / code.Length,2);
        }
    }
}
