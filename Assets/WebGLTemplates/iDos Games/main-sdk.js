(function() 
{
    function getPlatformFromURL() {  
        const urlParams = new URLSearchParams(window.location.search);  
        return urlParams.get('platform');  
    }

    function getPlatform() 
	{
		const platformFromURL = getPlatformFromURL();

		if (platformFromURL === "telegram")
		{
			return "telegram";
		}
		
		return "web";
	}

    function loadSDK(platform, callback) 
	{  
        if (platform === "telegram")
        {
            const script = document.createElement("script");  
            script.src = "https://telegram.org/js/telegram-web-app.js";  
            script.onload = () =>
            {
                Telegram.WebApp.ready();
                callback();
            };
            document.head.appendChild(script);
        }
        
    }

    function getStartAppParameter() 
	{
		if(platform === "telegram")
		{
			const initDataUnsafe = Telegram.WebApp.initDataUnsafe;
			if (initDataUnsafe && initDataUnsafe.start_param) 
			{
				return initDataUnsafe.start_param;
			}
			return null;
		}
		else
		{
			const urlParams = new URLSearchParams(window.location.search);
			return urlParams.get('startapp');
		}
    }

    function getInitDataUnsafe() {  
        if (platform === "telegram") {  
            return JSON.stringify(Telegram.WebApp.initDataUnsafe);  
        }  
        return null;  
    }

    function shareAppLink(appUrl) 
	{  
        if (platform === "telegram") 
		{  
            const shareUrl = `https://t.me/share/url?url=` + appUrl;
            Telegram.WebApp.openTelegramLink(shareUrl);  
        }  
    }

	function openInvoice(invoiceUrl) {  
        if (platform === "telegram") {  
            Telegram.WebApp.openInvoice(invoiceUrl);  
        } else {  
            window.open(invoiceUrl, '_blank');  
        }  
    }

	window.getPlatform = function() { return platform; };
    window.getStartAppParameter = getStartAppParameter;
    window.getInitDataUnsafe = getInitDataUnsafe;
    window.shareAppLink = shareAppLink;
	window.openInvoice = openInvoice;
  
    const platform = getPlatform();
    console.log("Platform: " + platform);
    loadSDK(platform, () => {  
        console.log("SDK is ready to be used");  
    });  
})();  
