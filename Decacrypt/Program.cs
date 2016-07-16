﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Security.Cryptography;

namespace Decacrypt
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    // Contains methods for prime number calculation.
    static class Prime
    {
        // Finds a large prime within a range.
        // I: int n - Prime size in bits; string method - Method used for test; int k - iterations of testing;
        // O: BigInteger - The large prime. 
        static public BigInteger FindPrime(int n, string method, int k)
        {
            var rng = new RNGCryptoServiceProvider();

            while (true)
            {
                byte[] bytes = new byte[n / 8];
                rng.GetBytes(bytes);
                BigInteger p = new BigInteger(bytes);
                switch (method)
                {
                    case "Fermat":
                        if (Fermat(p, k) == 1)
                            return p;
                        break;
                    case "MillerRabin":
                        if (MillerRabin(p, k) == 1)
                            return p;
                        break;
                }
            }
        }

        // Tests if a value is prime using Fermat algorithm.
        // I: BigInteger n - Value for testing; int k - iterations of testing;
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static private byte Fermat(BigInteger n, int k)
        {
            Random random = new Random();

            // Returns 2 for invalid if input is less than or equal to 1, or if the k value is 0 or less.
            if ((n <= 1) || (k <= 0))
                return 2;

            // Manually returns prime for 2 and 3 as the fermat primality test only works for values above 3.
            else if ((n == 2) || (n == 3))
                return 1;

            // Begins a loop for k iterations
            for (var i = 0; i < k; i++)
            {
                // Generates a random number between 2 and the lower of n - 2 and int.MaxValue.
                // This is because the random function will not take a value larger than int.MaxValue.
                BigInteger a = RandomRange(2, n - 2);

                // Returns 0 for composite if a^(n-1)%n does not equal 1.
                if (BigInteger.ModPow(a, n - 1, n) != 1)
                    return 0;
            }
            // If it passes all iterations of testing, return 1 for prime.
            return 1;
        }

        // Tests if a value is prime using MillerRabin algorithm.
        // I: BigInteger n - Value for testing; int k - How thorough the test is. (iterations of testing)
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static private byte MillerRabin(BigInteger n, int k)
        {
            // Array of first 16 primes.
            int[] lowPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 };
            BigInteger d = n - 1;
            int s = 0;

            // Returns 2 for invalid if input is less than or equal to 1, or if the k value is 0 or less.
            if ((n <= 1) || (k <= 0))
                return 2;

            // Loops through each value of lowPrimes.
            for (var i = 0; i < 16; i++)
            {
                // Returns 0 for composite if n is divisible by lowPrimes[i] AND if n is not equal to lowPrimes[i].
                if ((n % lowPrimes[i] == 0) && (n != lowPrimes[i]))
                    return 0;
            }

            // Converts n - 1 into the form d*(s^2). If even, divides d by 2 and increases s.
            while (d % 2 == 0)
            {
                d >>= 1;
                s++;
            }

            // Loop with k iterations. The chance of a composite appearing as probably prime for large numbers is 1/(4^k). At 50 iterations the chance is 1 in 10^30.
            for (var i = 0; i < k; i++)
            {
                // Creates random integer a between 2 and the lower of int.MaxValue and n - 1.
                BigInteger a = RandomRange(2, n - 1);

                // Sets x to a^d % n.
                BigInteger x = BigInteger.ModPow(a, d, n);

                // This boolean is used to break the inner loop and continue the parent loop.
                bool tmp = true;

                if (x == 1 || x == n - 1)
                    continue;

                // Tests the value of d with all combinations of factors of 2.
                for (var r = 1; r <= s - 1 && tmp; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == 1)
                        return 0;
                    // Setting tmp to false causes this for loop to end and skips the return at the end and moves to the next iteration of the outer loop.
                    if (x == n - 1)
                        tmp = false;
                }
                // Skips the return statement if at any point in the previous (inner) loop, x was equal to n - 1 and then continues the parent loop.
                if (tmp)
                    return 0;
            }
            return 1;
        }

        // Generates a secure random BigInteger in a range, inclusive.
        // I: BigInteger min - Lower side of range; BigInteger max - Upper side of range;
        // O: BigInteger - Random BigInteger within range.
        static public BigInteger RandomRange(BigInteger min, BigInteger max)
        {
            // Creates an array 'bytes' of the max value converted to bytes.
            // A byte array is essentially the number represented in base 256.
            byte[] bytes = max.ToByteArray();
            BigInteger n;

            // Used to generate cryptographically secure bytes or byte arrays.
            var random = new RNGCryptoServiceProvider();

            // Runs code block until 'n' is within the rnage of min and max, inclusive.
            do
            {
                // Fills byte array 'bytes' with random bytes.
                random.GetBytes(bytes);
                n = new BigInteger(bytes);
            } while (n > max || n < min);

            return n;
        }
    }
}
