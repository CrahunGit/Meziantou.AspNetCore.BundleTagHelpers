namespace Crahun.AspNetCore.BundleTagHelpers
{
    public interface IBundleProvider
    {
        Bundle GetBundle(string name);
    }
}