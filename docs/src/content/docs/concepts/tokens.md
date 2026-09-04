---
title: "Tokens: roles as types"
description: How Go Mokkit tests declare, produce and read artifacts — a typed token instead of a capture, checked by the compiler.
---

Go Mokkit has no captures — eager chains delete the placeholder they existed to make safe. What remains
is the real question captures answered: **how does an artifact travel between phases** without a `var`
declared above the test, and without a stringly-typed lookup?

The answer is a **token**: a type that names a role, and declares in the same line what that role stands
for.

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

**Prefer `Of`.** A value cannot be written through by accident, which keeps a read-only phase read-only.
Reach for `Ref` when the artifact has *identity* — a recording double whose state the Act mutates and a
later Inspect observes; a copy there would silently assert on stale state.

Reading a role nobody produced fails at the test's line, naming what *was* arranged:

```
discount_test.go:23: mokkit: nothing arranged for main_test.Ghost (have: main_test.Buyer, main_test.Seller)
```

## What the compiler checks

This is where tokens beat both `out var` captures and any string-keyed registry:

- A **misspelt role** is `undefined: Byer` — a build error.
- A **role of the wrong kind** is a build error too: a verb declared
  `func (a Arrange) UserExists[K mokkit.Token[User]](...)` will not accept `Cart`, because `Cart`'s
  token declares an `Order`. The role/artifact pairing is enforced by the constraint, not remembered by
  the reader.
- The role lands in the **step label** — `arrange: UserExists[Buyer]` — via `mokkit.NameOf[K]()`, so a
  failure names the actor it was acting for.

## Verbs generic over the token

A producing verb takes the role as a type parameter, fills the sink, and returns the chain — so the
chain never breaks to get an artifact out:

```go
func (a Arrange) UserExists[K mokkit.Token[User]](s Status) Arrange {
	a.Helper()
	a.Add("UserExists["+mokkit.NameOf[K]()+"]", func(ctx context.Context, h mokkit.Host) error {
		u := newUser(mokkit.NameOf[K](), s)
		*a.New[K]() = u
		h.Resolve[*fakeUsers]().add(u)

		return nil
	})

	return a
}
```

## The return form, for one-off artifacts

When a test has a single artifact and no reason to name it, skip the token entirely. Chains are eager, so
a producing verb can just hand its artifact back, bound with `:=` at the point it is created:

```go
client := f.Arrange().AClient(WithName("Acme"))

result := f.Act().GetClient(client.ID)
```

Nothing declared above, no pointer, and go-to-definition lands on the verb that made it. The cost: such a
verb is terminal — its return type ends the chain — which is exactly why the token form exists for tests
with more than one actor. A suite mixes both freely.

Tokens are static by nature: a role is a type, so it cannot be picked at run time. A table-driven loop
over "roles" is what the return form is for — bind the artifact to the loop variable.

:::note[Coming from C#?]
`Capture`, `Trapture`, `Prop`, `Ensure` and `EnsureValue` have no Go equivalents, and nothing replaced
them one-for-one — eager execution made the placeholder itself unnecessary. The guard `EnsureValue`
provided (reading a value that was never produced) is `Of`'s loud failure. See
[Captures](/concepts/captures/) for the C# side of this story.
:::
