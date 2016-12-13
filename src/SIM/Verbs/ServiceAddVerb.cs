﻿namespace SIM.Verbs
{
  using CommandLine;
  using JetBrains.Annotations;
  using SIM.Abstract.Connection;
  using SIM.Abstract.Services;
  using SIM.Commands;
  using SIM.Services;

  public class SqlAddVerb : ServiceAddCommand
  {
    [UsedImplicitly]
    public SqlAddVerb() 
      : this(Default.ConnectionStringFactory, Default.ServiceStore)
    {
    }                                         

    private SqlAddVerb(IConnectionStringFactory connectionStringFactory, IServiceStore serviceStore) 
      : base(connectionStringFactory, serviceStore)
    { 
    }

    public override ServiceType? ServiceType => Services.ServiceType.SqlServer;

    [Option('n', "name", Required = true)]
    public override string ServiceName { get; set; }
                     
    [Option('c', "connectionString", Required = true)]
    public override string ConnectionString { get; set; }
  }
}