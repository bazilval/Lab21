using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
/*1.    Имеется пустой участок земли (двумерный массив) и план сада, 
 * который необходимо реализовать. Эту задачу выполняют два садовника, 
 * которые не хотят встречаться друг с другом. Первый садовник начинает 
 * работу с верхнего левого угла сада и перемещается слева направо, сделав ряд, 
 * он спускается вниз. Второй садовник начинает работу с нижнего правого угла сада 
 * и перемещается снизу вверх, сделав ряд, он перемещается влево. 
 * Если садовник видит, что участок сада уже выполнен другим садовником, 
 * он идет дальше. Садовники должны работать параллельно. 
 * Создать многопоточное приложение, моделирующее работу садовников.*/

namespace Lab21
{
    class Program
    {
        public static string[,] landArray;
        public static string[,] gardenArray;
        public static string[] treesArray = { "@", "#", "&", "^" };
        public static string[] gardenersArray = { "Красный", "Зелёный" };
        public static bool[] readyArray = { false, false };
        public static int waiter;

        static object locker = new object();
        public static void Main(string[] args)
        {
            Console.Write("Введите размер стороны участка: ");
            int n = Int32.Parse(Console.ReadLine());
            landArray = new string[n, n];
            gardenArray = new string[n, n];
            Random r = new Random();
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    landArray[row, col] = "*";
                    gardenArray[row, col] = "" + treesArray[r.Next(0, 3)];
                }
            }

            Console.WriteLine("Вот каким сад должен быть:");
            PrintLand(gardenArray);
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("А вот какой он сейчас:");
            PrintLand(landArray);
            Console.WriteLine("\nЗапустим туда красного и зелёного садовников?");
            Console.Write("Введите сколько милисекунд дать каждому на ход: ");
            try
            {
                waiter = Int32.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Принято значение по умолчанию = 500 мс.");
                waiter = 500;
            }
            Console.WriteLine();

            for (int i = 0; i < 2; i++)
            {
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(GardenerMove);
                Thread thread = new Thread(threadStart);
                thread.Name = gardenersArray[i];
                thread.Start(n);
            }
            while (!readyArray[0] || !readyArray[1])
            {
               
            }
            Console.WriteLine("\nРабота над садом завершена! Нажми любую клавишу, чтобы выйти.\n");
            Console.ReadKey();

        }
        static void PrintLand(string[,] land)
        {
            int n = land.GetUpperBound(0) + 1;
            string areaInfo = "";
            for (int row = 0; row < n; row++)
            {
                Console.Write("|");
                for (int col = 0; col < n; col++)
                {
                    areaInfo = land[row, col];
                    Console.Write(areaInfo + "|");
                }
                Console.WriteLine();
            }

        }
        static void AjustArea(int n, int row, int col, string gardener)
        {
            int cur = Console.CursorTop;
            Console.SetCursorPosition(0,cur-1);
            ConsoleColor color = (gardener == "Красный") ? ConsoleColor.Red : ConsoleColor.Green;
            ConsoleColor color2 = (gardener == "Красный") ? ConsoleColor.Green : ConsoleColor.Red;
            Console.ForegroundColor = color;
            if (landArray[row, col] == "*")
            {
                landArray[row, col] = gardenArray[row, col];
                Console.Write($"{gardener} ");
                Console.ResetColor();
                Console.Write($"выполнил участок ");
                Console.ForegroundColor = color;
                Console.WriteLine($"({row + 1}:{col + 1})                              ");

                cur = Console.CursorTop;
                Console.SetCursorPosition(1 + col * 2, 4 + n + row);
                Console.Write(gardenArray[row, col]);
                Console.SetCursorPosition(0, cur);
            }
            else
            {
                Console.Write($"{gardener} ");
                Console.ResetColor();
                Console.Write($"наткнулся на готовый участок ");
                Console.ForegroundColor = color2;
                Console.WriteLine($"({row + 1}:{ col + 1})       ");
            }
            Console.ResetColor();
            Thread.Sleep(waiter);
        }

        static void GardenerMove(object n)
        {
            int N = (int)n;
            int tRowG, tColG, tRowR, tColR;

            if (Thread.CurrentThread.Name == "Зелёный")
            {
                for (int colG = 0; colG < N; colG++)
                {
                    for (int rowG = 0; rowG < N; rowG++)
                    {
                        tColG = N - 1 - colG;
                        tRowG = N - 1 - rowG;
                        if (((N - 1) % 2 != 0 && tColG % 2 == 0) || ((N - 1) % 2 == 0 && tColG % 2 != 0)) tRowG = N - 1 - tRowG;
                        lock(locker)
                        {
                            AjustArea(N, tRowG, tColG, Thread.CurrentThread.Name);
                        }
                    }
                }
                readyArray[0] = true;
            }
            else
            {
                for (int rowR = 0; rowR < N; rowR++)
                {
                    for (int colR = 0; colR < N; colR++)
                    {
                        tColR = colR;
                        tRowR = rowR;
                        if (rowR % 2 != 0) tColR = N - 1 - tColR;
                        lock (locker)
                        {
                            AjustArea(N, tRowR, tColR, Thread.CurrentThread.Name);
                        }
                    }
                }
                readyArray[1] = true;
            }

        }
    }
}
