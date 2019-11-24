using System;
using Lowy.Bind;
using Lowy.UIFramework;

public class SelectRoleUIUIView : UIView
{
    [Inject] private SelectRoleUIUIView view;
    protected override void PlayEnterAnim(AbsContent content, Action<AbsContent> end)
    {
        gameObject.SetActive(true);
        //TODO 实现动画，并在动画结束时调用
        //----    动画结束后调用
        end?.Invoke(content);
        //----
    }

    protected override void PlayExitAnim(AbsContent content, Action<AbsContent> end)
    {
        //TODO 实现动画，并在动画结束时调用
        //----    动画结束后调用
        gameObject.SetActive(false);
        end?.Invoke(content);
        //----
    }
}

public class SelectRoleUIContent : AbsContent
{
    public override UIContentType ContentType => UIContentType.UIView;
}