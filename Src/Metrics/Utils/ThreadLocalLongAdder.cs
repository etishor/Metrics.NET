
using System.Threading;

namespace Metrics.Utils
{
    public sealed class ThreadLocalLongAdder
    {
        private sealed class ValueHolder
        {
            public long value;

            public ValueHolder() { }
            public ValueHolder(long value) { this.value = value; }

            public long GetAndReset()
            {
                return Interlocked.Exchange(ref this.value, 0L);
            }
        }

        private readonly ThreadLocal<ValueHolder> local = new ThreadLocal<ValueHolder>(() => new ValueHolder(), true);

        public ThreadLocalLongAdder() { }

        public ThreadLocalLongAdder(long value)
        {
            this.local.Value.value = value;
        }

        public long Value { get { return Sum(); } }

        public void Add(long value)
        {
            this.local.Value.value += value;
        }

        public void Increment()
        {
            this.local.Value.value++;
        }

        public void Decrement()
        {
            this.local.Value.value--;
        }

        public void SetValue(long value)
        {
            Reset();
            this.local.Value.value = value;
        }

        public long GetAndSet(long value)
        {
            long sum = 0;
            foreach (var val in this.local.Values)
            {
                sum += val.value;
                val.value = 0L;
            }
            this.local.Value.value = value;
            return sum;
        }

        public long GetAndReset()
        {
            long sum = 0;
            foreach (var val in this.local.Values)
            {
                sum += val.GetAndReset();
            }
            return sum;
        }

        private long Sum()
        {
            long sum = 0;
            foreach (var value in this.local.Values)
            {
                sum += value.value;
            }
            return sum;
        }

        public void Reset()
        {
            foreach (var value in this.local.Values)
            {
                value.value = 0L;
            }
        }
    }
}
