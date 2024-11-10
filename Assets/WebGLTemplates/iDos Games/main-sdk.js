(function() 
{
    function getFullURL() {  
        return window.location.href;  
    }

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

    function loadAdsGramSDK(callback) 
    {
        if (platform === "telegram")
        {
            const script = document.createElement("script");  
            script.src = "https://sad.adsgram.ai/js/sad.min.js";  
            script.onload = callback;  
            script.onerror = () => {  
                console.error("Failed to load AdsGram SDK");  
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

    let AdController;
    function showAd(blockId) {  
        if (!AdController) {  
            AdController = window.Adsgram.init({ blockId: blockId });  
            console.log("AdsGram initialized with blockId:", blockId);  
        }  
      
        AdController.show().then((result) => {  
            if (result.done) {  
                console.log("User has finished watching the Ad");  
                window.igcInstance.SendMessage('WebFunctionHandler', 'OnAdComplete', JSON.stringify(result));  
            }  
        }).catch((result) => {  
            console.warn("Error displaying ad or ad skipped", result);  
            window.igcInstance.SendMessage('WebFunctionHandler', 'OnAdError', JSON.stringify(result));  
        });  
    }

    function copyToClipboard(text) {  
        navigator.clipboard.writeText(text).then(function() {  
            console.log('Text copied to clipboard: ' + text);  
        }).catch(function(err) {  
            console.error('Could not copy text: ', err);  
        });  
    }

    function pasteFromClipboard() {  
        navigator.clipboard.readText().then(function(text) {  
            window.igcInstance.SendMessage('WebFunctionHandler', 'OnPasteFromClipboard', text);  
        }, function(err) {  
            console.error('Could not read text from clipboard: ', err);  
        });  
    }

    const cacheName = "web-cache";

    function cacheSaveData(key, data) {  
        const url = "cache://" + key;  
        const response = new Response(new Blob([data], { type: 'application/octet-stream' }));  
        return caches.open(cacheName).then(cache => cache.put(url, response));  
    }  
      
    function cacheLoadData(key, callback) {      
        const url = "cache://" + key;      
        caches.open(cacheName).then(cache =>       
            cache.match(url).then(response => {      
                if (response) {      
                    response.arrayBuffer().then(buffer => {      
                        callback(buffer);      
                    });      
                } else {      
                    callback(null);      
                }      
            })      
        ).catch(err => {      
            console.error("Cache load failed", err);      
            callback(null);      
        });      
    }
      
    function cacheDeleteData(key) {  
        const url = "cache://" + key;  
        return caches.open(cacheName).then(cache => cache.delete(url));  
    }  
      
    function cacheClear() {  
        return caches.delete(cacheName);  
    }  

    window.cacheSaveData = cacheSaveData;
    window.cacheLoadData = cacheLoadData;
    window.cacheDeleteData = cacheDeleteData;
    window.cacheClear = cacheClear;

    window.getFullURL = getFullURL;
	window.getPlatform = function() { return platform; };
    window.getStartAppParameter = getStartAppParameter;
    window.getInitDataUnsafe = getInitDataUnsafe;
    window.shareAppLink = shareAppLink;
	window.openInvoice = openInvoice;
    window.showAd = showAd;
    window.copyToClipboard = copyToClipboard;
    window.pasteFromClipboard = pasteFromClipboard;
  
    const platform = getPlatform();
    console.log("Platform: " + platform);
    loadSDK(platform, () => {  
        console.log("SDK is ready to be used");  
    });  

    loadAdsGramSDK(() => {  
        console.log("AdsGram SDK is ready to be used");  
    }); 
})();  
