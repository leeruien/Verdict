"""
Verdict – System Architecture Diagram
Run:  python3 architecture_diagram.py
Output: architecture_diagram.png
Requires: pip install graphviz  +  brew install graphviz
"""

from graphviz import Digraph

g = Digraph(
    "VerdictArchitecture",
    filename="architecture_diagram",
    format="png",
    graph_attr={
        "rankdir": "TB",
        "splines": "ortho",
        "nodesep": "0.7",
        "ranksep": "1.0",
        "fontname": "Helvetica",
        "bgcolor": "#F8F9FC",
        "pad": "0.5",
        "label": "Verdict – System Architecture",
        "labelloc": "t",
        "fontsize": "20",
        "fontcolor": "#052767",
    },
    node_attr={"fontname": "Helvetica", "fontsize": "11"},
    edge_attr={"fontname": "Helvetica", "fontsize": "9", "color": "#555555"},
)

# ── Styles ──────────────────────────────────────────────────────────────────

def box(label, sublabel="", color="#DDEEFF", border="#4477AA", text="#052767", shape="box"):
    full = f"{label}\n{sublabel}" if sublabel else label
    return dict(
        label=full,
        shape=shape,
        style="filled,rounded",
        fillcolor=color,
        color=border,
        fontcolor=text,
        penwidth="1.5",
    )

def ext(label, sublabel=""):
    return box(label, sublabel, color="#FFF3E0", border="#E65100", text="#BF360C")

def db_node(label, sublabel=""):
    return box(label, sublabel, color="#E8F5E9", border="#2E7D32", text="#1B5E20", shape="cylinder")

def svc(label, sublabel=""):
    return box(label, sublabel, color="#F3E5F5", border="#6A1B9A", text="#4A148C")

def page(label, sublabel=""):
    return box(label, sublabel, color="#E3F2FD", border="#1565C0", text="#0D47A1")

def layer_label(text):
    return dict(
        label=text,
        shape="plaintext",
        fontcolor="#888888",
        fontsize="10",
        style="",
    )

# ── CLIENT TIER ─────────────────────────────────────────────────────────────

with g.subgraph(name="cluster_client") as c:
    c.attr(
        label="Client", style="rounded,filled", fillcolor="#EEF2FF",
        color="#7986CB", fontcolor="#3949AB", fontsize="13",
    )
    c.node("browser", **box("Web Browser", "HTTP/HTTPS + WebSocket (SignalR)",
                             color="#C5CAE9", border="#3949AB", text="#1A237E"))

# ── BLAZOR SERVER TIER ──────────────────────────────────────────────────────

with g.subgraph(name="cluster_blazor") as b:
    b.attr(
        label="Blazor Server  (.NET 10)",
        style="rounded,filled", fillcolor="#E8EAF6",
        color="#3949AB", fontcolor="#1A237E", fontsize="13",
    )

    # Pages sub-cluster
    with b.subgraph(name="cluster_pages") as p:
        p.attr(label="Pages / Components", style="rounded,dashed",
               color="#7986CB", fontcolor="#3949AB", fontsize="11")
        p.node("pages_auth",    **page("Auth Pages",     "Login · Register\nForgotPassword · ResetPassword"))
        p.node("pages_content", **page("Content Pages",  "Home · DilemmaDetail\nGroup · Search"))
        p.node("pages_social",  **page("Social Pages",   "Profile · UserProfile\nInbox · Notifications"))
        p.node("pages_admin",   **page("Admin Pages",    "FounderReports"))
        p.node("pages_settings",**page("Settings",       "Profile · Privacy\nSecurity · Account"))

    # Services sub-cluster
    with b.subgraph(name="cluster_services") as s:
        s.attr(label="Services", style="rounded,dashed",
               color="#9C27B0", fontcolor="#6A1B9A", fontsize="11")
        s.node("svc_notif",   **svc("NotificationService",       "Vote · Comment · Removal · NewPost"))
        s.node("svc_expiry",  **svc("ExpiryNotificationService", "Background – 30 min poll"))
        s.node("svc_email",   **svc("EmailSender",               "SMTP – confirm · password"))
        s.node("svc_supabase",**svc("SupabaseAuthService",       "Signup · Reset · Token verify"))
        s.node("svc_founder", **svc("FounderService",            "IsFounder() check"))
        s.node("svc_badge",   **svc("BadgeNotifier",             "Singleton – unread count"))
        s.node("svc_recents", **svc("RecentGroupsNotifier",      "Singleton – sidebar groups"))
        s.node("svc_cache",   **svc("ResetSignInCache",          "One-time token store"))

    # Identity + EF
    with b.subgraph(name="cluster_data") as d:
        d.attr(label="Data Access", style="rounded,dashed",
               color="#F57F17", fontcolor="#E65100", fontsize="11")
        d.node("identity",  **box("ASP.NET Identity",  "Auth · Lockout · Password hash",
                                   color="#FFF9C4", border="#F9A825", text="#5D4037"))
        d.node("efcore",    **box("Entity Framework Core 10", "Migrations · LINQ queries",
                                   color="#FFF9C4", border="#F9A825", text="#5D4037"))

# ── EXTERNAL SERVICES TIER ──────────────────────────────────────────────────

with g.subgraph(name="cluster_external") as e:
    e.attr(
        label="External Services",
        style="rounded,filled", fillcolor="#FBE9E7",
        color="#BF360C", fontcolor="#BF360C", fontsize="13",
    )

    e.node("supabase_auth", **ext("Supabase Auth",   "/auth/v1 REST API\nSignup · Confirm · Reset"))
    e.node("supabase_db",   **db_node("Supabase PostgreSQL", "Primary data store\nEF Core migrations"))
    e.node("smtp",          **ext("SMTP Server",     "Transactional email\nConfirmation · Password"))
    e.node("filesystem",    **ext("Local Filesystem","wwwroot/uploads/avatars\nProfile photos"))

# ── EDGES: Client ↔ Blazor ──────────────────────────────────────────────────

g.edge("browser", "pages_auth",
       label="HTTPS + SignalR", style="bold", color="#3949AB", fontcolor="#3949AB")
g.edge("browser", "pages_content",  style="bold", color="#3949AB")
g.edge("browser", "pages_social",   style="bold", color="#3949AB")
g.edge("browser", "pages_settings", style="bold", color="#3949AB")
g.edge("browser", "pages_admin",    style="bold", color="#3949AB")

# ── EDGES: Pages → Services ─────────────────────────────────────────────────

g.edge("pages_content",  "svc_notif",   style="dashed", color="#7986CB")
g.edge("pages_social",   "svc_notif",   style="dashed", color="#7986CB")
g.edge("pages_admin",    "svc_notif",   style="dashed", color="#7986CB")
g.edge("pages_auth",     "svc_supabase",style="dashed", color="#7986CB")
g.edge("pages_settings", "svc_supabase",style="dashed", color="#7986CB")
g.edge("pages_auth",     "svc_email",   style="dashed", color="#7986CB")
g.edge("pages_settings", "svc_email",   style="dashed", color="#7986CB")
g.edge("pages_content",  "svc_badge",   style="dashed", color="#7986CB")
g.edge("pages_social",   "svc_badge",   style="dashed", color="#7986CB")
g.edge("pages_content",  "svc_recents", style="dashed", color="#7986CB")

# ── EDGES: Pages / Services → Data Access ───────────────────────────────────

g.edge("pages_auth",     "identity", style="dashed", color="#F9A825")
g.edge("pages_content",  "efcore",   style="dashed", color="#F9A825")
g.edge("pages_social",   "efcore",   style="dashed", color="#F9A825")
g.edge("pages_admin",    "efcore",   style="dashed", color="#F9A825")
g.edge("pages_settings", "efcore",   style="dashed", color="#F9A825")
g.edge("svc_notif",      "efcore",   style="dashed", color="#F9A825")
g.edge("svc_expiry",     "efcore",   style="dashed", color="#F9A825")
g.edge("identity",       "efcore",   color="#F9A825", style="bold")

# ── EDGES: Data Access → External ───────────────────────────────────────────

g.edge("efcore",         "supabase_db",   label="EF Core / Npgsql",
       style="bold", color="#2E7D32", fontcolor="#2E7D32")
g.edge("svc_supabase",   "supabase_auth", label="HTTP REST",
       style="bold", color="#BF360C", fontcolor="#BF360C")
g.edge("svc_email",      "smtp",          label="SMTP",
       style="bold", color="#E65100", fontcolor="#E65100")
g.edge("pages_settings", "filesystem",    label="File upload",
       style="bold", color="#E65100", fontcolor="#E65100")

# ── LEGEND ───────────────────────────────────────────────────────────────────

with g.subgraph(name="cluster_legend") as l:
    l.attr(label="Legend", style="rounded,filled", fillcolor="#FFFFFF",
           color="#AAAAAA", fontsize="11", fontcolor="#555555")
    l.node("leg1", **page("Page / Component"))
    l.node("leg2", **svc("Service"))
    l.node("leg3", **ext("External / 3rd party"))
    l.node("leg4", **db_node("Database"))
    l.node("leg_e1", shape="plaintext", label="──────  Data flow (sync)",  fontcolor="#555555", fontsize="10")
    l.node("leg_e2", shape="plaintext", label="- - - -  Internal call",    fontcolor="#555555", fontsize="10")

g.render(view=False, cleanup=False)
print("✅  architecture_diagram.png generated successfully.")
