# Contributing to BenedictBulletHell.Patterns

First off, **thank you** for considering contributing! I truly believe in open source and the power of community collaboration. Unlike many repositories, I actively welcome contributions of all kinds - from bug fixes to new features.

## My Promise to Contributors

- **I will respond to every PR and issue** - I guarantee feedback on all contributions
- **Bug fixes are obvious accepts** - If it fixes a bug, it's getting merged
- **New features are welcome** - I'm genuinely open to new ideas and enhancements
- **Direct line of communication** - If I'm not responding to a PR or issue, email me directly at johnvondrashek@gmail.com

## How to Contribute

### Reporting Bugs

1. **Check existing issues** to avoid duplicates
2. **Open a new issue** with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Your environment (.NET version, game framework if applicable)

### Suggesting Features

I'm open to new features! When suggesting:
1. Explain the problem you're trying to solve
2. Describe your proposed solution
3. Consider if it fits the library's scope (pattern definition, not bullet lifecycle management)

Some areas where contributions would be especially welcome:
- New pattern types (check [DESIGN.md](DESIGN.md) for the architecture)
- Performance optimizations (allocation-free paths)
- Framework-specific vector extensions
- Documentation improvements

### Submitting Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality (using xUnit)
5. Ensure all tests pass: `dotnet test`
6. Commit with clear messages
7. Push to your fork
8. Open a Pull Request

That's it! I'll review it and provide feedback.

### Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/benedict-bullethell-patterns.git
cd benedict-bullethell-patterns

# Build the solution
dotnet build

# Run tests
dotnet test

# Build the NuGet package locally
dotnet pack BenedictBulletHell.Patterns/BenedictBulletHell.Patterns.csproj
```

### Project Structure

```
BenedictBulletHell.Patterns/
├── Core/           # IBulletPattern, BulletSpawn, PatternContext
├── Patterns/
│   ├── Basic/      # SingleShot, Spread, Ring, Burst, Aimed
│   ├── Advanced/   # Spiral, Wave, Homing
│   ├── Composite/  # Sequence, Parallel, Repeat, Loop
│   └── Modifiers/  # Rotating, Oscillating
├── Builders/       # PatternBuilder (fluent API)
└── Extensions/     # VectorExtensions for MonoGame/Unity
```

### Code Style

- Target .NET Standard 2.1 for broad compatibility
- Use `init` properties for immutable pattern construction
- Keep patterns stateless and thread-safe
- Prefer `struct` for value types (`BulletSpawn`, `PatternContext`)
- Include XML documentation for all public APIs

### Your First Contribution

Never contributed to open source before? No problem! Look for issues labeled `good first issue` or `help wanted`. Resources:
- [How to Make a Pull Request](http://makeapullrequest.com/)
- [First Timers Only](https://www.firsttimersonly.com/)

## Code of Conduct

This project follows the [Rule of St. Benedict](CODE_OF_CONDUCT.md) as its code of conduct. Please read it - it's been guiding communities for over 1,500 years.

## Questions?

- Open an issue
- Email me: johnvondrashek@gmail.com

I appreciate every contribution, big or small. Thank you for being part of this project!
