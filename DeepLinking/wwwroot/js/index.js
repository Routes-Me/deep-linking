
function GetDevice() {
    if (/iemobile|windows phone/i.test(navigator.userAgent.toLowerCase())) {
        return "windows";
    }
    else if (/android/i.test(navigator.userAgent.toLowerCase())) {
        return "android";
    }
    else if (/blackberry/i.test(navigator.userAgent.toLowerCase())) {
        return "blackberry";
    }
    else if (/ipad|iphone|ipod/i.test(navigator.userAgent.toLowerCase())) {
        return "ios";
    }
    else {
        return "web";
    }
}