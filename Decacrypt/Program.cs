using System;
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

    static class Prime
    {
        // Finds a large prime within a range.
        // I: int n - How many bits the prime is.
        // O: BigInteger - The large prime. 
        static public BigInteger FindPrime(int n)
        {
            return 0;
        }

        // Tests if a value is prime.
        // I: BigInteger n - Value for testing; int k - How thorough the test is. (iterations of testing)
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static private byte Fermat(BigInteger n, int k)
        {
            return 0;
        }

        // Tests if a value is prime.
        // I: BigInteger n - Value for testing; int k - How thorough the test is. (iterations of testing)
        // O: Byte - 0 for composite, 1 for prime, 2 for invalid.
        static private byte MillerRabin(BigInteger n, int k)
        {
            return 0;
        }
    }
}
