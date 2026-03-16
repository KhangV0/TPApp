/**
 * home-analytics.js — Clickable doughnut + tabbed KPI count-up
 *
 * ═══════════════════════════════════════════════════════════════
 * HƯỚNG DẪN CẬP NHẬT SỐ LIỆU:
 *
 * Tất cả số liệu nằm trong file: /js/home-analytics-data.json
 * Chỉ cần sửa file JSON đó, không cần sửa code JS này.
 *
 * Cấu trúc file JSON:
 *   - kpiTotal[]      : 8 KPI cards tab "Dữ liệu tổng"
 *   - kpi2026[]       : 8 KPI cards tab "Dữ liệu 2026"
 *   - statBoxes[]     : 5 stat boxes dưới chart
 *   - doughnutChart.sectors[] : dữ liệu biểu đồ tròn
 *   - lastUpdated     : ngày cập nhật hiển thị
 * ═══════════════════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var LEGEND_COLOR = '#334155';
    var TOOLTIP_BG   = '#1e293b';
    var MainDomain   = document.querySelector('meta[name="main-domain"]')?.content || '/';

    // ── Count-up animation ────────────────────────────────────────
    function easeOut(t) { return 1 - Math.pow(1 - t, 3); }
    function countUp(el) {
        var target = parseInt(el.dataset.target, 10);
        if (!target && target !== 0) return;
        var dur = 1400, start = performance.now();
        (function step(now) {
            var p = Math.min((now - start) / dur, 1);
            el.textContent = Math.round(easeOut(p) * target).toLocaleString('vi-VN');
            if (p < 1) requestAnimationFrame(step);
        })(start);
    }

    // ── Load JSON data then build everything ──────────────────────
    function loadDataAndBuild() {
        fetch('/js/home-analytics-data.json?v=' + Date.now())
            .then(function (res) { return res.json(); })
            .then(function (data) {
                applyKpiData(data);
                applyStatBoxes(data);
                buildLineChart(data);
                buildDoughnutChart(data);
                setupTabCountUp();
            })
            .catch(function (err) {
                console.warn('[home-analytics] Không tải được data JSON, dùng giá trị mặc định trong HTML.', err);
                // Fallback: vẫn chạy count-up trên giá trị có sẵn trong HTML
                document.querySelectorAll('.js-cu[data-target]').forEach(countUp);
                setupTabCountUp();
            });
    }

    // ── Apply KPI cards data from JSON ────────────────────────────
    function applyKpiData(data) {
        // Tab "Dữ liệu tổng"
        var totalCards = document.querySelectorAll('#kpiTotal .tp-card');
        if (data.kpiTotal && totalCards.length) {
            data.kpiTotal.forEach(function (item, i) {
                if (totalCards[i]) {
                    var valEl = totalCards[i].querySelector('.tp-card__value');
                    var lblEl = totalCards[i].querySelector('.tp-card__label');
                    if (valEl) { valEl.dataset.target = item.value; }
                    if (lblEl) { lblEl.textContent = item.label; }
                }
            });
        }

        // Tab "Dữ liệu 2026"
        var cards2026 = document.querySelectorAll('#kpi2026 .tp-card');
        if (data.kpi2026 && cards2026.length) {
            data.kpi2026.forEach(function (item, i) {
                if (cards2026[i]) {
                    var valEl = cards2026[i].querySelector('.tp-card__value');
                    var lblEl = cards2026[i].querySelector('.tp-card__label');
                    if (valEl) { valEl.dataset.target = item.value; }
                    if (lblEl) { lblEl.textContent = item.label; }
                }
            });
        }

        // Ngày cập nhật
        if (data.lastUpdated) {
            var updEl = document.querySelector('.tp-updated');
            if (updEl) {
                updEl.innerHTML = '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg> Cập nhật: ' + data.lastUpdated;
            }
        }

        // Count-up cho tab active (tổng)
        document.querySelectorAll('#kpiTotal .js-cu[data-target]').forEach(countUp);
    }

    // ── Apply 5 stat boxes data from JSON ─────────────────────────
    function applyStatBoxes(data) {
        if (!data.statBoxes) return;
        var boxes = document.querySelectorAll('.tp-stat-boxes .tp-stat-box');
        data.statBoxes.forEach(function (item, i) {
            if (boxes[i]) {
                var valEl = boxes[i].querySelector('.tp-stat-box__val');
                var lblEl = boxes[i].querySelector('.tp-stat-box__lbl');
                if (valEl) {
                    valEl.dataset.target = item.value;
                    countUp(valEl);
                }
                if (lblEl) { lblEl.textContent = item.label; }
                // Update link từ JSON
                if (item.link) {
                    boxes[i].setAttribute('href', item.link);
                }
            }
        });
    }

    // ── Build line chart from JSON ─────────────────────────────────
    function buildLineChart(data) {
        if (typeof Chart === 'undefined') return;
        var lineCtx = document.getElementById('tpLineChart');
        if (!lineCtx || lineCtx._chartBuilt) return;
        if (!data.lineChart || !data.lineChart.labels || !data.lineChart.datasets) return;

        lineCtx._chartBuilt = true;

        var datasets = data.lineChart.datasets.map(function (ds, idx) {
            return {
                label: ds.label,
                data: ds.data,
                borderColor: ds.color,
                backgroundColor: ds.color + '18',
                borderWidth: 2.5,
                pointRadius: 4,
                pointHoverRadius: 7,
                pointBackgroundColor: '#fff',
                pointBorderColor: ds.color,
                pointBorderWidth: 2,
                pointHoverBorderWidth: 3,
                tension: 0.35,
                fill: true
            };
        });

        new Chart(lineCtx, {
            type: 'line',
            data: {
                labels: data.lineChart.labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                animation: { duration: 1000, easing: 'easeOutQuart' },
                layout: { padding: { top: 4, right: 8 } },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.05)', drawBorder: false },
                        ticks: {
                            color: '#64748b',
                            font: { size: 10 },
                            callback: function (val) {
                                if (val >= 1000) return (val / 1000).toFixed(0) + 'K';
                                return val;
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: LEGEND_COLOR,
                            usePointStyle: true,
                            pointStyle: 'circle',
                            boxWidth: 8,
                            padding: 12,
                            font: { size: 11 }
                        }
                    },
                    tooltip: {
                        backgroundColor: TOOLTIP_BG,
                        padding: 10,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx) {
                                return '  ' + ctx.dataset.label + ': ' + ctx.parsed.y.toLocaleString('vi-VN');
                            }
                        }
                    }
                }
            }
        });
    }

    // ── Build doughnut chart from JSON ────────────────────────────
    function buildDoughnutChart(data) {
        if (typeof Chart === 'undefined') return;
        var typeCtx = document.getElementById('tpTypeChart');
        if (!typeCtx || typeCtx._chartBuilt) return;
        if (!data.doughnutChart || !data.doughnutChart.sectors) return;

        var sectors = data.doughnutChart.sectors;
        var labels  = sectors.map(function (s) { return s.label; });
        var values  = sectors.map(function (s) { return s.value; });
        var colors  = sectors.map(function (s) { return s.color; });
        var urls    = sectors.map(function (s) { return s.url; });

        typeCtx._chartBuilt = true;
        new Chart(typeCtx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#fff',
                    hoverOffset: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '52%',
                animation: { duration: 900, easing: 'easeOutQuart' },
                layout: { padding: 4 },
                plugins: {
                    legend: {
                        position: 'bottom',
                        align: 'center',
                        labels: {
                            color: LEGEND_COLOR,
                            usePointStyle: true,
                            pointStyle: 'circle',
                            boxWidth: 8,
                            padding: 8,
                            font: { size: 11 }
                        }
                    },
                    tooltip: {
                        backgroundColor: TOOLTIP_BG, padding: 10, cornerRadius: 8,
                        callbacks: {
                            label: function (ctx) {
                                return '  ' + ctx.label + ': ' + ctx.parsed + '%';
                            },
                            afterLabel: function () {
                                return '  👆 Click để xem chi tiết';
                            }
                        }
                    }
                },
                onHover: function (event, elements) {
                    event.native.target.style.cursor = elements.length > 0 ? 'pointer' : 'default';
                },
                onClick: function (event, elements) {
                    if (elements.length > 0) {
                        var index = elements[0].index;
                        var url = MainDomain + urls[index];
                        window.open(url, '_blank');
                    }
                }
            }
        });
    }

    // ── Tab switch: trigger count-up ──────────────────────────────
    function setupTabCountUp() {
        var tab2026Btn = document.querySelector('[data-bs-target="#kpi2026"]');
        if (tab2026Btn) {
            tab2026Btn.addEventListener('shown.bs.tab', function () {
                document.querySelectorAll('#kpi2026 .js-cu-2026[data-target]').forEach(countUp);
            });
        }
        var tabTotalBtn = document.querySelector('[data-bs-target="#kpiTotal"]');
        if (tabTotalBtn) {
            tabTotalBtn.addEventListener('shown.bs.tab', function () {
                document.querySelectorAll('#kpiTotal .js-cu[data-target]').forEach(countUp);
            });
        }
    }

    // ── Init ──────────────────────────────────────────────────────
    function tryInit() {
        if (document.getElementById('tpTypeChart') || document.getElementById('tpLineChart')) {
            loadDataAndBuild();
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', tryInit);
    } else {
        tryInit();
    }
    window.addEventListener('load', tryInit);

})();
