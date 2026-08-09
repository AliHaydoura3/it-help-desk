# Role and access model

The help desk uses exactly one role per user. Every authenticated role inherits
the employee self-service capabilities; operational access is then added by
explicit permissions rather than by assigning additional roles.

| Capability | Employee | Manager | IT Support Agent | Admin |
| --- | :---: | :---: | :---: | :---: |
| Manage own profile and notifications | Yes | Yes | Yes | Yes |
| Create and track own tickets | Yes | Yes | Yes | Yes |
| Edit or cancel own open tickets | Yes | Yes | Yes | Yes |
| Comment and mention agents on own tickets | Yes | Yes | Yes | Yes |
| View the support queue | No | Read-only | Yes | Yes |
| Comment on another user's ticket | No | No | Yes | Yes |
| Edit or cancel another user's ticket | No | No | Yes | Yes |
| Assign, auto-assign, reassign, or escalate | No | No | Yes | Yes |
| Change ticket status | No | No | Assigned tickets only | Any ticket |
| Use internal support notes | No | No | Yes | Yes |
| View assignment history | No | All visible tickets | All tickets | All tickets |
| View operational reports | No | Yes | No | Yes |
| Manage users and roles | No | No | No | Yes |
| View system activity logs | No | No | No | Yes |

## Workflow rules

- A support agent must be the current assignee before changing status.
- A support agent may assign or reassign a ticket to any active support agent,
  including themselves.
- Admins have explicit status override authority.
- Managers can observe other users' tickets, conversations, histories, and
  reports, but cannot mutate them.
- Resolved or closed tickets cannot be assigned, reassigned, or escalated.
- Closed and cancelled ticket conversations are read-only.
- All status, assignment, escalation, comment, and ticket mutations are
  recorded in ticket history and/or activity logs.

## Manager visibility scope

The current domain does not contain a Team aggregate or a manager-to-employee
relationship. Manager monitoring is therefore organization-wide and read-only.
Team-only visibility should be introduced with an explicit Team model rather
than inferred from roles or email addresses.
