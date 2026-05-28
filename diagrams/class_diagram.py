"""
Verdict – Class Diagram Generator
Run:  python class_diagram.py
Output: class_diagram.png  (also class_diagram.pdf alongside it)
Requires: pip install graphviz
          graphviz binary: brew install graphviz  (macOS)
"""

from graphviz import Digraph

g = Digraph(
    "Verdict",
    filename="class_diagram",
    format="png",
    graph_attr={
        "rankdir": "LR",
        "splines": "ortho",
        "nodesep": "0.6",
        "ranksep": "1.2",
        "fontname": "Helvetica",
        "bgcolor": "white",
    },
    node_attr={
        "fontname": "Helvetica",
        "fontsize": "11",
        "shape": "none",
        "margin": "0",
    },
    edge_attr={
        "fontname": "Helvetica",
        "fontsize": "9",
    },
)

# ── Helpers ────────────────────────────────────────────────────────────────

def uml_class(name, fields, stereotype=None, color="#DDEEFF", header_color="#4477AA"):
    """Return an HTML-like label for a UML class box."""
    stereo = f'<BR/><FONT POINT-SIZE="9"><I>&lt;&lt;{stereotype}&gt;&gt;</I></FONT>' if stereotype else ""
    rows = "".join(
        f'<TR><TD ALIGN="LEFT" BALIGN="LEFT">'
        f'<FONT COLOR="#333333">{f}</FONT></TD></TR>'
        for f in fields
    )
    return (
        f'<<TABLE BORDER="0" CELLBORDER="1" CELLSPACING="0" CELLPADDING="4">'
        f'<TR><TD BGCOLOR="{header_color}"><FONT COLOR="white"><B>{name}</B>{stereo}</FONT></TD></TR>'
        f'{rows}'
        f'</TABLE>>'
    )

def enum_class(name, values):
    rows = "".join(
        f'<TR><TD ALIGN="LEFT"><FONT COLOR="#333333">{v}</FONT></TD></TR>'
        for v in values
    )
    return (
        f'<<TABLE BORDER="0" CELLBORDER="1" CELLSPACING="0" CELLPADDING="4">'
        f'<TR><TD BGCOLOR="#AA7744"><FONT COLOR="white"><B>{name}</B>'
        f'<BR/><FONT POINT-SIZE="9"><I>&lt;&lt;enum&gt;&gt;</I></FONT></FONT></TD></TR>'
        f'{rows}'
        f'</TABLE>>'
    )

def service_class(name, methods, color="#DDFFD8", header_color="#2E7D32"):
    rows = "".join(
        f'<TR><TD ALIGN="LEFT"><FONT COLOR="#333333">{m}</FONT></TD></TR>'
        for m in methods
    )
    return (
        f'<<TABLE BORDER="0" CELLBORDER="1" CELLSPACING="0" CELLPADDING="4">'
        f'<TR><TD BGCOLOR="{header_color}"><FONT COLOR="white"><B>{name}</B>'
        f'<BR/><FONT POINT-SIZE="9"><I>&lt;&lt;service&gt;&gt;</I></FONT></FONT></TD></TR>'
        f'{rows}'
        f'</TABLE>>'
    )

# ── Models cluster ─────────────────────────────────────────────────────────

with g.subgraph(name="cluster_models") as m:
    m.attr(label="Models", style="rounded,dashed", color="#AAAACC", fontcolor="#444444")

    m.node("ApplicationUser", uml_class("ApplicationUser", [
        "+ Id : string",
        "+ UserName : string",
        "+ Email : string",
        "+ DisplayName : string?",
        "+ Bio : string?",
        "+ ProfilePhotoPath : string?",
        "+ HidePostsFromProfile : bool",
        "+ HideCommentsFromProfile : bool",
    ], stereotype="IdentityUser"))

    m.node("Dilemma", uml_class("Dilemma", [
        "+ Id : Guid",
        "+ UserId : string",
        "+ Title : string",
        "+ Description : string",
        "+ Category : string",
        "+ ExpiresAt : DateTime?",
        "+ CreatedAt : DateTime",
        "+ ImagePath : string?",
        "+ ImagePaths : string?",
    ]))

    m.node("DilemmaOption", uml_class("DilemmaOption", [
        "+ Id : Guid",
        "+ DilemmaId : Guid",
        "+ OptionText : string",
    ]))

    m.node("Vote", uml_class("Vote", [
        "+ Id : Guid",
        "+ UserId : string",
        "+ DilemmaOptionId : Guid",
        "+ CreatedAt : DateTime",
    ]))

    m.node("Comment", uml_class("Comment", [
        "+ Id : Guid",
        "+ UserId : string",
        "+ DilemmaId : Guid",
        "+ Body : string",
        "+ CreatedAt : DateTime",
        "+ EditedAt : DateTime?",
        "+ ParentCommentId : Guid?",
    ]))

    m.node("Notification", uml_class("Notification", [
        "+ Id : Guid",
        "+ RecipientUserId : string",
        "+ Type : NotificationType",
        "+ Message : string",
        "+ DilemmaId : Guid?",
        "+ IsRead : bool",
        "+ CreatedAt : DateTime",
    ]))

    m.node("NotificationType", enum_class("NotificationType", [
        "Vote",
        "Comment",
        "ExpiringSoon",
        "NewPost",
        "ContentRemoved",
    ]))

    m.node("CategorySubscription", uml_class("CategorySubscription", [
        "+ Id : Guid",
        "+ UserId : string",
        "+ Category : string",
        "+ SubscribedAt : DateTime",
    ]))

    m.node("Conversation", uml_class("Conversation", [
        "+ Id : Guid",
        "+ InitiatorId : string",
        "+ RecipientId : string",
        "+ Status : ConversationStatus",
        "+ CreatedAt : DateTime",
    ]))

    m.node("ConversationStatus", enum_class("ConversationStatus", [
        "Pending",
        "Accepted",
        "Rejected",
    ]))

    m.node("DirectMessage", uml_class("DirectMessage", [
        "+ Id : Guid",
        "+ ConversationId : Guid",
        "+ SenderId : string",
        "+ Body : string",
        "+ SentAt : DateTime",
    ]))

    m.node("Community", uml_class("Community", [
        "+ Id : Guid",
        "+ Name : string",
        "+ Slug : string",
        "+ Description : string",
        "+ Icon : string",
        "+ CreatedByUserId : string",
        "+ CreatedAt : DateTime",
    ]))

    m.node("Report", uml_class("Report", [
        "+ Id : Guid",
        "+ ReporterUserId : string",
        "+ DilemmaId : Guid?",
        "+ CommentId : Guid?",
        "+ Reason : string",
        "+ CreatedAt : DateTime",
    ]))

    m.node("Draft", uml_class("Draft", [
        "+ Id : Guid",
        "+ UserId : string",
        "+ Title : string",
        "+ Description : string",
        "+ Category : string",
        "+ ExpiresAtHours : int",
        "+ OptionsJson : string",
        "+ SavedAt : DateTime",
    ]))

    m.node("PendingRegistration", uml_class("PendingRegistration", [
        "+ Id : Guid",
        "+ Email : string",
        "+ DisplayName : string",
        "+ PasswordHash : string",
        "+ CreatedAt : DateTime",
        "+ ExpiresAt : DateTime",
    ]))

# ── Services cluster ───────────────────────────────────────────────────────

with g.subgraph(name="cluster_services") as s:
    s.attr(label="Services", style="rounded,dashed", color="#88AA88", fontcolor="#444444")

    s.node("NotificationService", service_class("NotificationService", [
        "+ NotifyVoteAsync()",
        "+ NotifyCommentAsync()",
        "+ NotifyNewPostAsync()",
        "+ NotifyPostRemovedAsync()",
        "+ NotifyCommentRemovedAsync()",
    ]))

    s.node("ExpiryNotificationService", service_class("ExpiryNotificationService", [
        "# ExecuteAsync()  [BackgroundService]",
        "– CheckExpiringDilemmasAsync()",
    ]))

    s.node("SupabaseAuthService", service_class("SupabaseAuthService", [
        "+ SignUpAsync()",
        "+ ResendConfirmationEmailAsync()",
        "+ VerifyTokenHashAsync()",
        "+ SendPasswordResetEmailAsync()",
        "+ VerifyRecoveryAndUpdatePasswordAsync()",
        "+ UpdatePasswordAsync()",
        "+ DeleteUserAsync()",
    ]))

    s.node("EmailSender", service_class("EmailSender", [
        "+ SendConfirmationLinkAsync()",
        "+ SendPasswordResetLinkAsync()",
        "+ SendPasswordChangedAsync()",
    ]))

    s.node("FounderService", service_class("FounderService", [
        "+ IsFounder(email) : bool",
    ]))

    s.node("BadgeNotifier", service_class("BadgeNotifier", [
        "+ NotifyAsync()",
        "+ Subscribe() / Unsubscribe()",
    ]))

    s.node("RecentGroupsNotifier", service_class("RecentGroupsNotifier", [
        "+ NotifyVisitedAsync()",
        "+ Subscribe() / Unsubscribe()",
    ]))

    s.node("ResetSignInCache", service_class("ResetSignInCache", [
        "+ Store(token, email)",
        "+ Consume(token) : string?",
    ]))

# ── Data cluster ───────────────────────────────────────────────────────────

with g.subgraph(name="cluster_data") as d:
    d.attr(label="Data", style="rounded,dashed", color="#CCAA44", fontcolor="#444444")

    d.node("ApplicationDbContext", uml_class("ApplicationDbContext", [
        "+ Dilemmas",
        "+ DilemmaOptions",
        "+ Votes",
        "+ Comments",
        "+ Notifications",
        "+ CategorySubscriptions",
        "+ Conversations",
        "+ DirectMessages",
        "+ Communities",
        "+ Reports",
        "+ Drafts",
        "+ PendingRegistrations",
    ], stereotype="IdentityDbContext", header_color="#996600"))

# ── Relationships ──────────────────────────────────────────────────────────

COMP  = dict(arrowhead="diamond", arrowtail="none", dir="both", color="#333333")
ASSOC = dict(arrowhead="open",    arrowtail="none", color="#666666", style="dashed")
USES  = dict(arrowhead="vee",     arrowtail="none", color="#888888", style="dashed")
INH   = dict(arrowhead="empty",   arrowtail="none", color="#444444")

# Model → Model
g.edge("ApplicationUser", "Dilemma",             label="1..*", **COMP)
g.edge("Dilemma",         "DilemmaOption",        label="1..*", **COMP)
g.edge("DilemmaOption",   "Vote",                 label="0..*", **COMP)
g.edge("Vote",            "ApplicationUser",      label="*..1", **ASSOC)
g.edge("Dilemma",         "Comment",              label="0..*", **COMP)
g.edge("Comment",         "ApplicationUser",      label="*..1", **ASSOC)
g.edge("Comment",         "Comment",              label="replies", **ASSOC)
g.edge("Notification",    "ApplicationUser",      label="*..1", **ASSOC)
g.edge("Notification",    "Dilemma",              label="0..1", **ASSOC)
g.edge("Notification",    "NotificationType",     label="type", **ASSOC)
g.edge("CategorySubscription", "ApplicationUser", label="*..1", **ASSOC)
g.edge("Conversation",    "ApplicationUser",      label="initiator/recipient", **ASSOC)
g.edge("Conversation",    "ConversationStatus",   label="status", **ASSOC)
g.edge("DirectMessage",   "Conversation",         label="*..1", **COMP)
g.edge("DirectMessage",   "ApplicationUser",      label="sender", **ASSOC)
g.edge("Community",       "ApplicationUser",      label="createdBy", **ASSOC)
g.edge("Report",          "ApplicationUser",      label="reporter", **ASSOC)
g.edge("Report",          "Dilemma",              label="0..1", **ASSOC)
g.edge("Report",          "Comment",              label="0..1", **ASSOC)
g.edge("Draft",           "ApplicationUser",      label="*..1", **ASSOC)

# Services → Models (uses)
g.edge("NotificationService",      "ApplicationDbContext", **USES)
g.edge("ExpiryNotificationService","ApplicationDbContext", **USES)
g.edge("ApplicationDbContext",     "ApplicationUser",      label="manages", **USES)

g.render(view=False, cleanup=False)
print("✅  class_diagram.png generated successfully.")
print("    Open it in VSCode or any image viewer.")
