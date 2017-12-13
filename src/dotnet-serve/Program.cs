using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace McMaster.DotNet.Server
{
    class Program
    {
        // Return codes
        private const int ERROR = 2;
        private const int OK = 0;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                return await CommandLineApplication.ExecuteAsync<SimpleServer>(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unexpected error: " + ex.ToString());
                Console.ResetColor();
                return ERROR;
            }
        }
    }
}
