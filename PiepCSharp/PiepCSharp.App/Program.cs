using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compression;
using NeuralNetwork;
namespace PiepCSharp.App
{
     class Program
    {
        
        static void Main(string[] args)
        {

            List<int> text = new List<int>();
            text.Add(100);
            text.Add(100);
            text.Add(100);
            text.Add(100);
            text.Add(100);
            text.Add(100);
            text.Add(200);
            text.Add(200);
            text.Add(200);
            text.Add(100);
            text.Add(250);
            text.Add(100);
            text.Add(200);
            text.Add(100);
            text.Add(250);
           
            // Tampil Data
            Console.WriteLine("Data Asli");
            foreach (int item in text)
            {
                Console.Write("{0} ",item);
            }
            Console.WriteLine("\nJumlah Bit Data Asli {0} x 8 = {1} bit",text.Count,text.Count*8);
            var huffman = new Huffman<int>(text);
            Console.WriteLine("\nData Encode");
            List<int> encoding = huffman.Encode(text);
            foreach (int item in encoding)
            {
                Console.Write("{0} ",item);
            }
            Console.WriteLine("\nJumlah Bit Data Encoding {0}", encoding.Count);

            List<int> decoding = huffman.Decode(encoding);

            Console.WriteLine("\nData Decoding");
            foreach (int item in decoding)
            {
                Console.Write("{0} ",item);
            }
            Console.WriteLine("\n\nDetail Compressed");

            var ints = new HashSet<int>(text);
            foreach (int c in ints)
            {
                encoding = huffman.Encode(c);
                Console.Write("{0} : ",c);
                foreach (int bit in encoding)
                {
                    Console.Write("{0}",bit);
                }
                Console.WriteLine();
            }
            
            Console.ReadLine();
        }
    }
}
