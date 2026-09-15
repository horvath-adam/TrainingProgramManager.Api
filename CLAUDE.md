# TrainingProgramManager – Project Instructions

## Project context

This repository is used for a university-level ASP.NET Core Web API course and live teaching demos.

- Target platform: .NET 10
- Main technology: ASP.NET Core Web API
- API style: controller-based unless the current task explicitly says otherwise
- Development environment: Visual Studio 2026
- Main project: `TrainingProgramManager.Api`

The project is developed incrementally across multiple teaching modules. Treat the current task as the scope boundary.

## Language rules

- Write all code in English.
- Use English for:
  - identifiers;
  - class, method, property, parameter, variable, and file names;
  - namespaces;
  - API routes;
  - configuration keys;
  - technical names.
- When a code comment is useful, write it bilingually in this order:

```csharp
// EN: English explanation.
// HU: Magyar magyarázat.
```

- Keep the English and Hungarian comments semantically equivalent.
- Do not translate code identifiers into Hungarian.
- Do not add comments to self-explanatory code.
- Prefer a few useful teaching comments over excessive commenting.

## Change policy

- Make only the changes required by the current task.
- Preserve existing project structure and conventions.
- Do not refactor unrelated code.
- Do not introduce new architecture, abstractions, packages, frameworks, or patterns unless explicitly requested.
- Do not implement features that belong to later teaching modules unless explicitly requested.
- Prefer the smallest correct change.
- Do not generate placeholder layers, interfaces, DTOs, services, repositories, or other artifacts unless the task requires them.
- Do not replace an existing working approach with a different one without a clear task requirement.

## C# and ASP.NET Core conventions

- Use clear, idiomatic modern C#.
- Prefer simple and readable code suitable for teaching.
- Avoid unnecessary abstraction and overengineering.
- Use nullable reference types consistently with the existing project.
- Prefer async APIs when asynchronous work is actually required.
- Follow existing naming and formatting conventions.
- Keep controllers focused on HTTP/API responsibilities.
- Keep routing explicit and easy to understand.
- Preserve controller-based API design unless the task explicitly requests Minimal APIs.

## Packages and framework features

- Prefer built-in .NET and ASP.NET Core features when they satisfy the task.
- Add a NuGet package only when it is required by the task.
- Do not add packages speculatively.
- Do not upgrade or replace unrelated packages.
- Keep package changes minimal and explain why a new package is needed.

## Educational constraints

- Code must remain suitable for live demonstration and student review.
- Prefer solutions that make the relevant concept visible rather than hiding it behind unnecessary abstraction.
- Do not skip important teaching steps by generating a more advanced final architecture.
- Respect the incremental order of the course modules.
- If several valid solutions exist, prefer the one that best matches the current project and teaching objective.
- Do not introduce advanced patterns merely as “best practice” when they are outside the current module.

## Verification

After making changes:

- ensure the project still builds;
- check for obvious compile-time errors;
- verify that changed routes, configuration, and dependencies are consistent;
- do not change unrelated files just to make the solution look cleaner.

If verification cannot be completed, state exactly what remains unverified.

## Response style

Keep responses concise and task-focused.

When code is changed:

1. state what was changed;
2. mention any important design or framework decision;
3. state how to verify the result.

Do not provide long tutorials unless explicitly requested.

## Scope rule

If the current task conflicts with these project instructions, follow the explicit current task.

If the requested change would introduce functionality that appears to belong to a later module, do not add it unless the task explicitly requires it.
