using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Data;
using FableQuestTool.Models;
using FableQuestTool.Services;
using FableQuestTool.ViewModels;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityEditorViewModel : ObservableObject
{
    private readonly MainViewModel mainViewModel;

    public ObservableCollection<EntityTabViewModel> EntityTabs { get; } = new();

    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private EntityTabViewModel? selectedTab;

    public EntityEditorViewModel(MainViewModel mainViewModel)
    {
        this.mainViewModel = mainViewModel;
        LoadExistingEntities();
    }

    private void LoadExistingEntities()
    {
        EntityTabs.Clear();

        foreach (var entity in mainViewModel.Project.Entities)
        {
            var tab = new EntityTabViewModel(entity);
            EntityTabs.Add(tab);
        }

        if (EntityTabs.Count > 0)
        {
            SelectedTabIndex = 0;
            SelectedTab = EntityTabs[0];
        }
    }

    [RelayCommand]
    private void AddNewEntity()
    {
        var newEntity = new QuestEntity
        {
            Id = Guid.NewGuid().ToString(),
            ScriptName = $"Entity{mainViewModel.Project.Entities.Count + 1}",
            DefName = "CREATURE_BOWERSTONE_POSH_VILLAGER_MALE_UNEMPLOYED",
            EntityType = EntityType.Creature,
            SpawnMethod = SpawnMethod.AtMarker,
            SpawnMarker = "MK_OVID_DAD"
        };

        mainViewModel.Project.Entities.Add(newEntity);

        var newTab = new EntityTabViewModel(newEntity);
        EntityTabs.Add(newTab);

        SelectedTabIndex = EntityTabs.Count - 1;
        SelectedTab = newTab;

        mainViewModel.IsModified = true;
        mainViewModel.StatusText = "New entity added.";
    }

    [RelayCommand]
    private void RemoveEntity(EntityTabViewModel? tab)
    {
        if (tab == null)
        {
            return;
        }

        int index = EntityTabs.IndexOf(tab);
        EntityTabs.Remove(tab);
        mainViewModel.Project.Entities.Remove(tab.Entity);

        if (EntityTabs.Count > 0)
        {
            SelectedTabIndex = Math.Max(0, index - 1);
            SelectedTab = EntityTabs[SelectedTabIndex];
        }
        else
        {
            SelectedTab = null;
        }

        mainViewModel.IsModified = true;
        mainViewModel.StatusText = "Entity removed.";
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

        // Deep copy the entity including NEW properties
        var duplicate = new QuestEntity
        {
            Id = Guid.NewGuid().ToString(),
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
            IsQuestTarget = tab.Entity.IsQuestTarget,        // NEW: Copy green highlight setting
            ShowOnMinimap = tab.Entity.ShowOnMinimap,        // NEW: Copy minimap marker setting
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
                Id = Guid.NewGuid().ToString(),
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

        // Copy connections (note: node IDs have changed, so we need to remap)
        // For now, connections are not copied since node IDs change
        // The user can reconnect nodes after duplicating

        mainViewModel.Project.Entities.Add(duplicate);

        var newTab = new EntityTabViewModel(duplicate);
        EntityTabs.Add(newTab);

        SelectedTabIndex = EntityTabs.Count - 1;
        SelectedTab = newTab;

        mainViewModel.IsModified = true;
        mainViewModel.StatusText = "Entity duplicated.";
    }

    public void SaveAllTabs()
    {
        foreach (var tab in EntityTabs)
        {
            tab.SaveToEntity();
        }
    }

    public void RefreshFromProject()
    {
        LoadExistingEntities();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        // Synchronize SelectedTab with SelectedTabIndex when tabs are switched
        if (value >= 0 && value < EntityTabs.Count)
        {
            SelectedTab = EntityTabs[value];
        }
        else
        {
            SelectedTab = null;
        }
    }

    partial void OnSelectedTabChanged(EntityTabViewModel? value)
    {
        // Could add logic here to save previous tab or perform cleanup
    }
}
