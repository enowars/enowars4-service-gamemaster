using System;

namespace RandomTest
{
    class Program
    {

        static byte fNextBytes(int InternalSample)
        {
            return (byte)(InternalSample % (Byte.MaxValue + 1));
        }
        static void Main(string[] args)
        {

            Random r = new Random();
            r.Next();
            r.Next();
            r.Next();
            r.Next();
            r.Next();
            r.Next();
            r.Next();
            Console.WriteLine("Hello World!");
            for (int i = 0; 56 != ++i;)
            {
                var next = r.Next();
                Console.WriteLine($"{i:0} {next:X4} || {fNextBytes(next):X}") ;
            }
            Console.ReadKey();
            Console.WriteLine((int.MaxValue - 1) / int.MaxValue);
            Console.ReadKey();
        }
    }
}
