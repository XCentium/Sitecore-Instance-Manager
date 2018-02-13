using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SIM.Tool.CmdLine
{

  [DataContract]
  public class SIMConsoleConfig
  {
    [DataMember]
    public SIMConsoleSettings Settings { get; set; }

    [DataMember]
    public SIMConsoleCommandList Commands { get; set; }

    public static SIMConsoleConfig GetConfig(string file)
    {
      var serializer = new DataContractJsonSerializer(typeof(SIMConsoleConfig));
      var stream = File.OpenRead(file);
      var config = serializer.ReadObject(stream) as SIMConsoleConfig;
      stream.Close();
      return config;
    }

    public SIMConsoleCommand FindCommand(string name)
    {
      return Commands.Where(t => t.Name == name).FirstOrDefault();
    }

  }

  [CollectionDataContract]
  public class SIMConsoleCommandList : List<SIMConsoleCommand>
  {

  }

  [DataContract]
  public class SIMConsoleCommand
  {
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Action { get; set; }

    [DataMember]
    public SIMConsoleProduct Product { get; set; }

    [DataMember]
    public SIMConsoleInstance Instance { get; set; }

    [DataMember]
    public SIMConsoleOptions Options { get; set; }

    [DataMember]
    public SIMConsoleConfiguredValues Answers { get; set; }

  }

  [CollectionDataContract]
  public class SIMConsoleConfiguredValues : List<SIMConfiguredValue>
  {
    public string GetConfiguredValue(string title, string defaultValue)
    {
      var ans = this.Where(x => x.Question.ToLower() == title.ToLower()).FirstOrDefault();
      return ans != null ? ans.Answer : defaultValue;
    }

    public bool GetConfiguredValue(string title, bool defaultValue)
    {
      var ans = this.Where(x => x.Question.ToLower() == title.ToLower()).FirstOrDefault();
      return ans != null ? (ans.Answer.ToLower() == "true" || ans.Answer.ToLower() == "yes") : defaultValue;
    }
  }

  public class SIMConfiguredValue
  {
    [DataMember]
    public string Question { get; set; }

    [DataMember]
    public string Answer { get; set; }
  }

    [DataContract]
  public class SIMConsoleSettings
  {
    [DataMember]
    public string LocalRepository { get; set; }
    [DataMember]
    public string InstanceRoot { get; set; }
    [DataMember]
    public string LicensePath { get; set; }
    [DataMember]
    public string ConnectionString { get; set; }
  }

  [DataContract]
  public class SIMConsoleProduct
  {
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string Version { get; set; }
    [DataMember]
    public string Revision { get; set; }
    [DataMember]
    public List<string> SitecoreModules { get; set; }
    [DataMember]
    public List<string> CustomModules { get; set; }
    [DataMember]
    public List<string> ConfigPresets { get; set; }
  }

  [DataContract]
  public class SIMConsoleInstance
  {
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public List<string> Host { get; set; }
    [DataMember]
    public string SqlPrefix { get; set; }
    [DataMember]
    public bool AttachSql { get; set; }
    [DataMember]
    public string RootDir { get; set; }
    [DataMember]
    public string WebIdentity { get; set; }
    [DataMember]
    public string SqlIdentity { get; set; }
    [DataMember]
    public bool ForceFramework4 { get; set; }
    [DataMember]
    public bool Is32Bit { get; set; }
    [DataMember]
    public bool IsClassic { get; set; }
  }

  [DataContract]
  public class SIMConsoleOptions
  {
    [DataMember]
    public bool InstallRadControls { get; set; }
    [DataMember]
    public bool InstallDictionaries { get; set; }
    [DataMember]
    public bool ServerSideRedirects { get; set; }
    [DataMember]
    public bool IncreaseExecutionTimeout { get; set; }
    [DataMember]
    public bool Preheat { get; set; }
    [DataMember]
    public string Sitecore8Roles { get; set; }
    [DataMember]
    public string Sitecore9Roles { get; set; }
  }



}
