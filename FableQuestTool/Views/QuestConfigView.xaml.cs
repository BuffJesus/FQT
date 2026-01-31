using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using FableQuestTool.Models;
using FableQuestTool.Services;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class QuestConfigView : System.Windows.Controls.UserControl
{
    private static readonly Regex StateNamePattern = new("^[A-Za-z][A-Za-z0-9_]*$");
    private readonly CodeGenerator codeGenerator = new();
    private bool isInitializing = true;

    public QuestConfigViewModel QuestConfigViewModel { get; } = new();

    public QuestConfigView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new System.Windows.Controls.TextChangedEventHandler(OnAnyFieldChanged));
        AddHandler(System.Windows.Controls.ComboBox.SelectionChangedEvent, new System.Windows.Controls.SelectionChangedEventHandler(OnAnyFieldChanged));
        AddHandler(System.Windows.Controls.CheckBox.CheckedEvent, new System.Windows.RoutedEventHandler(OnAnyFieldChanged));
        AddHandler(System.Windows.Controls.CheckBox.UncheckedEvent, new System.Windows.RoutedEventHandler(OnAnyFieldChanged));

        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        isInitializing = false;
        UpdatePreview();
        SetSectionVisibility();
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsAdvancedMode))
        {
            SetSectionVisibility();
        }
    }

    private void OnAnyFieldChanged(object? sender, System.Windows.RoutedEventArgs e)
    {
        if (isInitializing || ViewModel == null)
        {
            return;
        }

        ViewModel.IsModified = true;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (ViewModel == null)
        {
            return;
        }

        PreviewText.Text = codeGenerator.GenerateQuestScript(ViewModel.Project);
    }

    private void OnSectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        SetSectionVisibility();
    }

    private void SetSectionVisibility()
    {
        if (SectionList == null || BasicInfoGroup == null)
        {
            return;
        }

        string? section = (SectionList.SelectedItem as System.Windows.Controls.ListBoxItem)?.Content?.ToString();
        bool advanced = ViewModel?.IsAdvancedMode == true;
        if (string.IsNullOrWhiteSpace(section))
        {
            BasicInfoGroup.Visibility = System.Windows.Visibility.Visible;
            QuestCardGroup.Visibility = System.Windows.Visibility.Visible;
            RegionsGroup.Visibility = System.Windows.Visibility.Visible;
            RewardsGroup.Visibility = System.Windows.Visibility.Visible;
            BoastsGroup.Visibility = System.Windows.Visibility.Visible;
            StatesGroup.Visibility = System.Windows.Visibility.Visible;
            ThreadsGroup.Visibility = System.Windows.Visibility.Visible;
            return;
        }

        if (!advanced && (section == "Optional Challenges" || section == "Background Tasks"))
        {
            SectionList.SelectedIndex = 0;
            section = "Basic Info";
        }

        BasicInfoGroup.Visibility = section == "Basic Info" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        QuestCardGroup.Visibility = section == "Quest Card" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        RegionsGroup.Visibility = section == "Regions" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        RewardsGroup.Visibility = section == "Rewards" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        BoastsGroup.Visibility = section == "Optional Challenges" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        StatesGroup.Visibility = section == "States" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        ThreadsGroup.Visibility = section == "Background Tasks" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        MapOffsetsPanel.Visibility = advanced ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        BoastsGroup.Visibility = advanced ? BoastsGroup.Visibility : System.Windows.Visibility.Collapsed;
        ThreadsGroup.Visibility = advanced ? ThreadsGroup.Visibility : System.Windows.Visibility.Collapsed;
    }

    private void OnAddRegion(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string region = RegionInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(region))
        {
            return;
        }

        foreach (string existing in ViewModel.Project.Regions)
        {
            if (string.Equals(existing, region, StringComparison.OrdinalIgnoreCase))
            {
                RegionInput.Text = string.Empty;
                return;
            }
        }

        ViewModel.Project.Regions.Add(region);
        RegionInput.Text = string.Empty;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Region added.";
        UpdatePreview();
    }

    private void OnRemoveRegion(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (RegionsList.SelectedItem is string region)
        {
            ViewModel.Project.Regions.Remove(region);
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Region removed.";
            UpdatePreview();
        }
    }

    private void OnRewardItemSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (RewardItemsList.SelectedItem is string item)
        {
            RewardItemInput.Text = item;
        }
    }

    private void OnAddRewardItem(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string item = RewardItemInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        ViewModel.Project.Rewards.Items.Add(item);
        RewardItemInput.Text = string.Empty;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Reward item added.";
        UpdatePreview();
    }

    private void OnUpdateRewardItem(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null || RewardItemsList.SelectedItem is not string current)
        {
            return;
        }

        string updated = RewardItemInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(updated))
        {
            return;
        }

        int index = ViewModel.Project.Rewards.Items.IndexOf(current);
        if (index >= 0)
        {
            ViewModel.Project.Rewards.Items[index] = updated;
            RewardItemsList.SelectedIndex = index;
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Reward item updated.";
            UpdatePreview();
        }
    }

    private void OnRemoveRewardItem(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (RewardItemsList.SelectedItem is string item)
        {
            ViewModel.Project.Rewards.Items.Remove(item);
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Reward item removed.";
            UpdatePreview();
        }
    }

    private void OnRewardAbilitySelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (RewardAbilitiesList.SelectedItem is string ability)
        {
            RewardAbilityInput.Text = ability;
        }
    }

    private void OnAddRewardAbility(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string ability = RewardAbilityInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(ability))
        {
            return;
        }

        ViewModel.Project.Rewards.Abilities.Add(ability);
        RewardAbilityInput.Text = string.Empty;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Reward ability added.";
        UpdatePreview();
    }

    private void OnUpdateRewardAbility(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null || RewardAbilitiesList.SelectedItem is not string current)
        {
            return;
        }

        string updated = RewardAbilityInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(updated))
        {
            return;
        }

        int index = ViewModel.Project.Rewards.Abilities.IndexOf(current);
        if (index >= 0)
        {
            ViewModel.Project.Rewards.Abilities[index] = updated;
            RewardAbilitiesList.SelectedIndex = index;
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Reward ability updated.";
            UpdatePreview();
        }
    }

    private void OnRemoveRewardAbility(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (RewardAbilitiesList.SelectedItem is string ability)
        {
            ViewModel.Project.Rewards.Abilities.Remove(ability);
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Reward ability removed.";
            UpdatePreview();
        }
    }

    private void OnBoastSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (BoastsList.SelectedItem is QuestBoast boast)
        {
            BoastIdInput.Text = boast.BoastId.ToString(CultureInfo.InvariantCulture);
            BoastTextInput.Text = boast.Text;
            BoastRenownInput.Text = boast.RenownReward.ToString(CultureInfo.InvariantCulture);
            BoastGoldInput.Text = boast.GoldReward.ToString(CultureInfo.InvariantCulture);
            BoastTextIdInput.Text = boast.TextId.ToString(CultureInfo.InvariantCulture);
            BoastIsBoastInput.IsChecked = boast.IsBoast;
        }
    }

    private void OnAddBoast(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (!int.TryParse(BoastIdInput.Text.Trim(), out int boastId))
        {
            System.Windows.MessageBox.Show("Boast ID must be a number.", "Boast", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        QuestBoast boast = new()
        {
            BoastId = boastId,
            Text = BoastTextInput.Text.Trim(),
            RenownReward = ParseInt(BoastRenownInput.Text, 0),
            GoldReward = ParseInt(BoastGoldInput.Text, 0),
            TextId = ParseInt(BoastTextIdInput.Text, 0),
            IsBoast = BoastIsBoastInput.IsChecked == true
        };

        ViewModel.Project.Boasts.Add(boast);
        ClearBoastInputs();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Boast added.";
        UpdatePreview();
    }

    private void OnUpdateBoast(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null || BoastsList.SelectedItem is not QuestBoast current)
        {
            return;
        }

        if (!int.TryParse(BoastIdInput.Text.Trim(), out int boastId))
        {
            System.Windows.MessageBox.Show("Boast ID must be a number.", "Boast", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        QuestBoast updated = new()
        {
            BoastId = boastId,
            Text = BoastTextInput.Text.Trim(),
            RenownReward = ParseInt(BoastRenownInput.Text, 0),
            GoldReward = ParseInt(BoastGoldInput.Text, 0),
            TextId = ParseInt(BoastTextIdInput.Text, 0),
            IsBoast = BoastIsBoastInput.IsChecked == true
        };

        int index = ViewModel.Project.Boasts.IndexOf(current);
        if (index >= 0)
        {
            ViewModel.Project.Boasts[index] = updated;
            BoastsList.SelectedIndex = index;
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Boast updated.";
            UpdatePreview();
        }
    }

    private void OnRemoveBoast(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (BoastsList.SelectedItem is QuestBoast boast)
        {
            ViewModel.Project.Boasts.Remove(boast);
            ClearBoastInputs();
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Boast removed.";
            UpdatePreview();
        }
    }

    private void ClearBoastInputs()
    {
        BoastIdInput.Clear();
        BoastTextInput.Clear();
        BoastRenownInput.Clear();
        BoastGoldInput.Clear();
        BoastTextIdInput.Clear();
        BoastIsBoastInput.IsChecked = true;
    }

    private void OnAddState(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string name = StateNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) || !StateNamePattern.IsMatch(name))
        {
            System.Windows.MessageBox.Show("State name must start with a letter and contain only letters, numbers, and underscores.",
                "State Name", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        string type = (StateTypeInput.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "bool";
        object? defaultValue = ParseDefault(type, StateDefaultInput.Text.Trim());

        QuestState state = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Type = type,
            Persist = StatePersistInput.IsChecked == true,
            DefaultValue = defaultValue
        };

        ViewModel.Project.States.Add(state);
        StateNameInput.Clear();
        StateDefaultInput.Clear();
        StateTypeInput.SelectedIndex = 0;
        StatePersistInput.IsChecked = true;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "State variable added.";
        UpdatePreview();
    }

    private void OnRemoveState(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (StatesList.SelectedItem is QuestState state)
        {
            ViewModel.Project.States.Remove(state);
            ViewModel.IsModified = true;
            ViewModel.StatusText = "State variable removed.";
            UpdatePreview();
        }
    }

    private void OnThreadSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ThreadsList.SelectedItem is QuestThread thread)
        {
            ThreadFunctionInput.Text = thread.FunctionName;
            ThreadRegionInput.Text = thread.Region;
            ThreadDescriptionInput.Text = thread.Description;
        }
    }

    private void OnAddThread(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string function = ThreadFunctionInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(function))
        {
            return;
        }

        QuestThread thread = new()
        {
            FunctionName = function,
            Region = ThreadRegionInput.Text.Trim(),
            Description = ThreadDescriptionInput.Text.Trim()
        };

        ViewModel.Project.Threads.Add(thread);
        ClearThreadInputs();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Thread added.";
        UpdatePreview();
    }

    private void OnUpdateThread(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null || ThreadsList.SelectedItem is not QuestThread current)
        {
            return;
        }

        QuestThread updated = new()
        {
            FunctionName = ThreadFunctionInput.Text.Trim(),
            Region = ThreadRegionInput.Text.Trim(),
            Description = ThreadDescriptionInput.Text.Trim()
        };

        int index = ViewModel.Project.Threads.IndexOf(current);
        if (index >= 0)
        {
            ViewModel.Project.Threads[index] = updated;
            ThreadsList.SelectedIndex = index;
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Thread updated.";
            UpdatePreview();
        }
    }

    private void OnRemoveThread(object sender, System.Windows.RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ThreadsList.SelectedItem is QuestThread thread)
        {
            ViewModel.Project.Threads.Remove(thread);
            ClearThreadInputs();
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Thread removed.";
            UpdatePreview();
        }
    }

    private void ClearThreadInputs()
    {
        ThreadFunctionInput.Clear();
        ThreadRegionInput.Clear();
        ThreadDescriptionInput.Clear();
    }

    private static int ParseInt(string text, int fallback)
    {
        if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            return value;
        }

        return fallback;
    }

    private static object? ParseDefault(string type, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return type switch
            {
                "int" => 0,
                "string" => string.Empty,
                _ => false
            };
        }

        if (type == "int")
        {
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }

            return 0;
        }

        if (type == "string")
        {
            return input;
        }

        if (bool.TryParse(input, out bool result))
        {
            return result;
        }

        return false;
    }
}
