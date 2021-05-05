using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AWSCM.CommonShared
{
   [Serializable]
   public class RESTClientResponse
   {
      public RESTClientResponse() { }

      public RESTClientResponse( bool success, string response )
      {
         this.Success = success;
         this.Response = response;
      }
      public RESTClientResponse( HttpResponseMessage response )
      {
         if ( response != null )
         {
            Response = response.Content.ReadAsStringAsync().Result;
            Success = response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK;
            if ( !Success )
               Response = $"Error: {response.StatusCode}, Reason: { response.ReasonPhrase}, Description: {Response}";
            httpResponseMessage = response;
         }
      }

      public bool Success { get; private set; } = false;
      public string Response { get; private set; } = string.Empty;

      private HttpResponseMessage httpResponseMessage = null;
      public Dictionary<string, string> Cookies
      {
         get
         {
            if ( !cookieSet && httpResponseMessage != null )
            {
               cookies = RESTClient.GetCookies( httpResponseMessage );
               cookieSet = true;
            }
            return cookies;
         }
      }

      private Dictionary<string, string> cookies = new Dictionary<string, string>();

      private bool cookieSet = false;

   }

   public static class RESTClient
   {
      private static readonly HttpClient client = new HttpClient();

      public static Dictionary<string, string> GetCookies( HttpResponseMessage response )
      {
         var cookies = new Dictionary<string, string>();
         if ( response != null && response.Headers.TryGetValues( "Set-Cookie", out var setCookies ) )
         {
            foreach ( var cookie in setCookies )
            {
               foreach ( var token in cookie.Split( ';' ) )
               {
                  //split only first occurance of '='
                  var keyVal = token.Split('=', 2 );
                  if ( keyVal.Count() == 2 ) //take this
                  {
                     cookies.TryAdd( keyVal[0], keyVal[1].TrimStart( '\"' ).TrimEnd( '\"' ) );
                  }
               }
            }
         }
         return cookies;
      }

      public static async Task<RESTClientResponse> InvokeApi( string requestUri, HttpMethod httpMethod = null, HttpContent content = null, Dictionary<string, string> headers = null, Dictionary<string, string> cookies = null )
      {
         RESTClientResponse clientResponse = null;
         try
         {
            var uri = new Uri(requestUri);
            using ( var client = new HttpClient() )
            {
               var request = new HttpRequestMessage( httpMethod == null ? HttpMethod.Get : httpMethod, uri );
               if ( content != null )
                  request.Content = content;
               client.DefaultRequestHeaders.Clear();
               SetHeaders( request, headers );
               SetCookie( request, cookies );
               var response = await client.SendAsync( request );
               clientResponse = new RESTClientResponse( response );
            }
         }

         catch ( Exception ex )
         {
            var responseString = $"Error: {ex.Message}";
            if ( ex.InnerException != null )
               responseString = responseString + $"Inner Error: {ex.InnerException.Message}";
            clientResponse = new RESTClientResponse( false, responseString );
         }
         return clientResponse;
      }

      public static void SetCookie( HttpRequestMessage reuestMessage, Dictionary<string, string> cookies )
      {
         if ( reuestMessage != null && cookies != null && cookies.Count > 0 )
         {
            reuestMessage.Headers.Add( "Cookie", string.Join( ";", cookies.Select( kv => kv.Key + "=" + kv.Value ).ToArray() ) );
         }
      }

      public static void SetHeaders( HttpRequestMessage reuestMessage, Dictionary<string, string> headers )
      {
         if ( reuestMessage != null && headers != null && headers.Count > 0 )
         {
            foreach ( var keyValPair in headers )
               reuestMessage.Headers.TryAddWithoutValidation( keyValPair.Key, keyValPair.Value );
         }
      }

      //public static async Task<RESTClientResponse> InvokeApi( string requestUri, HttpMethod httpMethod = null, HttpContent content = null, params object[] args )
      //{
      //   var success = false;
      //   var errorMessage = string.Empty;
      //   var responseString =  string.Empty;

      //   try
      //   {
      //      //var request = new HttpRequestMessage( httpMethod == null ? HttpMethod.Get : httpMethod, requestUri );
      //      //if ( content != null )
      //      //   request.Content = content;
      //      client.DefaultRequestHeaders.Clear();
      //      //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
      //      //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/plain");
      //      HttpResponseMessage response = null;
      //      if ( httpMethod == HttpMethod.Post )
      //         response = await client.PostAsync( requestUri, content );
      //      else
      //         response = await client.GetAsync( requestUri );

      //      responseString = await response.Content.ReadAsStringAsync();
      //      success = response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK;
      //      if ( !success )
      //         responseString = $"Error: {response.StatusCode}, Reason: { response.ReasonPhrase}, Description: {responseString}";
      //   }
      //   catch ( Exception ex )
      //   {
      //      responseString = $"Error: {ex.Message}";
      //      if ( ex.InnerException != null )
      //         responseString = responseString + $"Inner Error: {ex.InnerException.Message}";
      //   }
      //   return new RESTClientResponse( success, responseString );
      //}


   }
}
