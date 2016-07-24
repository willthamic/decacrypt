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
        Key MainKey = new Key();
        

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
        }

        public void UpdateKey()
        {
            BigInteger.TryParse(textBoxP.Text, out MainKey.p);
            BigInteger.TryParse(textBoxQ.Text, out MainKey.q);
            BigInteger.TryParse(textBoxN.Text, out MainKey.n);
            BigInteger.TryParse(textBoxE.Text, out MainKey.e);
            BigInteger.TryParse(textBoxD.Text, out MainKey.d);
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
            UpdateKey();

            // Generate a new prime with the amount of bits specified by the trackbar.
            MainKey.p = CryptoMath.FindPrime(trackBarP.Value, "MillerRabin", 50);

            // Reset textboxes 'q' and 'n' to the correct values.
            textBoxP.Text = MainKey.p.ToString();
            if (MainKey.q != 0)
                textBoxN.Text = (MainKey.p * MainKey.q).ToString();
        }

        private void buttonNewQ_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Generate a new prime with the amount of bits specified by the trackbar.
            MainKey.q = CryptoMath.FindPrime(trackBarQ.Value, "MillerRabin", 50);

            // Reset textboxes 'q' and 'n' to the correct values.
            textBoxQ.Text = MainKey.q.ToString();
            if (MainKey.p != 0)
                textBoxN.Text = (MainKey.p * MainKey.q).ToString();
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

        private void buttonNewE_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Generate a prime at the size of the bits set by the 'e' trackbar.
            BigInteger pubExp = CryptoMath.FindPrime(trackBarE.Value, "MillerRabin", 50);
            textBoxE.Text = pubExp.ToString();
            MainKey.e = pubExp;

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1)
            {
                BigInteger pvtExp = CryptoMath.ModInv(MainKey.e, (MainKey.p - 1) * (MainKey.q - 1));
                textBoxD.Text = pvtExp.ToString();
                MainKey.d = pvtExp;
            }

        }

        // If both 'n' and 'q' are defined and valid, calculate and fill in 'p'.
        private void buttonFixP_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[1] == 1 && (validity[2] == 1 || validity[2] == 2))
            {
                MainKey.p = MainKey.n / MainKey.q;
                textBoxP.Text = MainKey.p.ToString();
            }
        }

        // If both 'n' and 'p' are defined and valid, calculate and fill in 'q'.
        private void buttonFixQ_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && (validity[2] == 1 || validity[2] == 2))
            {
                MainKey.q = MainKey.n / MainKey.p;
                textBoxQ.Text = MainKey.q.ToString();
            }
        }

        // If both, 'p' and 'q' are defined and valid, calculate and fill in 'n'.
        private void buttonFixN_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1)
            {
                MainKey.n = MainKey.p * MainKey.q;
                textBoxN.Text = MainKey.n.ToString();
            }
        }

        // If both, 'p', 'q' and 'd' are defined and valid, calculate and fill in 'e'.
        private void buttonFixE_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1 && validity[4] == 1)
            {
                BigInteger pubKey = CryptoMath.ModInv(MainKey.d, (MainKey.p - 1) * (MainKey.q - 1));
                textBoxE.Text = pubKey.ToString();
                MainKey.e = pubKey;
            }
        }

        private void buttonFixD_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1 && validity[3] == 1)
            {
                BigInteger pvtKey = CryptoMath.ModInv(MainKey.e, (MainKey.p - 1) * (MainKey.q - 1));
                textBoxD.Text = pvtKey.ToString();
                MainKey.d = pvtKey;
            }
        }
    }
}
