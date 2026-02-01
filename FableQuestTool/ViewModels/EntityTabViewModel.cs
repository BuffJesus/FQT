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

    public ObservableCollection<NodeOption> FilteredNodes { get; } = new();

    private int nodeSeed = 0;

    public EntityTabViewModel(QuestEntity entity)
    {
        this.entity = entity;
        var config = FableConfig.Load();
        this.entityBrowserService = new EntityBrowserService(config);
        tabTitle = string.IsNullOrWhiteSpace(entity.ScriptName) ? "New Entity" : entity.ScriptName;
        entityIcon = GetEntityIcon(entity.EntityType);

        // Listen for property changes to update tab title
        entity.PropertyChanged += Entity_PropertyChanged;

        LoadNodePalette();
        LoadExistingNodes();
        LoadDropdownData();

        // Listen for node collection changes to update available events
        Nodes.CollectionChanged += (s, e) => UpdateAvailableEvents();
    }

    private void LoadDropdownData()
    {
        try
        {
            // Load all entities to populate dropdowns
            allEntities = entityBrowserService.GetAllEntities();

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

            // Fallback to static creature list
            if (entity.EntityType == EntityType.Creature)
            {
                AvailableDefinitions.Clear();
                foreach (var creature in GameData.Creatures)
                {
                    AvailableDefinitions.Add(creature);
                }
            }

            // Still allow text entry even if loading fails
        }
    }

    private void OnEntityPropertyChangedForDropdowns(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QuestEntity.EntityType) ||
            e.PropertyName == nameof(QuestEntity.SpawnRegion))
        {
            UpdateAvailableDefinitions();
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
            return;
        }

        IEnumerable<TngEntity> filteredEntities = allEntities;

        // Filter by spawn region if specified
        if (!string.IsNullOrWhiteSpace(entity.SpawnRegion))
        {
            filteredEntities = filteredEntities.Where(e =>
                e.RegionName != null &&
                e.RegionName.Equals(entity.SpawnRegion, StringComparison.OrdinalIgnoreCase));
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

        // Filter by spawn method
        if (spawnMethod == SpawnMethod.BindExisting)
        {
            // For BindExisting, show only scriptable entities (those with script names)
            filteredEntities = filteredEntities.Where(e => e.HasScriptName);
        }
        else if (spawnMethod == SpawnMethod.AtMarker)
        {
            // For AtMarker, show scriptable entities as marker names
            filteredEntities = filteredEntities.Where(e => e.HasScriptName);
        }

        var markers = filteredEntities
            .Select(e => e.ScriptName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(m => m)
            .ToList();

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
        }

        // Load connections from entity model
        foreach (var conn in entity.Connections)
        {
            var sourceNode = Nodes.FirstOrDefault(n => n.Id == conn.FromNodeId);
            var targetNode = Nodes.FirstOrDefault(n => n.Id == conn.ToNodeId);

            if (sourceNode != null && targetNode != null)
            {
                // Find the correct output connector by FromPort name
                ConnectorViewModel? sourceConnector = null;
                if (!string.IsNullOrEmpty(conn.FromPort))
                {
                    sourceConnector = sourceNode.Output.FirstOrDefault(o => o.Title == conn.FromPort);
                }
                // Fallback to first output if not found or no FromPort specified
                if (sourceConnector == null)
                {
                    sourceConnector = sourceNode.Output.FirstOrDefault();
                }

                // Find the correct input connector by ToPort name
                ConnectorViewModel? targetConnector = null;
                if (!string.IsNullOrEmpty(conn.ToPort))
                {
                    targetConnector = targetNode.Input.FirstOrDefault(i => i.Title == conn.ToPort);
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
    }

    [RelayCommand]
    private void DeleteNode()
    {
        if (SelectedNode == null)
        {
            return;
        }

        var connectionsToRemove = Connections.Where(c =>
            c.Source?.Equals(SelectedNode.Output.FirstOrDefault()) == true ||
            c.Target?.Equals(SelectedNode.Input.FirstOrDefault()) == true).ToList();

        foreach (var conn in connectionsToRemove)
        {
            Connections.Remove(conn);
        }

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
    private void AddOutputPin()
    {
        if (SelectedNode == null || SelectedNode.Category != "flow")
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
        if (SelectedNode == null || SelectedNode.Output.Count <= 1 || SelectedNode.Category != "flow")
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

            var existingConnection = Connections.FirstOrDefault(c =>
                c.Source == PendingConnection.Source && c.Target == targetConnector);
            if (existingConnection != null)
            {
                PendingConnection = null;
                return;
            }

            var connection = new ConnectionViewModel
            {
                Source = PendingConnection.Source,
                Target = targetConnector
            };

            Connections.Add(connection);
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

        NodeSearchText = string.Empty;
        UpdateFilteredNodes();
        IsNodeMenuOpen = true;
    }

    [RelayCommand]
    private void CloseNodeMenu()
    {
        IsNodeMenuOpen = false;
        NodeSearchText = string.Empty;
    }

    [RelayCommand]
    private void SelectNodeFromMenu(NodeOption? option)
    {
        if (option?.Definition == null)
        {
            return;
        }

        nodeSeed++;
        var newNode = CreateNode(option.Definition, NodeMenuGraphPosition.X, NodeMenuGraphPosition.Y);
        Nodes.Add(newNode);

        if (PendingConnection?.Source != null)
        {
            var targetConnector = newNode.Input.FirstOrDefault();
            if (targetConnector != null)
            {
                var connection = new ConnectionViewModel
                {
                    Source = PendingConnection.Source,
                    Target = targetConnector
                };
                Connections.Add(connection);
            }
            PendingConnection = null;
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

        var allNodes = SimpleNodes.Concat(AdvancedNodes);

        if (string.IsNullOrWhiteSpace(NodeSearchText))
        {
            foreach (var node in allNodes)
            {
                FilteredNodes.Add(node);
            }
        }
        else
        {
            var searchLower = NodeSearchText.ToLower();
            foreach (var node in allNodes.Where(n =>
                n.Label.ToLower().Contains(searchLower) ||
                n.Category.ToLower().Contains(searchLower)))
            {
                FilteredNodes.Add(node);
            }
        }
    }

    [RelayCommand]
    private void CreateRedirectionNode(System.Windows.Point location)
    {
        if (PendingConnection?.Source == null)
        {
            return;
        }

        var sourceConnector = PendingConnection.Source;

        var flowDef = NodeDefinitions.GetAllNodes().FirstOrDefault(n => n.Type == "sequence");
        if (flowDef == null)
        {
            return;
        }

        var redirectNode = CreateNode(flowDef, location.X, location.Y);
        redirectNode.IsRedirectionNode = true;

        if (redirectNode.Input.Count == 0)
        {
            redirectNode.Input.Add(new ConnectorViewModel { Title = "Input" });
        }
        if (redirectNode.Output.Count == 0)
        {
            redirectNode.Output.Add(new ConnectorViewModel { Title = "Output" });
        }

        Nodes.Add(redirectNode);

        var targetConnector = redirectNode.Input.FirstOrDefault();
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
            EntityType.Creature => "👤",
            EntityType.Object => "📦",
            EntityType.Effect => "✨",
            EntityType.Light => "💡",
            _ => "❓"
        };
    }

    public void UpdateTabTitle()
    {
        TabTitle = string.IsNullOrWhiteSpace(entity.ScriptName) ? "New Entity" : entity.ScriptName;
        EntityIcon = GetEntityIcon(entity.EntityType);
    }

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
