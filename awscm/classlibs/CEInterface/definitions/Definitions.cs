using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AWSCM.CEInterface.Definitions
{

   public struct KeyValue
   {
      [JsonPropertyName( "key" )]
      public string Key { get; set; }

      [JsonPropertyName( "value" )]
      public string Value { get; set; }
   }

   public struct ReplConfig
   {

      [JsonPropertyName( "volumeEncryptionKey" )]
      public string VolumeEncryptionKey { get; set; }

      [JsonPropertyName( "replicationTags" )]
      public List<KeyValue> ReplicationTags { get; set; }

      [JsonPropertyName( "disablePublicIp" )]
      public bool DisablePublicIp { get; set; }

      [JsonPropertyName( "subnetHostProject" )]
      public string SubnetHostProject { get; set; }

      [JsonPropertyName( "replicationSoftwareDownloadSource" )]
      public string ReplicationSoftwareDownloadSource { get; set; }

      [JsonPropertyName( "replicationServerType" )]
      public string ReplicationServerType { get; set; }

      [JsonPropertyName( "useLowCostDisks" )]
      public bool UseLowCostDisks { get; set; }

      [JsonPropertyName( "computeLocationId" )]
      public string ComputeLocationId { get; set; }

      [JsonPropertyName( "cloudCredentials" )]
      public string CloudCredentials { get; set; }

      [JsonPropertyName( "subnetId" )]
      public string SubnetId { get; set; }

      [JsonPropertyName( "logicalLocationId" )]
      public string LogicalLocationId { get; set; }

      [JsonPropertyName( "bandwidthThrottling" )]
      public int BandwidthThrottling { get; set; }

      [JsonPropertyName( "useDedicatedServer" )]
      public bool UseDedicatedServer { get; set; }

      [JsonPropertyName( "zone" )]
      public string Zone { get; set; }

      [JsonPropertyName( "replicatorSecurityGroupIDs" )]
      public List<string> ReplicatorSecurityGroupIDs { get; set; }

      [JsonPropertyName( "usePrivateIp" )]
      public bool UsePrivateIp { get; set; }

      [JsonPropertyName( "proxyUrl" )]
      public string ProxyUrl { get; set; }

      [JsonPropertyName( "volumeEncryptionAllowed" )]
      public bool VolumeEncryptionAllowed { get; set; }

      [JsonPropertyName( "objectStorageLocation" )]
      public string ObjectStorageLocation { get; set; }

      [JsonPropertyName( "archivingEnabled" )]
      public bool ArchivingEnabled { get; set; }

      [JsonPropertyName( "converterType" )]
      public string ConverterType { get; set; }

      [JsonPropertyName( "storageLocationId" )]
      public string StorageLocationId { get; set; }

      public void SetDefault()
      {
         this.DisablePublicIp = true;
         this.UseLowCostDisks = true;
         this.UseDedicatedServer = true;
         this.UsePrivateIp = true;
         this.VolumeEncryptionAllowed = true;
         this.ArchivingEnabled = true;
         this.BandwidthThrottling = 0;
      }
   }
}
