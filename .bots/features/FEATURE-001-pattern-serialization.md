# Feature: Pattern Serialization and Deserialization System

**ID**: FEATURE-001
**Status**: complete
**Created**: 2025-01-27T00:00:00Z
**Priority**: high
**Complexity**: complex

## Description

Add comprehensive JSON serialization and deserialization support for all bullet pattern types. This feature enables developers to save, load, share, and version-control bullet patterns, opening the door for pattern libraries, visual editors, and runtime pattern configuration. This is a foundational capability that transforms patterns from code-only definitions into data-driven, reusable assets.

**Why this feature matters:**
- **Save/Load Patterns**: Persist complex boss attack patterns to disk for reuse across game sessions
- **Pattern Sharing**: Export/import patterns as JSON files, enabling community pattern libraries
- **Runtime Configuration**: Load patterns from configuration files, enabling modding and easy difficulty adjustments
- **Visual Editor Support**: Provides the data format foundation for future visual pattern design tools
- **Version Control**: Track pattern changes over time using standard version control systems
- **Cross-Platform Compatibility**: JSON format works across all platforms and languages

This feature touches every pattern type in the library and requires a robust, extensible serialization architecture that can handle nested composite patterns, modifiers, and future pattern types.

## Requirements

- [x] Define JSON schema for all pattern types (basic, advanced, composite, modifiers)
- [x] Implement `IPatternSerializer` interface for serialization/deserialization
- [x] Create JSON serialization implementation using System.Text.Json
- [x] Support serialization of all existing pattern types:
  - Basic: Single, Burst, Spread, Ring, Aimed
  - Advanced: Spiral, Wave
  - Composite: Sequence, Parallel, Repeat, Loop
  - Modifiers: Rotating
- [x] Handle nested patterns (e.g., Rotating(Sequence(Ring, Spread)))
- [x] Preserve all pattern properties (counts, speeds, angles, delays, etc.)
- [x] Round-trip serialization: serialize → deserialize → identical behavior
- [x] Version-aware deserialization for future schema changes
- [x] Error handling for invalid/malformed JSON
- [x] Support for optional metadata in serialized patterns
- [x] Extension point for custom pattern types (via attributes or registration)

## Technical Details

### Architecture

The serialization system will use a polymorphic JSON structure with type discriminators:

```json
{
  "type": "RingPattern",
  "bulletCount": 8,
  "speed": 150.0,
  "startAngle": 0.0
}
```

For composite patterns:
```json
{
  "type": "SequencePattern",
  "looping": false,
  "patterns": [
    { "type": "RingPattern", "bulletCount": 8, "speed": 150.0 },
    { "type": "SpreadPattern", "count": 5, "angleSpread": 45.0, "speed": 200.0 }
  ]
}
```

### Implementation Approach

1. **Core Serialization Interface**:
   ```csharp
   public interface IPatternSerializer
   {
       string Serialize(IBulletPattern pattern);
       IBulletPattern Deserialize(string json);
       void Serialize(IBulletPattern pattern, Stream stream);
       IBulletPattern Deserialize(Stream stream);
   }
   ```

2. **Pattern Type Registry**: Use a registry system to map pattern types to serialization logic, allowing extensibility for future pattern types.

3. **JSON Schema**: Define a clear, versioned JSON schema that can evolve over time while maintaining backward compatibility.

4. **Polymorphic Deserialization**: Use System.Text.Json's polymorphic serialization features or custom converters to handle the type hierarchy.

5. **Validation**: Validate deserialized patterns to ensure they're well-formed (e.g., positive counts, valid angles).

### Design Considerations

- **Performance**: Serialization should be fast enough for runtime use (save/load during gameplay)
- **File Size**: JSON should be compact but readable (consider compression for large pattern libraries)
- **Extensibility**: New pattern types should be easy to add without breaking existing serialized patterns
- **Versioning**: Support schema versioning to handle future changes gracefully
- **Error Recovery**: Provide helpful error messages when deserialization fails

### Integration Points

- Add serialization methods to the `Pattern` factory class for convenience
- Consider adding extension methods: `pattern.ToJson()`, `Pattern.FromJson(string)`
- May require adding serialization attributes or implementing `ISerializable`-like pattern on pattern classes
- Consider creating a separate `BenedictBulletHell.Patterns.Serialization` namespace

## Dependencies

- None (uses System.Text.Json from .NET Standard 2.1)

## Test Scenarios

1. **Basic Pattern Round-Trip**: Serialize a RingPattern, deserialize it, verify identical spawn behavior
2. **Nested Composite Patterns**: Serialize a Rotating(Sequence(Ring, Spread)), deserialize, verify behavior
3. **All Pattern Types**: Test serialization/deserialization of every pattern type in the library
4. **Edge Cases**: Empty sequences, infinite loops, extreme values (very large counts, speeds)
5. **Error Handling**: Invalid JSON, missing fields, wrong types, version mismatches
6. **Performance**: Serialize/deserialize complex patterns (100+ nested patterns) within acceptable time
7. **File I/O**: Save to file, load from file, verify file contents are valid JSON
8. **Version Compatibility**: Test that future schema versions can still deserialize old patterns

## Acceptance Criteria

- [x] All existing pattern types can be serialized to JSON
- [x] All serialized patterns can be deserialized back to identical `IBulletPattern` instances
- [x] Deserialized patterns produce identical spawn behavior as original patterns (verified via unit tests)
- [x] Nested composite patterns (e.g., Rotating(Sequence(...))) serialize/deserialize correctly
- [x] Invalid JSON produces clear, actionable error messages
- [x] Serialization performance is acceptable for runtime use (< 10ms for typical patterns)
- [x] JSON output is human-readable and well-formatted
- [x] Documentation includes examples of serialized pattern JSON
- [x] Unit tests cover all pattern types and edge cases
- [x] Integration tests verify round-trip serialization with real pattern usage
