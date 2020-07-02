#if UNITY_2018_3_OR_NEWER
#define SUPPORT_NESTED_PREFAB 
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

using System;
using Object = UnityEngine.Object;
using System.Linq;

#if SUPPORT_NESTED_PREFAB
using UnityEditor.Experimental.SceneManagement; 
#endif


namespace vietlabs.fr2
{
    public class FR2_SceneCache
    {

        public static FR2_SceneCache Api
        {
            get
            {
                if (_api == null)
                {
                    _api = new FR2_SceneCache();
                }
                return _api;
            }

        }

        private static FR2_SceneCache _api;
        private bool dirty = true;
        public bool Dirty
        {
            get { return dirty; }
        }


        public class HashValue{
            public SerializedProperty pro;
            public bool isSceneObject;
            public HashValue(SerializedProperty pro, bool isSceneObject)
            {
                this.pro = pro;
                this.isSceneObject = isSceneObject;
            }
        }
        Dictionary<Component, HashSet<HashValue>> _cache = new Dictionary<Component, HashSet<HashValue>>();
        public Dictionary<Component, HashSet<HashValue>> cache
        {
            get
            {
                if (_cache == null)
                {
                    // ready = true; 
                    refreshCache(window);
                }
                return _cache;
            }
        }
        public Dictionary<string, HashSet<Component>> folderCache = new Dictionary<string, HashSet<Component>>();
        public static Action onReady;
        public static bool ready = true;
        private List<GameObject> listGO;
        public int total;
        public int current;
        private IWindow window;
        public void refreshCache(IWindow window)
        {
            if (window == null) return;

            // if(!ready) return;
            this.window = window;

            _cache = new Dictionary<Component, HashSet<HashValue>>();
            folderCache = new Dictionary<string, HashSet<Component>>();
            ready = false;

            List<GameObject> listRootGO = null;

#if SUPPORT_NESTED_PREFAB

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                
                var rootPrefab = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                if (rootPrefab != null)
                {
                    listRootGO = new List<GameObject> { rootPrefab };
                }
            }

#else
#endif
            if(listRootGO == null)
            {
                listGO = FR2_Unity.getAllObjsInCurScene().ToList();
            }
            else
            {
                listGO = new List<GameObject>();
                foreach(var item in listRootGO)
                {
                    listGO.AddRange(FR2_Unity.getAllChild(item, true)); 
                }
            }

            total = listGO.Count;
            current = 0;
            // Debug.Log("refresh cache total " + total);
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            // foreach (var item in FR2_Helper.getAllObjsInCurScene())
            // {
            //     // Debug.Log("object in scene: " + item.name);
            //     Component[] components = item.GetComponents<Component>();
            //     foreach (var com in components)
            //     {
            //         if(com == null) continue;
            //         SerializedObject serialized = new SerializedObject(com);
            //         SerializedProperty it = serialized.GetIterator().Copy();
            //         while (it.NextVisible(true))
            //         {

            //             if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
            //             if (it.objectReferenceValue == null) continue;

            // 			if(!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<SerializedProperty>());
            // 			if(!_cache[com].Contains(it))
            // 				_cache[com].Add(it.Copy());
            //         }
            //     }
            // }
            dirty = false;
        }

        private void OnUpdate()
        {
            for (int i = 0; i < 5 * FR2_Cache.Api.priority; i++)
            {
                if (listGO == null || listGO.Count <= 0)
                {
                    //done
                    // Debug.Log("done");
                    EditorApplication.update -= OnUpdate;
                    ready = true;
                    dirty = false;
                    listGO = null;
                    if (onReady != null)
                    {
                        onReady();
                    }
                    if (window != null) window.OnSelectionChange();
                    return;
                }
                int index = listGO.Count - 1;

                var go = listGO[index];
                if (go == null) continue;

                Component[] components = go.GetComponents<Component>();

                foreach (var com in components)
                {
                    if (com == null) { continue; }
                    SerializedObject serialized = new SerializedObject(com);
                    SerializedProperty it = serialized.GetIterator().Copy();
                    while (it.NextVisible(true))
                    {

                        if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                        if (it.objectReferenceValue == null) continue;

                        bool isSceneObject = true;
                        string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dir = System.IO.Path.GetDirectoryName(path);
                            if (!string.IsNullOrEmpty(dir))
                            {
                                isSceneObject = false;
                                if (!folderCache.ContainsKey(dir)) folderCache.Add(dir, new HashSet<Component>());
                                if (!folderCache[dir].Contains(com))
                                    folderCache[dir].Add(com);
                            }
                        }
                        if (!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<HashValue>());
                        _cache[com].Add(new HashValue(it.Copy(), isSceneObject));


                        // if (!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<SerializedProperty>());
                        // if (!_cache[com].Contains(it))
                        //     _cache[com].Add(it.Copy());
                        // string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                        
                        // if (string.IsNullOrEmpty(path)) continue;
                        // string dir = System.IO.Path.GetDirectoryName(path);
                        // if (string.IsNullOrEmpty(dir)) continue;
                        // if (!folderCache.ContainsKey(dir)) folderCache.Add(dir, new HashSet<Component>());
                        // if (!folderCache[dir].Contains(com))
                        //     folderCache[dir].Add(com);
                    }
                }
                listGO.RemoveAt(index);
                current++;
            }
        }

        public FR2_SceneCache()
        {

#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged -= OnSceneChanged;
			EditorApplication.hierarchyChanged += OnSceneChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
			EditorApplication.hierarchyWindowChanged += OnSceneChanged;
#endif

#if UNITY_2018_2_OR_NEWER
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#endif

#if UNITY_2017_1_OR_NEWER
            EditorSceneManager.activeSceneChanged -= OnSceneChanged;
            EditorSceneManager.activeSceneChanged += OnSceneChanged;

            EditorSceneManager.sceneLoaded -= OnSceneChanged;
            EditorSceneManager.sceneLoaded += OnSceneChanged;

            Undo.postprocessModifications -= OnModify;
            Undo.postprocessModifications += OnModify;
#endif

#if SUPPORT_NESTED_PREFAB
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += prefabClose;
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += prefabClose;


#endif
        }
#if SUPPORT_NESTED_PREFAB
       
        private void prefabClose(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                FR2_SceneCache.Api.refreshCache(this.window);
                return;
            }

            SetDirty();
        }

        private void prefabOnpen(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                FR2_SceneCache.Api.refreshCache(this.window);
                return;
            }

            SetDirty();
        }
#endif

#if UNITY_2017_1_OR_NEWER
        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            OnSceneChanged();
        }
        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            OnSceneChanged();
        }
#endif
        private void OnSceneChanged()
        {
            if (!Application.isPlaying)
            {
                FR2_SceneCache.Api.refreshCache(this.window);
                return;
            }

            SetDirty();
        }

#if UNITY_2017_1_OR_NEWER
        private UndoPropertyModification[] OnModify(UndoPropertyModification[] modifications)
        {
            for (int i = 0; i < modifications.Length; i++)
            {
                if (modifications[i].currentValue.objectReference != null)
                {
                    SetDirty();
                    break;
                }
            }

            return modifications;
        }
#endif



        public void SetDirty()
        {
            dirty = true;
        }
       
    }
}