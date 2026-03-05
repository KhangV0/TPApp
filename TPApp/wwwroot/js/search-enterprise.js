/* ═══════════════════════════════════════════════════
   search-enterprise.js   –   Techport Enterprise Search
   AJAX tab switching, sort, pagination, pushState
   ═══════════════════════════════════════════════════ */
(function () {
    'use strict';

    const HOST = '#resultsHost';
    const SKEL_COUNT = 4;

    // ── Current state ──────────────────────────────
    let state = {
        q: '',
        type: 'all',
        sort: 'relevance',
        page: 1
    };

    // ── Init ───────────────────────────────────────
    function init() {
        // Read initial state from URL
        const params = new URLSearchParams(window.location.search);
        state.q = params.get('q') || '';
        state.type = params.get('type') || 'all';
        state.sort = params.get('sort') || 'relevance';
        state.page = parseInt(params.get('page')) || 1;

        bindEvents();
    }

    // ── Event binding ──────────────────────────────
    function bindEvents() {
        // Type filter pills
        document.addEventListener('click', function (e) {
            const pill = e.target.closest('.se-filter-pill[data-type]');
            if (pill) {
                e.preventDefault();
                state.type = pill.dataset.type;
                state.page = 1;
                loadResults();
            }
        });

        // Sort dropdown
        const sortSel = document.getElementById('seSort');
        if (sortSel) {
            sortSel.addEventListener('change', function () {
                state.sort = this.value;
                state.page = 1;
                loadResults();
            });
        }

        // Pagination (delegated)
        document.addEventListener('click', function (e) {
            const btn = e.target.closest('.se-page-btn[data-page]');
            if (btn && !btn.classList.contains('disabled') && !btn.classList.contains('active')) {
                e.preventDefault();
                state.page = parseInt(btn.dataset.page);
                loadResults();
            }
        });

        // Browser back / forward
        window.addEventListener('popstate', function () {
            const params = new URLSearchParams(window.location.search);
            state.q = params.get('q') || '';
            state.type = params.get('type') || 'all';
            state.sort = params.get('sort') || 'relevance';
            state.page = parseInt(params.get('page')) || 1;
            loadResults(true); // skip pushState
        });
    }

    // ── Load results via AJAX ──────────────────────
    function loadResults(skipPush) {
        if (!state.q) return;

        const host = document.querySelector(HOST);
        if (!host) return;

        // Show skeleton
        host.innerHTML = buildSkeleton();

        // Update URL
        if (!skipPush) {
            const url = buildUrl('/Search/Index');
            history.pushState(null, '', url);
        }

        // Fetch partial
        const partialUrl = buildUrl('/Search/ResultsPartial');
        fetch(partialUrl)
            .then(r => { if (!r.ok) throw new Error(r.status); return r.text(); })
            .then(html => {
                host.innerHTML = html;
                updateActivePill();
                updateSortDropdown();
                scrollToTop();
            })
            .catch(err => {
                console.error('Search AJAX error:', err);
                host.innerHTML = '<div class="se-empty"><i class="bi bi-exclamation-triangle"></i>'
                    + '<h5>Không thể tải kết quả</h5><p>Vui lòng thử lại.</p></div>';
            });
    }

    // ── URL builder ────────────────────────────────
    function buildUrl(base) {
        const p = new URLSearchParams();
        p.set('q', state.q);
        if (state.type !== 'all') p.set('type', state.type);
        if (state.sort !== 'relevance') p.set('sort', state.sort);
        if (state.page > 1) p.set('page', state.page);
        return base + '?' + p.toString();
    }

    // ── UI updates ─────────────────────────────────
    function updateActivePill() {
        document.querySelectorAll('.se-filter-pill[data-type]').forEach(pill => {
            pill.classList.toggle('active', pill.dataset.type === state.type);
        });
    }

    function updateSortDropdown() {
        const sel = document.getElementById('seSort');
        if (sel) sel.value = state.sort;
    }

    function scrollToTop() {
        const host = document.querySelector(HOST);
        if (host) {
            host.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    // ── Skeleton builder ───────────────────────────
    function buildSkeleton() {
        let html = '<div class="se-skeleton">';
        for (let i = 0; i < SKEL_COUNT; i++) {
            html += `
                <div class="se-skel-card">
                    <div class="se-skel-icon"></div>
                    <div class="se-skel-body">
                        <div class="se-skel-line title"></div>
                        <div class="se-skel-line w100"></div>
                        <div class="se-skel-line w75"></div>
                        <div class="se-skel-chips">
                            <div class="se-skel-chip"></div>
                            <div class="se-skel-chip"></div>
                            <div class="se-skel-chip"></div>
                        </div>
                    </div>
                </div>`;
        }
        html += '</div>';
        return html;
    }

    // ── Public API (for inline onclick if needed) ──
    window.seSearch = { loadResults, state };

    // ── Start ──────────────────────────────────────
    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', init);
    else
        init();
})();
