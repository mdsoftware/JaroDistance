using System;
using System.Collections.Generic;
using System.Text;

namespace JaroDistance
{
    class Program
    {
        static void Main(string[] args)
        {
            JaroCalculator calc = JaroCalculator.New();

            string s = @"Quick brown fox jumps over a lazy dog";
            string t = @"Quick brown fox jumps over a lazy dog";
            Console.WriteLine("'{0}' <-> '{1}' = {2}", s, t, calc.Calculate(s, t).ToString("0.00000"));

            t = @"Quick brown fox jumps over a lazy fog";
            Console.WriteLine("'{0}' <-> '{1}' = {2}", s, t, calc.Calculate(s, t).ToString("0.00000"));

            t = @"Quick brown fox jumps over a quick dog";
            Console.WriteLine("'{0}' <-> '{1}' = {2}", s, t, calc.Calculate(s, t).ToString("0.00000"));

            t = @"Mammy, where you go, it's just a dream";
            Console.WriteLine("'{0}' <-> '{1}' = {2}", s, t, calc.Calculate(s, t).ToString("0.00000"));

            s = @"Abcdefgi";
            t = @"Rstuwxyz";
            Console.WriteLine("'{0}' <-> '{1}' = {2}", s, t, calc.Calculate(s, t).ToString("0.00000"));

            Console.Write(">>> PRESS ENTER");
            Console.ReadLine();
        }
    }
}
