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
