
namespace WinFormsScanSaraff.Twain
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal sealed class TwTypeAttribute : Attribute
    {
        public TwTypeAttribute(TwType type) => this.TwType = type;

        public TwType TwType { get; private set; }
    }
}
