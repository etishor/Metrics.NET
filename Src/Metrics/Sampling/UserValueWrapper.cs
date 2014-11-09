
using System.Collections.Generic;
namespace Metrics.Sampling
{
    public sealed class UserValueWrapper
    {
        public static readonly IComparer<UserValueWrapper> Comparer = new UserValueComparer();

        public readonly long Value;
        public readonly string UserValue;

        public UserValueWrapper()
            : this(0, null)
        { }

        public UserValueWrapper(long value, string userValue = null)
        {
            this.Value = value;
            this.UserValue = userValue;
        }

        private class UserValueComparer : IComparer<UserValueWrapper>
        {
            public int Compare(UserValueWrapper x, UserValueWrapper y)
            {
                return Comparer<long>.Default.Compare(x.Value, y.Value);
            }
        }
    }
}
