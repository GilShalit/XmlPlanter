window.WindowInerrHeight = function () {
    return window.innerHeight;
}
window.WindowInnerWidth = function () {
    return window.innerWidth;
}
window.resizeEditors = function (id) {
    let wh = window.innerHeight;
    var sourceE = document.getElementById(id);
    var height = `${window.innerHeight - 150}px`;
    sourceE.style.height = height;
    return height
}