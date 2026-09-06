---
title: "Tokens: roles as types"
description: How Go Mokkit tests declare, produce and read artifacts — a typed token instead of a capture, checked by the compiler.
---

Go Mokkit has no captures. Artifacts travel between phases through a **token**: a type that names a
role, and declares in the same line what that role stands for.

```go
type (
	Buyer  struct{ mokkit.Artifact[User] }
	Seller struct{ mokkit.Artifact[User] }
	Cart   struct{ mokkit.Artifact[Order] }
)
```

One line each, declared once for the suite. The artifact's type is *inferred from the token*, so every
call site spells only the role:

```go
f.Arrange().
    UserExists[Buyer](Vip).
    UserExists[Seller](Regular).
    OrderFor[Cart](f.Of[Buyer](), 100)

discount := f.Act().DiscountFor[Cart]()
```

## The three accessors

| accessor | side | returns | when it fails |
| --- | --- | --- | --- |
| `f.New[Buyer]()` | write | `*User` — the sink a producing verb fills | never (create-or-get) |
| `f.Of[Buyer]()` | read | `User` — a value | loudly, if no verb produced the role |
| `f.Ref[Buyer]()` | read | `*User` — the pointer | loudly, if no verb produced the role |

Use `Ref` when the artifact has *identity* — a recording double whose state the Act mutates and a later
Inspect observes. Everywhere else use `Of`.

Reading a role nobody produced fails at the test's line, naming what *was* arranged:

```
discount_test.go:23: mokkit: nothing arranged for main_test.Ghost (have: main_test.Buyer, main_test.Seller)
```

## What the compiler checks

- A **misspelt role** is `undefined: Byer` — a build error.
- A **role of the wrong kind** is a build error too: a verb declared
  `func (a Arrange) UserExists[K mokkit.Token[User]](...)` will not accept `Cart`, because `Cart`'s
  token declares an `Order`. The role/artifact pairing is enforced by the constraint, not remembered by
  the reader.
- The role lands in the **step label** — `arrange: UserExists[Buyer]` — through the `For` form of the
  step runners (`DoFor[K]`, `GetFor[K]`, `TryFor[K]`), so a failure names the actor it was acting for.

## Verbs generic over the token

A producing verb takes the role as a type parameter, fills the sink, and returns the chain — so the
chain never breaks to get an artifact out:

```go
func (a Arrange) UserExists[K mokkit.Token[User]](s Status) Arrange {
	a.Helper()

	return mokkit.DoFor[K](a, func(h mokkit.Host) {
		u := newUser(mokkit.NameOf[K](), s)
		*a.New[K]() = u
		h.Resolve[*fakeUsers]().add(u)
	})
}
```

## The return form, for one-off artifacts

When a test has a single artifact and no reason to name it, skip the token entirely. Chains are eager, so
a producing verb can just hand its artifact back, bound with `:=` at the point it is created:

```go
client := f.Arrange().AClient(WithName("Acme"))

result := f.Act().GetClient(client.ID)
```

Such a verb is terminal — its return type ends the chain — so use the token form for tests with more
than one actor. A suite mixes both freely.

Tokens are static by nature: a role is a type, so it cannot be picked at run time. A table-driven loop
over "roles" is what the return form is for — bind the artifact to the loop variable.

:::note[Coming from C#?]
`Capture`, `Trapture`, `Prop`, `Ensure` and `EnsureValue` have no Go equivalents, and nothing replaced
them one-for-one — eager execution made the placeholder itself unnecessary. The guard `EnsureValue`
provided (reading a value that was never produced) is `Of`'s loud failure. See
[Captures](/concepts/captures/) for the C# side of this story.
:::
