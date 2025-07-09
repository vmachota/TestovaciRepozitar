//using System;
//
//using System.Collections.Generic;
//using System.Text;
//
//namespace TotalApp
//{
//    public class MeriniFce
//    {
//        
//    }
//}
//
//
//
// Decompiled with JetBrains decompiler
// Type: GeoTEL.SokkiaCom
// Assembly: Athletics, Version=3.8.5.11, Culture=neutral, PublicKeyToken=null
// MVID: A86888DC-3ACC-4154-A84D-5327A6642F7B
// Assembly location: D:\Downloads\atletika-topcon-hacking\athletics\Athletics.exe
//END//


using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace GeoTEL
{
    internal class SokkiaCom : IDisposable
    {
        private SerialPort _port;
        private System.Threading.Timer timeWait;
        private char[] buffer = new char[300];
        private int DataLenght;
        private int buferIndex;
        private bool _timeElapsed;
        private bool resultWait;
        private bool ConnectFalse = true;

        public event DataReceived DataOUT;

        public SokkiaCom()
        {
            this.timeWait = new System.Threading.Timer(new TimerCallback(this.timeWait_Tick), (object)null, -1, 10000);
            this._port = new SerialPort("COM7", 115200);
            this._port.Open();
            this._port.DataReceived += new SerialDataReceivedEventHandler(this._port_Connect);
        }

        public void Dispose()
        {
            try
            {
                if (this._port.IsOpen)
                    this._port.Close();
                this._port.Dispose();
                this.buffer = (char[])null;
            }
            catch (Exception ex)
            {
            }
        }

        public bool Connect(string ProgramName)
        {
            try
            {
                while (true)
                {
                    this._port.Write("Tl " + ProgramName + "\r");
                    this.Wait(10000);
                    if (!this.resultWait)
                    {
                        if (this.ConnectFalse)
                        {
                            this.ConnectFalse = false;
                            this._port.Write("Tc\r");
                            Thread.Sleep(500);
                        }
                        else
                            goto label_4;
                    }
                    else
                        break;
                }
                this._port.DataReceived -= new SerialDataReceivedEventHandler(this._port_Connect);
                this._port.DataReceived += new SerialDataReceivedEventHandler(this._port_DataReceived);
                return true;
            label_4:
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            this.Set_StatusBar(0);
            this._port.Write("Tc\r");
            Application.DoEvents();
            this._port.Close();
            this._port.DataReceived -= new SerialDataReceivedEventHandler(this._port_DataReceived);
        }

        private void _port_Connect(object s, SerialDataReceivedEventArgs e)
        {
            string str = this._port.ReadExisting();
            if (str == "Tl " + (object)'\u0006')
                this._port.Write("T1 \r");
            else if (str == "T1 " + (object)'\u0006')
                this._port.Write("T2 \r");
            else if (str == "T2 " + (object)'\u0006')
            {
                this.resultWait = true;
                this.StopWait();
            }
            else
                this.StopWait();
        }

        private void _port_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            try
            {
                int count = this._port.BytesToRead;
                if (count <= 0)
                    return;
                if (this.buferIndex + count > 300)
                {
                    this.ClearBuffer();
                    count = 299;
                }
                this._port.Read(this.buffer, this.buferIndex, count);
                this.buferIndex += count;
                int typ = 0;
                int num = this.Search(this.buffer, out typ);
                //MessageBox.Show(num.ToString() + " search");
                if (num == -1 || num == 0)
                    return;
                int length = num + 1;
                char[] chArray = new char[length];
                try
                {
                    Array.Copy((Array)this.buffer, (Array)chArray, length);
                    this.DataOUT(chArray, typ);
                    this.ClearBuffer();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("error " + ee.ToString());
                }
                //MessageBox.Show("data out called");
            }
            catch (Exception eeee)
            {
                MessageBox.Show("error 2xxx " + eeee.ToString());
                this.ClearBuffer();
            }
        }

        private void ClearBuffer()
        {
            Array.Clear((Array)this.buffer, 0, 300);
            this.buferIndex = 0;
        }

        private int Search(char[] source, out int typ)
        {
            try
            {
                int num1 = Array.IndexOf<char>(source, '\n');
                if (num1 > -1)
                {
                    typ = 1;
                    return num1;
                }
                int num2 = Array.IndexOf<char>(source, '\u0006');
                if (num2 > -1)
                {
                    typ = 2;
                    return num2;
                }
                int num3 = Array.IndexOf<char>(source, '\u0015');
                if (num3 > -1)
                {
                    typ = 3;
                    return num3;
                }
                typ = 0;
                return -1;
            }
            catch
            {
                typ = 0;
                return -1;
            }
        }

        private void Wait(int timervalue)
        {
            this._timeElapsed = false;
            this.resultWait = false;
            this.timeWait.Change(timervalue, timervalue);
            while (!this._timeElapsed)
                Application.DoEvents();
        }

        private void timeWait_Tick(object sender)
        {
            this._timeElapsed = true;
            this.timeWait.Change(-1, 1);
        }

        private void StopWait()
        {
            this.timeWait_Tick((object)this);
        }

        public void MeasAngle_ST1()
        {
            this._port.Write("*ST1\r");
        }

        public void MeasSD_ST2()
        {
            this._port.Write("*ST2\r");
        }

        public void StopMeasST()
        {
            this._port.Write("*ST0\r");
            Thread.Sleep(200);
        }

        public void AutomaticTarget() { this._port.Write("*SJ000000\r"); }

        public void Rotate2Face() { this._port.Write("*DHF\r"); }

        public void RotateToAngle(double ha, double va)
        {
            ha *= 10000.0;
            va *= 10000.0;
            this._port.Write("*DHA" + ha.ToString().PadLeft(7, '0') + "VA" + va.ToString().PadLeft(7, '0') + "\r");
        }

        public void StopRotate() { this._port.Write("*R\r"); }

        public void InstrumentID() { this._port.Write("A\r"); }

        public void InstrumentGetParameter() { this._port.Write("B\r"); }

        public void InstrumentSetParameter(string par) { this._port.Write("/B " + par + "\r"); }

        public void EdmGet() { this._port.Write("C\r"); }

        public void EdmSet(int mode) { this._port.Write("/C " + mode.ToString() + "\r"); }

        public void Set_HzAngle(double angle) { this._port.Write("/Dc " + angle.ToString() + "\r"); }

        public void Dist_Fine_S() { this._port.Write("Xa\r"); }

        public void Dist_Fine_R() { this._port.Write("Xb\r"); }

        public void Dist_Rapid_S() { this._port.Write("Xc\r"); }

        public void Dist_Rapid_R() { this._port.Write("Xd\r"); }

        public void Dist_Tracking() { this._port.Write("Xe\r"); }

        public void Set_HZ_0() { this._port.Write("Xh\r"); }

        public void Set_StatusBar(int mode) { this._port.Write("Ts " + mode.ToString() + "\r"); }

        public void Set_DisplayMode(int mode) { this._port.Write("Tf " + mode.ToString() + "\r"); }

        public void Set_NotificationSwitchProgramMode(int mode)
        {
            this._port.Write("Tk " + mode.ToString() + "\r");
        }

        public void Get_CommPortNumber()
        {
            this._port.Write("Tp\r");
        }

        public void Get_Atmospheric() { this._port.Write("Di\r"); }

        public void Set_Atmospheric(string par) { this._port.Write("/Di " + par + "\r"); }
    }
}
