using System;
using System.Diagnostics;
using System.Windows.Forms;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Fiddler;
using FiddlerOAuthAddon.Properties;

[assembly: Fiddler.RequiredVersion("2.3.5.0")]

namespace FiddlerOAuthAddon
{
    public class OAuthAddon : IAutoTamper3 
    {
        private bool OAuthAuthFound = false;
        private static OAuthConsumerContext consumerContext = new OAuthConsumerContext();
        private static OAuthSession session;
        internal static bool EnableOAuthOthorization = false;

        private TabPage oPage;
        private OAuthPropertiesUI propertiesUI;

        public static void Apply(string consumerKey, string consumerSecret, string signatureMethod, string token, string tokenSecret)
        {
            try
            {
                consumerContext = new OAuthConsumerContext()
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    SignatureMethod = signatureMethod
                };

                session = new OAuthSession(consumerContext,
                                    new Uri("https://oauth.intuit.com/oauth/v1/get_request_token"),
                                    new Uri("https://workplace.intuit.com/Connect/Begin"),
                                    new Uri("https://oauth.intuit.com/oauth/v1/get_access_token"))
                {
                    AccessToken = new AccessToken
                    {
                        Token = token,
                        TokenSecret = tokenSecret
                    },
                    ConsumerContext = { UseHeaderForOAuthParameters = true }
                };
            }
            catch (Exception exception)
            {

                MessageBox.Show(string.Format("Apply Failed with message {0}", exception.Message));
            }
                
        }


        public OAuthAddon()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            if (versionInfo.FileMajorPart >= 3 || versionInfo.FileMinorPart >= 1)
                return;
            int num = (int)MessageBox.Show(Resources.OAuthAddonVersionMEssage, Resources.OAuthAddonName);
            throw new Exception();
        }

        #region Implementation of IFiddlerExtension

        public void OnLoad()
        {
            try
            {
                oPage = new TabPage("OAuth Settings");
                FiddlerApplication.UI.tabsViews.ImageList.Images.Add(Resources.oauth);
                oPage.ImageIndex = (int)FiddlerApplication.UI.tabsViews.ImageList.Images.Count - 1;

                propertiesUI = new OAuthPropertiesUI();
                oPage.Controls.Add(propertiesUI);
                propertiesUI.Dock = DockStyle.Fill;
                FiddlerApplication.UI.tabsViews.TabPages.Add(oPage); 
            }
            catch (Exception exception)
            {

                MessageBox.Show(string.Format("Load of OAuth Plugin failed with message {0}", exception.Message));
            }
            
        }



        public void OnBeforeUnload()
        {
        }

        #endregion

        #region Implementation of IAutoTamper

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (EnableOAuthOthorization && !OAuthAuthFound)
            {
                try
                {
                    HTTPRequestHeaders headers = oSession.oRequest.headers;

                    string oAuthHeader =
                    session.Request().ForMethod(headers.HTTPMethod).ForUri(new Uri(oSession.fullUrl)).SignWithToken().Context.
                        GenerateOAuthParametersForHeader();

                    oSession.oRequest.headers.Add("Authorization", oAuthHeader);
                }
                catch (Exception exception)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break(); 
#endif

                    FiddlerApplication.Log.LogString(string.Format("Failed to add OAuth authorization for URL {0} with Message {1}", oSession.fullUrl, exception.Message)); 
                }
            }
            OAuthAuthFound = false;
        }

        public void AutoTamperRequestAfter(Session oSession)
        {

        }

        public void AutoTamperResponseBefore(Session oSession)
        {
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
        }

        public void OnBeforeReturningError(Session oSession)
        {
        }

        public void OnPeekAtResponseHeaders(Session oSession)
        {
        }

        public void OnPeekAtRequestHeaders(Session oSession)
        {
            HTTPRequestHeaders headers = oSession.oRequest.headers;

            if(headers.ExistsAndContains("Authorization", "OAuth"))
            {
                OAuthAuthFound = true;
#if DEBUG
                FiddlerApplication.Log.LogString(string.Format("OAuth Authorization found for request {0}", oSession.fullUrl)); 
#endif
            }
            else
            {
                OAuthAuthFound = false;
            }
        }

        #endregion
    }
}
