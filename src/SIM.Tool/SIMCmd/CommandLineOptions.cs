using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SIM.Tool.SIMCmd
{
  public enum BooleanValue
  {
    False = 0,
    True = 1
  }

  public static class BooleanValueExtensions
  {
    public static bool IsSet(this BooleanValue value)
    {
      return value == BooleanValue.True;
    }
  }

  public class CommandLineArguments
  {
    public SimGlobalOptions Options { get; private set; }

    public string Pipeline { get; private set; }

    public int ParseOptions(string[] args)
    {
      var result = Parser.Default.ParseArguments<SimInstallOptions, SimDeleteOptions>(args).MapResult(
          (SimInstallOptions opts) => SetInstallOptions(opts),
          (SimDeleteOptions opts) => SetDeleteOptions(opts),
          errs => ErrorHandler(errs)
        );

      return 0;
    }

    public bool VerbIsValid()
    {
      return !string.IsNullOrEmpty(Pipeline);
    }

    private int ErrorHandler(IEnumerable<Error> err)
    {
      return 1;
    }

    private int SetInstallOptions(SimInstallOptions opts)
    {
      Pipeline = "install";
      Options = opts;

      return 0;
    }

    private int SetDeleteOptions(SimDeleteOptions opts)
    {
      Pipeline = "delete";
      Options = opts;

      return 0;
    }

  }

  public class SimGlobalOptions
  {
    [Option("LocalRepository", Required = true, HelpText = "SIM local repository")]
    public string LocalRepository { get; set; }

    [Option("InstanceRoot", Required = true, HelpText = "Sitecore instance directory")]
    public string InstanceRoot { get; set; }

    [Option("LicensePath", Required = true, HelpText = "Sitecore license file path")]
    public string LicensePath { get; set; }

    [Option("ConnectionString", Required = true, HelpText = "Sitecore connection string")]
    public string ConnectionString { get; set; }


    [Option("Questions", HelpText = "Pass answers to interactive questions from SIM as question=>answer")]

    public IEnumerable<SimCmdQuestion> Questions { get; set; }

    public virtual string ShowOptions()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("SIM Command global options");
      sb.AppendLine($"LocalRepository: {LocalRepository}");
      sb.AppendLine($"InstanceRoot: {InstanceRoot}");
      sb.AppendLine($"LicensePath: {LicensePath}");
      sb.AppendLine($"ConnectionString: {ConnectionString}");
      return sb.ToString();
    }
  }

  [Verb("install", HelpText = "install a new sitecore instance")]
  public class SimInstallOptions : SimGlobalOptions
  {
    [Option("InstanceName", Required = true, HelpText = "Sitecore instance name in IIS")]
    public string InstanceName { get; set; }

    [Option("RootDirectory", Required = true, HelpText = "Sitecore instance directory (must be a subdirectory of --InstanceRoot)")]
    public string RootDirectory { get; set; }

    [Option("SqlPrefix", Required = true, HelpText = "Prefix for Sitecore sql server databases")]
    public string SqlPrefix { get; set; }

    [Option("Hosts", Required = true, HelpText = "Sitecore domain names to assign to IIS and the local hosts file")]
    public IEnumerable<string> Hosts { get; set; }

    [Option("ProductVersion", Required = true, HelpText = "Sitecore product version to install")]
    public string ProductVersion { get; set; }

    [Option("ProductRevision", Required = true, HelpText = "Sitecore product revision to install")]
    public string ProductRevision { get; set; }

    [Option("WebIdentity", Default = "NetworkService", HelpText = "Identity of the sitecore instance AppPool")]
    public string WebIdentity { get; set; }

    [Option("SqlIdentity", Default = "NT AUTHORITY\\NETWORKSERVICE", HelpText = "Identity of the SQL Server service")]
    public string SqlIdentity { get; set; }

    [Option("Sitecore8Roles", Default = "", HelpText = "Sitecore 8 roles to install")]
    public string Sitecore8Roles { get; set; }

    [Option("Sitecore9Roles", Default = "", HelpText = "Sitecore 9 roles to install")]
    public string Sitecore9Roles { get; set; }

    [Option("SitecoreModules", HelpText = "Official sitecore modules to install")]
    public IEnumerable<string> SitecoreModules { get; set; }

    [Option("CustomModules", HelpText = "Custom sitecore modules to install")]
    public IEnumerable<string> CustomModules { get; set; }

    [Option("ConfigPresets", HelpText = "Sitecore configuration presets to install")]
    public IEnumerable<string> ConfigPresets { get; set; }

    [Option("AttachSql", Default = BooleanValue.True, HelpText = "Attach local sql databases if true")]
    public BooleanValue AttachSql { get; set; }

    [Option("ForceFramework4", Default = BooleanValue.True, HelpText = "Use .net 4 framework if true")]
    public BooleanValue ForceFramework4 { get; set; }

    [Option("Is32Bit", Default = BooleanValue.False, HelpText = "Use a 32bit app pool if true")]
    public BooleanValue Is32Bit { get; set; }

    [Option("IsClassic", Default = BooleanValue.False, HelpText = "Use classic mode for this app pool")]
    public BooleanValue IsClassic { get; set; }

    [Option("SkipInstallRadControls", Default = BooleanValue.False, HelpText = "Skip install of rapid application development tools for sitecore. Faster?")]
    public BooleanValue SkipInstallRadControls { get; set; }

    [Option("SkipInstallDictionaries", Default = BooleanValue.False, HelpText = "Skip install dictionaries for sitecore. Faster?")]
    public BooleanValue SkipInstallDictionaries { get; set; }

    [Option("ServerSideRedirects", Default = BooleanValue.False, HelpText = "ServerSideRedirects")]
    public BooleanValue ServerSideRedirects { get; set; }

    [Option("IncreaseExecutionTimeout", Default = BooleanValue.False, HelpText = "Increase the execution timeout for the website to 24 hours")]
    public BooleanValue IncreaseExecutionTimeout { get; set; }

    [Option("Preheat", Default = BooleanValue.True, HelpText = "Call http keepalive url after installation")]
    public BooleanValue Preheat { get; set; }

    public override string ShowOptions()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine(base.ShowOptions());
      sb.AppendLine("Running SIM install pipeline");
      sb.AppendLine($"InstanceName: {InstanceName}");
      sb.AppendLine($"RootDirectory: {RootDirectory}");
      sb.AppendLine($"SqlPrefix: {SqlPrefix}");
      sb.AppendLine($"Hosts: {Hosts.Count()}");
      foreach (var host in Hosts)
      {
        sb.AppendLine($"\t{host}");
      }
      sb.AppendLine($"ProductVersion: {ProductVersion}");
      sb.AppendLine($"ProductRevision: {ProductRevision}");
      sb.AppendLine($"WebIdentity: {WebIdentity}");
      sb.AppendLine($"SqlIdentity: {SqlIdentity}");
      sb.AppendLine($"Sitecore8Roles: {Sitecore8Roles}");
      sb.AppendLine($"Sitecore9Roles: {Sitecore9Roles}");
      sb.AppendLine($"SitecoreModules: {SitecoreModules.Count()}");
      foreach (var mod in SitecoreModules)
      {
        sb.AppendLine($"\t{mod}");
      }

      sb.AppendLine($"CustomModules: {CustomModules.Count()}");
      foreach (var mod in CustomModules)
      {
        sb.AppendLine($"\t{mod}");
      }

      sb.AppendLine($"ConfigPresets: {ConfigPresets.Count()}");
      foreach (var mod in ConfigPresets)
      {
        sb.AppendLine($"\t{mod}");
      }

      sb.AppendLine($"AttachSql: {AttachSql}");
      sb.AppendLine($"ForceFramework4: {ForceFramework4}");
      sb.AppendLine($"Is32Bit: {Is32Bit}");
      sb.AppendLine($"IsClassic: {IsClassic}");
      sb.AppendLine($"InstallRadControls: {SkipInstallRadControls}");
      sb.AppendLine($"InstallDictionaries: {SkipInstallDictionaries}");
      sb.AppendLine($"ServerSideRedirects: {ServerSideRedirects}");
      sb.AppendLine($"IncreaseExecutionTimeout: {IncreaseExecutionTimeout}");
      sb.AppendLine($"Preheat: {Preheat}");

      sb.AppendLine($"Questions: {Questions.Count()}");
      foreach (var mod in Questions)
      {
        sb.AppendLine($"\tQuestion: {mod.Question}");
        sb.AppendLine($"\tAnswer: {mod.Answer}");
      }


      return sb.ToString();
    }
  }

  [Verb("delete", HelpText = "Delete an existing sitecore instance")]
  public class SimDeleteOptions : SimGlobalOptions
  {
    [Option("InstanceName", Required = true, HelpText = "Instance name of the sitecore instance to delete")]
    public string InstanceName { get; set; }
  }

  public class SimCmdQuestion
  {
    public string Question { get; set; }

    public string Answer { get; set; }

    public SimCmdQuestion(string qna)
    {
      string[] splitqna = qna.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
      Question = splitqna[0];
      Answer = splitqna[1];
    }


  }
}
