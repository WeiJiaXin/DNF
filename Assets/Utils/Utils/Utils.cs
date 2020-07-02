using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utils
{
    /// <summary>
    /// Gets the time stamp. Millisecond
    /// </summary>
    /// <returns>The time stamp.</returns>
    public static double GetTimeStampMillisecond()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return ts.TotalMilliseconds;
    }

    public static RectTransform Recttr(this Component mono)
    {
        return mono.transform as RectTransform;
    }

    /// <summary>
    /// Gets the time stamp. seconds
    /// </summary>
    /// <returns>The time stamp.</returns>
    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }

    public static IEnumerator FuncDelayRun(Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    public static string FormatNumberWithComma(long num)
    {
        return String.Format("{0:n0}", num);
    }

    /// <summary>
    /// 根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="t"></param>T值
    /// <param name="p0"></param>起始点
    /// <param name="p1"></param>控制点
    /// <param name="p2"></param>目标点
    /// <returns></returns>根据T值计算出来的贝赛尔曲线点
    private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    /// <summary>
    /// 获取存储贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float) segmentNum;
            Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                controlPoint, endPoint);
            path[i - 1] = pixel;
            //Debug.Log(path[i - 1]);
        }

        return path;
    }

    /// <summary>
    /// 获取屏幕一个点在世界里的坐标
    /// <para>正交相机</para>
    /// </summary>
    /// <param name="camera">相机</param>
    /// <param name="source">屏幕坐标</param>
    /// <param name="height">世界中的高度位置,因为投射的是一个直线,需要有一个交叉面</param>
    /// <param name="pos">输出坐标</param>
    /// <returns>是否有效</returns>
    public static bool GetPoint(Camera camera, Vector3 source, float height, out Vector3 pos)
    {
        //拿到手指位置的Viewport
        Vector3 view = camera.ScreenToViewportPoint(source);
        float w = GetAspectRatio() * camera.orthographicSize * 2 * (view.x - 0.5f);
        float h = camera.orthographicSize * 2 * (view.y - 0.5f);
        //拿到相机坐标和高度的比例
        var camPos = camera.transform.position;
        var inoutPos = camPos + camera.transform.up * h + camera.transform.right * w;
        if (Math.Abs(inoutPos.y - height) < 0.0001f)
            pos = inoutPos;
        else
        {
            var forward = camera.transform.forward;
            if (forward.y == 0)
                throw new Exception("相机的forward的Y轴不能为0,此时会和世界平行!");
            float h2y = (inoutPos.y - height) / forward.y;
            pos = inoutPos - forward * (h2y);
        }

        return true;
    }

    /// <summary>
    /// 获取集合中随机几个不重复的元素
    /// </summary>
    /// <param name="list">原始集合</param>
    /// <param name="num">数量</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IList<T> GetRandomListItems<T>(IList<T> list, int num)
    {
        //另外这样写也要注意,也不是深度复制喽,关于深度复制可以做为一个新话题来说,这儿就不说啦;
        IList<T> temp_list = new List<T>(list);
        //取出的项,保存在此列表
        IList<T> return_list = new List<T>();
        //Random random = new Random(unchecked((int)DateTime.Now.Ticks));
        System.Random random = new System.Random();
        for (int i = 0; i < num; i++)
        {
            //判断如果列表还有可以取出的项,以防下标越界
            if (temp_list.Count > 0)
            {
                //在列表中产生一个随机索引
                int arrIndex = random.Next(0, temp_list.Count);
                //将此随机索引的对应的列表元素值复制出来
                return_list.Add(temp_list[arrIndex]);
                //然后删掉此索引的列表项
                temp_list.RemoveAt(arrIndex);
            }
            else
            {
                //列表项取完后,退出循环,比如列表本来只有10项,但要求取出20项.
                break;
            }
        }

        return return_list;
    }

    public static float GetMatchWidthOrHeight()
    {
        float width = Screen.width;
        float height = Screen.height;
        float aspectRatio = width / height;
        if (aspectRatio >= 0.7f)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    //返回宽高比
    public static float GetAspectRatio()
    {
        float width = Screen.width;
        float height = Screen.height;
        return width / height;
    }


    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T result = obj.GetComponent<T>();
        if (result == null)
        {
            result = obj.AddComponent<T>();
        }

        return result;
    }

    public static bool IsNames(this AnimatorStateInfo info, params string[] names)
    {
        foreach (var name in names)
        {
            if (info.IsName(name))
                return true;
        }

        return false;
    }

    public static bool IsTags(this AnimatorStateInfo info, params string[] tags)
    {
        foreach (var tag in tags)
        {
            if (info.IsTag(tag))
                return true;
        }

        return false;
    }

    public static void Log(params object[] content)
    {
        string s = string.Empty;
        for (int i = 0; i < content.Length; i++)
        {
            s += content[i].ToString() + " ";
        }

        Debug.Log(s);
    }

    //时间戳转日期
    public static String Unix2Time(double stamp)
    {
//        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);//utc
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0)); //当地
        startTime = startTime.AddSeconds(stamp);
        return startTime.ToString("yyyy-MM-dd"); //HH:mm:ss.fff
    }
}