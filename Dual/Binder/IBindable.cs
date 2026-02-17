namespace Dual.Binder
{
    public interface IBindable
    {
        BinderDual CreateBinderDual(IDual dualToBind);

        DualNotSpecifiedCompositor<IDual> CreateBinderDual();
    }
}