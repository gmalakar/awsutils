using System;
using System.Collections.Generic;
using System.Text;

namespace AWSCM.AWSOperations.Common
{
   public class Constants
   {
      #region Constant Definitions

      /// <summary>
      /// Represents a stock message for when a response is null.
      /// </summary>
      public const string NULL_RESPONSE_MESSAGE = "The returned response was null. Please investigate the cause and/or try again.";

      /// <summary>
      /// Represents the stock EC2 auto start 'tag'.
      /// </summary>
      public const string AUTO_START_TAG = "auto-start";

      /// <summary>
      /// Represents the stock EC2 auto stop 'tag'.
      /// </summary>
      public const string AUTO_STOP_TAG = "auto-stop";

      #endregion Constant Definitions  }
   }
}
