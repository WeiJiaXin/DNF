public class RoleBase : CharacterBase
{
    protected IInput input;
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<IInput>();
    }
}