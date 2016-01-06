using System;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [TraitDiscoverer("Microsoft.UnitTests.Core.XUnit.CategoryTraitDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CategoryAttribute : Attribute, ITraitAttribute {
        public string[] Categories { get; }

        public CategoryAttribute(params string[] categories) {
            Categories = categories;
        }
    }
}