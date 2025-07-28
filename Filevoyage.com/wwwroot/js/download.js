// wwwroot/js/download.js
(function () {
    const cnt = document.getElementById('countdown');
    const target = cnt ? cnt.getAttribute('data-target') : null;
    if (!cnt || !target) return;

    let s = 3;
    const iv = setInterval(() => {
        s -= 1;
        if (s <= 0) {
            clearInterval(iv);
            window.location.href = target;
        } else {
            cnt.textContent = String(s);
        }
    }, 1000);
})();
