// Theme toggle (light / dark) + copy-to-clipboard helper.
(function () {
    var root = document.documentElement;

    function applyTheme(t) {
        root.setAttribute('data-theme', t);
        root.setAttribute('data-bs-theme', t);
        try { localStorage.setItem('dpp-theme', t); } catch (e) { }
    }

    var toggle = document.getElementById('themeToggle');
    if (toggle) {
        toggle.addEventListener('click', function () {
            applyTheme(root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark');
        });
    }

    // <button data-copy-target="#id" data-copied="Copied!"> copies the value of the element with that id.
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-copy-target]');
        if (!btn) return;
        var src = document.querySelector(btn.getAttribute('data-copy-target'));
        if (!src) return;
        var text = src.value !== undefined ? src.value : src.textContent;
        var label = btn.querySelector('[data-copy-label]') || btn;
        var original = label.textContent;

        function done() {
            label.textContent = btn.getAttribute('data-copied') || 'Copied';
            btn.classList.add('is-copied');
            setTimeout(function () { label.textContent = original; btn.classList.remove('is-copied'); }, 1800);
        }
        function fallback() {
            try {
                src.focus(); src.select && src.select();
                document.execCommand('copy');
                done();
            } catch (err) { }
        }

        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(text).then(done, fallback);
        } else {
            fallback();
        }
    });
})();
