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

    window.toggleSidebar = toggleSidebar;

    // Initial load
    document.addEventListener('DOMContentLoaded', restoreState);
    // Blazor enhanced navigation
    document.addEventListener('enhancedload', restoreState);
})();
