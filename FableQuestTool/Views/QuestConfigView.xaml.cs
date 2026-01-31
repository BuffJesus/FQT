using System;
using System.Globalization;
using System.Text.RegularExpressions;
using FableQuestTool.Models;
using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class QuestConfigView : System.Windows.Controls.UserControl
{
    private static readonly Regex StateNamePattern = new("^[A-Za-z][A-Za-z0-9_]*$");

    public QuestConfigView()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

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
                RegionInput.Clear();
                return;
            }
        }

        ViewModel.Project.Regions.Add(region);
        RegionInput.Clear();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Region added.";
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
        RewardItemInput.Clear();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Reward item added.";
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
        RewardAbilityInput.Clear();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Reward ability added.";
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

        if (!int.TryParse(BoastRenownInput.Text.Trim(), out int renown))
        {
            renown = 0;
        }

        if (!int.TryParse(BoastGoldInput.Text.Trim(), out int gold))
        {
            gold = 0;
        }

        if (!int.TryParse(BoastTextIdInput.Text.Trim(), out int textId))
        {
            textId = 0;
        }

        QuestBoast boast = new()
        {
            BoastId = boastId,
            Text = BoastTextInput.Text.Trim(),
            RenownReward = renown,
            GoldReward = gold,
            TextId = textId,
            IsBoast = BoastIsBoastInput.IsChecked == true
        };

        ViewModel.Project.Boasts.Add(boast);
        BoastIdInput.Clear();
        BoastTextInput.Clear();
        BoastRenownInput.Clear();
        BoastGoldInput.Clear();
        BoastTextIdInput.Clear();
        BoastIsBoastInput.IsChecked = true;
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Boast added.";
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
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Boast removed.";
        }
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
        ThreadFunctionInput.Clear();
        ThreadRegionInput.Clear();
        ThreadDescriptionInput.Clear();
        ViewModel.IsModified = true;
        ViewModel.StatusText = "Thread added.";
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
            ViewModel.IsModified = true;
            ViewModel.StatusText = "Thread removed.";
        }
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
