using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var stdout = Console.OpenStandardOutput();
            var writer = new StreamWriter(stdout) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);

            Console.WriteLine("Now this goes to Docker logs");

            Console.WriteLine("Hello World!");

            while(true)
            {
                Console.WriteLine("Test");
                Thread.Sleep(1000); 
            }
        }
    }
}
