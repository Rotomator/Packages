using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace Atea
{
    //HTTP RELATED ACTIVITIES
    namespace HTTP
    {
        public class postXML : CodeActivity
        {
            [Category("Input")]
            [RequiredArgument]
            public InArgument<string> xmlFile { get; set; }

            [Category("Input")]
            public InArgument<string> URL { get; set; }

            [Category("Output")]
            public OutArgument<string> WebResponseCode { get; set; }

            protected override void Execute(CodeActivityContext context)
            {
                var XMLtoSend = xmlFile.Get(context);
                var url = URL.Get(context);

                if (helperFunctions.InternetConnected("https://eshop.atea.com/no/"))
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                    byte[] bytes;
                    bytes = System.Text.Encoding.ASCII.GetBytes(XMLtoSend);

                    request.ContentType = "text/xml; encoding='utf-8'";
                    request.ContentLength = bytes.Length;
                    request.Method = "POST";

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();

                    HttpWebResponse response;
                    response = (HttpWebResponse)request.GetResponse();
                    WebResponseCode.Set(context, ((int)response.StatusCode).ToString());
                }
                else
                {
                    WebResponseCode.Set(context, "No internet connection");
                }
            }
        }

        public class getPrice : CodeActivity
        {
            [Category("Input")]
            [RequiredArgument]
            public InArgument<string> Part { get; set; }

            [Category("Input")]
            public InArgument<string> CustomerID { get; set; }

            [Category("Input")]
            public InArgument<string> URL { get; set; }

            [Category("Output")]
            public OutArgument<string> Price { get; set; }

            protected override void Execute(CodeActivityContext context)
            {
                string part = Part.Get(context);
                string customerid = CustomerID.Get(context);
                string url3 = URL.Get(context);
                string sendXML3 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ProductPriceRequest xmlns=\"http://Atea.ESHOP.FreeChoice.Web.Service\"><productId>" + part + "</productId><customerId>" + customerid + "</customerId></ProductPriceRequest>";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url3);

                byte[] bytes;
                bytes = System.Text.Encoding.ASCII.GetBytes(sendXML3);

                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream responseStream = response.GetResponseStream();
                        string responseStr = new StreamReader(responseStream).ReadToEnd();
                        responseStr = System.Text.RegularExpressions.Regex.Match(responseStr, @"<price>[0-9.,]+").Value.Replace("<price>", "").Trim();

                        Price.Set(context, responseStr);
                    }
                    else { Price.Set(context, "Operation failed with the response: " + response.StatusCode); }
                }
                catch (Exception e)
                {
                    Price.Set(context, "Operation failed with the response: " + e.Message);
                }
            }
        }
    }

    //OTHER ACTIVITIES
    namespace Other
    {
        public class getScreenDimensions : CodeActivity
        {
            [Category("Output")]
            public OutArgument<string[]> Dimensions { get; set; }

            protected override void Execute(CodeActivityContext context)
            {
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                string[] dim = { screenWidth.ToString(), screenHeight.ToString() };

                Dimensions.Set(context, dim);
            }
        }
    }
}



//These functions are only visible in this code, and are not available for stand-alone use in UiPath.
public class helperFunctions
{
    //check if there is a internet connection
    public static Boolean InternetConnected(String url)
    {
        try
        {
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(url))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

    }
}
