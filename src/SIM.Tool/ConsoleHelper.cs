// ReSharper disable HeuristicUnreachableCode
// ReSharper disable CSharpWarnings::CS0162

using JetBrains.Annotations;
using Newtonsoft.Json;
using SIM.Core;
using SIM.Extensions;
using SIM.Instances;
using SIM.IO;
using SIM.IO.Real;
using SIM.Pipelines;
using SIM.Pipelines.Install;
using SIM.Pipelines.Processors;
using SIM.Products;
using Sitecore.Diagnostics.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using SIM.Tool.CmdLine;

namespace SIM.Tool
{
  public class ConsoleHelper
  {
    public static void Process(string[] args)
    {
      var config = SIMConsoleConfig.GetConfig(args[1]);

      SIMConsoleCommand command = config.FindCommand(args[2]);

      var simRunner = new ConsoleRunner
      {
        Settings = config.Settings
      };

      simRunner.Init();

      simRunner.RunCommand(command);
    }
  }
} 