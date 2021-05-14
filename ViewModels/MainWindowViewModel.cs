using Avalonia.Controls;
using BugFablesDataEditor.BugFablesEnums;
using BugFablesDataEditor.Models;
using BugFablesDataEditor.Views;
using Common.MessageBox.Enums;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace BugFablesDataEditor.ViewModels
{
  public class MainWindowViewModel : ViewModelBase
  {
    private bool directorySaved = false;
    private string[] mapDescriptions = CommonUtils.GetEnumDescriptions<Map>();
    private Entity defaultEnitty = new Entity();

    private string[] _npcTypesDescription = CommonUtils.GetEnumDescriptions<NPCType>();
    public string[] NPCTypesDescription
    {
      get { return _npcTypesDescription; }
    }

    private string[] _interactionsDescription = CommonUtils.GetEnumDescriptions<Interaction>();
    public string[] InteractionsDescription
    {
      get { return _interactionsDescription; }
    }

    private string[] _deathTypeDescription = CommonUtils.GetEnumDescriptions<DeathType>();
    public string[] DeathTypeDescription
    {
      get { return _deathTypeDescription; }
    }

    private string[] _actionBehaviorsDescription = CommonUtils.GetEnumDescriptions<ActionBehaviors>();
    public string[] ActionBehaviorsDescription
    {
      get { return _actionBehaviorsDescription; }
    }

    private string[] _animIdDescription = CommonUtils.GetEnumDescriptions<AnimID>();
    public string[] AnimIdDescription
    {
      get { return _animIdDescription; }
    }

    private string[] _objectTypesDescription = CommonUtils.GetEnumDescriptions<ObjectTypes>();
    public string[] ObjectTypesDescription
    {
      get { return _objectTypesDescription; }
    }

    private string _currentDirectoryPath = "No entity directory, open an existing directory or create a new one";
    public string CurrentDirectoryPath
    {
      get { return _currentDirectoryPath; }
      set { _currentDirectoryPath = value; this.RaisePropertyChanged(); }
    }

    private EntityDirectory _entityDirectory;
    public EntityDirectory EntityDirectory
    {
      get { return _entityDirectory; }
      set { _entityDirectory = value; this.RaisePropertyChanged(); }
    }

    private bool _directoryInUse = false;
    public bool DirectoryInUse
    {
      get { return _directoryInUse; }
      set { _directoryInUse = value; this.RaisePropertyChanged(); }
    }

    private ObservableCollection<string> _keysDescriptions = new ObservableCollection<string>();
    public ObservableCollection<string> KeysDescriptions
    {
      get { return _keysDescriptions; }
      set { _keysDescriptions = value; this.RaisePropertyChanged(); }
    }

    private ObservableCollection<string> _lineIndexesDescriptions = new ObservableCollection<string>();
    public ObservableCollection<string> LineIndexesDescriptions
    {
      get { return _lineIndexesDescriptions; }
      set { _lineIndexesDescriptions = value; this.RaisePropertyChanged(); }
    }

    private int _selectedKeyIndex = -1;
    public int SelectedKeyIndex
    {
      get { return _selectedKeyIndex; }
      set
      {
        _selectedKeyIndex = value;
        this.RaisePropertyChanged();
        this.RaisePropertyChanged(nameof(CurrentEntity));
        if (CurrentKey != -1)
        {
          SelectedLineIndex = -1;
          RebuildLineIndexesDescriptions();
        }
      }
    }

    private int _selectedLineIndex = -1;
    public int SelectedLineIndex
    {
      get { return _selectedLineIndex; }
      set
      {
        _selectedLineIndex = value;
        this.RaisePropertyChanged();
        this.RaisePropertyChanged(nameof(CurrentEntity));
      }
    }

    public int CurrentKey
    {
      get
      {
        if (SelectedKeyIndex < 0)
          return -1;
        else if (SelectedKeyIndex < (int)Map.COUNT)
          return SelectedKeyIndex;
        else
          return int.Parse(KeysDescriptions[SelectedKeyIndex]);
      }
    }

    public Entity CurrentEntity
    {
      get
      {
        if (CurrentKey == -1 || SelectedLineIndex == -1)
          return defaultEnitty;
        return EntityDirectory.Entities[CurrentKey][SelectedLineIndex];
      }
    }

    private GeneralView _generalView;
    public GeneralView GeneralView
    {
      get { return _generalView; }
      set { _generalView = value; this.RaisePropertyChanged(); }
    }

    private ActionsView _actionsView;
    public ActionsView ActionsView
    {
      get { return _actionsView; }
      set { _actionsView = value; this.RaisePropertyChanged(); }
    }

    private ArraysView _arraysView;
    public ArraysView ArraysView
    {
      get { return _arraysView; }
      set { _arraysView = value; this.RaisePropertyChanged(); }
    }

    private CollidersView _colliderView;
    public CollidersView CollidersView
    {
      get { return _colliderView; }
      set { _colliderView = value; this.RaisePropertyChanged(); }
    }

    private EmoticonsView _emoticaonsView;
    public EmoticonsView EmoticonsView
    {
      get { return _emoticaonsView; }
      set { _emoticaonsView = value; this.RaisePropertyChanged(); }
    }

    private LocationView _locationView;
    public LocationView LocationView
    {
      get { return _locationView; }
      set { _locationView = value; this.RaisePropertyChanged(); }
    }

    private PropertiesView _propertiesView;
    public PropertiesView PropertiesView
    {
      get { return _propertiesView; }
      set { _propertiesView = value; this.RaisePropertyChanged(); }
    }

    public ReactiveCommand<Unit, Unit> CmdSaveDirectory { get; set; }

    public MainWindowViewModel()
    {
      EntityDirectory directory = new EntityDirectory();
      EntityDirectory = directory;
      Initialise(directory);
    }

    public MainWindowViewModel(EntityDirectory directory)
    {
      EntityDirectory = directory;
      Initialise(directory);

      GeneralView = new GeneralView(this);
      ActionsView = new ActionsView(this);
      ArraysView = new ArraysView(this);
      CollidersView = new CollidersView(this);
      EmoticonsView = new EmoticonsView(this);
      LocationView = new LocationView(this);
      PropertiesView = new PropertiesView(this);
    }

    private void Initialise(EntityDirectory directory)
    {
      CmdSaveDirectory = ReactiveCommand.CreateFromTask(async () =>
      {
        OpenFolderDialog dlg = new OpenFolderDialog();
        dlg.Title = "Select the location to save the directory";
        string filePath = await dlg.ShowAsync(CommonUtils.MainWindow);
        if (!string.IsNullOrEmpty(filePath))
        {
          try
          {
            EntityDirectory.WriteDirectory(filePath);
            CurrentDirectoryPath = filePath;
            directorySaved = true;
          }
          catch (Exception ex)
          {
            var msg = CommonUtils.GetMessageBox("Error opening save file",
                        "An error occured while saving the save file: " + ex.Message, ButtonEnum.Ok, Icon.Error);
            await msg.ShowDialog(CommonUtils.MainWindow);
          }
        }
      }, this.WhenAnyValue(x => x.DirectoryInUse));
    }

    public async void NewDirectory()
    {
      if (DirectoryInUse && !directorySaved)
      {
        var msg = CommonUtils.GetMessageBox("Directory in use", "An unsaved directory is currently in use, " +
                  "creating a new directory will loose all unsaved changes,\nare you sure you want to proceed?",
                  ButtonEnum.YesNo, Icon.Question);
        await msg.ShowDialog(CommonUtils.MainWindow);
        if (msg.ButtonResult == ButtonResult.No)
          return;
      }

      EntityDirectory.ResetToDefault();
      CurrentDirectoryPath = "New directory being created, save it to store it";
      DirectoryInUse = true;
      directorySaved = false;
      RebuildKeysDescriptions();
    }

    public async void OpenDirectory()
    {
      if (DirectoryInUse && !directorySaved)
      {
        var msg = CommonUtils.GetMessageBox("Directory in use", "An unsaved directory is currently in use, " +
                  "opening a directory will loose all unsaved changes,\nare you sure you want to proceed?",
                  ButtonEnum.YesNo, Icon.Question);
        await msg.ShowDialog(CommonUtils.MainWindow);
        if (msg.ButtonResult == ButtonResult.No)
          return;
      }

      OpenFolderDialog dlg = new OpenFolderDialog();
      dlg.Title = "Select a Bug Fables entitydata directory";
      string filePaths = await dlg.ShowAsync(CommonUtils.MainWindow);
      if (string.IsNullOrEmpty(filePaths))
        return;

      try
      {
        EntityDirectory.ResetToDefault();
        EntityDirectory.LoadDirectory(filePaths);
        CurrentDirectoryPath = filePaths;
        DirectoryInUse = true;
        directorySaved = true;
        RebuildKeysDescriptions();
      }
      catch (Exception ex)
      {
        EntityDirectory.ResetToDefault();
        var msg = CommonUtils.GetMessageBox("Error opening directory",
                    "An error occured while opening the directory: " + ex.Message, ButtonEnum.Ok, Icon.Error);
        await msg.ShowDialog(CommonUtils.MainWindow);
      }
    }

    private void RebuildKeysDescriptions()
    {
      KeysDescriptions.Clear();
      foreach (var key in EntityDirectory.Entities.Keys.OrderBy(x => x))
      {
        if (key < 0 || key >= (int)Map.COUNT)
          KeysDescriptions.Add(key.ToString());
        else
          KeysDescriptions.Add(mapDescriptions[key]);
      }
      this.RaisePropertyChanged(nameof(KeysDescriptions));
    }


    private void RebuildLineIndexesDescriptions()
    {
      LineIndexesDescriptions.Clear();
      for (int i = 0; i < EntityDirectory.Entities[CurrentKey].Count; i++)
      {
        LineIndexesDescriptions.Add(i + " - " + EntityDirectory.Entities[CurrentKey][i].Name);
      }
      this.RaisePropertyChanged(nameof(LineIndexesDescriptions));
    }

    public void Exit()
    {
      CommonUtils.MainWindow.Close();
    }
  }
}
