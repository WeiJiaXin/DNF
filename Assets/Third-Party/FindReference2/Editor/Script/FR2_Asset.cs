//#define FR2_DEBUG_BRACE_LEVEL
//#define FR2_DEBUG_SYMBOL
// #define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    public enum FR2_AssetType
    {
        UNKNOWN,
        FOLDER,
        SCRIPT,
        SCENE,
        DLL,
        REFERENCABLE,
        BINARY_ASSET,
        MODEL,
        TERRAIN,
        NON_READABLE
    }
	
    public enum FR2_AssetState
    {
        NEW,
        CACHE,
        MISSING
    }

    [Serializable]
    public class FR2_Asset
    {
        // ------------------------------ CONSTANTS ---------------------------

	    private static readonly HashSet<string> SCRIPT_EXTENSIONS = new HashSet<string>()
	    {
	    	".cs", ".js", ".boo",".h"
	    };

	    private static readonly HashSet<string> REFERENCABLE_EXTENSIONS = new HashSet<string>()
        {
            ".anim", ".controller", ".mat", ".unity", ".guiskin", ".prefab",
            ".overridecontroller", ".mask", ".rendertexture", ".cubemap", ".flare",
	        ".mat", ".prefab", ".physicsmaterial", ".fontsettings", ".asset", ".prefs"/*,".spriteatlas"*/
        };

	    private static readonly HashSet<string> IGNORE_GUIDS = new HashSet<string>()
        {
            // "00000000000000001000000000000000", // Assets 
            // "00000000000000002000000000000000", // ProjectSettings/InputManager.asset
            // "00000000000000003000000000000000", // ProjectSettings/TagManager.asset
            // // "00000000000000004000000000000000", // ProjectSettings/ProjectSettings.asset
            // "00000000000000005000000000000000", // Library/BuildPlayer.prefs
            // "00000000000000006000000000000000", // ProjectSettings/AudioManager.asset
            // "00000000000000007000000000000000", // ProjectSettings/TimeManager.asset
            // "00000000000000008000000000000000", // ProjectSettings/DynamicsManager.asset
            // "00000000000000009000000000000000", // ProjectSettings/QualitySettings.asset
		
            // // Reserved
            // "00000000000000000000000000000000",
            // "0000000000000000a000000000000000",
            // // "0000000000000000b000000000000000", // ProjectSettings/EditorBuildSettings.asset
            // // "0000000000000000c000000000000000", // ProjectSettings/EditorSettings.asset
            // "0000000000000000d000000000000000", // Library/unity editor resources
            // "0000000000000000e000000000000000", // Library/unity default resources
            // "0000000000000000f000000000000000"  // Resources/unity_builtin_extra
        };
        internal static readonly HashSet<string> IGNORE_UNUSED_GUIDS = new HashSet<string>()
        {
             "00000000000000001000000000000000", // Assets 
             "00000000000000002000000000000000", // ProjectSettings/InputManager.asset
             "00000000000000003000000000000000", // ProjectSettings/TagManager.asset
             "00000000000000004000000000000000", // ProjectSettings/ProjectSettings.asset
             "00000000000000005000000000000000", // Library/BuildPlayer.prefs
             "00000000000000006000000000000000", // ProjectSettings/AudioManager.asset
             "00000000000000007000000000000000", // ProjectSettings/TimeManager.asset
             "00000000000000008000000000000000", // ProjectSettings/DynamicsManager.asset
             "00000000000000009000000000000000", // ProjectSettings/QualitySettings.asset

             // Reserved
             "00000000000000000000000000000000",
             "0000000000000000a000000000000000",
              "0000000000000000b000000000000000", // ProjectSettings/EditorBuildSettings.asset
              "0000000000000000c000000000000000", // ProjectSettings/EditorSettings.asset
             "0000000000000000d000000000000000", // Library/unity editor resources
             "0000000000000000e000000000000000", // Library/unity default resources
             "0000000000000000f000000000000000",  // Resources/unity_builtin_extra,
             "00000000000000005100000000000000",  //Physics2DSettings.asset
             "00000000000000006100000000000000",  //GraphicsSettings.asset
             "00000000000000007100000000000000",  //ClusterInputManager.asset
             "0000000000000000a100000000000000",  //UnityConnectSettings.asset
             "0000000000000000b100000000000000",  //PresetManager.asset
             "0000000000000000c100000000000000",  //VFXManager.asset

             "00000000000000004100000000000000"  //NavMeshAreas.asset
        };

        private static readonly HashSet<string> SCRIPT_KEYWORDS = new HashSet<string>()
	    {
		    "abstract", "as", "base", "break", "case", "catch", "checked", "class", "const", "continue",
		    "default", "delegate", "do", "else", "enum", "event", "explicit", "extern", "finally",
		    "fixed", "for", "foreach", "goto", "if", "implicit", "in", "interface", "internal", "is",
		    "lock", "namespace", "new", "operator", "out", "override", "params", "private",
		    "protected", "public", "readonly", "ref", "return", "sealed", "sizeof", "stackalloc", "static",
		    "struct", "switch", "this", "throw", "try", "typeof", "unchecked", "unsafe", "using", "virtual",
		    "volatile", "while",
		    "define", "elif", "else", "endif", "endregion", "error", "if", "line", "pragma", "region", "undef",
		    "warning",
		    "bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "sbyte", "short",
		    "string", "uint", "ulong", "ushort", "void",
		    "var", "true", "false", "Rect", "RectOffset", "CustomEditor", "null", "UNITY_4_3",
		    "UNITY_5", "UNITY_4", "Type", "BindingFlags", "get", "set", "NonSerialized", "Serialized", "Package",
		    "GUIContent", "AppDomain"
	    };
        private static readonly Dictionary<int, string> HashClassesNormal = new Dictionary<int, string>()
        {
            {1, 	"UnityEngine.GameObject"},
			{2, 	"UnityEngine.Component"},
			{4, 	"UnityEngine.Transform"},
			{8, 	"UnityEngine.Behaviour"},
			{12, 	"UnityEngine.ParticleAnimator"},
			{15, 	"UnityEngine.EllipsoidParticleEmitter"},
			{20, 	"UnityEngine.Camera"},
			{21, 	"UnityEngine.Material"},
			{23, 	"UnityEngine.MeshRenderer"},
			{25, 	"UnityEngine.Renderer"},
			{26, 	"UnityEngine.ParticleRenderer"},
			{27, 	"UnityEngine.Texture"},
			{28, 	"UnityEngine.Texture2D"},
			{33, 	"UnityEngine.MeshFilter"},
			{41, 	"UnityEngine.OcclusionPortal"},
			{43, 	"UnityEngine.Mesh"},
			{45, 	"UnityEngine.Skybox"},
			{47, 	"UnityEngine.QualitySettings"},
			{48, 	"UnityEngine.Shader"},
			{49, 	"UnityEngine.TextAsset"},
			{50, 	"UnityEngine.Rigidbody2D"},
			{53, 	"UnityEngine.Collider2D"},
			{54, 	"UnityEngine.Rigidbody"},
			{56, 	"UnityEngine.Collider"},
			{57, 	"UnityEngine.Joint"},
			{58, 	"UnityEngine.CircleCollider2D"},
			{59, 	"UnityEngine.HingeJoint"},
			{60, 	"UnityEngine.PolygonCollider2D"},
			{61, 	"UnityEngine.BoxCollider2D"},
			{62, 	"UnityEngine.PhysicsMaterial2D"},
			{64, 	"UnityEngine.MeshCollider"},
			{65, 	"UnityEngine.BoxCollider"},
			{68, 	"UnityEngine.EdgeCollider2D"},
			{72, 	"UnityEngine.ComputeShader"},
			{74, 	"UnityEngine.AnimationClip"},
			{75, 	"UnityEngine.ConstantForce"},
			{81, 	"UnityEngine.AudioListener"},
			{82, 	"UnityEngine.AudioSource"},
			{83, 	"UnityEngine.AudioClip"},
			{84, 	"UnityEngine.RenderTexture"},
			{87, 	"UnityEngine.MeshParticleEmitter"},
			{88, 	"UnityEngine.ParticleEmitter"},
			{89, 	"UnityEngine.Cubemap"},
			{90, 	"Avatar"},
			{92, 	"UnityEngine.GUILayer"},
			{93, 	"UnityEngine.RuntimeAnimatorController"},
			{95, 	"UnityEngine.Animator"},
			{96, 	"UnityEngine.TrailRenderer"},
			{102, 	"UnityEngine.TextMesh"},
			{104, 	"UnityEngine.RenderSettings"},
			{108, 	"UnityEngine.Light"},
			{111, 	"UnityEngine.Animation"},
			{114, 	"UnityEngine.MonoBehaviour"},
			{115, 	"UnityEditor.MonoScript"},
			{117, 	"UnityEngine.Texture3D"},
			{119, 	"UnityEngine.Projector"},
			{120, 	"UnityEngine.LineRenderer"},
			{121, 	"UnityEngine.Flare"},
			{123, 	"UnityEngine.LensFlare"},
			{124, 	"UnityEngine.FlareLayer"},
			{128, 	"UnityEngine.Font"},
			{129, 	"UnityEditor.PlayerSettings"},
			{131, 	"UnityEngine.GUITexture"},
			{132, 	"UnityEngine.GUIText"},
			{133, 	"UnityEngine.GUIElement"},
			{134, 	"UnityEngine.PhysicMaterial"},
			{135, 	"UnityEngine.SphereCollider"},
			{136, 	"UnityEngine.CapsuleCollider"},
			{137, 	"UnityEngine.SkinnedMeshRenderer"},
			{138, 	"UnityEngine.FixedJoint"},
			{142, 	"UnityEngine.AssetBundle"},
			{143, 	"UnityEngine.CharacterController"},
			{144, 	"UnityEngine.CharacterJoint"},
			{145, 	"UnityEngine.SpringJoint"},
			{146, 	"UnityEngine.WheelCollider"},
			{152, 	"UnityEngine.MovieTexture"},
			{153, 	"UnityEngine.ConfigurableJoint"},
			{154, 	"UnityEngine.TerrainCollider"},
			{156, 	"UnityEngine.TerrainData"},
			{157, 	"UnityEngine.LightmapSettings"},
			{158, 	"UnityEngine.WebCamTexture"},
			{159, 	"UnityEditor.EditorSettings"},
			{162, 	"UnityEditor.EditorUserSettings"},
			{164, 	"UnityEngine.AudioReverbFilter"},
			{165, 	"UnityEngine.AudioHighPassFilter"},
			{166, 	"UnityEngine.AudioChorusFilter"},
			{167, 	"UnityEngine.AudioReverbZone"},
			{168, 	"UnityEngine.AudioEchoFilter"},
			{169, 	"UnityEngine.AudioLowPassFilter"},
			{170, 	"UnityEngine.AudioDistortionFilter"},
			{171, 	"UnityEngine.SparseTexture"},
			{180, 	"UnityEngine.AudioBehaviour"},
			{182, 	"UnityEngine.WindZone"},
			{183, 	"UnityEngine.Cloth"},
			{192, 	"UnityEngine.OcclusionArea"},
			{193, 	"UnityEngine.Tree"},
			{198, 	"UnityEngine.ParticleSystem"},
			{199, 	"UnityEngine.ParticleSystemRenderer"},
			{200, 	"UnityEngine.ShaderVariantCollection"},
			{205, 	"UnityEngine.LODGroup"},
			{207, 	"UnityEngine.Motion"},
			{212, 	"UnityEngine.SpriteRenderer"},
			{213, 	"UnityEngine.Sprite"},
			{215, 	"UnityEngine.ReflectionProbe"},
			{218, 	"UnityEngine.Terrain"},
			{220, 	"UnityEngine.LightProbeGroup"},
			{221, 	"UnityEngine.AnimatorOverrideController"},
			{222, 	"UnityEngine.CanvasRenderer"},
			{223, 	"UnityEngine.Canvas"},
			{224, 	"UnityEngine.RectTransform"},
			{225, 	"UnityEngine.CanvasGroup"},
			{226, 	"UnityEngine.BillboardAsset"},
			{227, 	"UnityEngine.BillboardRenderer"},
			{229, 	"UnityEngine.AnchoredJoint2D"},
			{230, 	"UnityEngine.Joint2D"},
			{231, 	"UnityEngine.SpringJoint2D"},
			{232, 	"UnityEngine.DistanceJoint2D"},
			{233, 	"UnityEngine.HingeJoint2D"},
			{234, 	"UnityEngine.SliderJoint2D"},
			{235, 	"UnityEngine.WheelJoint2D"},
			{246, 	"UnityEngine.PhysicsUpdateBehaviour2D"},
			{247, 	"UnityEngine.ConstantForce2D"},
			{248, 	"UnityEngine.Effector2D"},
			{249, 	"UnityEngine.AreaEffector2D"},
			{250, 	"UnityEngine.PointEffector2D"},
			{251, 	"UnityEngine.PlatformEffector2D"},
			{252, 	"UnityEngine.SurfaceEffector2D"},
			{258, 	"UnityEngine.LightProbes"},
			{290, 	"UnityEngine.AssetBundleManifest"},
			{1003, 	"UnityEditor.AssetImporter"},
			{1004, 	"UnityEditor.AssetDatabase"},
			{1006, 	"UnityEditor.TextureImporter"},
			{1007, 	"UnityEditor.ShaderImporter"},
			{1011, 	"UnityEngine.AvatarMask"},
			{1020, 	"UnityEditor.AudioImporter"},
			{1029, 	"UnityEditor.DefaultAsset"},
			{1032, 	"UnityEditor.SceneAsset"},
			{1035, 	"UnityEditor.MonoImporter"},
			{1040, 	"UnityEditor.ModelImporter"},
			{1042, 	"UnityEditor.TrueTypeFontImporter"},
			{1044, 	"UnityEditor.MovieImporter"},
			{1045, 	"UnityEditor.EditorBuildSettings"},
			{1050, 	"UnityEditor.PluginImporter"},
			{1051, 	"UnityEditor.EditorUserBuildSettings"},
			{1105, 	"UnityEditor.HumanTemplate"},
			{1110, 	"UnityEditor.SpeedTreeImporter"},
			{1113, 	"UnityEditor.LightmapParameters"}
        };
        // private static Dictionary<int, string> _HashClasses;
        // private static Dictionary<int, string> HashClasses
        // {
        //     get{
        //         if(_HashClasses == null)
        //         {
        //             _HashClasses = new Dictionary<int, string>();
        //             string s ="";
        //             foreach(var item in HashClassesNormal) 
        //             {
        //                 s+= "{"+ item.Key + ", \t" + "\""+item.Value.FullName+"\"},\n";
        //                 // Debug.Log(item.Value + "  => " +f);
        //                 _HashClasses.Add(item.Key, item.Value.ToString());
        //             }
        //             Debug.Log(s);

        //         }
        //         return _HashClasses;
        //     }
        // }
        private static Dictionary<int, Type> HashClasses = new Dictionary<int, Type>();
	    
	    private static readonly HashSet<string> SCRIPT_SYMBOL = new HashSet<string>()
        {
            "class", "interface", "enum", "struct", "delegate"
        };
        // [SerializeField] public string assetPath;
        // [SerializeField] public string assetFolder;
        // [SerializeField] public string assetName;

        // [SerializeField] public int cacheStamp;
        // [SerializeField] public string extension;
        // [SerializeField] public string fileInfoHash;

        // [SerializeField] public long fileSize;

        // public string guid;

        // [SerializeField] public bool inEditor;
        // [SerializeField] public bool inPlugins;
        // [SerializeField] public bool inResources;
        // [SerializeField] public bool inStreamingAsset;
        public string guid;
        private void checkLoadContent()
        {
            LoadAssetInfo();
        }
	    internal string assetPath
        {
            get{
                checkLoadContent();
                return m_assetPath;
            }
        }
        internal string assetFolder
        {
            get{
                checkLoadContent();
                return m_assetFolder;
            }
        }
        internal string assetName
        {
            get{
                checkLoadContent();
                return m_assetName;
            }
            set{
                m_assetName = value;
            }
        }

        internal int cacheStamp
        {
            get{
                checkLoadContent();
                return m_cacheStamp;
            }
        }
        internal string extension
        {
            get{
                checkLoadContent();
                return m_extension;
            }
        }
        internal string fileInfoHash
        {
            get{
                checkLoadContent();
                return m_fileInfoHash;
            }
        }

        internal long fileSize
        {
            get{
                checkLoadContent();
                return m_fileSize;
            }
        }


        internal bool inEditor
        {
            get{
                checkLoadContent();
                return m_inEditor;
            }
        }
        internal bool inPlugins
        {
            get{
                checkLoadContent();
                return m_inPlugins;
            }
        }
        internal bool inResources
        {
            get{
                checkLoadContent();
                return m_inResources;
            }
        }
        internal bool inStreamingAsset
        {
            get{
                checkLoadContent();
                return m_inStreamingAsset;
            }
        }


        //mmmm
        internal string m_assetPath;
        internal string m_assetFolder;
        internal string m_assetName;

        internal int m_cacheStamp;
        internal string m_extension;
        internal string m_fileInfoHash;

        internal long m_fileSize;

        internal bool m_isAssetFile;
        internal bool m_inEditor;
        internal bool m_inPlugins;
        internal bool m_inResources;
        internal bool m_inStreamingAsset;


        internal bool loaded;
	    //public 
	    List<string> ScriptSymbols = null; // class, enum, delegate, interface definitions
	    //public 
	    List<string> ScriptTokens = null; // possibly used symbols

        [NonSerialized] internal List<string> ScriptUsage = new List<string>();

        public int stamp;

        //internal FR2_AssetType __type;

        //{
        //    get { return __type; }
        //    set { __type = value;
        //        if (assetPath.EndsWith("paintjob.asset"))
        //        {
        //            Debug.Log(assetPath + ":" + value);
        //        }
        //    }
        //}

        internal FR2_AssetState state;
	    public FR2_AssetType type;
        [Serializable]
        private class Classes {
            public string guid;
            public List<int> ids;
        }
        [SerializeField] private List<Classes> UseGUIDsList = new List<Classes>();
	    private Dictionary<string, HashSet<int>> _UseGUIDs;
	    public Dictionary<string, HashSet<int>> UseGUIDs
        {
            get {
				if (_UseGUIDs != null) return _UseGUIDs;

                _UseGUIDs = new Dictionary<string, HashSet<int>>(UseGUIDsList.Count);
                for(int i = 0; i < UseGUIDsList.Count; i++)
                {
                    string guid = UseGUIDsList[i].guid;
                    if(_UseGUIDs.ContainsKey(guid))
                    {
                        for(int j = 0; j < UseGUIDsList[i].ids.Count; j++)
                        {
                            int val = UseGUIDsList[i].ids[j];
                            if(_UseGUIDs[guid].Contains(val)) continue;
                            _UseGUIDs[guid].Add(UseGUIDsList[i].ids[j]);
                        }
                    }
                    else
                    {
                        _UseGUIDs.Add(guid, new HashSet<int>(UseGUIDsList[i].ids));

                    }
                }
                return _UseGUIDs;
            }    
        }
	    
        //[NonSerialized] internal List<FR2_Asset> UsedBy		= new List<FR2_Asset>();
        internal HashSet<int> HashUsedByClassesIds = new HashSet<int>();
        internal Dictionary<string, FR2_Asset> UsedByMap = new Dictionary<string, FR2_Asset>();
        public void AddUsedBy(string guid, FR2_Asset asset)
	    {
            if(UsedByMap.ContainsKey(guid)) return;
            UsedByMap.Add(guid, asset);
            HashSet<int> output;
            if(HashUsedByClassesIds == null) HashUsedByClassesIds = new HashSet<int>();
            if(asset.UseGUIDs.TryGetValue(this.guid, out output))
            {
                foreach(var item in output)
                {
                    HashUsedByClassesIds.Add(item); 
                }
            }
            
            // int classId = HashUseByClassesIds    
        }

        public FR2_Asset(string guid)
        {
            this.guid = guid;

            type = FR2_AssetType.UNKNOWN;
            // UseGUIDs = new Dictionary<string, HashSet<int>>();
	        //ScriptSymbols = new List<string>();
	        //ScriptTokens = new List<string>();
        }
        public int UsageCount()
        {
            return UsedByMap.Count;
        }

        // ------------------------------- GETTERS -----------------------------

        internal string parentFolderPath
        {
            get { return assetPath.Substring(0, assetPath.LastIndexOf('/')); }
        }

        internal bool IsFolder
        {
            get { return type == FR2_AssetType.FOLDER; }
        }

        internal bool IsScript
        {
            get { return type == FR2_AssetType.SCRIPT; }
        }

        static public float ignoreTS;
        float excludeTS;


        bool _isExcluded;
        internal bool IsExcluded
        {
            get {
                if (excludeTS >= ignoreTS) return _isExcluded; 
                excludeTS = ignoreTS;

                _isExcluded = false;

                var h = FR2_Setting.IgnoreAsset;
                foreach (var item in FR2_Setting.IgnoreAsset)
                {
                    if (assetPath.StartsWith(item))
                    {
                        _isExcluded = true;
                        break;
                    }
                }

                return _isExcluded;
            }
        }

        internal bool IsReferencable
        {
            get { return type == FR2_AssetType.REFERENCABLE || type == FR2_AssetType.SCENE; }
        }

        internal bool IsBinaryAsset
        {
            get
            {
                return type == FR2_AssetType.BINARY_ASSET ||
                       type == FR2_AssetType.MODEL ||
                       type == FR2_AssetType.TERRAIN;
            }
        }

        internal bool IsMissing
        {
            get { return state == FR2_AssetState.MISSING; }
        }

        public override string ToString()
        {
            return string.Format("FR2_Asset[{0}]", assetName);
        }

        //--------------------------------- STATIC ----------------------------

        internal static bool IsValidGUID(string guid)
        {
	        if (IGNORE_GUIDS.Contains(guid))
            {
                return false;
            }
    
            var p = AssetDatabase.GUIDToAssetPath(guid);
	        if (p != null && !FR2_Cache.ASSET_ROOT_ALLOW.Any(x=> p.StartsWith(x, StringComparison.Ordinal)))
                return false; // Asset can be missing but can not be at invalid path 
	        return p != FR2_Cache.CachePath;
        }

        internal void MarkAsDirty()
        {
            //Debug.Log("Mark Dirty : " + assetName + ":" + type);
            m_fileInfoHash = null;
            m_cacheStamp = 0;
            stamp = 0;
            loaded = false;
        }

        // --------------------------------- APIs ------------------------------

        internal void GuessAssetType()
        {
	        if (SCRIPT_EXTENSIONS.Contains(m_extension))
            {
                type = FR2_AssetType.SCRIPT;
            }
	        else if (REFERENCABLE_EXTENSIONS.Contains(m_extension))
            {
                var isUnity = m_extension == ".unity";
                type = isUnity ? FR2_AssetType.SCENE : FR2_AssetType.REFERENCABLE;

                if (m_extension == ".asset" || isUnity/* || m_extension == ".spriteatlas"*/)
                {
                    var buffer = new byte[5];

                    try
                    {
                        var stream = File.OpenRead(m_assetPath);
                        stream.Read(buffer, 0, 5);
                        stream.Close();
                    }
#if FR2_DEBUG
                    catch (Exception e)
                    {
                        Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + m_assetPath); 
#else
                    catch
                    {
#endif
                        state = FR2_AssetState.MISSING;
                        return;
                    }

                    var str = string.Empty;
                    foreach (var t in buffer)
                    {
                        str += (char) t;
                    }

                    if (str != "%YAML")
                    {
                        type = FR2_AssetType.BINARY_ASSET;
                        //var assetData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                        //type = assetData is TerrainData ? FR2_AssetType.TERRAIN : FR2_AssetType.BINARY_ASSET;
                        //assetData = null;
                        //FR2_Unity.UnloadUnusedAssets();
                    }
                }
            }
            else if (m_extension == ".fbx")
            {
                type = FR2_AssetType.MODEL;
            }
            else if (m_extension == ".dll")
            {
                type = FR2_AssetType.DLL;
            }
            else
            {
                type = FR2_AssetType.NON_READABLE;
            }
        }
        

        static public float lastRefreshTS = 0f;
	    [SerializeField] internal float loadInfoTS;

        internal void LoadAssetInfo()
	    {
            if (string.IsNullOrEmpty(m_assetPath))
            {
                // Force refresh for the first time!
                loadInfoTS = lastRefreshTS -1;
            }

		    if (loadInfoTS >= lastRefreshTS)
            {
                // #if FR2_DEBUG
                // Debug.Log("Skipped: " + loadInfoTS + ":" + AssetDatabase.GUIDToAssetPath(guid));
                // #endif
                return;
            }

#if FR2_DEBUG
            Debug.LogWarning("Refreshing ... " + loadInfoTS + ":" + AssetDatabase.GUIDToAssetPath(guid));
#endif

		    loadInfoTS = lastRefreshTS;

	        m_assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var assetPath = m_assetPath;
#if FR2_DEBUG
	        Debug.Log("LoadAssetInfo: " + m_assetPath);
#endif

            if (string.IsNullOrEmpty(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }
            
#if FR2_DEBUG
            // if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            if (!FR2_Cache.ASSET_ROOT_ALLOW.Any(x=> assetPath.StartsWith(x, StringComparison.Ordinal)))
            {

                Debug.LogWarning("Something wrong ! Should never be here !\n"  + assetPath + "\n" + guid);

                return;
            }
#endif

            var info = new FileInfo(assetPath);
            m_assetName = info.Name;

            var assetName = m_assetName;
            m_extension = info.Extension.ToLower();
            m_assetFolder = assetPath.Substring(7, Mathf.Max(0, assetPath.Length - assetName.Length - 7));

            var assetTime = FR2_Unity.Epoch(info.LastWriteTime);

            // check meta timestamp as well
            var metaInfo = new FileInfo(assetPath + ".meta");
            var metaTime = FR2_Unity.Epoch(metaInfo.LastWriteTime);

            // 7 = "Assets/".Length
            loaded = (stamp == Mathf.Max(metaTime, assetTime));
            
            if (Directory.Exists(info.FullName))
            {
                type = FR2_AssetType.FOLDER;
            }
            else if (File.Exists(info.FullName))
            {
                if (type == FR2_AssetType.UNKNOWN) GuessAssetType();

                m_fileSize = info.Length;
                m_inEditor = assetPath.Contains("/Editor/") || assetPath.Contains("/Editor Default Resources/");
                m_inResources = assetPath.Contains("/Resources/");
                m_inStreamingAsset = assetPath.Contains("/StreamingAssets/");
                m_inPlugins = assetPath.Contains("/Plugins/");
                
                m_fileInfoHash = info.Length + info.Extension;
                m_isAssetFile = assetPath.EndsWith(".asset", StringComparison.Ordinal);
            }
            else
            {
                state = FR2_AssetState.MISSING;
            }
        }

        internal void LoadContent(bool force)
        {
            LoadAssetInfo();

            if (IsMissing || type == FR2_AssetType.NON_READABLE) return;

            if (type == FR2_AssetType.DLL)
            {
#if FR2_DEBUG
            Debug.LogWarning("Parsing DLL not yet supportted ");
#endif
                return;
            }

            if (loaded && !force) return;

            // Check for file / folder changes & validate if file / folder exist
            var newStamp = stamp;
            var exist = true;

            FileSystemInfo info;

            if (IsFolder)
            {
                info = new DirectoryInfo(assetPath);
            }
            else
            {
                info = new FileInfo(assetPath);
            }

            exist = info.Exists;
            var assetTime = FR2_Unity.Epoch(info.LastWriteTime);

            // check meta timestamp as well
            var metaInfo = new FileInfo(assetPath + ".meta");
            var metaTime = FR2_Unity.Epoch(metaInfo.LastWriteTime);

            newStamp = Mathf.Max(assetTime, metaTime);

            if (!exist)
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            loaded = true;
            if (newStamp == stamp && !force)
            {
#if FR2_DEBUG
            Debug.Log("Unchanged : " + stamp + ":" + assetName + ":" + type);
#endif
                return; // nothing changed
            }
            stamp = newStamp;

            ClearUseGUIDs();
            

            if (IsFolder)
            {
                LoadFolder();
            }
            else if (IsReferencable)
            {
                // LoadYAML();
                LoadYAML2();
            }
            else if (IsBinaryAsset)
            {
                LoadBinaryAsset();
            }
            else if (IsScript)
            {
                // LoadScript();
            }
        }

        internal void AddUseGUID(string fguid, int fFileId = -1, bool checkExist = true)
        {
            // if (checkExist && UseGUIDs.ContainsKey(fguid)) return;
            if (!IsValidGUID(fguid)) return;
            if(!UseGUIDs.ContainsKey(fguid))
            {
                UseGUIDsList.Add(new Classes()
                {
                    guid = fguid,
                    ids =new List<int>()
                });
                UseGUIDs.Add(fguid, new HashSet<int>());
            }
            if(fFileId != -1)
            {
                if(UseGUIDs[fguid].Contains(fFileId)) return;
                UseGUIDs[fguid].Add(fFileId);
                var i = UseGUIDsList.FirstOrDefault(x => x.guid == fguid);
                if(i != null)
                {
                    i.ids.Add(fFileId);
                }
            }
        }

        // ----------------------------- STATIC  ---------------------------------------

        internal static int SortByExtension(FR2_Asset a1, FR2_Asset a2)
        {
            if (a1 == null) return -1;
            if (a2 == null) return 1;

            var result = a1.extension.CompareTo(a2.extension);
            return result == 0 ? a1.assetName.CompareTo(a2.assetName) : result;
        }

        internal static List<FR2_Asset> FindUsage(FR2_Asset asset)
        {
            if (asset == null) return null;

            var refs = FR2_Cache.Api.FindAssets(asset.UseGUIDs.Keys.ToArray(), true);
	        
	        //if (asset.ScriptUsage != null)
	        //{
	        //	for (var i = 0; i < asset.ScriptUsage.Count; i++)
	        //	{
		    //    	var symbolList = FR2_Cache.Api.FindAllSymbol(asset.ScriptUsage[i]);
		    //    	if (symbolList.Contains(asset)) continue;
		        	
		    //    	var symbol = symbolList[0];
		    //    	if (symbol == null || refs.Contains(symbol)) continue;
		    //    	refs.Add(symbol);
	        //	}	
	        //}
	        
            return refs;
        }

        internal static List<FR2_Asset> FindUsedBy(FR2_Asset asset)
        {
            return asset.UsedByMap.Values.ToList();
        }
	    
	    internal static List<string> FindUsageGUIDs(FR2_Asset asset, bool includeScriptSymbols)
	    {
	    	var result = new HashSet<string>();
	    	if (asset == null) 
	    	{
	    		Debug.LogWarning("Asset invalid : " + asset.assetName);
	    		return result.ToList();
	    	}
	    	
	    	// for (var i = 0;i < asset.UseGUIDs.Count; i++)
	    	// {
	    	// 	result.Add(asset.UseGUIDs[i]);
	    	// }
            foreach (var item in asset.UseGUIDs)
	    	{
	    		result.Add(item.Key);
	    	}
	    	
	    	//if (!includeScriptSymbols) return result.ToList();
	    	
	    	//if (asset.ScriptUsage != null)
	    	//{
		    //	for (var i = 0; i < asset.ScriptUsage.Count; i++)
		    //	{
			//    	var symbolList = FR2_Cache.Api.FindAllSymbol(asset.ScriptUsage[i]);
			//    	if (symbolList.Contains(asset)) continue;
			    	
			//    	var symbol = symbolList[0];
			//    	if (symbol == null || result.Contains(symbol.guid)) continue;
			    	
			//    	result.Add(symbol.guid);
		    //	}	
	    	//}
	    	
	    	return result.ToList();
	    }
	    
	    internal static List<string> FindUsedByGUIDs(FR2_Asset asset)
	    {
		    return asset.UsedByMap.Keys.ToList();
	    }
	    
        // ----------------------------- DRAW  ---------------------------------------
        

        internal float Draw(Rect r, bool highlight, bool drawPath = true, IWindow window = null)
        {
//, bool hasMouse
            var singleLine = r.height <= 18f;
            var rw = r.width;
            var selected = FR2_Selection.IsSelect(guid);

            r.height = 16f;
            var hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

            if (hasMouse && Event.current.button == 1)
            {
	            var menu = new GenericMenu();
	            if (extension == ".prefab")
	            {
	            	menu.AddItem(new GUIContent("Edit in Scene"), false, EditPrefab);	
	            }
	            
                menu.AddItem(new GUIContent("Open"), false, Open);
	            menu.AddItem(new GUIContent("Ping"), false, Ping);
                menu.AddItem(new GUIContent(guid), false, CopyGUID);
                //menu.AddItem(new GUIContent("Reload"), false, Reload);

                menu.AddSeparator(string.Empty);


                if (selected)
                {
                    menu.AddDisabledItem(new GUIContent("Add to Selection"));
                    menu.AddItem(new GUIContent("Remove from Selection"), false, RemoveFromSelection);
                }
                else
                {
                    menu.AddItem(new GUIContent("Add to Selection"), false, AddToSelection);
                    menu.AddDisabledItem(new GUIContent("Remove from Selection"));
                }

                menu.AddItem(new GUIContent("Copy path"), false, CopyAssetPath);
                menu.AddItem(new GUIContent("Copy full path"), false, CopyAssetPathFull);

                //if (IsScript)
                //{
                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptSymbols, "+ ", "Definitions", "No Definition", false);
	                
                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptUsage, "-> ", "Depends", "No Dependency", true);
                //}

                menu.ShowAsContext();
                Event.current.Use();
            }
            
            if (IsMissing)
            {
	            if (!singleLine) r.y += 16f;
	            if (Event.current.type != EventType.Repaint) return 0;
                GUI.Label(r, "(missing) " + guid, EditorStyles.whiteBoldLabel);
                return 0;
            }

            //if (IsScript)
            //{
            //    var w = 40f;
            //    var rr = r;
            //    rr.x += r.width - w;
            //    rr.width = w;

            //    GUI.Label(rr,
            //        (ScriptSymbols != null ? "" + ScriptSymbols.Count : "-") + "|" +
            //        (ScriptUsage != null ? "" + ScriptUsage.Count : "-"));
            //}

	        //var usageRect = LeftRect(20f, ref r);
	        
	        var iconRect = LeftRect(16f, ref r);
	        if (Event.current.type == EventType.Repaint)
	        {
	        	var icon = AssetDatabase.GetCachedIcon(assetPath);
            	if (icon != null) GUI.DrawTexture(iconRect, icon);
	        }
	        
           
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
            	var pingRect = FR2_Setting.PingRow ? new Rect(0,r.y, r.x + r.width, r.height) : iconRect;
            	if (pingRect.Contains(Event.current.mousePosition))
            	{
            		if(Event.current.control || Event.current.command)
            		{
            			if(selected)
            			{
            				RemoveFromSelection();
            			}
            			else
            			{
            				AddToSelection();
            			}

                        if (window != null) window.Repaint();

                    }
            		Ping();	
            		//Event.current.Use();
            	}
            }
            
	        if (Event.current.type != EventType.Repaint) return 0;

            if (UsedByMap != null && UsedByMap.Count > 0)
            {
               var str = new GUIContent(UsedByMap.Count.ToString());
			   var countRect = iconRect;
			   countRect.xMin-=16f;
               GUI.Label(countRect, str);
            }

	        var pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(assetFolder)).x : 0;
	        var nameW = EditorStyles.boldLabel.CalcSize(new GUIContent(assetName)).x;
            var cc = FR2_Cache.Api.setting.SelectedColor;

            if (singleLine)
            {
                var lbRect = LeftRect(pathW + nameW, ref r);

                if (highlight || selected)
                {
                    var c = GUI.color;
                    GUI.color = cc;
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c;
                }
                       
                if (drawPath)
                {
                    GUI.Label(LeftRect(pathW, ref lbRect), assetFolder, EditorStyles.miniLabel);
                    lbRect.xMin -= 4f;
                    GUI.Label(lbRect, assetName, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(lbRect, assetName);
                }
            }
            else
            {
                if (drawPath)
                    GUI.Label(new Rect(r.x, r.y + 16f, r.width, r.height), assetFolder, EditorStyles.miniLabel);

                var lbRect = LeftRect(nameW, ref r);
                if (highlight || selected)
                {
                    var c = GUI.color;
                    GUI.color = cc;
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c;
                }
                GUI.Label(lbRect, assetName, EditorStyles.boldLabel);
            }
            int margin = 15;
            float sub =(window != null && window.IsFocusingDuplicate) ? 100 : 40;
            Rect re = new Rect(r.x + r.width - sub, r.y, 20, r.height);
            if(FR2_Setting.ShowUsedByClassed && HashUsedByClassesIds != null)
            {
                foreach(var item in HashUsedByClassesIds)
                {
                    if(!HashClassesNormal.ContainsKey(item)) continue;
                    
                    string name = HashClassesNormal[item];
                    Type t  = null;
                    if(!HashClasses.TryGetValue(item, out t))
                    {
                        t= FR2_Unity.GetType(name);
                        HashClasses.Add(item, t);
                    }
	                //GUIContent content = new GUIContent(EditorGUIUtility.ObjectContent(null, t).image, name);
	                GUIContent content;
	                if (!cacheImage.TryGetValue(name, out content))
	                {
	                	content = new GUIContent(EditorGUIUtility.ObjectContent(null, t).image, name);
	                	cacheImage.Add(name, content);
	                }
	                
                    GUI.Label(re, content);
                    re.x -= margin;
                    // Debug.Log(item);
                }
            }
                

            if (Event.current.type == EventType.Repaint)
            {
                return rw < pathW + nameW ? 32f : 18f;
            }

            return r.height;
        }
        
	   internal static Dictionary<string, GUIContent> cacheImage = new Dictionary<string, GUIContent>();

        private Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }

        internal GenericMenu AddArray(GenericMenu menu, List<string> list, string prefix, string title,
            string emptyTitle, bool showAsset, int max = 10)
        {
            //if (list.Count > 0)
            //{
            //    if (list.Count > max)
            //    {
            //        prefix = string.Format("{0} _{1}/", title, list.Count) + prefix;
            //    }

            //    //for (var i = 0; i < list.Count; i++)
            //    //{
            //    //    var def = list[i];
            //    //    var suffix = showAsset ? "/" + FR2_Cache.Api.FindSymbol(def).assetName : string.Empty;
            //    //    menu.AddItem(new GUIContent(prefix + def + suffix), false, () => OpenScript(def));
            //    //}
            //}
            //else
            {
                menu.AddItem(new GUIContent(emptyTitle), true, null);
            }

            return menu;
        }

        internal void CopyGUID()
        {
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log(guid);
        }

        internal void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = assetName;
            Debug.Log(assetName);
        }

        internal void CopyAssetPath()
        {
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }

        internal void CopyAssetPathFull()
        {
            var fullName = new FileInfo(assetPath).FullName;
            EditorGUIUtility.systemCopyBuffer = fullName;
            Debug.Log(fullName);
        }
                               
        internal void AddToSelection()
        {
            if (!FR2_Selection.IsSelect(guid))
            {
                FR2_Selection.AppendSelection(guid);
            }

            //var list = Selection.objects.ToList();
            //var obj = FR2_Unity.LoadAssetAtPath<Object>(assetPath);
            //if (!list.Contains(obj))
            //{
            //    list.Add(obj);
            //    Selection.objects = list.ToArray();
            //}
        }

        internal void RemoveFromSelection()
        { 
            if (FR2_Selection.IsSelect(guid))
            {
                FR2_Selection.RemoveSelection(guid);
            }                                
        }

        internal void Ping()
        {
            EditorGUIUtility.PingObject(
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object))
            );
        }

        internal void Open()
        {
            AssetDatabase.OpenAsset(
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object))
            );
        }
	    
	    internal void EditPrefab()
	    {
	    	var prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
	    	Object.Instantiate(prefab);
	    }

        //internal void OpenScript(string definition)
        //{
        //    var asset = FR2_Cache.Api.FindSymbol(definition);
        //    if (asset == null) return;

        //    EditorGUIUtility.PingObject(
        //        AssetDatabase.LoadAssetAtPath(asset.assetPath, typeof(Object))
        //        );
        //}

        internal void Reload()
        {
            LoadAssetInfo();
            LoadContent(true);
        }

        // ----------------------------- SERIALIZED UTILS ---------------------------------------

        private static SerializedProperty[] xGetSerializedProperties(Object go, bool processArray)
        {
            var so = new SerializedObject(go);
            so.Update();
            var result = new List<SerializedProperty>();

            var iterator = so.GetIterator();
            while (iterator.NextVisible(true))
            {
                var copy = iterator.Copy();

                if (processArray && iterator.isArray)
                {
                    result.AddRange(xGetSOArray(copy));
                }
                else
                {
                    result.Add(copy);
                }
            }

            return result.ToArray();
        }

        internal static List<SerializedProperty> xGetSOArray(SerializedProperty prop)
        {
            var size = prop.arraySize;
            var result = new List<SerializedProperty>();

            for (var i = 0; i < size; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);

                if (p.isArray)
                {
                    result.AddRange(xGetSOArray(p.Copy()));
                }
                else
                {
                    //Debug.Log(p.name + ":" + p.propertyType + ":" + p.isArray);
                    result.Add(p.Copy());
                }
            }

            return result;
        }

        // ----------------------------- LOAD ASSETS ---------------------------------------

        internal void LoadGameObject(GameObject go)
        {
            var compList = go.GetComponentsInChildren<Component>();
            for (var i = 0; i < compList.Length; i++)
            {
                LoadSerialized(compList[i]);
            }
        }

        internal void LoadSerialized(Object target)
        {
            var props = xGetSerializedProperties(target, true);

            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].propertyType != SerializedPropertyType.ObjectReference) continue;

                var refObj = props[i].objectReferenceValue;
                if (refObj == null) continue;

                var refGUID = AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(refObj)
                    );

                //Debug.Log("Found Reference BinaryAsset <" + assetPath + "> : " + refGUID + ":" + refObj);
                AddUseGUID(refGUID);
            }
        }

        internal void LoadTerrainData(TerrainData terrain)
        {
            #if UNITY_2018_3_OR_NEWER
            var arr0 = terrain.terrainLayers;
            for (var i = 0; i < arr0.Length; i ++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr0[i]);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }
            #endif


            var arr = terrain.detailPrototypes;

            for (var i = 0; i < arr.Length; i ++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr[i].prototypeTexture);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            var arr2 = terrain.treePrototypes;
            for (var i = 0; i < arr2.Length; i ++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr2[i].prefab);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            var arr3 = FR2_Unity.GetTerrainTextureDatas(terrain);
            for (var i = 0; i < arr3.Length; i ++)
            {
                var texs = arr3[i];
                for(int k = 0; k < texs.textures.Length; k++)
                {
                    var tex = texs.textures[k];
                    if(tex == null) continue;

                    var aPath = AssetDatabase.GetAssetPath(tex);
                    if(string.IsNullOrEmpty(aPath)) continue;

                    var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                    if(string.IsNullOrEmpty(refGUID)) continue;

                    AddUseGUID(refGUID);
                }
            }
        }
        private void ClearUseGUIDs()
	    {
#if FR2_DEBUG
		    Debug.Log("ClearUseGUIDs: " + assetPath);
#endif
		    
            UseGUIDs.Clear();
            UseGUIDsList.Clear();
        }
        internal void LoadBinaryAsset()
        {
            ClearUseGUIDs();

            //var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            //for (var i = 0;i < assets.Length; i++){
            //	Debug.Log(i + " : "+ assets[i].name + ":" + assets[i].GetType() + "\n" + 
            //		//EditorUtility.GetAssetPath(assets[i]) + "\n"
            //		assets[i].GetHashCode()
            //	);
            //}

            var assetData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            if (assetData is GameObject)
            {
                type = FR2_AssetType.MODEL;
                LoadGameObject(assetData as GameObject);
            }
            else if (assetData is TerrainData)
            {
                type = FR2_AssetType.TERRAIN;
                LoadTerrainData(assetData as TerrainData);
            }

            //Debug.Log("LoadBinaryAsset :: " + assetData + ":" + type);

            assetData = null;
            FR2_Unity.UnloadUnusedAssets();
        }

        internal void LoadYAML()
        {
            if (!File.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            var text = string.Empty;
            try
            {
                text = File.ReadAllText(assetPath);
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }

#if FR2_DEBUG
	        Debug.Log("LoadYAML: " + assetPath);
#endif
            
	        //if(assetPath.Contains("Myriad Pro - Bold SDF"))
            //{
            //    Debug.Log("no ne");
            //}
	        // PERFORMANCE HOG!
            // var matches = Regex.Matches(text, @"\bguid: [a-f0-9]{32}\b");
            var matches = Regex.Matches(text, @".*guid: [a-f0-9]{32}.*\n");

            foreach (Match match in matches)
            {
                var guidMatch = Regex.Match(match.Value, @"\bguid: [a-f0-9]{32}\b");
                var refGUID = guidMatch.Value.Replace("guid: ", string.Empty);

                var fileIdMatch = Regex.Match(match.Value, @"\bfileID: ([0-9]*).*");
                int id = -1;
                try{
                    id =int.Parse(fileIdMatch.Groups[1].Value) / 100000;
                }
                catch{}
                AddUseGUID(refGUID,id);
            }
	        
	        //var idx = text.IndexOf("guid: ");
			//var counter=0;
	        //while (idx != -1)
	        //{
	        //	var guid = text.Substring(idx + 6, 32);
	        //	if (UseGUIDs.Contains(guid)) continue;
	        //	AddUseGUID(guid);
				
			//	idx += 39;
			//	if (idx > text.Length-40) break;
				
	        //	//Debug.Log(assetName + ":" +  guid);
	        //	idx = text.IndexOf("guid: ", idx + 39);
			//	if (counter++ > 100) break;
	        //}
			
			//if (counter > 100){
			//	Debug.LogWarning("Never finish on " + assetName);
			//}
        }
        internal void LoadYAML2()
        {
            if (!File.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }
            if(m_isAssetFile)
            {
                var s = AssetDatabase.LoadAssetAtPath<FR2_Cache>(assetPath);
                if (s != null) return;
            }

            // var text = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(assetPath)) 
                {
                    while (sr.Peek() >= 0) 
                    {
                        string line = sr.ReadLine();
                        int index = line.IndexOf("guid: ");
                        if(index < 0) continue;
                        string refGUID = line.Substring(index+6, 32);
                        int indexFileId = line.IndexOf("fileID: ");
                        int fileID = -1;
                        if(indexFileId >= 0)
                        {
                            indexFileId += 8;
                            string fileIDStr = line.Substring(indexFileId, line.IndexOf(',', indexFileId) - indexFileId);
                            try{
                                fileID =int.Parse(fileIDStr) / 100000;
                            }
                            catch{}
                        }
                        AddUseGUID(refGUID,fileID);
                    }
                }
                
                #if FR2_DEBUG
	            if (UseGUIDsList.Count > 0)
	            {
	            	Debug.Log(assetPath + ":" + UseGUIDsList.Count);
	            }
	            #endif
	            
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }
        }

        internal void LoadFolder()
        {
            if (!Directory.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            try
            {
                var files = Directory.GetFiles(assetPath);
                var dirs = Directory.GetDirectories(assetPath);

                foreach (var f in files)
                {
	                if (f.EndsWith(".meta", StringComparison.Ordinal)) continue;

                    var fguid = AssetDatabase.AssetPathToGUID(f);
                    if (string.IsNullOrEmpty(fguid)) continue;
                    // AddUseGUID(fguid, true);
                    AddUseGUID(fguid);
                }

                foreach (var d in dirs)
                {
                    var fguid = AssetDatabase.AssetPathToGUID(d);
                    if (string.IsNullOrEmpty(fguid)) continue;
                    // AddUseGUID(fguid, true);
                    AddUseGUID(fguid);
                }
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("LoadFolder() error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
            }
            //Debug.Log("Load Folder :: " + assetName + ":" + type + ":" + UseGUIDs.Count);
        }

        internal void LoadScript()
        {
            ScriptSymbols.Clear();
            ScriptTokens.Clear();

            var text = string.Empty;

            if (!File.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            try
            {
                text = File.ReadAllText(assetPath);
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("LoadScript() error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }

            var idx = -1;
            var l = text.Length;

            int matchIdx;
            int matchCount;
            //bool isSymbol = false;

            //Debug.Log("loading ... " + assetName);
            //string lastKeyword = null;
            //string lastWord = null;
            var currentScope = new List<string>();
            var braceLevel = 0;

            var stMap = new Dictionary<string, string>();

            while (++idx < l)
            {
                var c = text[idx];

                // Skip comments
                if (c == '/' && idx < l - 1)
                {
                    var c1 = text[idx + 1];

                    if (c1 == '/')
                    {
                        //line comment
                        idx++;

                        while (++idx < l)
                        {
                            c1 = text[idx];
                            if (c1 == '\r' || c1 == '\n') break;
                        }
                    }
                    else if (c1 == '*')
                    {
                        //block comment
                        idx++;

                        while (++idx < l)
                        {
                            c1 = text[idx];
                            if (c1 != '*' || idx == l - 1) continue;

                            c1 = text[idx + 1];
                            if (c1 == '/') break;
                        }
                    }
                }

                // Skip strings
                if (c == '"' && idx < l - 2)
                {
                    //var fromIdx = idx;

                    while (++idx < l)
                    {
                        var c1 = text[idx];
                        if (c1 == '"' && text[idx - 1] != '\\') break;
                    }

                    //Debug.Log("Skip string \n" + text.Substring(fromIdx, idx-fromIdx));
                    continue;
                }

                if (c == '{')
                {
                    //if (braceLevel == currentScope.Count && lastWord != null && (lastKeyword == "class" || lastKeyword == "namespace")) {
                    //	currentScope.Add(lastKeyword);
                    //	lastWord = null;

                    //}

                    braceLevel++;
#if FR2_DEBUG_BRACE_LEVEL
				Debug.Log("------->" + braceLevel + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif
                }
                else if (c == '}')
                {
#if FR2_DEBUG_BRACE_LEVEL
				Debug.Log("<--------" + braceLevel + ":" + currentScope[currentScope.Count - 1] + ":" + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif

                    if ((currentScope.Count > 0) && (braceLevel == currentScope.Count))
                    {
#if FR2_DEBUG_BRACE_LEVEL
					Debug.Log("out scope : " + currentScope[currentScope.Count - 1]);
#endif

                        currentScope.RemoveAt(currentScope.Count - 1);
                    }

                    braceLevel--;
                }

                if (!char.IsLetter(c) && c != '_') continue;

                matchIdx = idx;
                matchCount = 1;

                while (++idx < l && char.IsLetterOrDigit(c = text[idx]) || c == '_')
                {
                    matchCount++;
                }

                if (matchIdx > 0 && text[matchIdx - 1] == '.') continue; //skip function / method names
                if (text[matchIdx] == '_')
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; // skip names starts with _
                }

                var word = text.Substring(matchIdx, matchCount);

                //skip using and var
                if ((word == "using") && char.IsWhiteSpace(text[matchIdx + matchCount]))
                {
                    while (idx++ < l - 2)
                    {
                        c = text[idx];
                        if (c == '\n' || c == '\r') break;
                    }

                    //Debug.Log("skip using ... " + text.Substring(matchIdx, idx-matchIdx));
                    //isSymbol = false;
                    continue;
                }

                if (word == "var" && char.IsWhiteSpace(text[matchIdx + matchCount]))
                {
                    while (idx++ < l - 2)
                    {
                        c = text[idx];
                        if (c == ';' || c == '=' || char.IsLetterOrDigit(c)) break;
                    }

                    //Debug.Log("skip var ... " + text.Substring(matchIdx, idx-matchIdx));
                    //isSymbol = false;
                }

	            if (SCRIPT_KEYWORDS.Contains(word))
                {
		            var isSymbol = SCRIPT_SYMBOL.Contains(word);
                    var isScope = word == "namespace" || word == "class";
                    var hasBrace = false;

                    if (isSymbol || isScope)
                    {
                        var fromIdx = idx;
                        var hasChar = false;

                        while (idx++ < l - 2)
                        {
                            c = text[idx];
                            if (c == '{' || c == ':' || c == '<' || (hasChar && char.IsWhiteSpace(c))) break;
                            if (!hasChar) hasChar = char.IsLetter(c) || c == '_';
                        }

                        while (char.IsWhiteSpace(c))
                        {
                            //fast forward to first non-whitespace character
                            idx++;
                            if(idx >= text.Length) break;
                            c = text[idx];
                        }

                        hasBrace = c == '{';

                        var nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

                        if (word == "delegate")
                        {
                            fromIdx = idx;
                            while (idx++ < l - 2)
                            {
                                c = text[idx];
                                if (!char.IsLetterOrDigit(c) && c != '_') break;
                            }
                            nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

//#if FR2_DEBUG_SYMBOL
//						Debug.Log("Delegate detected ! " + nextWord);
//#endif
                        }

                        if (isSymbol)
                        {
                            string symb;
                            var ns = currentScope.Count > 0
                                ? string.Join(".", currentScope.ToArray()) + "."
                                : string.Empty;
                            if (stMap.TryGetValue(nextWord, out symb))
                            {
                                if (symb == "@token")
                                {
                                    stMap[nextWord] = ns + nextWord;
                                }
                                else
                                {
#if FR2_DEV
								Debug.LogWarning("Not yet support same symbol name definitions <" + ns + nextWord + "> : " + symb);
#endif
                                }
                            }
                            else
                            {
                                stMap.Add(nextWord, ns + nextWord);
                            }

//#if FR2_DEBUG_SYMBOL
//						Debug.LogWarning(word + " : " + (ns + nextWord));
//#endif
                        }

                        if (isScope)
                        {
                            currentScope.Add(nextWord);

//#if FR2_DEBUG_SYMBOL
//						Debug.LogWarning("Add scope : " + nextWord + ":" + braceLevel);
//#endif
                        }

                        if (c == ':' && isSymbol)
                        {
                            //extends
                            fromIdx = idx + 1;
                            while (idx++ < l - 2)
                            {
                                c = text[idx];
                                if (c == '{') break;
                            }

                            hasBrace = true;
                            nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

                            if (!stMap.ContainsKey(nextWord))
                            {
                                stMap.Add(nextWord, "@token");
                            }
                        }

                        if (hasBrace)
                        {
                            braceLevel++;
#if FR2_DEBUG_BRACE_LEVEL
						Debug.Log("------->" + braceLevel + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif
                        }
                    }

                    continue;
                }

                if (matchCount < 2)
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; // skip short names
                }

                if (char.ToLower(text[matchIdx]) == text[matchIdx])
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; //starts with lowercase character
                }

                //if (isSymbol){
                //	if (!ScriptSymbols.Contains(word)){
                //		//Debug.Log("Symbol --------- " + word);

                //		ScriptSymbols.Add(word);
                //		if (ScriptTokens.Contains(word)) ScriptTokens.Remove(word);	
                //	}
                //} else 

                if (!stMap.ContainsKey(word))
                {
                    var isFuncCall = idx < l - 2 && text[idx + 1] == '(';
                    if (isFuncCall) continue; //skip funtion calls

                    stMap.Add(word, "@token");
                }

                //isSymbol = false;
            }

            foreach (var item in stMap)
            {
                if (item.Value == "@token")
                {
                    ScriptTokens.Add(item.Key);

#if FR2_DEBUG_SYMBOL
				Debug.Log("Add Token : " + item.Key);
#endif
                }
                else
                {
                    ScriptSymbols.Add(item.Value);

#if FR2_DEBUG_SYMBOL
				Debug.Log("Add Symbol : " + item.Value);
#endif
                }
            }
        }

       // ----------------------------- REPLACE GUIDS ---------------------------------------

        internal bool ReplaceReference(string fromGUID, string toGUID, TerrainData terrain = null)
        {
            if (IsMissing) {return false;}

            if (IsReferencable)
            {
                var text = string.Empty;

                if (!File.Exists(assetPath))
                {
                    state = FR2_AssetState.MISSING;
                    return false;
                }

                try
                {
                    text = File.ReadAllText(assetPath).Replace("\r", "\n");
                    File.WriteAllText(assetPath, text.Replace(fromGUID, toGUID));
                    // AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
                    return true;
                }
                catch (Exception e)
                {
                    state = FR2_AssetState.MISSING; 
//#if FR2_DEBUG
                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + assetPath);
//#endif
                }
                return false;
            }

            if (type == FR2_AssetType.TERRAIN)
            {
                var fromObj = FR2_Unity.LoadAssetWithGUID<Object>(fromGUID);
                var toObj = FR2_Unity.LoadAssetWithGUID<Object>(toGUID);
                var found = 0;
                // var terrain = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)) as TerrainData;

                if (fromObj is Texture2D)
                {
                    var arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D) fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D) toObj;
                        }
                    }
                    terrain.detailPrototypes = arr;
                    FR2_Unity.ReplaceTerrainTextureDatas(terrain, (Texture2D) fromObj, (Texture2D) toObj);
                   
                }

                if (fromObj is GameObject)
                {
                    var arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject) fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject) toObj;
                        }
                    }
                    terrain.treePrototypes = arr2;
                }

                // EditorUtility.SetDirty(terrain);
                // AssetDatabase.SaveAssets();

                fromObj = null;
                toObj = null;
                terrain = null;
                // FR2_Unity.UnloadUnusedAssets();

                return found > 0;
            }

            Debug.LogWarning("Something wrong, should never be here - Ignored <" + assetPath +
                             "> : not a readable type, can not replace ! " + type);
            return false;
        }
        
    }
}