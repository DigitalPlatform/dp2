using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RfidCenter
{
    // https://stackoverflow.com/questions/6661055/using-interlocked-compareexchange-operation-on-a-bool-value/18027246#18027246
    public class AtomicBoolean
    {
        private const int TRUE_VALUE = 1;
        private const int FALSE_VALUE = 0;
        private int zeroOrOne = FALSE_VALUE;

        public AtomicBoolean()
            : this(false)
        { }

        public AtomicBoolean(bool initialValue)
        {
            this.Value = initialValue;
        }

        /// <summary>
        /// Provides (non-thread-safe) access to the backing value
        /// </summary>
        public bool Value
        {
            get
            {
                return zeroOrOne == TRUE_VALUE;
            }
            set
            {
                zeroOrOne = (value ? TRUE_VALUE : FALSE_VALUE);
            }
        }

        /// <summary>
        /// Attempt changing the backing value from true to false.
        /// </summary>
        /// <returns>Whether the value was (atomically) changed from false to true.</returns>
        public bool FalseToTrue()
        {
            return SetWhen(true, false);
        }

        /// <summary>
        /// Attempt changing the backing value from false to true.
        /// </summary>
        /// <returns>Whether the value was (atomically) changed from true to false.</returns>
        public bool TrueToFalse()
        {
            return SetWhen(false, true);
        }

        /// <summary>
        /// Attempt changing from "whenValue" to "setToValue".
        /// Fails if this.Value is not "whenValue".
        /// </summary>
        /// <param name="setToValue"></param>
        /// <param name="whenValue"></param>
        /// <returns></returns>
        public bool SetWhen(bool setToValue, bool whenValue)
        {
            int comparand = whenValue ? TRUE_VALUE : FALSE_VALUE;
            int result = Interlocked.CompareExchange(ref zeroOrOne, (setToValue ? TRUE_VALUE : FALSE_VALUE), comparand);
            bool originalValue = result == TRUE_VALUE;
            return originalValue == whenValue;
        }
    }
}
