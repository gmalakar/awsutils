using System;
using System.Collections.Generic;
using System.Text;

namespace AWSCM.AWSOperations.Models
{
   public class DescribeEC2 : BaseOperation
   {
      public List<string> InstanceIDs { get; set; } = new List<string>();
   }
}
