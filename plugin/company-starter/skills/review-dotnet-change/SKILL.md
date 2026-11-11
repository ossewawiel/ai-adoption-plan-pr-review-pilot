---
name: review-dotnet-change
description: Review a small .NET / C# change for a human reviewer and write the result as Summary, Suggestions and Potential Risks. Use when asked to review a diff, a branch or a pull request in a .NET repository.
---

# Review a .NET change

This is the same rubric the local review tool in this repository sends to its provider
(`src/ReviewAssistant.Core/ReviewPromptBuilder.cs`). A test keeps the two in step.

## Get the change

Review the diff the person points you at. If they name none, use the current branch against main:
`git diff main...HEAD`. Read only the files in that diff unless you need a caller to judge a risk.

## The rubric

You review a small .NET / C# change for a human reviewer. You advise; the person decides.

Look for, in this order:
1. Security and privacy: secrets, personal information, injection, unsafe logging.
2. Correctness: wrong results, unhandled edge cases, error handling.
3. Compatibility: public behaviour or signatures that existing callers rely on.
4. Tests: behaviour that changed without a test.
5. Maintainability: only when it will cost the team later.

Rules:
- Say what changed and why it matters, then suggestions, then risks.
- Point at a file and line for every suggestion and risk.
- Give each item a severity (low, medium, high) and say how confident you are (low, medium, high).
- If you are not sure, say so with low confidence. Do not invent problems.
- Markers such as [REDACTED:secret] replace sensitive values. Never guess what they hid.

## The output

Write one Markdown file, `review.md`, with exactly these headings:

```markdown
# AI Review: <repository> (<branch>)

## Summary
- What changed and why it matters. (`path/File.cs:12`)

## Suggestions
- **[MEDIUM]** The suggestion. (`path/File.cs:12`)
  _category · confidence: high_

## Potential Risks
- **[HIGH]** The risk. (`path/File.cs:12`)
  _category · confidence: medium_
```

Do not approve, merge or push anything. The review is advice; a person decides.
