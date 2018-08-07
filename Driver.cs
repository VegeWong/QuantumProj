using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;


namespace Shor
{
    using ShorQuantum;
    class Driver
    { 
        
        static void Main(string[] args)
        {
            Console.WriteLine("Program Shor running...")
            if (args.Length == 0)
            {
                Console.WriteLine("\nPlease type the numbers you want to factor")
                return;
            }
            else
            {
                for (int i = 1； i <= args.Length; ++i)
                {
                    long r = Procedure(Convert.ToInt64(args[i - 1]));
                    if (r != -1)
                        Console.WriteLine(string.Format("\n{0} is a factor of {1}", Convert.Tostring(r), args[i - 1]));
                    else
                        Console.WriteLine(string.Format("\nShor factoring of {0} fails", args[i - 1]));
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
            long y = Math.Log(2, n);
            for (long b = 2; b <= Math.sqrt(n); ++b)
            {
                double x = y / b;
                double simulateValue = 2 ^ x;
                long rc = Fpow(Math.Ceiling(simulateValue), b);
                long rf = Fpow(Math.Floor(simulateValue), b);
                if (rc == n || rf == n) return b; 
            }

            // Step3
            Random rd = new Random();
            for (int i = 0; i < s3repT; ++i)
            {
                long x = rd.next(1, n);
                long gcdxn = Gcd(x, n);
                if (gcdxn > 1)
                    return gcdxn;
                else
                {
                    // Step4
                    long ord = orderFinding(x, n);
                    // Step5
                    if (ord & 1 == 0)
                    {
                        long xpow = x ^ (ord / 2);
                        if (xpow % n != n - 1)
                        {
                            tmp = Gcd(xpow - 1, n);
                            if (tmp != 1 && n % tmp == 0)
                                return tmp;
                            tmp = Gcd(xpow + 1, n);
                            if (tmp != 1 && n % tmp == 0)
                                return tmp;   
                        }
                    }
                    return -1;
                }
            }
            

        }
        static long orderFinding(long base, long n)
        {
            using  (var sim = new QuantumSimulat(randomNumberGeneratorSeed: 2018))
            {
                var res = QuantumOrderFinding.Run(sim).Result;

                //　Result process
                long numerator = 0;
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
            eps = 1e-10;
            long[] M = new long[20]
            int n = 0;
            bool flag = true
            while (flag)
            {
                M[n] = Math.Floor(m + eps);
                if (Math.Abs(m - M[n]) < eps)
                    flag = false;
                else
                    m = 1 / (m - M[n]);
                ++n;
            }
            
            // Reconstruct
            long numerator = 0;
            long denominator = 1;
            while (--n >= 0)
            {
                tmp = numerator + denominator * M[n];
                numerator = denominator;
                denominator = tmp;
            }
            return numerator;
        }

        // Util-function
        static long Fpow(long base, long exp)
        {
            long res = 1;
            while (exp != 0)
            {
                if (exp & 1 == 1)
                    res = res * base % N;
                base = base ^ 2 % N;
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