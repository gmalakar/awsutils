using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AWSCM.CommonShared
{
   public class Symmetric
   {
      private static byte[] EncryptByteArray( byte[] encryptionKey, byte[] dataToEncrypt )
      {
         byte[] encryptedData;

         using ( var aes = new AesCryptoServiceProvider() )
         {
            aes.Key = encryptionKey;

            // Inject identifier for AES encryption.
            byte[] tempIV = aes.IV;
            tempIV.SetValue( aesIdentifier[0], 7 );
            tempIV.SetValue( aesIdentifier[1], 8 );
            aes.IV = tempIV;
            using ( var cryptoTransform = aes.CreateEncryptor() )
               encryptedData = cryptoTransform.TransformFinalBlock( dataToEncrypt, 0, dataToEncrypt.Length );
            encryptedData = aes.IV.Concat( encryptedData ).ToArray();
         }

         return encryptedData;
      }

      private static byte[] DecryptByteArray( byte[] encryptionKey, byte[] dataToDecrypt )
      {
         byte[] initializationVector = new byte[ 16 ];
         byte[] decryptedData = null;

         for ( int x = 0; x < 16; x++ )
            initializationVector[x] = dataToDecrypt[x];

         using ( var aes = new AesCryptoServiceProvider() )
         {
            aes.Key = encryptionKey;
            aes.IV = initializationVector;
            using ( var cryptoTransform = aes.CreateDecryptor() )
               decryptedData = cryptoTransform.TransformFinalBlock( dataToDecrypt, 16, dataToDecrypt.Length - 16 );
         }

         return decryptedData;
      }


      public static string EncryptString( string key, string plainText )
      {
         string cipherText = string.Empty;
         var encryptedByte = EncryptByteArray( string.IsNullOrEmpty( key )? secretKey: Encoding.UTF8.GetBytes( key ), Encoding.UTF8.GetBytes( plainText ) );
         if ( encryptedByte != null )
            cipherText = Convert.ToBase64String( encryptedByte );
         return cipherText;
      }

      public static string DecryptString( string key, string cipherText )
      {
         string decryptedString = string.Empty;
         var decryptedByte = DecryptByteArray(string.IsNullOrEmpty( key )? secretKey:Encoding.UTF8.GetBytes( key ), Convert.FromBase64String( cipherText ) );
         if ( decryptedByte != null )
            decryptedString = Encoding.UTF8.GetString( decryptedByte );
         return decryptedString;
      }


      public static bool TryDecrypt( string key, string cipherText, out string plainText, out string errorMessage )
      {
         plainText = string.Empty;
         errorMessage = string.Empty;
         var success = false;
         try
         {
            plainText = DecryptString( key, cipherText );
            success = true;
         }
         catch ( Exception ex )
         {
            errorMessage = ExceptionUtils.ParseException( ex );
         }
         return success;
      }

      public static bool TryEncrypt( string key, string plainText, out string cipherText, out string errorMessage )
      {
         cipherText = string.Empty;
         errorMessage = string.Empty;
         var success = false;
         try
         {
            cipherText = EncryptString( key, plainText );
            success = true;
         }
         catch ( Exception ex )
         {
            errorMessage = ExceptionUtils.ParseException( ex );
         }
         return success;
      }

      private static readonly byte[] secretKey = { 45, 103, 111, 117, 116, 97, 109, 32, 109, 97, 108, 97, 107, 97, 114, 45 };
      private static readonly byte[] aesIdentifier = {0xf5, 0x5a};
   }
}
