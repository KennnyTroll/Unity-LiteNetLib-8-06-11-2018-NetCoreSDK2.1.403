using System;
using System.Threading;
using System.Globalization;

namespace Server_LiteNetLib_8_Net_Core
{
    class Program
    {
        static void EventInit()
        {
            while (TheServer._netManager.IsRunning && (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Add && Console.ReadKey(true).Key == ConsoleKey.Enter)))
            {
                TheServer._netManager.PollEvents();

                Globals._Server.SendPlayerPositions();

                Thread.Sleep(Config.Send_Fz);
            }
            TheServer._netManager.Stop();
        }

        static void Main(string[] args)
        {
            if (!TheServer._netManager.Start(Config.Serveur_Port))
            {
                Console.WriteLine($"({Config.Serv_name}) SERVER INIT FAILED!");
                return;
            }
            else
            {
                Console.WriteLine($">> ({Config.Serv_name}) SERVER : ON Port {Config.Serveur_Port} | DATE {DateTime.Now.ToString("dd/M/yyyy MM:ss tt", CultureInfo.InvariantCulture)})");
                Globals._Server.InitializePackets();
                EventInit();
            }
        }
    }
}
