using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Languages.Core.Test {
    internal static class AssertionExtensions {
        public static TokenAssertions<TTokenType> Should<TTokenType>(this IToken<TTokenType> token) {
            return new TokenAssertions<TTokenType>(token);
        }
    }
}