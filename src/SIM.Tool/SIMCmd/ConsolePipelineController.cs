﻿using JetBrains.Annotations;
using SIM.Core;
using SIM.Extensions;
using SIM.Pipelines;
using SIM.Pipelines.Processors;
using Sitecore.Diagnostics.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM.Tool.SIMCmd
{
  public class ConsolePipelineController : IPipelineController
  {
    private List<SimCmdQuestion> Answers;

    private long ticks1 = Seconds(), ticks2 = Seconds();

    public ConsolePipelineController(List<SimCmdQuestion> answers = null)
    {
      this.Answers = answers;
    }

    [NotNull]
    public List<Processor> Processors
    {
      set
      {
        Assert.ArgumentNotNull(value, nameof(value));
        List<Processor> list = new List<Processor>();
      }
    }

    private static long Seconds()
    {
      return DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
    }

    private string GetConfiguredValue(string title, string defaultValue)
    {
      var ans = Answers.Where(x => x.Question.ToLower() == title.ToLower()).FirstOrDefault();
      return ans != null ? ans.Answer : defaultValue;
    }

    private bool GetConfiguredValue(string title, bool defaultValue)
    {
      var ans = Answers.Where(x => x.Question.ToLower() == title.ToLower()).FirstOrDefault();
      return ans != null ? (ans.Answer.ToLower() == "true" || ans.Answer.ToLower() == "yes") : defaultValue;
    }

    #region IPipelineController implementation
    public double Maximum { get; set; }

    public Pipeline Pipeline { get; set; }

    public string Ask([NotNull] string title, [NotNull] string defaultValue)
    {
      //Console.Write($"\r\nAsk: {title} default is ({defaultValue})");
      string value = GetConfiguredValue(title, defaultValue);
      Console.Write($"\r\nUsing configured value: {value}) for input: {title}");
      return value;
    }

    public bool Confirm([NotNull] string message)
    {
      bool value = GetConfiguredValue(message, true);
      string answer = value ? "Yes" : "No";
      Console.Write($"\r\nUsing configured value: {answer} for confirmation: {message})");
      return value;
    }

    public string Select([NotNull] string message, [NotNull] IEnumerable<string> options, bool allowMultipleSelection = false, string defaultValue = null)
    {
      string value = GetConfiguredValue(message, defaultValue);
      Console.Write($"\r\nUsing configured value {value} for select: {message}");
      return defaultValue;
    }

    public void Dispose()
    {
    }

    public void Execute([NotNull] string path, [CanBeNull] string args)
    {
      Assert.ArgumentNotNull(path, nameof(path));

      if (args.EmptyToNull() == null)
      {
        CoreApp.RunApp(path);
        return;
      }

      CoreApp.RunApp(path, args);
    }

    public void Finish([NotNull] string message, bool closeInterface)
    {
      Console.Write($"\r\nFinish: {message} ");

    }

    public void IncrementProgress()
    {
      ticks1 = Seconds();
      if (ticks1 - ticks2 > 1)
      {
        Console.Write(".");
        ticks2 = ticks1 = Seconds();
      }
    }

    public void IncrementProgress(long progress)
    {
      IncrementProgress();
    }

    public void Pause()
    {
      Console.Write($"\r\nPause:");
    }

    public void ProcessorCrashed([NotNull] string error)
    {
      Console.Write($"\r\n --- Exception: {error}");
    }

    public void ProcessorDone([NotNull] string title)
    {
      Console.Write($" --- Completed: {title}");
    }

    public void ProcessorSkipped([NotNull] string processorName)
    {
      Console.Write($"\r\nProcessor Skipped: {processorName}");
    }

    public void ProcessorStarted([NotNull] string title)
    {
      Console.Write($"\r\nProcessor Started: {title}");
    }

    public void Resume()
    {
      Console.Write($"\r\nResume:");
    }

    public void SetProgress(long progress)
    {
      IncrementProgress();
    }

    public void Start([NotNull] string title, [NotNull] List<Step> steps)
    {
      Console.Write($"\r\nStart: {title} ");
      Assert.ArgumentNotNull(title, nameof(title));
      Assert.ArgumentNotNull(steps, nameof(steps));

      Start(title, steps, 0);
    }

    protected void Start([NotNull] string title, [NotNull] List<Step> steps, int value)
    {
      Assert.ArgumentNotNull(title, nameof(title));
      Assert.ArgumentNotNull(steps, nameof(steps));

      Maximum = value;
      List<Processor> p = new List<Processor>();
      foreach (Step step in steps)
      {
        List<Processor> pp = step._Processors;
        p.AddRange(pp);
      }

      Processors = p;
    }

    #endregion
  }

}