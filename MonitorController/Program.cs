using System;

namespace MonitorInputController
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MonitorController controller = new MonitorController(1920 + 100);

                if (args.Length > 0)
                {
                    try
                    {
                        controller.SetMonitorInputSource(uint.Parse(args[0]));
                    }
                    catch (FormatException exc)
                    {
                        Console.WriteLine(exc);

                    }
                }
                else
                {
                    var maximumSource = controller.GetMonitorInputSourceCount();
                    var currentSource = controller.GetMonitorInputSource();
                    Console.WriteLine(currentSource);

                    string userCode;
                    do
                    {
                        Console.Write($"Input source port number [0-{maximumSource}] : ");
                        userCode = Console.ReadLine();
                        try
                        {
                            controller.SetMonitorInputSource(uint.Parse(userCode));
                        }
                        catch (FormatException exception)
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine(exception);
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    } while (!string.IsNullOrWhiteSpace(userCode));
                }
            }
            catch (Exception exc)
            {
#if DEBUG
                throw;
#endif
                Console.WriteLine(exc.ToString());
            }
        }
    }
}