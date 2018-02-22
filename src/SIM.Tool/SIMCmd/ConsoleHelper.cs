using SIM.Tool.SIMCmd;
using System.Linq;

namespace SIM.Tool
{
  public class ConsoleHelper
  {
    public static void Process(string[] args)
    {
      CommandLineArguments options = new CommandLineArguments();

      if (options.ParseOptions(args.Skip(1).ToArray()) == 0)
      {
        CommandLineApp console = new CommandLineApp();
        console.RunCommand(options);
      }
    }
  }
} 