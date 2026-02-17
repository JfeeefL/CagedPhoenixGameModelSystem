using Binder.ModelData;

namespace GameModelSystem.Editor
{
    public static class UniAttribute
    {
        public static T Resolve<T>(object attribute)
        {
            return ((UniAttributeData<T>)attribute).GetValue();
        }
    }
}