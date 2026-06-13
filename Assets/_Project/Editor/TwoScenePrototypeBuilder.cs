using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TwoScenePrototypeBuilder
{
    private const string ShopScenePath = "Assets/_Project/Scenes/ShopScene.unity";
    private const string DungeonScenePath = "Assets/_Project/Scenes/DungeonScene.unity";
    private const string SpritePath = "Assets/_Project/Art/Sprites/PrototypeSquare.png";
    private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/PrototypePlayer.prefab";
    private const string EnemyPrefabPath = "Assets/_Project/Prefabs/Enemies/PrototypeEnemy.prefab";
    private const string PickupPrefabPath = "Assets/_Project/Prefabs/Items/TestRelicPickup.prefab";
    private const string NormalEnemyPath = "Assets/_Project/Prefabs/Enemies/PrototypeNormalEnemy.prefab";
    private const string FastEnemyPath = "Assets/_Project/Prefabs/Enemies/PrototypeFastEnemy.prefab";
    private const string TankEnemyPath = "Assets/_Project/Prefabs/Enemies/PrototypeTankEnemy.prefab";

    [MenuItem("Tools/The Immortal Merchant/Build Shop + Dungeon Scenes")]
    public static void BuildScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (ScenesAlreadyExist() && !EditorUtility.DisplayDialog(
                "Rebuild Shop and Dungeon?",
                "This replaces the generated ShopScene and DungeonScene. The original Prototype01 scene is not changed.",
                "Rebuild",
                "Cancel"))
        {
            return;
        }

        Sprite sprite = LoadRequiredAsset<Sprite>(SpritePath);
        GameObject playerPrefab = LoadRequiredAsset<GameObject>(PlayerPrefabPath);
        GameObject baseEnemyPrefab = LoadRequiredAsset<GameObject>(EnemyPrefabPath);
        GameObject basePickupPrefab = LoadRequiredAsset<GameObject>(PickupPrefabPath);

        ItemData normalItem = CreateOrUpdateItem(
            "Assets/_Project/ScriptableObjects/Items/PrototypeScrap.asset",
            "prototype_scrap",
            "Prototype Scrap",
            20,
            ItemRarity.Common);
        ItemData fastItem = CreateOrUpdateItem(
            "Assets/_Project/ScriptableObjects/Items/PrototypeSwiftShard.asset",
            "prototype_swift_shard",
            "Prototype Swift Shard",
            30,
            ItemRarity.Uncommon);
        ItemData tankItem = CreateOrUpdateItem(
            "Assets/_Project/ScriptableObjects/Items/PrototypeHeavyCore.asset",
            "prototype_heavy_core",
            "Prototype Heavy Core",
            50,
            ItemRarity.Rare);

        GameObject normalPickup = CreatePickupVariant(
            basePickupPrefab,
            "Assets/_Project/Prefabs/Items/PrototypeScrapPickup.prefab",
            "PrototypeScrapPickup",
            normalItem,
            new Color(0.95f, 0.78f, 0.2f));
        GameObject fastPickup = CreatePickupVariant(
            basePickupPrefab,
            "Assets/_Project/Prefabs/Items/PrototypeSwiftShardPickup.prefab",
            "PrototypeSwiftShardPickup",
            fastItem,
            new Color(0.25f, 0.95f, 0.95f));
        GameObject tankPickup = CreatePickupVariant(
            basePickupPrefab,
            "Assets/_Project/Prefabs/Items/PrototypeHeavyCorePickup.prefab",
            "PrototypeHeavyCorePickup",
            tankItem,
            new Color(0.72f, 0.35f, 1f));

        GameObject normalEnemy = CreateEnemyVariant(
            baseEnemyPrefab, NormalEnemyPath, "PrototypeNormalEnemy", normalPickup,
            0.2f, 30, 2f, 10, 1f, Vector3.one, new Color(0.9f, 0.18f, 0.18f));
        GameObject fastEnemy = CreateEnemyVariant(
            baseEnemyPrefab, FastEnemyPath, "PrototypeFastEnemy", fastPickup,
            0.1f, 20, 3.5f, 7, 0.7f, new Vector3(0.75f, 0.75f, 1f), new Color(1f, 0.45f, 0.12f));
        GameObject tankEnemy = CreateEnemyVariant(
            baseEnemyPrefab, TankEnemyPath, "PrototypeTankEnemy", tankPickup,
            0.35f, 60, 1.2f, 15, 1.2f, new Vector3(1.3f, 1.3f, 1f), new Color(0.55f, 0.2f, 0.8f));

        BuildShopScene(sprite);
        BuildDungeonScene(
            sprite,
            playerPrefab,
            normalEnemy,
            fastEnemy,
            tankEnemy,
            new[] { normalItem, fastItem, tankItem });
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ShopScenePath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ShopScenePath));

        EditorUtility.DisplayDialog(
            "Two-scene loop ready",
            "ShopScene and DungeonScene were created. ShopScene is open and is now the first scene in Build Settings.",
            "OK");
    }

    private static bool ScenesAlreadyExist()
    {
        return AssetDatabase.LoadAssetAtPath<SceneAsset>(ShopScenePath) != null
            || AssetDatabase.LoadAssetAtPath<SceneAsset>(DungeonScenePath) != null;
    }

    private static T LoadRequiredAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new System.InvalidOperationException(
                $"Required asset not found at {path}. Run 'Build Prototype 0.1 Scene' first.");
        }

        return asset;
    }

    private static ItemData CreateOrUpdateItem(
        string path,
        string itemId,
        string displayName,
        int sellPrice,
        ItemRarity rarity)
    {
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, path);
        }

        item.itemId = itemId;
        item.displayName = displayName;
        item.sellPrice = sellPrice;
        item.rarity = rarity;
        item.description = "Temporary content used to validate prototype variety.";
        EditorUtility.SetDirty(item);
        return item;
    }

    private static GameObject CreatePickupVariant(
        GameObject basePrefab,
        string path,
        string objectName,
        ItemData item,
        Color color)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(basePrefab));
        root.name = objectName;
        root.GetComponent<ItemPickup>().itemData = item;
        root.GetComponent<SpriteRenderer>().color = color;
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject CreateEnemyVariant(
        GameObject basePrefab,
        string path,
        string objectName,
        GameObject dropPrefab,
        float dropChance,
        int maxHealth,
        float moveSpeed,
        int contactDamage,
        float damageInterval,
        Vector3 scale,
        Color color)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(basePrefab));
        root.name = objectName;
        root.transform.localScale = scale;
        root.GetComponent<SpriteRenderer>().color = color;

        EnemyHealth health = root.GetComponent<EnemyHealth>();
        health.maxHealth = maxHealth;
        health.itemDropPrefab = dropPrefab;
        health.itemDropChance = dropChance;

        EnemyChaser chaser = root.GetComponent<EnemyChaser>();
        chaser.moveSpeed = moveSpeed;
        chaser.contactDamage = contactDamage;
        chaser.damageInterval = damageInterval;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void BuildShopScene(Sprite sprite)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.09f, 0.07f, 0.05f));
        CreateBackground(sprite, new Color(0.28f, 0.20f, 0.12f));
        CreatePersistentSystems();

        GameObject shopObject = new GameObject("Shop");
        ShopManager shop = shopObject.AddComponent<ShopManager>();
        shop.damageUpgradeCost = 20;
        shop.damageUpgradeCostIncrease = 15;
        shop.damageIncrease = 5;

        SceneNavigation navigation = new GameObject("SceneNavigation").AddComponent<SceneNavigation>();
        CreateShopInterface(shop, navigation);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ShopScenePath);
    }

    private static void BuildDungeonScene(
        Sprite sprite,
        GameObject playerPrefab,
        GameObject normalEnemyPrefab,
        GameObject fastEnemyPrefab,
        GameObject tankEnemyPrefab,
        ItemData[] chestRewards)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreatePersistentSystems();

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        player.transform.position = new Vector3(-3f, 0f, 0f);

        Camera camera = CreateCamera(new Color(0.04f, 0.06f, 0.09f));
        CameraFollow2D cameraFollow = camera.gameObject.AddComponent<CameraFollow2D>();
        cameraFollow.target = player.transform;

        CreateRoomGeometry(sprite, "Room1Geometry", Vector2.zero, new Color(0.12f, 0.18f, 0.22f), true, false);
        CreateRoomGeometry(sprite, "CombatRoomGeometry", new Vector2(18f, 7f), new Color(0.24f, 0.11f, 0.12f), false, true);
        CreateRoomGeometry(sprite, "RewardRoomGeometry", new Vector2(18f, -7f), new Color(0.10f, 0.24f, 0.17f), false, true);
        CreateBranchingCorridors(sprite);

        DungeonDoor combatDoor = CreateDoor(sprite, "CombatRouteDoor", new Vector2(11f, 4f), new Vector2(3f, 0.6f));
        DungeonDoor rewardDoor = CreateDoor(sprite, "RewardRouteDoor", new Vector2(11f, -4f), new Vector2(3f, 0.6f));

        DungeonManager dungeonManager = new GameObject("DungeonManager").AddComponent<DungeonManager>();
        dungeonManager.earlyExitPortal = CreateExitPortal(sprite, "EarlyExitPortal", new Vector2(6f, -3.5f));
        DungeonExitPortal combatExit = CreateExitPortal(sprite, "CombatExitPortal", new Vector2(21.5f, 10f));
        DungeonExitPortal rewardExit = CreateExitPortal(sprite, "RewardExitPortal", new Vector2(21.5f, -10f));
        dungeonManager.exitPortal = combatExit;

        RoomEncounter room1 = CreateRoomEncounter(
            scene,
            "Room1Encounter",
            1,
            true,
            false,
            dungeonManager,
            new[] { combatDoor, rewardDoor },
            new[] { normalEnemyPrefab, fastEnemyPrefab },
            new[] { new Vector3(2.5f, 1.5f, 0f), new Vector3(3f, -2f, 0f) });

        RoomEncounter combatRoom = CreateRoomEncounter(
            scene,
            "CombatRouteEncounter",
            2,
            false,
            true,
            dungeonManager,
            new[] { combatDoor },
            new[] { tankEnemyPrefab, normalEnemyPrefab, normalEnemyPrefab },
            new[] { new Vector3(18f, 8.5f, 0f), new Vector3(15.5f, 5.2f, 0f), new Vector3(20.5f, 5.5f, 0f) });
        combatRoom.completionPortal = combatExit;

        RouteChoiceController routeChoice = new GameObject("RouteChoiceController").AddComponent<RouteChoiceController>();
        routeChoice.combatRouteDoor = combatDoor;
        routeChoice.rewardRouteDoor = rewardDoor;
        routeChoice.combatEncounter = combatRoom;
        routeChoice.rewardExitPortal = rewardExit;

        CreateRouteChoiceTrigger(routeChoice, true, new Vector2(11f, 5f));
        CreateRouteChoiceTrigger(routeChoice, false, new Vector2(11f, -5f));
        CreatePrototypeChest(sprite, chestRewards, new Vector2(19.5f, -7f));
        CreateHealingPickup(sprite, new Vector2(16.5f, -7f));
        CreateDungeonInterface(player.GetComponent<PlayerHealth>(), dungeonManager);
        routeChoice.statusText = dungeonManager.enemiesRemainingText;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DungeonScenePath);
    }

    private static void CreatePersistentSystems()
    {
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("InventoryManager").AddComponent<InventoryManager>();
    }

    private static Camera CreateCamera(Color backgroundColor)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateBackground(Sprite sprite, Color color)
    {
        GameObject floor = new GameObject("Prototype Floor");
        floor.transform.position = new Vector3(0f, 0f, 2f);
        floor.transform.localScale = new Vector3(20f, 12f, 1f);

        SpriteRenderer renderer = floor.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = -10;
    }

    private static DungeonExitPortal CreateExitPortal(Sprite sprite, string objectName, Vector2 position)
    {
        GameObject portalObject = new GameObject(objectName);
        portalObject.transform.position = position;
        portalObject.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

        SpriteRenderer renderer = portalObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;

        CircleCollider2D collider = portalObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.55f;
        return portalObject.AddComponent<DungeonExitPortal>();
    }

    private static void CreateRoomGeometry(
        Sprite sprite,
        string roomName,
        Vector2 center,
        Color floorColor,
        bool eastOpening,
        bool westOpening)
    {
        GameObject room = new GameObject(roomName);
        CreateBlock(sprite, "Floor", center, new Vector2(14f, 10f), floorColor, -10, false, room.transform);

        CreateBlock(sprite, "NorthWall", center + Vector2.up * 5.25f, new Vector2(14.5f, 0.5f), Color.black, 2, true, room.transform);
        CreateBlock(sprite, "SouthWall", center + Vector2.down * 5.25f, new Vector2(14.5f, 0.5f), Color.black, 2, true, room.transform);

        if (westOpening)
        {
            CreateSplitVerticalWall(sprite, center + Vector2.left * 7.25f, room.transform, "WestWall");
        }
        else
        {
            CreateBlock(sprite, "WestWall", center + Vector2.left * 7.25f, new Vector2(0.5f, 11f), Color.black, 2, true, room.transform);
        }

        if (eastOpening)
        {
            CreateSplitVerticalWall(sprite, center + Vector2.right * 7.25f, room.transform, "EastWall");
        }
        else
        {
            CreateBlock(sprite, "EastWall", center + Vector2.right * 7.25f, new Vector2(0.5f, 11f), Color.black, 2, true, room.transform);
        }
    }

    private static void CreateSplitVerticalWall(Sprite sprite, Vector2 center, Transform parent, string name)
    {
        CreateBlock(sprite, name + "Top", center + Vector2.up * 3.5f, new Vector2(0.5f, 4f), Color.black, 2, true, parent);
        CreateBlock(sprite, name + "Bottom", center + Vector2.down * 3.5f, new Vector2(0.5f, 4f), Color.black, 2, true, parent);
    }

    private static void CreateBranchingCorridors(Sprite sprite)
    {
        Color corridorColor = new Color(0.15f, 0.15f, 0.18f);
        CreateBlock(sprite, "MainCorridor", new Vector2(9f, 0f), new Vector2(4f, 3f), corridorColor, -9, false, null);
        CreateBlock(sprite, "BranchJunction", new Vector2(11f, 0f), new Vector2(3f, 11f), corridorColor, -9, false, null);
        CreateBlock(sprite, "CombatCorridor", new Vector2(14.5f, 7f), new Vector2(7f, 3f), corridorColor, -9, false, null);
        CreateBlock(sprite, "RewardCorridor", new Vector2(14.5f, -7f), new Vector2(7f, 3f), corridorColor, -9, false, null);

        CreateBlock(sprite, "JunctionWestWallTop", new Vector2(9f, 4f), new Vector2(0.5f, 5f), Color.black, 2, true, null);
        CreateBlock(sprite, "JunctionWestWallBottom", new Vector2(9f, -4f), new Vector2(0.5f, 5f), Color.black, 2, true, null);
    }

    private static DungeonDoor CreateDoor(Sprite sprite, string name, Vector2 position, Vector2 scale)
    {
        GameObject doorObject = CreateBlock(sprite, name, position, scale, new Color(0.65f, 0.2f, 0.1f), 4, true, null);
        return doorObject.AddComponent<DungeonDoor>();
    }

    private static GameObject CreateBlock(
        Sprite sprite,
        string name,
        Vector2 position,
        Vector2 scale,
        Color color,
        int sortingOrder,
        bool addCollider,
        Transform parent)
    {
        GameObject block = new GameObject(name);
        block.transform.position = position;
        block.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        if (parent != null)
        {
            block.transform.SetParent(parent, true);
        }

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        if (addCollider)
        {
            block.AddComponent<BoxCollider2D>();
        }

        return block;
    }

    private static RoomEncounter CreateRoomEncounter(
        Scene scene,
        string name,
        int roomNumber,
        bool startsActive,
        bool isFinalRoom,
        DungeonManager dungeonManager,
        DungeonDoor[] roomDoors,
        GameObject[] enemyPrefabs,
        Vector3[] enemyPositions)
    {
        GameObject roomObject = new GameObject(name);
        RoomEncounter encounter = roomObject.AddComponent<RoomEncounter>();
        encounter.roomNumber = roomNumber;
        encounter.startsActive = startsActive;
        encounter.isFinalRoom = isFinalRoom;
        encounter.dungeonManager = dungeonManager;
        encounter.doors.AddRange(roomDoors);

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefabs[i], scene);
            enemy.transform.position = enemyPositions[i];
            enemy.transform.SetParent(roomObject.transform, true);
            encounter.enemies.Add(enemy.GetComponent<EnemyHealth>());
        }

        return encounter;
    }

    private static void CreateRoomTrigger(RoomEncounter encounter, Vector2 position)
    {
        GameObject triggerObject = new GameObject("Room2Trigger");
        triggerObject.transform.position = position;
        BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 3f);
        RoomTrigger trigger = triggerObject.AddComponent<RoomTrigger>();
        trigger.roomEncounter = encounter;
    }

    private static void CreateRouteChoiceTrigger(RouteChoiceController controller, bool combatRoute, Vector2 position)
    {
        GameObject triggerObject = new GameObject(combatRoute ? "CombatRouteTrigger" : "RewardRouteTrigger");
        triggerObject.transform.position = position;
        BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3f, 1f);

        RouteChoiceTrigger trigger = triggerObject.AddComponent<RouteChoiceTrigger>();
        trigger.routeChoice = controller;
        trigger.choosesCombatRoute = combatRoute;
    }

    private static void CreatePrototypeChest(Sprite sprite, ItemData[] rewards, Vector2 position)
    {
        GameObject chest = CreateBlock(sprite, "PrototypeChest", position, new Vector2(0.9f, 0.7f), new Color(0.9f, 0.6f, 0.12f), 4, false, null);
        BoxCollider2D collider = chest.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        PrototypeChest prototypeChest = chest.AddComponent<PrototypeChest>();
        prototypeChest.rewardItems = rewards;
        prototypeChest.amount = 1;
    }

    private static void CreateHealingPickup(Sprite sprite, Vector2 position)
    {
        GameObject healing = CreateBlock(sprite, "HealingPickup", position, new Vector2(0.55f, 0.55f), new Color(0.2f, 1f, 0.35f), 4, false, null);
        CircleCollider2D collider = healing.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        healing.AddComponent<HealingPickup>().healAmount = 25;
    }

    private static void CreateShopInterface(ShopManager shop, SceneNavigation navigation)
    {
        Canvas canvas = CreateCanvas();
        CreateTitle(canvas.transform, "MERCHANT SHOP");

        TextMeshProUGUI gold = CreateText(canvas.transform, "GoldText", "Gold: 0", new Vector2(30f, -100f), 34f);
        TextMeshProUGUI inventory = CreateText(canvas.transform, "InventoryText", "Run inventory:", new Vector2(30f, -155f), 28f);
        inventory.rectTransform.sizeDelta = new Vector2(600f, 350f);
        TextMeshProUGUI upgrade = CreateText(canvas.transform, "UpgradeText", "Damage upgrade: 0 (+0)", new Vector2(30f, -520f), 28f);
        TextMeshProUGUI runSummary = CreateText(canvas.transform, "RunSummaryText", "No completed runs yet.", new Vector2(30f, -585f), 26f);
        runSummary.rectTransform.sizeDelta = new Vector2(1000f, 90f);

        SimpleUIManager ui = canvas.gameObject.AddComponent<SimpleUIManager>();
        ui.goldText = gold;
        ui.inventoryText = inventory;
        ui.upgradeText = upgrade;
        ui.runSummaryText = runSummary;
        ui.shopManager = shop;

        Button sell = CreateButton(canvas.transform, "SellButton", "Sell All Loot", new Vector2(-40f, 210f));
        Button upgradeButton = CreateButton(canvas.transform, "UpgradeButton", "Buy Damage +5", new Vector2(-40f, 140f));
        Button dungeon = CreateButton(canvas.transform, "EnterDungeonButton", "Enter Dungeon", new Vector2(-40f, 40f));

        UnityEventTools.AddPersistentListener(sell.onClick, shop.SellAll);
        UnityEventTools.AddPersistentListener(upgradeButton.onClick, shop.BuyDamageUpgrade);
        UnityEventTools.AddPersistentListener(dungeon.onClick, navigation.EnterDungeon);
        CreateEventSystem();
    }

    private static void CreateDungeonInterface(PlayerHealth health, DungeonManager dungeonManager)
    {
        Canvas canvas = CreateCanvas();
        CreateTitle(canvas.transform, "DUNGEON");

        TextMeshProUGUI gold = CreateText(canvas.transform, "GoldText", "Gold: 0", new Vector2(30f, -100f), 30f);
        TextMeshProUGUI healthText = CreateText(canvas.transform, "HealthText", "Health: 100/100", new Vector2(30f, -145f), 30f);
        TextMeshProUGUI inventory = CreateText(canvas.transform, "InventoryText", "Run inventory:", new Vector2(30f, -195f), 26f);
        inventory.rectTransform.sizeDelta = new Vector2(550f, 300f);
        TextMeshProUGUI upgrade = CreateText(canvas.transform, "UpgradeText", "Damage upgrade: 0 (+0)", new Vector2(30f, -500f), 26f);
        TextMeshProUGUI enemiesRemaining = CreateText(
            canvas.transform,
            "EnemiesRemainingText",
            "Enemies remaining: 3",
            new Vector2(0f, -85f),
            28f);
        RectTransform enemiesRect = enemiesRemaining.rectTransform;
        enemiesRect.anchorMin = new Vector2(0.5f, 1f);
        enemiesRect.anchorMax = new Vector2(0.5f, 1f);
        enemiesRect.pivot = new Vector2(0.5f, 1f);
        enemiesRect.anchoredPosition = new Vector2(0f, -85f);
        enemiesRect.sizeDelta = new Vector2(700f, 50f);
        enemiesRemaining.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI controls = CreateText(
            canvas.transform,
            "ControlsText",
            "WASD: Move   Space: Dash   J / Left Click: Attack",
            new Vector2(30f, 35f),
            24f);
        controls.rectTransform.anchorMin = Vector2.zero;
        controls.rectTransform.anchorMax = Vector2.zero;
        controls.rectTransform.pivot = Vector2.zero;

        SimpleUIManager ui = canvas.gameObject.AddComponent<SimpleUIManager>();
        ui.goldText = gold;
        ui.healthText = healthText;
        ui.inventoryText = inventory;
        ui.upgradeText = upgrade;
        ui.playerHealth = health;

        dungeonManager.enemiesRemainingText = enemiesRemaining;
        CreateEventSystem();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateTitle(Transform parent, string title)
    {
        TextMeshProUGUI text = CreateText(parent, "TitleText", title, new Vector2(0f, -25f), 42f);
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -25f);
        rect.sizeDelta = new Vector2(700f, 60f);
        text.alignment = TextAlignmentOptions.Center;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string value,
        Vector2 position,
        float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(900f, 50f);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.38f, 0.56f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(390f, 58f);

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, 25f);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] generatedScenes =
        {
            new EditorBuildSettingsScene(ShopScenePath, true),
            new EditorBuildSettingsScene(DungeonScenePath, true)
        };

        EditorBuildSettingsScene[] otherScenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != ShopScenePath && scene.path != DungeonScenePath)
            .ToArray();

        EditorBuildSettings.scenes = generatedScenes.Concat(otherScenes).ToArray();
    }
}
