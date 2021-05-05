using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AWSCM.CommonShared
{
   public static partial class CacheManager
   {
      public static void SaveToCache<T>( string key, T value )
      {
         FileBasedCache.AddOrUpdate<T>( key, value );
      }

      public static void SaveToCache( string key, string value )
      {
         FileBasedCache.AddOrUpdate( key, value );
      }

      public static void RemoveFromCache( string key )
      {
         FileBasedCache.TryRemove( key );
      }

      public static bool TryGet( string key, out string value, out string message )
      {
         return FileBasedCache.TryGet( key, out value, out message );
      }

      public static bool TryGet<T>( string key, out T value, out string message )
      {
         var valid = false;
         value = default( T );
         valid = FileBasedCache.TryGet( key, out string value2, out message );

         if ( valid )
            value = StringUtils.ConvertFromJSON<T>( value2 );

         return valid;
      }

      public static string Get( string key )
      {
         FileBasedCache.TryGet( key, out string value, out _ );
         return value;
      }
      public static T Get<T>( string key )
      {
         TryGet<T>( key, out T value, out _ );
         return value;
      }
   }

   public static class FileBasedCache
   {
      static FileBasedCache()
      {
         Directory.CreateDirectory( Path.GetDirectoryName( mappedCachedFile ) );
         DeSerializeFromFile();
      }

      public static void ClearCache()
      {
         cachedDictionary.Clear();
         if ( File.Exists( mappedCachedFile ) )
            File.Delete( mappedCachedFile );
      }

      public static bool TryGet<T>( string key, out T value, out string message ) where T : new()
      {
         var get = false;
         message = string.Empty;
         value = default( T );

         if ( cachedDictionary.TryGetValue( key, out string cachedVal ) )
         {
            value = DeSerializeValue<T>( cachedVal );
            get = true;
         }
         else
            message = $"Key [{key}] not found";

         return get;
      }

      public static bool TryGet( string key, out string value, out string message )
      {
         var get = false;
         message = string.Empty;

         if ( cachedDictionary.TryGetValue( key, out value ) )
         {
            get = true;
         }
         else
            message = $"Key [{key}] not found";

         return get;
      }

      public static bool TryRemove( string key )
      {
         var removed = false;
         if ( cachedDictionary.ContainsKey( key ) )
         {
            cachedDictionary.Remove( key );
            removed = true;
            SerializeToFile();//save to file
         }
         return removed;
      }

      public static void AddOrUpdate<T>( string key, T value )
      {
         var added = true;
         var jsonString = StringUtils.ConvertToJSON( value );

         if ( cachedDictionary.ContainsKey( key ) )
            cachedDictionary[key] = jsonString;
         else
            added = cachedDictionary.TryAdd( key, jsonString );

         if ( added ) //save to file
            SerializeToFile();
      }

      public static void AddOrUpdate( string key, string value )
      {
         var added = true;
         if ( cachedDictionary.ContainsKey( key ) )
            cachedDictionary[key] = value;
         else
            added = cachedDictionary.TryAdd( key, value );

         if ( added ) //save to file
            SerializeToFile();
      }

      private static void SerializeToFile()
      {
         if ( cachedDictionary != null )
            File.WriteAllText( mappedCachedFile, Symmetric.EncryptString( ENCRYPTION_KEY, StringUtils.ConvertToJSON( cachedDictionary ) ) );
      }

      private static void DeSerializeFromFile()
      {
         if ( File.Exists( mappedCachedFile ) )
         {
            try
            {
               cachedDictionary = StringUtils.ConvertFromJSON<Dictionary<string, string>>( Symmetric.DecryptString( ENCRYPTION_KEY, File.ReadAllText( mappedCachedFile ) ) );
            }
            catch //just a new cached dictionary
            {
               cachedDictionary = new Dictionary<string, string>();
            }
         }
         else
            cachedDictionary = new Dictionary<string, string>();
      }

      private static T DeSerializeValue<T>( string value ) where T : new()
      {
         return StringUtils.ConvertFromJSON<T>( value );
      }

      static Dictionary<string, string> cachedDictionary;
      const string MAPFILENAME = "cache\\internalCache.dat";
      const string ENCRYPTION_KEY = "*rakalam matuog*";
      public static string cachedFolder = Utilities.AssemblyHomeDirectory;
      public static string mappedCachedFile = Path.Combine( cachedFolder, MAPFILENAME );
   }
}
