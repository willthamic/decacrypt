using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

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
            Application.Run(new Decacrypt());
        }
    }

    class Key
    {
        public BigInteger p;
        public BigInteger q;
        public BigInteger n;
        public BigInteger e;
        public BigInteger d;

        // Checks to see if the key is valid.
        // O: byte[] - length of 6, each index represents one of elements of the key, with a final overall validity index. The meaning of the values are listed below.
        //    Index [0]   p: 0 - undefined; 1 - valid; 2 - composite; 8 - negative/low.
        //    Index [1]   q: 0 - undefined; 1 - valid; 2 - composite; 8 - negative/low.
        //    Index [2]   n: 0 - undefined; 1 - valid; 2 - possibly valid; 3 - prime; 4 - too many factors (p or q not prime); 5 - incorrect product; 8 - negative/low.
        //    Index [3]   e: 0 - undefined; 1 - valid; 2 - possibly valid; 3 - too high; 4 - not coprime with (p-1)(q-1); 5 - not modinv of e mod (p-1)(q-1); 8 - negative/low.
        //    Index [4]   d: 0 - undefined; 1 - valid; 2 - possibly valid; 3 - too high; 4 - not coprime with (p-1)(q-1); 5 - not modinv of d mod (p-1)(q-1); 8 - negative/low.
        //    Index [5] all: 1 - valid; 2 - possibly valid; 3 - invalid.
        public byte[] Validate()
        {
            // Array showing the current validity of the key.
            byte[] array = { 0, 0, 0, 0, 0, 0 };

            // If 'p' is defined, test it for primality with the MillerRabin algorithm.
            if (p != 0)
                // The MillerRabin function returns 0 for composite, 1 for prime, 2 for invalid.
                // This has to be converted to 2 for composite, 1 for prime, 8 for invalid, for consistency, and then set to the index for 'p'.
                array[0] = new byte[] { 2, 1, 8 }[CryptoMath.MillerRabin(p, 50)];

            // If 'p' is defined, test it for primality with the MillerRabin algorithm.
            if (q != 0)
                // The MillerRabin function returns 0 for composite, 1 for prime, 2 for invalid.
                // This has to be converted to 2 for composite, 1 for prime, 8 for invalid, for consistency, and then set to the index for 'q'.
                array[1] = new byte[] { 2, 1, 8 }[CryptoMath.MillerRabin(q, 50)];

            // Runs if 'n' is defined (value other than 0).
            if (n != 0)
                array[2] = ValidateModulus(p, q, n, array);

            // Runs if 'e' is defined.
            if (e != 0)
                array[3] = ValidateExponent(p, q, n, e, d);

            // Runs if 'd' is defined.
            if (d != 0)
                array[4] = ValidateExponent(p, q, n, d, e);

            // If the first 5 indexes are equal to 1, for valid, set the index 6 to 1 for valid.
            if (array[0] == 1 && array[1] == 1 && array[2] == 1 && array[3] == 1 && array[4] == 1)
                array[5] = 1;

            // If any of the first 5 indexes are not either valid, or possibly valid, or undefined, set the index for 6 to 3 for invalid.
            else if ((array[0] != 0) && (array[0] != 1))
                array[5] = 3;
            else if ((array[1] != 0) && (array[1] != 1))
                array[5] = 3;
            else if ((array[2] != 1) && (array[2] != 2))
                array[5] = 3;
            else if ((array[3] != 1) && (array[3] != 2))
                array[5] = 3;
            else if ((array[4] != 0) && (array[4] != 1) && (array[4] != 2))
                array[5] = 3;
            else
                array[5] = 2;

            return array;
        }

        // Check to see if a modulus, typically 'n', is valid.
        // I: BigInteger p & q - The primes that make up 'n'; BigInteger n - Modulus to be checked; byte[] array - Array of current key validity.
        // O: Byte - 0 - undefined; 1 - valid; 2 - possibly valid; 3 - prime; 4 - too many factors (p or q not prime); 5 - incorrect product; 8 - negative/low.
        public byte ValidateModulus(BigInteger p, BigInteger q, BigInteger n, byte[] array)
        {
            byte validity = 0;

            // Sets the index for 'n' to 8 for negative/low if the value is less than 6.
            if (n < 6)
                validity = 8;

            // Runs if both 'p' and 'q' are defined.
            else if ((p != 0) && (q != 0))
            {
                // If both 'p' and 'q' are prime and p*q equals n, set the index for 'n' to 1 for valid.
                if ((array[0] == 1) && (array[1] == 1) && (p * q == n))
                    validity = 1;

                // If 'p*q' does not equal 'n', set the index for 'n' to 5 for incorrect product. 
                else if (p * q != n)
                    validity = 5;

                // If either 'p' or 'q' is composite, and 'p*q' equals 'n', set the index for 'n' to 4 for too many factors.
                else if (((array[0] == 0) || (array[1] == 0)) && (p * q == n))
                    validity = 4;

                // If 'n' is prime, set the index for 'n' to 3 for prime.
                else if (CryptoMath.MillerRabin(n, 50) == 1)
                    validity = 3;

                // If these pass, set the index for 'n' to 1 for valid.
                else
                    validity = 1;
            }

            // Runs if either 'p' or 'q' are undefined (equal to 0).
            else
            {
                // If 'n' is composite, set the index for 'n' to 2 for possibly valid.
                if (CryptoMath.MillerRabin(n, 50) == 0)
                    validity = 2;
            }

            return validity;
        }

        // Checks if an exponent is valid.
        // I: BigInteger p & q - primes making up 'n'; BigInteger n - modulus; BigInteger e - Exponent being validated; BigInteger d - Exponent being validated against.
        // O: Byte - 0 - undefined; 1 - valid; 2 - possibly valid; 3 - too high; 4 - not coprime with (p-1)(q-1); 5 - not modinv of d mod (p-1)(q-1); 8 - negative/low.
        public byte ValidateExponent(BigInteger p, BigInteger q, BigInteger n, BigInteger e, BigInteger d)
        {
            byte validity = 0;

            // If 'e' is lower than two, set the index for 'e' to 8 for negative/low.
            if (e < 2)
                validity = 8;

            // Runs if 'p' and 'q' and 'n' are defined.
            else if ((p != 0) && (q != 0) && (n != 0))
            {
                // If 'e' is larger than or equal to '(p-1)(n-1)', set the index for 'e' to 3 for too high.
                if (e >= (p - 1) * (n - 1))
                    validity = 3;

                // If 'e' and '(p-1)(n-1)' are not coprime, set the index for 'e' to 4 for not coprime with '(p-1)(n-1)'.
                else if (BigInteger.GreatestCommonDivisor(e, (p - 1) * (q - 1)) != 1)
                    validity = 4;

                // If 'd' is defined and the modular inverse of 'd' mod '(p-1)(q-1)' does not equal e, set the index for 'e' to 5 for not modinv of d mod (p-1)(q-1).
                else if ((e != 0) && (d != 0) && (CryptoMath.ModInv(d, (p - 1) * (q - 1)) != e))
                    validity = 5;

                // If above pass, set the index for 'e' to 1 for valid.
                else
                    validity = 1;
            }

            // Runs if either 'p' or 'q' or 'n' are undefined.
            else if ((p == 0) || (q == 0))
            {
                // If 'n' is defined and 'e' is larger than 'n', set the index of 'e' to 3 for too high.
                if ((n != 0) && (e >= n))
                    validity = 3;

                // If above pass, set the index of 'e' to 2 for possibly valid.
                else
                    validity = 2;
            }

            return validity;
        }
    }

    class Base {
        // Converts values between bases.
        // I: string value - Value being converted; string inputBase - Base of the input; string outputBase - Base of the output.
        // O: string - converted input.
        static public string ConvertBase (string value, string inputBase, string outputBase)
        {
            if (value == "")
                return "";

            if (inputBase == outputBase)
                return value;

            // Regex values that select characters other than characters used for that base.
            Regex B64 = new Regex(@"[^A-Za-z0-9=+/]");
            Regex HEX = new Regex(@"[^0-9A-Fa-f]");
            Regex DEC = new Regex(@"[^0-9]");

            // BigInteger used for conversion.
            BigInteger dec = 0;

            // Converts input to decimal.
            switch (inputBase)
            {
                // If any of the characters in the input are selected by the corresponding regex, return error.
                case "B64":
                    if (B64.IsMatch(value))
                        return "ERROR: INVALID";
                    try
                    {
                        dec = new BigInteger(Convert.FromBase64String(value));
                    } catch
                    {
                        return "ERROR: INVALID";
                    }
                    break;
                case "HEX":
                    if (HEX.IsMatch(value))
                        return "ERROR: INVALID";
                    dec = BigInteger.Parse(value, NumberStyles.AllowHexSpecifier);
                    break;
                case "DEC":
                    if (DEC.IsMatch(value))
                        return "ERROR: INVALID";
                    BigInteger.TryParse(value, out dec);
                    break;
                default:
                    return "ERROR: INVALID";
            }

            // Converts decimal to output.
            switch (outputBase)
            {
                case "B64":
                    return Convert.ToBase64String(dec.ToByteArray());
                case "HEX":
                    return dec.ToString("X");
                case "DEC":
                    return dec.ToString();
                default:
                    return "ERROR: INVALID";
            }
        }
    }

    // Contains math methods for cryptography.
    class CryptoMath
    {
        // Finds a large prime within a range.
        // I: int n - Prime size in bits; string method - Method used for test; int k - iterations of testing;
        // O: BigInteger - The large prime. 
        static public BigInteger FindPrime(int n, string method, int k)
        {
            if (n < 8)
                return -1;
            if (k < 1)
                return -1;

            // Random Number Generator
            var rng = new RNGCryptoServiceProvider();

            // Begins loop until prime is found.
            while (true)
            {
                // Creates a random BigInteger for testing with the amount of bits specified by the input 'n'.
                byte[] bytes = new byte[n / 8];
                rng.GetBytes(bytes);
                BigInteger p = new BigInteger(bytes);

                // Begins a switch that uses the specified method to check if prime.
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
                    default:
                        return -1;
                }
            }
        }

        // Tests if a value is prime using Fermat algorithm.
        // I: BigInteger n - Value for testing; int k - iterations of testing;
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static public byte Fermat(BigInteger n, int k)
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

                // Returns 0 for composite if 'a^(n-1)%n' does not equal 1.
                if (BigInteger.ModPow(a, n - 1, n) != 1)
                    return 0;
            }
            // If it passes all iterations of testing, return 1 for prime.
            return 1;
        }

        // Tests if a value is prime using MillerRabin algorithm.
        // I: BigInteger n - Value for testing; int k - How thorough the test is. (iterations of testing)
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static public byte MillerRabin(BigInteger n, int k)
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

                if ((x == 1) || (x == n - 1))
                    continue;

                // Tests the value of d with all combinations of factors of 2.
                for (var r = 1; (r <= s - 1) && tmp; r++)
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
            } while ((n > max) || (n < min));

            return n;
        }

        // Finds the modular inverse of a number 'a' modulo another number 'b'.
        // I: BigInteger a; BigInteger b;
        // O: BigInteger - modular inverse of a mod b.
        static public BigInteger ModInv(BigInteger a, BigInteger n)
        {
            // These BigIntegers are used to calculate the inverse.
            BigInteger t = 0;
            BigInteger tn = 1;
            BigInteger r = n;
            BigInteger rn = a;

            // These BigIntegers are used for swapping the above values.
            BigInteger t_temp;
            BigInteger tn_temp;
            BigInteger r_temp;
            BigInteger rn_temp;

            while (rn != 0)
            {
                BigInteger quotient = r / rn;

                // Some of the values need to be swapped, which requires temporary variables.
                t_temp = tn;
                tn_temp = t - quotient * tn;
                r_temp = rn;
                rn_temp = r - quotient * rn;

                // Temporary variables are assigned to the actual variables.
                t = t_temp;
                tn = tn_temp;
                r = r_temp;
                rn = rn_temp;
            }
            // If 'r' is greater than 1, return -1 for invalid.
            if (r > 1)
                return -1;
            // If 't' is less than 0, add 'n' to 't'.
            if (t < 0)
                t += n;

            // Return the inverse 't'.
            return t;
        }
    }
}
