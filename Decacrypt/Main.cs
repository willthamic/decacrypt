using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace Decacrypt
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void buttonVerifyKey_Click(object sender, EventArgs e)
        {
            // Create object using class 'Key' called 'key'.
            Key key = new Key();

            // Parse the inputs into the the valiables inside the object.
            BigInteger.TryParse(textBoxP.Text, out key.p);
            BigInteger.TryParse(textBoxQ.Text, out key.q);
            BigInteger.TryParse(textBoxN.Text, out key.n);
            BigInteger.TryParse(textBoxE.Text, out key.e);
            BigInteger.TryParse(textBoxD.Text, out key.d);

            // Validate the key and set the output array to a byte array, 'validity'.
            byte[] validity = key.Validate();

            // Display a message box, first showing 'Valid' or 'Invalid', then the array which is necessary for debugging.
            MessageBox.Show(((validity[5] == 1) || (validity[5] == 2) ? "Valid: " : "Invalid: ") + string.Join(",", validity));
        }

        private void buttonNewP_Click(object sender, EventArgs e)
        {
            // Generate a new prime with the amount of bits specified by the trackbar.
            BigInteger p = CryptoMath.FindPrime(trackBarP.Value, "MillerRabin", 50);

            // Set 'q' to the textboxQ.
            BigInteger q;
            BigInteger.TryParse(textBoxQ.Text, out q);

            // Reset textboxes 'p' and 'n' the the correct values.
            textBoxP.Text = p.ToString();
            textBoxN.Text = (p * q).ToString();
        }

        private void buttonNewQ_Click(object sender, EventArgs e)
        {
            // Set 'p' to the textboxP.
            BigInteger p;
            BigInteger.TryParse(textBoxQ.Text, out p);

            // Generate a new prime with the amount of bits specified by the trackbar.
            BigInteger q = CryptoMath.FindPrime(trackBarQ.Value, "MillerRabin", 50);

            // Reset textboxes 'q' and 'n' to the correct values.
            textBoxQ.Text = q.ToString();
            textBoxN.Text = (p * q).ToString();
        }

        private void buttonNewN_Click(object sender, EventArgs e)
        {
            // Generate two primes at size of half the bits set by the 'n' trackbar.
            BigInteger p = CryptoMath.FindPrime(trackBarN.Value / 2, "MillerRabin", 50);
            BigInteger q = CryptoMath.FindPrime(trackBarN.Value / 2, "MillerRabin", 50);

            // Reset the textboxes 'p', 'q', and 'n' to the correct values.
            textBoxP.Text = p.ToString();
            textBoxQ.Text = q.ToString();
            textBoxN.Text = (p * q).ToString();
        }
    }
}
