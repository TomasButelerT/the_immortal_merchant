using System.IO;
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

public static class PrototypeSceneBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/Prototype01.unity";
    private const string SpritePath = "Assets/_Project/Art/Sprites/PrototypeSquare.png";
    private const string ItemPath = "Assets/_Project/ScriptableObjects/Items/TestRelic.asset";
    private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/PrototypePlayer.prefab";
    private const string EnemyPrefabPath = "Assets/_Project/Prefabs/Enemies/PrototypeEnemy.prefab";
    private const string PickupPrefabPath = "Assets/_Project/Prefabs/Items/TestRelicPickup.prefab";

    [MenuItem("Tools/The Immortal Merchant/Build Prototype 0.1 Scene")]
    public static void BuildPrototypeScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            bool rebuild = EditorUtility.DisplayDialog(
                "Rebuild Prototype 0.1?",
                "Prototype01 already exists. Rebuilding will replace that generated scene and update its generated prefabs.",
                "Rebuild",
                "Cancel");

            if (!rebuild)
            {
                return;
            }
        }

        try
        {
            EnsureFolders();
            EnsureTextMeshProResources();

            int enemyLayer = EnsureLayer("Enemy");
            EnsureTag("Player");

            Sprite squareSprite = EnsurePrototypeSprite();
            ItemData testRelic = EnsureTestItem();
            GameObject pickupPrefab = CreatePickupPrefab(squareSprite, testRelic);
            GameObject playerPrefab = CreatePlayerPrefab(squareSprite, enemyLayer);
            GameObject enemyPrefab = CreateEnemyPrefab(squareSprite, pickupPrefab, enemyLayer);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateBackground(squareSprite);
            CreateGameSystems();

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = Vector3.zero;

            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
            enemy.transform.position = new Vector3(4f, 0f, 0f);

            ShopManager shop = CreateShop();
            CreateInterface(player.GetComponent<PlayerHealth>(), shop);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

            EditorUtility.DisplayDialog(
                "Prototype 0.1 ready",
                "The scene, test item and prefabs were created successfully. Open Prototype01 and press Play.",
                "OK");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Prototype build failed",
                "Unity could not finish the setup. Open the Console for the exact error.",
                "OK");
        }
    }

    private static void EnsureFolders()
    {
        string[] folders =
        {
            "Assets/_Project/Editor",
            "Assets/_Project/Scenes",
            "Assets/_Project/Art/Sprites",
            "Assets/_Project/ScriptableObjects/Items",
            "Assets/_Project/Prefabs/Player",
            "Assets/_Project/Prefabs/Enemies",
            "Assets/_Project/Prefabs/Items"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                string name = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static void EnsureTextMeshProResources()
    {
        if (TMP_Settings.defaultFontAsset != null)
        {
            return;
        }

        string packageCache = Path.GetFullPath("Library/PackageCache");
        string uguiPackage = Directory
            .GetDirectories(packageCache, "com.unity.ugui@*")
            .FirstOrDefault();
        string packagePath = uguiPackage == null
            ? string.Empty
            : Path.Combine(uguiPackage, "Package Resources", "TMP Essential Resources.unitypackage");

        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException(
                "TextMesh Pro essentials are missing. In Unity, use Window > TextMeshPro > Import TMP Essential Resources.");
        }

        AssetDatabase.ImportPackage(packagePath, false);
        AssetDatabase.Refresh();
    }

    private static void EnsureTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    private static int EnsureLayer(string layerName)
    {
        int existingLayer = LayerMask.NameToLayer(layerName);
        if (existingLayer >= 0)
        {
            return existingLayer;
        }

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < 32; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }

        throw new System.InvalidOperationException("There is no free user layer for the Enemy layer.");
    }

    private static Sprite EnsurePrototypeSprite()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = Enumerable.Repeat(Color.white, 32 * 32).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    private static ItemData EnsureTestItem()
    {
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(ItemPath);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, ItemPath);
        }

        item.itemId = "test_relic";
        item.displayName = "Test Relic";
        item.sellPrice = 20;
        item.rarity = ItemRarity.Common;
        item.description = "Prototype test item.";
        EditorUtility.SetDirty(item);
        return item;
    }

    private static GameObject CreatePlayerPrefab(Sprite sprite, int enemyLayer)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        AddSprite(player, sprite, new Color(0.2f, 0.55f, 1f));
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        ConfigureBody(body);
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<PlayerController2D>();
        player.AddComponent<PlayerHealth>();

        GameObject attackPoint = new GameObject("AttackPoint");
        attackPoint.transform.SetParent(player.transform, false);
        attackPoint.transform.localPosition = new Vector3(0.8f, 0f, 0f);

        PlayerAttack attack = player.AddComponent<PlayerAttack>();
        attack.attackPoint = attackPoint.transform;
        attack.attackDistance = 0.8f;
        attack.attackRadius = 0.75f;
        attack.damage = 10;
        attack.attackCooldown = 0.4f;
        attack.enemyLayers = 1 << enemyLayer;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
        Object.DestroyImmediate(player);
        return prefab;
    }

    private static GameObject CreatePickupPrefab(Sprite sprite, ItemData item)
    {
        GameObject pickup = new GameObject("TestRelicPickup");
        pickup.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        AddSprite(pickup, sprite, new Color(1f, 0.78f, 0.12f));

        BoxCollider2D collider = pickup.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.itemData = item;
        itemPickup.amount = 1;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pickup, PickupPrefabPath);
        Object.DestroyImmediate(pickup);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab(Sprite sprite, GameObject pickupPrefab, int enemyLayer)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.layer = enemyLayer;
        AddSprite(enemy, sprite, new Color(0.9f, 0.18f, 0.18f));

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        ConfigureBody(body);
        enemy.AddComponent<BoxCollider2D>();

        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        health.maxHealth = 30;
        health.itemDropPrefab = pickupPrefab;

        EnemyChaser chaser = enemy.AddComponent<EnemyChaser>();
        chaser.moveSpeed = 2f;
        chaser.contactDamage = 10;
        chaser.damageInterval = 1f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static void ConfigureBody(Rigidbody2D body)
    {
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private static void AddSprite(GameObject target, Sprite sprite, Color color)
    {
        SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateBackground(Sprite sprite)
    {
        GameObject floor = new GameObject("Prototype Floor");
        floor.transform.position = new Vector3(0f, 0f, 2f);
        floor.transform.localScale = new Vector3(20f, 12f, 1f);
        SpriteRenderer renderer = floor.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.18f, 0.2f, 0.24f);
        renderer.sortingOrder = -10;
    }

    private static void CreateGameSystems()
    {
        GameObject systems = new GameObject("GameSystems");
        systems.AddComponent<GameManager>();
        systems.AddComponent<InventoryManager>();
    }

    private static ShopManager CreateShop()
    {
        GameObject shopObject = new GameObject("Shop");
        ShopManager shop = shopObject.AddComponent<ShopManager>();
        shop.damageUpgradeCost = 20;
        shop.damageIncrease = 5;
        return shop;
    }

    private static void CreateInterface(PlayerHealth playerHealth, ShopManager shop)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        TextMeshProUGUI goldText = CreateText(canvasObject.transform, "GoldText", "Gold: 0", new Vector2(30f, -30f), 32f);
        TextMeshProUGUI healthText = CreateText(canvasObject.transform, "HealthText", "Health: 100/100", new Vector2(30f, -75f), 32f);
        TextMeshProUGUI inventoryText = CreateText(canvasObject.transform, "InventoryText", "Run inventory:", new Vector2(30f, -130f), 28f);
        inventoryText.rectTransform.sizeDelta = new Vector2(500f, 300f);

        TextMeshProUGUI controls = CreateText(
            canvasObject.transform,
            "ControlsText",
            "WASD: Move   Space: Dash   J / Left Click: Attack",
            new Vector2(30f, -1015f),
            25f);
        controls.rectTransform.anchorMin = new Vector2(0f, 0f);
        controls.rectTransform.anchorMax = new Vector2(0f, 0f);
        controls.rectTransform.pivot = new Vector2(0f, 0f);

        SimpleUIManager ui = canvasObject.AddComponent<SimpleUIManager>();
        ui.goldText = goldText;
        ui.healthText = healthText;
        ui.inventoryText = inventoryText;
        ui.playerHealth = playerHealth;

        Button sellButton = CreateButton(canvasObject.transform, "SellButton", "Sell All", new Vector2(-260f, 60f));
        Button upgradeButton = CreateButton(canvasObject.transform, "UpgradeButton", "Buy Damage +5 (20 gold)", new Vector2(-260f, 130f));
        UnityEventTools.AddPersistentListener(sellButton.onClick, shop.SellAll);
        UnityEventTools.AddPersistentListener(upgradeButton.onClick, shop.BuyDamageUpgrade);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string value,
        Vector2 anchoredPosition,
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
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(900f, 50f);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.34f, 0.52f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(360f, 55f);

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

    private static void AddSceneToBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes.Any(scene => scene.path == ScenePath))
        {
            return;
        }

        EditorBuildSettings.scenes = scenes
            .Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) })
            .ToArray();
    }
}
