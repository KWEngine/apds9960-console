using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APDS9960
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestConnection();
            //return;

            SerialPort p = new SerialPort("COM3");
            if (p.IsOpen)
                p.Close();
            p.Open();

            byte[] writeAnswer = new byte[1];


            byte[] readId = new byte[] { 0x55, (0x39 << 1) + 1, 0x92, 0x01 };
            byte[] readIdReturnValue = new byte[1];

            p.Write(readId, 0, readId.Length);
            p.Read(readIdReturnValue, 0, readIdReturnValue.Length);

            Console.WriteLine("Connected to sensor id: " + AsHex(readIdReturnValue[0]));



            // CONFIG 0xA1
            byte configRegister0xA1_bits = 0b00000000;
            Console.Write("Writing " + AsBits<byte>(configRegister0xA1_bits) + " to register 0xA1...");
            byte[] configRegister0xA1 = new byte[] { 0x55, 0x39 << 1, 0xA1, 1, 0b00000000 };
            p.Write(configRegister0xA1, 0, configRegister0xA1.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xA1"));

            // CONFIG 0xA2
            byte configRegister0xA2_bits = 0b00000000;
            Console.Write("Writing " + AsBits<byte>(configRegister0xA2_bits) + " to register 0xA2...");
            byte[] configRegister0xA2 = new byte[] { 0x55, 0x39 << 1, 0xA2, 1, configRegister0xA2_bits };
            p.Write(configRegister0xA2, 0, configRegister0xA2.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xA2"));

            // CONFIG 0xA3
            // 011: gain 8x (000 for 1x)
            //  00: 100mA (11 for 12.5mA)
            // 111: 40ms wait time // 000 = 0ms
            byte configRegister0xA3_bits = 0b01111000;
            Console.Write("Writing " + AsBits<byte>(configRegister0xA3_bits) + " to register 0xA3...");
            byte[] configRegister0xA3 = new byte[] { 0x55, 0x39 << 1, 0xA3, 1, configRegister0xA3_bits };
            p.Write(configRegister0xA3, 0, configRegister0xA3.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xA3"));

            // CONFIG 0xA6
            //      00: 8microseconds
            //  111111: 64 pulses (000000 for 1 pulse)
            byte configRegister0xA6_bits = 0b00111111;
            Console.Write("Writing " + AsBits<byte>(configRegister0xA6_bits) + " to register 0xA6...");
            byte[] configRegister0xA6 = new byte[] { 0x55, 0x39 << 1, 0xA6, 1, configRegister0xA6_bits };
            p.Write(configRegister0xA6, 0, configRegister0xA6.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xA6"));

            // CONFIG 0xAA
            //  000000: reserved as 0
            //      00: all directions active
            byte configRegister0xAA_bits = 0b00000000;
            Console.Write("Writing " + AsBits<byte>(configRegister0xAA_bits) + " to register 0xAA...");
            byte[] configRegister0xAA = new byte[] { 0x55, 0x39 << 1, 0xAA, 1, configRegister0xAA_bits };
            p.Write(configRegister0xAA, 0, configRegister0xAA.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xAA"));

            // CONFIG 0xAB
            //   00000: reserved as 0
            //       1: do a fifo list clear
            //       0: interrupt disabled
            //       1: enter gesture mode immediately
            byte configRegister0xAB_bits = 0b00000101;
            Console.Write("Writing " + AsBits<byte>(configRegister0xAB_bits) + " to register 0xAB...");
            byte[] configRegister0xAB = new byte[] { 0x55, 0x39 << 1, 0xAB, 1, configRegister0xAB_bits };
            p.Write(configRegister0xAB, 0, configRegister0xAB.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0xAB"));

            // POWER ON
            byte configRegisterPowerOn_bits = 0b01000001;
            Console.Write("Writing " + AsBits<byte>(configRegisterPowerOn_bits) + " to register 0x80...");
            byte[] powerOn = new byte[] { 0x55, 0x39 << 1, 0x80, 1, configRegisterPowerOn_bits };
            p.Write(powerOn, 0, powerOn.Length);
            p.Read(writeAnswer, 0, writeAnswer.Length);
            Console.WriteLine(writeAnswer[0] == 1 ? "OK" : throw new Exception("unable to set register 0x80"));

            Console.WriteLine("---------------");

            while (true)
            {
                // check if valid entries are available:
                byte[] read0xAF = new byte[] { 0x55, (0x39 << 1) + 1, 0xAF, 0x01 };
                byte[] read0xAFValue = new byte[1];
                p.Write(read0xAF, 0, read0xAF.Length);
                p.Read(read0xAFValue, 0, read0xAFValue.Length);

                bool validDataAvailable = read0xAFValue[0] % 2 == 1;
                if (validDataAvailable)
                {
                    // READ how many entries are listed:
                    //0xAE
                    byte[] read0xAE = new byte[] { 0x55, (0x39 << 1) + 1, 0xAE, 0x01 };
                    byte[] read0xAEValue = new byte[1];

                    p.Write(read0xAE, 0, read0xAE.Length);
                    p.Read(read0xAEValue, 0, read0xAEValue.Length);
                    byte datasetcount = read0xAEValue[0];
                    if (debug)
                    {
                        Console.WriteLine("Dataset count: " + datasetcount);
                    }

                    byte[] upDB = new byte[256];
                    byte[] downDB = new byte[256];
                    byte[] leftDB = new byte[256];
                    byte[] rightDB = new byte[256];
                    int j = 0;
                    while (datasetcount > 0)
                    {
                        // read gesture set:
                        byte[] read0xFC = new byte[] { 0x55, (0x39 << 1) + 1, 0xFC, 0x04 };
                        byte[] read0xFCValue = new byte[4];

                        p.Write(read0xFC, 0, read0xFC.Length);
                        p.Read(read0xFCValue, 0, read0xFCValue.Length);


                        byte up = read0xFCValue[0];
                        byte down = read0xFCValue[1];
                        byte left = read0xFCValue[2];
                        byte right = read0xFCValue[3];

                        upDB[j] = up;
                        downDB[j] = down;
                        leftDB[j] = left;
                        rightDB[j] = right;
                        j++;
                        datasetcount -= 1;
                    }


                    byte u_first = 0;
                    byte d_first = 0;
                    byte l_first = 0;
                    byte r_first = 0;
                    byte u_last = 0;
                    byte d_last = 0;
                    byte l_last = 0;
                    byte r_last = 0;

                    for (int i = 0; i < upDB.Length; i++)
                    {
                        if (upDB[i] >= THRESHOLD && downDB[i] >= THRESHOLD && leftDB[i] >= THRESHOLD && rightDB[i] >= THRESHOLD)
                        {
                            u_first = upDB[i];
                            d_first = downDB[i];
                            l_first = leftDB[i];
                            r_first = rightDB[i];
                            break;
                        }
                    }

                    if (u_first == 0 || d_first == 0 || l_first == 0 || r_first == 0)
                    {
                        System.Threading.Thread.Sleep(10);
                        continue;
                    }

                    for (int i = upDB.Length - 1; i >= 0; i--)
                    {
                        if (upDB[i] >= THRESHOLD && downDB[i] >= THRESHOLD && leftDB[i] >= THRESHOLD && rightDB[i] >= THRESHOLD)
                        {
                            u_last = upDB[i];
                            d_last = downDB[i];
                            l_last = leftDB[i];
                            r_last = rightDB[i];
                            break;
                        }
                    }

                    int ud_ratio_first = ((u_first - d_first) * 100) / (u_first + d_first);
                    int lr_ratio_first = ((l_first - r_first) * 100) / (l_first + r_first);
                    int ud_ratio_last = ((u_last - d_last) * 100) / (u_last + d_last);
                    int lr_ratio_last = ((l_last - r_last) * 100) / (l_last + r_last);

                    int ud_delta = ud_ratio_last - ud_ratio_first;
                    int lr_delta = lr_ratio_last - lr_ratio_first;

                    // Misst die absolute Differenz zwischen UP/DOWN- und LEFT/RIGHT-Werten:
                    // Beispiel:
                    //      -> ud_delta ist "-54"
                    //      -> lr_delta ist "-48"
                    //      -> Absolute Differenz ist 54 - 48 = 6
                    //      -> Nur wenn die abs. Diff. größer als ein Schwellenwert ist (z.B. 10)
                    //         konnte die Geste eindeutig als oben/unten bzw. links/rechts erkannt werden.
                    int differenceUDLR = Math.Abs(Math.Abs(ud_delta) - Math.Abs(lr_delta));

                    // Wenn die absolute Differenz groß genug ist, und wenn der UD-Wert groß genug ist
                    // UND wenn der absolute UD-Wert größer als der LR-Wert ist, dann:
                    if (differenceUDLR > K2THRESHOLD && Math.Abs(ud_delta) >= KTHRESHOLD && Math.Abs(ud_delta) > Math.Abs(lr_delta))
                    {
                        if (ud_delta < 0)
                            Console.WriteLine("DOWN");
                        else if (ud_delta > 0)
                            Console.WriteLine("UP");
                    }
                    // Gleiches für LR:
                    if (differenceUDLR > K2THRESHOLD && Math.Abs(lr_delta) >= KTHRESHOLD && Math.Abs(ud_delta) < Math.Abs(lr_delta))
                    {
                        if(lr_delta < 0)
                            Console.WriteLine("RIGHT");
                        else if(lr_delta > 0)
                            Console.WriteLine("LEFT");
                    }

                    System.Threading.Thread.Sleep(10); // Sleep kann variieren - alles unter 16ms ist nicht garantiert!
                }
            }

            p.Close();
        }

        private const byte THRESHOLD = 64;      // Grenze für einzelne Messwerte aus der FIFO-Queue
        private const byte K2THRESHOLD = 10;    // Grenze für die absolute Differenz zwischen UD/LR-Werten
        private const byte KTHRESHOLD = 10;     // Grenze für die berechneten Messwerte
        private static bool debug = false;


        // Methode mit Template-Parameter
        // Aufruf z.B. so: 
        //  string x = AsBits<byte>(54);
        //  (die spitze Klammer beinhaltet den Typ, der übergeben wird)
        private static string AsBits<T>(T value)
        {
            if (value is int)
            {
                int x = (int)(object)value;
                return Convert.ToString(x, 2).PadLeft(32, '0');
            }
            else if (value is short)
            {
                short x = (short)(object)value;
                return Convert.ToString(x, 2).PadLeft(16, '0');
            }
            else if (value is byte)
            {
                byte x = (byte)(object)value;
                return Convert.ToString(x, 2).PadLeft(8, '0');
            }
            else
                throw new Exception("invalid type cast.");
        }

        private static string AsHex<T>(T value)
        {
            if (value is int)
            {
                int x = (int)(object)value;
                return Convert.ToString(x, 16).PadLeft(8, '0').ToUpper();
            }
            else if (value is short)
            {
                short x = (short)(object)value;
                return Convert.ToString(x, 16).PadLeft(4, '0').ToUpper();
            }
            else if (value is byte)
            {
                byte x = (byte)(object)value;
                return Convert.ToString(x, 16).PadLeft(2, '0').ToUpper();
            }
            else
                throw new Exception("invalid type cast.");
        }
    }
}
