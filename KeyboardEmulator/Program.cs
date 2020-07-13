using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardEmulator
{
    class Program
    {

        static int Delay = 40;



        [DllImport("User32.dll")] // подключение системной библиотеки
        public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize); // функция, которая отправляет запрос на идентификацию и активацию действии клавиши

        [StructLayout(LayoutKind.Sequential)] //Для ручного расположения полей в памяти в порядке объявления
        public struct MOUSEINPUT // структура для эмуляции мыши
        {
            int dx;
            int dy;
            uint mouseData;
            uint dwFlags;
            uint time;
            IntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)] //Для ручного расположения полей в памяти в порядке объявления
        public struct KEYBDINPUT // структура для эмуляции клавиатуры
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)] //Для ручного расположения полей в памяти в порядке объявления
        public struct HARDWAREINPUT // структура для эмуляции подключённого внешнего устройства
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        };

        [StructLayout(LayoutKind.Explicit)] //Для указания размера каждого поля
        public struct INPUT // структура для выбора типа устройства
        {
            [FieldOffset(0)] // задаем 0 байт для переменной type
            public int type;
            [FieldOffset(4)] // задаем 4 байта для структуры MOUSEINPUT
            public MOUSEINPUT mouse;
            [FieldOffset(4)] // задаем 4 байта для структуры KEYBDINPUT
            public KEYBDINPUT keyboard;
            [FieldOffset(4)] // задаем 4 байта для структуры HARDWAREINPUT
            public HARDWAREINPUT hardware;
        };
        const uint KEYEVENTF_KEYUP = 0x0002; // событие Up
        const uint KEYEVENTF_SCANCODE = 0x0008; // событие Down + установка типа выбора определения клавиши

        public struct ConvertResult
        {
            public bool isLitter;
            public bool isUpper;
            public uint key;
        }

        static string shifts = "!@#$%^&*()_+<>:~?|\"";
        static string shiftv = "1234567890-=,.;`/\\'";

        public static ConvertResult ConvertCharToVirtualKey(char ch)
        {
            
            var result = new ConvertResult();
            result.isLitter = ch.ToString().ToUpper() != ch.ToString().ToLower();
            result.isUpper = result.isLitter && (ch.ToString().ToUpper() == ch.ToString());

            //Enter
            if ( ch == '\n' )
            {
                result.key = 28;
                return result;
            }

            //Tab
            if (ch == '\t')
            {
                result.key = 15;
                return result;
            }

            //only shift symbols
            if (shifts.IndexOf(ch) >= 0)
            {
                result.isUpper = true;
                ch = shiftv[shifts.IndexOf(ch)];
            }

            //find symbol
            var s = ch.ToString().ToUpper();
            for(uint i =0; i < lines.Length; i++)
            {
                if( s == lines[i])
                {
                    result.key = i;
                    return result;
                }
            }

            //return $ if symbol not found
            result.isUpper = true;
            result.key = 5;
            return result;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        static void typeSymbol(char symbol)
        {
            INPUT[] inputs = new INPUT[1];


            var key = ConvertCharToVirtualKey(symbol);

            //shift down
            if (key.isUpper)
            {
                System.Threading.Thread.Sleep(Delay);
                inputs[0].type = 1; // выбрана клавиатура
                inputs[0].keyboard.dwFlags = KEYEVENTF_SCANCODE; // Down
                inputs[0].keyboard.wScan = 42; // скэн код клавиши
                SendInput(1, inputs, Marshal.SizeOf(inputs[0])); // эмулируем нажатие
            }

            System.Threading.Thread.Sleep(Delay);
            pressKey(key.key);

            if (key.isUpper
                )
            {
                System.Threading.Thread.Sleep(Delay);
                inputs[0].type = 1;
                inputs[0].keyboard.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
                inputs[0].keyboard.wScan = 42;
                SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
            }
        }

        static void pressKey(uint key)
        {

            INPUT[] inputs = new INPUT[1];

            inputs[0].type = 1;
            inputs[0].keyboard.dwFlags = KEYEVENTF_SCANCODE;
            inputs[0].keyboard.wScan = (ushort)key;
            SendInput(1, inputs, Marshal.SizeOf(inputs[0]));

            inputs[0].type = 1;
            inputs[0].keyboard.wScan = (ushort)key;
            inputs[0].keyboard.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP; // Up 
            SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
        }

        static string[] lines;

        static void Main(string[] args)
        {

            lines = File.ReadLines("keys.csv").ToArray();


            for (int i = 10; i > 0; i--)
            {
                Console.WriteLine(i);
                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("Go!");


            var text = File.ReadAllText("input.txt");

            foreach (char ch in text)
            {
                if(ch != '\r')
                {
                    typeSymbol(ch);
                }
                
            }

            //for(int i = 3; i <= 255; i++)
            //{
            //    pressKey(i);
            //    pressKey(2);
            //}


            Console.WriteLine("Finish");
            Console.ReadKey();
        }
    }
}
