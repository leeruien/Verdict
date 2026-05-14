(function () {
    function applyState(collapsed) {
        var sidebar = document.querySelector('.sidebar');
        var btn = document.querySelector('.sidebar-toggle');
        if (!sidebar) return;
        sidebar.classList.toggle('collapsed', collapsed);
        if (btn) btn.setAttribute('aria-label', collapsed ? 'Expand sidebar' : 'Collapse sidebar');
    }

    function toggleSidebar() {
        var sidebar = document.querySelector('.sidebar');
        if (!sidebar) return;
        var next = !sidebar.classList.contains('collapsed');
        applyState(next);
        localStorage.setItem('sidebarCollapsed', next ? '1' : '0');
    }

    function restoreState() {
        applyState(localStorage.getItem('sidebarCollapsed') === '1');
    }

    function addRecentGroup(slug, name, icon) {
        var recents = JSON.parse(localStorage.getItem('recentGroups') || '[]');
        // Remove existing entry so it moves to front instead of duplicating
        recents = recents.filter(function (g) { return g.slug !== slug; });
        // Push to front, then evict the oldest beyond 3
        recents.unshift({ slug: slug, name: name, icon: icon });
        recents = recents.slice(0, 3);
        localStorage.setItem('recentGroups', JSON.stringify(recents));
    }

    function getRecentGroups() {
        // Slice defensively in case of stale data from a previous format
        return JSON.parse(localStorage.getItem('recentGroups') || '[]').slice(0, 3);
    }

    window.toggleSidebar = toggleSidebar;
    window.addRecentGroup = addRecentGroup;
    window.getRecentGroups = getRecentGroups;

    // Initial load
    document.addEventListener('DOMContentLoaded', restoreState);
    // Blazor enhanced navigation
    document.addEventListener('enhancedload', restoreState);
})();
