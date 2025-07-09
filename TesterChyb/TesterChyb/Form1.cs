using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using GeoTEL;

namespace TesterChyb
{
    public partial class Form1 : Form
    {
        private GeoTEL.SokkiaCom _sokkia = new GeoTEL.SokkiaCom();

        string typmereni;
        int SH = Screen.PrimaryScreen.Bounds.Height;
        int SW = Screen.PrimaryScreen.Bounds.Width;
        public Form1()
        {
            InitializeComponent();

            //Port
            _sokkia.Connect("Athletics");
            _sokkia.DataOUT += new GeoTEL.DataReceived(_sokkia_DataOUT);

            //PortSettings
            _port.WriteTimeout = 2000;
            _port.ReadTimeout = 5000;

            //Vizuals
            label1.Location = new Point(0, 0);
            label2.Location = new Point(0, SH / 8);
            label3.Location = new Point(0, SH / 4);
            label4.Location = new Point(0, SH * 3 / 8);
            Measure.Location = new Point(0, SH * 7 / 8);
            test1.Location = new Point(SW / 3, SH * 7 / 8);
            Exit.Location = new Point(SW * 2 / 3, SH * 7 / 8);

            label1.Size = new Size(SW, SH / 8);
            label2.Size = new Size(SW, SH / 8);
            label3.Size = new Size(SW, SH / 8);
            label4.Size = new Size(SW, SH / 8);
            Measure.Size = new Size(SW / 3, SH / 8);
            test1.Size = new Size(SW / 3, SH / 8);
            Exit.Size = new Size(SW / 3, SH / 8);
        }
        //Mereni
        private delegate void DataReceivedSokkia(char[] array, int typ);

        private void _sokkia_DataOUT(char[] data, int typ)
        {
            this.Invoke(new DataReceivedSokkia(this.DataRecievedHandler), data, typ);
        }


        private void DataRecievedHandler(char[] array, int typ)
        {
            var sb = new StringBuilder();
            sb.Append(array);
            if (typmereni == "uhel")
            {
                label1.Text = sb.ToString();
                label1.BackColor = Color.Blue;
            }
            else if (typmereni == "delka")
            {
                label2.Text = sb.ToString();
                label2.BackColor = Color.Blue;
            }
            else
            {
                label3.Text = "ERROR!";
                label3.BackColor = Color.Blue;
            }

            //MessageBox.Show(label1.Text);
            _sokkia.StopMeasST();
            label4.BackColor = Color.Red;
        }


        //MereniNefunguje
        private void Measure_Click(object sender, EventArgs e)
        {
            //PromeneKruh.XM = Math.Round(PromeneKruh.DM * Math.Sin(PromeneKruh.UM * Math.PI / 200), 4);
            //PromeneKruh.YM = Math.Round(PromeneKruh.DM * Math.Cos(PromeneKruh.UM * Math.PI / 200), 4);
            //PromeneKruh.RM = Math.Round(Math.Sqrt(Math.Pow(PromeneKruh.SX - PromeneKruh.XM,2) + Math.Pow(PromeneKruh.SY - PromeneKruh.YM,2)), 4);

            try
            {
                typmereni = "uhel";
                _sokkia.MeasAngle_ST1();
                test1.BackColor = Color.Green;//udělá se
                typmereni = "delka";
                _sokkia.MeasSD_ST2();
                Exit.BackColor = Color.Yellow;//udělá se
            }
            catch (Exception a)
            {
                Measure.BackColor = Color.Red;
                MessageBox.Show("exception " + a.ToString());
                _sokkia.StopMeasST();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}