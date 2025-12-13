using System.IO;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Serialization
{
    /// <summary>
    /// Interface for serializing and deserializing bullet patterns.
    /// </summary>
    public interface IPatternSerializer
    {
        /// <summary>
        /// Serializes a pattern to a JSON string.
        /// </summary>
        /// <param name="pattern">The pattern to serialize.</param>
        /// <returns>A JSON string representation of the pattern.</returns>
        string Serialize(IBulletPattern pattern);

        /// <summary>
        /// Deserializes a pattern from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized pattern.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the JSON is invalid or missing required fields.</exception>
        /// <exception cref="System.Text.Json.JsonException">Thrown when the JSON cannot be parsed.</exception>
        IBulletPattern Deserialize(string json);

        /// <summary>
        /// Serializes a pattern to a stream.
        /// </summary>
        /// <param name="pattern">The pattern to serialize.</param>
        /// <param name="stream">The stream to write to.</param>
        void Serialize(IBulletPattern pattern, Stream stream);

        /// <summary>
        /// Deserializes a pattern from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The deserialized pattern.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the JSON is invalid or missing required fields.</exception>
        /// <exception cref="System.Text.Json.JsonException">Thrown when the JSON cannot be parsed.</exception>
        IBulletPattern Deserialize(Stream stream);
    }
}
