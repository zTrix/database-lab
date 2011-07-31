using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private int lastLogLen = -1;
        public Form1()
        {
            InitializeComponent();
        }

        public void setList(Array names)
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            foreach(String i in names)
            {
                comboBox1.Items.Add((i as String).Trim());
                comboBox2.Items.Add((i as String).Trim());
            }
        }

        public void fail(String reason)
        {
            printResult(false);
            printLog(Environment.NewLine + "Error: " + reason);
        }

        public void printLog(String log)
        {
            textBox3.AppendText(log);
            lastLogLen = log.Length;
        }

        public void printResult(bool ok)
        {
            StringBuilder sb = new StringBuilder(100);
            for (int i = lastLogLen; i < 66; i++)
            {
                sb.Append(" ");
            }
            if (ok)
            {
                textBox3.AppendText(sb.ToString()+"[  OK  ]" + Environment.NewLine);
            }
            else
            {
                textBox3.AppendText(sb.ToString() + "[FAILED]" + Environment.NewLine);
            }
        }

        public void clearLog()
        {
            textBox3.Clear();
            textBox3.Text = String.Empty;
            textBox3.Update();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:";
            openFileDialog.Filter = "XML文件|*.xml|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK){
                String fName = openFileDialog.FileName;
                int lastD = fName.LastIndexOf("\\");
                textBox2.Text = fName.Substring(lastD+1);
                textBox2.Update();
                Program.xml_name = fName;
                WindowsFormsApplication1.Program.start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String a = comboBox1.Text.Trim();
            String b = comboBox2.Text.Trim();
            clearLog();
            if (Program.type == 1)
            {
                printLog("New query: " + a + "/" + b + " ...");
            }
            else if (Program.type == 2)
            {
                printLog("New query: " + a + "//" + b + " ...");
            }

            if (Program.xml_name.Equals(""))
            {
                printResult(false);
                printLog(Environment.NewLine + "Error: no XML specified!" + Environment.NewLine);
                return;
            }

            if (a.Length > 0 && b.Length > 0)
            {
                printResult(true);
            }
            else
            {
                printResult(false);
                printLog(Environment.NewLine + "Error: empty key not allowed!" + Environment.NewLine);
                return;
            }

            Program.search(a, b);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Program.type == 1)
            {
                Program.type = 2;
                button4.Image = global::XMLHarder.Properties.Resources.two;
            }
            else
            {
                Program.type = 1;
                button4.Image = global::XMLHarder.Properties.Resources.one;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Program.disk+"\\";
            saveFileDialog1.Title = "Save text Files";
            //saveFileDialog1.CheckFileExists = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string name = saveFileDialog1.FileName;
                string txt = textBox3.Text;
                printLog("Saving log output to "+name+" ...");
                System.IO.File.WriteAllText(name, txt);
                printResult(true);
            }
        }

    }
}
