using System;

namespace Lowy.Event
{
    public static class EventSetting
    {
        private static EventSettingContent _setting;
        private static EventSettingContent Setting
        {
            get { return _setting = _setting ?? EventUtility.GetSetting(); }
        }

        /// <summary>
        /// 调度时异常是否打断下面的调用
        /// </summary>
        public static bool DispatchExceptionInterrupt
        {
            get => Setting.dispatchExceptionInterrupt;
            set
            {
                Setting.dispatchExceptionInterrupt = value;
                SaveSetting();
            }
        }

        private static void SaveSetting()
        {
            EventUtility.SaveSetting(Setting);
        }
    }
}