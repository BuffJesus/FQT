using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FableQuestTool.Models;
using FableQuestTool.Services;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // ConvertBack is handled by the OnUseContainerChanged event handler
        // Return the value as-is to avoid binding errors
        return System.Windows.Data.Binding.DoNothing;
    }
}

public partial class QuestConfigView : System.Windows.Controls.UserControl
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public QuestConfigView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
        PopulateEntityScriptNames();

        // Initialize checkbox state based on whether Container exists
        if (ViewModel?.Project?.Rewards?.Container != null)
        {
            UseContainerCheckBox.IsChecked = true;
        }
    }

    private void PopulateEntityScriptNames()
    {
        if (SpawnReferenceInput == null)
        {
            return;
        }

        if (ViewModel?.Project?.Entities == null)
        {
            return;
        }

        // Store current value
        string? currentValue = SpawnReferenceInput.Text;

        SpawnReferenceInput.Items.Clear();

        // Get all entity script names from the quest project
        foreach (var entity in ViewModel.Project.Entities)
        {
            if (!string.IsNullOrWhiteSpace(entity.ScriptName))
            {
                SpawnReferenceInput.Items.Add(entity.ScriptName);
            }
        }

        // Restore current value if it existed
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            SpawnReferenceInput.Text = currentValue;
        }
    }

    private void PopulateContainerDefinitions()
    {
        if (ContainerDefNameInput == null || ViewModel == null)
        {
            return;
        }

        try
        {
            // Get Fable install path from settings
            var config = Config.FableConfig.Load();
            var entityBrowser = new Services.EntityBrowserService(config);

            // Store current value
            string? currentValue = ContainerDefNameInput.Text;

            ContainerDefNameInput.Items.Clear();

            // Get all chest definitions from game files
            var chestDefinitions = entityBrowser.GetAllChestDefinitions();

            foreach (var def in chestDefinitions)
            {
                ContainerDefNameInput.Items.Add(def);
            }

            // If no chests found, add some common object definitions as fallback
            if (chestDefinitions.Count == 0)
            {
                var objectDefinitions = entityBrowser.GetAllObjectDefinitions();
                foreach (var def in objectDefinitions)
                {
                    if (def.Contains("CHEST", StringComparison.OrdinalIgnoreCase) ||
                        def.Contains("BARREL", StringComparison.OrdinalIgnoreCase) ||
                        def.Contains("CONTAINER", StringComparison.OrdinalIgnoreCase))
                    {
                        ContainerDefNameInput.Items.Add(def);
                    }
                }
            }

            // Restore current value if it existed
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                ContainerDefNameInput.Text = currentValue;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating container definitions: {ex.Message}");
        }
    }

    private void OnSectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string? section = (SectionList.SelectedItem as ListBoxItem)?.Content?.ToString();
        bool advanced = ViewModel.IsAdvancedMode;

        if (string.IsNullOrWhiteSpace(section))
        {
            // Show all
            BasicInfoGroup.Visibility = Visibility.Visible;
            QuestCardGroup.Visibility = Visibility.Visible;
            RegionsGroup.Visibility = Visibility.Visible;
            RewardsGroup.Visibility = Visibility.Visible;
            BoastsGroup.Visibility = Visibility.Visible;
            StatesGroup.Visibility = Visibility.Visible;
            ThreadsGroup.Visibility = Visibility.Visible;
            return;
        }

        // Handle advanced mode restrictions
        if (!advanced && (section == "Optional Challenges" || section == "Background Tasks"))
        {
            SectionList.SelectedIndex = 0;
            section = "Basic Info";
        }

        BasicInfoGroup.Visibility = section == "Basic Info" ? Visibility.Visible : Visibility.Collapsed;
        QuestCardGroup.Visibility = section == "Quest Card" ? Visibility.Visible : Visibility.Collapsed;
        RegionsGroup.Visibility = section == "Regions" ? Visibility.Visible : Visibility.Collapsed;
        RewardsGroup.Visibility = section == "Rewards" ? Visibility.Visible : Visibility.Collapsed;
        BoastsGroup.Visibility = section == "Optional Challenges" ? Visibility.Visible : Visibility.Collapsed;
        StatesGroup.Visibility = section == "States" ? Visibility.Visible : Visibility.Collapsed;
        ThreadsGroup.Visibility = section == "Background Tasks" ? Visibility.Visible : Visibility.Collapsed;

        // Advanced-only sections
        BoastsGroup.Visibility = advanced ? BoastsGroup.Visibility : Visibility.Collapsed;
        ThreadsGroup.Visibility = advanced ? ThreadsGroup.Visibility : Visibility.Collapsed;

        // Refresh entity script names and container definitions when Rewards section is shown
        if (section == "Rewards")
        {
            PopulateEntityScriptNames();
            PopulateContainerDefinitions();
        }
    }

    private void OnAutoCalculateMapOffset(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.Project == null)
        {
            return;
        }

        try
        {
            string primaryRegion = ViewModel.Project.Regions?.FirstOrDefault() ?? "";
            var offset = WorldMapCoordinateService.GetMapOffsetForQuest(primaryRegion, ViewModel.Project.Entities);

            ViewModel.Project.WorldMapOffsetX = offset.X;
            ViewModel.Project.WorldMapOffsetY = offset.Y;

            ViewModel.IsModified = true;
            ViewModel.StatusText = $"Map offset auto-calculated: ({offset.X}, {offset.Y}) based on region '{primaryRegion}'";
            UpdatePreview();
        }
        catch (Exception ex)
        {
            ViewModel.StatusText = "Failed to auto-calculate map offset.";
            System.Windows.MessageBox.Show(
                $"Failed to auto-calculate map offset: {ex.Message}",
                "Map Offset",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void OnAddRegion(object sender, RoutedEventArgs e)
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

        // Check for duplicates
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

    private void OnRemoveRegion(object sender, RoutedEventArgs e)
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

    private void OnUseContainerChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        bool useContainer = UseContainerCheckBox.IsChecked == true;

        if (useContainer && ViewModel.Project.Rewards.Container == null)
        {
            // Create new container configuration with defaults
            ViewModel.Project.Rewards.Container = new ContainerReward();
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Container rewards enabled.";
        }
        else if (!useContainer && ViewModel.Project.Rewards.Container != null)
        {
            // Remove container configuration
            ViewModel.Project.Rewards.Container = null;
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Container rewards disabled.";
        }

        UpdatePreview();
    }

    private void OnSpawnLocationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel?.Project.Rewards.Container == null)
        {
            return;
        }

        int selected = SpawnLocationInput.SelectedIndex;

        // Update the model
        ViewModel.Project.Rewards.Container.SpawnLocation = selected switch
        {
            0 => ContainerSpawnLocation.NearMarker,
            1 => ContainerSpawnLocation.NearEntity,
            2 => ContainerSpawnLocation.FixedPosition,
            _ => ContainerSpawnLocation.NearMarker
        };

        // Update UI visibility
        if (selected == 2) // FixedPosition
        {
            SpawnReferencePanel.Visibility = Visibility.Collapsed;
            SpawnPositionPanel.Visibility = Visibility.Visible;
        }
        else // NearMarker or NearEntity
        {
            SpawnReferencePanel.Visibility = Visibility.Visible;
            SpawnPositionPanel.Visibility = Visibility.Collapsed;

            // Update label
            SpawnReferenceLabel.Text = selected == 0 ? "Marker Name" : "Entity Script Name";
        }

        ViewModel.IsModified = true;
        UpdatePreview();
    }

    private void OnAddContainerItem(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.Project.Rewards.Container == null)
        {
            return;
        }

        string item = ContainerItemInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        ViewModel.Project.Rewards.Container.Items.Add(item);
        ContainerItemInput.Text = string.Empty;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Container item added.";
        UpdatePreview();
    }

    private void OnRemoveContainerItem(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.Project.Rewards.Container == null)
        {
            return;
        }

        if (ContainerItemsList.SelectedItem is string item)
        {
            ViewModel.Project.Rewards.Container.Items.Remove(item);
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Container item removed.";
            UpdatePreview();
        }
    }

    private void OnRewardAbilitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (RewardAbilitiesList.SelectedItem is string ability)
        {
            RewardAbilityInput.Text = ability;
        }
    }

    private void OnRewardItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RewardItemsList.SelectedItem is string item)
        {
            RewardItemInput.Text = item;
        }
    }

    private void OnAddRewardItem(object sender, RoutedEventArgs e)
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

    private void OnUpdateRewardItem(object sender, RoutedEventArgs e)
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

    private void OnRemoveRewardItem(object sender, RoutedEventArgs e)
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

    private void OnAddRewardAbility(object sender, RoutedEventArgs e)
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

    private void OnUpdateRewardAbility(object sender, RoutedEventArgs e)
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

    private void OnRemoveRewardAbility(object sender, RoutedEventArgs e)
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

    private void OnBoastSelected(object sender, SelectionChangedEventArgs e)
    {
        if (BoastsList.SelectedItem is QuestBoast boast)
        {
            BoastIdInput.Text = boast.BoastId.ToString();
            BoastTextInput.Text = boast.Text;
            BoastRenownInput.Text = boast.RenownReward.ToString();
            BoastGoldInput.Text = boast.GoldReward.ToString();
            IsBoastCheck.IsChecked = boast.IsBoast;
        }
    }

    private void OnAddBoast(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string boastIdText = BoastIdInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(boastIdText))
        {
            System.Windows.MessageBox.Show("Please enter a Boast ID.", "Boast ID", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(boastIdText, out int boastId))
        {
            System.Windows.MessageBox.Show("Boast ID must be a whole number.", "Boast ID", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        int.TryParse(BoastRenownInput.Text, out int renown);
        int.TryParse(BoastGoldInput.Text, out int gold);

        QuestBoast boast = new()
        {
            BoastId = boastId,
            Text = BoastTextInput.Text.Trim(),
            RenownReward = renown,
            GoldReward = gold,
            IsBoast = IsBoastCheck.IsChecked == true
        };

        ViewModel.Project.Boasts.Add(boast);
        ClearBoastInputs();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Boast added.";
        UpdatePreview();
    }

    private void OnRemoveBoast(object sender, RoutedEventArgs e)
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
        BoastRenownInput.Text = "0";
        BoastGoldInput.Text = "0";
        IsBoastCheck.IsChecked = true;
    }

    private void OnAddState(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        string name = StateNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            System.Windows.MessageBox.Show("Please enter a state name.", "State Name", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        string type = (StateTypeInput.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "bool";
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

    private void OnRemoveState(object sender, RoutedEventArgs e)
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

    private void OnThreadSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ThreadsList.SelectedItem is QuestThread thread)
        {
            ThreadFunctionInput.Text = thread.FunctionName;
            ThreadRegionInput.Text = thread.Region;
            ThreadDescriptionInput.Text = thread.Description;
            ThreadIntervalInput.Text = thread.IntervalSeconds.ToString(CultureInfo.InvariantCulture);
            ThreadExitStateInput.Text = thread.ExitStateName;
            ThreadExitValueInput.IsChecked = thread.ExitStateValue;
        }
    }

    private void OnAddThread(object sender, RoutedEventArgs e)
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

        float intervalSeconds = ParseInterval(ThreadIntervalInput.Text.Trim());

        QuestThread thread = new()
        {
            FunctionName = function,
            Region = ThreadRegionInput.Text.Trim(),
            Description = ThreadDescriptionInput.Text.Trim(),
            IntervalSeconds = intervalSeconds,
            ExitStateName = ThreadExitStateInput.Text.Trim(),
            ExitStateValue = ThreadExitValueInput.IsChecked == true
        };

        ViewModel.Project.Threads.Add(thread);
        ClearThreadInputs();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Thread added.";
        UpdatePreview();
    }

    private void OnUpdateThread(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null || ThreadsList.SelectedItem is not QuestThread thread)
        {
            return;
        }

        thread.FunctionName = ThreadFunctionInput.Text.Trim();
        thread.Region = ThreadRegionInput.Text.Trim();
        thread.Description = ThreadDescriptionInput.Text.Trim();
        thread.IntervalSeconds = ParseInterval(ThreadIntervalInput.Text.Trim());
        thread.ExitStateName = ThreadExitStateInput.Text.Trim();
        thread.ExitStateValue = ThreadExitValueInput.IsChecked == true;

        // Refresh the list
        int index = ThreadsList.SelectedIndex;
        ThreadsList.Items.Refresh();
        ThreadsList.SelectedIndex = index;

        ViewModel.IsModified = true;
        ViewModel.StatusText = "Thread updated.";
        UpdatePreview();
    }

    private void OnRemoveThread(object sender, RoutedEventArgs e)
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
        ThreadIntervalInput.Clear();
        ThreadExitStateInput.Clear();
        ThreadExitValueInput.IsChecked = true;
    }

    private static float ParseInterval(string text)
    {
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && value > 0)
        {
            return value;
        }

        return 0.5f;
    }

    private static object? ParseDefault(string type, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return type switch
            {
                "bool" => false,
                "int" => 0,
                "string" => string.Empty,
                _ => null
            };
        }

        return type switch
        {
            "bool" => text.Equals("true", StringComparison.OrdinalIgnoreCase) || text == "1",
            "int" => int.TryParse(text, out int i) ? i : 0,
            "string" => text,
            _ => text
        };
    }

    public void UpdatePreview()
    {
        if (ViewModel?.Project == null || PreviewText == null)
        {
            return;
        }

        try
        {
            var generator = new CodeGenerator();
            string code = generator.GenerateQuestScript(ViewModel.Project);
            PreviewText.Text = code;
        }
        catch (Exception ex)
        {
            PreviewText.Text = $"// Error generating preview:\n// {ex.Message}";
        }
    }
}
