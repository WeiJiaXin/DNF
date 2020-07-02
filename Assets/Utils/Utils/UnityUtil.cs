using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class UnityUtil
{
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string b64)
    {
        byte[] decodedBytes = Convert.FromBase64String(b64);
        string decodedText = Encoding.UTF8.GetString(decodedBytes);
        return decodedText;
    }

    //这里默认Canvas是1080*1920，并且是宽适配
    public static float SceneWidth => 1080;
    public static float SceneHeight => (1080f / Screen.width)*Screen.height;

    public static Sprite LoadTexture2DToSprite(string path, bool hasAlpha = true)
    {
        var imageAsset = Resources.Load<TextAsset>(path);
        Texture2D texture2D = new Texture2D(2, 2, hasAlpha ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
        texture2D.LoadImage(imageAsset.bytes);

        var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f), 100);
        return sprite;
    }

    public static void SetAlpha(this SpriteRenderer renderer, float alpha)
    {
        if (null != renderer)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public static void SetAlpha(this Image image, float alpha)
    {
        if (null != image)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public static void SetAlpha(this RawImage image, float alpha)
    {
        if (null != image)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public static void SetAlpha(this Text text, float alpha)
    {
        if (null != text)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }

    public static void SetAlpha(this GameObject canvasGroup, float alpha)
    {
        canvasGroup.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public static void SetColor(this Image image, Color color, float alpha = 1f)
    {
        if (null != image)
        {
            color.a = alpha;
            image.color = color;
        }
    }

    public static void SetColor(this Image image, string hex, float alpha = 1f)
    {
        if (null != image)
        {
            Color color = new Color();
            color.a = alpha;
            ColorUtility.TryParseHtmlString(hex, out color);
            image.color = color;
        }
    }

    public static void SetColor(this SpriteRenderer renderer, Color color, float alpha = 1f)
    {
        if (null != renderer)
        {
            color.a = alpha;
            renderer.color = color;
        }
    }

    public static void SetColor(this Text text, string hex, float alpha = 1f)
    {
        Color color = new Color();
        color.a = alpha;
        ColorUtility.TryParseHtmlString(hex, out color);
        text.color = color;
    }

    public static void SetActive<T>(this T component, bool value) where T : MonoBehaviour
    {
        if (null == component)
        {
            return;
        }

        var go = component.gameObject;
        if (go.activeSelf != value)
        {
            go.SetActive(value);
        }
    }

    public static List<T> Clone<T>(this List<T> cloneList)
    {
        List<T> list = new List<T>();
        list.AddRange(cloneList);
        return list;
    }

    public static List<T> DeepClone<T>(this List<T> cloneList) where T : ICloneable
    {
        var list = new List<T>(cloneList.Count);
        for (int i = 0; i < cloneList.Count; i++)
        {
            list.Add((T) cloneList[i].Clone());
        }

        return list;
    }

    public static void AspectFitSize(this Image image, float targetX, float targetY)
    {
        image.SetNativeSize();
        float scale = 1;
        if (image.rectTransform.sizeDelta.x > 0 && image.rectTransform.sizeDelta.y > 0)
        {
            float aspectRatio = image.rectTransform.sizeDelta.x / image.rectTransform.sizeDelta.y;
            float targetRatio = targetX / targetY;
            if (aspectRatio < targetRatio)
            {
                scale = targetY / image.rectTransform.sizeDelta.y;
            }
            else
            {
                scale = targetX / image.rectTransform.sizeDelta.x;
            }
        }

        image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x * scale,
            image.rectTransform.sizeDelta.y * scale);
    }

    public static void SetSortingOrder(this GameObject item, int sortingOrder)
    {
        if (item == null)
        {
            return;
        }

        var sg = item.GetComponent<SortingGroup>();
        if (sg)
        {
            sg.sortingOrder = sortingOrder;
        }
        else
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sr.sortingOrder = sortingOrder;
            }
        }
    }

    public static string SetSortingLayer(this GameObject item, string layerName)
    {
        if (item == null)
        {
            return null;
        }

        string sourceLayerName = null;
        var sg = item.GetComponent<SortingGroup>();
        if (sg)
        {
            sourceLayerName = sg.sortingLayerName;
            sg.sortingLayerName = layerName;
        }
        else
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sourceLayerName = sr.sortingLayerName;
                sr.sortingLayerName = layerName;
            }
        }

        return sourceLayerName;
    }

    public static string SetSortingLayerAndOrder(this GameObject item, string layerName, int sortingOrder)
    {
        if (item == null)
        {
            return null;
        }

        string sourceLayerName = null;
        var sg = item.GetComponent<SortingGroup>();
        if (sg)
        {
            sourceLayerName = sg.sortingLayerName;
            sg.sortingLayerName = layerName;
            sg.sortingOrder = sortingOrder;
        }
        else
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sourceLayerName = sr.sortingLayerName;
                sr.sortingLayerName = layerName;
                sr.sortingOrder = sortingOrder;
            }
        }

        return sourceLayerName;
    }

    public static void GetSortingLayerAndOrder(this GameObject item, out string layerName, out int sortingOrder)
    {
        layerName = "Default";
        sortingOrder = 0;

        if (item == null)
        {
            return;
        }

        var sg = item.GetComponent<SortingGroup>();
        if (sg)
        {
            layerName = sg.sortingLayerName;
            sortingOrder = sg.sortingOrder;
        }
        else
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr)
            {
                layerName = sr.sortingLayerName;
                sortingOrder = sr.sortingOrder;
            }
        }
    }

    public static bool ClickCoverUI()
    {
        List<RaycastResult> rs = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current), rs);
#if (UNITY_IOS || UNITY_ANDROID)&&!UNITY_EDITOR
            if(Input.touchCount<=0)
                return true;
        if (EventSystem.current != null && (rs.Count > 0 || EventSystem.current.IsPointerOverGameObject(0)))
#else
        if (EventSystem.current != null && (rs.Count > 0 || EventSystem.current.IsPointerOverGameObject()))
#endif
        {
            return true;
        }

        return false;
    }

    public static string Md5Sum(this string strToEncrypt)
    {
        UTF8Encoding ue = new UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        MD5CryptoServiceProvider md5 =
            new MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    public static void AddNoContains<T>(this List<T> list, T t)
    {
        if (!list.Contains(t))
        {
            list.Add(t);
        }
    }

    public static bool RemoveContains<T>(this List<T> list, T t)
    {
        if (list.Contains(t))
        {
            return list.Remove(t);
        }

        return false;
    }

    public static bool IsEditor()
    {
#if UNITY_EDITOR
        return true;
#endif
        return false;
    }

    public static bool IsIphoneXDevice()
    {
        bool isIphoneXDevice = false;
#if UNITY_EDITOR
        isIphoneXDevice = true;
#endif
        string modelStr = SystemInfo.deviceModel;
#if UNITY_IOS
        // iPhoneX:"iPhone10,3","iPhone10,6"  iPhoneXR:"iPhone11,8"  iPhoneXS:"iPhone11,2"  iPhoneXS Max:"iPhone11,6"
        isIphoneXDevice = modelStr.Equals("iPhone10,3") || modelStr.Equals("iPhone10,6") ||
                          modelStr.Equals("iPhone11,8") || modelStr.Equals("iPhone11,2") ||
                          modelStr.Equals("iPhone11,6");
#endif
        return isIphoneXDevice;
    }


    public static void AdjustOutline(this Text text)
    {
        Outline outline = text.GetComponent<Outline>();
        if (outline)
        {
            Vector2 distance = outline.effectDistance;
            float value = Mathf.Max(Mathf.Abs(distance.x), Mathf.Abs(distance.y));

            if (value > 2)
            {
                outline.effectDistance = new Vector2(2, -2);
            }

            for (int i = 0; i < value - 2; i++)
            {
                var temp = text.gameObject.AddComponent<Outline>();
                temp.effectDistance = new Vector2(1, -1);
                temp.effectColor = outline.effectColor;
            }
        }
    }

    public static bool CheckNetwork()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                //without network
                return false;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                //4G
                return true;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                //wifi
                return true;
        }

        return false;
    }
}