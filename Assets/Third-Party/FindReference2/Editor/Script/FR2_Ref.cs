using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.IO;

namespace vietlabs.fr2 
{
    public class FR2_SceneRef : FR2_Ref
    {
		public string scenePath = "";
		public string sceneFullPath = "";
		public string targetType;
        public FR2_SceneRef(int index, int depth, FR2_Asset asset, FR2_Asset by) : base(index, depth, asset, by)
        {
			isSceneRef = false;
        }
		public HashSet<string> usingType = new HashSet<string>();
		public FR2_SceneRef(int depth, UnityEngine.Object target) : base(0, depth, null, null)
		{
			this.component = target;
			this.depth = depth;
			isSceneRef = true;
			GameObject obj = target as GameObject;
			if(obj == null)
			{
				Component com = target as Component;
				if(com != null)
				{
					obj = com.gameObject;
				}
			}
			scenePath = FR2_Unity.GetGameObjectPath(obj, false);
			if(component == null) return;
			sceneFullPath = scenePath + component.name;
			targetType = component.GetType().Name;
		}
		public override bool isSelected()
		{
			return component == null? false: FR2_Selection.HScene.Contains(component);
		}
		public void Draw(Rect r, IWindow window)
		{
			var selected = isSelected();
			var margin = 2;
				Rect left = new Rect(r);
				left.width = r.width /3f;
				Rect right = new Rect(r);
					right.x += left.width + margin;
					right.width = r.width *2f/3 -margin;
					
			if (/* FR2_Setting.PingRow && */ Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					var pingRect = FR2_Setting.PingRow 
								?new Rect(0,r.y, r.x + r.width, r.height)
								:left;
					if (pingRect.Contains(Event.current.mousePosition))
					{
						if(Event.current.control || Event.current.command)
						{
							if(selected)
							{
								FR2_Selection.RemoveSelection(this);
								
							}
							else
							{
								FR2_Selection.AppendSelection(this);
							}
                            if (window != null) window.Repaint();
                        }
						EditorGUIUtility.PingObject(component);
						//Event.current.Use();
					}
				}

			
				

				EditorGUI.ObjectField(left, GUIContent.none, component, typeof(GameObject), true);

				
					

					bool drawPath = FR2_Setting.GroupMode != FR2_RefDrawer.Mode.Folder;
					var pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(scenePath)).x : 0;
					string assetName = component.name;
					// if(usingType!= null && usingType.Count > 0)
					// {
					// 	assetName += " -> ";
					// 	foreach(var item in usingType)
					// 	{
					// 		assetName += item + " - ";
					// 	}
					// 	assetName = assetName.Substring(0, assetName.Length - 3);
					// }
					var cc = FR2_Cache.Api.setting.SelectedColor;

					Rect lableRect = new Rect(
						right.x, 
						right.y,
						pathW + EditorStyles.boldLabel.CalcSize(new GUIContent(assetName)).x, 
						right.height);
					if (selected)
					{
						var c = GUI.color;
						GUI.color = cc;
						GUI.DrawTexture(lableRect, EditorGUIUtility.whiteTexture);
						GUI.color = c;
					}
					if (drawPath)
					{
						GUI.Label(LeftRect(pathW, ref right), scenePath, EditorStyles.miniLabel);
						right.xMin -= 4f;
						GUI.Label(right, assetName, EditorStyles.boldLabel);
					}
            else
            {
                GUI.Label(right, assetName);
            }


            if (!FR2_Setting.ShowUsedByClassed || usingType == null) return;
			float sub = 10;
            Rect re = new Rect(r.x + r.width - sub, r.y, 20, r.height);
            Type t = null;
            foreach (var item in usingType)
            {
                var name = item;
                if (!CacheType.TryGetValue(item, out t))
                {
                    t = FR2_Unity.GetType(name);
                    // if (t == null)
                    // {
                    // 	continue;
                    // } 
                    CacheType.Add(item, t);
                }

                GUIContent content;
				var width = 0.0f;
                if (!FR2_Asset.cacheImage.TryGetValue(name, out content))
                {
                    if (t == null)
                    {
                        content = new GUIContent(name);
						
                    }
                    else
                    {
                        var text = EditorGUIUtility.ObjectContent(null, t).image;
                        if (text == null)
                        {
                            content = new GUIContent(name);
                        }
                        else
						{
							 content = new GUIContent(text, name);
						}
                    }


                    FR2_Asset.cacheImage.Add(name, content);
                }

				if(content.image == null)
				{
					width = EditorStyles.label.CalcSize(content).x;
				}
				else width = 20;

                re.x -= width;
                re.width = width;

                GUI.Label(re, content);
                re.x -= margin;// margin;

            }
						
            
	        		// var nameW = EditorStyles.boldLabel.CalcSize(new GUIContent(assetName)).x;
				
		}
		internal static Dictionary<string, Type>  CacheType = new Dictionary<string, Type>();
		private Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }
		// ------------------------- Scene use scene objects
		static public Dictionary<string, FR2_Ref> FindSceneUseSceneObjects(GameObject[] targets)
        {
			Dictionary<string, FR2_Ref> results = new Dictionary<string, FR2_Ref>();
            var objs = Selection.gameObjects;
			for(int i = 0; i < objs.Length; i++)
			{
				if(FR2_Unity.IsInAsset(objs[i])) continue;

				string key = objs[i].GetInstanceID().ToString();
				if(!results.ContainsKey(key))
				{
					results.Add(key, new FR2_SceneRef(0, objs[i]));
				}
				var coms = objs[i].GetComponents<Component>();
				var SceneCache = FR2_SceneCache.Api.cache;
				for(int j = 0; j < coms.Length; j++)
				{
					HashSet<FR2_SceneCache.HashValue> hash = null;
					if(SceneCache.TryGetValue(coms[j], out hash))
					{
						foreach(var item in hash)
						{
							if(item.isSceneObject)
							{
								var obj = item.pro.objectReferenceValue;
								string key1 = obj.GetInstanceID().ToString();
								if(!results.ContainsKey(key1))
								{
									results.Add(key1, new FR2_SceneRef(1, obj));
								}
							}
						}
					}
				}

			}
			return results;
        }

		// ------------------------- Scene in scene
		static public Dictionary<string, FR2_Ref> FindSceneInScene(GameObject[] targets)
        {
			Dictionary<string, FR2_Ref> results = new Dictionary<string, FR2_Ref>();
            var objs = Selection.gameObjects;
			for(int i = 0; i < objs.Length; i++)
			{
				if(FR2_Unity.IsInAsset(objs[i])) continue;

				string key = objs[i].GetInstanceID().ToString();
				if(!results.ContainsKey(key))
				{
					results.Add(key, new FR2_SceneRef(0, objs[i]));
				}
				

				foreach(var item in FR2_SceneCache.Api.cache)
				{
					foreach(var item1 in item.Value)
					{
						// if(item.Key.gameObject.name == "ScenesManager")
						// Debug.Log(item1.objectReferenceValue);
						GameObject ob = null;
						if(item1.pro.objectReferenceValue is GameObject)
						{
							ob = item1.pro.objectReferenceValue as GameObject;
						}
						else
						{
							var com = (item1.pro.objectReferenceValue as Component);
							if(com == null) continue;
							ob = com.gameObject;
						}
						
						if(ob == null) continue;
						if(ob != objs[i]) continue;
						key = item.Key.GetInstanceID().ToString();
						if(!results.ContainsKey(key)) results.Add(key, new FR2_SceneRef(1, item.Key));
						(results[key] as FR2_SceneRef).usingType.Add(item1.pro.objectReferenceValue.GetType().FullName);
					}
				}
			}
			return results;
        }


		// ------------------------- Ref in scene
		static Action<Dictionary<string, FR2_Ref>> onFindRefInSceneComplete;
		static Dictionary<string, FR2_Ref> refs = new Dictionary<string, FR2_Ref>();
		static string[] cacheAssetGuids;
        public static IWindow window { get; set; }

        static public Dictionary<string, FR2_Ref> FindRefInScene(string[] assetGUIDs, bool depth, Action<Dictionary<string, FR2_Ref>> onComplete, IWindow win) 
		{
            // var watch = new System.Diagnostics.Stopwatch();
            // watch.Start();
            window = win;
            cacheAssetGuids = assetGUIDs;
			onFindRefInSceneComplete = onComplete;
			if(FR2_SceneCache.ready)
			{
				FindRefInScene();
			}
			else
			{
				FR2_SceneCache.onReady -= FindRefInScene;
				FR2_SceneCache.onReady += FindRefInScene;
			}
			return refs;
		}

        private static void FindRefInScene()
        {
			 refs = new Dictionary<string, FR2_Ref>();
            for(int i = 0; i < cacheAssetGuids.Length; i++)
			{
				FR2_Asset asset = FR2_Cache.Api.Get(cacheAssetGuids[i]);
	            if(asset == null) continue;
				Add(refs, asset, 0);

	            ApplyFilter(refs, asset);
			}
			if(onFindRefInSceneComplete!= null)
			{
				onFindRefInSceneComplete(refs);
			}
			FR2_SceneCache.onReady -= FindRefInScene;
        //    UnityEngine.Debug.Log("Time find ref in scene " + watch.ElapsedMilliseconds);
        }
		private static void FilterAll(Dictionary<string, FR2_Ref> refs, Object obj, string targetPath)
        {
            // ApplyFilter(refs, obj, targetPath);
        }

        private static void ApplyFilter(Dictionary<string, FR2_Ref> refs, FR2_Asset asset)
        {
			var targetPath = AssetDatabase.GUIDToAssetPath(asset.guid);
			if (string.IsNullOrEmpty(targetPath)) return; // asset not found - might be deleted!
	            
	            //asset being moved!
	            if (targetPath != asset.assetPath)
	            {
	            	//Debug.LogWarning("assetPathChagned: \n" + path + "\n" + asset.assetPath);
	            	asset.LoadAssetInfo();
	            }

			var target = AssetDatabase.LoadAssetAtPath(targetPath, typeof(Object));
            if (target == null) 
			{ 
				Debug.LogWarning("target is null"); 
				return; 
			}

            bool targetIsGameobject = target is GameObject;

			if(targetIsGameobject)
			{
				foreach (var item in FR2_Unity.getAllObjsInCurScene())
				{
					if (FR2_Unity.CheckIsPrefab(item))
					{
						var itemGUID = FR2_Unity.GetPrefabParent(item);
						// Debug.Log(item.name + " itemGUID: " + itemGUID);
						// Debug.Log(target.name + " asset.guid: " + asset.guid);
						if (itemGUID == asset.guid)
						{
							Add(refs,item, 1);
						}
					}
				}
			}
			string dir = System.IO.Path.GetDirectoryName(targetPath);
            if(FR2_SceneCache.Api.folderCache.ContainsKey(dir))
			{
				foreach(var item in FR2_SceneCache.Api.folderCache[dir])
				{
					if(FR2_SceneCache.Api.cache.ContainsKey(item))
					{
						foreach(var item1 in FR2_SceneCache.Api.cache[item])
						{
								if (targetPath == AssetDatabase.GetAssetPath(item1.pro.objectReferenceValue)) 
									Add(refs, item, 1);
						}
					}
					
				}	
			}
        }
		private static void Add(Dictionary<string, FR2_Ref> refs, FR2_Asset asset, int depth)
        {
			string targetId = asset.guid;
            if (!refs.ContainsKey(targetId))
            {
                refs.Add(targetId, new FR2_Ref(0, depth, asset, null));
            }
        }
        private static void Add(Dictionary<string, FR2_Ref> refs, Object target, int depth)
        {
			string targetId = target.GetInstanceID().ToString();
            if (!refs.ContainsKey(targetId))
            {
                refs.Add(targetId, new FR2_SceneRef(depth, target));
            }
        }
        
    }
    public class FR2_Ref
	{
		public int index;
		public int type;
		public int depth;
		public int matchingScore;
		public FR2_Asset asset;
		public FR2_Asset addBy;

		public bool isSceneRef;
		public UnityEngine.Object component;
		public string group;
		public string GetSceneObjId()
		{
			if(component == null) return string.Empty;
			return component.GetInstanceID().ToString();
		}
		
		public FR2_Ref(int index, int depth, FR2_Asset asset, FR2_Asset by)
		{
			this.index = index;
			this.depth = depth;
			
			this.asset = asset;
			if(asset != null)
				type = AssetType.GetIndex(asset.extension);
			addBy = by;
			// isSceneRef = false;
		}
		public FR2_Ref(int index, int depth, FR2_Asset asset, FR2_Asset by, string group):this(index, depth, asset, by)
		{
			this.group = group;
			// isSceneRef = false;
		}
		public virtual bool isSelected()
		{
			return FR2_Selection.IsSelect(asset.guid);
		}
		

		// public FR2_Ref(int depth, UnityEngine.Object target)
		// {
		// 	this.component = target;
		// 	this.depth = depth;
		// 	// isSceneRef = true;
		// }
		internal List<FR2_Ref> Append(Dictionary<string, FR2_Ref> dict, params string[] guidList)
		{
			var result = new List<FR2_Ref>();
			
			if (FR2_Cache.Api.disabled)
			{
				//Debug.LogWarning("Cache is disabled!");
				return result;
			}
			
			if (!FR2_Cache.isReady) 
			{
				Debug.LogWarning("Cache not yet ready! Please wait!");
				return result;
			}
			
			//filter to remove items that already in dictionary
			for (var i = 0;i <guidList.Length;i ++)
			{
				var guid = guidList[i];
				if (dict.ContainsKey(guid)) continue;
				
				var child = FR2_Cache.Api.Get(guid);
				if (child == null) continue;
				
				var r = new FR2_Ref(dict.Count, depth + 1, child, asset);
				if (!asset.IsFolder) dict.Add(guid, r);
				result.Add(r);
			}
			
			return result;
		}
		
		internal void AppendUsedBy(Dictionary<string, FR2_Ref> result, bool deep)
		{
			// var list = Append(result, FR2_Asset.FindUsedByGUIDs(asset).ToArray());
			// if (!deep) return;
			
			// // Add next-level
			// for (var i = 0;i < list.Count;i ++)
			// {
			// 	list[i].AppendUsedBy(result, true);
			// }

			var h = asset.UsedByMap;
			var list = deep ? new List<FR2_Ref>() : null;

			foreach (var kvp in h)
			{
				var guid = kvp.Key;
				if (result.ContainsKey(guid)) continue;

				var child = FR2_Cache.Api.Get(guid);
				if (child == null) continue;
				if (child.IsMissing) continue;
				
				var r = new FR2_Ref(result.Count, depth + 1, child, asset);
				if (!asset.IsFolder) result.Add(guid, r);
				if (deep) list.Add(r);
			}

			if (!deep) return;

			foreach (var item in list)
			{
				item.AppendUsedBy(result, true);
			}
		}
		
		internal void AppendUsage(Dictionary<string, FR2_Ref> result, bool deep)
		{	
			var h = asset.UseGUIDs;
			var list = deep ? new List<FR2_Ref>() : null;

			foreach (var kvp in h)
			{
				var guid = kvp.Key;
				if (result.ContainsKey(guid)) continue;

				var child = FR2_Cache.Api.Get(guid);
				if (child == null) continue;
				if (child.IsMissing) continue;
				
				var r = new FR2_Ref(result.Count, depth + 1, child, asset);
				if (!asset.IsFolder) result.Add(guid, r);
				if (deep) list.Add(r);
			}

			if (!deep) return;

			foreach (var item in list)
			{
				item.AppendUsage(result, true);
			}
		}
		
		// --------------------- STATIC UTILS -----------------------
		
		internal static Dictionary<string, FR2_Ref> FindRefs(string[] guids, bool usageOrUsedBy, bool addFolder)
		{
			var dict = new Dictionary<string, FR2_Ref>();
			var list = new List<FR2_Ref>();
			
			for (var i = 0;i < guids.Length;i ++)
			{
				var guid = guids[i];
				if (dict.ContainsKey(guid))	continue;
				
				var asset = FR2_Cache.Api.Get(guid);
				if (asset == null) continue;
				
				var r = new FR2_Ref(i, 0, asset, null);
				if (!asset.IsFolder || addFolder) dict.Add(guid, r);
				list.Add(r);
			}
			
			for (var i = 0;i < list.Count; i++)
			{
				if (usageOrUsedBy)
				{
					list[i].AppendUsage(dict, true);
				} else {
					list[i].AppendUsedBy(dict, true);
				}
			}
			
			//var result = dict.Values.ToList();
			//result.Sort((item1, item2)=>{
			//	return item1.index.CompareTo(item2.index);
			//});
			
			return dict;
		}
		
		
		static public Dictionary<string, FR2_Ref> FindUsage(string[] guids)	{ return FindRefs(guids, true, true); }
		static public Dictionary<string, FR2_Ref> FindUsedBy(string[] guids)	{ return FindRefs(guids, false, true); }
		static public Dictionary<string, FR2_Ref> FindUsageScene(GameObject[] objs, bool depth) 
		{
			var dict = new Dictionary<string, FR2_Ref>();
			// var list = new List<FR2_Ref>();

			for(int i = 0; i < objs.Length; i++)
			{
				if (FR2_Unity.IsInAsset(objs[i])) continue;//only get in scene 
				
				//add selection
				if(!dict.ContainsKey(objs[i].GetInstanceID().ToString()))
				{
					dict.Add(objs[i].GetInstanceID().ToString(), new FR2_SceneRef(0, objs[i]));
				}
				
                foreach	(var item in FR2_Unity.GetAllRefObjects(objs[i]))
				{
					AppendUsageScene(dict, item);
				}
				
				if(depth)
				{
					foreach(var child in FR2_Unity.getAllChild(objs[i]))
					{
						foreach	(var item2 in FR2_Unity.GetAllRefObjects(child))
						{
							AppendUsageScene(dict, item2);
						}
					}
				}
			}
			return dict;
		}

		private static void AppendUsageScene(Dictionary<string, FR2_Ref> dict, UnityEngine.Object obj)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			if(string.IsNullOrEmpty(path)) return;
			string guid = AssetDatabase.AssetPathToGUID(path);
			if(string.IsNullOrEmpty(guid)) return;

			if (dict.ContainsKey(guid))	return;
		
			var asset = FR2_Cache.Api.Get(guid);
			if (asset == null) return;
			var r = new FR2_Ref(0, 1, asset, null);
			dict.Add(guid, r);
		}
	}
	
	
	public class FR2_RefDrawer : IRefDraw
    {
		
		public enum Mode 
		{
			Dependency,
			Type,	
			Extension,
			Folder, 	
			None
		}
		
		public enum Sort 
		{
			Type,
			Path
		}
		
		
		// ORIGINAL
		FR2_Asset[] source;
		Dictionary<string, FR2_Ref> refs;
		
		// FILTERING
		//static Sort sort;
		//static Mode mode;
		//static HashSet<int> excludes = new HashSet<int>();
		// static string searchTerm = string.Empty;
		string searchTerm = string.Empty;

		FR2_TreeUI2.GroupDrawer groupDrawer;
        public IWindow window { get; set; }
		List<FR2_Ref> list;
		
		public bool isDrawRefreshSceneCache;
		public string Lable;

		// STATUS
		bool dirty;
		bool caseSensitive;
		bool selectFilter;
		bool showSearch = true;
		
		int excludeCount;
		
		public FR2_RefDrawer(IWindow window)
		{
            this.window = window;
			groupDrawer = new FR2_TreeUI2.GroupDrawer(DrawGroup, DrawAsset);
		}
		
		void DrawGroup(Rect r, string label, int childCount)
		{
            if (FR2_Setting.GroupMode == Mode.Folder)
			{
				var tex = AssetDatabase.GetCachedIcon("Assets");
				GUI.DrawTexture(new Rect(r.x-2f, r.y-2f, 16f, 16f), tex);
				r.xMin += 16f;
			}
			
			GUI.Label(r, label + " (" + childCount + ")", EditorStyles.boldLabel);


		    var hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

		    if (hasMouse && Event.current.button == 1)
		    {
		        var menu = new GenericMenu();
		        menu.AddItem(new GUIContent("Select"), false, () =>
		        {
		            var ids = groupDrawer.GetChildren(label);
					FR2_Selection.ClearSelection();
					for (var i = 0; i < ids.Length; i++)
		            {
		                FR2_Ref rf;
						if (!refs.TryGetValue(ids[i], out rf)) continue;

		                FR2_Selection.AppendSelection(rf);
                    }  
					
						
                    // FR2_Selection.h.Clear();
		            // for (var i = 0; i < ids.Length; i++)
		            // {
		            //     FR2_Selection.h.Add(ids[i]);
                    // }                         
		        });
		        menu.AddItem(new GUIContent("Append Selection"), false, () =>
		        {
		            var ids = groupDrawer.GetChildren(label); 
		            for (var i = 0; i < ids.Length; i++)
		            {
						FR2_Ref rf;
						if (!refs.TryGetValue(ids[i], out rf)) continue;

		                FR2_Selection.AppendSelection(rf);
		            }
                });
		        menu.AddItem(new GUIContent("Remove From Selection"), false, () =>
		        {
		            var ids = groupDrawer.GetChildren(label);
		            for (var i = 0; i < ids.Length; i++)
		            {
						FR2_Ref rf;
						if (!refs.TryGetValue(ids[i], out rf)) continue;

						FR2_Selection.RemoveSelection(rf);
		                // if (FR2_Selection.h.Contains(ids[i]))
		                // {
		                //     FR2_Selection.h.Remove(ids[i]);
		                // }
		            }
                });

		        menu.ShowAsContext();
		        Event.current.Use();
		    }
        }

		
		void DrawAsset(Rect r, string guid)
		{
            FR2_Ref rf;
			if (!refs.TryGetValue(guid, out rf)) return;
			
			if (rf.depth == 1) //mode != Mode.Dependency && 
			{
				var c = GUI.color;
				GUI.color = Color.blue;
				GUI.DrawTexture(new Rect(r.x-4f, r.y + 2f, 2f, 2f), EditorGUIUtility.whiteTexture);
				GUI.color = c;
			}
			if(rf.isSceneRef)
			{
				if(rf.component == null) return;
				FR2_SceneRef re = rf as FR2_SceneRef;
				if(re != null)
				{
					re.Draw(r, window);
				}
				
			}
			else
			{
				rf.asset.Draw(r, false, FR2_Setting.GroupMode != Mode.Folder, window);
			}

			
		}
		
		string GetGroup(FR2_Ref rf)
		{
			// Debug.Log(Lable + "  " + FR2_Setting.GroupMode);
			if (FR2_Setting.GroupMode == Mode.None) return string.Empty;
			
			if (rf.depth == 0) return "Selection";
			FR2_SceneRef sr = null;
			if(rf.isSceneRef)
			{
				sr = rf as FR2_SceneRef;
				if(sr == null) return string.Empty;
			}
			
			switch (FR2_Setting.GroupMode)
			{
				case Mode.Extension : return rf.isSceneRef  ? sr.targetType : rf.asset.extension;
				case Mode.Type : 
				{
					return rf.isSceneRef  ? sr.targetType : AssetType.FILTERS[rf.type].name;	
				}
				
				case Mode.Folder : return rf.isSceneRef ? sr.scenePath : rf.asset.assetFolder;
				
				case Mode.Dependency : 
				{
					return rf.depth == 1 ? "Direct Usage" : "Indirect Usage";
				}
			}

            return string.Empty;
		}
		
		void SortGroup(List<string> groups)
		{
			groups.Sort( (item1, item2) =>
			{
				if (item1 == "Others" || item2 == "Selection") return 1;
				if (item2 == "Others" || item1 == "Selection") return -1;
				return item1.CompareTo(item2);
			});
		}
		
		public FR2_RefDrawer Reset(string[] assetGUIDs, bool isUsage)
		{
			//Debug.Log("Reset :: " + assetGUIDs.Length + "\n" + string.Join("\n", assetGUIDs));
			
			if (isUsage)
			{
				refs = FR2_Ref.FindUsage(assetGUIDs);	
			} else {
				refs = FR2_Ref.FindUsedBy(assetGUIDs);	
			}
			
			//RefreshFolders();
			
			// Remove folders && items in assetGUIDs
			//var map = new Dictionary<string, int>();
			//for (var i = 0;i < assetGUIDs.Length; i++)
			//{
			//	map.Add(assetGUIDs[i], i);	
			//}
			
			//for (var i = refs.Count-1; i>=0; i--)
			//{
			//	var a = refs[i].asset;
			//	if (!a.IsFolder) continue; // && !map.ContainsKey(refs[i].asset.guid)
			//	refs.RemoveAt(i); //Remove folders and items in Selection
			//}
			
			dirty = true;
			if (list != null) list.Clear();
			return this;
		}
		public FR2_RefDrawer Reset(GameObject[] objs, bool findDept, bool findPrefabInAsset)
		{
			refs = FR2_Ref.FindUsageScene(objs,findDept);

			if (findPrefabInAsset)
			{
				var guids = new List<string>();
				for (int i = 0; i < objs.Length; i++)
				{
					var guid = FR2_Unity.GetPrefabParent(objs[i]);
					if (string.IsNullOrEmpty(guid)) continue;

					guids.Add(guid);
				}

				var usageRefs = FR2_Ref.FindUsage(guids.ToArray());
				foreach (var kvp in usageRefs)
				{
					if (refs.ContainsKey(kvp.Key)) continue;
					if (guids.Contains(kvp.Key))
					{
						kvp.Value.depth = 1;
					}

					refs.Add(kvp.Key, kvp.Value);
				}
			}
			
			dirty = true;
			if (list != null) list.Clear();
			return this;
		}

		//ref in scene
		public FR2_RefDrawer Reset(string[] assetGUIDs, IWindow window)
		{
			refs = FR2_SceneRef.FindRefInScene(assetGUIDs, true, SetRefInScene, window);	
			dirty = true;
			if (list != null) list.Clear();
			return this;
		}
		private void SetRefInScene(Dictionary<string, FR2_Ref> data)
		{
			refs = data;
			dirty = true;
			if (list != null) list.Clear();
		}
		//scene in scene
		public FR2_RefDrawer ResetSceneInScene(GameObject[] objs)
		{
			refs = FR2_SceneRef.FindSceneInScene(objs);	
			dirty = true;
			if (list != null) list.Clear();
			return this;
		}

		public FR2_RefDrawer ResetSceneUseSceneObjects(GameObject[] objs)
		{
			refs = FR2_SceneRef.FindSceneUseSceneObjects(objs);	
			dirty = true;
			if (list != null) list.Clear();
			return this;
		}

        public FR2_RefDrawer ResetUnusedAsset()
        {
            var lst = FR2_Cache.Api.ScanUnused();

            refs = lst.ToDictionary(x => x.guid, x => new FR2_Ref(0,1,x,null));
            dirty = true;
            if (list != null) list.Clear();
            return this;
        }

        public void RefreshSort()
		{
			if(list == null) return;
			list.Sort((r1, r2)=>
			{
				if(r1.isSceneRef && r2.isSceneRef)
				{
					FR2_SceneRef rs1 = r1 as FR2_SceneRef;
					FR2_SceneRef rs2 = r2 as FR2_SceneRef;
					if(rs1 != null && rs2 != null)
					{
						return SortAsset(rs1.sceneFullPath, rs2.sceneFullPath,
						rs1.targetType, rs2.targetType,
						FR2_Setting.SortMode == Sort.Path);
					}
					
				}
				if(r1.isSceneRef) return 0;
				if(r2.isSceneRef) return 1;
				int v = string.IsNullOrEmpty(searchTerm) ? 0 : r2.matchingScore.CompareTo(r1.matchingScore);
				if (v != 0) return v;
				
				return SortAsset(
					r1.asset.assetPath, r2.asset.assetPath,
					r1.asset.extension, r2.asset.extension,
					FR2_Setting.SortMode == Sort.Path
				);
			});
			
			//folderDrawer.GroupByAssetType(list);
			groupDrawer.Reset<FR2_Ref>(list, 
			rf => rf.isSceneRef ? rf.GetSceneObjId() : rf.asset.guid
			, GetGroup, SortGroup);
		}
		
		public bool isExclueAnyItem()
		{
			return excludeCount > 0;
		}
		
		void ApplyFilter()
		{
			dirty = false;
			
			if (refs == null) return;
			
			if (list == null)
			{
				list = new List<FR2_Ref>();
			} else {
				list.Clear();	
			}
			
			var minScore = searchTerm.Length;
			
			var term1 = searchTerm;
			if (!caseSensitive) term1 = term1.ToLower();
			var term2 = term1.Replace(" ", string.Empty);
			
			excludeCount = 0;
			
			foreach (var item in refs)
			{
				var r = item.Value;
				
				if (r.depth == 0 && !FR2_Setting.ShowSelection) continue;
				if (FR2_Setting.IsTypeExcluded(r.type)) 
				{
					excludeCount++;
					continue; //skip this one
				}
				
				if (!showSearch || string.IsNullOrEmpty(searchTerm))
				{
					r.matchingScore = 0;
					list.Add(r);
					continue;
				}
				
				//calculate matching score
				var name1 = r.isSceneRef ? (r as FR2_SceneRef).sceneFullPath : r.asset.assetName;
				if (!caseSensitive) name1 = name1.ToLower();
				var name2 = name1.Replace(" ", string.Empty);
				
				var score1 = FR2_Unity.StringMatch(term1, name1);
				var score2 = FR2_Unity.StringMatch(term2, name2);
				
				r.matchingScore = Mathf.Max(score1, score2);
				if (r.matchingScore > minScore) list.Add(r);
			}
			
			RefreshSort();
		}
		public void SetDirty()
		{
			dirty = true;
		}
		int SortAsset(string term11, string term12, string term21, string term22, bool swap)
		{	
			if (string.IsNullOrEmpty(term11)) return -1;
			if (string.IsNullOrEmpty(term22)) return 1;
			
			var v1 = term11.CompareTo(term12);
			var v2 = term21.CompareTo(term22);
			return swap ? (v1 == 0 ? v2 : v1) : (v2 == 0 ? v1 : v2);
		}
		
		public static GUIStyle toolbarSearchField;
		public static GUIStyle toolbarSearchFieldCancelButton;
		public static GUIStyle toolbarSearchFieldCancelButtonEmpty;
		public static void InitSearchStyle()
		{
					toolbarSearchField = "ToolbarSeachTextFieldPopup";
					toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
					toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
		}
		private bool showContent = true;
		private bool showIgnore;
		// public void Draw(string searchLable ="", bool showRefreshSceneCache = false)
		public bool Draw()
		{
			if (refs == null || refs.Count == 0) return false;
			
			if (dirty || list == null) ApplyFilter();
			// if (showSearch)
				DrawSearch(Lable,isDrawRefreshSceneCache);
				// DrawSearch(searchLable,showRefreshSceneCache);

			if(!showContent) return false;
           
			groupDrawer.Draw();
			return false;
			
			
		}
		private void DrawSearch(string searchLable ="", bool showRefreshSceneCache = false)
		{
			if (toolbarSearchField == null) {
					InitSearchStyle();
				}
				if(showRefreshSceneCache && !FR2_SceneCache.ready)
				{
					var rect = GUILayoutUtility.GetRect(1, Screen.width, 18f, 18f);
					int cur = FR2_SceneCache.Api.current, total = FR2_SceneCache.Api.total;
					EditorGUI.ProgressBar(rect, cur *1f / total, string.Format("{0} / {1}", cur, total));
				}
				
				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				{
					if(!string.IsNullOrEmpty(searchLable))
					{
						if(showContent) GUILayout.BeginHorizontal(GUILayout.Width(120f));
						{
							showContent = EditorGUILayout.Foldout(showContent, searchLable);	
						}
						if(showContent) GUILayout.EndHorizontal();
						
					}
					if(!showContent)
					{
						GUILayout.EndHorizontal();
						return;
					} 
					var v = GUILayout.Toggle(caseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(24f));
					if (v != caseSensitive)
					{
						caseSensitive = v;
						dirty = true;
					}
					
					GUILayout.Space(2f);
					var value = GUILayout.TextField(searchTerm, toolbarSearchField);	
					if (searchTerm != value)
					{
						searchTerm = value;	
						dirty = true;
					}
					
					var style = string.IsNullOrEmpty(searchTerm) ? toolbarSearchFieldCancelButtonEmpty : toolbarSearchFieldCancelButton;
					if (GUILayout.Button("Cancel", style))
					{
						searchTerm = string.Empty;
						dirty = true;
					}
					GUILayout.Space(2f);
					var width = 24;
					if(showRefreshSceneCache)
					{
						Color col = GUI.color;
						if(FR2_SceneCache.Api.Dirty)
						{
							GUI.color = new Color32(255,0,0, 100);
						}
						
						var color = GUI.contentColor;
                		GUI.contentColor = Color.black;

						if(GUILayout.Button(FR2_Window.Icon.icons.Refresh, EditorStyles.toolbarButton, GUILayout.Width(width)))
						{
							FR2_Asset.lastRefreshTS = Time.realtimeSinceStartup;
							FR2_SceneCache.Api.refreshCache(window);
						}

						GUI.contentColor = color;

						if(FR2_SceneCache.Api.Dirty)
						{
							GUI.color = col;
						}
					}
					// else
					// {
					// 	var color = GUI.contentColor;
                	// 	GUI.contentColor = Color.black;

					// 	if(GUILayout.Button(FR2_Window.Icon.icons.Refresh, EditorStyles.toolbarButton, GUILayout.Width(width)))
					// 	{
					// 		 FR2_Cache.Api.Check4Changes(true, true);
                	// 		FR2_SceneCache.Api.SetDirty();
					// 	}

					// 	GUI.contentColor = color;
					// }
				}
				GUILayout.EndHorizontal();
		}
		public Dictionary<string, FR2_Ref> getRefs()
		{
			return refs;
		}

	    public int ElementCount()
	    {
	        if (refs == null) return 0;
	        return refs.Count;
	        // return refs.Where(x => x.Value.depth != 0).Count();
	    }
	}
}

