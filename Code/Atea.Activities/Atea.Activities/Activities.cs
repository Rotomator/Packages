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
            [RequiredArgument]
            public InArgument<string> CustomerID { get; set; }

            [Category("Input")]
            [RequiredArgument]
            public InArgument<string> URL { get; set; }

            [Category("Output")]
            public OutArgument<string> Price { get; set; }

            protected override void Execute(CodeActivityContext context)
            {
                string part = Part.Get(context);
                string customerid = CustomerID.Get(context);
                string url = URL.Get(context);

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                    request.Method = "POST";
                    request.ContentType = "text/xml";

                    string SOAPReqBody = String.Format(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://web.pricecatalog.ws.io.etailer.netset.se/"">
                                                       <soapenv:Header/>
                                                       <soapenv:Body>
                                                          <web:GetCatalogSingleProductPricesRequest>
                                                             <CustomerID>{0}</CustomerID>
                                                             <ProductID productnumbertype=""EXTERNALID"">{1}</ProductID>
                                                          </web:GetCatalogSingleProductPricesRequest>
                                                       </soapenv:Body>
                                                     </soapenv:Envelope>", customerid, part);

                    byte[] bytes = Encoding.UTF8.GetBytes(SOAPReqBody);
                    request.ContentLength = bytes.Length;

                    using (Stream putStream = request.GetRequestStream())
                    {
                        putStream.Write(bytes, 0, bytes.Length);
                    }

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string servireResult = reader.ReadToEnd();
                            Price.Set(context, System.Text.RegularExpressions.Regex.Match(servireResult, @"<Price>[0-9.,]+").Value.Replace("<Price>", "").Trim());
                        }
                    }
                    else
                        throw new Exception("Operation failed with the response: " + response.StatusCode);
                }
                catch (Exception e)
                {
                    Price.Set(context, $"While getting price from web service: {e.Message}");
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
