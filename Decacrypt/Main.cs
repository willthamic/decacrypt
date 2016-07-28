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
            // Sets default state to comboBoxes.
            comboBoxP.SelectedItem = "DEC";
            comboBoxQ.SelectedItem = "DEC";
            comboBoxN.SelectedItem = "DEC";
            comboBoxE.SelectedItem = "DEC";
            comboBoxD.SelectedItem = "DEC";
            comboBoxM.SelectedItem = "DEC";
            comboBoxC.SelectedItem = "DEC";
        }

        public void UpdateKey()
        {
            // Converts and assigns the values of the textboxes to the key.
            BigInteger.TryParse(Base.ConvertBase(textBoxP.Text, comboBoxP.Text, "DEC"), out MainKey.p);
            BigInteger.TryParse(Base.ConvertBase(textBoxQ.Text, comboBoxQ.Text, "DEC"), out MainKey.q);
            BigInteger.TryParse(Base.ConvertBase(textBoxN.Text, comboBoxN.Text, "DEC"), out MainKey.n);
            BigInteger.TryParse(Base.ConvertBase(textBoxE.Text, comboBoxE.Text, "DEC"), out MainKey.e);
            BigInteger.TryParse(Base.ConvertBase(textBoxD.Text, comboBoxD.Text, "DEC"), out MainKey.d);
        }
        
        public void UpdateText()
        {
            // Converts and sets the values of the key to the textboxes.
            if (MainKey.p != 0)
                textBoxP.Text = Base.ConvertBase(MainKey.p.ToString(), "DEC", comboBoxP.Text);
            if (MainKey.q != 0)
                textBoxQ.Text = Base.ConvertBase(MainKey.q.ToString(), "DEC", comboBoxQ.Text);
            if (MainKey.n != 0)
                textBoxN.Text = Base.ConvertBase(MainKey.n.ToString(), "DEC", comboBoxN.Text);
            if (MainKey.e != 0)
                textBoxE.Text = Base.ConvertBase(MainKey.e.ToString(), "DEC", comboBoxE.Text);
            if (MainKey.d != 0)
                textBoxD.Text = Base.ConvertBase(MainKey.d.ToString(), "DEC", comboBoxD.Text);
        }

        private void buttonVerifyKey_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Validate the key and set the output array to a byte array, 'validity'.
            byte[] validity = MainKey.Validate();

            // Display a message box, first showing 'Valid' or 'Invalid', then the array which is necessary for debugging.
            MessageBox.Show(((validity[5] == 1) || (validity[5] == 2) ? "Valid: " : "Invalid: ") + string.Join(",", validity));
        }

        private void buttonNewP_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Generate a new prime with the amount of bits specified by the trackbar.
            MainKey.p = CryptoMath.FindPrime(trackBarP.Value, "MillerRabin", 50);

            // Reset textboxes to the correct values.
            UpdateText();
        }

        private void buttonNewQ_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Generate a new prime with the amount of bits specified by the trackbar.
            MainKey.q = CryptoMath.FindPrime(trackBarQ.Value, "MillerRabin", 50);

            // Reset textboxes to the correct values.
            UpdateText();
        }

        private void buttonNewN_Click(object sender, EventArgs e)
        {
            UpdateKey();

            // Generate two primes at size of half the bits set by the 'n' trackbar.
            MainKey.p = CryptoMath.FindPrime(trackBarN.Value / 2, "MillerRabin", 50);
            MainKey.q = CryptoMath.FindPrime(trackBarN.Value / 2, "MillerRabin", 50);
            MainKey.n = MainKey.p * MainKey.q;

            // Reset textboxes to the correct values.
            UpdateText();
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
                MainKey.d = CryptoMath.ModInv(MainKey.e, (MainKey.p - 1) * (MainKey.q - 1));
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If both 'n' and 'q' are defined and valid, calculate and fill in 'p'.
        private void buttonFixP_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[1] == 1 && (validity[2] == 1 || validity[2] == 2))
            {
                MainKey.p = MainKey.n / MainKey.q;
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If both 'n' and 'p' are defined and valid, calculate and fill in 'q'.
        private void buttonFixQ_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && (validity[2] == 1 || validity[2] == 2))
            {
                MainKey.q = MainKey.n / MainKey.p;
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If both, 'p' and 'q' are defined and valid, calculate and fill in 'n'.
        private void buttonFixN_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1)
            {
                MainKey.n = MainKey.p * MainKey.q;
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If both, 'p', 'q' and 'd' are defined and valid, calculate and fill in 'e'.
        private void buttonFixE_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1 && validity[4] == 1)
            {
                MainKey.e = CryptoMath.ModInv(MainKey.d, (MainKey.p - 1) * (MainKey.q - 1));
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If both, 'p', 'q' and 'e' are defined and valid, calculate and fill in 'd'. 
        private void buttonFixD_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            if (validity[0] == 1 && validity[1] == 1 && validity[3] == 1)
            {
                MainKey.d = CryptoMath.ModInv(MainKey.e, (MainKey.p - 1) * (MainKey.q - 1));
            }

            // Reset textboxes to the correct values.
            UpdateText();
        }

        // If the key is valid, calculate the ciphertext.
        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            // Runs of the key is valid for encryption.
            if ((validity[2] == 1 || validity[2] == 2) && (validity[3] == 1 || validity[3] == 2))
            {
                BigInteger m;
                BigInteger.TryParse(textBoxM.Text, out m);

                if (m >= MainKey.n)
                    return;

                textBoxC.Text = BigInteger.ModPow(m, MainKey.e, MainKey.n).ToString();
            }
        }

        // If the key is valid, calculate the plaintext.
        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            UpdateKey();

            byte[] validity = MainKey.Validate();

            // Runs of the key is valid for decryption.
            if ((validity[2] == 1 || validity[2] == 2) && (validity[4] == 1 || validity[4] == 2))
            {
                BigInteger c;
                BigInteger.TryParse(textBoxC.Text, out c);

                if (c >= MainKey.n)
                    return;

                textBoxM.Text = BigInteger.ModPow(c, MainKey.d, MainKey.n).ToString();
            }
        }

    }
}
