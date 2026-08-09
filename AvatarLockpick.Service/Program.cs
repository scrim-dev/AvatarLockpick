using System.Threading;

namespace AvatarLockpick.Service
{
    public class Program
    {
        public const string AppVersion = "2026.08.09-3";

        public static void Main(string[] args)
        {
            using var mutex = new Mutex(true, "AvatarLockpick.Service", out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine("AvatarLockpick.Service is already running. Exiting.");
                return;
            }

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
