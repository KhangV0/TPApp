/* home-analytics.js
 * Initialises Chart.js Line + Doughnut for the Home Analytics Section.
 * Data is read from data-* attributes on .tp-analytics to keep Razor clean.
 * No hard-coded numbers. No external dependencies beyond Chart.js.
 */
(function () {
    'use strict';

    // ── Shared defaults ──────────────────────────────────────────
    Chart.defaults.font.family = "'Inter', 'Segoe UI', system-ui, sans-serif";
    Chart.defaults.font.size = 11;
    Chart.defaults.color = '#94a3b8';

    const COLORS = {
        products: '#3b82f6',
        projects: '#10b981',
        suppliers: '#f59e0b',
        experts: '#8b5cf6',
        cn: '#3b82f6',
        tb: '#06b6d4',
        tt: '#8b5cf6',
    };

    // Light-mode chart palette
    const LEGEND_COLOR = '#334155';
    const TICK_COLOR = '#64748b';
    const GRID_COLOR = '#e5e7eb';
    const TOOLTIP_BG = '#1e293b';

    // ── Count-up animation ───────────────────────────────────────
    function easeOut(t) { return 1 - Math.pow(1 - t, 3); }
    function countUp(el) {
        const target = parseInt(el.dataset.target, 10);
        if (!target) return;
        const dur = 1500;
        const start = performance.now();
        (function step(now) {
            const p = Math.min((now - start) / dur, 1);
            el.textContent = Math.round(easeOut(p) * target).toLocaleString('vi-VN');
            if (p < 1) requestAnimationFrame(step);
        })(start);
    }

    // ── Init on intersection ─────────────────────────────────────
    const panel = document.querySelector('.tp-analytics');
    if (!panel) return;

    let chartsInitialised = false;

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (!entry.isIntersecting || chartsInitialised) return;
            chartsInitialised = true;
            observer.disconnect();

            // Trigger count-up on KPI values
            document.querySelectorAll('.js-cu[data-target]').forEach(countUp);

            // Parse JSON from data attributes
            let labels, growth, types;
            try {
                labels = JSON.parse(panel.dataset.labels || '[]');
                growth = JSON.parse(panel.dataset.growth || '{}');
                types = JSON.parse(panel.dataset.types || '{}');
            } catch (e) {
                console.warn('[HomeAnalytics] JSON parse error', e);
                return;
            }

            // ── 1. Line Chart — 6-month growth ────────────────────
            const growthCtx = document.getElementById('tpGrowthChart');
            if (growthCtx && labels.length) {
                new Chart(growthCtx, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [
                            { label: 'Sản phẩm', data: growth.products, borderColor: COLORS.products, tension: .35, pointRadius: 2.5, borderWidth: 2, fill: false },
                            { label: 'Dự án', data: growth.projects, borderColor: COLORS.projects, tension: .35, pointRadius: 2.5, borderWidth: 2, fill: false },
                            { label: 'Nhà cung ứng', data: growth.suppliers, borderColor: COLORS.suppliers, tension: .35, pointRadius: 2, borderWidth: 1.5, fill: false },
                            { label: 'Chuyên gia', data: growth.experts, borderColor: COLORS.experts, tension: .35, pointRadius: 2, borderWidth: 1.5, fill: false },
                        ]
                    },
                    options: {
                        responsive: true,
                        animation: { duration: 900, easing: 'easeOutQuart' },
                        interaction: { mode: 'index', intersect: false },
                        plugins: {
                            legend: { labels: { color: LEGEND_COLOR, usePointStyle: true, boxWidth: 8, padding: 12 } },
                            tooltip: { backgroundColor: TOOLTIP_BG, padding: 10, cornerRadius: 8 }
                        },
                        scales: {
                            x: { ticks: { color: TICK_COLOR }, grid: { color: GRID_COLOR } },
                            y: { ticks: { color: TICK_COLOR, stepSize: 2 }, grid: { color: GRID_COLOR }, beginAtZero: true }
                        }
                    }
                });
            }

            // ── 2. Doughnut — product type breakdown ───────────────
            const typeCtx = document.getElementById('tpTypeChart');
            if (typeCtx && types.labels && types.values) {
                new Chart(typeCtx, {
                    type: 'doughnut',
                    data: {
                        labels: types.labels,
                        datasets: [{
                            data: types.values,
                            backgroundColor: [COLORS.cn, COLORS.tb, COLORS.tt],
                            borderWidth: 0,
                            hoverOffset: 5
                        }]
                    },
                    options: {
                        cutout: '68%',
                        animation: { duration: 800 },
                        plugins: {
                            legend: { position: 'right', labels: { color: LEGEND_COLOR, usePointStyle: true, boxWidth: 8, padding: 10 } },
                            tooltip: { backgroundColor: TOOLTIP_BG, padding: 10, cornerRadius: 8 }
                        }
                    }
                });
            }
        });
    }, { threshold: 0.2 });

    observer.observe(panel);
})();
