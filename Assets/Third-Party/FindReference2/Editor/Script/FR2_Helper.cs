using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace vietlabs.fr2
{
    public class FR2_Helper
    {
        public static IEnumerable<GameObject> getAllObjsInCurScene()
        {
            // foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
            // {
            //    yield return obj;
            // }

            
            for(int j =0; j < SceneManager.sceneCount; j++)
            {
                Scene scene = SceneManager.GetSceneAt(j);
                foreach(var item in  GetGameObjectsInScene(scene))
                {
                    yield return item;
                }
            }
            
            if(EditorApplication.isPlaying)
            {
            //dont destroy scene
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                Object.DontDestroyOnLoad( temp );
                UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
                Object.DestroyImmediate( temp );
                temp = null;
        
                foreach(var item in  GetGameObjectsInScene(dontDestroyOnLoad))
                {
                    yield return item;
                }
            }
            finally
            {
                if( temp != null )
                    Object.DestroyImmediate( temp );
            }
            }
            
        }
        static IEnumerable<GameObject> GetGameObjectsInScene(Scene scene)
        {
            List<GameObject> rootObjects = new List<GameObject>();
                scene.GetRootGameObjects(rootObjects);

                // iterate root objects and do something
                for (int i = 0; i < rootObjects.Count; ++i)
                {
                    GameObject gameObject = rootObjects[i];

                    foreach (var item in getAllChild(gameObject))
                    {
                        yield return item;
                    }
                    yield return gameObject;
                }
        }
        public static IEnumerable<GameObject> getAllChild(GameObject target)
        {
            if (target.transform.childCount > 0)
            {
                for (int i = 0; i < target.transform.childCount; i++)
                    {
                        yield return target.transform.GetChild(i).gameObject;
                        foreach (var item in getAllChild(target.transform.GetChild(i).gameObject))
                    {
                        yield return item;
                    }

                    }
            }
            
        }

        public static IEnumerable<Object> GetAllRefObjects(GameObject obj)
        {
                Component[] components = obj.GetComponents<Component>();
                foreach (var com in components)
                {
                    if(com == null) continue;
                    SerializedObject serialized = new SerializedObject(com);
                    SerializedProperty it = serialized.GetIterator().Copy();
                    while (it.NextVisible(true))
                    {

                        if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                        if (it.objectReferenceValue == null) continue;
                        yield return it.objectReferenceValue;
                    }
                }
            
        }

        public static int StringMatch(string pattern, string input)
		{
			if (input == pattern) return int.MaxValue;
			if (input.Contains(pattern)) return int.MaxValue-1;
			
			int pidx = 0;
			int score = 0;
			int tokenScore = 0;
			
			for (var i = 0;i < input.Length; i++)
			{
				var ch = input[i];
				if (ch == pattern[pidx])
				{
					tokenScore += tokenScore + 1; //increasing score for continuos token
					pidx++;
					if (pidx >= pattern.Length) break;
				} else {
					tokenScore = 0;
				}
				
				score += tokenScore;
			}
			
			return score;
		}

        static internal readonly AssetType[] FILTERS = new AssetType[]
			{
				new AssetType("Scene",			".unity"),
				new AssetType("Prefab", 		".prefab"),
				new AssetType("Model",			".3df", ".3dm", ".3dmf", ".3dv", ".3dx", ".c5d", ".lwo", ".lws", ".ma", ".mb", ".mesh", ".vrl", ".wrl", ".wrz", ".fbx", ".dae", ".3ds", ".dxf", ".obj", ".skp", ".max", ".blend"),
				new AssetType("Material",		".mat", ".cubemap", ".physicsmaterial"),
				new AssetType("Texture",		".ai", ".apng", ".png", ".bmp", ".cdr", ".dib", ".eps", ".exif", ".ico", ".icon", ".j", ".j2c", ".j2k", ".jas", ".jiff", ".jng", ".jp2", ".jpc", ".jpe", ".jpeg", ".jpf", ".jpg", "jpw", "jpx", "jtf", ".mac", ".omf", ".qif", ".qti", "qtif", ".tex", ".tfw", ".tga", ".tif", ".tiff", ".wmf", ".psd", ".exr", ".rendertexture"),
				new AssetType("Video",			".asf", ".asx", ".avi", ".dat", ".divx", ".dvx", ".mlv", ".m2l", ".m2t", ".m2ts", ".m2v", ".m4e", ".m4v", "mjp", ".mov", ".movie", ".mp21", ".mp4", ".mpe", ".mpeg", ".mpg", ".mpv2", ".ogm", ".qt", ".rm", ".rmvb", ".wmv", ".xvid", ".flv"),
				new AssetType("Audio",			".mp3", ".wav", ".ogg", ".aif", ".aiff", ".mod", ".it", ".s3m", ".xm"),
				new AssetType("Script",			".cs", ".js", ".boo"),
				new AssetType("Text",			".txt", ".json", ".xml", ".bytes", ".sql"),
				new AssetType("Shader",			".shader", ".cginc"),
				new AssetType("Animation",		".anim", ".controller", ".overridecontroller", ".mask"),
				new AssetType("Unity Asset",	".asset", ".guiskin", ".flare", ".fontsettings", ".prefs"),
				new AssetType("Others") 		//
			};
			
			static public int GetIndex(string ext)
			{
				for (var i = 0;i < FILTERS.Length-1; i++)
				{
					if (FILTERS[i].extension.Contains(ext))	return i;
				}
				return FILTERS.Length-1; //Others
			}
            public static void GuiLine( int i_height = 1 )

        {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height );

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );

        }
    }
}