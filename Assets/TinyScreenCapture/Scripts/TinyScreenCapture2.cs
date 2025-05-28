using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TinyScreenCapture2 : MonoBehaviour
{

#if UNITY_EDITOR
    private static TinyScreenCapture2 Instance { get; set; }
    [Header("Prefix for saved file.")]
    [SerializeField]
    private string _basePath = "/../Assets/TinyScreenCapture/ScreenCapture/";
    [SerializeField]
    private string _fileName = "ScreenCapture";
    [SerializeField]
    private bool _vertical = true;
    [SerializeField]
    private KeyCode _captureKey = KeyCode.F1;
    [Space]
    [Header("Resolution settings")]
    [SerializeField]
    private Vector2Int[] _portraitResolutions = {
       new Vector2Int(1080, 1920),
       new Vector2Int(1290, 2796),
       new Vector2Int(2048, 2732),
   };

    private Texture2D _texture;
    private Vector2Int _startSize;

    private string CaptureRoot
    {
        get
        {
            string project = Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
            _basePath = Path.Combine(Application.dataPath, "..",
                project + _fileName);
            return _basePath;
        }
    }


    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        _startSize = Screen.width > Screen.height ? new Vector2Int(Screen.width, Screen.height) : new Vector2Int(Screen.height, Screen.width);
    }
    void Update()
    {
        if (Input.GetKeyDown(_captureKey))
            StartCoroutine(TinyCapture());
    }

    private void OnValidate()
    {
        var path = CaptureRoot;
        bool check = false;

        foreach (var kvp in _portraitResolutions)
        {
            if (CheckResilution(kvp))
            {
                continue;
            }

            GetWH(kvp, out int width, out int height);
            print("Not found: " + new Vector2Int(width, height));
            EnsureGameViewSize(width, height);
            check = true;
        }

        if (!check)
        {
            print("full resolution");
        }
    }

    private IEnumerator TinyCapture()
    {

        print("Capturing...");

        Vector2Int[] resolutions = _portraitResolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            yield return null;

            int width, height;
            GetWH(resolutions[i], out width, out height);

            if (SetSize(width, height) == -1)
            {
                print("Size not found: " + width + "x" + height);
                continue;
            }
            yield return null;
            yield return new WaitForEndOfFrame();

            _texture = GetTexture();
            Save(resolutions, i, _texture);

            yield return null;
        }

        print("End Capturing");
    }

    private void GetWH(Vector2Int resolution, out int width, out int height)
    {
        width = _vertical ? resolution.x : resolution.y;
        height = _vertical ? resolution.y : resolution.x;
    }

    private static int SetSize(int width, int height)
    {
        var type = GameViewUtils.GetCurrentGroupType();
        int id = GameViewUtils.FindSize(type, width, height);

        if (id > 0)
        {
            GameViewUtils.SetSize(id);
        }

        return id;
    }

    private bool CheckResilution(Vector2Int resolution)
    {
        GetWH(resolution, out int width, out int height);
        var type = GameViewUtils.GetCurrentGroupType();
        int id = GameViewUtils.FindSize(type, width, height);
        return id >= 0;
    }

    private Texture2D GetTexture()
    {
        _texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _texture.Apply();
        return _texture;
    }

    private void Save(Vector2Int[] resolutions, int i, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string timestamp = System.DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
        //string basePath = Application.dataPath + _basePath;
        //string targetFolderPath = basePath + $"{resolutions[i].x}x{resolutions[i].y}/";
        string targetFolderPath = Path.Combine(CaptureRoot, $"{resolutions[i].x}x{resolutions[i].y}");
        if (!Directory.Exists(targetFolderPath))
            Directory.CreateDirectory(targetFolderPath);

        string filePath = Path.Combine(targetFolderPath, $"{_fileName}_{timestamp}.png");
        File.WriteAllBytes(filePath, bytes);
    }
    /// <summary>
    /// Создаёт пользовательский preset Game View, если его нет.
    /// </summary>
    static int EnsureGameViewSize(int width, int height, string label = null)
    {
        // 0. если уже есть такой пресет — просто вернуть его индекс
        var groupType = GameViewUtils.GetCurrentGroupType();
        int id = GameViewUtils.FindSize(groupType, width, height);
        if (id >= 0) return id;

        // 1. получаем доступ к скрытому синглтону GameViewSizes
        Assembly asm = typeof(Editor).Assembly;
        Type sizesType = asm.GetType("UnityEditor.GameViewSizes");
        Type singleType = asm.GetType("UnityEditor.ScriptableSingleton`1").MakeGenericType(sizesType);
        object sizesInst = singleType.GetProperty("instance").GetValue(null);

        // 2. выбираем текущую группу (Standalone, iOS, Android…)
        MethodInfo getGroup = sizesType.GetMethod("GetGroup");
        object groupInst = getGroup.Invoke(sizesInst, new object[] { (int)groupType });

        // 3. готовим данные для конструктора GameViewSize
        Type gvSizeType = asm.GetType("UnityEditor.GameViewSize");
        Type enumType = asm.GetType("UnityEditor.GameViewSizeType");
        object fixedEnum = Enum.Parse(enumType, "FixedResolution");

        ConstructorInfo ctor = gvSizeType.GetConstructor(new[] {
        enumType, typeof(int), typeof(int), typeof(string)
    });
        if (ctor == null)
        {
            Debug.LogError("GameViewSize constructor not found — Unity API изменился.");
            return -1;
        }

        object newSize = ctor.Invoke(new object[] {
        fixedEnum,
        width,
        height,
        label ?? $"{width}x{height}"
    });

        // 4. добавляем пресет в группу
        MethodInfo addCustomSize = groupInst.GetType().GetMethod("AddCustomSize");
        addCustomSize.Invoke(groupInst, new[] { newSize });

        // 5. ищем индекс снова и возвращаем
        return GameViewUtils.FindSize(groupType, width, height);
    }

    [ContextMenu("Tiny Zip ScreenCaptures")]
    public void ZipCaptures()
    {
        if (!Directory.Exists(CaptureRoot))
        {
            Debug.LogWarning("Папка скриншотов не найдена");
            return;
        }

        //string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //string zipPath = CaptureRoot + "_" + stamp + ".zip";
        string zipPath = CaptureRoot + ".zip";


        try
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(CaptureRoot, zipPath);
            Debug.Log($"Zip создан: {zipPath}");
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError("Не удалось создать zip: " + e.Message);
        }
    }
#endif
}