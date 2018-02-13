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

namespace SIM.Tool.CmdLine
{
  public class ConsoleRunner
  {
    public SIMConsoleSettings Settings { get; set; }

    public ConsolePipelineController controller = new ConsolePipelineController();

    private RealFileSystem fileSystem;

    private IFile license;

    private SqlConnectionStringBuilder connection;

    public ConsoleRunner()
    {
      fileSystem = new RealFileSystem();
    }

    public void Init()
    {
      license = fileSystem.ParseFile(Settings.LicensePath);

      ProductManager.Initialize(Settings.LocalRepository);

      InstanceManager.Default.Initialize(Settings.InstanceRoot);

      var pipelinesConfig = XmlDocumentEx.LoadXml(PipelinesConfig.Contents);

      var resultPipelinesNode = pipelinesConfig.SelectSingleNode("/pipelines") as XmlElement;

      Assert.IsNotNull(resultPipelinesNode, "Can't find pipelines configuration node");

      PipelineManager.Initialize(resultPipelinesNode);

      ConsolePipelineController controller = new ConsolePipelineController();

      connection = new SqlConnectionStringBuilder(Settings.ConnectionString);
    }


    public IEnumerable<Product> GetModules(SIMConsoleProduct main)
    {
      List<string> moduleProducts = new List<string>();

      foreach (var preset in main.ConfigPresets)
      {
        string filename = FileSystem.FileSystem.Local.Directory.GetFiles("Configurations", "*.zip", SearchOption.AllDirectories)
        .Where(f => false || f.ContainsIgnoreCase(preset)).FirstOrDefault();

        if (!string.IsNullOrEmpty(filename))
        {
          moduleProducts.Add(filename);
        }
      }

      return moduleProducts.Select(g => Product.GetFilePackageProduct(g)).ToList();
    }

    public Product GetProduct(SIMConsoleProduct main)
    {
      return Products.ProductManager.FindProduct(ProductType.Standalone, main.Name, main.Version, main.Revision);
    }

    public void RunCommand(SIMConsoleCommand cmd)
    {
      if (!CommandIsValid(cmd))
      {
        return;
      }
      if (cmd.Action == "install")
      {
        var instance = cmd.Instance;        
        var opts = cmd.Options;
        var prd = GetProduct(cmd.Product);
        var modules = GetModules(cmd.Product);

        var installArgs = new InstallArgs(
          instance.Name, instance.Host.ToArray(), instance.SqlPrefix, instance.AttachSql,
          prd,
          fileSystem.ParseFolder(instance.RootDir),
          connection,
          instance.SqlIdentity,
          instance.WebIdentity,
          license,
          instance.ForceFramework4,
          instance.Is32Bit,
          instance.IsClassic,
          opts.InstallRadControls, opts.InstallDictionaries, opts.ServerSideRedirects, opts.IncreaseExecutionTimeout, opts.Preheat, opts.Sitecore8Roles, opts.Sitecore9Roles,
          modules
       );

        ConsolePipelineController controller = new ConsolePipelineController(cmd.Answers);

        PipelineManager.StartPipeline("install", installArgs, controller, false);
      }else if(cmd.Action == "delete")
      {
        Instance inst = InstanceManager.Default.GetInstance(cmd.Instance.Name);

        if (inst != null)
        {
          var deleteArgs = new DeleteArgs(inst, connection);
          ConsolePipelineController controller = new ConsolePipelineController(cmd.Answers);
          PipelineManager.StartPipeline("delete", deleteArgs, controller, false);
        }
        else
        {
          Console.WriteLine($"Website Name {cmd.Instance.Name} not found... skipping delete task");
        }
      }
    }

    private bool CommandIsValid(SIMConsoleCommand cmd)
    {
      if(WebServerManager.WebsiteExists(cmd.Instance.Name))
      {
        Console.WriteLine($"Website Name {cmd.Instance.Name} already exists... skipping install task");
        return false;
      }

      foreach (var host in cmd.Instance.Host)
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
  }
}
