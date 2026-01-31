using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FableQuestTool.Models;
using FableQuestTool.Services;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

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

    private void OnRewardAbilitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (RewardAbilitiesList.SelectedItem is string ability)
        {
            RewardAbilityInput.Text = ability;
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

    private void OnUpdateThread(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null || ThreadsList.SelectedItem is not QuestThread thread)
        {
            return;
        }

        thread.FunctionName = ThreadFunctionInput.Text.Trim();
        thread.Region = ThreadRegionInput.Text.Trim();
        thread.Description = ThreadDescriptionInput.Text.Trim();

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
