using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System;
using System.IO;

namespace monacoEditorCSharp
{
    public class Program
    {
        public static int Main(string[] args)
        {

            try
            {
                StartWebHost();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
            finally
            {

            }
        }


        private static void StartWebHost()
        {

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
