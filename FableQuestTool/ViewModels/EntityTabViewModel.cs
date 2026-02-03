using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FableQuestTool.Config;
using FableQuestTool.Data;
using FableQuestTool.Models;
using FableQuestTool.Services;

namespace FableQuestTool.ViewModels;

public sealed partial class EntityTabViewModel : ObservableObject
{
    private readonly QuestEntity entity;
    private readonly EntityBrowserService entityBrowserService;
    private List<TngEntity> allEntities = new();

    public ObservableCollection<string> AvailableDefinitions { get; } = new();
    public ObservableCollection<string> AvailableRegions { get; } = new();
    public ObservableCollection<string> AvailableMarkers { get; } = new();
    public ObservableCollection<string> AvailableEvents { get; } = new();
    public ObservableCollection<VariableDefinition> Variables { get; } = new();

    // Object Reward properties
    public ObservableCollection<string> ObjectRewardItems { get; } = new();
    public ObservableCollection<string> AvailableRewardItems { get; } = new();

    [ObservableProperty]
    private string newRewardItem = string.Empty;

    [ObservableProperty]
    private string tabTitle;

    [ObservableProperty]
    private string entityIcon;

    public QuestEntity Entity => entity;

    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<NodeOption> SimpleNodes { get; } = new();
    public ObservableCollection<NodeOption> AdvancedNodes { get; } = new();

    [ObservableProperty]
    private NodeViewModel? selectedNode;

    [ObservableProperty]
    private PendingConnectionViewModel? pendingConnection;

    [ObservableProperty]
    private bool isNodeMenuOpen;

    [ObservableProperty]
    private System.Windows.Point nodeMenuPosition;

    [ObservableProperty]
    private System.Windows.Point nodeMenuGraphPosition;

    [ObservableProperty]
    private string nodeSearchText = string.Empty;

    [ObservableProperty]
    private int nodeMenuSelectedIndex = -1;

    private ConnectorViewModel? menuConnectionSource;

    public ObservableCollection<NodeOption> FilteredNodes { get; } = new();
    public ObservableCollection<NodeCategoryGroup> GroupedFilteredNodes { get; } = new();

    [ObservableProperty]
    private string newVariableName = string.Empty;

    [ObservableProperty]
    private string newVariableType = "String";

    private int nodeSeed = 0;
    private readonly Dictionary<string, int> variableUsageIndices = new(StringComparer.OrdinalIgnoreCase);

    public EntityTabViewModel(QuestEntity entity)
    {
        this.entity = entity;
        var config = FableConfig.Load();
        this.entityBrowserService = new EntityBrowserService(config);

        System.Diagnostics.Debug.WriteLine($"EntityTabViewModel Constructor: FablePath = {config.FablePath ?? "NULL"}");

        tabTitle = string.IsNullOrWhiteSpace(entity.ScriptName) ? "New Entity" : entity.ScriptName;
        entityIcon = GetEntityIcon(entity.EntityType);

        // Listen for property changes to update tab title
        entity.PropertyChanged += Entity_PropertyChanged;

        LoadNodePalette();
        LoadVariablesFromEntity();
        UpdateVariableNodes();
        LoadExistingNodes();
        AttachNodeHandlers();
        UpdateAvailableEvents();
        UpdateVariableUsageCounts();
        LoadDropdownDataAsync(config);
        LoadAvailableRewardItems();
        SyncObjectRewardItemsFromEntity();

        // Listen for node collection changes to update available events and variable usage
        Nodes.CollectionChanged += (s, e) =>
        {
            UpdateAvailableEvents();
            HandleNodeCollectionChanged(e);
            UpdateVariableUsageCounts();
        };
    }

    private async void LoadDropdownDataAsync(FableConfig config)
    {
        try
        {
            // Check if Fable path is configured
            if (string.IsNullOrWhiteSpace(config.FablePath))
            {
                System.Diagnostics.Debug.WriteLine("EntityTabViewModel: Fable path not configured, using fallback static data");
                LoadFallbackDefinitions();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Loading entities from {config.FablePath}");

            // Load all entities to populate dropdowns
            allEntities = await System.Threading.Tasks.Task.Run(() => entityBrowserService.GetAllEntities());

            if (allEntities == null || allEntities.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("EntityTabViewModel: No entities loaded from game files, using fallback static data");
                LoadFallbackDefinitions();
                return;
            }

            // Debug: Show sample of loaded entities
            var sampleEntities = allEntities.Take(5).ToList();
            System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Sample entities loaded:");
            foreach (var e in sampleEntities)
            {
                System.Diagnostics.Debug.WriteLine($"  - DefType: {e.DefinitionType}, Region: {e.RegionName}, Category: {e.Category}");
            }

            // Debug: Show unique regions
            var uniqueRegions = allEntities.Where(e => !string.IsNullOrWhiteSpace(e.RegionName))
                .Select(e => e.RegionName).Distinct().OrderBy(r => r).Take(10).ToList();
            System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Sample regions found: {string.Join(", ", uniqueRegions)}");

            // Initial definition load
            UpdateAvailableDefinitions();

            System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Loaded {allEntities.Count} entities, {AvailableDefinitions.Count} definitions available");

            // Get unique region names
            var regions = RegionTngMapping.GetAllRegionNames()
                .OrderBy(r => r)
                .ToList();

            AvailableRegions.Clear();
            foreach (var region in regions)
            {
                AvailableRegions.Add(region);
            }

            // Initial marker load
            UpdateAvailableMarkers();

            // Listen for property changes to update dropdowns
            entity.PropertyChanged += OnEntityPropertyChangedForDropdowns;
        }
        catch (Exception ex)
        {
            // If loading fails, populate with fallback static data
            System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Failed to load dropdown data: {ex.Message}");
            LoadFallbackDefinitions();
        }
    }

    private void LoadFallbackDefinitions()
    {
        AvailableDefinitions.Clear();

        // Populate based on entity type
        switch (entity.EntityType)
        {
            case EntityType.Creature:
                foreach (var creature in GameData.Creatures)
                {
                    AvailableDefinitions.Add(creature);
                }
                break;
            case EntityType.Object:
                foreach (var obj in GameData.Objects)
                {
                    AvailableDefinitions.Add(obj);
                }
                break;
        }

        // Populate regions
        AvailableRegions.Clear();
        foreach (var region in GameData.Regions)
        {
            AvailableRegions.Add(region);
        }

        System.Diagnostics.Debug.WriteLine($"EntityTabViewModel: Loaded fallback definitions - {AvailableDefinitions.Count} definitions");
    }

    private void OnEntityPropertyChangedForDropdowns(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QuestEntity.EntityType))
        {
            UpdateAvailableDefinitions();
        }
        else if (e.PropertyName == nameof(QuestEntity.SpawnRegion))
        {
            // Only update definitions if we have game data (not using fallback)
            if (allEntities != null && allEntities.Count > 0)
            {
                UpdateAvailableDefinitions();
                UpdateAvailableMarkers(); // Also update markers when region changes
            }
        }
        else if (e.PropertyName == nameof(QuestEntity.SpawnMethod))
        {
            UpdateAvailableMarkers();
        }
    }

    private void UpdateAvailableDefinitions()
    {
        AvailableDefinitions.Clear();

        if (allEntities == null || allEntities.Count == 0)
        {
            // If no entities loaded from game files, use fallback static data
            LoadFallbackDefinitions();
            return;
        }

        IEnumerable<TngEntity> filteredEntities = allEntities;

        // Filter by spawn region if specified
        if (!string.IsNullOrWhiteSpace(entity.SpawnRegion))
        {
            int beforeCount = filteredEntities.Count();
            filteredEntities = filteredEntities.Where(e =>
                e.RegionName != null &&
                e.RegionName.Equals(entity.SpawnRegion, StringComparison.OrdinalIgnoreCase));
            int afterCount = filteredEntities.Count();
            System.Diagnostics.Debug.WriteLine($"Region filter '{entity.SpawnRegion}': {beforeCount} -> {afterCount} entities");
        }

        // Filter by entity type
        switch (entity.EntityType)
        {
            case EntityType.Creature:
                filteredEntities = filteredEntities.Where(e =>
                    e.Category == EntityCategory.Creature ||
                    e.Category == EntityCategory.NPC ||
                    e.Category == EntityCategory.Enemy ||
                    e.Category == EntityCategory.CreatureGenerator);
                break;

            case EntityType.Object:
                filteredEntities = filteredEntities.Where(e =>
                    e.Category == EntityCategory.Object ||
                    e.Category == EntityCategory.Chest ||
                    e.Category == EntityCategory.Door ||
                    e.Category == EntityCategory.QuestItem);
                break;

            case EntityType.Effect:
                // For effects, show all entities as there's no specific filter
                // Could be extended if effect-specific entities are identified
                break;

            case EntityType.Light:
                // For lights, show all entities as there's no specific filter
                // Could be extended if light-specific entities are identified
                break;
        }

        var definitions = filteredEntities
            .Select(e => e.DefinitionType)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var def in definitions)
        {
            AvailableDefinitions.Add(def);
        }

        System.Diagnostics.Debug.WriteLine($"UpdateAvailableDefinitions: Region={entity.SpawnRegion}, Type={entity.EntityType}, Found {definitions.Count} definitions");
    }

    private void UpdateAvailableMarkers()
    {
        AvailableMarkers.Clear();

        if (allEntities == null || allEntities.Count == 0)
        {
            return;
        }

        var region = entity.SpawnRegion;
        var spawnMethod = entity.SpawnMethod;

        IEnumerable<TngEntity> filteredEntities = allEntities;

        // Filter by region if specified
        if (!string.IsNullOrWhiteSpace(region))
        {
            filteredEntities = filteredEntities.Where(e =>
                string.Equals(e.RegionName, region, System.StringComparison.OrdinalIgnoreCase));
        }

        // Filter by spawn method and entity type
        if (spawnMethod == SpawnMethod.BindExisting)
        {
            // For BindExisting, show only scriptable entities (those with script names)
            filteredEntities = filteredEntities.Where(e => e.HasScriptName);
        }
        else if (spawnMethod == SpawnMethod.AtMarker)
        {
            // For AtMarker, show only Marker entities with script names
            filteredEntities = filteredEntities.Where(e =>
                e.Category == EntityCategory.Marker && e.HasScriptName);
        }

        var markers = filteredEntities
            .Select(e => e.ScriptName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"UpdateAvailableMarkers: Region={region}, Method={spawnMethod}, Found {markers.Count} markers");

        foreach (var marker in markers)
        {
            AvailableMarkers.Add(marker);
        }
    }

    private void Entity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QuestEntity.ScriptName) || e.PropertyName == nameof(QuestEntity.EntityType))
        {
            UpdateTabTitle();
        }
    }

    private void UpdateAvailableEvents()
    {
        AvailableEvents.Clear();

        // Find all "defineEvent" nodes and extract their event names
        var eventNodes = Nodes.Where(n => n.Type == "defineEvent");

        foreach (var node in eventNodes)
        {
            if (node.Properties.TryGetValue("eventName", out var eventName))
            {
                var eventNameStr = eventName?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(eventNameStr) && !AvailableEvents.Contains(eventNameStr))
                {
                    AvailableEvents.Add(eventNameStr);
                }
            }
        }

        // Sort alphabetically
        var sorted = AvailableEvents.OrderBy(e => e).ToList();
        AvailableEvents.Clear();
        foreach (var evt in sorted)
        {
            AvailableEvents.Add(evt);
        }
    }

    private void LoadNodePalette()
    {
        var allNodes = NodeDefinitions.GetAllNodes();

        foreach (var node in allNodes.Where(n => !n.IsAdvanced))
        {
            SimpleNodes.Add(new NodeOption(node.Label, node.Category, node.Icon)
            {
                Type = node.Type,
                Definition = node
            });
        }

        foreach (var node in allNodes.Where(n => n.IsAdvanced))
        {
            AdvancedNodes.Add(new NodeOption(node.Label, node.Category, node.Icon)
            {
                Type = node.Type,
                Definition = node
            });
        }
    }

    private void LoadExistingNodes()
    {
        // Load nodes from entity model
        foreach (var node in entity.Nodes)
        {
            var nodeDef = NodeDefinitions.GetAllNodes().FirstOrDefault(n => n.Type == node.Type);
            if (nodeDef == null)
            {
                nodeDef = TryBuildVariableNodeDefinition(node);
            }

            if (nodeDef != null)
            {
                var nodeVm = new NodeViewModel
                {
                    Id = node.Id, // Preserve the saved ID
                    Title = nodeDef.Label,
                    Category = nodeDef.Category,
                    Icon = nodeDef.Icon,
                    Type = node.Type,
                    Definition = nodeDef,
                    Location = new System.Windows.Point(node.X, node.Y)
                };

                // Convert Config (object values) to Properties (string values)
                if (node.Config != null)
                {
                    foreach (var kvp in node.Config)
                    {
                        nodeVm.Properties[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                    }
                }

                // Initialize connectors (True/False for branching nodes)
                nodeVm.InitializeConnectors();

                // Update title for event nodes to show event name
                nodeVm.UpdateTitleForEventNodes();

                Nodes.Add(nodeVm);
            }
            else if (node.Type == "reroute")
            {
                var rerouteNode = new NodeViewModel
                {
                    Id = node.Id,
                    Title = string.Empty,
                    Category = "flow",
                    Icon = "◇",
                    Type = "reroute",
                    IsRerouteNode = true,
                    IsRedirectionNode = true,
                    Location = new System.Windows.Point(node.X, node.Y)
                };

                rerouteNode.Input.Clear();
                rerouteNode.Output.Clear();

                rerouteNode.Input.Add(new ConnectorViewModel
                {
                    Title = string.Empty,
                    ConnectorType = ConnectorType.Exec,
                    IsInput = true
                });
                rerouteNode.Output.Add(new ConnectorViewModel
                {
                    Title = string.Empty,
                    ConnectorType = ConnectorType.Exec,
                    IsInput = false
                });

                Nodes.Add(rerouteNode);
            }
        }

        EnsureFlowOutputConnectors();

        // Load connections from entity model
        foreach (var conn in entity.Connections)
        {
            var sourceNode = Nodes.FirstOrDefault(n => n.Id == conn.FromNodeId);
            var targetNode = Nodes.FirstOrDefault(n => n.Id == conn.ToNodeId);

            if (sourceNode != null && targetNode != null)
            {
                // Find the correct output connector by FromPort name (case-insensitive)
                ConnectorViewModel? sourceConnector = null;
                if (!string.IsNullOrEmpty(conn.FromPort))
                {
                    sourceConnector = sourceNode.Output.FirstOrDefault(o =>
                        string.Equals(o.Title, conn.FromPort, StringComparison.OrdinalIgnoreCase));
                }
                // Fallback to first output if not found or no FromPort specified
                if (sourceConnector == null)
                {
                    sourceConnector = sourceNode.Output.FirstOrDefault();
                }

                // Find the correct input connector by ToPort name (case-insensitive)
                ConnectorViewModel? targetConnector = null;
                if (!string.IsNullOrEmpty(conn.ToPort))
                {
                    targetConnector = targetNode.Input.FirstOrDefault(i =>
                        string.Equals(i.Title, conn.ToPort, StringComparison.OrdinalIgnoreCase));
                }
                // Fallback to first input if not found or no ToPort specified
                if (targetConnector == null)
                {
                    targetConnector = targetNode.Input.FirstOrDefault();
                }

                if (sourceConnector != null && targetConnector != null)
                {
                    Connections.Add(new ConnectionViewModel
                    {
                        Source = sourceConnector,
                        Target = targetConnector
                    });
                }
            }
        }
    }

    public void SaveToEntity()
    {
        SaveVariablesToEntity();

        // Save nodes back to entity model
        entity.Nodes.Clear();
        foreach (var node in Nodes)
        {
            var behaviorNode = new BehaviorNode
            {
                Id = node.Id,
                Type = node.Type,
                Category = node.Category,
                Label = node.Title,
                Icon = node.Icon,
                X = node.Location.X,
                Y = node.Location.Y
            };

            // Convert Properties (string values) to Config (object values)
            foreach (var kvp in node.Properties)
            {
                behaviorNode.Config[kvp.Key] = kvp.Value;
            }

            entity.Nodes.Add(behaviorNode);
        }

        // Save connections
        entity.Connections.Clear();
        foreach (var conn in Connections)
        {
            if (conn.Source == null || conn.Target == null)
            {
                continue;
            }

            var sourceNode = Nodes.FirstOrDefault(n => n.Output.Contains(conn.Source));
            var targetNode = Nodes.FirstOrDefault(n => n.Input.Contains(conn.Target));

            if (sourceNode != null && targetNode != null)
            {
                entity.Connections.Add(new NodeConnection
                {
                    FromNodeId = sourceNode.Id,
                    FromPort = conn.Source?.Title ?? "Output",
                    ToNodeId = targetNode.Id,
                    ToPort = conn.Target?.Title ?? "Input"
                });
            }
        }
    }

    private void EnsureFlowOutputConnectors()
    {
        var flowNodes = Nodes.Where(n => n.Category == "flow").ToList();
        if (flowNodes.Count == 0)
        {
            return;
        }

        var connectionsBySource = entity.Connections
            .GroupBy(c => c.FromNodeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var sourceNode in flowNodes)
        {
            if (!connectionsBySource.TryGetValue(sourceNode.Id, out var sourceConnections))
            {
                // Ensure a single default output pin exists
                if (sourceNode.Output.Count == 0)
                {
                    sourceNode.Output.Add(new ConnectorViewModel
                    {
                        Title = "▶",
                        ConnectorType = ConnectorType.Exec,
                        IsInput = false
                    });
                }

                // Remove extra unused outputs
                while (sourceNode.Output.Count > 1)
                {
                    sourceNode.Output.RemoveAt(sourceNode.Output.Count - 1);
                }

                continue;
            }

            var requiredPorts = sourceConnections
                .Select(c => c.FromPort)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requiredPorts.Count == 0)
            {
                if (sourceNode.Output.Count == 0)
                {
                    sourceNode.Output.Add(new ConnectorViewModel
                    {
                        Title = "▶",
                        ConnectorType = ConnectorType.Exec,
                        IsInput = false
                    });
                }

                while (sourceNode.Output.Count > 1)
                {
                    sourceNode.Output.RemoveAt(sourceNode.Output.Count - 1);
                }

                continue;
            }

            // Dedupe existing outputs by title, keep the first instance.
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = sourceNode.Output.Count - 1; i >= 0; i--)
            {
                var title = sourceNode.Output[i].Title ?? string.Empty;
                if (existing.Contains(title))
                {
                    sourceNode.Output.RemoveAt(i);
                }
                else
                {
                    existing.Add(title);
                }
            }

            // Add any missing ports from saved connections.
            foreach (var port in requiredPorts)
            {
                bool exists = sourceNode.Output.Any(o =>
                    string.Equals(o.Title, port, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    sourceNode.Output.Add(new ConnectorViewModel
                    {
                        Title = port,
                        ConnectorType = ConnectorType.Exec,
                        IsInput = false
                    });
                }
            }

            // Remove outputs that are no longer referenced.
            for (int i = sourceNode.Output.Count - 1; i >= 0; i--)
            {
                var title = sourceNode.Output[i].Title ?? string.Empty;
                if (!requiredPorts.Contains(title, StringComparer.OrdinalIgnoreCase))
                {
                    sourceNode.Output.RemoveAt(i);
                }
            }

            // Order outputs to match connection port order.
            var ordered = requiredPorts
                .Select(port => sourceNode.Output.FirstOrDefault(o =>
                    string.Equals(o.Title, port, StringComparison.OrdinalIgnoreCase)))
                .Where(o => o != null)
                .ToList();

            sourceNode.Output.Clear();
            foreach (var output in ordered)
            {
                sourceNode.Output.Add(output!);
            }
        }
    }

    private void AttachNodeHandlers()
    {
        foreach (var node in Nodes)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.PropertyChanged += OnNodePropertyChanged;
        }
    }

    private void HandleNodeCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (NodeViewModel node in e.NewItems)
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (NodeViewModel node in e.OldItems)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
            }
        }
    }

    private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeViewModel.Properties))
        {
            UpdateVariableUsageCounts();
        }
        else if (e.PropertyName == nameof(NodeViewModel.Location) && sender is NodeViewModel node)
        {
            UpdateReroutePositionsForNode(node);
        }
    }

    [RelayCommand]
    private void AddNode(NodeOption option)
    {
        if (option?.Definition == null)
        {
            return;
        }

        nodeSeed++;
        double offset = 30 * (nodeSeed % 6);
        Nodes.Add(CreateNode(option.Definition, 140 + offset, 140 + offset));
    }

    [RelayCommand]
    private void ApplyTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return;
        }

        var allNodes = NodeDefinitions.GetAllNodes();
        Nodes.Clear();
        Connections.Clear();

        if (template == "Talk")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onHeroTalks"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "showDialogue"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
        else if (template == "Kill")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onHeroHits"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "giveReward"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
        else if (template == "Fetch")
        {
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "onItemPresented"), 120, 120));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "giveReward"), 420, 160));
            Nodes.Add(CreateNode(allNodes.First(n => n.Type == "completeQuest"), 720, 200));
        }
        else if (template == "Reward")
        {
            // Set up as a reward object - configure entity settings
            entity.EntityType = EntityType.Object;
            entity.SpawnMethod = SpawnMethod.AtMarker;

            // Set a default object definition if empty
            if (string.IsNullOrWhiteSpace(entity.DefName))
            {
                entity.DefName = "OBJECT_CHEST_SILVER";
            }

            // Set a default script name if empty
            if (string.IsNullOrWhiteSpace(entity.ScriptName))
            {
                entity.ScriptName = "RewardObject";
            }

            // Enable object rewards with defaults
            entity.ObjectReward = new ObjectReward
            {
                Gold = 100,
                Experience = 50,
                OneTimeOnly = true,
                DestroyAfterReward = true,
                ShowMessage = true
            };

            // Add a default item
            entity.ObjectReward.Items.Add("OBJECT_HEALTH_POTION");

            // Sync to UI
            SyncObjectRewardItemsFromEntity();
            OnPropertyChanged(nameof(HasObjectReward));
            OnPropertyChanged(nameof(ObjectRewardGold));
            OnPropertyChanged(nameof(ObjectRewardExperience));
            OnPropertyChanged(nameof(ObjectRewardOneTimeOnly));
            OnPropertyChanged(nameof(ObjectRewardDestroyAfter));
            OnPropertyChanged(nameof(ObjectRewardShowMessage));

            // Update dropdowns for Object type
            UpdateAvailableDefinitions();
            UpdateTabTitle();
        }
    }

    [RelayCommand]
    private void DeleteNode()
    {
        if (SelectedNode == null)
        {
            return;
        }

        RemoveConnectionsForNode(SelectedNode);

        Nodes.Remove(SelectedNode);
        SelectedNode = null;
    }

    [RelayCommand]
    private void DuplicateNode()
    {
        if (SelectedNode?.Definition == null)
        {
            return;
        }

        var duplicate = CreateNode(SelectedNode.Definition,
            SelectedNode.Location.X + 30,
            SelectedNode.Location.Y + 30);

        foreach (var prop in SelectedNode.Properties)
        {
            duplicate.Properties[prop.Key] = prop.Value;
        }

        Nodes.Add(duplicate);
        SelectedNode = duplicate;
    }

    [RelayCommand]
    private void DeleteNodeFromContext(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }

        SelectedNode = node;
        DeleteNode();
    }

    [RelayCommand]
    private void DuplicateNodeFromContext(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }

        SelectedNode = node;
        DuplicateNode();
    }

    [RelayCommand]
    private void BreakNodeLinksFromContext(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }

        RemoveConnectionsForNode(node);
    }

    [RelayCommand]
    private void AddOutputPinFromContext(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }

        SelectedNode = node;
        AddOutputPin();
    }

    [RelayCommand]
    private void RemoveOutputPinFromContext(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }

        SelectedNode = node;
        RemoveOutputPin();
    }

    [RelayCommand]
    private void AddOutputPin()
    {
        if (SelectedNode == null || SelectedNode.Category != "flow" || SelectedNode.IsRerouteNode)
        {
            return;
        }

        var pinNumber = SelectedNode.Output.Count;
        SelectedNode.Output.Add(new ConnectorViewModel
        {
            Title = $"Then {pinNumber}"
        });
    }

    [RelayCommand]
    private void RemoveOutputPin()
    {
        if (SelectedNode == null || SelectedNode.Output.Count <= 1 || SelectedNode.Category != "flow" || SelectedNode.IsRerouteNode)
        {
            return;
        }

        var lastOutput = SelectedNode.Output.LastOrDefault();
        if (lastOutput != null)
        {
            var connectionsToRemove = Connections.Where(c => c.Source == lastOutput).ToList();
            foreach (var conn in connectionsToRemove)
            {
                Connections.Remove(conn);
            }

            SelectedNode.Output.Remove(lastOutput);
        }
    }

    [RelayCommand]
    private void BreakConnection(ConnectionViewModel? connection)
    {
        if (connection == null)
        {
            return;
        }

        Connections.Remove(connection);
        UpdateReroutePositionsForConnection(connection);
    }

    [RelayCommand]
    private void InsertRerouteOnConnectionFromContext(ConnectionViewModel? connection)
    {
        if (connection?.Source == null || connection.Target == null)
        {
            return;
        }

        var source = connection.Source.Anchor;
        var target = connection.Target.Anchor;
        var location = new System.Windows.Point((source.X + target.X) / 2.0, (source.Y + target.Y) / 2.0);
        CreateRerouteOnConnection((connection, location));
    }

    private void RemoveConnectionsForNode(NodeViewModel node)
    {
        if (node.Input.Count == 0 && node.Output.Count == 0)
        {
            return;
        }

        var connectors = new HashSet<ConnectorViewModel>();
        foreach (var input in node.Input)
        {
            connectors.Add(input);
        }

        foreach (var output in node.Output)
        {
            connectors.Add(output);
        }

        var connectionsToRemove = Connections.Where(c =>
            (c.Source != null && connectors.Contains(c.Source)) ||
            (c.Target != null && connectors.Contains(c.Target))).ToList();

        foreach (var conn in connectionsToRemove)
        {
            Connections.Remove(conn);
            UpdateReroutePositionsForConnection(conn);
        }
    }

    private bool isUpdatingReroutePositions;

    private void UpdateReroutePositionsForNode(NodeViewModel movedNode)
    {
        if (isUpdatingReroutePositions)
        {
            return;
        }

        var connectedReroutes = new HashSet<NodeViewModel>();
        foreach (var conn in Connections)
        {
            var sourceNode = GetNodeForConnector(conn.Source, isOutput: true);
            var targetNode = GetNodeForConnector(conn.Target, isOutput: false);

            if (sourceNode == movedNode && targetNode?.IsRerouteNode == true)
            {
                connectedReroutes.Add(targetNode);
            }
            else if (targetNode == movedNode && sourceNode?.IsRerouteNode == true)
            {
                connectedReroutes.Add(sourceNode);
            }
        }

        foreach (var reroute in connectedReroutes)
        {
            UpdateReroutePosition(reroute);
        }
    }

    private void UpdateReroutePositionsForConnection(ConnectionViewModel connection)
    {
        var sourceNode = GetNodeForConnector(connection.Source, isOutput: true);
        var targetNode = GetNodeForConnector(connection.Target, isOutput: false);

        if (sourceNode?.IsRerouteNode == true)
        {
            UpdateReroutePosition(sourceNode);
        }
        if (targetNode?.IsRerouteNode == true)
        {
            UpdateReroutePosition(targetNode);
        }
    }

    private void UpdateReroutePosition(NodeViewModel rerouteNode)
    {
        if (!rerouteNode.IsRerouteNode || isUpdatingReroutePositions)
        {
            return;
        }

        var rerouteInput = rerouteNode.Input.FirstOrDefault();
        var rerouteOutput = rerouteNode.Output.FirstOrDefault();
        if (rerouteInput == null || rerouteOutput == null)
        {
            return;
        }

        var incoming = Connections.FirstOrDefault(c => c.Target == rerouteInput);
        var outgoing = Connections.FirstOrDefault(c => c.Source == rerouteOutput);
        if (incoming?.Source == null || outgoing?.Target == null)
        {
            return;
        }

        var from = incoming.Source.Anchor;
        var to = outgoing.Target.Anchor;
        var mid = new System.Windows.Point((from.X + to.X) / 2.0, (from.Y + to.Y) / 2.0);

        isUpdatingReroutePositions = true;
        rerouteNode.Location = mid;
        isUpdatingReroutePositions = false;
    }

    private NodeViewModel? GetNodeForConnector(ConnectorViewModel? connector, bool isOutput)
    {
        if (connector == null)
        {
            return null;
        }

        return isOutput
            ? Nodes.FirstOrDefault(n => n.Output.Contains(connector))
            : Nodes.FirstOrDefault(n => n.Input.Contains(connector));
    }

    [RelayCommand]
    private void StartConnection(ConnectorViewModel? connector)
    {
        if (connector == null)
        {
            return;
        }

        PendingConnection = new PendingConnectionViewModel
        {
            Source = connector
        };
    }

    [RelayCommand]
    private void FinishConnection(object? parameter)
    {
        try
        {
            ConnectorViewModel? targetConnector = null;

            if (parameter is ConnectorViewModel connector)
            {
                targetConnector = connector;
            }
            else if (parameter is ValueTuple<object, object> tuple)
            {
                targetConnector = tuple.Item2 as ConnectorViewModel;
            }

            if (PendingConnection?.Source == null || targetConnector == null)
            {
                PendingConnection = null;
                return;
            }

            if (PendingConnection.Source == targetConnector)
            {
                PendingConnection = null;
                return;
            }

            var sourceConnector = PendingConnection.Source;

            // Enforce output -> input only; swap if started from input.
            if (sourceConnector.IsInput && !targetConnector.IsInput)
            {
                (sourceConnector, targetConnector) = (targetConnector, sourceConnector);
            }
            else if (sourceConnector.IsInput == targetConnector.IsInput)
            {
                PendingConnection = null;
                return;
            }

            // Ensure only one connection per input connector.
            var existingToTarget = Connections.Where(c => c.Target == targetConnector).ToList();
            foreach (var conn in existingToTarget)
            {
                Connections.Remove(conn);
                UpdateReroutePositionsForConnection(conn);
            }

            var existingConnection = Connections.FirstOrDefault(c =>
                c.Source == sourceConnector && c.Target == targetConnector);
            if (existingConnection != null)
            {
                PendingConnection = null;
                return;
            }

            var connection = new ConnectionViewModel
            {
                Source = sourceConnector,
                Target = targetConnector
            };

            Connections.Add(connection);
            UpdateReroutePositionsForConnection(connection);
            PendingConnection = null;
        }
        catch
        {
            PendingConnection = null;
        }
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorViewModel? connector)
    {
        if (connector == null)
        {
            return;
        }

        var connectionsToRemove = Connections.Where(c =>
            c.Source == connector || c.Target == connector).ToList();

        foreach (var conn in connectionsToRemove)
        {
            Connections.Remove(conn);
            UpdateReroutePositionsForConnection(conn);
        }
    }

    [RelayCommand]
    private void OpenNodeMenu(object? parameter)
    {
        if (parameter is ValueTuple<System.Windows.Point, System.Windows.Point> tuple)
        {
            NodeMenuPosition = tuple.Item1;
            NodeMenuGraphPosition = tuple.Item2;
        }
        else if (parameter is System.Windows.Point position)
        {
            NodeMenuPosition = position;
            NodeMenuGraphPosition = position;
        }

        if (PendingConnection?.Source != null)
        {
            menuConnectionSource = PendingConnection.Source;
            PendingConnection = null;
        }
        else
        {
            menuConnectionSource = null;
        }

        NodeSearchText = string.Empty;
        UpdateFilteredNodes();
        IsNodeMenuOpen = true;
    }

    [RelayCommand]
    private void CloseNodeMenu()
    {
        IsNodeMenuOpen = false;
        NodeSearchText = string.Empty;
        menuConnectionSource = null;
    }

    [RelayCommand]
    private void SelectNodeFromMenu(NodeOption? option)
    {
        if (option?.Definition == null)
        {
            return;
        }

        NodeMenuSelectedIndex = option.MenuIndex;
        nodeSeed++;
        var newNode = CreateNode(option.Definition, NodeMenuGraphPosition.X, NodeMenuGraphPosition.Y);
        Nodes.Add(newNode);

        var source = menuConnectionSource;
        if (source != null)
        {
            var targetConnector = newNode.Input.FirstOrDefault();
            if (targetConnector != null)
            {
                var connection = new ConnectionViewModel
                {
                    Source = source,
                    Target = targetConnector
                };
                Connections.Add(connection);
            }
            menuConnectionSource = null;
        }

        IsNodeMenuOpen = false;
    }

    partial void OnNodeSearchTextChanged(string value)
    {
        UpdateFilteredNodes();
    }

    private void UpdateFilteredNodes()
    {
        FilteredNodes.Clear();
        GroupedFilteredNodes.Clear();

        var allNodes = SimpleNodes.Concat(AdvancedNodes).ToList();
        bool hideTriggers = ShouldHideTriggerNodes();

        IEnumerable<NodeOption> filteredNodes;

        if (string.IsNullOrWhiteSpace(NodeSearchText))
        {
            filteredNodes = allNodes;
        }
        else
        {
            var searchLower = NodeSearchText.ToLower();
            filteredNodes = allNodes.Where(n =>
                n.Label.ToLower().Contains(searchLower) ||
                n.Category.ToLower().Contains(searchLower) ||
                (n.Type?.ToLower().Contains(searchLower) == true));
        }

        if (hideTriggers)
        {
            filteredNodes = filteredNodes.Where(n => n.Category != "trigger");
        }

        // Add nodes to flat list
        int menuIndex = 0;
        foreach (var node in filteredNodes)
        {
            node.MenuIndex = menuIndex;
            FilteredNodes.Add(node);
            menuIndex++;
        }

        if (FilteredNodes.Count > 0)
        {
            NodeMenuSelectedIndex = Math.Clamp(NodeMenuSelectedIndex, 0, FilteredNodes.Count - 1);
        }
        else
        {
            NodeMenuSelectedIndex = -1;
        }

        // Group nodes by category for better organization
        var categoryOrder = new[] { "trigger", "action", "condition", "flow", "custom", "variable" };
        var grouped = filteredNodes
            .GroupBy(n => n.Category)
            .OrderBy(g => Array.IndexOf(categoryOrder, g.Key) >= 0
                ? Array.IndexOf(categoryOrder, g.Key)
                : 999)
            .ToList();

        foreach (var group in grouped)
        {
            var categoryName = group.Key switch
            {
                "trigger" => "Triggers (Events)",
                "action" => "Actions",
                "condition" => "Conditions",
                "flow" => "Flow Control",
                "custom" => "Custom Events",
                "variable" => "Variables",
                _ => group.Key
            };

            GroupedFilteredNodes.Add(new NodeCategoryGroup(categoryName, group.ToList()));
        }
    }

    /// <summary>
    /// Check if there's a pending connection (used for UI hints)
    /// </summary>
    public bool HasPendingConnection => PendingConnection?.Source != null || menuConnectionSource != null;

    /// <summary>
    /// Get a description of what's being dragged
    /// </summary>
    public string PendingConnectionHint => PendingConnection?.Source != null
        ? $"Release to connect from {PendingConnection.Source.Title}"
        : menuConnectionSource != null
            ? $"Release to connect from {menuConnectionSource.Title}"
        : "Right-click to add nodes";

    [RelayCommand]
    private void CreateRedirectionNode(System.Windows.Point location)
    {
        CreateRerouteNode(location);
    }

    /// <summary>
    /// Creates a UE5-style reroute node - a small circular node for organizing wires
    /// </summary>
    [RelayCommand]
    private void CreateRerouteNode(System.Windows.Point location)
    {
        if (PendingConnection?.Source == null)
        {
            return;
        }

        var sourceConnector = PendingConnection.Source;

        // Create a minimal reroute node (UE5 style)
        var rerouteNode = new NodeViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = "",  // No title for reroute nodes
            Category = "flow",
            Icon = "â—†",  // Diamond icon
            Type = "reroute",
            IsRerouteNode = true,
            IsRedirectionNode = true,  // Keep for backwards compatibility
            Location = location
        };

        // Clear default connectors and add single input/output
        rerouteNode.Input.Clear();
        rerouteNode.Output.Clear();

        // Match the connector type from the source
        var connType = sourceConnector.ConnectorType;

        rerouteNode.Input.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = true
        });
        rerouteNode.Output.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = false
        });

        Nodes.Add(rerouteNode);

        // Connect from source to the reroute node
        var targetConnector = rerouteNode.Input.FirstOrDefault();
        if (targetConnector != null)
        {
            var connection = new ConnectionViewModel
            {
                Source = sourceConnector,
                Target = targetConnector
            };
            Connections.Add(connection);
        }

        PendingConnection = null;
    }

    /// <summary>
    /// Creates a reroute node on an existing connection (double-click on wire)
    /// </summary>
    [RelayCommand]
    private void CreateRerouteOnConnection(object? parameter)
    {
        if (parameter is not ValueTuple<ConnectionViewModel, System.Windows.Point> tuple)
        {
            return;
        }

        var (existingConnection, location) = tuple;

        if (existingConnection.Source == null || existingConnection.Target == null)
        {
            return;
        }

        // Get connector type from the existing connection
        var connType = existingConnection.Source.ConnectorType;

        // Create a minimal reroute node (UE5 style)
        var rerouteNode = new NodeViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = "",
            Category = "flow",
            Icon = "â—†",
            Type = "reroute",
            IsRerouteNode = true,
            IsRedirectionNode = true,
            Location = location
        };

        // Clear default connectors and add single input/output
        rerouteNode.Input.Clear();
        rerouteNode.Output.Clear();

        rerouteNode.Input.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = true
        });
        rerouteNode.Output.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = false
        });

        Nodes.Add(rerouteNode);

        // Store the original target
        var originalSource = existingConnection.Source;
        var originalTarget = existingConnection.Target;

        // Remove the original connection
        Connections.Remove(existingConnection);

        // Create new connections: original source -> reroute -> original target
        var rerouteInput = rerouteNode.Input.FirstOrDefault();
        var rerouteOutput = rerouteNode.Output.FirstOrDefault();

        if (rerouteInput != null && rerouteOutput != null)
        {
            // Source to reroute
            Connections.Add(new ConnectionViewModel
            {
                Source = originalSource,
                Target = rerouteInput
            });

            // Reroute to target
            Connections.Add(new ConnectionViewModel
            {
                Source = rerouteOutput,
                Target = originalTarget
            });
        }
    }

    /// <summary>
    /// Creates a reroute node from the context menu (standalone, not connected)
    /// </summary>
    [RelayCommand]
    private void CreateStandaloneRerouteNode()
    {
        var location = NodeMenuGraphPosition;

        var rerouteNode = new NodeViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = "",
            Category = "flow",
            Icon = "â—†",
            Type = "reroute",
            IsRerouteNode = true,
            IsRedirectionNode = true,
            Location = location
        };

        rerouteNode.Input.Clear();
        rerouteNode.Output.Clear();

        // Default to Exec type for standalone reroute
        var connType = menuConnectionSource?.ConnectorType ?? PendingConnection?.Source?.ConnectorType ?? ConnectorType.Exec;

        rerouteNode.Input.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = true
        });
        rerouteNode.Output.Add(new ConnectorViewModel
        {
            Title = "",
            ConnectorType = connType,
            IsInput = false
        });

        Nodes.Add(rerouteNode);

        // If there's a pending connection, connect it to the reroute node
        if (menuConnectionSource != null)
        {
            var targetConnector = rerouteNode.Input.FirstOrDefault();
            if (targetConnector != null)
            {
                Connections.Add(new ConnectionViewModel
                {
                    Source = menuConnectionSource,
                    Target = targetConnector
                });
            }
            menuConnectionSource = null;
        }

        IsNodeMenuOpen = false;
    }

    private bool ShouldHideTriggerNodes()
    {
        if (menuConnectionSource == null)
        {
            return false;
        }

        var sourceNode = Nodes.FirstOrDefault(n => n.Output.Contains(menuConnectionSource));
        return sourceNode?.Category == "trigger";
    }

    /// <summary>
    /// Add a new variable to the entity
    /// </summary>
    [RelayCommand]
    private void AddVariable()
    {
        if (string.IsNullOrWhiteSpace(NewVariableName))
        {
            return;
        }

        // Check for duplicate names
        if (Variables.Any(v => v.Name.Equals(NewVariableName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var variable = new VariableDefinition
        {
            Name = NewVariableName,
            Type = NewVariableType,
            DefaultValue = GetDefaultValueForType(NewVariableType)
        };

        Variables.Add(variable);
        NewVariableName = string.Empty;

        // Update the node palette to include the new variable nodes
        UpdateVariableNodes();
        UpdateVariableUsageCounts();
    }

    /// <summary>
    /// Remove a variable from the entity
    /// </summary>
    [RelayCommand]
    private void RemoveVariable(VariableDefinition? variable)
    {
        if (variable == null)
        {
            return;
        }

        Variables.Remove(variable);
        UpdateVariableNodes();
        UpdateVariableUsageCounts();
    }

    private static string GetDefaultValueForType(string type)
    {
        return type switch
        {
            "Boolean" => "false",
            "Integer" => "0",
            "Float" => "0.0",
            "String" => "",
            _ => ""
        };
    }

    private void UpdateVariableNodes()
    {
        // Remove existing variable nodes from palette
        var variableNodesToRemove = SimpleNodes.Where(n => n.Type?.StartsWith("var_") == true).ToList();
        foreach (var node in variableNodesToRemove)
        {
            SimpleNodes.Remove(node);
        }

        // Add Get/Set nodes for each variable
        foreach (var variable in Variables)
        {
            SimpleNodes.Add(new NodeOption($"Get {variable.Name}", "variable", "ðŸ“¥")
            {
                Type = $"var_get_{variable.Name}",
                Definition = CreateGetVariableDefinition(variable)
            });

            SimpleNodes.Add(new NodeOption($"Set {variable.Name}", "variable", "ðŸ“¤")
            {
                Type = $"var_set_{variable.Name}",
                Definition = CreateSetVariableDefinition(variable)
            });
        }
    }

    private void UpdateVariableUsageCounts()
    {
        variableUsageIndices.Clear();
        foreach (var variable in Variables)
        {
            variable.UsageCount = 0;
        }

        if (Variables.Count == 0 || Nodes.Count == 0)
        {
            return;
        }

        foreach (var node in Nodes)
        {
            foreach (var entry in node.Properties)
            {
                if (entry.Value is not string value)
                {
                    continue;
                }

                if (!value.StartsWith("$", StringComparison.Ordinal))
                {
                    continue;
                }

                string variableName = value.Substring(1).Trim();
                if (string.IsNullOrWhiteSpace(variableName))
                {
                    continue;
                }

                var variable = Variables.FirstOrDefault(v =>
                    v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));
                if (variable != null)
                {
                    variable.UsageCount++;
                }
            }
        }
    }

    [RelayCommand]
    private void FindVariableUsage(VariableDefinition? variable)
    {
        if (variable == null)
        {
            return;
        }

        var matchingNodes = Nodes
            .Where(n => n.Properties.Values
                .OfType<string>()
                .Any(v => v.StartsWith("$", StringComparison.Ordinal) &&
                          v.Substring(1).Equals(variable.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matchingNodes.Count == 0)
        {
            return;
        }

        int index = 0;
        if (variableUsageIndices.TryGetValue(variable.Name, out int currentIndex))
        {
            index = (currentIndex + 1) % matchingNodes.Count;
        }

        variableUsageIndices[variable.Name] = index;
        SelectedNode = matchingNodes[index];
    }

    private void LoadVariablesFromEntity()
    {
        Variables.Clear();
        if (entity.Variables == null || entity.Variables.Count == 0)
        {
            return;
        }

        foreach (var variable in entity.Variables)
        {
            Variables.Add(new VariableDefinition
            {
                Name = variable.Name,
                Type = variable.Type,
                DefaultValue = variable.DefaultValue
            });
        }
    }

    private void SaveVariablesToEntity()
    {
        entity.Variables.Clear();
        foreach (var variable in Variables)
        {
            entity.Variables.Add(new EntityVariable
            {
                Name = variable.Name,
                Type = variable.Type,
                DefaultValue = variable.DefaultValue
            });
        }
    }

    private NodeDefinition? TryBuildVariableNodeDefinition(BehaviorNode node)
    {
        const string getPrefix = "var_get_";
        const string setPrefix = "var_set_";

        if (!node.Type.StartsWith(getPrefix) && !node.Type.StartsWith(setPrefix))
        {
            return null;
        }

        string variableName = node.Type.StartsWith(getPrefix)
            ? node.Type.Substring(getPrefix.Length)
            : node.Type.Substring(setPrefix.Length);

        var variable = Variables.FirstOrDefault(v =>
            v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));

        if (variable == null)
        {
            variable = new VariableDefinition
            {
                Name = variableName,
                Type = "String",
                DefaultValue = string.Empty
            };
        }

        return node.Type.StartsWith(getPrefix)
            ? CreateGetVariableDefinition(variable)
            : CreateSetVariableDefinition(variable);
    }

    private static NodeDefinition CreateGetVariableDefinition(VariableDefinition variable)
    {
        string luaName = BuildLuaVariableName(variable.Name);
        return new NodeDefinition
        {
            Type = $"var_get_{variable.Name}",
            Label = $"Get {variable.Name}",
            Category = "variable",
            Icon = "ðŸ“¥",
            IsAdvanced = false,
            Description = $"Gets the value of variable '{variable.Name}'",
            Properties = new List<NodeProperty>(),
            CodeTemplate = $"local {luaName}_value = {luaName}\n{{CHILDREN}}"
        };
    }

    private static NodeDefinition CreateSetVariableDefinition(VariableDefinition variable)
    {
        string luaName = BuildLuaVariableName(variable.Name);
        string nodeType = MapVariableTypeToNodeType(variable.Type);
        bool isString = nodeType == "string";
        string valueTemplate = isString ? "\"{value}\"" : "{value}";

        return new NodeDefinition
        {
            Type = $"var_set_{variable.Name}",
            Label = $"Set {variable.Name}",
            Category = "variable",
            Icon = "ðŸ“¤",
            IsAdvanced = false,
            Description = $"Sets the value of variable '{variable.Name}'",
            Properties = new List<NodeProperty>
            {
                new() { Name = "value", Type = nodeType, Label = "Value", DefaultValue = variable.DefaultValue }
            },
            CodeTemplate = $"{luaName} = {valueTemplate}\n{{CHILDREN}}"
        };
    }

    private static string MapVariableTypeToNodeType(string variableType)
    {
        return variableType switch
        {
            "Boolean" => "bool",
            "Integer" => "int",
            "Float" => "float",
            "String" => "string",
            _ => "string"
        };
    }

    private static string BuildLuaVariableName(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return "var_unnamed";
        }

        var chars = variableName.Select(c =>
        {
            if (c <= 127 && (char.IsLetterOrDigit(c) || c == '_'))
            {
                return c;
            }

            return '_';
        }).ToArray();

        string normalized = new string(chars);
        if (char.IsDigit(normalized[0]))
        {
            normalized = "_" + normalized;
        }

        return "var_" + normalized;
    }

    private static NodeViewModel CreateNode(NodeDefinition nodeDef, double x, double y)
    {
        var node = new NodeViewModel
        {
            Title = nodeDef.Label,
            Category = nodeDef.Category,
            Icon = nodeDef.Icon,
            Type = nodeDef.Type,
            Definition = nodeDef,
            Location = new System.Windows.Point(x, y)
        };

        // Initialize properties with default values
        if (nodeDef.Properties != null)
        {
            foreach (var prop in nodeDef.Properties)
            {
                node.Properties[prop.Name] = prop.DefaultValue ?? string.Empty;
            }
        }

        // Initialize connectors (True/False for branching nodes)
        node.InitializeConnectors();

        // Update title for event nodes
        node.UpdateTitleForEventNodes();

        return node;
    }

    private static string GetEntityIcon(EntityType type)
    {
        return type switch
        {
            EntityType.Creature => "ðŸ‘¤",
            EntityType.Object => "ðŸ“¦",
            EntityType.Effect => "âœ¨",
            EntityType.Light => "ðŸ’¡",
            _ => "â“"
        };
    }

    public void UpdateTabTitle()
    {
        TabTitle = string.IsNullOrWhiteSpace(entity.ScriptName) ? "New Entity" : entity.ScriptName;
        EntityIcon = GetEntityIcon(entity.EntityType);
    }

    public void CreateVariableNodeAtPosition(string variableName, bool isSetNode, System.Windows.Point graphPosition)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return;
        }

        var variable = Variables.FirstOrDefault(v =>
            v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));

        if (variable == null)
        {
            return;
        }

        NodeDefinition definition = isSetNode
            ? CreateSetVariableDefinition(variable)
            : CreateGetVariableDefinition(variable);

        var node = CreateNode(definition, graphPosition.X, graphPosition.Y);
        Nodes.Add(node);
    }

    #region Object Reward Properties

    /// <summary>
    /// Whether this entity has object rewards enabled
    /// </summary>
    public bool HasObjectReward
    {
        get => entity.ObjectReward != null;
        set
        {
            if (value && entity.ObjectReward == null)
            {
                entity.ObjectReward = new ObjectReward();
                SyncObjectRewardItemsFromEntity();
            }
            else if (!value)
            {
                entity.ObjectReward = null;
                ObjectRewardItems.Clear();
            }
            OnPropertyChanged();
        }
    }

    public int ObjectRewardGold
    {
        get => entity.ObjectReward?.Gold ?? 0;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.Gold = value;
            OnPropertyChanged();
        }
    }

    public int ObjectRewardExperience
    {
        get => entity.ObjectReward?.Experience ?? 0;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.Experience = value;
            OnPropertyChanged();
        }
    }

    public bool ObjectRewardOneTimeOnly
    {
        get => entity.ObjectReward?.OneTimeOnly ?? true;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.OneTimeOnly = value;
            OnPropertyChanged();
        }
    }

    public bool ObjectRewardDestroyAfter
    {
        get => entity.ObjectReward?.DestroyAfterReward ?? true;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.DestroyAfterReward = value;
            OnPropertyChanged();
        }
    }

    public bool ObjectRewardShowMessage
    {
        get => entity.ObjectReward?.ShowMessage ?? true;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.ShowMessage = value;
            OnPropertyChanged();
        }
    }

    public string ObjectRewardCustomMessage
    {
        get => entity.ObjectReward?.CustomMessage ?? string.Empty;
        set
        {
            EnsureObjectReward();
            entity.ObjectReward!.CustomMessage = value;
            OnPropertyChanged();
        }
    }

    private void EnsureObjectReward()
    {
        if (entity.ObjectReward == null)
        {
            entity.ObjectReward = new ObjectReward();
            OnPropertyChanged(nameof(HasObjectReward));
        }
    }

    private void SyncObjectRewardItemsFromEntity()
    {
        ObjectRewardItems.Clear();
        if (entity.ObjectReward?.Items != null)
        {
            foreach (var item in entity.ObjectReward.Items)
            {
                ObjectRewardItems.Add(item);
            }
        }
    }

    private void SyncObjectRewardItemsToEntity()
    {
        if (entity.ObjectReward != null)
        {
            entity.ObjectReward.Items.Clear();
            foreach (var item in ObjectRewardItems)
            {
                entity.ObjectReward.Items.Add(item);
            }
        }
    }

    [RelayCommand]
    private void AddRewardItem()
    {
        if (string.IsNullOrWhiteSpace(NewRewardItem))
            return;

        EnsureObjectReward();
        ObjectRewardItems.Add(NewRewardItem);
        entity.ObjectReward!.Items.Add(NewRewardItem);
        NewRewardItem = string.Empty;
    }

    [RelayCommand]
    private void RemoveRewardItem(string? item)
    {
        if (string.IsNullOrWhiteSpace(item))
            return;

        ObjectRewardItems.Remove(item);
        entity.ObjectReward?.Items.Remove(item);
    }

    private void LoadAvailableRewardItems()
    {
        AvailableRewardItems.Clear();
        foreach (var obj in GameData.Objects)
        {
            AvailableRewardItems.Add(obj);
        }
    }

    #endregion

    [RelayCommand]
    private void BrowseEntities()
    {
        var browserView = new FableQuestTool.Views.EntityBrowserView();
        var browserViewModel = new EntityBrowserViewModel();
        browserView.DataContext = browserViewModel;

        browserViewModel.LoadEntitiesCommand.Execute(null);

        if (browserView.ShowDialog() == true && browserViewModel.SelectedEntity != null)
        {
            var selected = browserViewModel.SelectedEntity;

            // Auto-fill entity properties from selected entity
            entity.ScriptName = selected.ScriptName;
            entity.DefName = selected.DefinitionType;
            entity.SpawnRegion = selected.RegionName;

            // Try to determine entity type from category
            entity.EntityType = selected.Category switch
            {
                Models.EntityCategory.Creature => EntityType.Creature,
                Models.EntityCategory.NPC => EntityType.Creature,
                Models.EntityCategory.Enemy => EntityType.Creature,
                Models.EntityCategory.Object => EntityType.Object,
                Models.EntityCategory.Chest => EntityType.Object,
                Models.EntityCategory.Door => EntityType.Object,
                Models.EntityCategory.QuestItem => EntityType.Object,
                _ => EntityType.Creature
            };

            // Set spawn method based on whether it has a ScriptName
            entity.SpawnMethod = selected.HasScriptName ? SpawnMethod.BindExisting : SpawnMethod.AtMarker;

            // If it has a marker, use it
            if (!string.IsNullOrWhiteSpace(selected.ScriptName))
            {
                entity.SpawnMarker = selected.ScriptName;
            }

            // Store position if available
            entity.SpawnX = selected.PositionX;
            entity.SpawnY = selected.PositionY;
            entity.SpawnZ = selected.PositionZ;

            UpdateTabTitle();

            // Notify UI to refresh
            OnPropertyChanged(nameof(Entity));
        }
    }
}



