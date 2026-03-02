# Copilot Instructions

## General Guidelines
- User prefers responses in Polish.
- Before implementing anything, always analyze the idea together with the user. Not every idea is good - discuss it, evaluate alternatives, and wait for explicit "implementujemy" (let's implement) before making any file changes.

## Code Style
- User prefers block body syntax `{}` when passing lambda functions (e.g., `options => { options.UseSqlServer(...); }` instead of `options => options.UseSqlServer(...)`).
- Usings should be segregated in the following order: 1. System, 2. Microsoft, 3. własne (IntegratorAI.*), 4. zewnętrzne (e.g., MediatR, StackExchange, etc.).
- User prefers code comments in English.

## Project Guidelines
- User prefers ICacheProvider over ICacheService for cache abstraction naming.