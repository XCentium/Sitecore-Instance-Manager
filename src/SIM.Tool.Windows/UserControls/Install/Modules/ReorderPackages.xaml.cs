namespace SIM.Tool.Windows.UserControls.Install.Modules
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Globalization;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using System.Windows.Input;
  using System.Windows.Shapes;
  using Microsoft.Win32;
  using SIM.Products;
  using SIM.Tool.Base;
  using SIM.Tool.Base.Pipelines;
  using SIM.Tool.Base.Wizards;
  using Sitecore.Diagnostics.Base;
  using JetBrains.Annotations;
  using Newtonsoft.Json;

  internal class ReadmeToVisibilityConverter : IValueConverter
  {
    #region Public methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return string.IsNullOrEmpty(((Dictionary<bool, string>)value).Values.FirstOrDefault()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  internal class ReadmeToStringConverter : IValueConverter
  {
    #region Public methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((Dictionary<bool, string>)value).Values.FirstOrDefault();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  internal class ReadmeToColorConverter : IValueConverter
  {
    #region Public methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((Dictionary<bool, string>)value).Keys.FirstOrDefault() ? "red" : "gray";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  public partial class ReorderPackages : IWizardStep, IFlowControl, ICustomButton
  {
    #region Fields

    private readonly ObservableCollection<Product> _ActualProducts = new ObservableCollection<Product>();

    #endregion

    #region Constructors

    public ReorderPackages()
    {
      InitializeComponent();
    }

    #endregion

    #region Public Methods

    #region IStateControl Members

    #region Public properties

    public WizardArgs WizardArgs { get; set; }

    #endregion

    #region Public methods

    public bool SaveChanges(WizardArgs wizardArgs)
    {
      return true;
    }

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Public methods

    public void InitializeStep(WizardArgs wizardArgs)
    {
      WizardArgs = wizardArgs;
      var args = (InstallModulesWizardArgs)wizardArgs;
      _ActualProducts.Clear();
      args._Modules.ForEach(module => _ActualProducts.Add(module));

      modulesList.ItemsSource = _ActualProducts;
      selectedProductLabel.DataContext = args.Product;
      SIM.Pipelines.Install.InstallArgs iArgs = (SIM.Pipelines.Install.InstallArgs) WizardArgs.ToProcessorArgs();
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      System.IO.StringWriter sw = new System.IO.StringWriter(sb);
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        writer.Formatting = Formatting.Indented;
        writer.WriteStartObject();

        writer.WritePropertyName("InstanceName");
        writer.WriteValue(iArgs.Product.Name);

        writer.WritePropertyName("InstanceHostNames");

        writer.WriteStartArray();
        foreach (var hostName in iArgs._HostNames)
        {
          writer.WriteValue(hostName);
        }
        writer.WriteEndArray();

        writer.WritePropertyName("InstanceSqlPrefix");
        writer.WriteValue(iArgs.InstanceSqlPrefix);
        writer.WritePropertyName("InstanceAttachSql");
        writer.WriteValue(iArgs.InstanceAttachSql);
        writer.WritePropertyName("ProductName");
        writer.WriteValue(iArgs.Product.ToString());
        writer.WritePropertyName("RootFolderPath");
        writer.WriteValue(iArgs.RootFolderPath);
        writer.WritePropertyName("ConnectionString");
        writer.WriteValue(iArgs.ConnectionString.ToString());
        writer.WritePropertyName("SqlServerIdentity");
        writer.WriteValue(iArgs.SqlServerIdentity);
        writer.WritePropertyName("WebServerIdentity");
        writer.WriteValue(iArgs.WebServerIdentity);
        writer.WritePropertyName("LicenseFilePath");
        writer.WriteValue(iArgs.LicenseFilePath);
        writer.WritePropertyName("ForceNetFramework4");
        writer.WriteValue(iArgs.ForceNetFramework4);
        writer.WritePropertyName("Is32Bit");
        writer.WriteValue(iArgs.Is32Bit);
        writer.WritePropertyName("IsClassic");
        writer.WriteValue(iArgs.IsClassic);
        writer.WritePropertyName("InstallRadControls");
        writer.WriteValue(iArgs.InstallRadControls);
        writer.WritePropertyName("InstallDictionaries");
        writer.WriteValue(iArgs.InstallDictionaries);
        writer.WritePropertyName("ServerSideRedirect");
        writer.WriteValue(iArgs.ServerSideRedirect);
        writer.WritePropertyName("IncreaseExecutionTimeout");
        writer.WriteValue(iArgs.IncreaseExecutionTimeout);
        writer.WritePropertyName("PreHeat");
        writer.WriteValue(iArgs.PreHeat);
        writer.WritePropertyName("InstallRolls8");
        writer.WriteValue(iArgs.InstallRoles8);
        writer.WritePropertyName("InstallRolls9");
        writer.WriteValue(iArgs.InstallRoles9);
        writer.WritePropertyName("Modules");

        writer.WriteStartArray();
        foreach (var hostName in iArgs._Modules)
        {
          writer.WriteValue(hostName.Name);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
      }
    }

    public class SIMCmdArgs
    {
      public string InstanceName { get; set; }
      public string [] InstanceHostName { get; set; }
      public string InstanceSqlPrefix { get; set; }
      public bool InstanceAttachSql { get; set; }

      public string ProductName { get; set; }
    }


    #endregion

    #region Private methods

    private void ModuleSelected([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
    {
      modulesList.SelectedIndex = -1;
    }

    #endregion

    #region Drag-n-Drop Implementation

    protected void ModulesListDrop(object sender, DragEventArgs e)
    {
      Product droppedData = e.Data.GetData(typeof(Product)) as Product;
      Product target = ((ListBoxItem)sender).DataContext as Product;

      Assert.IsNotNull(droppedData, "[ReorderProducts] Drag-n-drop: droppedData is null");
      Assert.IsNotNull(target, "[ReorderProducts] Drag-n-drop: target is null");
      var removeIndex = _ActualProducts.IndexOf(droppedData);
      var insertIndex = _ActualProducts.IndexOf(target);

      if (removeIndex < insertIndex)
      {
        _ActualProducts.Insert(insertIndex + 1, droppedData);
        _ActualProducts.RemoveAt(removeIndex);
      }
      else
      {
        var removeIndexNext = removeIndex + 1;
        if (modulesList.Items.Count + 1 > removeIndexNext)
        {
          _ActualProducts.Insert(insertIndex, droppedData);
          _ActualProducts.RemoveAt(removeIndexNext);
        }
      }
    }

    protected void ModulesListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
      if (sender is ListBoxItem)
      {
        ListBoxItem draggedItem = sender as ListBoxItem;
        DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
        draggedItem.IsSelected = true;
      }
    }

    #endregion

    #endregion

    #region ICustomButton Implementation

    #region Public properties

    public string CustomButtonText
    {
      get
      {
        return "Add Package";
      }
    }

    #endregion

    #region Public methods

    public void CustomButtonClick()
    {
      var openDialog = new OpenFileDialog
      {
        CheckFileExists = true, 
        Filter = "Sitecore Package or ZIP Archive (*.zip)|*.zip", 
        Title = "Choose Package", 
        Multiselect = true
      };
      if (openDialog.ShowDialog(Window.GetWindow(this)) == true)
      {
        foreach (string path in openDialog.FileNames)
        {
          var fileName = System.IO.Path.GetFileName(path);

          if (string.IsNullOrEmpty(fileName))
          {
            continue;
          }

          Product product = Product.GetFilePackageProduct(path);
          if (!_ActualProducts.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
          {
            _ActualProducts.Add(product);
          }
        }
      }
    }

    #endregion

    #endregion

    #region Public methods

    public bool OnMovingBack(WizardArgs wizardArgs)
    {
      return true;
    }

    public bool OnMovingNext(WizardArgs wizardArgs)
    {
      var args = (InstallModulesWizardArgs)wizardArgs;
      Product product = args.Product;
      Assert.IsNotNull(product, nameof(product));
      IEnumerable<Product> selected = _ActualProducts;
      args._Modules.Clear();
      args._Modules.AddRange(selected);

      if (!(args is InstallWizardArgs) && !args._Modules.Any())
      {
        WindowHelper.HandleError("You haven't chosen any module to install", false);
        return false;
      }

      return true;
    }

    #endregion

    #region Private methods

    private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
    {
      ((Rectangle)sender).Cursor = Cursors.Hand;
    }

    #endregion
  }
}