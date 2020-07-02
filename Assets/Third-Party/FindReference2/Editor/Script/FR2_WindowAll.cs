using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{

    // filter, ignore anh huong ket qua thi hien mau do
    // optimize lag duplicate khi use
    public class FR2_WindowAll : FR2_WindowBase, IHasCustomMenu
    {
        public override bool ShowScene { get { return false; } }

        [MenuItem("Window/Find Reference 2")]
        private static void ShowWindow()
        {
            var _window = CreateInstance<FR2_WindowAll>();
            _window.InitIfNeeded();
            FR2_Unity.SetWindowTitle(_window, "FR2 Assets");
            _window.Show();
        }




        [NonSerialized] internal FR2_DuplicateTree2 Duplicated;
        [NonSerialized] internal FR2_RefDrawer RefUnUse;
        [NonSerialized] internal FR2_RefDrawer UsesDrawer;
        [NonSerialized] internal FR2_RefDrawer UsedByDrawer;
        [NonSerialized] internal FR2_RefDrawer SceneToAssetDrawer;


        [NonSerialized] internal FR2_RefDrawer RefInScene;
        [NonSerialized] internal FR2_RefDrawer SceneUsesDrawer; //scene use another scene objects
        [NonSerialized] internal FR2_RefDrawer RefSceneInScene;

        internal int level;
        private Vector2 scrollPos;
        private string tempGUID;
        private Object tempObject;












        void OnEnable()
        {
            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;
            Repaint();
        }

        protected override void InitIfNeeded()
        {
            {
                if (UsesDrawer != null) return;
                UsesDrawer = new FR2_RefDrawer(this)
                {
                    Lable = "Assets"
                };
                UsedByDrawer = new FR2_RefDrawer(this)
                {
                    Lable = "Assets"
                };
                Duplicated = new FR2_DuplicateTree2(this);
                SceneToAssetDrawer = new FR2_RefDrawer(this)
                {
                    Lable = "Assets",
                    isDrawRefreshSceneCache = false
                };
                RefUnUse = new FR2_RefDrawer(this)
                {
                    Lable = "Unsed Asset",
                    isDrawRefreshSceneCache = false
                };
            }

            if (SceneUsesDrawer != null) return;
            SceneUsesDrawer = new FR2_RefDrawer(this)
            {
                Lable = "Scene Objects",
                isDrawRefreshSceneCache = true
            };
            RefInScene = new FR2_RefDrawer(this)
            {
                Lable = "Scene Objects",
                isDrawRefreshSceneCache = true
            };
            RefSceneInScene = new FR2_RefDrawer(this)
            {
                Lable = "Scene Objects",
                isDrawRefreshSceneCache = true
            };

            lockSelection = false;

#if UNITY_2018_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#elif UNITY_2017_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged += OnSceneChanged;
#endif

            FR2_Cache.onReady -= OnReady;
            FR2_Cache.onReady += OnReady;

            FR2_Setting.OnIgnoreChange -= OnSelectionChange;
            FR2_Setting.OnIgnoreChange += OnSelectionChange;

            Repaint(); 
        }
        
#if UNITY_2018_OR_NEWER
        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            if (IsFocusingFindInScene || IsFocusingSceneToAsset || IsFocusingSceneInScene)
            {
                OnSelectionChange();
            }
        }
#endif
        protected override void OnReady()
        {
            OnSelectionChange();
            if (IsFocusingUnused)
            {
                RefUnUse.ResetUnusedAsset();
            }
        }

        public override void OnSelectionChange()
        {
            Repaint();

            isNoticeIgnore = false;
            if (!FR2_Cache.isReady) return;
            if (lockSelection) return;
            if (focusedWindow == null) return;

            if (SceneUsesDrawer == null) InitIfNeeded();
            if (UsesDrawer == null) InitIfNeeded();

            ids = FR2_Unity.Selection_AssetGUIDs;

            //ignore selection on asset when selected any object in scene
            if (Selection.gameObjects.Length > 0 && !FR2_Unity.IsInAsset(Selection.gameObjects[0]))
            {
                ids = new string[0];
            }

            level = 0;
            if (IsFocusingSceneToAsset)
            {
                SceneToAssetDrawer.Reset(Selection.gameObjects, true, true);
            }

            if (IsFocusingUses)
            {
                UsesDrawer.Reset(ids, true);
            }
            if (IsFocusingUsedBy)
                UsedByDrawer.Reset(ids, false);

            if (IsFocusingSceneInScene)
                RefSceneInScene.ResetSceneInScene(Selection.gameObjects);

            if (IsFocusingUses)
            {
                //Debug.Log("is focusing uses" + Selection.gameObjects[0]);
                SceneUsesDrawer.ResetSceneUseSceneObjects(Selection.gameObjects);
            }
            if (IsFocusingFindInScene)
            {
                RefInScene.Reset(ids, this as IWindow);
            }

            if (IsFocusingGUIDs)
            {
                objs = new Object[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    objs[i] = FR2_Unity.LoadAssetAtPath<Object>
                        (
                            AssetDatabase.GUIDToAssetPath(ids[i])
                        );
                }
            }


            if (FR2_SceneCache.Api.Dirty && !Application.isPlaying)
            {
                FR2_SceneCache.Api.refreshCache(this);
            }

            EditorApplication.delayCall -= Repaint;
            EditorApplication.delayCall += Repaint;
        }




        // public static Rect windowRect;


        //return true if draw tool along


        protected override void OnGUI()
        {
            if (!CheckDrawHeader()) return;

            //if (Selected.Count == 0){
            //  GUILayout.Label("Nothing selected");
            //} 
            Rect rectTop = GetTopPanelRect();
            Rect rectBot = GetBotPanelRect();

            RefDrawConfig config1 = null, config2 = null, config3 = null;
            bool drawTool = false;
            int drawCount = 0;
            if (IsFocusingUses || IsFocusingSceneToAsset)
            {
                if (UsesDrawer.ElementCount() <= 0)
                {
                    drawTool = GetDrawConfig(SceneUsesDrawer, SceneToAssetDrawer, out config1, out config2);
                }
                else
                {
                    drawTool = GetDrawConfig(UsesDrawer, out config3);
                }
            }
            else if (IsFocusingSceneInScene || IsFocusingFindInScene || IsFocusingUsedBy)
            {
                if (RefSceneInScene.ElementCount() <= 0)
                {

                    drawTool = GetDrawConfig(RefInScene, UsedByDrawer, out config1, out config2);
                }
                else
                {
                    drawTool = GetDrawConfig(RefSceneInScene, out config3);

                }
            }
            else if (IsFocusingDuplicate)
            {
                drawTool = GetDrawConfig(Duplicated, out config1);
            }
            else if (IsFocusingGUIDs)
            {
                drawCount++;
                if (AnyToolInBot)
                {
                    GUILayout.BeginArea(rectTop);
                    DrawGUIDs();
                    GUILayout.EndArea();
                    drawTool = true;
                }
                else
                {
                    DrawGUIDs();
                    drawTool = false;
                }
            }
            else if (IsFocusingUnused)
            {

                drawCount++;
                if (AnyToolInBot)
                {
                    GUILayout.BeginArea(rectTop);
                    RefUnUse.Draw();
                    GUILayout.EndArea();
                    drawTool = true;
                }
                else
                {
                    RefUnUse.Draw();
                    drawTool = false;
                }
            }

            if (!IsFocusingGUIDs && !IsFocusingUnused)
            {
                drawCount += DrawConfig(config1, rectTop, rectBot, ref willRepaint);
                drawCount += DrawConfig(config2, rectTop, rectBot, ref willRepaint);
                drawCount += DrawConfig(config3, rectTop, rectBot, ref willRepaint);
            }
            if (drawTool)
            {
                drawCount++;
                GUILayout.BeginArea(rectBot);
                DrawTool();
                GUILayout.EndArea();

            }

            DrawFooter();

            OnAfterGUI(drawCount);
        }


        protected override void maskDirty()
        {

            UsedByDrawer.SetDirty();
            UsesDrawer.SetDirty();
            Duplicated.SetDirty();
            SceneToAssetDrawer.SetDirty();
            RefUnUse.SetDirty();

            RefInScene.SetDirty();
            RefSceneInScene.SetDirty();
            SceneUsesDrawer.SetDirty();
        }
        protected override void RefreshSort()
        {

            UsedByDrawer.RefreshSort();
            UsesDrawer.RefreshSort();
            Duplicated.RefreshSort();
            SceneToAssetDrawer.RefreshSort();
            RefUnUse.RefreshSort();


        }
        // public bool isExcludeByFilter;

        protected override bool checkNoticeFilter()
        {
            bool rsl = false;

            if (IsFocusingUsedBy && !rsl) rsl = UsedByDrawer.isExclueAnyItem();
            if (IsFocusingDuplicate) return Duplicated.isExclueAnyItem();
            if (IsFocusingUses && rsl == false) rsl = UsesDrawer.isExclueAnyItem();
            if (IsFocusingSceneToAsset && rsl == false) rsl = SceneToAssetDrawer.isExclueAnyItem();


            if (IsFocusingFindInScene && !rsl) rsl = RefInScene.isExclueAnyItem();
            if (IsFocusingSceneInScene && !rsl) rsl = RefSceneInScene.isExclueAnyItem();
            //tab use by
            return rsl;
        }
        protected override bool checkNoticeIgnore()
        {
            bool rsl = isNoticeIgnore;
            return rsl;
        }




        Object[] objs;
        string[] ids;
        private void DrawGUIDs()
        {

            GUILayout.Label("GUID to Object", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            {
                var guid = EditorGUILayout.TextField(tempGUID ?? string.Empty);
                EditorGUILayout.ObjectField(tempObject, typeof(Object), false, GUILayout.Width(120f));

                if (GUILayout.Button("Paste", EditorStyles.miniButton, GUILayout.Width(70f)))
                {
                    guid = EditorGUIUtility.systemCopyBuffer;
                }

                if (guid != tempGUID && !string.IsNullOrEmpty(guid))
                {
                    tempGUID = guid;

                    tempObject = FR2_Unity.LoadAssetAtPath<Object>
                    (
                        AssetDatabase.GUIDToAssetPath(tempGUID)
                    );
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            if (objs == null || ids == null) return;

            //GUILayout.Label("Selection", EditorStyles.boldLabel);
            if (ids.Length == objs.Length)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                {
                    for (var i = 0; i < ids.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.ObjectField(objs[i], typeof(Object), false);
                            var idi = ids[i];
                            GUILayout.TextField(idi, GUILayout.Width(240f));
                            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50f)))
                            {
                                EditorGUIUtility.systemCopyBuffer = tempGUID = idi;

                                if (!string.IsNullOrEmpty(tempGUID))
                                {

                                    tempObject = FR2_Unity.LoadAssetAtPath<Object>
                                    (
                                        AssetDatabase.GUIDToAssetPath(tempGUID)
                                    );
                                }
                                //Debug.Log(EditorGUIUtility.systemCopyBuffer);  
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }

            // GUILayout.FlexibleSpace();
            // GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Merge Selection To"))
            {
                FR2_Export.MergeDuplicate();

            }
            EditorGUILayout.ObjectField(tempObject, typeof(Object), false, GUILayout.Width(120f));
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
    }
}