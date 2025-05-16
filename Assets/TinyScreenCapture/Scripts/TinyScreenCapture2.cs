using UnityEngine;
using System.Collections;
using System.IO;

public class TinyScreenCapture2 : MonoBehaviour
{

#if UNITY_EDITOR
    private static TinyScreenCapture2 Instance { get; set; }
    [Header("Prefix for saved file.")]
    [SerializeField]
    private string _basePath = "/../Assets/TinyScreenCapture/ScreenCapture/";
    [SerializeField]
    private string _fileName = "screenshoot";
    [SerializeField]
    private bool _vertical = true;
    [SerializeField]
    private KeyCode _captureKey = KeyCode.F1;
    [Space]
    [Header("Resolution settings")]
    [SerializeField]
    private Vector2Int[] _portraitResolutions = {
       new Vector2Int(1320, 2868),
       new Vector2Int(1290, 2786),
       new Vector2Int(2064, 2752),
       new Vector2Int(2048, 2732)
   };
    [SerializeField]
    private Vector2Int[] _landscapeResolutions = {
       new Vector2Int(2868, 1320),
       new Vector2Int(2786, 1290),
       new Vector2Int(2752, 2064),
       new Vector2Int(2732, 2048)
   };
    private Texture2D _texture;
    private Vector2Int _startSize;
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
    private IEnumerator TinyCapture()
    {

        print("Capturing...");

        Vector2Int[] resolutions = _vertical ? _portraitResolutions : _landscapeResolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            yield return null;

            int width = resolutions[i].x;
            int height = resolutions[i].y;

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

    private static int SetSize(int width, int height)
    {
        var type = GameViewUtils.GetCurrentGroupType();
        int id = GameViewUtils.FindSize(type, width, height);

        GameViewUtils.SetSize(id);
        return id;

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
        string basePath = Application.dataPath + _basePath;
        string targetFolderPath = basePath + $"{resolutions[i].x}x{resolutions[i].y}/";
        if (!Directory.Exists(targetFolderPath))
        {
            Directory.CreateDirectory(targetFolderPath);
        }
        File.WriteAllBytes(targetFolderPath + $"{_fileName}_{timestamp}.png", bytes);
    }

#endif
}