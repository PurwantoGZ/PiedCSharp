using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compression;
namespace PiepCSharp.App
{
     class Program
    {
        private const string Example = "saya suka sama susu situ";
        static void Main(string[] args)
        {
            
            Console.WriteLine("asasa");
            var huffman = new Huffman<char>(Example);
            /*1. Encode String*/
            List<int> encoding = huffman.Encode(Example);
            /*2. Decode Encoding */
            List<char> decoding = huffman.Decode(encoding);

            var outString = new string(decoding.ToArray());

            Console.WriteLine(outString == Example ? "Encoding/Worked" : "Encoding/Decoding Failed");

            var chars = new HashSet<char>(Example);
            foreach (char c in chars)
            {
                encoding = huffman.Encode(c);
                Console.Write("{0}: ", c);

                foreach (int bit in encoding)
                {
                    Console.Write("{0} ", bit);
                }
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}
