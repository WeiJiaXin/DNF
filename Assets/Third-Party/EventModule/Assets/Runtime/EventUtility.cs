using UnityEngine;

namespace Lowy.Event
{
    public static class EventUtility
    {
        private const string EVENT_MANAGER_WINDOW_CONTENT_KEY = "EVENT_MANAGER_WINDOW_CONTENT_KEY";
        private const string EVENT_MANAGER_SETTING_CONTENT_KEY = "EVENT_MANAGER_SETTING_CONTENT_KEY";
        
        public static EventWindowContent GetContent()
        {
            if (PlayerPrefs.HasKey(EVENT_MANAGER_WINDOW_CONTENT_KEY))
                return JsonUtility.FromJson<EventWindowContent>(PlayerPrefs.GetString(EVENT_MANAGER_WINDOW_CONTENT_KEY));
            return new EventWindowContent();
        }

        public static void SaveContent(EventWindowContent content)
        {
            PlayerPrefs.SetString(EVENT_MANAGER_WINDOW_CONTENT_KEY, JsonUtility.ToJson(content));
        }

        public static EventSettingContent GetSetting()
        {
            if (PlayerPrefs.HasKey(EVENT_MANAGER_SETTING_CONTENT_KEY))
                return JsonUtility.FromJson<EventSettingContent>(PlayerPrefs.GetString(EVENT_MANAGER_SETTING_CONTENT_KEY));
            return new EventSettingContent();
        }

        public static void SaveSetting(EventSettingContent content)
        {
            PlayerPrefs.SetString(EVENT_MANAGER_SETTING_CONTENT_KEY, JsonUtility.ToJson(content));
        }
    }
    
    public class EventWindowContent
    {
        //0-首选项,1-Event列表
        public int tab = 0;
        public string cs_path = "Scripts/Event";
        //
        public string eventName = "NewEventEve";
    }
    public class EventSettingContent
    {
        /// <summary>
        /// 调度时异常是否打断下面的调用
        /// </summary>
        public bool dispatchExceptionInterrupt = true;
    }
}