function showCurrentNav(currentID) {
    let navElements = document.getElementsByClassName('nav-link');
    for (var i = 0, len = navElements.length | 0; i < len; i = i + 1 | 0) {
        let el = navElements[i];
        el.classList.remove('nav-link-selected');
    }
    var current = document.getElementById(currentID);
    current.classList.add('nav-link-selected');
}

window.isChrome = function () {
    const ua = navigator.userAgent;
    return ua.indexOf('Chrome') 
}

window.resizeEditors = function (id) {
    //console.log('resize ' + id);
    var sourceE = document.getElementById(id);
    var height = `${window.innerHeight - 150}px`;
    var width = `${(window.innerWidth - 120) / 2}px`;

    //    console.log('h= ' + sourceE.style.height + ' w= ' +sourceE.style.width);
    sourceE.style.width = width;
    sourceE.style.height = height;
    //    console.log('h= ' + sourceE.style.height + ' w= ' + sourceE.style.width);
}

window.resizeEditor = function (id) {
    console.log('resize ' + id);
    var sourceE = document.getElementById(id);
    var height = `${window.innerHeight - 150}px`;
    var width = `${window.innerWidth - 120 }px`;

    //    console.log('h= ' + sourceE.style.height + ' w= ' +sourceE.style.width);
    sourceE.style.width = width;
    sourceE.style.height = height;
    //    console.log('h= ' + sourceE.style.height + ' w= ' + sourceE.style.width);
}

window.getWidth = function (id) {
    var el = document.getElementById(id);
    var sWidth = el.querySelector('.monaco-editor').style.width;
    var iWidth = parseInt(sWidth.substring(0, sWidth.length - 2));
    return (iWidth + 2) + 'px';
}
window.getHeight = function (id) {
    var el = document.getElementById(id);
    var sHeight = el.querySelector('.monaco-editor').style.height;
    var iHeight = parseInt(sHeight.substring(0, sHeight.length - 2));
    return (iHeight + 2 - 57) + 'px';
}

window.setSize = function(id, width,height) {
    document.getElementById(id).style.width = width;
    document.getElementById(id).style.height = height;
}
window.registerResize = function () {
    window.onresize = function () {
        //alert('Inside handler for resize event');
        resizeEditors('editor-source');
        resizeEditors('editor-target');
    }
}
window.registerResize1 = function () {
    window.onresize = function () {
        //alert('Inside handler for resize event');
        resizeEditor('editor-source');
    }
}

//https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm
function BlazorDownloadFile(filename, contentType, content) {
    // Blazor marshall byte[] to a base64 string, so we first need to convert the string (content) to a Uint8Array to create the File
    const data = base64DecToArr(content);

    // Create the URL
    const file = new File([data], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    var navigator = document.getElementById("navTop");
    navigator.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_blank";
    a.click();

    // We don't need to keep the url, let's release the memory
    // On Safari it seems you need to comment this line... (please let me know if you know why)
    URL.revokeObjectURL(exportUrl);
}

// Convert a base64 string to a Uint8Array. This is needed to create a blob object from the base64 string.
// The code comes from: https://developer.mozilla.org/fr/docs/Web/API/WindowBase64/D%C3%A9coder_encoder_en_base64
function b64ToUint6(nChr) {
    return nChr > 64 && nChr < 91 ? nChr - 65 : nChr > 96 && nChr < 123 ? nChr - 71 : nChr > 47 && nChr < 58 ? nChr + 4 : nChr === 43 ? 62 : nChr === 47 ? 63 : 0;
}

function base64DecToArr(sBase64, nBlocksSize) {
    var
        sB64Enc = sBase64.replace(/[^A-Za-z0-9\+\/]/g, ""),
        nInLen = sB64Enc.length,
        nOutLen = nBlocksSize ? Math.ceil((nInLen * 3 + 1 >> 2) / nBlocksSize) * nBlocksSize : nInLen * 3 + 1 >> 2,
        taBytes = new Uint8Array(nOutLen);

    for (var nMod3, nMod4, nUint24 = 0, nOutIdx = 0, nInIdx = 0; nInIdx < nInLen; nInIdx++) {
        nMod4 = nInIdx & 3;
        nUint24 |= b64ToUint6(sB64Enc.charCodeAt(nInIdx)) << 18 - 6 * nMod4;
        if (nMod4 === 3 || nInLen - nInIdx === 1) {
            for (nMod3 = 0; nMod3 < 3 && nOutIdx < nOutLen; nMod3++, nOutIdx++) {
                taBytes[nOutIdx] = nUint24 >>> (16 >>> nMod3 & 24) & 255;
            }
            nUint24 = 0;
        }
    }
    return taBytes;
}