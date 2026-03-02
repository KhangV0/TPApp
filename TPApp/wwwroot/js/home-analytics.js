/* home-analytics.js — Static hardcoded data, no DB required */
(function () {
    'use strict';

    // ── Static chart data ─────────────────────────────────────────
    const MONTHS = ['T10/25', 'T11/25', 'T12/25', 'T1/26', 'T2/26', 'T3/26'];
    const VISITS = [5200, 6100, 7400, 8200, 9000, 11500];
    const USERS = [1800, 2200, 2700, 3100, 3800, 4600];

    const SECTOR_LABELS = [
        'Điện, điện tử, CNTT',
        'Thực phẩm',
        'Môi trường',
        'Y tế',
        'CN sinh học',
        'Trồng trọt',
        'Khác'
    ];
    const SECTOR_VALUES = [30, 17, 10, 7, 5, 5, 26];
    const SECTOR_COLORS = ['#3b82f6', '#f59e0b', '#10b981', '#ef4444', '#8b5cf6', '#06b6d4', '#94a3b8'];

    const LEGEND_COLOR = '#334155';
    const TICK_COLOR = '#64748b';
    const GRID_COLOR = '#e5e7eb';
    const TOOLTIP_BG = '#1e293b';

    // ── Count-up animation ────────────────────────────────────────
    function easeOut(t) { return 1 - Math.pow(1 - t, 3); }
    function countUp(el) {
        var target = parseInt(el.dataset.target, 10);
        if (!target) return;
        var dur = 1400, start = performance.now();
        (function step(now) {
            var p = Math.min((now - start) / dur, 1);
            el.textContent = Math.round(easeOut(p) * target).toLocaleString('vi-VN');
            if (p < 1) requestAnimationFrame(step);
        })(start);
    }

    // ── Build charts ──────────────────────────────────────────────
    function buildCharts() {
        if (typeof Chart === 'undefined') return;

        // Count-up on all KPI values
        document.querySelectorAll('.js-cu[data-target]').forEach(countUp);

        // Line Chart
        var growthCtx = document.getElementById('tpGrowthChart');
        if (growthCtx && !growthCtx._chartBuilt) {
            growthCtx._chartBuilt = true;
            new Chart(growthCtx, {
                type: 'line',
                data: {
                    labels: MONTHS,
                    datasets: [
                        {
                            label: 'Lượt truy cập',
                            data: VISITS,
                            borderColor: '#3b82f6',
                            backgroundColor: 'rgba(59,130,246,.10)',
                            tension: .38, pointRadius: 3, borderWidth: 2.5, fill: true
                        },
                        {
                            label: 'Người dùng',
                            data: USERS,
                            borderColor: '#10b981',
                            backgroundColor: 'rgba(16,185,129,.10)',
                            tension: .38, pointRadius: 3, borderWidth: 2.5, fill: true
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    animation: { duration: 900, easing: 'easeOutQuart' },
                    interaction: { mode: 'index', intersect: false },
                    plugins: {
                        legend: { labels: { color: LEGEND_COLOR, usePointStyle: true, boxWidth: 8, padding: 12 } },
                        tooltip: {
                            backgroundColor: TOOLTIP_BG, padding: 10, cornerRadius: 8,
                            callbacks: { label: function (ctx) { return ' ' + ctx.dataset.label + ': ' + ctx.parsed.y.toLocaleString('vi-VN'); } }
                        }
                    },
                    scales: {
                        x: { ticks: { color: TICK_COLOR }, grid: { color: GRID_COLOR } },
                        y: { ticks: { color: TICK_COLOR }, grid: { color: GRID_COLOR }, beginAtZero: true }
                    }
                }
            });
        }

        // Doughnut Chart
        var typeCtx = document.getElementById('tpTypeChart');
        if (typeCtx && !typeCtx._chartBuilt) {
            typeCtx._chartBuilt = true;
            new Chart(typeCtx, {
                type: 'doughnut',
                data: {
                    labels: SECTOR_LABELS,
                    datasets: [{
                        data: SECTOR_VALUES,
                        backgroundColor: SECTOR_COLORS,
                        borderWidth: 2,
                        borderColor: '#fff',
                        hoverOffset: 8
                    }]
                },
                options: {
                    cutout: '52%',
                    animation: { duration: 900, easing: 'easeOutQuart' },
                    layout: { padding: 4 },
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                color: LEGEND_COLOR,
                                usePointStyle: true,
                                pointStyle: 'circle',
                                boxWidth: 8,
                                padding: 10,
                                font: { size: 10 }
                            }
                        },
                        tooltip: {
                            backgroundColor: TOOLTIP_BG, padding: 10, cornerRadius: 8,
                            callbacks: {
                                label: function (ctx) {
                                    return '  ' + ctx.label + ': ' + ctx.parsed + '%';
                                }
                            }
                        }
                    }
                }
            });
        }
    }

    // ── Run: try immediately, fallback to DOMContentLoaded & window.load ──
    function tryInit() {
        if (document.getElementById('tpGrowthChart') && document.getElementById('tpTypeChart')) {
            buildCharts();
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', tryInit);
    } else {
        tryInit(); // DOM already ready
    }
    // Extra safety: also run on window load (covers async layout shifts)
    window.addEventListener('load', tryInit);

})();
