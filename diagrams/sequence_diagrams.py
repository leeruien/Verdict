"""
Verdict - Sequence Diagrams
Run:  python3 sequence_diagrams.py
Output: seq_1_registration.png
        seq_2_login.png
        seq_3_dilemma_vote.png
Requires: pip install matplotlib
"""

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyArrowPatch

# ── Drawing engine ─────────────────────────────────────────────────────────

COLORS = {
    "actor":   "#1565C0",
    "system":  "#4527A0",
    "db":      "#2E7D32",
    "external":"#E65100",
    "note":    "#FFF9C4",
    "alt":     "#F3E5F5",
    "ret":     "#888888",
    "msg":     "#1A237E",
    "line":    "#CCCCCC",
}

def draw_sequence(title, participants, steps, filename, figsize=(16, 12)):
    """
    participants : list of (id, label, kind)   kind in actor|system|db|external
    steps        : list of dicts - see helpers below
    """
    n = len(participants)
    fig, ax = plt.subplots(figsize=figsize)
    ax.set_xlim(-0.5, n - 0.5)
    # will set ylim after counting rows
    ax.axis("off")
    fig.patch.set_facecolor("#F8F9FC")

    col = {p[0]: i for i, p in enumerate(participants)}

    # ── header boxes ──────────────────────────────────────────────────────
    HEADER_H = 0.55
    BOX_W    = 0.75

    kind_colors = {
        "actor":    ("#1565C0", "#E3F2FD"),
        "system":   ("#4527A0", "#EDE7F6"),
        "db":       ("#2E7D32", "#E8F5E9"),
        "external": ("#E65100", "#FBE9E7"),
    }

    def draw_header(ax, x, y, label, kind):
        fc, bc = kind_colors.get(kind, ("#333333", "#EEEEEE"))
        rect = mpatches.FancyBboxPatch(
            (x - BOX_W / 2, y - HEADER_H / 2), BOX_W, HEADER_H,
            boxstyle="round,pad=0.05", linewidth=1.5,
            edgecolor=fc, facecolor=bc,
        )
        ax.add_patch(rect)
        ax.text(x, y, label, ha="center", va="center",
                fontsize=9, fontweight="bold", color=fc,
                wrap=True, multialignment="center")

    # ── layout ────────────────────────────────────────────────────────────
    TOP      = 0        # title sits above y=0
    HEAD_Y   = -0.8
    STEP_H   = 0.72     # vertical spacing per step
    total_steps = sum(1 + s.get("_extra_rows", 0) for s in steps)
    BOTTOM   = HEAD_Y - HEADER_H / 2 - total_steps * STEP_H - 0.5

    ax.set_ylim(BOTTOM, TOP + 0.3)

    # title
    ax.text(n / 2 - 0.5, TOP + 0.1, title,
            ha="center", va="top", fontsize=14, fontweight="bold",
            color="#052767")

    # header boxes + lifelines
    for pid, label, kind in participants:
        x = col[pid]
        draw_header(ax, x, HEAD_Y, label, kind)
        ax.plot([x, x], [HEAD_Y - HEADER_H / 2, BOTTOM],
                color=COLORS["line"], linewidth=1, linestyle="dashed", zorder=0)

    # duplicate footer boxes at the bottom
    for pid, label, kind in participants:
        x = col[pid]
        draw_header(ax, x, BOTTOM + HEADER_H / 2, label, kind)

    # ── draw steps ────────────────────────────────────────────────────────
    y = HEAD_Y - HEADER_H / 2 - STEP_H * 0.6
    row_y = []
    for s in steps:
        row_y.append(y)
        y -= STEP_H * (1 + s.get("_extra_rows", 0))

    def arrow(ax, x1, x2, y, label, ret=False, color=None):
        c = color or (COLORS["ret"] if ret else COLORS["msg"])
        ls = "--" if ret else "-"
        style = "arc3,rad=0.0"
        ax.annotate(
            "", xy=(x2, y), xytext=(x1, y),
            arrowprops=dict(
                arrowstyle="->" if not ret else "->",
                color=c, lw=1.4,
                linestyle=ls,
                connectionstyle=style,
            ),
            zorder=3,
        )
        mx = (x1 + x2) / 2
        offset = 0.08 if x2 > x1 else -0.08
        ax.text(mx, y + 0.10, label,
                ha="center", va="bottom", fontsize=8.5,
                color=c, fontweight="bold" if not ret else "normal",
                bbox=dict(boxstyle="round,pad=0.15", fc="white", ec="none", alpha=0.85))

    def self_arrow(ax, x, y, label, color=None):
        c = color or COLORS["msg"]
        ax.annotate(
            "", xy=(x, y - 0.22), xytext=(x, y),
            arrowprops=dict(
                arrowstyle="->", color=c, lw=1.4,
                connectionstyle="arc3,rad=-0.6",
            ), zorder=3,
        )
        ax.text(x + 0.22, y - 0.11, label,
                ha="left", va="center", fontsize=8.5, color=c, fontweight="bold",
                bbox=dict(boxstyle="round,pad=0.15", fc="white", ec="none", alpha=0.85))

    def alt_box(ax, y_top, y_bot, label, color="#F3E5F5"):
        x_left  = -0.45
        x_right = n - 0.55
        rect = mpatches.FancyBboxPatch(
            (x_left, y_bot), x_right - x_left, y_top - y_bot,
            boxstyle="square,pad=0", linewidth=1,
            edgecolor="#9C27B0", facecolor=color, alpha=0.25, zorder=1,
        )
        ax.add_patch(rect)
        ax.text(x_left + 0.05, y_top - 0.05, label,
                ha="left", va="top", fontsize=8, color="#6A1B9A",
                fontweight="bold",
                bbox=dict(boxstyle="round,pad=0.2", fc="#CE93D8", ec="none", alpha=0.6))

    def note_box(ax, x, y, text, color="#FFF9C4"):
        ax.text(x, y, text, ha="center", va="center", fontsize=8,
                color="#5D4037", style="italic",
                bbox=dict(boxstyle="round,pad=0.3", fc=color, ec="#F9A825", lw=0.8))

    def divider(ax, y, label):
        ax.axhline(y, color="#BBBBBB", linewidth=0.8, linestyle=":")
        ax.text(n / 2 - 0.5, y + 0.06, label,
                ha="center", va="bottom", fontsize=8, color="#888888")

    # process each step
    for i, s in enumerate(steps):
        y = row_y[i]
        kind = s.get("kind", "msg")

        if kind == "msg":
            if s["frm"] == s["to"]:
                self_arrow(ax, col[s["frm"]], y, s["label"],
                           color=s.get("color"))
            else:
                arrow(ax, col[s["frm"]], col[s["to"]], y, s["label"],
                      ret=s.get("ret", False), color=s.get("color"))

        elif kind == "note":
            note_box(ax, col[s["at"]], y, s["label"],
                     color=s.get("color", "#FFF9C4"))

        elif kind == "alt":
            # marks the START of an alt block; _end_row tells us the end index
            end_y = row_y[s["end_row"]] if s.get("end_row") is not None else y - STEP_H
            alt_box(ax, y + STEP_H * 0.4, end_y - STEP_H * 0.3,
                    s["label"], color=s.get("color", "#F3E5F5"))

        elif kind == "divider":
            divider(ax, y, s["label"])

    plt.tight_layout(pad=0.5)
    plt.savefig(filename, dpi=150, bbox_inches="tight",
                facecolor=fig.get_facecolor())
    plt.close()
    print(f"✅  {filename}")


# ══════════════════════════════════════════════════════════════════════════════
# DIAGRAM 1 - User Registration & Email Verification
# ══════════════════════════════════════════════════════════════════════════════

P1 = [
    ("user",     "User",               "actor"),
    ("blazor",   "Blazor Server",      "system"),
    ("pg",       "PostgreSQL\n(Supabase)", "db"),
    ("supaauth", "Supabase Auth\nAPI",  "external"),
    ("inbox",    "User's Email\nInbox", "external"),
]

S1 = [
    dict(kind="msg",  frm="user",     to="blazor",   label="Fill & submit Register form"),
    dict(kind="msg",  frm="blazor",   to="pg",        label="FindByEmailAsync() + check username"),
    dict(kind="msg",  frm="pg",       to="blazor",    label="Not found → OK to proceed", ret=True),
    dict(kind="msg",  frm="blazor",   to="blazor",    label="Hash password (Identity PasswordHasher)"),
    dict(kind="msg",  frm="blazor",   to="pg",        label="INSERT PendingRegistration\n(email, displayName, passwordHash, expiresAt)"),
    dict(kind="msg",  frm="pg",       to="blazor",    label="Saved", ret=True),
    dict(kind="msg",  frm="blazor",   to="supaauth",  label="SignUpAsync(email, password, confirmUrl)"),
    dict(kind="msg",  frm="supaauth", to="inbox",     label="Send confirmation email (magic link)"),
    dict(kind="msg",  frm="supaauth", to="blazor",    label="200 OK", ret=True),
    dict(kind="msg",  frm="blazor",   to="user",      label="Show: Check your inbox", ret=True),
    dict(kind="divider", label="── User clicks confirmation link in email ──"),
    dict(kind="msg",  frm="user",     to="blazor",    label="GET /auth/do-signin?token_hash=..."),
    dict(kind="msg",  frm="blazor",   to="supaauth",  label="VerifyTokenHashAsync(token_hash, 'email')"),
    dict(kind="msg",  frm="supaauth", to="blazor",    label="Return verified email address", ret=True),
    dict(kind="msg",  frm="blazor",   to="pg",        label="SELECT PendingRegistration WHERE email = ?"),
    dict(kind="msg",  frm="pg",       to="blazor",    label="Return pending record + passwordHash", ret=True),
    dict(kind="msg",  frm="blazor",   to="pg",        label="INSERT AspNetUsers (EmailConfirmed=true)\n+ restore PasswordHash"),
    dict(kind="msg",  frm="blazor",   to="pg",        label="DELETE PendingRegistration"),
    dict(kind="msg",  frm="blazor",   to="blazor",    label="SignInAsync() → write auth cookie"),
    dict(kind="msg",  frm="blazor",   to="user",      label="Redirect to /  (logged in)", ret=True,
         color="#2E7D32"),
]

draw_sequence(
    "Flow 1 - User Registration & Email Verification",
    P1, S1, "seq_1_registration.png", figsize=(17, 13),
)


# ══════════════════════════════════════════════════════════════════════════════
# DIAGRAM 2 - Login (happy path · unverified · locked out)
# ══════════════════════════════════════════════════════════════════════════════

P2 = [
    ("user",      "User",                "actor"),
    ("blazor",    "Blazor Server",       "system"),
    ("identity",  "ASP.NET Identity",    "system"),
    ("pg",        "PostgreSQL",          "db"),
    ("supaauth",  "Supabase Auth\nAPI",  "external"),
]

S2 = [
    dict(kind="msg",  frm="user",     to="blazor",   label="Submit Login form (email, password)"),
    dict(kind="msg",  frm="blazor",   to="identity",  label="PasswordSignInAsync(email, pwd,\nlockoutOnFailure: true)"),
    dict(kind="msg",  frm="identity", to="pg",        label="SELECT AspNetUsers WHERE Email = ?"),

    dict(kind="alt",  label="[A] Happy path - credentials valid, email confirmed",
         end_row=7, color="#E8F5E9"),
    dict(kind="msg",  frm="pg",       to="identity",  label="User found, EmailConfirmed = true", ret=True),
    dict(kind="msg",  frm="identity", to="pg",        label="UPDATE AccessFailedCount = 0"),
    dict(kind="msg",  frm="identity", to="blazor",    label="result.Succeeded = true", ret=True),
    dict(kind="msg",  frm="blazor",   to="user",      label="Write auth cookie → Redirect /",
         ret=True, color="#2E7D32"),

    dict(kind="alt",  label="[B] Wrong password (attempt < 5) - increment failure counter",
         end_row=11, color="#FBE9E7"),
    dict(kind="msg",  frm="pg",       to="identity",  label="User found", ret=True),
    dict(kind="msg",  frm="identity", to="pg",        label="UPDATE AccessFailedCount += 1"),
    dict(kind="msg",  frm="identity", to="blazor",    label="result.Failed", ret=True),
    dict(kind="msg",  frm="blazor",   to="user",      label="Show: Invalid email or password",
         ret=True, color="#BF360C"),

    dict(kind="alt",  label="[C] 5th failed attempt - account locked for 15 minutes",
         end_row=16, color="#FCE4EC"),
    dict(kind="msg",  frm="pg",       to="identity",  label="User found, AccessFailedCount = 4", ret=True),
    dict(kind="msg",  frm="identity", to="pg",        label="SET LockoutEnd = NOW() + 15 min"),
    dict(kind="msg",  frm="identity", to="blazor",    label="result.IsLockedOut = true", ret=True),
    dict(kind="msg",  frm="blazor",   to="user",      label="Show: Too many failed attempts...",
         ret=True, color="#BF360C"),

    dict(kind="alt",  label="[D] Email not confirmed - auto-resend verification",
         end_row=23, color="#E8EAF6"),
    dict(kind="msg",  frm="pg",       to="identity",  label="User found, EmailConfirmed = false", ret=True),
    dict(kind="msg",  frm="identity", to="blazor",    label="result.IsNotAllowed = true", ret=True),
    dict(kind="msg",  frm="blazor",   to="pg",        label="SELECT PendingRegistration WHERE email = ?"),
    dict(kind="msg",  frm="pg",       to="blazor",    label="Return pending record", ret=True),
    dict(kind="msg",  frm="blazor",   to="supaauth",  label="ResendConfirmationEmailAsync(email)"),
    dict(kind="msg",  frm="supaauth", to="blazor",    label="200 OK", ret=True),
    dict(kind="msg",  frm="blazor",   to="user",
         label="Show: Verify your email - confirmation sent to [email]",
         ret=True, color="#6A1B9A"),
]

draw_sequence(
    "Flow 2 - Login  (happy path · unverified email · account lockout)",
    P2, S2, "seq_2_login.png", figsize=(17, 17),
)


# ══════════════════════════════════════════════════════════════════════════════
# DIAGRAM 3 - Post Dilemma → Vote → Comment → Notifications
# ══════════════════════════════════════════════════════════════════════════════

P3 = [
    ("poster",   "User A\n(Poster)",         "actor"),
    ("voter",    "User B\n(Voter)",           "actor"),
    ("blazor",   "Blazor Server",            "system"),
    ("notif",    "NotificationService",      "system"),
    ("pg",       "PostgreSQL",               "db"),
]

S3 = [
    dict(kind="divider", label="── Poster creates a dilemma ──"),
    dict(kind="msg",  frm="poster",  to="blazor",  label="Submit new dilemma\n(title, options, category, expiry)"),
    dict(kind="msg",  frm="blazor",  to="pg",       label="INSERT Dilemma + DilemmaOptions"),
    dict(kind="msg",  frm="pg",      to="blazor",   label="Saved with new Guid", ret=True),
    dict(kind="msg",  frm="blazor",  to="notif",    label="NotifyNewPostAsync(dilemma, posterUserId)"),
    dict(kind="msg",  frm="notif",   to="pg",        label="SELECT CategorySubscriptions\nWHERE category = ? AND userId ≠ poster"),
    dict(kind="msg",  frm="pg",      to="notif",    label="Return subscriber list", ret=True),
    dict(kind="msg",  frm="notif",   to="pg",        label="INSERT Notification row\nper subscriber (Type=NewPost)"),
    dict(kind="msg",  frm="blazor",  to="poster",   label="Redirect to /dilemma/{id}", ret=True,
         color="#2E7D32"),

    dict(kind="divider", label="── Voter views & votes on the dilemma ──"),
    dict(kind="msg",  frm="voter",   to="blazor",  label="Navigate to /dilemma/{id}"),
    dict(kind="msg",  frm="blazor",  to="pg",       label="SELECT Dilemma + Options + Votes\n+ Comments (with Users)"),
    dict(kind="msg",  frm="pg",      to="blazor",   label="Return full dilemma graph", ret=True),
    dict(kind="msg",  frm="blazor",  to="voter",    label="Render dilemma page", ret=True),
    dict(kind="msg",  frm="voter",   to="blazor",   label="CastVote(selectedOption)"),
    dict(kind="msg",  frm="blazor",  to="pg",        label="INSERT Vote (userId, dilemmaOptionId)"),
    dict(kind="msg",  frm="blazor",  to="notif",    label="NotifyVoteAsync(voterName, dilemma)"),
    dict(kind="msg",  frm="notif",   to="pg",        label="UPSERT Notification for poster\n(Type=Vote - merge multiple voters)"),
    dict(kind="msg",  frm="blazor",  to="pg",        label="Reload dilemma (fresh vote counts)"),
    dict(kind="msg",  frm="blazor",  to="voter",    label="Update vote bars in UI", ret=True,
         color="#2E7D32"),

    dict(kind="divider", label="── Voter posts a comment ──"),
    dict(kind="msg",  frm="voter",   to="blazor",  label="PostComment(body)"),
    dict(kind="msg",  frm="blazor",  to="pg",       label="INSERT Comment\n(userId, dilemmaId, body, createdAt)"),
    dict(kind="msg",  frm="blazor",  to="notif",    label="NotifyCommentAsync(voterName, dilemma, userId)"),
    dict(kind="msg",  frm="notif",   to="pg",        label="INSERT Notification for poster\n(Type=Comment)"),
    dict(kind="msg",  frm="blazor",  to="voter",    label="Refresh comments section", ret=True,
         color="#2E7D32"),

    dict(kind="divider", label="── Poster views notifications ──"),
    dict(kind="msg",  frm="poster",  to="blazor",  label="Open /notification"),
    dict(kind="msg",  frm="blazor",  to="pg",       label="SELECT Notifications\nWHERE recipientUserId = poster"),
    dict(kind="msg",  frm="pg",      to="blazor",   label="Vote + Comment notifications", ret=True),
    dict(kind="msg",  frm="blazor",  to="poster",   label="Show notification feed (🗳️ vote, 💬 comment)",
         ret=True, color="#2E7D32"),
]

draw_sequence(
    "Flow 3 - Post Dilemma → Vote → Comment → Notifications",
    P3, S3, "seq_3_dilemma_vote.png", figsize=(16, 16),
)

print("\nAll three sequence diagrams generated.")
