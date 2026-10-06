document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('themeToggle');
    const html = document.documentElement;
    const savedTheme = localStorage.getItem('sao-theme');

    if (savedTheme) {
        html.setAttribute('data-theme', savedTheme);
    }

    if (toggle) {
        toggle.addEventListener('click', function () {
            const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-theme', next);
            localStorage.setItem('sao-theme', next);
        });
    }

    document.querySelectorAll('[data-count]').forEach(function (element) {
        const target = Number(element.getAttribute('data-count') || 0);
        const duration = 700;
        const start = performance.now();

        function update(now) {
            const progress = Math.min((now - start) / duration, 1);
            const value = Math.round(target * progress);
            element.textContent = value.toLocaleString();
            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                element.textContent = target.toLocaleString();
            }
        }

        requestAnimationFrame(update);
    });
});
