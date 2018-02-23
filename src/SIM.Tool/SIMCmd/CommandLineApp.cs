using SIM.Adapters.WebServer;
using SIM.Extensions;
using SIM.Instances;
using SIM.IO;
using SIM.IO.Real;
using SIM.Pipelines;
using SIM.Pipelines.Delete;
using SIM.Pipelines.Install;
using SIM.Products;
using Sitecore.Diagnostics.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SIM.Tool.SIMCmd;

namespace SIM.Tool.SIMCmd
{
  public class CommandLineApp
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

    private CommandLineArguments CmdSettings;

    public ConsolePipelineController controller = new ConsolePipelineController();

    private RealFileSystem fileSystem;

    private IFile license;

    private SqlConnectionStringBuilder connection;

    public CommandLineApp()
    {

    }

    public void RunCommand(CommandLineArguments options)
    {
      if (options.VerbIsValid())
      {
        Initialize(options);

        switch (CmdSettings.Pipeline)
        {
          case "install":

            var installOpts = CmdSettings.Options as SimInstallOptions;

            if (InstallArgumentsAreValid(installOpts))
            {
              InstallSitecoreInstance(installOpts);
            }
            break;

          case "delete":
            var deleteOpts = CmdSettings.Options as SimDeleteOptions;

            DeleteSitecoreInstance(deleteOpts);
            break;

          default:
            break;
        }
      }
      else
      {
        Console.WriteLine($"No valid options given...");
      }
    }

    private void Initialize(CommandLineArguments options)
    {
      CmdSettings = options;

      fileSystem = new RealFileSystem();

      license = fileSystem.ParseFile(CmdSettings.Options.LicensePath);

      ProductManager.Initialize(CmdSettings.Options.LocalRepository);

      InstanceManager.Default.Initialize(CmdSettings.Options.InstanceRoot);

      var pipelinesConfig = XmlDocumentEx.LoadXml(PipelinesConfig.Contents);

      var resultPipelinesNode = pipelinesConfig.SelectSingleNode("/pipelines") as XmlElement;

      Assert.IsNotNull(resultPipelinesNode, "Can't find pipelines configuration node");

      PipelineManager.Initialize(resultPipelinesNode);

      ConsolePipelineController controller = new ConsolePipelineController();

      connection = new SqlConnectionStringBuilder(CmdSettings.Options.ConnectionString);
    }

    private IEnumerable<Product> GetModules(Product mainProduct, SimInstallOptions main)
    {
      List<Product> scModules = new List<Product>();

      foreach (var scModule in main.SitecoreModules)
      {
        var modules = ProductManager.Modules.Where(m => m.IsMatchRequirements(mainProduct)).OrderByDescending(m => m.SortOrder);

        var scProd = modules.Where(module => module.Name.EqualsIgnoreCase(scModule)).FirstOrDefault();

        if (scProd != null)
        {
          scModules.Add(scProd);
        }

      }

      foreach (var module in main.CustomModules)
      {
        var path = File.Exists(module) ? module : Path.Combine(CmdSettings.Options.LocalRepository, module);

        if (!File.Exists(path)) continue;

        string filename = Path.GetFileName(path);

        FileSystem.FileSystem.Local.File.Copy(path, Path.Combine(ApplicationManager.FilePackagesFolder, filename), true);

        CopyManifestIfPresent(path, filename);

        if (File.Exists(path))
        {
          scModules.Add(Product.GetFilePackageProduct(Path.Combine(ApplicationManager.FilePackagesFolder, filename)));
        }
      }


      List<string> moduleProducts = new List<string>();

      foreach (var preset in main.ConfigPresets)
      {
        string filename = FileSystem.FileSystem.Local.Directory.GetFiles("Configurations", "*.zip", SearchOption.AllDirectories)
        .Where(f => false || f.ContainsIgnoreCase(preset)).FirstOrDefault();

        if (!string.IsNullOrEmpty(filename))
        {
          scModules.Add(Product.GetFilePackageProduct(filename));
        }
      }

      return scModules;
    }

    private static void CopyManifestIfPresent(string path, string filename)
    {
      string manifestFilename = Path.GetFileNameWithoutExtension(filename) + ".manifest.xml";

      string manifestPath = Path.Combine(Path.GetDirectoryName(path), manifestFilename);

      if (File.Exists(manifestPath))
      {
        FileSystem.FileSystem.Local.File.Copy(manifestPath, Path.Combine(ApplicationManager.FilePackagesFolder, manifestFilename), true);
      }
    }

    private Product GetProduct(string version, string revision)
    {
      return Products.ProductManager.FindProduct(ProductType.Standalone, "Sitecore CMS", version, revision);
    }

    private void InstallSitecoreInstance(SimInstallOptions opts)
    {
      var instance = opts.InstanceName;

      var product = GetProduct(opts.ProductVersion,opts.ProductRevision);

      var modules = GetModules(product, opts);

      var installArgs = new InstallArgs(
        opts.InstanceName, opts.Hosts.ToArray(), opts.SqlPrefix, opts.AttachSql.IsSet(),
        product,
        fileSystem.ParseFolder(opts.RootDirectory),
        connection,
        opts.SqlIdentity,
        opts.WebIdentity,
        license,
        opts.ForceFramework4.IsSet(),
        opts.Is32Bit.IsSet(),
        opts.IsClassic.IsSet(),
        opts.SkipInstallRadControls.IsSet(), opts.SkipInstallDictionaries.IsSet(), opts.ServerSideRedirects.IsSet(), 
        opts.IncreaseExecutionTimeout.IsSet(), opts.Preheat.IsSet(), 
        opts.Sitecore8Roles, opts.Sitecore9Roles,
        modules
     );

      ConsolePipelineController controller = new ConsolePipelineController(opts.Questions.ToList());

      PipelineManager.StartPipeline("install", installArgs, controller, false);
    }

    private bool InstallArgumentsAreValid(SimInstallOptions opts)
    {
      if(WebServerManager.WebsiteExists(opts.InstanceName))
      {
        Console.WriteLine($"Website Name {opts.InstanceName} already exists... skipping install task");
        return false;
      }

      foreach (var host in opts.Hosts)
      {
        var hostExists = WebServerManager.HostBindingExists(host);
        if (hostExists)
        {
          Console.WriteLine($"Host Name {host} already exists... skipping install task");
          return false;
        } 
      }
      return true;
    }

    private void DeleteSitecoreInstance(SimDeleteOptions deleteOpts)
    {
      Instance inst = InstanceManager.Default.GetInstance(deleteOpts.InstanceName);

      if (inst != null)
      {
        var deleteArgs = new DeleteArgs(inst, connection);

        ConsolePipelineController controller = new ConsolePipelineController(deleteOpts.Questions.ToList());

        PipelineManager.StartPipeline("delete", deleteArgs, controller, false);
      }
      else
      {
        Console.WriteLine($"Website Name {deleteOpts.InstanceName} not found... skipping delete task");
      }
    }

  }
}
