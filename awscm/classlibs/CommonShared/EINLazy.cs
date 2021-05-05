using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AWSCM.CommonShared
{
   public static class EINLazy
   {
      public static Lazy<T> Get<T>( Func<T> valueFactory ) => new Lazy<T>( valueFactory, LazyThreadSafetyMode.PublicationOnly );

      public static Lazy<T> Get<T>( Func<T> valueFactory, LazyThreadSafetyMode mode ) => new Lazy<T>( valueFactory, mode );
   }
}
