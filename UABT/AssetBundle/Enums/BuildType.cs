// ReSharper disable IdentifierTypo
namespace Hi3Helper.UABT
{
    public class BuildType(string type)
    {
        public bool IsPatch => type == "p";
    }
}
