mergeInto(LibraryManager.library, {  
    GetPlatform: function() {  
        if (typeof window.getPlatform === 'function') {  
            var platform = window.getPlatform();  
            if (platform === null) {  
                platform = "";  
            }  
            var bufferSize = lengthBytesUTF8(platform) + 1;  
            var buffer = _malloc(bufferSize);  
            stringToUTF8(platform, buffer, bufferSize);  
            return buffer;  
        } else {  
            console.warn("getPlatform function is not defined");  
            return 0;  
        }  
    },
    
    GetStartAppParameter: function() {  
        if (typeof window.getStartAppParameter === 'function') {  
            var startAppParameter = window.getStartAppParameter();  
            if (startAppParameter === null) {  
                startAppParameter = "";  
            }  
            var bufferSize = lengthBytesUTF8(startAppParameter) + 1;  
            var buffer = _malloc(bufferSize);  
            stringToUTF8(startAppParameter, buffer, bufferSize);  
            return buffer;  
        } else {  
            console.warn("getStartAppParameter function is not defined");  
            return 0;  
        }  
    },  
  
    ShareAppLink: function(appUrl) {  
        if (typeof window.shareAppLink === 'function') {  
            var appUrlStr = UTF8ToString(appUrl);  
            window.shareAppLink(appUrlStr);  
        } else {  
            console.warn("shareAppLink function is not defined or Telegram SDK is not loaded");  
        }  
    },

    OpenInvoice: function(invoiceUrl) {  
        if (typeof window.openInvoice === 'function') {  
            var invoiceUrlStr = UTF8ToString(invoiceUrl);  
            window.openInvoice(invoiceUrlStr);  
        } else {  
            console.warn("openInvoice function is not defined or Telegram SDK is not loaded");  
        }  
    },

    GetInitDataUnsafe: function() {  
        if (typeof window.getInitDataUnsafe === 'function') {  
            var initData = window.getInitDataUnsafe();  
            if (initData === null) {  
                initData = "";  
            }  
            var bufferSize = lengthBytesUTF8(initData) + 1;  
            var buffer = _malloc(bufferSize);  
            stringToUTF8(initData, buffer, bufferSize);  
            return buffer;  
        } else {  
            console.warn("getInitDataUnsafe function is not defined");  
            return 0;  
        }  
    },

    ShowAd: function(blockId) {  
        if (typeof window.showAd === 'function') {  
            var blockIdStr = UTF8ToString(blockId);  
            window.showAd(blockIdStr);  
        } else {  
            console.warn("showAd function is not defined or AdsGram SDK is not loaded");  
        }  
    },

    CopyToClipboard: function (text) {  
        var str = UTF8ToString(text);  
        window.copyToClipboard(str);  
    },

    PasteFromClipboard: function () {  
        window.pasteFromClipboard();  
    },

    GetFullURL: function() {    
        if (typeof window.getFullURL === 'function') {    
            var fullURL = window.getFullURL();    
            var bufferSize = lengthBytesUTF8(fullURL) + 1;    
            var buffer = _malloc(bufferSize);    
            stringToUTF8(fullURL, buffer, bufferSize);    
            return buffer;    
        } else {    
            console.warn("getFullURL function is not defined");    
            return 0;    
        }    
    },

    CacheSaveData: function(key, dataPtr, length) {    
        var keyStr = UTF8ToString(key);    
        var data = new Uint8Array(HEAPU8.buffer, dataPtr, length);  
        window.cacheSaveData(keyStr, data)  
            .then(() => console.log('Cache saved successfully'))  
            .catch(err => console.error("Cache save failed", err));  
    },  
  
    CacheLoadData: function(key, callback) {  
    var keyStr = UTF8ToString(key);  
    window.cacheLoadData(keyStr, function(buffer) {  
        if (buffer) {  
            var dataPtr = _malloc(buffer.byteLength);  
            HEAPU8.set(new Uint8Array(buffer), dataPtr);  
            {{{ makeDynCall('vii', 'callback') }}}(dataPtr, buffer.byteLength);  
        } else {  
            {{{ makeDynCall('vii', 'callback') }}}(0, 0);  
        }  
    });  
    },
  
    CacheDeleteData: function(key) {  
        var keyStr = UTF8ToString(key);    
        window.cacheDeleteData(keyStr)  
            .then(() => console.log('Cache deleted successfully'))  
            .catch(err => console.error("Cache delete failed", err));  
    },  
  
    CacheClear: function() {  
        window.cacheClear()  
            .then(() => console.log('Cache cleared successfully'))  
            .catch(err => console.error("Cache clear failed", err));  
    },

    OnWindowFocus: function() {  
        if (typeof window.onWindowFocus === 'function') {  
            window.onWindowFocus();  
        } else {  
            console.warn("onWindowFocus function is not defined");  
        }  
    },  
  
    OnWindowBlur: function() {  
        if (typeof window.onWindowBlur === 'function') {  
            window.onWindowBlur();  
        } else {  
            console.warn("onWindowBlur function is not defined");  
        }  
    },  
  
    OnWindowBeforeUnload: function() {  
        if (typeof window.onWindowBeforeUnload === 'function') {  
            window.onWindowBeforeUnload();  
        } else {  
            console.warn("onWindowBeforeUnload function is not defined");  
        }  
    }
});  
