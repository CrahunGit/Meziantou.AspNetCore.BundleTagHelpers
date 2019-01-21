using System.Collections.Generic;

namespace Crahun.AspNetCore.BundleTagHelpers
{
    public class Bundle
    {
        public string Name { get; set; }
        public string OutputFileUrl { get; set; }
        public IList<string> InputFileUrls { get; set; }
    }
}
