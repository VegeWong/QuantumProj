using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Threading.Tasks;

namespace Shor
{
    using ShorQuantum;
    class Driver
    { 
        static void Main(string[] args)
        {
            Console.WriteLine("Program Shor running...");
            if (args.Length == 0)
            {
                Console.WriteLine("\nPlease type the numbers you want to factor");
                return;
            }
            else
            {
                for (int i = 1; i <= args.Length; ++i)
                {
                    long targetNum = Convert.ToInt64(args[i - 1]);
                    long r = Procedure(targetNum);
                    string str = args[i - 1] + "=";
                    if (r == -1)
                    {
                        r = targetNum;
                        Console.WriteLine("Found: " + Convert.ToString(r));
                    }
                    str += Convert.ToString(r);
                    Console.WriteLine("Found: " + Convert.ToString(r));
                    while ((targetNum /= r) != 1)
                    {
                        r = Procedure(targetNum);
                        if (r == -1)
                            r = targetNum;
                        Console.WriteLine("Found: " + Convert.ToString(r));
                        str += "x" + Convert.ToString(r);
                    }
                    Console.WriteLine(str);
                }
            }

        }

        // === Algorithm: Reduction of factoring to order-finding ===
        // Inputs: A composite number N
        // Outputs: A non-trivial factor of N .
        // Runtime: O((log N ) 3 ) operations. Succeeds with probability O(1).
        // Procedure:
        // 1. If N is even, return the factor 2.
        // 2. Determine whether N = a^b for integers a ≥ 1 and b ≥ 2, and if so
        // return the factor a (uses the classical algorithm of Exercise 5.17).
        // 3. Randomly choose x in the range 1 to N −1. If gcd(x, N ) > 1 then return
        // the factor gcd(x, N).
        // 4. Use the order-finding subroutine to find the order r of x modulo N .
        // 5. If r is even and x r/2 = − 1(mod N ) then compute gcd(x r/2 − 1, N ) and
        // gcd(x r/2 + 1, N ), and test to see if one of these is a non-trivial factor,
        // returning that factor if so. Otherwise, the algorithm fails.

        static long Procedure(long n)
        {   
            long s3repT = 5;

            // Step1
            if (n % 2 == 0) return 2;

            // Step2
            double y = Math.Log(2, n);
            for (long b = 2; b <= Math.Sqrt(n); ++b)
            {
                double x = y / b;
                double simulateValue = Math.Pow(2.0, x);
                long rc = FpowMod(Convert.ToInt64(Math.Ceiling(simulateValue)), b, n);
                long rf = FpowMod(Convert.ToInt64(Math.Floor(simulateValue)), b, n);
                if (rc == n || rf == n) return b; 
            }

            // Step3
            Random rd = new Random();
            long[] presult = new long [s3repT];
            for (int i = 0; i < s3repT; ++i)
                presult[i] = -1;
            ParallelLoopResult result = Parallel.For (0, s3repT,
                                            (i, ParallelLoopState) =>
            {
                try
                {
            
                    Console.WriteLine(string.Format("Thread{0} begins", Convert.ToString(i)));
                    long x = rd.Next(2, Convert.ToInt32(Math.Sqrt(n)) + 1);
                    presult[i] = ParallelWork(x, n);
            
                }
                catch
                {
                    presult[i] = -1;
                }
            });
            while (!result.IsCompleted) ;
            for (int i = 0; i < s3repT; ++i)
                if (presult[i] != -1)
                    return presult[i];
            return -1;
        }

        static long ParallelWork(long x, long n)
        {
            long gcdxn = Gcd(x, n);
            if (gcdxn > 1)
            {
                // Console.WriteLine(Convert.ToString(x)+':'+Convert.ToString(gcdxn));
                return gcdxn;
            }
            else
            {
                // Step4
                long ord = orderFinding(x, n);

                // Step5
                if (ord % 2 == 0)
                {
                    long xpow = x ^ (ord / 2);
                    if (xpow % n != n - 1)
                    {
                        long tmp = Gcd(xpow - 1, n);
                        if (tmp != 1 && n % tmp == 0)
                        {
                            // Console.WriteLine(Convert.ToString(x)+':'+Convert.ToString(tmp));
                            return tmp;
                        }
                        else
                        {
                            tmp = Gcd(xpow + 1, n);
                            if (tmp != 1 && n % tmp == 0)
                            {
                                // Console.WriteLine(Convert.ToString(x)+':'+Convert.ToString(tmp));
                                return tmp;
                            }
                        } 
                    }
                }
            }
            // Console.WriteLine(Convert.ToString(x)+':'+Convert.ToString(-1));
            return -1;
        }

        static long orderFinding(long nbase, long n)
        {
            long numerator = 0;
            long numOfbit;
            int a = Convert.ToInt32(Math.Log(Convert.ToDouble(n))/Math.Log(2.0)) + 1;
            int b = a + 1;
            using  (var sim = new QuantumSimulator(randomNumberGeneratorSeed: 2018))
            {
                var res = QuantumOrderFinding.Run(sim, nbase, n, a, b).Result;

                //　Result process
                numOfbit = res.Length;
                for (int i = 0; i < numOfbit; ++i)
                {
                    numerator <<= 1;
                    numerator += res[i];
                }
                // double fraction = numerator / 2 ^ numOfbit;
            }
            return ContinueFractionAlgorithm(numerator / 2 ^ numOfbit);
        }


        static long ContinueFractionAlgorithm(double m)
        {
            double eps = 1e-10;
            long[] M = new long[20];
            int n = 0;
            bool flag = true;
            while (flag)
            {
                M[n] = Convert.ToInt64(Math.Floor(m + eps));
                if (Math.Abs(m - M[n]) < eps)
                    flag = false;
                else
                    m = 1 / (m - M[n]);
                ++n;
            }
            
            // Reconstruct
            long numerator = 0;
            long denominator = 1;
            long tmp;
            while (--n >= 0)
            {
                tmp = numerator + denominator * M[n];
                numerator = denominator;
                denominator = tmp;
            }
            return numerator;
        }

        // Util-function
        static long FpowMod(long nbase, long exp, long N)
        {
            long res = 1;
            while (exp != 0)
            {
                if (exp % 2 == 1)
                    res = res * nbase % N;
                nbase = nbase ^ 2 % N;
                exp = exp >> 1;
            }
            return res;
        }

        static long Gcd(long a, long b)
        {
            long tmp;
            while (b != 0)
            {
                tmp = b;
                b = a % b;
                a = tmp;
            }
            return a;
        }
    }


}