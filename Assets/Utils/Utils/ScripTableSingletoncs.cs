using System;

namespace Yangtze
{
    using UnityEngine;

    [Serializable]
    public class SpriteAssetPair
    {
        public string spriteName;
        public Sprite sprite;
    }

    public class ScripTableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Load();
                    if (instance)
                    {
                        var self = instance as ScripTableSingleton<T>;
                        if (self)
                        {
                            self.Init();
                        }
                    }
                }

                return instance;
            }
        }

        private static T Load()
        {
            return Resources.Load<T>("Asset/" + typeof(T));
        }

        protected virtual void Init()
        {
        }
    }
}