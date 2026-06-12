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
        GameObject enemyPrefab = LoadRequiredAsset<GameObject>(EnemyPrefabPath);

        BuildShopScene(sprite);
        BuildDungeonScene(sprite, playerPrefab, enemyPrefab);
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

    private static void BuildShopScene(Sprite sprite)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.09f, 0.07f, 0.05f));
        CreateBackground(sprite, new Color(0.28f, 0.20f, 0.12f));
        CreatePersistentSystems();

        GameObject shopObject = new GameObject("Shop");
        ShopManager shop = shopObject.AddComponent<ShopManager>();
        shop.damageUpgradeCost = 20;
        shop.damageIncrease = 5;

        SceneNavigation navigation = new GameObject("SceneNavigation").AddComponent<SceneNavigation>();
        CreateShopInterface(shop, navigation);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ShopScenePath);
    }

    private static void BuildDungeonScene(Sprite sprite, GameObject playerPrefab, GameObject enemyPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.04f, 0.06f, 0.09f));
        CreateBackground(sprite, new Color(0.12f, 0.18f, 0.22f));
        CreatePersistentSystems();

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        player.transform.position = Vector3.zero;

        Vector3[] enemyPositions =
        {
            new Vector3(4f, 0f, 0f),
            new Vector3(-4f, 2f, 0f),
            new Vector3(2f, -3f, 0f)
        };

        foreach (Vector3 position in enemyPositions)
        {
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
            enemy.transform.position = position;
        }

        SceneNavigation navigation = new GameObject("SceneNavigation").AddComponent<SceneNavigation>();
        CreateDungeonInterface(player.GetComponent<PlayerHealth>(), navigation);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DungeonScenePath);
    }

    private static void CreatePersistentSystems()
    {
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("InventoryManager").AddComponent<InventoryManager>();
    }

    private static void CreateCamera(Color backgroundColor)
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

    private static void CreateShopInterface(ShopManager shop, SceneNavigation navigation)
    {
        Canvas canvas = CreateCanvas();
        CreateTitle(canvas.transform, "MERCHANT SHOP");

        TextMeshProUGUI gold = CreateText(canvas.transform, "GoldText", "Gold: 0", new Vector2(30f, -100f), 34f);
        TextMeshProUGUI inventory = CreateText(canvas.transform, "InventoryText", "Run inventory:", new Vector2(30f, -155f), 28f);
        inventory.rectTransform.sizeDelta = new Vector2(600f, 350f);
        TextMeshProUGUI upgrade = CreateText(canvas.transform, "UpgradeText", "Damage upgrade: 0 (+0)", new Vector2(30f, -520f), 28f);

        SimpleUIManager ui = canvas.gameObject.AddComponent<SimpleUIManager>();
        ui.goldText = gold;
        ui.inventoryText = inventory;
        ui.upgradeText = upgrade;

        Button sell = CreateButton(canvas.transform, "SellButton", "Sell All Loot", new Vector2(-40f, 210f));
        Button upgradeButton = CreateButton(canvas.transform, "UpgradeButton", "Buy Damage +5 (20 gold)", new Vector2(-40f, 140f));
        Button dungeon = CreateButton(canvas.transform, "EnterDungeonButton", "Enter Dungeon", new Vector2(-40f, 40f));

        UnityEventTools.AddPersistentListener(sell.onClick, shop.SellAll);
        UnityEventTools.AddPersistentListener(upgradeButton.onClick, shop.BuyDamageUpgrade);
        UnityEventTools.AddPersistentListener(dungeon.onClick, navigation.EnterDungeon);
        CreateEventSystem();
    }

    private static void CreateDungeonInterface(PlayerHealth health, SceneNavigation navigation)
    {
        Canvas canvas = CreateCanvas();
        CreateTitle(canvas.transform, "DUNGEON");

        TextMeshProUGUI gold = CreateText(canvas.transform, "GoldText", "Gold: 0", new Vector2(30f, -100f), 30f);
        TextMeshProUGUI healthText = CreateText(canvas.transform, "HealthText", "Health: 100/100", new Vector2(30f, -145f), 30f);
        TextMeshProUGUI inventory = CreateText(canvas.transform, "InventoryText", "Run inventory:", new Vector2(30f, -195f), 26f);
        inventory.rectTransform.sizeDelta = new Vector2(550f, 300f);
        TextMeshProUGUI upgrade = CreateText(canvas.transform, "UpgradeText", "Damage upgrade: 0 (+0)", new Vector2(30f, -500f), 26f);

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

        Button shop = CreateButton(canvas.transform, "ReturnToShopButton", "Return To Shop", new Vector2(-40f, 40f));
        UnityEventTools.AddPersistentListener(shop.onClick, navigation.ReturnToShop);
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
