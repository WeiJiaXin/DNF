using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace vietlabs.fr2
{
    public class FR2_Export
    {
        [MenuItem("Assets/FR2/Toggle Ignore", false, 19)]
        private static void Ignore()
        {
            Object[] actives = Selection.objects;
            for(int i = 0; i < actives.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(actives[i]);
                if(path.Equals(FR2_Cache.DEFAULT_CACHE_PATH)) continue;
                if(FR2_Setting.IgnoreAsset.Contains(path))
                {
                    FR2_Setting.RemoveIgnore(path);
                }
                else
                {
                    FR2_Setting.AddIgnore(path);
                }
            }
        }
        [MenuItem("Assets/FR2/Copy GUID", false, 20)]
        private static void CopyGUID()
        {
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(Selection.activeObject)
                );
        }

        [MenuItem("Assets/FR2/Export Selection", false, 21)]
        private static void ExportSelection()
        {
            FR2_Unity.ExportSelection();
        }

        [MenuItem("Assets/FR2/Select Dependencies (assets I use)", false, 22)]
        private static void SelectDependencies_wtme()
        {
            SelectDependencies(false);
        }

        [MenuItem("Assets/FR2/Select Dependencies included me", false, 23)]
        private static void SelectDependencies_wme()
        {
            SelectDependencies(true);
        }

        //[MenuItem("Assets/FR2/Select")] 
        [MenuItem("Assets/FR2/Select Used (assets used me)", false, 24)]
        private static void SelectUsed_wtme()
        {
            SelectUsed(false);
        }

        [MenuItem("Assets/FR2/Select Used included me", false, 25)]
        private static void SelectUsed_wme()
        {
            SelectUsed(true);
        }

        [MenuItem("Assets/FR2/Export Dependencies", false, 40)]
        private static void ExportDependencies()
        {
            Selection.objects = GetSelectionDependencies().ToArray();
            FR2_Unity.ExportSelection();
        }

        [MenuItem("Assets/FR2/Export Assets (no scripts)", false, 41)]
        private static void ExportAsset()
        {
            var list = GetSelectionDependencies();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is MonoScript)
                {
                    list.RemoveAt(i);
                }
                //Debug.Log(i + ":" + list[i] + ":" + list[i].GetType());
            }
            Selection.objects = list.ToArray();
            FR2_Unity.ExportSelection();
        }

        //[MenuItem("Assets/FR2/Tools/Merge Duplicates")]

        private static bool isMergeProcessing;
        public static bool IsMergeProcessing{get{return isMergeProcessing;}}
        public static void MergeDuplicate()
        {
            // for (int i = 0; i < Selection.objects.Length; i++)
            // {
            //     Object item = Selection.objects[i];
            //     Debug.Log(item.name);
            // }
            var guid = EditorGUIUtility.systemCopyBuffer;
            // var wat = new System.Diagnostics.Stopwatch();
            // wat.Start();
            //validate clipboard guid
            var gPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(gPath) || !gPath.StartsWith("Assets/"))
            {
                Debug.LogWarning("Invalid guid <" + guid + "> in clipboard, can not replace !");
                return;
            }

            var guids = FR2_Unity.Selection_AssetGUIDs.ToList();
            if (!guids.Contains(guid))
            {
                Debug.LogWarning("Clipboard guid <" + guid +
                                 "> not found in Selection, you may not intentionally replace selection assets by clipboard guid");
                return;
            }

            guids.Remove(guid);
            if (guids.Count == 0)
            {
                Debug.LogWarning("No new asset selected to replace, must select all duplications to replace");
                return;
            }

            var assetList = FR2_Cache.Api.FindAssets(guids.ToArray(), false);
            // var dictAsset = new Dictionary<string, int>();
           
            //replace one by one
            listReplace = new Dictionary<string, ProcessReplaceData>();
            lstThreads = new List<Thread>();
            for (var i = assetList.Count - 1; i >= 0; i--)
            {
                // Debug.Log("FR2 Replace GUID : " + assetList[i].guid + " ---> " + guid + " : " + assetList[i].UsedByMap.Count + " assets updated");
                var fromId = assetList[i].guid;

                var arr = assetList[i].UsedByMap.Values.ToList();
                for (var j = 0; j < arr.Count; j ++)
                {
                    var a = arr[j];
                    if(!listReplace.ContainsKey(a.assetPath))
                    {
                        listReplace.Add(a.assetPath, new ProcessReplaceData());
                    }
                        listReplace[a.assetPath].datas.Add(new ReplaceData()
                    {
                        from = fromId,
                        to = guid,
                        asset = a
                    });
                }
            }
            foreach(var item in listReplace)
            {
                item.Value.processIndex = item.Value.datas.Count -1;
            }
            isMergeProcessing = true;
            EditorApplication.update -= ApplicationUpdate;
            EditorApplication.update += ApplicationUpdate;
            
            // for (var i = assetList.Count - 1; i >= 0; i--)
            // {
            //     // Debug.Log("FR2 Replace GUID : " + assetList[i].guid + " ---> " + guid + " : " + assetList[i].UsedByMap.Count + " assets updated");
            //     var from = assetList[i].guid;

            //     var arr = assetList[i].UsedByMap.Values.ToList();
            //     for (var j = 0; j < arr.Count; j ++)
            //     {
            //         var a = arr[j];
            //         var result = a.ReplaceReference(from, guid);

            //         if (result && !dictAsset.ContainsKey(a.guid))
            //         {
            //             dictAsset.Add(a.guid, 1);
            //         }
            //     }
            // }
            // Debug.Log("Time replace guid " + wat.ElapsedMilliseconds);
            // wat = new System.Diagnostics.Stopwatch();
            // wat.Start();
            // var listRefresh = dictAsset.Keys.ToList();
            // for (var i = 0; i < listRefresh.Count; i++)
            // {
            //     FR2_Cache.Api.RefreshAsset(listRefresh[i], true);
            // }

            // FR2_Cache.Api.RefreshSelection();
            // FR2_Cache.Api.Check4Usage();
            // AssetDatabase.Refresh();
            // Debug.Log("Time replace guid " + wat.ElapsedMilliseconds);
        }
        private class ProcessReplaceData
        {
            public int processIndex;
            public List<ReplaceData> datas = new List<ReplaceData>();
        }
        private class ReplaceData
        {
            public string from;
            public string to;
            public FR2_Asset asset;
            public bool isSucess;
            public bool isTerrian;
            public bool isUpdated;
            public TerrainData terrainData;
        }
        private static Dictionary<string, ProcessReplaceData> listReplace;
        private static List<Thread> lstThreads;
        // private static int processIndex;
        private const int maxThread = 5;
        private static void ApplicationUpdate()
        {
            bool notComplete = listReplace.Where(x=>x.Value.processIndex >= 0).Count() > 0;
            if(lstThreads.Count <= 0 && notComplete)
            {
                //add to thread
                foreach(var item in listReplace)
                {
                    if(item.Value.processIndex >= 0)
                    {
                        var a = item.Value.datas[item.Value.processIndex--];
                        a.isTerrian = a.asset.type == FR2_AssetType.TERRAIN;
                    if(a.isTerrian) 
                        a.terrainData =AssetDatabase.LoadAssetAtPath(a.asset.assetPath, typeof(Object)) as TerrainData;
                    Thread thread = new Thread(
                        delegate(){
                            lock(a)
                            {
                                a.isSucess = a.asset.ReplaceReference(a.from, a.to, a.terrainData);
                                // Debug.Log(a.asset.assetPath+" "+ a.isSucess);
                            }
                            
                        });
                        lstThreads.Add(thread);
                        thread.Start();
                        }
                }
            }
            for(int i = lstThreads.Count -1; i>=0; i--)
            {
                if(!lstThreads[i].IsAlive)
                    lstThreads.RemoveAt(i);
            }
            
            foreach(var item in listReplace)
            {
                    var lst = item.Value.datas;
                    for(int i = 0; i< lst.Count; i++)
                    {
                        ReplaceData data = lst[i];
                        if(!data.isUpdated && data.isSucess )
                        {
                            data.isUpdated = true;
                            if(data.isTerrian)
                            {
                                EditorUtility.SetDirty(data.terrainData);
                                AssetDatabase.SaveAssets();
                                data.terrainData = null;
                                FR2_Unity.UnloadUnusedAssets();
                            }
                            else
                            {
                                AssetDatabase.ImportAsset(data.asset.assetPath, ImportAssetOptions.Default);
                            }
                        }
                    }
                }


            if(lstThreads.Count <= 0 && !notComplete)//complete 
            {
                HashSet<string> guidsRefreshed = new HashSet<string>();
                EditorApplication.update -= ApplicationUpdate;
                foreach(var item in listReplace)
                {
                    var lst = item.Value.datas;
                    for(int i = 0; i< lst.Count; i++)
                    {
                        ReplaceData data = lst[i];
                        if(data.isSucess && !guidsRefreshed.Contains(data.asset.guid))
                        {
                            guidsRefreshed.Add(data.asset.guid);
                            FR2_Cache.Api.RefreshAsset(data.asset.guid, true);
                        }
                    }
                }
                lstThreads = null;
                listReplace = null;
                FR2_Cache.Api.RefreshSelection();
                FR2_Cache.Api.Check4Usage();
                // EditorUtility.SetDirty(FR2_Cache.Api);
                AssetDatabase.Refresh();
                isMergeProcessing = false;
                
            }
        }


        //[MenuItem("Assets/FR2/Tools/Fix Model Import Material")]
        //public static void FixModelImportMaterial(){
        //	if (Selection.activeObject == null) return;
        //	CreatePrefabReplaceModel((GameObject)Selection.activeObject);
        //}

        //[MenuItem("GameObject/FR2/Paste Materials", false, 10)]
        //public static void PasteMaterials(){
        //	if (Selection.activeObject == null) return;

        //	var r = Selection.activeGameObject.GetComponent<Renderer>();
        //	Undo.RecordObject(r, "Replace Materials");
        //	r.materials = model_materials;
        //	EditorUtility.SetDirty(r);
        //}

        //[MenuItem("GameObject/FR2/Copy Materials", false, 10)]
        //public static void CopyMaterials(){
        //	if (Selection.activeObject == null) return;
        //	var r = Selection.activeGameObject.GetComponent<Renderer>();
        //	if (r == null) return;
        //	model_materials = r.sharedMaterials;
        //}

        //-------------------------- APIs ----------------------

        private static void SelectDependencies(bool includeMe)
        {
            var list = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe) AddToDict(dict, list.ToArray());

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, FR2_Asset.FindUsage(list[i]).ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }

        private static void SelectUsed(bool includeMe)
        {
            var list = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe) AddToDict(dict, list.ToArray());

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, list[i].UsedByMap.Values.ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }


        //-------------------------- UTILS ---------------------

        internal static void AddToDict(Dictionary<string, Object> dict, params FR2_Asset[] list)
        {
            for (var j = 0; j < list.Length; j++)
            {
                var guid = list[j].guid;
                if (!dict.ContainsKey(guid))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    dict.Add(guid, FR2_Unity.LoadAssetAtPath<Object>(assetPath));
                }
            }
        }

        private static List<Object> GetSelectionDependencies()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window !");
                return null;
            }
            return FR2_Cache.FindUsage(FR2_Unity.Selection_AssetGUIDs).Select(
                guid =>
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    return FR2_Unity.LoadAssetAtPath<Object>(assetPath);
                }
                ).ToList();
        }

        //	AssetDatabase.ImportAsset(oAssetPath, ImportAssetOptions.Default);
        //	importer.importMaterials = false;
        //	var importer = AssetImporter.GetAtPath(oAssetPath) as ModelImporter;
        //	var nModel = AssetDatabase.LoadAssetAtPath<GameObject>(oAssetPath);

        //	// Reimport model with importMaterial = false
        //	var extension = Path.GetExtension(oAssetPath);

        //	model_materials = model.GetComponent<Renderer>().sharedMaterials;
        //	var oGUID = AssetDatabase.AssetPathToGUID(oAssetPath);

        //	var oAssetPath = AssetDatabase.GetAssetPath(model);
        //	if (model == null) return;
        //{
        //static void CreatePrefabReplaceModel(GameObject model)

        //static Material[] model_materials;

        //	//create prefab from new model
        //	var prefabPath = oAssetPath.Replace(extension, ".prefab");
        //	var clone = (GameObject)Object.Instantiate(nModel);
        //	clone.GetComponent<Renderer>().sharedMaterials = model_materials;
        //	PrefabUtility.CreatePrefab(prefabPath, clone, ReplacePrefabOptions.ReplaceNameBased);
        //	AssetDatabase.SaveAssets();
        //	GameObject.DestroyImmediate(clone);
        //}
    }
}