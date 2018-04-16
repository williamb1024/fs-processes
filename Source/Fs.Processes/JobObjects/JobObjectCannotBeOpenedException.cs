using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Processes.JobObjects
{
    [Serializable]
    [ComVisible(false)]
    public class JobObjectCannotBeOpenedException: Exception
    {
        public JobObjectCannotBeOpenedException ()
            : base(Resources.JobObjectCannotBeOpenedException)
        {
        }

        public JobObjectCannotBeOpenedException ( string message )
            : base(message)
        {
        }

        public JobObjectCannotBeOpenedException ( string message, Exception innerException )
            : base(message, innerException)
        {
        }

        protected JobObjectCannotBeOpenedException ( SerializationInfo info, StreamingContext context )
            : base(info, context)
        {
        }
    }
}
