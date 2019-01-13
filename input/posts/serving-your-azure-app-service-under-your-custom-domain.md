Title: Serving your Azure App Service under your custom domain
Lead: Includes redirecting from your azurewebsites.net to your custom domain
Published: 11/15/2018
Image: /images/home-bg.jpg
Tags: 
    - Azure
    - Azure App Service
---

If you host your App Service on Azure you get a default address that is a sub domain of `azurewebsites.net`. If you are not just hosting a backend or personal web application where the url doesn't matter, you might want to add your custom domain. In this blog post I want to show a few things that you have to set up to achieve.

First you have to purchase your desired domain at any registrar that offers it.

- [godaddy](https://ch.godaddy.com/)
- [SwitchPlus](https://switchplus.ch/en/home/)
- [1and1](https://www.ionos.com/?ac=OM.US.US469K02463T2103a&PID=7518746&cjevent=346dac51e8a011e883b700560a180513)

If you have purchased your domain you will need a DNS Service. Which you have to configure so that your domain name gets resolved to your Azure App Service. For all my web pages I am using [Cloudflare](https://www.cloudflare.com/) as my DNS service, because the service that Cloudflare offers is just amazing. And there is a free tier solution at Cloudflare.

![image](/posts/images/AddSite.PNG)

After creating an account at Cloudflare we can add our site to the account. Cloudflare will automatically scan your domain for any previous DNS entries and for your current Nameservers. After your site was added to your account you have to change the current nameservers at your domain to the Cloudflare ones. Cloudflare has great tutorials on how to do that. 

![image](/posts/images/DNS.PNG)

Next you will have to configure your DNS entries. Go back to the Azure Portal and open the `App Service -> Your App Service -> Custom Domains Panel` blade.

![image](/posts/images/PortalSettings.PNG)

Click the `Add hostname` button and enter your domain / hostname you want to configure and click `Validate`. Azure will now check your DNS entries and since we don't have any configured yet the validation will fail. But the good thing is that Azure will show you what you will have to configure in your DNS to make it work. So go back to Cloudflare and add the records as shown in the Portal and revalidate. Your validation should now pass. Congratulations now your custom domain is hooked up with Azure.

 ![image](/posts/images/Validation.PNG)

If you came this far your web app gets served under your custom domain but it is also still accessible through the Azure sub domain. If you do not want your web page to be served under the Azure subdomain you can add a Url Rewrite section to your `web.config`. This will look like this (don't forget to replace `{YOUR_DOMAIN}` with your information):

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <remove name="aspNetCore" />
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModule" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="%LAUNCHER_PATH%" arguments="%LAUNCHER_ARGS%" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout">
        <environmentVariables />
      </aspNetCore>
      <rewrite>
        <rules>
          <rule name="Redirect azurewebsites-domain to custom-domain" stopProcessing="true">
            <match url=".*" />
            <conditions>
              <add input="{HTTP_HOST}" pattern="^{YOUR_DOMAIN}.azurewebsites.net$" />
            </conditions>
            <action type="Redirect" url="https://{YOUR_DOMAIN}/{R:0}" appendQueryString="true" redirectType="Permanent" />
          </rule>
        </rules>
      </rewrite>
    </system.webServer>
  </location>
</configuration>
```

If you add this you will automatically get redirected to your custom domain if you try to call the `azurewebsites.net` subdomain.
