using System;

namespace Ratatui;

// Integer 2D vector with ergonomic, game-dev-style API and swizzles.
public readonly struct Vec2i : IEquatable<Vec2i>
{
    public int X { get; }
    public int Y { get; }

    // Lowercase aliases for convenience (swizzle-like access)
    public int x => X;
    public int y => Y;
    public Vec2i xy => this;
    public Vec2i yx => new(Y, X);
    public Vec2i pos => this;
    public int w => X;
    public int h => Y;

    public Vec2i(int x, int y) { X = x; Y = y; }

    public static Vec2i Zero => new(0, 0);
    public static Vec2i One => new(1, 1);
    public static Vec2i UnitX => new(1, 0);
    public static Vec2i UnitY => new(0, 1);

    // Fluent editing
    public Vec2i WithX(int x) => new(x, Y);
    public Vec2i WithY(int y) => new(X, y);

    // Basic operators
    public static Vec2i operator +(Vec2i a, Vec2i b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2i operator -(Vec2i a, Vec2i b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2i operator *(Vec2i a, int s) => new(a.X * s, a.Y * s);
    public static Vec2i operator *(int s, Vec2i a) => new(a.X * s, a.Y * s);
    public static Vec2i operator /(Vec2i a, int s) => new(a.X / s, a.Y / s);

    // Indexer [0]=x, [1]=y
    public int this[int i] => i == 0 ? X : Y;

    // Deconstruct support
    public void Deconstruct(out int x, out int y) { x = X; y = Y; }

    // Tuple conversions
    public static implicit operator Vec2i((int x, int y) v) => new(v.x, v.y);
    public static implicit operator (int x, int y)(Vec2i v) => (v.X, v.Y);

    public override string ToString() => $"({X},{Y})";
    public bool Equals(Vec2i other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Vec2i o && Equals(o);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
