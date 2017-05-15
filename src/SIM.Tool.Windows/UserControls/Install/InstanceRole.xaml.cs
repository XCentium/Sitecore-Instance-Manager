﻿using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Sitecore.Diagnostics.Logging;

namespace SIM.Tool.Windows.UserControls.Install
{
  using System;
  using SIM.Tool.Base.Pipelines;
  using SIM.Tool.Base.Wizards;
  using Sitecore.Diagnostics.Base;
  using JetBrains.Annotations;

  public partial class InstanceRole : IWizardStep
  {
    public InstanceRole()
    {
      InitializeComponent();
    }

    public void InitializeStep([NotNull] WizardArgs wizardArgs)
    {
      Assert.ArgumentNotNull(wizardArgs, nameof(wizardArgs));

      InstallRoles.IsEnabled = false;
      InstallRoles.IsChecked = false;

      var args = (InstallWizardArgs)wizardArgs;
      try
      {
        var ver = args.Product.Version;
        var upd = args.Product.Update;
        var txt = $"{ver}.{upd}";

        if (txt == "8.1.3" || txt.StartsWith("8.2"))
        {
          InstallRoles.IsEnabled = true;
          if (!string.IsNullOrEmpty(args.InstallRoles))
          {
            InstallRoles.IsChecked = true;
            var radio = (RadioButton)RoleName.FindName(args.InstallRoles);
            Assert.IsNotNull(radio, $"{args.InstallRoles} is not supported");

            radio.IsChecked = true;
          }
        }
      }
      catch (Exception e)
      {
        Log.Error(e, "Something is wrong");
      }
    }

    public bool SaveChanges([NotNull] WizardArgs wizardArgs)
    {
      Assert.ArgumentNotNull(wizardArgs, nameof(wizardArgs));

      var args = (InstallWizardArgs)wizardArgs;
      if (InstallRoles.IsChecked != true)
      {
        args.InstallRoles = "";
        InstallWizardArgs.SaveLastTimeOption(nameof(args.InstallRoles), args.InstallRoles);

        return true;
      }

      var role = RoleName.Children.OfType<RadioButton>().FirstOrDefault(x => x.IsChecked == true).IsNotNull("role").Name;

      args.InstallRoles = role;
      InstallWizardArgs.SaveLastTimeOption(nameof(args.InstallRoles), args.InstallRoles);

      return true;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
      e.Handled = true;
    }
  }
}
