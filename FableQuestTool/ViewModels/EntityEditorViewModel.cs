using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FableQuestTool.Models;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    private readonly MainViewModel mainViewModel;

    public ObservableCollection<EntityTabViewModel> EntityTabs { get; } = new();

    [ObservableProperty]
    private EntityTabViewModel? selectedTab;

    [ObservableProperty]
    private int selectedTabIndex = -1;

    public EntityEditorViewModel(MainViewModel mainViewModel)
    {
        this.mainViewModel = mainViewModel;

        // Load existing entities from project
        LoadEntityTabs();

        // Create default entity if none exist
        if (EntityTabs.Count == 0)
        {
            AddNewEntity();
        }
    }

    public void ReloadEntities()
    {
        LoadEntityTabs();

        // Create default entity if none exist
        if (EntityTabs.Count == 0)
        {
            AddNewEntity();
        }
    }

    private void LoadEntityTabs()
    {
        EntityTabs.Clear();

        int totalNodes = 0;
        int totalConnections = 0;

        foreach (var entity in mainViewModel.Project.Entities)
        {
            totalNodes += entity.Nodes.Count;
            totalConnections += entity.Connections.Count;

            var tab = new EntityTabViewModel(entity);
            EntityTabs.Add(tab);
        }

        // Show diagnostic info
        if (EntityTabs.Count > 0)
        {
            var firstEntity = mainViewModel.Project.Entities[0];
            System.Windows.MessageBox.Show(
                $"Loaded {EntityTabs.Count} entities\n" +
                $"First entity: {firstEntity.ScriptName}\n" +
                $"Total nodes: {totalNodes}\n" +
                $"Total connections: {totalConnections}\n" +
                $"First entity nodes: {firstEntity.Nodes.Count}\n" +
                $"First entity connections: {firstEntity.Connections.Count}",
                "Entity Load Debug",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            SelectedTabIndex = 0;
            SelectedTab = EntityTabs[0];
        }
    }

    [RelayCommand]
    private void AddNewEntity()
    {
        var newEntity = new QuestEntity
        {
            Id = System.Guid.NewGuid().ToString(),
            ScriptName = $"Entity{mainViewModel.Project.Entities.Count + 1}",
            EntityType = EntityType.Creature
        };

        mainViewModel.Project.Entities.Add(newEntity);

        var tab = new EntityTabViewModel(newEntity);
        EntityTabs.Add(tab);

        SelectedTabIndex = EntityTabs.Count - 1;
        SelectedTab = tab;
    }

    [RelayCommand]
    private void RemoveEntity(EntityTabViewModel? tab)
    {
        if (tab == null || EntityTabs.Count == 1)
        {
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to remove entity '{tab.Entity.ScriptName}'?",
            "Remove Entity",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        var index = EntityTabs.IndexOf(tab);
        EntityTabs.Remove(tab);
        mainViewModel.Project.Entities.Remove(tab.Entity);

        if (EntityTabs.Count > 0)
        {
            SelectedTabIndex = System.Math.Max(0, index - 1);
            SelectedTab = EntityTabs[SelectedTabIndex];
        }
    }

    [RelayCommand]
    private void DuplicateEntity(EntityTabViewModel? tab)
    {
        if (tab == null)
        {
            return;
        }

        // Save current state to entity model
        tab.SaveToEntity();

        // Deep copy the entity
        var duplicate = new QuestEntity
        {
            Id = System.Guid.NewGuid().ToString(),
            ScriptName = $"{tab.Entity.ScriptName}_Copy",
            DefName = tab.Entity.DefName,
            EntityType = tab.Entity.EntityType,
            ExclusiveControl = tab.Entity.ExclusiveControl,
            AcquireControl = tab.Entity.AcquireControl,
            MakeBehavioral = tab.Entity.MakeBehavioral,
            Invulnerable = tab.Entity.Invulnerable,
            Unkillable = tab.Entity.Unkillable,
            Persistent = tab.Entity.Persistent,
            KillOnLevelUnload = tab.Entity.KillOnLevelUnload,
            SpawnMethod = tab.Entity.SpawnMethod,
            SpawnRegion = tab.Entity.SpawnRegion,
            SpawnMarker = tab.Entity.SpawnMarker,
            SpawnX = tab.Entity.SpawnX,
            SpawnY = tab.Entity.SpawnY,
            SpawnZ = tab.Entity.SpawnZ
        };

        // Copy nodes
        foreach (var node in tab.Entity.Nodes)
        {
            var duplicateNode = new BehaviorNode
            {
                Id = System.Guid.NewGuid().ToString(),
                Type = node.Type,
                Category = node.Category,
                Label = node.Label,
                Icon = node.Icon,
                X = node.X + 30,
                Y = node.Y + 30
            };

            // Deep copy Config dictionary
            if (node.Config != null)
            {
                foreach (var kvp in node.Config)
                {
                    duplicateNode.Config[kvp.Key] = kvp.Value;
                }
            }

            duplicate.Nodes.Add(duplicateNode);
        }

        // Copy connections
        foreach (var conn in tab.Entity.Connections)
        {
            duplicate.Connections.Add(new NodeConnection
            {
                FromNodeId = conn.FromNodeId,
                ToNodeId = conn.ToNodeId,
                FromPort = conn.FromPort,
                ToPort = conn.ToPort
            });
        }

        mainViewModel.Project.Entities.Add(duplicate);

        var newTab = new EntityTabViewModel(duplicate);
        EntityTabs.Add(newTab);

        SelectedTabIndex = EntityTabs.Count - 1;
        SelectedTab = newTab;
    }

    public void SaveAllTabs()
    {
        foreach (var tab in EntityTabs)
        {
            tab.SaveToEntity();
        }
    }

    partial void OnSelectedTabChanged(EntityTabViewModel? value)
    {
        // Could add logic here to save previous tab or perform cleanup
    }
}
