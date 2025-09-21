using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WindowsFormsApp2
{
    public partial class Form1: Form
    {
        private Random random = new Random();
    
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "#&@?%!'_+/";

        // Declare checkboxes as class-level fields
        private CheckBox checkBoxRequireUppercase;
        private CheckBox checkBoxRequireNumber;
        private CheckBox checkBoxRequireSpecialChar;
        private Label labelMeetsLength;
        private Label labelHasUppercase;
        private Label labelHasNumber;
        private Label labelHasSpecial;
        private NumericUpDown numericUpDownMinLength;
        private Label labelMinLength;
        public Form1()
        {
            InitializeComponent();
            InitializeCheckBoxes();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Length";
            label2.Text = "Your new password:";
            label3.Text = "Password generator";
            label4.Text = "Password suggestion history";
            button1.Text = "Generate";
            radioButton1.Text = "Upper+lowercase";
            radioButton2.Text = "Contains numbers";
            radioButton3.Text = "Contains special characters";
            radioButton1.Checked = true;
            label5.AutoSize = false;
            label5.TextAlign = ContentAlignment.MiddleLeft;
            label5.Text = "The selected line will be automatically copied into the main Windows clipboard";
            label5.Width = 90;
            label5.Height = 90;

        }

        private void InitializeCheckBoxes()
        {
            checkBoxRequireUppercase = new CheckBox { Text = "Include Uppercase Letters", Left = 180, Top = 128, Width = 200 };
            checkBoxRequireNumber = new CheckBox { Text = "Include Numbers", Left = 180, Top = 165, Width = 200 };
            checkBoxRequireSpecialChar = new CheckBox { Text = "Include Special Characters", Left = 180, Top = 202, Width = 250 };
           labelMeetsLength = new  Label { Text = "Meets Length Requirement ❌", Left = 180, Top = 249, Width = 250, ForeColor = System.Drawing.Color.Gray };
            labelHasUppercase = new Label { Text = "Contains Uppercase Letter ❌", Left = 180, Top = 286, Width = 250, ForeColor = System.Drawing.Color.Gray };
            labelHasNumber = new Label { Text = "Contains Number ❌", Left = 180, Top = 323, Width = 250, ForeColor = System.Drawing.Color.Gray };
            labelHasSpecial = new Label { Text = "Contains Special Character ❌", Left = 180, Top = 360, Width = 250, ForeColor = System.Drawing.Color.Gray };

            labelMinLength = new Label { Text = "Minimal Length", Left = 15, Top = 77 };
            numericUpDownMinLength = new NumericUpDown { Left = 15, Top = 97, Minimum = 4, Maximum = 20, Value = 8 };
      

            Controls.Add(checkBoxRequireUppercase);
            Controls.Add(checkBoxRequireNumber);
            Controls.Add(checkBoxRequireSpecialChar);
            Controls.Add(labelMeetsLength);
            Controls.Add(labelHasUppercase);
            Controls.Add(labelHasNumber);
            Controls.Add(labelHasSpecial);
            Controls.Add(labelMinLength);
            Controls.Add(labelMinLength);
            Controls.Add(numericUpDownMinLength);


        }


     
        private void createRandomPassword()
        {
            int length = (int)numericUpDown1.Value;
            int minLength = (int)numericUpDownMinLength.Value;

            if (length < minLength)
            {
                MessageBox.Show($"Password length must be at least {minLength}", "Invalid Length", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<char> passwordChars = new List<char>();
            string charPool = "";

            if (radioButton1.Checked)
            {
                charPool = UppercaseLetters + LowercaseLetters;
            }
            else if (radioButton2.Checked)
            {
                charPool = Numbers;
            }
            else if (radioButton3.Checked)
            {
                charPool = SpecialChars;
            }

            if (checkBoxRequireUppercase.Checked)
            {
                charPool += UppercaseLetters;
                passwordChars.Add(UppercaseLetters[random.Next(UppercaseLetters.Length)]);
            }
            if (checkBoxRequireNumber.Checked)
            {
                charPool += Numbers;
                passwordChars.Add(Numbers[random.Next(Numbers.Length)]);
            }
            if (checkBoxRequireSpecialChar.Checked)
            {
                charPool += SpecialChars;
                passwordChars.Add(SpecialChars[random.Next(SpecialChars.Length)]);
            }
            if (!radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked)
            {
                charPool += LowercaseLetters;
            }

            while (passwordChars.Count < length)
            {
                passwordChars.Add(charPool[random.Next(charPool.Length)]);
            }

            passwordChars = passwordChars.OrderBy(_ => random.Next()).ToList();
            string generatedPassword = new string(passwordChars.ToArray());

            textBox1.Text = generatedPassword;
            listBox1.Items.Add(generatedPassword);

            validatePassword(generatedPassword);
        }

        private void validatePassword(string password)
        {
            bool meetsLength = password.Length >= numericUpDownMinLength.Value;
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => SpecialChars.Contains(ch));

            labelMeetsLength.Text = meetsLength ? "Meets Length Requirement ✅" : "Meets Length Requirement ❌";
            labelMeetsLength.ForeColor = meetsLength ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            labelHasUppercase.Text = hasUppercase ? "Contains Uppercase Letter ✅" : "Contains Uppercase Letter ❌";
            labelHasUppercase.ForeColor = hasUppercase ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            labelHasNumber.Text = hasNumber ? "Contains Number ✅" : "Contains Number ❌";
            labelHasNumber.ForeColor = hasNumber ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            labelHasSpecial.Text = hasSpecial ? "Contains Special Character ✅" : "Contains Special Character ❌";
            labelHasSpecial.ForeColor = hasSpecial ? System.Drawing.Color.Green : System.Drawing.Color.Red;

        }


        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedPassword = listBox1.SelectedItem.ToString();
                Clipboard.SetText(selectedPassword);
                validatePassword(selectedPassword);
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            createRandomPassword();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

       
       
        }
    

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

       
        
         
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

           
            
        }
    }
}

