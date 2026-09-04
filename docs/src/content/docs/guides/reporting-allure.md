---
title: Report to Allure
description: Turn a Go Mokkit suite's vocabulary into Allure results — every test a scenario, every verb a step.
---

Go Mokkit's stages can be **observed**: an observer hears each stage entered, every step that ran — with
its phase, verb name, duration and outcome — and the stage closing with the test's verdict. The step
names arrive exactly as the suite spelled them (`arrange: UserExists[Buyer]`), which is precisely what a
test report wants to show.

`report/allure` is that observer, writing [Allure 2](https://allurereport.org/) result files. It lives in
the core module and depends on nothing but the standard library — allure-results is just JSON.

## Wire it in

```go
import "github.com/GrafGenerator/go-mokkit/report/allure"

func TestMain(m *testing.M) {
	setup, err := mokkit.NewSetup(context.Background(), mocks, app)
	if err != nil {
		panic(err)
	}

	var reporter *allure.Reporter
	if dir := os.Getenv("ALLURE_OUTPUT_PATH"); dir != "" {
		if reporter, err = allure.New(dir, allure.WithSuite("cards e2e")); err != nil {
			panic(err)
		}
		setup.Observe(reporter)
	}

	code := m.Run()

	// An observer never fails a test; an incomplete report is surfaced here.
	if reporter != nil {
		if err := reporter.Err(); err != nil {
			fmt.Fprintf(os.Stderr, "allure report incomplete: %v\n", err)
		}
	}

	os.Exit(code)
}
```

Run with `ALLURE_OUTPUT_PATH=./allure-results go test ./...` and feed the directory to the Allure CLI or
TestOps as usual.

## What a result looks like

One file per test, whose steps are the test's own vocabulary, with real timings:

```json
{
  "name": "TestOrange_AnAllowedPartyActivatesAndDrawsAVirtualCard",
  "status": "passed",
  "steps": [
    { "name": "arrange: AnOrangeCategory[Orange]", "status": "passed" },
    { "name": "arrange: AFreeVirtualCardInThePool[Orange]", "status": "passed" },
    { "name": "act: RegisterCard[Orange]", "status": "passed" },
    { "name": "inspect: theOutboxPayloadMentions", "status": "passed" }
  ]
}
```

A step that failed carries its message and trace; a step that **panicked** is reported as `broken` rather
than `failed`, so triage can tell an assertion from a crash. `historyId` is stable across runs (it hashes
the suite and test name), which is what lets TestOps track history and retries.

## The observer seam itself

`allure` is ~300 lines over a public seam you can point anywhere else — OpenTelemetry spans, a JUnit
writer, a metrics counter:

```go
type Observer interface {
	StageEntered(test, stageID string)
	StepRan(event StepEvent) // test, stage, phase, step, started, duration, err
	StageClosed(test, stageID string, failed bool)
}
```

Register any number with `setup.Observe(...)`. Two contract points worth knowing: observers must be safe
for concurrent use (`All` branches and parallel tests both emit), and an observer has no way to fail a
test — reporting problems are yours to surface, as `Reporter.Err()` does above.

:::note[C#]
The .NET side has no observer seam yet; this page is the Go half of the story. Allure's own adapters for
xUnit/NUnit work alongside Mokkit there, at test granularity rather than step granularity.
:::
